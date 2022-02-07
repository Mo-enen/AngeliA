using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class eRigidbody : Entity {


		// Const
		private static readonly int WATER_TAG = "Water".ACode();
		private const int WATER_RATE = 400;

		// Api
		public int FinalVelocityX => X - PrevX;
		public int FinalVelocityY => Y - PrevY;
		public virtual int PushLevel => 0;
		public virtual bool CarryOnTop => true;
		public bool InWater { get; private set; } = false;

		// Api-Ser
		public PhysicsLayer Layer = PhysicsLayer.Character;
		public PhysicsMask CollisionMask = PhysicsMask.Level | PhysicsMask.Environment | PhysicsMask.Character;
		public int VelocityX = 0;
		public int VelocityY = 0;
		public int Gravity = 5;
		public int MaxGravitySpeed = 64;
		public int OffsetX = 0;
		public int OffsetY = 0;

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
			// Water
			InWater = CellPhysics.Overlap(
				PhysicsMask.Level, Rect, null,
				CellPhysics.OperationMode.TriggerOnly,
				WATER_TAG
			) != null;
			// Gravity
			if (Gravity != 0) {
				VelocityY = Mathf.Clamp(
					VelocityY - Gravity,
					-MaxGravitySpeed * (InWater ? WATER_RATE : 1000) / 1000,
					int.MaxValue
				);
			}
			// Vertical Stopping
			if (VelocityY != 0 && CellPhysics.StopCheck(
				CollisionMask, this, VelocityY > 0 ? Direction4.Up : Direction4.Down
			)) {
				VelocityY = 0;
			}
			// Move
			PrevX = X;
			PrevY = Y;
			PerformMove(VelocityX, VelocityY, true);
		}


		private void PerformMove (int speedX, int speedY, bool carry) {

			int speedScale = InWater ? WATER_RATE : 1000;
			var pos = new Vector2Int(X + OffsetX, Y + OffsetY);
			var newPos = new Vector2Int(
				pos.x + speedX * speedScale / 1000,
				pos.y + speedY * speedScale / 1000
			);

			var _pos = CellPhysics.Move(
				CollisionMask, pos,
				newPos, new(Width, Height), this
			);

			X = _pos.x - OffsetX;
			Y = _pos.y - OffsetY;

			// Carry
			if (carry && speedY <= 0) {
				const int GAP = 1;
				int count = CellPhysics.ForAllOverlaps(
					CollisionMask, new(X + OffsetX, Y + OffsetY - GAP, Width, GAP), this
				);
				int finalL = 0;
				int finalR = 0;
				for (int i = 0; i < count; i++) {
					var hit = CellPhysics.OverlapResults[i];
					if (
						hit.Entity is eRigidbody hitRig &&
						hitRig.CarryOnTop &&
						hitRig.FinalVelocityX != 0 &&
						hitRig.Rect.yMax == Rect.y
					) {
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


		// API
		public static int GetPushLevel (Entity entity) =>
			entity == null ? int.MaxValue :
			entity is eRigidbody rig ? rig.PushLevel : 0;


	}
}