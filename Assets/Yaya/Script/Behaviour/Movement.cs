using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using AngeliaFramework;
using System.IO;

namespace Yaya {
	[System.Serializable]
	public partial class Movement {




		#region --- VAR ---


		// Const
		private const int JUMP_TOLERANCE = 4;
		private const int JUMP_GAP = 1;
		private const int CLIMB_CORRECT_DELTA = 36;
		private const int RUN_BREAK_GAP = 6;

		// Api
		public eYayaRigidbody Source { get; private set; } = null;
		public Vector2Int LastMoveDirection { get; private set; } = default;
		public int IntendedX { get; private set; } = 0;
		public int IntendedY { get; private set; } = 0;
		public int CurrentJumpCount { get; private set; } = 0;
		public int RunningAccumulateFrame { get; private set; } = 0;
		public int LastGroundFrame { get; private set; } = int.MinValue;
		public int LastGroundingFrame { get; private set; } = int.MinValue;
		public int LastEndMoveFrame { get; private set; } = int.MinValue;
		public int LastJumpFrame { get; private set; } = int.MinValue;
		public int LastDashFrame { get; private set; } = int.MinValue;
		public int LastSquatFrame { get; private set; } = int.MinValue;
		public int LastSquatingFrame { get; private set; } = int.MinValue;
		public int LastPoundingFrame { get; private set; } = int.MinValue;
		public int LastFlyFrame { get; private set; } = int.MinValue;
		public bool IsDashing { get; private set; } = false;
		public bool IsSquating { get; private set; } = false;
		public bool IsPounding { get; private set; } = false;
		public bool IsClimbing { get; private set; } = false;
		public bool IsFlying { get; private set; } = false;
		public bool FacingRight { get; private set; } = true;
		public int FinalVelocityX => Source.FinalVelocityX;
		public int FinalVelocityY => Source.FinalVelocityY;
		public bool FacingFront => !IsClimbing;
		public bool IsInsideGround => Source.InsideGround;
		public bool IsGrounded => Source.IsGrounded;
		public bool InWater => Source.InWater;
		public bool InAir => Source.InAir;
		public bool IsMoving => IntendedX != 0;
		public bool IsRunning => IsMoving && RunningAccumulateFrame >= RunTrigger;
		public bool IsRolling => !InWater && !IsPounding && !IsFlying && ((JumpRoll && CurrentJumpCount > 0) || (JumpSecondRoll && CurrentJumpCount > 1));
		public bool UseFreeStyleSwim => SwimInFreeStyle;

		// Short
		private int CurrentDashDuration => InWater && SwimInFreeStyle ? FreeSwimDashDuration : DashDuration;
		private int CurrentDashCooldown => InWater && SwimInFreeStyle ? FreeSwimDashCooldown : DashCooldown;

		// Data
		private readonly HitInfo[] c_HitboxCollisionFix = new HitInfo[8];
		private RectInt Hitbox = default;
		private int CurrentFrame = 0;
		private int LastIntendedX = 1;
		private bool HoldingJump = false;
		private bool PrevHoldingJump = false;
		private bool IntendedJump = false;
		private bool IntendedDash = false;
		private bool IntendedPound = false;
		private bool PrevInWater = false;
		private bool PrevGrounded = false;
		private int? ClimbPositionCorrect = null;


		#endregion




		#region --- MSG ---


		public void OnActived (eYayaRigidbody source) {
			Source = source;
			Source.Width = Width;
			Source.Height = Height;
		}


		public void Update () {
			CurrentFrame = Game.GlobalFrame;
			Update_Cache();
			Update_Jump();
			Update_Dash();
			if (Source.InSand) StopDash();
			Update_VelocityX();
			Update_VelocityY();
			IntendedJump = false;
			IntendedDash = false;
			IntendedPound = false;
			PrevHoldingJump = HoldingJump;
		}


