using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class eRigidbody : Entity {




		#region --- VAR ---

		// Const
		private const PhysicsMask COL_MASK = PhysicsMask.Level | PhysicsMask.Environment | PhysicsMask.Character;
		private const PhysicsMask ONEWAY_MASK = PhysicsMask.Level | PhysicsMask.Environment;

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
		public bool IsInAir => !IsGrounded && !InWater;
		public bool IsGrounded { get; set; } = false;
		public bool InWater { get; set; } = false;

		// Api-Ser
		public int VelocityX { get; set; } = 0;
		public int VelocityY { get; set; } = 0;
		public int OffsetX { get; set; } = 0;
		public int OffsetY { get; set; } = 0;
		public int GravityScale { get; set; } = 1000;

		// Data
		private static readonly HitInfo[] c_PerformMove = new HitInfo[16];
		//private static readonly HitInfo[] c_Oneway = new HitInfo[16];


		#endregion




		#region --- MSG ---


		public override void OnCreate (int frame) {
			PrevX = X;
			PrevY = Y;
		}


		public override void FillPhysics (int frame) => CellPhysics.FillEntity(CollisionLayer, this);


		public override void PhysicsUpdate (int frame) {

			base.PhysicsUpdate(frame);

			PrevX = X;
			PrevY = Y;
			var rect = Rect;

			// Grounded
			IsGrounded = GroundedCheck(rect);

			// Water
			InWater = CellPhysics.Overlap(
				PhysicsMask.Level, Rect, null,
				CellPhysics.OperationMode.TriggerOnly,
				Const.WATER_TAG
			);

			if (InsideGroundCheck()) {
				X += VelocityX;
				Y += VelocityY;
				return;
			}

			// Gravity
			if (GravityScale != 0) {
				int gravity = Const.GRAVITY * GravityScale / 1000;
				VelocityY = Mathf.Clamp(
					VelocityY - gravity,
					-Const.MAX_GRAVITY_SPEED * (InWater ? Const.WATER_SPEED_LOSE : 1000) / 1000,
					int.MaxValue
				);
			}

			// Vertical Stopping
			if (VelocityY != 0 && !CellPhysics.MoveCheck(
				COL_MASK, rect, this, VelocityY > 0 ? Direction4.Up : Direction4.Down
			)) {
				VelocityY = 0;
			}

			// Move
			PerformMove(VelocityX, VelocityY, false, false);

			if (!IsGrounded) {
				IsGrounded = GroundedCheck(Rect);
			}
		}


		#endregion




		#region --- API ---


		public static int GetPushLevel (Entity entity) => entity is eRigidbody rig ? rig.PushLevel : int.MaxValue;


		protected virtual bool InsideGroundCheck () => CellPhysics.Overlap(
			PhysicsMask.Level, new(
				X + OffsetX + Width / 2,
				Y + OffsetY + Height / 2,
				1, 1
			), this
		);


		protected virtual bool GroundedCheck (RectInt rect) =>
			!CellPhysics.RoomCheck(
				PhysicsMask.Level | PhysicsMask.Environment | PhysicsMask.Character,
				rect, this, Direction4.Down
			) || !CellPhysics.RoomCheck_Oneway(
				rect, this, Direction4.Down, true
			);


		public void PerformMove (int speedX, int speedY, bool ignoreCarry, bool ignoreOneway) {

			int speedScale = InWater ? Const.WATER_SPEED_LOSE : 1000;
			var pos = new Vector2Int(X + OffsetX, Y + OffsetY);

			speedX = speedX * speedScale / 1000;
			speedY = speedY * speedScale / 1000;

			if (ignoreOneway) {
				pos = CellPhysics.MoveIgnoreOneway(COL_MASK, pos, speedX, speedY, new(Width, Height), this);
			} else {
				pos = CellPhysics.Move(COL_MASK, pos, speedX, speedY, new(Width, Height), this, out bool stopX, out bool stopY);
				if (stopX) VelocityX = 0;
				if (stopY) VelocityY = 0;
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

		}


		#endregion




	}
}