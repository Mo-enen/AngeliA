using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eRigidbody : Entity, IInitialize {




		#region --- VAR ---


		// Api
		public int FinalVelocityX => X - PrevX;
		public int FinalVelocityY => Y - PrevY;
		public int PrevX { get; private set; } = 0;
		public int PrevY { get; private set; } = 0;
		public override RectInt Rect => new(X + OffsetX, Y + OffsetY, Width, Height);
		public bool IsGrounded { get; private set; } = false;
		public bool InWater { get; set; } = false;
		public bool InSand { get; set; } = false;
		public bool InsideGround { get; set; } = false;

		// Virtual
		public virtual int PhysicsLayer { get; } = 0;
		public virtual int CollisionMask { get; } = YayaConst.MASK_SOLID;
		public virtual bool CarryRigidbodyOnTop => true;
		public virtual bool IsInAir => !IsGrounded && !InWater;
		public virtual bool DestroyOnInsideGround => false;
		public virtual int AirDragX { get; } = 3;
		public virtual int AirDragY { get; } = 0;
		public virtual bool IgnoreRiseGravityShift => false;

		// Api-Ser
		public int VelocityX { get; set; } = 0;
		public int VelocityY { get; set; } = 0;
		public int OffsetX { get; set; } = 0;
		public int OffsetY { get; set; } = 0;
		public int GravityScale { get; set; } = 1000;

		// Data
		private static readonly HitInfo[] c_PerformMove = new HitInfo[16];
		private static PhysicsMeta PhysicsMeta = null;
		private int IgnoreGroundCheckFrame = int.MinValue;
		private int IgnoreGravityFrame = int.MinValue;


		#endregion




		#region --- MSG ---


		public static void InitializeWithGame (Game game) {
			if (game is Yaya yaya) {
				PhysicsMeta = yaya.PhysicsMeta ?? new();
			}
		}


		public override void OnActived () {
			PrevX = X;
			PrevY = Y;
			InSand = false;
			InWater = false;  																																			
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(PhysicsLayer, this);
		}


		public override void PhysicsUpdate () {

			int frame = Game.GlobalFrame;
			base.PhysicsUpdate();

			PrevX = X;
			PrevY = Y;
			var rect = Rect;

			// Grounded
			InsideGround = InsideGroundCheck();
			IsGrounded = InsideGround || GroundedCheck(rect);

			// Water & Sand
			bool prevInSand = InSand;
			InWater = CellPhysics.Overlap(YayaConst.MASK_LEVEL, Rect, null, OperationMode.TriggerOnly, YayaConst.WATER_TAG);
			InSand = CellPhysics.Overlap(YayaConst.MASK_LEVEL, Rect, null, OperationMode.TriggerOnly, YayaConst.QUICKSAND_TAG);

			if (InsideGround) {
				if (DestroyOnInsideGround) Active = false;
				PerformMove(VelocityX, VelocityY, false, false, true);
				return;
			}

			// Gravity
			if (GravityScale != 0 && frame > IgnoreGravityFrame) {
				int gravity = (VelocityY < 0 || IgnoreRiseGravityShift ? PhysicsMeta.Gravity : PhysicsMeta.GravityRise) * GravityScale / 1000;
				VelocityY = Mathf.Clamp(
					VelocityY - gravity,
					-PhysicsMeta.MaxGravitySpeed * (InWater ? PhysicsMeta.WaterSpeedLose : 1000) / 1000,
					int.MaxValue
				);
			}

			// Out Sand
			if (prevInSand && !InSand) {
				VelocityY = Mathf.Max(VelocityY, PhysicsMeta.QuicksandJumpOutSpeed);
			}

			// Hori Stopping
			if (
				VelocityX != 0 &&
				(!CellPhysics.RoomCheck(CollisionMask, rect, this, VelocityX > 0 ? Direction4.Right : Direction4.Left) ||
				!CellPhysics.RoomCheck_Oneway(CollisionMask, rect, this, VelocityX > 0 ? Direction4.Right : Direction4.Left, true))
			) VelocityX = VelocityX.Clamp(-1, 1);

			// Vertical Stopping
			if (
				VelocityY != 0 &&
				(!CellPhysics.RoomCheck(CollisionMask, rect, this, VelocityY > 0 ? Direction4.Up : Direction4.Down) ||
				!CellPhysics.RoomCheck_Oneway(CollisionMask, rect, this, VelocityY > 0 ? Direction4.Up : Direction4.Down, true))
			) VelocityY = 0;

			// Quicksand
			if (InSand) {
				VelocityX = VelocityX.Clamp(-PhysicsMeta.QuicksandMaxRunSpeed, PhysicsMeta.QuicksandMaxRunSpeed);
				IsGrounded = true;
			}

			// Move
			PerformMove(VelocityX, VelocityY);

			if (!IsGrounded) IsGrounded = GroundedCheck(Rect);

			// Ari Drag
			if (AirDragX != 0) VelocityX = VelocityX.MoveTowards(0, AirDragX);
			if (AirDragY != 0) VelocityY = VelocityY.MoveTowards(0, AirDragY);

			// Quicksand
			if (InSand) {
				VelocityY = VelocityY.Clamp(-PhysicsMeta.QuicksandSinkSpeed, PhysicsMeta.QuicksandJumpSpeed);
			}
		}


		#endregion




		#region --- API ---


		protected virtual bool InsideGroundCheck () => CellPhysics.Overlap(
			YayaConst.MASK_LEVEL, new(
				X + OffsetX + Width / 2,
				Y + OffsetY + Height / 2,
				1, 1
			), this
		);


		protected virtual bool GroundedCheck (RectInt rect) {
			if (Game.GlobalFrame <= IgnoreGroundCheckFrame) return IsGrounded;
			return !CellPhysics.RoomCheck(
				CollisionMask,
				rect, this, Direction4.Down
			) || !CellPhysics.RoomCheck_Oneway(
				YayaConst.MASK_MAP, rect, this, Direction4.Down, true
			);
		}


		public void PerformMove (int speedX, int speedY, bool ignoreCarry = false, bool ignoreOneway = false, bool ignoreLevel = false) {

			int speedScale = InWater ? PhysicsMeta.WaterSpeedLose : 1000;
			var pos = new Vector2Int(X + OffsetX, Y + OffsetY);

			speedX = speedX * speedScale / 1000;
			speedY = speedY * speedScale / 1000;

			int mask = CollisionMask;
			if (ignoreLevel) mask &= ~YayaConst.MASK_LEVEL;

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
				if (finalL + finalR != 0) {
					PerformMove(finalL + finalR, 0, true, false);
				}
			}

		}


		public void SetPosition (int x, int y) {
			if (x != X) X = PrevX = x;
			if (y != Y) Y = PrevY = y;
		}


		public void MakeGrounded (int frame = 0) {
			IsGrounded = true;
			IgnoreGroundCheckFrame = Game.GlobalFrame + frame;
		}


		public void IgnoreGravity (int frame = 0) => IgnoreGravityFrame = Game.GlobalFrame + frame;


		#endregion




	}
}