		private void Update_Cache () {

			// Ground
			if (IsGrounded) LastGroundingFrame = CurrentFrame;
			if (!PrevGrounded && IsGrounded) LastGroundFrame = CurrentFrame;
			PrevGrounded = IsGrounded;

			// Climb
			ClimbPositionCorrect = null;
			if (ClimbAvailable) {
				if (HoldingJump && CurrentJumpCount > 0 && Source.VelocityY > 0) {
					IsClimbing = false;
				} else {
					bool overlapClimb = ClimbCheck();
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
			if (InWater && SwimInFreeStyle) {
				IsDashing = DashAvailable && FreeSwimDashSpeed > 0 && !IsClimbing && CurrentFrame < LastDashFrame + FreeSwimDashDuration;
			} else {
				IsDashing = DashAvailable && DashSpeed > 0 && !IsClimbing && CurrentFrame < LastDashFrame + CurrentDashDuration && !IsInsideGround;
				if (IsDashing && IntendedY != -1) {
					// Stop when Dashing Without Holding Down
					LastDashFrame = int.MinValue;
					IsDashing = false;
					Source.VelocityX = Source.VelocityX * DashCancelLoseRate / 1000;
				}
			}

			// In/Out Water
			if (PrevInWater != InWater) {
				LastDashFrame = int.MinValue;
				IsDashing = false;
				if (InWater) {
					// In Water
					Source.VelocityY = Source.VelocityY * InWaterSpeedLoseRate / 1000;
				} else {
					// Out Water
					if (Source.VelocityY > 0) Source.VelocityY = JumpSpeed;
				}
			}
			PrevInWater = InWater;

			// Squat
			bool squating =
				SquatAvailable && IsGrounded && !IsClimbing && !Source.InSand && !IsInsideGround &&
				((!IsDashing && IntendedY < 0) || ForceSquatCheck());
			if (!IsSquating && squating) LastSquatFrame = CurrentFrame;
			if (squating) LastSquatingFrame = CurrentFrame;
			IsSquating = squating;

			// Pound
			IsPounding = PoundAvailable && !IsGrounded && !IsClimbing && !InWater && !IsDashing && !IsInsideGround &&
				(IsPounding ? IntendedY < 0 : IntendedPound);
			if (IsPounding) LastPoundingFrame = CurrentFrame;

			// Fly
			if (
				!HoldingJump || IsGrounded || InWater || IsClimbing ||
				IsDashing || IsInsideGround || IsPounding
			) {
				IsFlying = false;
			}

			// Facing
			FacingRight = LastIntendedX > 0;

			// Physics
			int prevHitboxHeight = Hitbox.height;
			Hitbox = new(Source.X - Width / 2, Source.Y, Width, GetCurrentHeight());
			Source.Width = Hitbox.width;
			Source.Height = Hitbox.height;
			Source.OffsetX = -Width / 2;
			Source.OffsetY = 0;
			if (Hitbox.height > prevHitboxHeight) CollisionFixOnHitboxChanged(prevHitboxHeight);
		}


		private void Update_Jump () {

			// Reset Count on Grounded
			if (CurrentFrame > LastJumpFrame + JUMP_GAP && (IsGrounded || InWater || IsClimbing) && !IntendedJump) {
				CurrentJumpCount = 0;
			}

			// Perform Jump
			if (IntendedJump && !IsSquating && (!IsClimbing || JumpWhenClimbAvailable)) {
				if (CurrentJumpCount < JumpCount) {
					// Jump
					if (InWater && SwimInFreeStyle) {
						// Free Dash in Water
						LastDashFrame = CurrentFrame;
						IsDashing = true;
						Source.VelocityX = 0;
						Source.VelocityY = 0;
					} else {
						// Perform Jump
						CurrentJumpCount++;
						Source.VelocityY = Mathf.Max(JumpSpeed, Source.VelocityY);
						LastDashFrame = int.MinValue;
						IsDashing = false;
						LastJumpFrame = CurrentFrame;
					}
					IsClimbing = false;
				} else if (FlyAvailable && CurrentJumpCount < JumpCount + FlyCount && CurrentFrame > LastFlyFrame + FlyCooldown) {
					// Fly
					LastDashFrame = int.MinValue;
					IsFlying = true;
					IsClimbing = false;
					IsDashing = false;
					CurrentJumpCount++;
					Source.VelocityY = Mathf.Max(FlySpeed, Source.VelocityY);
					LastFlyFrame = CurrentFrame;
				}
			}

			// Fall off Edge
			if (CurrentJumpCount == 0 && !IsGrounded && !InWater && !IsClimbing && CurrentFrame > LastGroundingFrame + JUMP_TOLERANCE) {
				CurrentJumpCount++;
			}

			// Jump Release
			if (PrevHoldingJump && !HoldingJump) {
				// Lose Speed if Raising
				if (!IsGrounded && CurrentJumpCount <= JumpCount && Source.VelocityY > 0) {
					Source.VelocityY = Source.VelocityY * JumpReleaseLoseRate / 1000;
				}
			}
		}


		private void Update_Dash () {
			if (
				DashAvailable && IntendedDash && IsGrounded &&
				CurrentFrame > LastDashFrame + CurrentDashDuration + CurrentDashCooldown
			) {
				// Perform Dash
				LastDashFrame = CurrentFrame;
				IsDashing = true;
				Source.VelocityY = 0;
			}
		}


		private void Update_VelocityX () {
			int speed, acc, dcc;
			if (IsClimbing) {
				// Climb
				speed = ClimbPositionCorrect.HasValue ? 0 : IntendedX * ClimbSpeedX;
				acc = int.MaxValue;
				dcc = int.MaxValue;
				if (ClimbPositionCorrect.HasValue) Source.X = Source.X.MoveTowards(ClimbPositionCorrect.Value, CLIMB_CORRECT_DELTA);
			} else if (IsDashing) {
				if (InWater && SwimInFreeStyle && !IsGrounded) {
					// Free Water Dash
					speed = LastMoveDirection.x * FreeSwimDashSpeed;
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
			} else if (InWater) {
				speed = IntendedX * SwimSpeed;
				acc = SwimAcceleration;
				dcc = SwimDecceleration;
			} else {
				bool running = RunningAccumulateFrame >= RunTrigger;
				speed = IntendedX * (running ? RunSpeed : WalkSpeed);
				acc = running ? RunAcceleration : WalkAcceleration;
				dcc = running ? RunDecceleration : WalkDecceleration;
			}
			if ((speed > 0 && Source.VelocityX < 0) || (speed < 0 && Source.VelocityX > 0)) {
				acc *= OppositeXAccelerationRate / 1000;
				dcc *= OppositeXAccelerationRate / 1000;
			}
			Source.VelocityX = Source.VelocityX.MoveTowards(speed, acc, dcc);
		}


		private void Update_VelocityY () {
			if (IsFlying) {
				// Fly
				Source.GravityScale = Source.VelocityY > 0 ? FlyGravityRiseRate : FlyGravityFallRate;
				Source.VelocityY = Mathf.Max(Source.VelocityY, -FlyFallSpeed);
			} else if (IsClimbing) {
				// Climb
				Source.VelocityY = (IntendedY <= 0 || ClimbCheck(true) ? IntendedY : 0) * ClimbSpeedY;
				Source.GravityScale = 0;
			} else if (InWater && SwimInFreeStyle) {
				if (IsDashing) {
					// Free Water Dash
					Source.VelocityY = Source.VelocityY.MoveTowards(
						LastMoveDirection.y * FreeSwimDashSpeed, FreeSwimDashAcceleration, int.MaxValue
					);
				} else {
					// Free Swim In Water
					Source.VelocityY = Source.VelocityY.MoveTowards(
						IntendedY * FreeSwimSpeed, FreeSwimAcceleration, FreeSwimDecceleration
					);
				}
				Source.GravityScale = 0;
			} else {
				// Gravity
				if (IsPounding) {
					// Pound
					Source.GravityScale = 0;
					Source.VelocityY = -PoundSpeed;
				} else if (HoldingJump && Source.VelocityY > 0) {
					// Jumping Raise
					Source.GravityScale = JumpRiseGravityRate;
				} else {
					// Else
					Source.GravityScale = 1000;
					if (InWater && IntendedY != 0) {
						// Normal Swim
						Source.GravityScale = 0;
						Source.VelocityY = Source.VelocityY.MoveTowards(
							IntendedY * SwimSpeed, SwimAcceleration, SwimDecceleration
						);
					}
				}
			}
		}


		#endregion




		#region --- API ---


		// Movement
		public void Move (Direction3 x, Direction3 y) {
			if (IntendedX != 0 && x == Direction3.None) LastEndMoveFrame = CurrentFrame;
			if (x != Direction3.None) RunningAccumulateFrame++;
			if (x == Direction3.None && CurrentFrame > LastEndMoveFrame + RUN_BREAK_GAP) RunningAccumulateFrame = 0;
			IntendedX = (int)x;
			IntendedY = (int)y;
			if (x != Direction3.None) LastIntendedX = IntendedX;
			if (x != Direction3.None || y != Direction3.None) {
				LastMoveDirection = new(IntendedX, IntendedY);
			}
		}


		public void HoldJump (bool holding) => HoldingJump = holding;


		public void Jump () => IntendedJump = InWater || IntendedY >= 0 || IsClimbing;


		public void Dash () {
			if (!DashAvailable) return;
			IntendedDash = DashSpeed > 0;
		}


		public void StopDash () {
			LastDashFrame = int.MinValue;
			IsDashing = false;
		}


		public void Pound () => IntendedPound = true;


		public void AntiKnockback () => Source.VelocityX = Source.VelocityX.MoveTowards(0, AntiKnockbackSpeed);


		#endregion




		#region --- LGC ---


		private bool ForceSquatCheck () {

			if (IsInsideGround) return false;

			var rect = new RectInt(
				Source.X + Source.OffsetX,
				Source.Y + Source.OffsetY + Height / 2,
				Source.Width,
				Height / 2
			);

			// Oneway Check
			if ((IsSquating || IsDashing) && !CellPhysics.RoomCheck_Oneway(
				YayaConst.MASK_MAP, rect, Source, Direction4.Up, false
			)) return true;

			// Overlap Check
			return CellPhysics.Overlap(YayaConst.MASK_MAP, rect, null);
		}


		private bool ClimbCheck (bool up = false) {
			if (IsInsideGround) return false;
			if (CellPhysics.Overlap(
				YayaConst.MASK_ENVIRONMENT,
				up ? Source.Rect.Shift(0, ClimbSpeedY) : Source.Rect,
				Source,
				OperationMode.TriggerOnly,
				YayaConst.CLIMB_TAG
			)) {
				return true;
			}
			if (CellPhysics.Overlap(
				YayaConst.MASK_ENVIRONMENT,
				up ? Source.Rect.Shift(0, ClimbSpeedY) : Source.Rect,
				Source,
				out var info,
				OperationMode.TriggerOnly,
				YayaConst.CLIMB_STABLE_TAG
			)) {
				ClimbPositionCorrect = info.Rect.CenterInt().x;
				return true;
			}
			return false;
		}


		private int GetCurrentHeight () {

			// Squating
			if (IsSquating) return SquatHeight;

			// Dashing
			if (IsDashing && (!InWater || IsGrounded)) return SquatHeight;

			// Swimming
			if (InWater) return SwimHeight;

			// Rolling
			if (IsRolling) return SquatHeight;

			// Fly
			if (IsFlying) return FlyHeight;

			// Normal
			return Height;
		}


		private void CollisionFixOnHitboxChanged (int prevHitboxHeight) {
			var rect = Hitbox.Shrink(0, 0, Const.CELL_SIZE / 4, 0);
			// Fix for Oneway
			int count = !IsClimbing ? CellPhysics.OverlapAll(
				c_HitboxCollisionFix, YayaConst.MASK_MAP, rect, Source,
				OperationMode.TriggerOnly, Const.ONEWAY_DOWN_TAG
			) : CellPhysics.OverlapAll(
				c_HitboxCollisionFix, YayaConst.MASK_MAP, rect, Source
			);
			for (int i = 0; i < count; i++) {
				var hit = c_HitboxCollisionFix[i];
				if (hit.Rect.yMin > rect.y) {
					Source.PerformMove(
						0, -Hitbox.height + prevHitboxHeight,
						true, false
					);
					if (IsGrounded) IsSquating = true;
					break;
				}
			}
		}


		#endregion




	}
}