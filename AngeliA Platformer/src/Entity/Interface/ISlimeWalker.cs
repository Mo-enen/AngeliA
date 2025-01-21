using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

internal interface ISlimeWalker {

	// VAR
	const int CHECKING_GAP = 16;

	/// <summary>
	/// When this value is "Up" the walker will be upside-down. "Center" means not attaching anything.
	/// </summary>
	public Direction5 AttachingDirection { get; set; }
	public Direction5 WalkingDirection =>
		AttachingDirection == Direction5.Center ? Direction5.Center :
		FacingPositive ? AttachingDirection.AntiClockwise() : AttachingDirection.Clockwise();
	public Int2 LocalPosition { get; set; }
	public Entity AttachingTarget { get; set; }
	public int AttachingID { get; set; }
	public IRect AttachingRect { get; set; }
	public int WalkSpeed { get; }
	public bool FacingPositive { get; set; }

	// API
	public static void ActiveWalker (ISlimeWalker walker) {
		walker.AttachingDirection = Direction5.Center;
		walker.AttachingTarget = null;
		walker.AttachingRect = default;
		walker.LocalPosition = default;
		walker.FacingPositive = true;
	}

	public static Direction5 RefreshAttachingDirection (ISlimeWalker walker) {

		if (walker is not Entity entity) return walker.AttachingDirection;
		var ePos = entity.XY;
		var eRect = entity.Rect;

		// Free Fall
		if (walker.AttachingDirection == Direction5.Center) {
			if (entity.FromWorld && Game.GlobalFrame == entity.SpawnFrame) {
				eRect.x = eRect.x.ToUnifyGlobal();
				eRect.y = eRect.y.ToUnifyGlobal();
				eRect.width = eRect.height = Const.CEL;
			}
			if (HitCheckFree(entity, eRect, out var hit, out var hitDir)) {
				walker.SetHit(hit, ePos);
				walker.AttachingDirection = hitDir.ToDirection5();
			}
			return walker.AttachingDirection;
		}

		// Walking
		var attachDir4 = walker.AttachingDirection.ToDirection4();
		var walkingDir = walker.WalkingDirection.ToDirection4();
		var wallTurnDir = walker.FacingPositive ? attachDir4.AntiClockwise() : attachDir4.Clockwise();
		var pitTurnDir = wallTurnDir.Opposite();

		// Chech for Turning
		if (HitCheck(
			entity, eRect.EdgeOutside(walkingDir, CHECKING_GAP),
			wallTurnDir, out var hitWall
		)) {
			// Wall Turn
			walker.SetHit(hitWall, ePos);
			walker.AttachingDirection = wallTurnDir.ToDirection5();

		} else if (HitCheck(
			entity,
			GetGroundCheckRect(eRect, attachDir4, walker.FacingPositive),
			attachDir4, out var hitGround
		)) {
			// Keep Walking
			walker.SetHit(hitGround, ePos);

		} else if (HitCheck(
			entity,
			eRect.EdgeOutside(attachDir4, CHECKING_GAP).Shift(walkingDir.Normal() * -Const.QUARTER),
			pitTurnDir, out var hitPit
		)) {
			// Pit Turn
			walker.SetHit(hitPit, ePos);
			walker.AttachingDirection = pitTurnDir.ToDirection5();

		} else if (HitCheck(
			entity, eRect.EdgeOutside(walker.AttachingDirection.ToDirection4(), CHECKING_GAP),
			attachDir4, out var hitGroundExpand
		)) {
			// Turn Around
			walker.SetHit(hitGroundExpand, ePos);
			walker.FacingPositive = !walker.FacingPositive;

		} else {
			// Fall
			walker.AttachingRect = default;
			walker.AttachingTarget = null;
			walker.AttachingDirection = Direction5.Center;
			walker.LocalPosition = default;
			walker.AttachingID = 0;
		}

		return walker.AttachingDirection;
	}

	public static Int2 GetAttachingPosition (ISlimeWalker walker) {
		if (walker is not Entity entity) return default;
		var result = entity.XY;
		if (walker.AttachingDirection != Direction5.Center) {
			if (walker.AttachingTarget != null) {
				result = walker.AttachingTarget.Rect.position + walker.LocalPosition;
			} else {
				result = walker.AttachingRect.position + walker.LocalPosition;
			}
		}
		return result;
	}

