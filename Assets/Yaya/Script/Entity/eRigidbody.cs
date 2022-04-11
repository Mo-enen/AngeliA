using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eRigidbody : Entity {




		#region --- VAR ---


		// Api
		public int FinalVelocityX => X - PrevX;
		public int FinalVelocityY => Y - PrevY;
		public override int PushLevel => 0;
		public int PrevX { get; private set; } = 0;
		public int PrevY { get; private set; } = 0;
		public override RectInt Rect => new(X + OffsetX, Y + OffsetY, Width, Height);
		public bool IsGrounded { get; set; } = false;
		public bool InWater { get; set; } = false;
		public bool InsideGround { get; set; } = false;

		// Virtual
		public virtual int CollisionLayer { get; } = 0;
		public virtual bool CarryRigidbodyOnTop => true;
		public virtual bool IsInAir => !IsGrounded && !InWater;
		public virtual bool DestroyOnInsideGround => false;
		public virtual int AirDragX { get; } = 3;
		public virtual int AirDragY { get; } = 0;

		// Api-Ser
		public int VelocityX { get; set; } = 0;
		public int VelocityY { get; set; } = 0;
		public int OffsetX { get; set; } = 0;
		public int OffsetY { get; set; } = 0;
		public int GravityScale { get; set; } = 1000;

		// Data
		private static readonly HitInfo[] c_PerformMove = new HitInfo[16];


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
			InsideGround = InsideGroundCheck();

			// Water
			InWater = CellPhysics.Overlap((int)PhysicsMask.Level, Rect, null, OperationMode.TriggerOnly, Const.WATER_TAG);

			if (InsideGround) {
				X += VelocityX;
				Y += VelocityY;
				if (DestroyOnInsideGround) Active = false;
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

			// Hori Stopping
			if (VelocityX != 0 && !CellPhysics.MoveCheck(
				(int)PhysicsMask.Solid, rect, this, VelocityX > 0 ? Direction4.Right : Direction4.Left
			)) {
				VelocityX = 0;
			}

			// Vertical Stopping
			if (VelocityY != 0 && !CellPhysics.MoveCheck(
				(int)PhysicsMask.Solid, rect, this, VelocityY > 0 ? Direction4.Up : Direction4.Down
			)) {
				VelocityY = 0;
			}

			// Move
			PerformMove(VelocityX, VelocityY, false, false);

			if (!IsGrounded) {
				IsGrounded = GroundedCheck(Rect);
			}

			// Ari Drag
			VelocityX = VelocityX.MoveTowards(0, AirDragX);
			VelocityY = VelocityY.MoveTowards(0, AirDragY);
		}


		#endregion




		#region --- API ---


		protected virtual bool InsideGroundCheck () => CellPhysics.Overlap(
			(int)PhysicsMask.Level, new(
				X + OffsetX + Width / 2,
				Y + OffsetY + Height / 2,
				1, 1
			), this
		);


		protected virtual bool GroundedCheck (RectInt rect) =>
			!CellPhysics.RoomCheck(
				(int)PhysicsMask.Solid,
				rect, this, Direction4.Down
			) || !CellPhysics.RoomCheck_Oneway(
				(int)PhysicsMask.Map, rect, this, Direction4.Down, true
			);


		public void PerformMove (int speedX, int speedY, bool ignoreCarry, bool ignoreOneway) {

			int speedScale = InWater ? Const.WATER_SPEED_LOSE : 1000;
			var pos = new Vector2Int(X + OffsetX, Y + OffsetY);

			speedX = speedX * speedScale / 1000;
			speedY = speedY * speedScale / 1000;

			int mask = (int)PhysicsMask.Solid;

			if (ignoreOneway) {
				pos = CellPhysics.MoveIgnoreOneway(mask, pos, speedX, speedY, new(Width, Height), this);
			} else {
				pos = CellPhysics.Move(mask, pos, speedX, speedY, new(Width, Height), this, out bool stopX, out bool stopY);
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
				int count = CellPhysics.OverlapAll(c_PerformMove, mask, new(X + OffsetX, Y + OffsetY - GAP, Width, GAP), this);
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
					PerformMove(finalL + finalR, 0, true, false);
				}
			}

		}


		#endregion




	}
}