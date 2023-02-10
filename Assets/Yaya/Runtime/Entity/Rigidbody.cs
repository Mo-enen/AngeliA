using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class Rigidbody : Entity {




		#region --- VAR ---


		// Api
		public override RectInt Rect => new(X + OffsetX, Y + OffsetY, Width, Height);
		public RectInt PrevRect { get; private set; } = default;
		public bool IsGrounded { get; private set; } = false;
		public bool IsInsideGround { get; set; } = false;
		public int VelocityX { get; set; } = 0;
		public int VelocityY { get; set; } = 0;
		public int OffsetX { get; set; } = 0;
		public int OffsetY { get; set; } = 0;
		public int GravityScale { get; set; } = 1000;
		public int GroundedID { get; private set; } = 0;
		protected bool InWater { get; set; } = false;
		protected bool InSand { get; set; } = false;

		// Override
		protected virtual bool PhysicsEnable => true;
		protected virtual int CollisionMask => YayaConst.MASK_SOLID;
		protected abstract int PhysicsLayer { get; }
		protected virtual int AirDragX => 3;
		protected virtual int AirDragY => 0;
		protected virtual bool DestroyWhenInsideGround => false;
		protected virtual bool IgnoreRiseGravityShift => false;

		// Data
		private int IgnoreGroundCheckFrame = int.MinValue;
		private int IgnoreGravityFrame = int.MinValue;


		#endregion




		#region --- MSG ---


		public override void OnActived () {
			base.OnActived();
			PrevRect = Rect;
			InSand = false;
			InWater = false;
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(PhysicsLayer, this);
			PrevRect = Rect;
		}


		public override void PhysicsUpdate () {

			base.PhysicsUpdate();

			if (!PhysicsEnable) {
				IsInsideGround = InsideGroundCheck();
				IsGrounded = GroundedCheck();
				InWater = CellPhysics.Overlap(YayaConst.MASK_LEVEL, Rect, null, OperationMode.TriggerOnly, YayaConst.WATER_TAG);
				InSand = CellPhysics.Overlap(YayaConst.MASK_LEVEL, Rect, null, OperationMode.TriggerOnly, YayaConst.QUICKSAND_TAG);
				VelocityX = 0;
				VelocityY = 0;
				return;
			}

			var rect = Rect;

			// Grounded
			IsInsideGround = InsideGroundCheck();
			if (IsInsideGround) {
				if (DestroyWhenInsideGround) {
					Active = false;
				} else {
					PerformMove(VelocityX, VelocityY, ignoreLevel: true);
					IsGrounded = GroundedCheck();
				}
				return;
			}


			// Gravity
			if (GravityScale != 0 && Game.GlobalFrame > IgnoreGravityFrame) {
				var pConfig = Game.Current.PhysicsConfig;
				int speedScale = InWater ? YayaConst.WATER_SPEED_LOSE : 1000;
				int gravity = (VelocityY < 0 || IgnoreRiseGravityShift ? pConfig.Gravity : pConfig.GravityRise) * GravityScale / 1000;
				VelocityY = Mathf.Clamp(
					VelocityY - gravity,
					-pConfig.MaxGravitySpeed * speedScale / 1000,
					int.MaxValue
				);
			}

			// Hori Stopping
			if (
				VelocityX != 0 &&
				(!CellPhysics.RoomCheck(CollisionMask, rect, this, VelocityX > 0 ? Direction4.Right : Direction4.Left) ||
				!CellPhysics.RoomCheckOneway(CollisionMask, rect, this, VelocityX > 0 ? Direction4.Right : Direction4.Left, true))
			) VelocityX = 0;

			// Vertical Stopping
			if (
				VelocityY != 0 &&
				(!CellPhysics.RoomCheck(CollisionMask, rect, this, VelocityY > 0 ? Direction4.Up : Direction4.Down) ||
				!CellPhysics.RoomCheckOneway(CollisionMask, rect, this, VelocityY > 0 ? Direction4.Up : Direction4.Down, true))
			) VelocityY = 0;

			// Move
			PerformMove(VelocityX, VelocityY);

			IsGrounded = GroundedCheck();

			// Ari Drag
			if (AirDragX != 0) VelocityX = VelocityX.MoveTowards(0, AirDragX);
			if (AirDragY != 0) VelocityY = VelocityY.MoveTowards(0, AirDragY);

			// Water & Sand
			bool prevInSand = InSand;
			InWater = CellPhysics.Overlap(YayaConst.MASK_LEVEL, Rect, null, OperationMode.TriggerOnly, YayaConst.WATER_TAG);
			InSand = CellPhysics.Overlap(YayaConst.MASK_LEVEL, Rect, null, OperationMode.TriggerOnly, YayaConst.QUICKSAND_TAG);

			// Quicksand
			if (InSand) {
				VelocityX = VelocityX.Clamp(-YayaConst.QUICK_SAND_MAX_RUN_SPEED, YayaConst.QUICK_SAND_MAX_RUN_SPEED);
			}

			// Out Sand
			if (prevInSand && !InSand && VelocityY > 0) {
				VelocityY = Mathf.Max(VelocityY, YayaConst.QUICK_SAND_JUMPOUT_SPEED);
			}

			// Quicksand
			if (InSand) {
				VelocityY = VelocityY.Clamp(-YayaConst.QUICK_SAND_SINK_SPEED, YayaConst.QUICK_SAND_JUMP_SPEED);
			}
		}


		#endregion




		#region --- API ---


		public virtual bool GroundedCheck () {
			if (IsInsideGround || InSand) return true;
			if (Game.GlobalFrame <= IgnoreGroundCheckFrame) return IsGrounded;
			if (VelocityY > 0) return false;
			var rect = Rect;
			bool result = !CellPhysics.RoomCheck(
				CollisionMask, rect, this, Direction4.Down, out var hit
			) || !CellPhysics.RoomCheckOneway(
				CollisionMask, rect, this, Direction4.Down, out hit, true
			);
			GroundedID = result ? hit.SourceID : 0;
			return result;
		}


		protected virtual bool InsideGroundCheck () {
			int sizeX = Width / 8;
			int sizeY = Height / 8;
			return CellPhysics.Overlap(
				YayaConst.MASK_LEVEL, new(
					X + OffsetX + Width / 2 - sizeX / 2,
					Y + OffsetY + Height / 2 - sizeY / 2,
					sizeX, sizeY
				), this
			);
		}


		public void PerformMove (int speedX, int speedY, bool ignoreOneway = false, bool ignoreLevel = false) {

			var pos = new Vector2Int(X + OffsetX, Y + OffsetY);

			int speedScale = InWater ? YayaConst.WATER_SPEED_LOSE : 1000;
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

		}


		public void MakeGrounded (int frame = 0, int blockID = 0) {
			IsGrounded = true;
			IgnoreGroundCheckFrame = Game.GlobalFrame + frame;
			if (blockID != 0) GroundedID = blockID;
		}


		public void MakeNotGrounded (int frame) {
			IsGrounded = false;
			IgnoreGroundCheckFrame = Game.GlobalFrame + frame;
		}


		public void IgnoreGravity (int frame = 0) => IgnoreGravityFrame = Game.GlobalFrame + frame;


		#endregion




	}
}