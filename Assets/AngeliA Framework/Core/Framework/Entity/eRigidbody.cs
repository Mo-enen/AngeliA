using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class eRigidbody : Entity {




		#region --- VAR ---

		// Const
		private const PhysicsMask COL_MASK = PhysicsMask.Level | PhysicsMask.Environment | PhysicsMask.Character;

		// Api
		public int FinalVelocityX => X - PrevX;
		public int FinalVelocityY => Y - PrevY;
		public override int X { get; set; } = 0;
		public override int Y { get; set; } = 0;
		public int PrevX { get; private set; } = 0;
		public int PrevY { get; private set; } = 0;
		public override RectInt Rect => new(X + OffsetX, Y + OffsetY, Width, Height);
		public virtual int PushLevel => 0;
		public virtual PhysicsLayer CollisionLayer { get; } = PhysicsLayer.Character;
		public virtual bool CarryRigidbodyOnTop => true;
		public bool IsGrounded { get; set; } = false;
		public bool InWater { get; set; } = false;

		// Api-Ser
		public int VelocityX { get; set; } = 0;
		public int VelocityY { get; set; } = 0;
		public int Gravity { get; set; } = 5;
		public int MaxGravitySpeed { get; set; } = 64;
		public int OffsetX { get; set; } = 0;
		public int OffsetY { get; set; } = 0;

		// Data
		private static readonly HitInfo[] c_PerformMove = new HitInfo[16];
		private static readonly HitInfo[] c_Oneway = new HitInfo[16];


		#endregion




		#region --- MSG ---


		public override void OnCreate (int frame) {
			PrevX = X;
			PrevY = Y;
		}


		public override void FillPhysics (int frame) => CellPhysics.FillEntity(CollisionLayer, this);


		public override void PhysicsUpdate (int frame) {

			PrevX = X;
			PrevY = Y;
			var rect = Rect;

			// Grounded
			IsGrounded = !CellPhysics.RoomCheck(
				PhysicsMask.Level | PhysicsMask.Environment | PhysicsMask.Character,
				rect, this, Direction4.Down
			) || !CellPhysics.RoomCheck_Oneway(
				rect, this, Direction4.Down, true
			);

			// Water
			InWater = CellPhysics.Overlap(
				PhysicsMask.Level, Rect, null,
				CellPhysics.OperationMode.TriggerOnly,
				Const.WATER_TAG
			) != null;

			if (InsideGroundCheck()) {
				X += VelocityX;
				Y += VelocityY;
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
				COL_MASK, rect, this, VelocityY > 0 ? Direction4.Up : Direction4.Down
			)) {
				VelocityY = 0;
			}

			// Move
			PerformMove(VelocityX, VelocityY, false, false);
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


		private void PerformMove (int speedX, int speedY, bool ignoreCarry, bool ignoreOneway) {

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
						COL_MASK, pos,
						new Vector2Int(pos.x + _sX, pos.y + _sY),
						new(Width, Height), this
					);
					if (newPos == pos) break;
					pos = newPos;
				}
			} else {
				// Normal
				pos = CellPhysics.Move(
					COL_MASK,
					pos,
					new Vector2Int(pos.x + speedX, pos.y + speedY),
					new(Width, Height),
					this
				);
			}

			X = pos.x - OffsetX;
			Y = pos.y - OffsetY;

			// Being Carry
			if (!ignoreCarry && speedY <= 0) {
				const int GAP = 1;
				int finalL = 0;
				int finalR = 0;
				int count = CellPhysics.OverlapAll(c_PerformMove, COL_MASK, new(X + OffsetX, Y + OffsetY - GAP, Width, GAP), this);
				for (int i = 0; i < count; i++) {
					var hit = c_PerformMove[i];
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
				c_PerformMove.Dispose();
				if (finalL + finalR != 0) {
					PerformMove(finalL + finalR, 0, true, true);
				}
			}

			// Oneway
			if (!ignoreOneway) {
				var rect = Rect;
				int oCount = CellPhysics.OverlapAll(c_Oneway, PhysicsLayer.Environment, rect, this, CellPhysics.OperationMode.TriggerOnly, Const.ONEWAY_TAG);
				for (int i = 0; i < oCount; i++) {
					var hit = c_Oneway[i];
					if (hit.Entity is eOneway oneway) {
						if (!oneway.PassCheck(
							new(PrevX + OffsetX, PrevY + OffsetY),
							new(X + OffsetX, Y + OffsetY),
							new(Width, Height),
							out var newPos
						)) {
							X = newPos.x - OffsetX;
							Y = newPos.y - OffsetY;
							switch (oneway.GateDirection) {
								case Direction4.Up:
								case Direction4.Down:
									VelocityY = 0;
									break;
								case Direction4.Left:
								case Direction4.Right:
									VelocityX = 0;
									break;
							}
						}
					}
				}
				c_Oneway.Dispose();
			}

		}


		#endregion




	}
}