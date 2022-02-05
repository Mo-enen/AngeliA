using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework.Physics;


namespace AngeliaFramework.Entities {
	public abstract class eRigidbody : Entity {


		// Api
		public int FinalVelocityX => X - PrevX;
		public int FinalVelocityY => Y - PrevY;

		// Api-Ser
		public PhysicsLayer Layer = PhysicsLayer.Character;
		public PhysicsMask CollisionMask = PhysicsMask.Level | PhysicsMask.Environment | PhysicsMask.Character;
		public int VelocityX = 0;
		public int VelocityY = 0;
		public int Gravity = 5;
		public int MaxGravitySpeed = 64;
		public int OffsetX = 0;
		public int OffsetY = 0;
		public int SpeedScale = 1000;

		// Data
		private int PrevX = 0;
		private int PrevY = 0;


		public override RectInt Rect => new(X + OffsetX, Y + OffsetY, Width, Height);


		// MSG
		public override void OnCreate (int frame) {
			PrevX = X;
			PrevY = Y;
		}


		public override void FillPhysics (int frame) => CellPhysics.FillEntity(Layer, this);


		public override void PhysicsUpdate (int frame) {

			if (Gravity != 0) {
				VelocityY = Mathf.Clamp(VelocityY - Gravity, -MaxGravitySpeed, int.MaxValue);
			}

			PrevX = X;
			PrevY = Y;

			PerformMove(VelocityX, VelocityY, true);

		}


		private void PerformMove (int speedX, int speedY, bool carry) {

			var pos = new Vector2Int(X + OffsetX, Y + OffsetY);
			var newPos = new Vector2Int(
				pos.x + speedX * SpeedScale / 1000,
				pos.y + speedY * SpeedScale / 1000
			);

			var _pos = CellPhysics.Move(
				CollisionMask, pos,
				newPos, new(Width, Height), this
			);

			X = _pos.x - OffsetX;
			Y = _pos.y - OffsetY;

			// Carry
			if (carry && speedY <= 0) {
				const int GAP = 2;
				int count = CellPhysics.ForAllOverlaps(CollisionMask, new(X + OffsetX, Y + OffsetY - GAP, Width, GAP), out var results, this);
				int finalL = 0;
				int finalR = 0;
				for (int i = 0; i < count; i++) {
					var hit = results[i];
					if (hit != null && hit.Entity is eRigidbody hitRig && hitRig.FinalVelocityX != 0) {
						if (hitRig.FinalVelocityX < 0) {
							// L
							if (Mathf.Abs(hitRig.FinalVelocityX) > Mathf.Abs(finalL)) {
								finalL = hitRig.FinalVelocityX;
							}
						} else {
							// R
							if (Mathf.Abs(hitRig.FinalVelocityX) > Mathf.Abs(finalR)) {
								finalR = hitRig.FinalVelocityX;
							}
						}
					}
				}
				if (finalL + finalR != 0) {
					PerformMove(finalL + finalR, 0, false);
				}
			}

		}


	}
}