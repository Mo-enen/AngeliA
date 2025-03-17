using AngeliA;

namespace AngeliA.Platformer;

[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[EntityAttribute.StageOrder(-2048)]
public abstract class Slope : Entity, IBlockEntity {




	#region --- VAR ---


	// Api
	public abstract Direction2 DirectionVertical { get; }
	public abstract Direction2 DirectionHorizontal { get; }
	public virtual int CollisionMask => PhysicsMask.DYNAMIC;


	#endregion




	#region --- MSG ---


	public override void FirstUpdate () {
		base.FirstUpdate();
		// Edge
		Physics.FillBlock(
			PhysicsLayer.ENVIRONMENT, TypeID, Rect, true,
			(DirectionHorizontal == Direction2.Left ? Tag.OnewayRight : Tag.OnewayLeft) | Tag.Mark
		);
		Physics.FillEntity(
			PhysicsLayer.ENVIRONMENT, this, true,
			(DirectionVertical == Direction2.Down ? Tag.OnewayUp : Tag.OnewayDown) | Tag.Mark
		);
		// Anti Through
		if (DirectionVertical == Direction2.Up) {
			Physics.FillBlock(
				PhysicsLayer.ENVIRONMENT, TypeID,
				DirectionHorizontal == Direction2.Left ?
				new IRect(X + Width, Y + 64, 1, 128) : new IRect(X, Y + 64, 1, 128),
				true, Tag.OnewayUp | Tag.Mark
			);
			Physics.FillBlock(
				PhysicsLayer.ENVIRONMENT, TypeID,
				DirectionHorizontal == Direction2.Left ?
				new IRect(X, Y - 1, 128, 1) : new IRect(X + Width - 128, Y - 1, 128, 1),
				true, DirectionHorizontal == Direction2.Left ? Tag.OnewayLeft | Tag.Mark : Tag.OnewayRight | Tag.Mark
			);
		} else {
			Physics.FillBlock(
				PhysicsLayer.ENVIRONMENT, TypeID,
				DirectionHorizontal == Direction2.Left ? Rect.EdgeOutsideRight(1) : Rect.EdgeOutsideLeft(1),
				true, Tag.OnewayDown | Tag.Mark
			);
		}
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		// Ignore Crossed Blocks
		if (DirectionVertical == Direction2.Up) {
			if (Physics.HasEntity<Slope>(IRect.Point(
				DirectionHorizontal == Direction2.Left ? X - Width / 2 : X + Width + Width / 2,
				Y - Height / 2
			), PhysicsMask.ENVIRONMENT, this, OperationMode.TriggerOnly)) {
				Physics.IgnoreOverlap(PhysicsMask.MAP, IRect.Point(
					DirectionHorizontal == Direction2.Left ? X + 1 : X + Width - 1,
					Y - 1
				), OperationMode.ColliderOnly);
			}
		}
		// Fix Rig
		var hits = Physics.OverlapAll(CollisionMask, Rect, out int count, this, OperationMode.ColliderAndTrigger);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is not Rigidbody rig) continue;
			if (rig is IAutoTrackWalker || rig is IRouteWalker || rig is ISlimeWalker) continue;
			if (CheckOverlap(rig.Rect, out int dis)) {
				FixPosition(rig, dis);
			}
		}
	}


	public override void LateUpdate () {
		base.LateUpdate();
		Draw();
	}


	#endregion




	#region --- LGC ---


	private bool CheckOverlap (IRect rect, out int distance) {
		distance = 0;
		//if (DirectionVertical == Direction2.Up && rect.y < Y) return false;
		int cornerX = DirectionHorizontal == Direction2.Left ? rect.xMax : rect.xMin;
		if (!cornerX.InRangeInclude(Rect.xMin, Rect.xMax)) return false;
		int cornerY = DirectionVertical == Direction2.Up ? rect.yMin : rect.yMax;
		if ((DirectionHorizontal == Direction2.Left) == (DirectionVertical == Direction2.Up)) {
			// LU, RD ◿ ◸
			distance = cornerY - (Rect.yMin + (cornerX - Rect.x));
		} else {
			// RU, LD ◺◹
			distance = cornerY - (Rect.yMax - (cornerX - Rect.x));
		}
		bool underTarget = distance < 0;
		distance = Util.Abs(distance);
		return (DirectionVertical == Direction2.Up) == underTarget;
	}


	private void FixPosition (Rigidbody target, int distance) {
		if (DirectionVertical == Direction2.Up) {
			// ◿ ◺
			if (target.VelocityY > 0) return;
			// Fix Pos
			target.MakeGrounded(1);
			target.PerformMove(
				DirectionHorizontal == Direction2.Left ? -distance / 2 : distance / 2,
				distance / 2
			);
			// Fix Velocity
			int finalVelX = target.VelocityX + target.MomentumX.value;
			if (finalVelX == 0) {
				// Fix X (Drop)
				target.VelocityY = 0;
				target.IgnoreGravity.True();
			} else {
				// Fix Y (Walk)
				if ((DirectionHorizontal == Direction2.Left) == (finalVelX > 0)) {
					// Walk Toward
					target.PerformMove(0, Util.Abs(finalVelX));
					target.VelocityY = 0;
				} else {
					// Walk Away
					target.PerformMove(0, -Util.Abs(finalVelX));
					target.VelocityY = 0;
				}
			}
		} else {
			// ◹ ◸
			// Down
			// Fix Pos
			target.PerformMove(DirectionHorizontal == Direction2.Left ? -distance : distance, 0);
		}
	}


	#endregion




}
