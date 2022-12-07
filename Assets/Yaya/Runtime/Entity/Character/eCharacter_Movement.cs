using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	public enum MovementState {
		Idle = 0,
		Walk, Run, JumpUp, JumpDown,
		SwimIdle, SwimMove, SwimDash,
		SquatIdle, SquatMove,
		Dash, Roll, Pound, Climb, Fly, Slide,
	}


	public abstract partial class eCharacter {




		#region --- VAR ---


		// Const
		private const int JUMP_TOLERANCE = 4;
		private const int JUMP_GAP = 1;
		private const int CLIMB_CORRECT_DELTA = 36;
		private const int RUN_BREAK_GAP = 6;
		private const int SLIDE_JUMP_CANCEL = 2;

		// Api
		public Vector2Int LastMoveDirection { get; private set; } = default;
		public int IntendedX { get; private set; } = 0;
		public int IntendedY { get; private set; } = 0;
		public int CurrentJumpCount { get; private set; } = 0;
		public bool UseFreeStyleSwim => SwimInFreeStyle;

		// Frame Cache
		public int RunningAccumulateFrame { get; private set; } = 0;
		public int LastGroundFrame { get; private set; } = int.MinValue;
		public int LastGroundingFrame { get; private set; } = int.MinValue;
		public int LastStartMoveFrame { get; private set; } = int.MinValue;
		public int LastEndMoveFrame { get; private set; } = int.MinValue;
		public int LastJumpFrame { get; private set; } = int.MinValue;
		public int LastDashFrame { get; private set; } = int.MinValue;
		public int LastSquatFrame { get; private set; } = int.MinValue;
		public int LastSquatingFrame { get; private set; } = int.MinValue;
		public int LastPoundingFrame { get; private set; } = int.MinValue;
		public int LastFlyFrame { get; private set; } = int.MinValue;

		// Movement State
		public MovementState MoveState { get; set; } = MovementState.Idle;
		public bool FacingRight { get; set; } = true;
		public bool FacingFront { get; set; } = true;
		public bool IsDashing { get; private set; } = false;
		public bool IsSquating { get; private set; } = false;
		public bool IsPounding { get; private set; } = false;
		public bool IsClimbing { get; private set; } = false;
		public bool IsFlying { get; private set; } = false;
		public bool IsSliding { get; private set; } = false;
		public bool IsMoving => IntendedX != 0;
		public bool IsRunning => IsMoving && RunningAccumulateFrame >= RunAccumulation;
		public bool IsRolling => !InWater && !IsPounding && !IsFlying && ((JumpWithRoll && CurrentJumpCount > 0) || (JumpSecondWithRoll && CurrentJumpCount > 1));

		// Short
		private int CurrentDashDuration => InWater && SwimInFreeStyle ? FreeSwimDashDuration : DashDuration;
		private int CurrentDashCooldown => InWater && SwimInFreeStyle ? FreeSwimDashCooldown : DashCooldown;

		// Data
		private static readonly HitInfo[] c_HitboxCollisionFix = new HitInfo[8];
		private static readonly HitInfo[] c_SlideCheck = new HitInfo[8];
		private RectInt Hitbox = default;
		private int CurrentFrame = 0;
		private int LastIntendedX = 1;
		private bool HoldingJump = false;
		private bool AllowHoldingJumpForFly = false;
		private bool PrevHoldingJump = false;
		private bool IntendedJump = false;
		private bool IntendedDash = false;
		private bool IntendedPound = false;
		private bool PrevInWater = false;
		private bool PrevGrounded = false;
		private int? ClimbPositionCorrect = null;


		#endregion




		#region --- MSG ---


		public void OnActived_Movement () {
			Width = MovementWidth;
			Height = MovementHeight;
			OffsetX = -MovementWidth / 2;
			OffsetY = 0;
		}


		public void Update_Movement () {
			CurrentFrame = Game.GlobalFrame;
			MovementUpdate_Cache();
			MovementUpdate_Jump();
			MovementUpdate_Dash();
			if (InSand) StopDash();
			MovementUpdate_VelocityX();
			MovementUpdate_VelocityY();
			IntendedJump = false;
			IntendedDash = false;
			IntendedPound = false;
			PrevHoldingJump = HoldingJump;
			MoveState =
				IsFlying ? MovementState.Fly :
				IsClimbing ? MovementState.Climb :
				IsPounding ? MovementState.Pound :
				IsSliding ? MovementState.Slide :
				IsRolling ? MovementState.Roll :
				IsDashing ? (!IsGrounded && InWater ? MovementState.SwimDash : MovementState.Dash) :
				IsSquating ? (IsMoving ? MovementState.SquatMove : MovementState.SquatIdle) :
				InWater && !IsGrounded ? (IsMoving ? MovementState.SwimMove : MovementState.SwimIdle) :
				InAir ? (VelocityY > 0 ? MovementState.JumpUp : MovementState.JumpDown) :
				IsRunning ? MovementState.Run :
				IsMoving ? MovementState.Walk :
				MovementState.Idle;
		}


		private void MovementUpdate_Cache () {

			// Ground
			if (IsGrounded) LastGroundingFrame = CurrentFrame;
			if (!PrevGrounded && IsGrounded) LastGroundFrame = CurrentFrame;
			PrevGrounded = IsGrounded;

			// Climb
			ClimbPositionCorrect = null;
			if (ClimbAvailable) {
				if (HoldingJump && CurrentJumpCount > 0 && VelocityY > 0) {
					IsClimbing = false;
				} else {
					bool overlapClimb = ClimbCheck();
					if (!IsClimbing) {
						if (overlapClimb && IntendedY > 0 && !IsSquating) IsClimbing = true;
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
				IsDashing = DashAvailable && DashSpeed > 0 && !IsClimbing && CurrentFrame < LastDashFrame + CurrentDashDuration && !InsideGround;
				if (IsDashing && IntendedY != -1) {
					// Stop when Dashing Without Holding Down
					LastDashFrame = int.MinValue;
					IsDashing = false;
					VelocityX = VelocityX * DashCancelLoseRate / 1000;
				}
			}

			// In/Out Water
			if (PrevInWater != InWater) {
				LastDashFrame = int.MinValue;
				IsDashing = false;
				if (InWater) {
					// In Water
					VelocityY = VelocityY * InWaterSpeedLoseRate / 1000;
				} else {
					// Out Water
					if (VelocityY > 0) VelocityY = JumpSpeed;
				}
			}
			PrevInWater = InWater;

			// Squat
			bool squating =
				SquatAvailable && IsGrounded && !IsClimbing && !InSand && !InsideGround &&
				((!IsDashing && IntendedY < 0) || ForceSquatCheck());
			if (!IsSquating && squating) LastSquatFrame = CurrentFrame;
			if (squating) LastSquatingFrame = CurrentFrame;
			IsSquating = squating;

			// Pound
			IsPounding =
				PoundAvailable && !IsGrounded && !IsClimbing && !InWater && !IsDashing && !InsideGround &&
				(IsPounding ? IntendedY < 0 : IntendedPound);
			if (IsPounding) LastPoundingFrame = CurrentFrame;

			// Fly
			if ((!HoldingJump && CurrentFrame > LastFlyFrame + FlyCooldown) || IsGrounded || InWater || IsClimbing || IsDashing || InsideGround || IsPounding) {
				IsFlying = false;
			}

			// Slide
			IsSliding = SlideCheck();

			// Facing
			FacingRight = LastIntendedX > 0;
			FacingFront = !IsClimbing;

			// Physics
			int prevHitboxHeight = Hitbox.height;
			int width = InWater ? SwimWidth : MovementWidth;
			Hitbox = new(X - width / 2, Y, width, GetCurrentHeight());
			Width = Hitbox.width;
			Height = Hitbox.height;
			OffsetX = -width / 2;
			OffsetY = 0;
			if (Hitbox.height > prevHitboxHeight) CollisionFixOnHitboxChanged(prevHitboxHeight);
		}


		private void MovementUpdate_Jump () {

			// Reset Count on Grounded
			if (CurrentFrame > LastJumpFrame + JUMP_GAP && (IsGrounded || InWater || IsClimbing) && !IntendedJump) {
				CurrentJumpCount = 0;
			}

			// Reset Count when Slide
			if (IsSliding && SlideJumpCountRecover > 0 && CurrentJumpCount > JumpCount - SlideJumpCountRecover) {
				CurrentJumpCount = (JumpCount - SlideJumpCountRecover).Clamp(0, JumpCount);
			}

			// Perform Jump/Fly
			if (!IsSquating && (!IsClimbing || JumpWhenClimbAvailable)) {
				// Jump
				if (CurrentJumpCount < JumpCount) {
					// Jump
					if (IntendedJump) {
						if (InWater && SwimInFreeStyle) {
							// Free Dash in Water
							LastDashFrame = CurrentFrame;
							IsDashing = true;
							VelocityX = 0;
							VelocityY = 0;
						} else {
							// Perform Jump
							CurrentJumpCount++;
							VelocityY = Mathf.Max(JumpSpeed, VelocityY);
							LastDashFrame = int.MinValue;
							IsDashing = false;
							IsSliding = false;
							LastJumpFrame = CurrentFrame;
						}
						IsClimbing = false;
					}
				} else if (FlyAvailable && CurrentJumpCount < JumpCount + FlyCount) {
					// Fly
					if (CurrentFrame > LastFlyFrame + FlyCooldown) {
						// Cooldown Ready
						if (IntendedJump || (HoldingJump && AllowHoldingJumpForFly)) {
							LastDashFrame = int.MinValue;
							IsFlying = true;
							IsClimbing = false;
							IsDashing = false;
							CurrentJumpCount++;
							VelocityY = Mathf.Max(FlySpeed, VelocityY);
							LastFlyFrame = CurrentFrame;
							AllowHoldingJumpForFly = false;
						}
					} else {
						// Not Cooldown
						if (IntendedJump) AllowHoldingJumpForFly = true;
						if (!HoldingJump) AllowHoldingJumpForFly = false;
					}
				}
			}

			// Fall off Edge
			if (CurrentJumpCount == 0 && !IsGrounded && !InWater && !IsClimbing && CurrentFrame > LastGroundingFrame + JUMP_TOLERANCE) {
				CurrentJumpCount++;
			}

			// Jump Release
			if (PrevHoldingJump && !HoldingJump) {
				// Lose Speed if Raising
				if (!IsGrounded && CurrentJumpCount <= JumpCount && VelocityY > 0) {
					VelocityY = VelocityY * JumpReleaseLoseRate / 1000;
				}
			}
		}


		private void MovementUpdate_Dash () {
			if (
				DashAvailable && IntendedDash && IsGrounded &&
				CurrentFrame > LastDashFrame + CurrentDashDuration + CurrentDashCooldown
			) {
				// Perform Dash
				LastDashFrame = CurrentFrame;
				IsDashing = true;
				VelocityY = 0;
			}
		}


		private void MovementUpdate_VelocityX () {
			int speed, acc, dcc;
			if (IsFlying && FlyGlideSpeed > 0) {
				// Glide
				speed = FacingRight ? FlyGlideSpeed : -FlyGlideSpeed;
				acc = FlyGlideAcceleration;
				dcc = FlyGlideDecceleration;
			} else if (IsClimbing) {
				// Climb
				speed = ClimbPositionCorrect.HasValue ? 0 : IntendedX * ClimbSpeedX;
				acc = int.MaxValue;
				dcc = int.MaxValue;
				if (ClimbPositionCorrect.HasValue) X = X.MoveTowards(ClimbPositionCorrect.Value, CLIMB_CORRECT_DELTA);
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
				bool running = IsRunning;
				speed = IntendedX * (running ? RunSpeed : WalkSpeed);
				acc = running ? RunAcceleration : WalkAcceleration;
				dcc = running ? RunDecceleration : WalkDecceleration;
			}
			if ((speed > 0 && VelocityX < 0) || (speed < 0 && VelocityX > 0)) {
				acc *= OppositeXAccelerationRate / 1000;
				dcc *= OppositeXAccelerationRate / 1000;
			}
			VelocityX = VelocityX.MoveTowards(speed, acc, dcc);
		}


		private void MovementUpdate_VelocityY () {
			if (IsFlying) {
				// Fly
				GravityScale = VelocityY > 0 ? FlyGravityRiseRate : FlyGravityFallRate;
				VelocityY = Mathf.Max(VelocityY, -FlyFallSpeed);
			} else if (IsClimbing) {
				// Climb
				VelocityY = (IntendedY <= 0 || ClimbCheck(true) ? IntendedY : 0) * ClimbSpeedY;
				GravityScale = 0;
			} else if (InWater && SwimInFreeStyle) {
				if (IsDashing) {
					// Free Water Dash
					VelocityY = VelocityY.MoveTowards(
						LastMoveDirection.y * FreeSwimDashSpeed, FreeSwimDashAcceleration, int.MaxValue
					);
				} else {
					// Free Swim In Water
					VelocityY = VelocityY.MoveTowards(
						IntendedY * FreeSwimSpeed, FreeSwimAcceleration, FreeSwimDecceleration
					);
				}
				GravityScale = 0;
			} else if (IsSliding) {
				// Slide
				VelocityY = -SlideDropSpeed;
				GravityScale = 0;
			} else {
				// Gravity
				if (IsPounding) {
					// Pound
					GravityScale = 0;
					VelocityY = -PoundSpeed;
				} else if (HoldingJump && VelocityY > 0) {
					// Jumping Raise
					GravityScale = JumpRiseGravityRate;
				} else {
					// Else
					GravityScale = 1000;
					if (InWater && IntendedY != 0) {
						// Normal Swim
						GravityScale = 0;
						VelocityY = VelocityY.MoveTowards(
							IntendedY * SwimSpeed, SwimAcceleration, SwimDecceleration
						);
					}
				}
			}
		}


		#endregion




		#region --- API ---


		public void Move (Direction3 x, Direction3 y) => MoveLogic((int)x, (int)y);


		public void Stop () {
			MoveLogic(0, 0);
			VelocityX = 0;
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


		public void AntiKnockback () => VelocityX = VelocityX.MoveTowards(0, AntiKnockbackSpeed);


		#endregion




		#region --- LGC ---


		private void MoveLogic (int x, int y) {
			if (IntendedX != 0 && x == 0) LastEndMoveFrame = CurrentFrame;
			if (IntendedX == 0 && x != 0) LastStartMoveFrame = CurrentFrame;
			if (x != 0) RunningAccumulateFrame++;
			if (x == 0 && CurrentFrame > LastEndMoveFrame + RUN_BREAK_GAP) RunningAccumulateFrame = 0;
			IntendedX = x;
			IntendedY = y;
			if (x != 0) LastIntendedX = IntendedX;
			if (x != 0 || y != 0) {
				LastMoveDirection = new(IntendedX, IntendedY);
			}
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
			return MovementHeight;
		}


		private void CollisionFixOnHitboxChanged (int prevHitboxHeight) {
			var rect = Hitbox.Shrink(0, 0, Const.CEL / 4, 0);
			// Fix for Oneway
			int count = CellPhysics.OverlapAll(
				c_HitboxCollisionFix, YayaConst.MASK_MAP, rect, this,
				OperationMode.TriggerOnly, Const.ONEWAY_DOWN_TAG
			);
			FixNow();
			count = CellPhysics.OverlapAll(c_HitboxCollisionFix, YayaConst.MASK_MAP, rect, this);
			FixNow();
			// Func
			void FixNow () {
				for (int i = 0; i < count; i++) {
					var hit = c_HitboxCollisionFix[i];
					if (hit.Rect.yMin > rect.y) {
						PerformMove(0, prevHitboxHeight - Hitbox.height, true);
						if (IsGrounded) IsSquating = true;
						break;
					}
				}
			}
		}


		// Check
		private bool ForceSquatCheck () {

			if (InsideGround) return false;

			var rect = new RectInt(
				X + OffsetX,
				Y + OffsetY + MovementHeight / 2,
				Width,
				MovementHeight / 2
			);

			// Oneway Check
			if ((IsSquating || IsDashing) && !CellPhysics.RoomCheckOneway(
				YayaConst.MASK_MAP, rect, this, Direction4.Up, false
			)) return true;

			// Overlap Check
			return CellPhysics.Overlap(YayaConst.MASK_MAP, rect, null);
		}


		private bool ClimbCheck (bool up = false) {
			if (InsideGround) return false;
			if (CellPhysics.Overlap(
				YayaConst.MASK_ENVIRONMENT,
				up ? Rect.Shift(0, ClimbSpeedY) : Rect,
				this,
				OperationMode.TriggerOnly,
				YayaConst.CLIMB_TAG
			)) {
				return true;
			}
			if (CellPhysics.Overlap(
				YayaConst.MASK_ENVIRONMENT,
				up ? Rect.Shift(0, ClimbSpeedY) : Rect,
				out var info,
				this,
				OperationMode.TriggerOnly,
				YayaConst.CLIMB_STABLE_TAG
			)) {
				ClimbPositionCorrect = info.Rect.CenterInt().x;
				return true;
			}
			return false;
		}


		private bool SlideCheck () {
			if (
				!SlideAvailable || IsGrounded || IsClimbing || IsDashing ||
				InWater || IsSquating || IsPounding || IsFlying ||
				Game.GlobalFrame < LastJumpFrame + SLIDE_JUMP_CANCEL ||
				IntendedX == 0 || VelocityY > -SlideDropSpeed
			) return false;
			var rect = new RectInt(
				IntendedX > 0 ? Hitbox.xMax : Hitbox.xMin - 1, Hitbox.y + Hitbox.height / 2,
				1, Hitbox.height / 2
			);
			if (SlideOnAllBlocks) {
				int count = CellPhysics.OverlapAll(c_SlideCheck, YayaConst.MASK_MAP, rect, this, OperationMode.ColliderOnly);
				for (int i = 0; i < count; i++) {
					var hit = c_SlideCheck[i];
					if (hit.Tag == YayaConst.NO_SLIDE_TAG) continue;
					return true;
				}
				return false;
			} else {
				return CellPhysics.Overlap(
					YayaConst.MASK_MAP, rect, this, OperationMode.ColliderOnly, YayaConst.SLIDE_TAG
				);
			}
		}


		#endregion




	}
}