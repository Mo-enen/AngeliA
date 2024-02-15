namespace AngeliA.Framework; 


public class WoodLogSlopeA : Slope, ICombustible {
	public override Direction2 DirectionVertical => Direction2.Up;
	public override Direction2 DirectionHorizontal => Direction2.Right;
	int ICombustible.BurnStartFrame { get; set; }
}


public class WoodLogSlopeB : Slope, ICombustible {
	public override Direction2 DirectionVertical => Direction2.Up;
	public override Direction2 DirectionHorizontal => Direction2.Left;
	int ICombustible.BurnStartFrame { get; set; }
}


public class BrickWallSlopeA : Slope {
	public override Direction2 DirectionVertical => Direction2.Up;
	public override Direction2 DirectionHorizontal => Direction2.Right;
}


public class BrickWallSlopeB : Slope {
	public override Direction2 DirectionVertical => Direction2.Up;
	public override Direction2 DirectionHorizontal => Direction2.Left;
}


public class BrickWallSlopeC : Slope {
	public override Direction2 DirectionVertical => Direction2.Down;
	public override Direction2 DirectionHorizontal => Direction2.Right;
}


public class BrickWallSlopeD : Slope {
	public override Direction2 DirectionVertical => Direction2.Down;
	public override Direction2 DirectionHorizontal => Direction2.Left;
}



public abstract class Slope : EnvironmentEntity {




	#region --- VAR ---


	// Api
	public abstract Direction2 DirectionVertical { get; }
	public abstract Direction2 DirectionHorizontal { get; }
	public virtual int CollisionMask => PhysicsMask.ENTITY;


	#endregion




	#region --- MSG ---


	public override void FillPhysics () {
		base.FillPhysics();
		CellPhysics.FillEntity(
			PhysicsLayer.ENVIRONMENT, this, true,
			DirectionHorizontal == Direction2.Left ? SpriteTag.ONEWAY_RIGHT_TAG : SpriteTag.ONEWAY_LEFT_TAG
		);
		CellPhysics.FillEntity(
			PhysicsLayer.ENVIRONMENT, this, true,
			DirectionVertical == Direction2.Down ? SpriteTag.ONEWAY_UP_TAG : SpriteTag.ONEWAY_DOWN_TAG
		);
	}


	public override void BeforePhysicsUpdate () {
		base.BeforePhysicsUpdate();
		// Fix Rig
		var hits = CellPhysics.OverlapAll(CollisionMask, Rect, out int count);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is not Rigidbody rig) continue;
			if (CheckOverlap(rig.Rect, out int dis)) FixPosition(rig, dis);
		}
	}


	public override void FrameUpdate () {
		base.FrameUpdate();
		CellRenderer.Draw(TypeID, Rect);
	}


	#endregion




	#region --- LGC ---


	private bool CheckOverlap (IRect rect, out int distance) {
		distance = 0;
		int cornerX = DirectionHorizontal == Direction2.Left ? rect.xMax : rect.xMin;
		if (!cornerX.InRange(Rect.xMin, Rect.xMax)) return false;
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
			// Fix Pos
			target.MakeGrounded(4);
			target.PerformMove(
				DirectionHorizontal == Direction2.Left ? -distance / 2 : distance / 2,
				DirectionVertical == Direction2.Down ? -distance / 2 : distance / 2
			);
			// Fix Velocity
			if (target.VelocityX == 0) {
				// Fix X (Drop)
				target.VelocityY = 0;
				target.IgnoreGravity();
			} else {
				// Fix Y (Walk)
				if ((DirectionHorizontal == Direction2.Left) == (target.VelocityX > 0)) {
					// Walk Toward
					target.Y += DirectionVertical == Direction2.Down ?
						-Util.Abs(target.VelocityX) :
						Util.Abs(target.VelocityX);
					target.VelocityY = 0;
				} else {
					// Walk Away
					target.Y -= Util.Abs(target.VelocityX);
					target.VelocityY = 0;
				}
			}
		} else {
			// Down
			// Fix Pos
			target.PerformMove(DirectionHorizontal == Direction2.Left ? -distance : distance, 0);
		}
	}


	#endregion




}
