using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;


namespace Yaya {
	public abstract class eSlope : Entity {




		#region --- VAR ---


		// Api
		public abstract Direction3 DirectionVertical { get; }
		public abstract Direction3 DirectionHorizontal { get; }
		public virtual int CollisionMask => (int)PhysicsMask.Rigidbody;

		// Data
		private readonly HitInfo[] c_Overlap = new HitInfo[16];


		#endregion




		#region --- MSG ---


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			int count = CellPhysics.OverlapAll(c_Overlap, CollisionMask, Rect);
			for (int i = 0; i < count; i++) {
				var hit = c_Overlap[i];
				if (hit.Entity is not eRigidbody rig) continue;
				if (CheckOverlap(rig.Rect, out int dis)) FixPosition(rig, dis);
			}
		}


		#endregion




		#region --- LGC ---


		private bool CheckOverlap (RectInt rect, out int distance) {
			distance = 0;
			int cornerX = DirectionHorizontal == Direction3.Left ? rect.xMax : rect.xMin;
			if (!cornerX.InRange(Rect.xMin, Rect.xMax)) return false;
			int cornerY = DirectionVertical == Direction3.Up ? rect.yMin : rect.yMax;
			if ((DirectionHorizontal == Direction3.Left) == (DirectionVertical == Direction3.Up)) {
				// LU, RD ◿ ◸
				distance = cornerY - (Rect.yMin + (cornerX - Rect.x));
			} else {
				// RU, LD ◺◹
				distance = cornerY - (Rect.yMax - (cornerX - Rect.x));
			}
			bool underTarget = distance < 0;
			distance = Mathf.Abs(distance);
			return (DirectionVertical == Direction3.Up) == underTarget;
		}


		private void FixPosition (eRigidbody target, int distance) {

			// Fix Pos
			if (DirectionVertical == Direction3.Up) {
				target.MakeGrounded();
				target.X += DirectionHorizontal == Direction3.Left ? -distance / 2 : distance / 2;
				target.Y += DirectionVertical == Direction3.Down ? -distance / 2 : distance / 2;
			} else {
				if (target.VelocityX == 0) {
					if (DirectionVertical == Direction3.Up) distance--;
					target.X += DirectionHorizontal == Direction3.Left ? -distance : distance;
				} else {
					target.Y += DirectionVertical == Direction3.Down ? -distance : distance;
				}
			}

			// Fix Velocity
			if (target.VelocityX == 0) {
				// Fix X (Drop)
				if (DirectionVertical == Direction3.Up) target.VelocityY = 0;
				target.IgnoreGravity();
			} else {
				// Fix Y (Walk)
				if ((DirectionHorizontal == Direction3.Left) == (target.VelocityX > 0)) {
					// Walk Toward
					target.VelocityY = DirectionVertical == Direction3.Down ?
						-Mathf.Abs(target.VelocityX) :
						Mathf.Abs(target.VelocityX);
				}
			}
		}


		#endregion




	}
}