	public static Int2 GetNextSlimePosition (ISlimeWalker walker) {

		if (walker is not Entity entity) return default;

		var result = entity.XY;
		var dir = walker.AttachingDirection;
		if (dir == Direction5.Center) return result;
		int velocity = walker.FacingPositive ? walker.WalkSpeed : -walker.WalkSpeed;

		switch (dir) {
			case Direction5.Left:
				result.x = walker.AttachingRect.xMax;
				result.y -= velocity;
				break;
			case Direction5.Right:
				result.x = walker.AttachingRect.xMin;
				result.y += velocity;
				break;
			case Direction5.Down:
				result.x += velocity;
				result.y = walker.AttachingRect.yMax;
				break;
			case Direction5.Up:
				result.x -= velocity;
				result.y = walker.AttachingRect.yMin;
				break;
		}

		walker.LocalPosition = walker.AttachingTarget != null ?
			result - walker.AttachingTarget.Rect.position :
			result - walker.AttachingRect.position;

		return result;
	}

	// UTL
	private static bool HitCheckFree (Entity walkerEntity, IRect rect, out PhysicsCell hit, out Direction4 hitDirection) {

		for (int i = 0; i < 4; i++) {

			hitDirection = (Direction4)((i + 1) % 4);

			var _rect = rect.EdgeOutside(hitDirection, 1);
			if (Physics.Overlap(
				PhysicsMask.MAP, _rect, out hit, walkerEntity
			)) return true;

			if (Physics.Overlap(
				PhysicsMask.MAP, _rect, out hit, walkerEntity,
				OperationMode.TriggerOnly, FrameworkUtil.GetOnewayTag(hitDirection.Opposite())
			)) return true;

		}

		hit = default;
		hitDirection = default;
		return false;
	}

	private static bool HitCheck (Entity walkerEntity, IRect rect, Direction4 attDir, out PhysicsCell hit) {
		hit = default;
		int minDis = int.MaxValue;

		var hits = Physics.OverlapAll(PhysicsMask.MAP, rect, out int count, walkerEntity);
		HitCheck(hits, count, walkerEntity, ref minDis, ref hit);

		hits = Physics.OverlapAll(
			PhysicsMask.MAP, rect, out count, walkerEntity,
			OperationMode.TriggerOnly,
			FrameworkUtil.GetOnewayTag(attDir.Opposite())
		);
		HitCheck(hits, count, walkerEntity, ref minDis, ref hit);

		return minDis != int.MaxValue;
	}

	private static void HitCheck (PhysicsCell[] hits, int count, Entity walkerEntity, ref int minDis, ref PhysicsCell hit) {
		int eX = walkerEntity.Rect.CenterX();
		int eY = walkerEntity.Rect.CenterY();
		for (int i = 0; i < count; i++) {
			var _hit = hits[i];
			int dis = (eX - _hit.Rect.CenterX()).Abs() + (eY - _hit.Rect.CenterY()).Abs();
			if (dis < minDis) {
				hit = _hit;
				minDis = dis;
			}
		}
	}

	private static IRect GetGroundCheckRect (IRect eRect, Direction4 attDir4, bool positiveWalk) {
		var rect = eRect.EdgeOutside(attDir4, CHECKING_GAP);
		if (positiveWalk) {
			return attDir4 switch {
				Direction4.Up => rect.LeftHalf(),
				Direction4.Down => rect.RightHalf(),
				Direction4.Left => rect.BottomHalf(),
				Direction4.Right => rect.TopHalf(),
				_ => default,
			};
		} else {
			return attDir4 switch {
				Direction4.Up => rect.RightHalf(),
				Direction4.Down => rect.LeftHalf(),
				Direction4.Left => rect.TopHalf(),
				Direction4.Right => rect.BottomHalf(),
				_ => default,
			};
		}
	}

	private void SetHit (PhysicsCell hit, Int2 ePos) {
		AttachingTarget = hit.Entity;
		AttachingRect = hit.Rect;
		LocalPosition = hit.Entity != null ?
			ePos - hit.Entity.Rect.position :
			ePos - hit.Rect.position;
		AttachingID = hit.SourceID;
	}

}
