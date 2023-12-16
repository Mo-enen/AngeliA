using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class Rigidbody : Entity {




		#region --- VAR ---


		// Const
		private const int WATER_SPEED_LOSE = 400;
		private const int QUICK_SAND_JUMPOUT_SPEED = 48;
		private const int QUICK_SAND_MAX_RUN_SPEED = 4;
		private const int QUICK_SAND_SINK_SPEED = 1;
		private const int QUICK_SAND_JUMP_SPEED = 12;

		// Api
		public delegate void WaterHandler (Rigidbody rig, int x, int y);
		public static event WaterHandler OnFallIntoWater;
		public static event WaterHandler OnJumpOutOfWater;
		public override IRect Rect => new(X + OffsetX, Y + OffsetY, Width, Height);
		public bool IsGrounded { get; private set; } = false;
		public bool IsInsideGround { get; private set; } = false;
		public bool InWater { get; private set; } = false;
		public bool InSand { get; private set; } = false;
		public bool OnSlippy { get; private set; } = false;
		public int VelocityX { get; set; } = 0;
		public int VelocityY { get; set; } = 0;
		public int OffsetX { get; protected set; } = 0;
		public int OffsetY { get; protected set; } = 0;
		public int GravityScale { get; protected set; } = 1000;
		public int GroundedID { get; private set; } = 0;
		public int DeltaPositionX => X - PrevX;
		public int DeltaPositionY => Y - PrevY;

		// Override
		protected abstract int PhysicalLayer { get; }
		protected virtual int CollisionMask => PhysicsMask.SOLID;
		protected virtual int Gravity => VelocityY <= 0 ? 5 : 3;
		protected virtual int AirDragX => 3;
		protected virtual int AirDragY => 0;
		protected virtual bool AllowBeingCarryByOtherRigidbody => true;
		protected virtual bool CarryOtherRigidbodyOnTop => true;
		protected virtual bool PhysicsEnable => true;
		protected virtual bool DestroyWhenInsideGround => false;
		public virtual bool AllowBeingPush => true;
		protected virtual int MaxGravitySpeed => 96;

		// Data
		private int IgnoreGroundCheckFrame = int.MinValue;
		private int IgnoreGravityFrame = int.MinValue;
		private int IgnorePhysicsFrame = -1;
		private int PrevX = 0;
		private int PrevY = 0;
		private int PrevPositionUpdateFrame = -1;


		#endregion




		#region --- MSG ---


		public override void OnActivated () {
			base.OnActivated();
			InSand = false;
			InWater = false;
			OnSlippy = false;
			VelocityX = 0;
			VelocityY = 0;
			IgnoreGroundCheckFrame = int.MinValue;
			IgnoreGravityFrame = int.MinValue;
			PrevX = X;
			PrevY = Y;
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(PhysicalLayer, this);
		}


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			RefreshPrevPosition();
		}


		public override void PhysicsUpdate () {

			base.PhysicsUpdate();
			var rect = Rect;

			bool prevInWater = InWater;
			bool prevInSand = InSand;
			int checkingMask = PhysicsMask.MAP & CollisionMask;
			InWater = CellPhysics.Overlap(checkingMask, rect.Shrink(0, 0, rect.height / 2, 0), null, OperationMode.TriggerOnly, Const.WATER_TAG);
			InSand = !InWater && CellPhysics.Overlap(checkingMask, rect, null, OperationMode.TriggerOnly, Const.QUICKSAND_TAG);
			OnSlippy = !InWater && !InSand && CellPhysics.Overlap(checkingMask, rect.Edge(Direction4.Down), this, OperationMode.ColliderOnly, Const.SLIP_TAG);
			IsInsideGround = InsideGroundCheck();

			if (!PhysicsEnable || Game.GlobalFrame <= IgnorePhysicsFrame) {
				IsGrounded = GroundedCheck();
				if (DestroyWhenInsideGround) Active = false;
				return;
			}

			// Grounded
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
				int speedScale = InWater ? WATER_SPEED_LOSE : 1000;
				VelocityY = Mathf.Clamp(
					VelocityY - Gravity * GravityScale / 1000,
					-MaxGravitySpeed * speedScale / 1000,
					int.MaxValue
				);
			}

			// Hori Stopping
			if (VelocityX != 0) {
				if (!CellPhysics.RoomCheckOneway(CollisionMask, rect, this, VelocityX > 0 ? Direction4.Right : Direction4.Left, true)) {
					VelocityX = 0;
				} else {
					var hits = CellPhysics.OverlapAll(CollisionMask, rect.Edge(VelocityX > 0 ? Direction4.Right : Direction4.Left), out int count, this);
					for (int i = 0; i < count; i++) {
						var hit = hits[i];
						if (hit.Entity is not Rigidbody hitRig) {
							VelocityX = 0;
							break;
						}
						VelocityX = VelocityX < 0 ?
							Mathf.Max(VelocityX, hitRig.VelocityX.LessOrEquelThanZero()) :
							Mathf.Min(VelocityX, hitRig.VelocityX.GreaterOrEquelThanZero());
						if (VelocityX == 0) break;
					}
				}
			}
			// Vertical Stopping
			if (VelocityY != 0) {
				if (!CellPhysics.RoomCheckOneway(CollisionMask, rect, this, VelocityY > 0 ? Direction4.Up : Direction4.Down, true)) {
					VelocityY = 0;
				} else {
					var hits = CellPhysics.OverlapAll(CollisionMask, rect.Edge(VelocityY > 0 ? Direction4.Up : Direction4.Down), out int count, this);
					for (int i = 0; i < count; i++) {
						var hit = hits[i];
						if (hit.Entity is not Rigidbody hitRig) {
							VelocityY = 0;
							break;
						}
						VelocityY = VelocityY < 0 ?
							Mathf.Max(VelocityY, hitRig.VelocityY.LessOrEquelThanZero()) :
							Mathf.Min(VelocityY, hitRig.VelocityY.GreaterOrEquelThanZero());
						if (VelocityY == 0) break;
					}
				}
			}

			// Move
			PerformMove(VelocityX, VelocityY);

			IsGrounded = GroundedCheck();

			// Ari Drag
			if (AirDragX != 0) VelocityX = VelocityX.MoveTowards(0, AirDragX);
			if (AirDragY != 0) VelocityY = VelocityY.MoveTowards(0, AirDragY);

			// Water Splash
			if (prevInWater != InWater && InWater == VelocityY < 0) {
				int waterX = X + OffsetX + Width / 2;
				int waterY = Y + OffsetY + Height / 2 + (InWater ? 0 : -VelocityY);
				if (prevInWater) OnJumpOutOfWater?.Invoke(this, waterX, waterY);
				if (InWater) OnFallIntoWater?.Invoke(this, waterX, waterY);
			}

			// Sand
			if (prevInSand && !InSand && VelocityY > 0) {
				VelocityY = Mathf.Max(VelocityY, QUICK_SAND_JUMPOUT_SPEED);
			}
			if (InSand) {
				VelocityX = VelocityX.Clamp(-QUICK_SAND_MAX_RUN_SPEED, QUICK_SAND_MAX_RUN_SPEED);
				VelocityY = VelocityY.Clamp(-QUICK_SAND_SINK_SPEED, QUICK_SAND_JUMP_SPEED);
			}

		}


		public override void FrameUpdate () {
			// Carry
			if (AllowBeingCarryByOtherRigidbody) {
				int speedLeft = 0;
				int speedRight = 0;
				var hits = CellPhysics.OverlapAll(CollisionMask, Rect.Edge(Direction4.Down), out int count, this);
				for (int i = 0; i < count; i++) {
					var hit = hits[i];
					if (hit.Entity is not Rigidbody rig || !rig.CarryOtherRigidbodyOnTop) continue;
					int deltaX = rig.X - rig.PrevX;
					if (deltaX.Abs() < rig.VelocityX.Abs()) {
						deltaX = rig.VelocityX;
					}
					if (deltaX < 0) {
						speedLeft = Mathf.Min(speedLeft, deltaX);
					} else if (deltaX > 0) {
						speedRight = Mathf.Max(speedRight, deltaX);
					}
				}
				int deltaVelX = speedRight + speedLeft;
				if (deltaVelX != 0) {
					PerformMove(deltaVelX, 0);
				}
			}
			// Base
			base.FrameUpdate();
		}


		#endregion




		#region --- API ---


		public void PerformMove (int speedX, int speedY, bool ignoreOneway = false, bool ignoreLevel = false) {

			if (!PhysicsEnable || Game.GlobalFrame <= IgnorePhysicsFrame) return;
			RefreshPrevPosition();
			var pos = new Int2(X + OffsetX, Y + OffsetY);

			int speedScale = InWater ? WATER_SPEED_LOSE : 1000;
			speedX = speedX * speedScale / 1000;
			speedY = speedY * speedScale / 1000;

			int mask = CollisionMask;
			if (ignoreLevel) mask &= ~PhysicsMask.LEVEL;

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


		public void MakeGrounded (int frame = 1, int blockID = 0) {
			IsGrounded = true;
			IgnoreGroundCheckFrame = Game.GlobalFrame + frame;
			if (blockID != 0) GroundedID = blockID;
		}


		public void MakeNotGrounded () {
			IsGrounded = false;
			GroundedID = 0;
			IgnoreGroundCheckFrame = Game.GlobalFrame + 1;
		}


		public void IgnoreGravity (int duration = 0) => IgnoreGravityFrame = Game.GlobalFrame + duration;


		public void IgnorePhysics (int duration = 1) => IgnorePhysicsFrame = Game.GlobalFrame + duration;


		public virtual void Push (int speedX) => PerformMove(speedX, 0);


		#endregion




		#region --- LGC ---


		private bool GroundedCheck () {
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


		private bool InsideGroundCheck () {
			int sizeX = Width / 8;
			int sizeY = Height / 8;
			return CellPhysics.Overlap(
				PhysicsMask.LEVEL & CollisionMask,
				new IRect(
					X + OffsetX + Width / 2 - sizeX / 2,
					Y + OffsetY + Height / 2 - sizeY / 2,
					sizeX, sizeY
				), this
			);
		}


		private void RefreshPrevPosition () {
			if (Game.GlobalFrame <= PrevPositionUpdateFrame) return;
			PrevPositionUpdateFrame = Game.GlobalFrame;
			PrevX = X;
			PrevY = Y;
		}


		#endregion




	}
}