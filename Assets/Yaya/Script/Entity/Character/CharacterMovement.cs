using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[System.Serializable]
	public class CharacterMovement {




		#region --- VAR ---


		// Const
		private const int JUMP_TOLERANCE = 4;
		private const int JUMP_GAP = 1;
		private const int CLIMB_CORRECT_DELTA = 36;
		private const int RUN_BREAK_GAP = 6;

		// Api
		public bool IsDashing { get; private set; } = false;
		public bool IsSquating { get; private set; } = false;
		public bool IsPounding { get; private set; } = false;
		public bool IsInsideGround => Rig.InsideGround;
		public bool IsClimbing { get; private set; } = false;
		public bool IsGrounded => Rig.IsGrounded;
		public bool InWater => Rig.InWater;
		public bool IsInAir => Rig.IsInAir;
		public int CurrentJumpCount { get; private set; } = 0;
		public bool FacingRight { get; private set; } = true;
		public bool FacingFront { get; private set; } = true;
		public bool IsMoving => IntendedX != 0;
		public bool IsRunning => IsMoving && MovingAccumulateFrame >= RunTrigger;
		public int FinalVelocityX => Rig.FinalVelocityX;
		public int FinalVelocityY => Rig.FinalVelocityY;

		// Short
		private int CurrentDashDuration => InWater && SwimInFreeStyle ? FreeSwimDashDuration : DashDuration;
		private int CurrentDashCooldown => InWater && SwimInFreeStyle ? FreeSwimDashCooldown : DashCooldown;

		// Ser
		[SerializeField] int Width = 150;
		[SerializeField] int Height = 150;

		[SerializeField] int MoveSpeed = 17;
		[SerializeField] int MoveAcceleration = 3;
		[SerializeField] int MoveDecceleration = 4;
		[SerializeField] int OppositeXAccelerationRate = 3000;
		[SerializeField] int RunTrigger = 60;
		[SerializeField] int RunSpeed = 32;
		[SerializeField] int GroundStuckLoseX = 2;
		[SerializeField] int GroundStuckLoseY = 6;

		[SerializeField] int JumpSpeed = 62;
		[SerializeField] int JumpCount = 2;
		[SerializeField] int JumpReleaseLoseRate = 700;
		[SerializeField] int JumpRaiseGravityRate = 600;
		[SerializeField] bool JumpThroughOneway = false;

		[SerializeField] bool DashAvailable = true;
		[SerializeField] int DashSpeed = 42;
		[SerializeField] int DashDuration = 12;
		[SerializeField] int DashCooldown = 4;
		[SerializeField] int DashAcceleration = 24;
		[SerializeField] int DashCancelLoseRate = 300;

		[SerializeField] bool SquatAvailable = true;
		[SerializeField] int SquatSpeed = 8;
		[SerializeField] int SquatAcceleration = 48;
		[SerializeField] int SquatDecceleration = 48;
		[SerializeField] int SquatHeight = 80;

		[SerializeField] bool PoundAvailable = true;
		[SerializeField] int PoundSpeed = 96;

		[SerializeField] bool SwimInFreeStyle = false;
		[SerializeField] int FreeSwimSpeed = 20;
		[SerializeField] int FreeSwimAcceleration = 4;
		[SerializeField] int FreeSwimDecceleration = 4;
		[SerializeField] int FreeSwimDashSpeed = 64;
		[SerializeField] int FreeSwimDashDuration = 4;
		[SerializeField] int FreeSwimDashCooldown = 4;
		[SerializeField] int FreeSwimDashAcceleration = 128;
		[SerializeField] int InWaterSpeedLoseRate = 500;

		[SerializeField] bool ClimbAvailable = true;
		[SerializeField] bool JumpWhenClimbAvailable = true;
		[SerializeField] int ClimbSpeedX = 12;
		[SerializeField] int ClimbSpeedY = 18;

		// Data
		private eRigidbody Rig = null;
		private int CurrentFrame = 0;
		private int IntendedX = 0;
		private int IntendedY = 0;
		private int LastGroundedFrame = int.MinValue;
		private int LastEndMoveFrame = int.MinValue;
		private int MovingAccumulateFrame = 0;
		private int LastJumpFrame = int.MinValue;
		private int LastDashFrame = int.MinValue;
		private bool HoldingJump = false;
		private bool PrevHoldingJump = false;
		private bool IntendedJump = false;
		private bool IntendedDash = false;
		private bool IntendedPound = false;
		private bool PrevInWater = false;
		private int? ClimbPositionCorrect = null;
		private RectInt Hitbox = default;
		private Vector2Int LastMoveDirection = default;


		#endregion




		#region --- MSG ---


		public void Init (eRigidbody ch) {
			Rig = ch;
			Rig.Width = Width;
			Rig.Height = Height;
		}


		public void PhysicsUpdate (int frame) {
			CurrentFrame = frame;
			Update_Cache();
			Update_Jump();
			Update_Dash();
			Update_VelocityX();
			Update_VelocityY();
			IntendedJump = false;
			IntendedDash = false;
			IntendedPound = false;
			PrevHoldingJump = HoldingJump;
		}


		private void Update_Cache () {

			// Ground
			if (IsGrounded) LastGroundedFrame = CurrentFrame;

			// Climb
			ClimbPositionCorrect = null;
			if (ClimbAvailable) {
				if (HoldingJump && CurrentJumpCount > 0 && Rig.VelocityY > 0) {
					IsClimbing = false;
				} else {
					int climbID = ClimbCheck();
					bool overlapClimb = climbID != 0;
					if (!IsClimbing) {
						if (overlapClimb && IntendedY > 0) IsClimbing = true;
					} else {
						if (IsGrounded || !overlapClimb) IsClimbing = false;
					}
				}
			} else {
				IsClimbing = false;
			}

			// Dash
			IsDashing = DashAvailable && DashSpeed > 0 && !IsClimbing && CurrentFrame < LastDashFrame + CurrentDashDuration && !IsInsideGround;
			if (IsDashing && IntendedY != -1) {
				// Stop when Dashing Without Holding Down
				LastDashFrame = int.MinValue;
				IsDashing = false;
				Rig.VelocityX = Rig.VelocityX * DashCancelLoseRate / 1000;
			}

			// Water
			// In/Out Water
			if (PrevInWater != InWater) {
				LastDashFrame = int.MinValue;
				IsDashing = false;
				if (InWater) {
					// In Water
					Rig.VelocityY = Rig.VelocityY * InWaterSpeedLoseRate / 1000;
				} else {
					// Out Water
					if (Rig.VelocityY > 0) Rig.VelocityY = JumpSpeed;
				}
			}
			PrevInWater = InWater;

			// Squat
			IsSquating =
				SquatAvailable && IsGrounded && !IsClimbing && !IsInsideGround &&
				((!IsDashing && IntendedY < 0) || ForceSquatCheck());

			// Pound
			IsPounding = PoundAvailable && !IsGrounded && !IsClimbing && !InWater && !IsDashing && !IsInsideGround &&
				(IsPounding ? IntendedY < 0 : IntendedPound);

			// Facing
			FacingRight = LastMoveDirection.x > 0;
			FacingFront = !IsClimbing;

			// Physics
			Hitbox = new(
				Rig.X - Width / 2,
				Rig.Y,
				Width,
				IsSquating || (IsDashing && (!InWater || !SwimInFreeStyle)) ? SquatHeight : Height
			);
			Rig.Width = Hitbox.width;
			Rig.Height = Hitbox.height;
			Rig.OffsetX = -Width / 2;
			Rig.OffsetY = 0;
		}


		private void Update_Jump () {
			// Reset Count on Grounded
			if (CurrentFrame > LastJumpFrame + JUMP_GAP && (IsGrounded || InWater || IsClimbing) && !IntendedJump) {
				CurrentJumpCount = 0;
			}
			// Perform Jump
			if (IntendedJump && CurrentJumpCount < JumpCount && !IsSquating && (!IsClimbing || JumpWhenClimbAvailable)) {
				if (InWater && SwimInFreeStyle) {
					// Free Dash In Water
					LastDashFrame = CurrentFrame;
					IsDashing = true;
					Rig.VelocityX = 0;
					Rig.VelocityY = 0;
				} else {
					// Jump
					CurrentJumpCount++;
					Rig.VelocityY = Mathf.Max(JumpSpeed, Rig.VelocityY);
					LastDashFrame = int.MinValue;
					IsDashing = false;
					LastJumpFrame = CurrentFrame;
				}
				IsClimbing = false;
			}
			// Fall off Edge
			if (CurrentJumpCount == 0 && !IsGrounded && !InWater && !IsClimbing && CurrentFrame > LastGroundedFrame + JUMP_TOLERANCE) {
				CurrentJumpCount++;
			}
			// Jump Release
			if (PrevHoldingJump && !HoldingJump) {
				// Lose Speed if Raising
				if (!IsGrounded && CurrentJumpCount <= JumpCount && Rig.VelocityY > 0) {
					Rig.VelocityY = Rig.VelocityY * JumpReleaseLoseRate / 1000;
				}
			}
		}


		private void Update_Dash () {
			if (
				DashAvailable && IntendedDash && IsGrounded && (!InWater || !SwimInFreeStyle) &&
				CurrentFrame > LastDashFrame + CurrentDashDuration + CurrentDashCooldown
			) {
				// Perform Dash
				LastDashFrame = CurrentFrame;
				IsDashing = true;
				Rig.VelocityY = 0;
			}
		}


		private void Update_VelocityX () {
			int speed, acc, dcc;
			if (IsClimbing) {
				// Climb
				speed = ClimbPositionCorrect.HasValue ? 0 : IntendedX * ClimbSpeedX;
				acc = int.MaxValue;
				dcc = int.MaxValue;
				if (ClimbPositionCorrect.HasValue) Rig.X = Rig.X.MoveTowards(ClimbPositionCorrect.Value, CLIMB_CORRECT_DELTA);
			} else if (IsDashing) {
				if (InWater && SwimInFreeStyle) {
					// Free Water Dash
					speed = LastMoveDirection.x * DashSpeed;
					acc = FreeSwimDashAcceleration;
					dcc = int.MaxValue;
				} else {
					// Normal Dash
					speed = FacingRight ? DashSpeed : -DashSpeed;
					acc = DashAcceleration;
					dcc = int.MaxValue;
				}
			} else if (IsSquating) {
				speed = IntendedX * SquatSpeed;
				acc = SquatAcceleration;
				dcc = SquatDecceleration;
			} else if (InWater && SwimInFreeStyle) {
				speed = IntendedX * FreeSwimSpeed;
				acc = FreeSwimAcceleration;
				dcc = FreeSwimDecceleration;
			} else {
				speed = IntendedX * (MovingAccumulateFrame >= RunTrigger ? RunSpeed : MoveSpeed);
				acc = MoveAcceleration;
				dcc = MoveDecceleration;
			}
			if ((speed > 0 && Rig.VelocityX < 0) || (speed < 0 && Rig.VelocityX > 0)) {
				acc *= OppositeXAccelerationRate / 1000;
				dcc *= OppositeXAccelerationRate / 1000;
			}
			Rig.VelocityX = Rig.VelocityX.MoveTowards(speed, acc, dcc);
			if (IsInsideGround) {
				Rig.VelocityX = Rig.VelocityX.MoveTowards(0, GroundStuckLoseX);
			}
		}


		private void Update_VelocityY () {
			if (IsClimbing) {
				// Climb
				Rig.VelocityY = (IntendedY <= 0 || ClimbCheck(true) != 0 ? IntendedY : 0) * ClimbSpeedY;
				Rig.GravityScale = 0;
			} else if (InWater && SwimInFreeStyle) {
				if (IsDashing) {
					// Free Dash
					Rig.VelocityY = Rig.VelocityY.MoveTowards(
						LastMoveDirection.y * FreeSwimDashSpeed, FreeSwimDashAcceleration, int.MaxValue
					);
				} else {
					// Free Swim In Water
					Rig.VelocityY = Rig.VelocityY.MoveTowards(
						IntendedY * FreeSwimSpeed, FreeSwimAcceleration, FreeSwimDecceleration
					);
				}
				Rig.GravityScale = 0;
			} else {
				// Gravity
				if (IsPounding) {
					// Pound
					Rig.GravityScale = 0;
					Rig.VelocityY = -PoundSpeed;
				} else if (HoldingJump && Rig.VelocityY > 0) {
					// Jumping Raise
					Rig.GravityScale = JumpRaiseGravityRate;
				} else if (!IsGrounded) {
					// In Air/Water
					Rig.GravityScale = 1000;
				} else {
					// Grounded
					Rig.GravityScale = 1000;
				}
			}
			if (IsInsideGround) {
				Rig.VelocityY = Rig.VelocityY.MoveTowards(0, GroundStuckLoseY);
			}
		}


		#endregion




		#region --- API ---


		public void Move (Direction3 x, Direction3 y) {
			if (IntendedX != 0 && x == Direction3.None) LastEndMoveFrame = CurrentFrame;
			if (x != Direction3.None) MovingAccumulateFrame++;
			if (x == Direction3.None && CurrentFrame > LastEndMoveFrame + RUN_BREAK_GAP) MovingAccumulateFrame = 0;
			IntendedX = (int)x;
			IntendedY = (int)y;
			if (x != Direction3.None) LastMoveDirection.x = IntendedX;
			if (y != Direction3.None) LastMoveDirection.y = IntendedY;
		}


		public void HoldJump (bool holding) => HoldingJump = holding;


		public void Jump () => IntendedJump = IntendedY >= 0 || IsClimbing;


		public void Dash () {
			if (!DashAvailable) return;
			IntendedDash = DashSpeed > 0;
			// Jump Through Oneway
			if (JumpThroughOneway) {
				Rig.SetPosition(Rig.X, Rig.Y - 2);
			}
		}


		public void Pound () => IntendedPound = true;


		#endregion




		#region --- LGC ---


		private bool ForceSquatCheck () {
			if (IsInsideGround) return false;
			var rect = new RectInt(
				Rig.X + Rig.OffsetX,
				Rig.Y + Rig.OffsetY + Height / 2,
				Rig.Width,
				Height / 2
			);
			bool overlap = CellPhysics.Overlap((int)PhysicsMask.Level, rect, null);
			if (overlap) return true;
			overlap = CellPhysics.Overlap((int)PhysicsMask.Environment, rect, null);
			if (overlap && IsSquating && IntendedY >= 0) {
				// Want to Stand Up but Overlaps
				return !CellPhysics.RoomCheck(
					(int)PhysicsMask.Map, rect, Rig, Direction4.Up
				);
			}
			return overlap;
		}


		private int ClimbCheck (bool up = false) {
			// 0: not overlap
			// 1: overlap without correct pos
			// 2: overlap and correct pos
			if (IsInsideGround) return 0;
			if (CellPhysics.Overlap(
				(int)PhysicsMask.Environment,
				up ? Rig.Rect.Shift(0, ClimbSpeedY) : Rig.Rect,
				Rig,
				out var info,
				OperationMode.TriggerOnly,
				YayaConst.CLIMB_TAG
			)) {
				if (info.Entity is eClimbable climb && (climb.CorrectPosition || ClimbSpeedX == 0)) {
					ClimbPositionCorrect = climb.Rect.CenterInt().x;
					return 2;
				}
				return 1;
			}
			return 0;
		}


		#endregion




	}
}