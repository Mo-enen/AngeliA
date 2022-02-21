using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class eRigidbody : Entity {




		#region --- VAR ---

		// Api
		public override int X {
			get => base.X;
			set => base.X = PrevX = value;
		}
		public override int Y {
			get => base.Y;
			set => base.Y = PrevY = value;
		}
		public override RectInt Rect => new(X + OffsetX, Y + OffsetY, Width, Height);
		public virtual int PushLevel => 0;
		public virtual PhysicsLayer CollisionLayer { get; } = PhysicsLayer.Character;
		public virtual PhysicsMask CollisionMask { get; } = PhysicsMask.Level | PhysicsMask.Environment | PhysicsMask.Character;
		public virtual bool CarryRigidbodyOnTop => true;
		public int FinalVelocityX => X - PrevX;
		public int FinalVelocityY => Y - PrevY;
		public bool InWater { get; private set; } = false;

		// Api-Ser
		public int VelocityX { get; set; } = 0;
		public int VelocityY { get; set; } = 0;
		public int Gravity { get; set; } = 5;
		public int MaxGravitySpeed { get; set; } = 64;
		public int OffsetX { get; set; } = 0;
		public int OffsetY { get; set; } = 0;

		// Data
		private int PrevX = 0;
		private int PrevY = 0;


		#endregion




		#region --- MSG ---


		public override void OnCreate (int frame) {
			PrevX = X;
			PrevY = Y;
		}


		public override void FillPhysics (int frame) => CellPhysics.FillEntity(CollisionLayer, this);


		public override void PhysicsUpdate (int frame) {

			// Water
			InWater = CellPhysics.Overlap(
				PhysicsMask.Level, Rect, null,
				CellPhysics.OperationMode.TriggerOnly,
				Const.WATER_TAG
			) != null;

			if (InsideGroundCheck()) {
				X += VelocityX;
				Y += VelocityY;
				PrevX = X;
				PrevY = Y;
				return;
			}

			// Gravity
			if (Gravity != 0) {
				VelocityY = Mathf.Clamp(
					VelocityY - Gravity,
					-MaxGravitySpeed * (InWater ? Const.WATER_SPEED_LOSE : 1000) / 1000,
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


		#endregion




		#region --- API ---


		public static int GetPushLevel (Entity entity) =>
			entity == null ? int.MaxValue :
			entity is eRigidbody rig ? rig.PushLevel : 0;


		protected virtual bool InsideGroundCheck () => CellPhysics.Overlap(
			PhysicsMask.Level, new(
				X + OffsetX + Width / 2,
				Y + OffsetY + Height / 2,
				1, 1
			), this
		) != null;


		#endregion




		#region --- LGC ---


		private void PerformMove (int speedX, int speedY, bool carry) {

			int speedScale = InWater ? Const.WATER_SPEED_LOSE : 1000;
			var pos = new Vector2Int(X + OffsetX, Y + OffsetY);

			speedX = speedX * speedScale / 1000;
			speedY = speedY * speedScale / 1000;

			if (Mathf.Abs(speedX) > Const.RIGIDBODY_FAST_SPEED || Mathf.Abs(speedY) > Const.RIGIDBODY_FAST_SPEED) {
				// Too Fast
				int _speedX = speedX;
				int _speedY = speedY;
				while (_speedX != 0 || _speedY != 0) {
					int _sX = Mathf.Clamp(_speedX, -Const.RIGIDBODY_FAST_SPEED, Const.RIGIDBODY_FAST_SPEED);
					int _sY = Mathf.Clamp(_speedY, -Const.RIGIDBODY_FAST_SPEED, Const.RIGIDBODY_FAST_SPEED);
					_speedX -= _sX;
					_speedY -= _sY;
					var newPos = CellPhysics.Move(
						CollisionMask, pos,
						new Vector2Int(pos.x + _sX, pos.y + _sY),
						new(Width, Height), this
					);
					if (newPos == pos) break;
					pos = newPos;
				}
			} else {
				// Normal
				pos = CellPhysics.Move(
					CollisionMask,
					pos,
					new Vector2Int(pos.x + speedX, pos.y + speedY),
					new(Width, Height),
					this
				);
			}

			X = pos.x - OffsetX;
			Y = pos.y - OffsetY;

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
						hitRig.CarryRigidbodyOnTop &&
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


		#endregion







	}
}