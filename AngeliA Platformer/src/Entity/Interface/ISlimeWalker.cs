using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

internal interface ISlimeWalker {

	// VAR
	/// <summary>
	/// When this value is "Up" the walker will be upside-down. "Center" means not attaching anything.
	/// </summary>
	public Direction5 AttachingDirection { get; set; }
	public Int2 LocalPosition { get; set; }
	public Entity AttachingTarget { get; set; }
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

		// Free Fall
		if (walker.AttachingDirection == Direction5.Center) {
			var eRect = entity.Rect;
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
		const int Q = Const.QUARTER;
		var groundPoint = (IRect)default;
		var wallPoint = (IRect)default;
		var pitPoint = (IRect)default;
		switch (walker.AttachingDirection) {
			case Direction5.Up:
				groundPoint = IRect.Point(entity.X, entity.Y + 1);
				pitPoint = IRect.Point(entity.X + (walker.FacingPositive ? Q : -Q), entity.Y + 1);
				wallPoint = IRect.Point(
					entity.X + (walker.FacingPositive ? -Q : Q),
					entity.Y - Q
				);
				break;
			case Direction5.Down:
				groundPoint = IRect.Point(entity.X, entity.Y - 1);
				pitPoint = IRect.Point(entity.X + (walker.FacingPositive ? -Q : Q), entity.Y - 1);
				wallPoint = IRect.Point(
					entity.X + (walker.FacingPositive ? Q : -Q),
					entity.Y + Q
				);
				break;
			case Direction5.Left:
				groundPoint = IRect.Point(entity.X - 1, entity.Y);
				pitPoint = IRect.Point(entity.X - 1, entity.Y + (walker.FacingPositive ? -Q : Q));
				wallPoint = IRect.Point(
					entity.X + Q,
					entity.Y + (walker.FacingPositive ? Q : -Q)
				);
				break;
			case Direction5.Right:
				groundPoint = IRect.Point(entity.X + 1, entity.Y);
				pitPoint = IRect.Point(entity.X + 1, entity.Y + (walker.FacingPositive ? Q : -Q));
				wallPoint = IRect.Point(
					entity.X - Q,
					entity.Y + (walker.FacingPositive ? -Q : Q)
				);
				break;
		}

		// Chech for Turning
		if (walker.HitCheck(entity, wallPoint, out var hitWall)) {
			// Wall Turn
			walker.SetHit(hitWall, ePos);
			walker.AttachingDirection = walker.FacingPositive ? walker.AttachingDirection.AntiClockwise() : walker.AttachingDirection.Clockwise();
		} else if (walker.HitCheck(entity, groundPoint, out var hitGround)) {
			// Keep Walking
			walker.SetHit(hitGround, ePos);
		} else if (walker.HitCheck(entity, pitPoint, out var hitPit)) {
			// Pit Turn
			walker.SetHit(hitPit, ePos);
			walker.AttachingDirection = walker.FacingPositive ? walker.AttachingDirection.Clockwise() : walker.AttachingDirection.AntiClockwise();
		} else {
			// Fall
			walker.AttachingRect = default;
			walker.AttachingTarget = null;
			walker.AttachingDirection = Direction5.Center;
		}

		return walker.AttachingDirection;
	}

	public static Int2 GetNextSlimePosition (ISlimeWalker walker) {

		if (walker is not Entity entity) return default;

		var result = entity.XY;
		var dir = walker.AttachingDirection;
		if (dir == Direction5.Center) return result;





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

	private bool HitCheck (Entity walkerEntity, IRect rect, out PhysicsCell hit) =>
		Physics.Overlap(
			PhysicsMask.MAP, rect, out hit, walkerEntity
		) || Physics.Overlap(
			PhysicsMask.MAP, rect, out hit, walkerEntity,
			OperationMode.TriggerOnly,
			FrameworkUtil.GetOnewayTag(AttachingDirection.Opposite().ToDirection4())
		);

	private void SetHit (PhysicsCell hit, Int2 ePos) {
		AttachingTarget = hit.Entity;
		AttachingRect = hit.Rect;
		LocalPosition = hit.Entity != null ?
			new Int2(hit.Entity.X, hit.Entity.Y) - ePos :
			hit.Rect.position - ePos;
	}

}
