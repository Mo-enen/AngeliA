using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	public enum CharacterMovementState {
		Idle = 0,
		Walk, Run, JumpUp, JumpDown,
		SwimIdle, SwimMove, SquatIdle, SquatMove,
		Dash, Rush, Crash, Pound, Climb, Fly, Slide, GrabTop, GrabSide, GrabFlip,
	}


	public abstract partial class Character {




		#region --- VAR ---


		// Const
		private const int JUMP_TOLERANCE = 4;
		private const int JUMP_REQUIRE_TOLERANCE = 6;
		private const int CLIMB_CORRECT_DELTA = 36;
		private const int RUN_BREAK_FRAME = 6;
		private const int SLIDE_JUMP_CANCEL = 2;
		private const int SLIDE_GROUND_TOLERANCE = 12;
		private const int GRAB_GROUND_TOLERANCE = 12;
		private const int CLIMB_GROUND_TOLERANCE = 12;
		private const int GRAB_JUMP_CANCEL = 2;
		private const int GRAB_DROP_CANCEL = 12;
		private const int GRAB_TOP_CHECK_GAP = 128;
		private const int CLIP_CORRECT_TOLERANCE = Const.CEL / 4;

		// Api
		public Vector2Int LastMoveDirection { get; private set; } = default;
		public int IntendedX { get; private set; } = 0;
		public int IntendedY { get; private set; } = 0;
		public int CurrentJumpCount { get; private set; } = 0;
		public bool FacingRight {
			get => LockFacingOnAttack && IsAttacking ? AttackStartFacingRight : _FacingRight;
			set => _FacingRight = value;
		}
		public bool FacingFront { get; private set; } = true;
		public virtual int GrowingHeight => MovementHeight;
		public virtual bool SpinOnGroundPound => false;
		public virtual bool FlyGlideAvailable => false;
		public virtual bool FlyAvailable => false;

		// Frame Cache
		public int RunningAccumulateFrame { get; set; } = 0;
		public int LastGroundFrame { get; private set; } = int.MinValue;
		public int LastGroundingFrame { get; private set; } = int.MinValue;
		public int LastStartMoveFrame { get; private set; } = int.MinValue;
		public int LastEndMoveFrame { get; private set; } = int.MinValue;
		public int LastJumpFrame { get; private set; } = int.MinValue;
		public int LastClimbFrame { get; private set; } = int.MinValue;
		public int LastDashFrame { get; private set; } = int.MinValue;
		public int LastRushFrame { get; private set; } = int.MinValue;
		public int LastCrashFrame { get; private set; } = int.MinValue;
		public int LastSlippyMoveStartFrame { get; private set; } = int.MinValue;
		public int LastSquatFrame { get; private set; } = int.MinValue;
		public int LastSquattingFrame { get; private set; } = int.MinValue;
		public int LastPoundingFrame { get; private set; } = int.MinValue;
		public int LastSlidingFrame { get; private set; } = int.MinValue;
		public int LastGrabbingFrame { get; private set; } = int.MinValue;
		public int LastFlyFrame { get; private set; } = int.MinValue;
		public int LastGrabFlipUpFrame { get; private set; } = int.MinValue;
		public int LastGrabFlipDownFrame { get; private set; } = int.MinValue;
		public int LastGrabCancelFrame { get; private set; } = int.MinValue;
		public int LastStartRunFrame { get; private set; } = int.MinValue;
		public int LastFacingChangeFrame { get; private set; } = 0;

		// Movement State
		public CharacterMovementState MovementState { get; private set; } = CharacterMovementState.Idle;
		public bool ReadyForRun => RunningAccumulateFrame >= WalkToRunAccumulation;
		public bool IsRolling => !InWater && !IsPounding && !IsFlying && !IsRushing && ((FirstJumpWithRoll && CurrentJumpCount > 0) || (SubsequentJumpWithRoll && CurrentJumpCount > 1) || (DashWithRoll && IsDashing));
		public bool IsGrabFlipping => IsGrabFlippingUp || IsGrabFlippingDown;
		public bool IsGrabFlippingUp => Game.GlobalFrame < LastGrabFlipUpFrame + Mathf.Max(GrabFlipThroughDuration, 1);
		public bool IsGrabFlippingDown => Game.GlobalFrame < LastGrabFlipDownFrame + Mathf.Max(GrabFlipThroughDuration, 1);
		public bool IsMoving => IntendedX != 0;
		public bool IsWalking => IntendedX != 0 && !ReadyForRun;
		public bool IsRunning => IntendedX != 0 && ReadyForRun;
		public bool IsDashing { get; private set; } = false;
		public bool IsRushing { get; private set; } = false;
		public bool IsCrashing { get; private set; } = false;
		public bool IsSquatting { get; private set; } = false;
		public bool IsPounding { get; private set; } = false;
		public bool IsClimbing { get; private set; } = false;
		public bool IsFlying { get; private set; } = false;
		public bool IsSliding { get; private set; } = false;
		public bool IsGrabbingTop { get; private set; } = false;
		public bool IsGrabbingSide { get; private set; } = false;

		// Data
		private RectInt Hitbox = default;
		private bool HoldingJump = false;
		private bool HoldingJumpForFly = false;
		private bool PrevHoldingJump = false;
		private bool IntendedJump = false;
		private bool IntendedDash = false;
		private bool IntendedRush = false;
		private bool IntendedPound = false;
		private bool PrevInWater = false;
		private bool PrevGrounded = false;
		private bool GrabDropLock = true;
		private bool GrabFlipUpLock = true;
		private bool AllowGrabSideMoveUp = false;
		private bool LockedFacingRight = true;
		private bool _FacingRight = true;
		private int? ClimbPositionCorrect = null;
		private int LockedFacingFrame = int.MinValue;
		private int RequireJumpFrame = int.MinValue;


		#endregion




		#region --- MSG ---


		private void OnActivated_Movement () {
			Width = MovementWidth;
			Height = MovementHeight;
			OffsetX = -MovementWidth / 2;
			OffsetY = 0;
			IsFlying = false;
			Hitbox = new RectInt(X, Y, MovementWidth, MovementHeight);
			RequireJumpFrame = int.MinValue;
		}


		private void PhysicsUpdate_Movement_GamePlay () {
			MovementUpdate_Cache();
			MovementUpdate_GrabFlip();
			MovementUpdate_ResetJumpCount();
			MovementUpdate_Jump();
			MovementUpdate_Dash();
			MovementState = GetCurrentMovementState();
			if (!IsInsideGround) {
				// General
				if (PhysicsEnable) {
					MovementUpdate_VelocityX();
					MovementUpdate_VelocityY();
				}
			} else {
				// Inside Ground
				VelocityX = IntendedX * WalkSpeed;
				VelocityY = VelocityY.MoveTowards(0, 2);
				if (IntendedJump) {
					VelocityY = WalkSpeed;
					Bounce();
				}
			}
			MovementUpdate_ClipCorrect();
		}


		private void PhysicsUpdate_Movement_After () {
			IntendedJump = false;
			IntendedDash = false;
			IntendedPound = false;
			IntendedRush = false;
			PrevHoldingJump = HoldingJump;
			if (MovementState == CharacterMovementState.Run) {
				if (LastStartRunFrame < 0) LastStartRunFrame = Game.GlobalFrame;
			} else if (LastStartRunFrame >= 0) {
				LastStartRunFrame = int.MinValue;
			}
		}


		private void MovementUpdate_Cache () {

			int frame = Game.GlobalFrame;

			// Ground
			if (IsGrounded) LastGroundingFrame = frame;
			if (!PrevGrounded && IsGrounded) LastGroundFrame = frame;
			PrevGrounded = IsGrounded;

			// Crash
			if (OnSlippy && IsGrounded && !IsCrashing && IsRunning) {
				if (LastSlippyMoveStartFrame < 0) {
					LastSlippyMoveStartFrame = Game.GlobalFrame;
				}
			} else {
				LastSlippyMoveStartFrame = int.MinValue;
			}
			if (CrashDuration > 0) {
				if (
					CrashWhenSlippy &&
					((LastSlippyMoveStartFrame >= 0 && frame > LastSlippyMoveStartFrame + CrashRunDurationRequire) ||
					(OnSlippy && IsGrounded && IsRushing))
				) {
					LastCrashFrame = frame;
				}
				IsCrashing = frame >= LastCrashFrame && frame < LastCrashFrame + CrashDuration;
			} else if (IsCrashing) {
				IsCrashing = false;
			}

			// In/Out Water
			if (PrevInWater != InWater) {
				LastDashFrame = int.MinValue;
				IsDashing = false;
				IsRushing = false;
				if (InWater) {
					// In Water
					VelocityY = VelocityY * InWaterSpeedLoseRate / 1000;
				} else {
					// Out Water
					if (VelocityY > 0) VelocityY = JumpSpeed;
				}
			}
			PrevInWater = InWater;

			// Climb
			ClimbPositionCorrect = null;
			if (ClimbAvailable && !IsRushing && !IsCrashing) {
				if (HoldingJump && CurrentJumpCount > 0 && VelocityY > 0) {
					IsClimbing = false;
				} else {
					bool overlapClimb = ClimbCheck();
					if (!IsClimbing) {
						if (overlapClimb && IntendedY > 0 && !IsSquatting) IsClimbing = true;
					} else {
						if (IsGrounded || !overlapClimb) IsClimbing = false;
					}
				}
			} else {
				IsClimbing = false;
			}
			if (IsClimbing) LastClimbFrame = frame;

			// Rush
			if (
				IntendedRush && RushAvailable && !IsCrashing &&
				frame > LastRushFrame + RushDuration + RushStiff + RushCooldown &&
				RushEnvironmentCheck()
			) {
				IsRushing = true;
				IsClimbing = false;
				LastRushFrame = frame;
				VelocityY = 0;
			}
			if (IsRushing && frame > LastRushFrame + RushDuration + RushStiff) {
				IsRushing = false;
				VelocityX = FacingRight ? RushStopSpeed : -RushStopSpeed;
			}

			// Dash
			IsDashing =
				DashAvailable && DashSpeed > 0 && !IsCrashing &&
				!IsClimbing && !IsInsideGround && !IsRushing && frame < LastDashFrame + DashDuration;
			if (IsDashing && IntendedY != -1) {
				// Stop when Dashing Without Holding Down
				LastDashFrame = int.MinValue;
				IsDashing = false;
				VelocityX = VelocityX * DashCancelLoseRate / 1000;
			}

			// Squat
			bool squatting =
				SquatAvailable && IsGrounded && !IsClimbing && !InSand && !IsInsideGround && !IsCrashing &&
				((!IsDashing && !IsRushing && IntendedY < 0) || ForceSquatCheck());
			if (!IsSquatting && squatting) LastSquatFrame = frame;
			if (squatting) LastSquattingFrame = frame;
			IsSquatting = squatting;

			// Pound
			IsPounding =
				PoundAvailable && !IsGrounded && !IsGrabbingSide && !IsGrabbingTop && !IsCrashing &&
				!IsClimbing && !InWater && !IsDashing && !IsRushing && !IsInsideGround &&
				(IsPounding ? IntendedY < 0 : IntendedPound);
			if (IsPounding) LastPoundingFrame = frame;

			// Grab
			bool prevGrabbingTop = IsGrabbingTop;
			bool prevGrabbingSide = !IsGrabbingTop && IsGrabbingSide;
			IsGrabbingTop = GrabTopCheck(out int grabbingY);
			IsGrabbingSide = GrabSideCheck(out AllowGrabSideMoveUp);
			if (IsGrabbingTop && IsGrabbingSide) {
				IsGrabbingTop = prevGrabbingTop;
				IsGrabbingSide = prevGrabbingSide;
			}
			if (IsGrabbingTop) {
				Y = grabbingY;
				Height = GrowingHeight * GrabTopHeightAmount / 1000;
			}
			if (IsGrabbingTop || IsGrabbingSide) LastGrabbingFrame = frame;

			// Grab Lock
			if (!IsGrabbingTop && IntendedY == 1) GrabFlipUpLock = true;
			if (IsGrabbingTop && IntendedY != 1) GrabFlipUpLock = false;
			if (!IsGrabbingTop && IntendedY == -1) GrabDropLock = true;
			if (IsGrabbingTop && IntendedY != -1) GrabDropLock = false;

			// Fly
			if (
				(!HoldingJump && frame > LastFlyFrame + FlyCooldown) ||
				IsGrounded || InWater || IsClimbing || IsDashing || IsRushing ||
				IsInsideGround || IsPounding || IsGrabbingSide || IsGrabbingTop
			) {
				IsFlying = false;
			}

			// Slide
			IsSliding = SlideCheck();
			if (IsSliding) LastSlidingFrame = frame;

			// Facing Right
			bool oldFacingRight = FacingRight;
			if (frame <= LockedFacingFrame && !IsSliding && !IsGrabbingSide) {
				FacingRight = LockedFacingRight;
			} else if (IntendedX != 0) {
				FacingRight = IntendedX > 0;
			}
			if (FacingRight != oldFacingRight) {
				LastFacingChangeFrame = frame;
			}

			// Facing Front
			FacingFront = !IsClimbing && (!Teleporting || TeleportEndFrame > 0);

			// Physics
			int width = InWater ? SwimWidth : MovementWidth;
			int height = GetCurrentHeight();
			Hitbox = new RectInt(
				X - width / 2,
				Y,
				width,
				Hitbox.height.MoveTowards(height, Const.CEL / 8, Const.CEL)
			);
			Width = Hitbox.width;
			Height = Hitbox.height;
			OffsetX = -width / 2;
			OffsetY = 0;

		}


		private void MovementUpdate_GrabFlip () {

			if (IsGrabbingTop) {

				// Grab Flip Up
				if (
					IntendedY > 0 &&
					GrabFlipThroughUpAvailable &&
					!GrabFlipUpLock &&
					GrabFlipCheck(true)
				) {
					LastGrabFlipUpFrame = Game.GlobalFrame;
					LastGrabCancelFrame = Game.GlobalFrame;
					LastDashFrame = int.MinValue;
					IsDashing = false;
				}

				// Grab Drop
				if (!GrabDropLock && IntendedY < 0) {
					if (!GrabSideCheck(out _)) {
						Y -= GRAB_TOP_CHECK_GAP;
						Hitbox.y = Y;
					}
					LastGrabCancelFrame = Game.GlobalFrame;
					IsGrabbingTop = false;
					LastDashFrame = int.MinValue;
					IsDashing = false;
				}
			}

			// Flip Down
			if (
				IntendedDash && IsGrounded && !InSand && GrabFlipThroughDownAvailable &&
				GrabFlipCheck(false)
			) {
				LastGrabFlipDownFrame = Game.GlobalFrame;
				LastGrabCancelFrame = Game.GlobalFrame;
				LastDashFrame = int.MinValue;
				IsDashing = false;
			}

		}


		private void MovementUpdate_ResetJumpCount () {

			if (CurrentJumpCount == 0) return;

			int frame = Game.GlobalFrame;

			// Reset Count on Grounded
			if (frame > LastJumpFrame + 1 && (IsGrounded || InWater) && !IntendedJump) {
				CurrentJumpCount = 0;
				return;
			}

			// Reset Count when Climb
			if (
				frame > LastJumpFrame + CLIMB_GROUND_TOLERANCE &&
				frame <= LastClimbFrame + CLIMB_GROUND_TOLERANCE
			) {
				CurrentJumpCount = 0;
				return;
			}

			// Reset Count when Slide
			if (
				ResetJumpCountWhenSlide &&
				frame > LastJumpFrame + SLIDE_GROUND_TOLERANCE &&
				frame <= LastSlidingFrame + SLIDE_GROUND_TOLERANCE
			) {
				CurrentJumpCount = 0;
				return;
			}

			// Reset Count when Grab
			if (
				ResetJumpCountWhenGrab &&
				frame > LastJumpFrame + GRAB_GROUND_TOLERANCE &&
				frame <= LastGrabbingFrame + GRAB_GROUND_TOLERANCE
			) {
				CurrentJumpCount = 0;
				return;
			}
		}


		private void MovementUpdate_Jump () {

			int frame = Game.GlobalFrame;

			bool movementAllowJump = !IsSquatting && !IsGrabbingTop && !IsInsideGround && !IsRushing && !IsGrabFlipping;

			// Perform Jump/Fly
			if (movementAllowJump && (!IsClimbing || JumpWhenClimbAvailable)) {
				// Jump
				if (CurrentJumpCount < JumpCount) {
					// Jump
					if (IntendedJump || frame < RequireJumpFrame + JUMP_REQUIRE_TOLERANCE) {
						// Perform Jump
						CurrentJumpCount++;
						VelocityY = Mathf.Max(InWater ? SwimJumpSpeed : JumpSpeed, VelocityY);
						if (IsGrabbingSide) {
							X += FacingRight ? -6 : 6;
						} else if (IsGrabbingTop) {
							VelocityY = 0;
							Y -= 3;
						}
						LastDashFrame = int.MinValue;
						IsDashing = false;
						IsSliding = false;
						IsGrabbingSide = false;
						IsGrabbingTop = false;
						LastJumpFrame = frame;
						IntendedJump = false;
						IsClimbing = false;
						RequireJumpFrame = int.MinValue;
					}
				} else if (FlyAvailable) {
					// Fly
					if (frame > LastFlyFrame + FlyCooldown) {
						// Cooldown Ready
						if (IntendedJump || (HoldingJump && HoldingJumpForFly)) {
							LastDashFrame = int.MinValue;
							IsFlying = true;
							IsClimbing = false;
							IsDashing = false;
							LastFlyFrame = frame;
							if (CurrentJumpCount <= JumpCount) {
								VelocityY = Mathf.Max(FlyRiseSpeed, VelocityY);
							}
							CurrentJumpCount++;
							IntendedJump = false;
							HoldingJumpForFly = false;
							RequireJumpFrame = int.MinValue;
						}
					} else {
						// Not Cooldown
						if (IntendedJump) HoldingJumpForFly = true;
						if (!HoldingJump) HoldingJumpForFly = false;
					}
				}
			}

			// Fall off Edge
			if (
				GrowJumpCountWhenFallOffEdge &&
				CurrentJumpCount == 0 &&
				!IsGrounded && !InWater && !IsClimbing &&
				frame > LastGroundingFrame + JUMP_TOLERANCE
			) {
				CurrentJumpCount++;
			}

			// Jump Release
			if (movementAllowJump && PrevHoldingJump && !HoldingJump) {
				// Lose Speed if Raising
				if (!IsGrounded && CurrentJumpCount <= JumpCount && VelocityY > 0) {
					VelocityY = VelocityY * JumpReleaseLoseRate / 1000;
				}
			}
		}


		private void MovementUpdate_Dash () {

			if (!IntendedDash || !IsGrounded || InSand || IsGrabFlipping) return;

			// Jump Though Oneway
			if (JumpDownThoughOneway && JumpThoughOnewayCheck()) {
				PerformMove(0, -Const.HALF, ignoreOneway: true);
				VelocityY = 0;
				return;
			}

			// Dash
			if (!DashAvailable || Game.GlobalFrame <= LastDashFrame + DashDuration + DashCooldown) return;
			LastDashFrame = Game.GlobalFrame;
			IsDashing = true;
			VelocityY = 0;
		}


		private void MovementUpdate_VelocityX () {

			int speed;
			int acc = int.MaxValue;
			int dcc = int.MaxValue;

			switch (MovementState) {

				// Walk and Run
				default:
					bool running = ReadyForRun;
					speed = IntendedX * (running ? RunSpeed : WalkSpeed);
					bool braking = (speed > 0 && VelocityX < 0) || (speed < 0 && VelocityX > 0);
					acc = running ?
						(braking ? RunBrakeAcceleration : RunAcceleration) :
						(braking ? WalkBrakeAcceleration : WalkAcceleration);
					dcc = running ? RunDeceleration : WalkDeceleration;
					break;

				// Squat
				case CharacterMovementState.SquatIdle:
				case CharacterMovementState.SquatMove:
					speed = IntendedX * SquatSpeed;
					acc = SquatAcceleration;
					dcc = SquatDeceleration;
					break;

				// Swim
				case CharacterMovementState.SwimMove:
					speed = IntendedX * SwimSpeed;
					acc = SwimAcceleration;
					dcc = SwimDeceleration;
					break;

				// Stop
				case CharacterMovementState.Slide:
				case CharacterMovementState.GrabSide:
				case CharacterMovementState.GrabFlip:
					speed = 0;
					break;

				// Dash
				case CharacterMovementState.Dash:
					speed = FacingRight ? DashSpeed : -DashSpeed;
					acc = DashAcceleration;
					break;

				// Rush
				case CharacterMovementState.Rush:
					speed =
						Game.GlobalFrame > LastRushFrame + RushDuration ? 0 :
						FacingRight ? RushSpeed : -RushSpeed;
					acc = RushAcceleration;
					dcc = RushDeceleration;
					break;

				// Crash
				case CharacterMovementState.Crash:
					speed = 0;
					acc = int.MaxValue;
					dcc = CrashDeceleration;
					break;

				// Climb
				case CharacterMovementState.Climb:
					speed = ClimbPositionCorrect.HasValue ? 0 : IntendedX * ClimbSpeedX;
					if (ClimbPositionCorrect.HasValue) X = X.MoveTowards(ClimbPositionCorrect.Value, CLIMB_CORRECT_DELTA);
					break;

				// Fly
				case CharacterMovementState.Fly: // Glide
					if (FlyGlideAvailable) {
						speed = FacingRight ? FlyMoveSpeed : -FlyMoveSpeed;
						acc = FlyAcceleration;
						dcc = FlyDeceleration;
					} else {
						speed = IntendedX * FlyMoveSpeed;
						acc = FlyAcceleration;
						dcc = FlyDeceleration;
					}
					break;

				// Grab Top
				case CharacterMovementState.GrabTop:
					speed = IntendedX * GrabMoveSpeedX;
					break;


			}

			// Speed Lose on Attack
			int loseRate = MovementLoseRateOnAttack;
			if (loseRate != 1000 && IsAttacking && IsGrounded) {
				speed = speed * loseRate / 1000;
			}

			// Push
			if (PushAvailable && !IsCrashing && IntendedX != 0 && speed != 0 && !NavigationEnable) {
				var hits = CellPhysics.OverlapAll(
				PhysicsMask.ENVIRONMENT,
				Rect.Shrink(0, 0, 4, 4).Edge(IntendedX < 0 ? Direction4.Left : Direction4.Right),
				out int count, this
			);
				bool pushing = false;
				int pushSpeed = IntendedX * PushSpeed;
				for (int i = 0; i < count; i++) {
					var hit = hits[i];
					if (hit.Entity is not Rigidbody rig || !rig.AllowBeingPush) continue;
					rig.Push(pushSpeed);
					pushing = true;
				}
				if (pushing) {
					speed = pushSpeed;
					acc = int.MaxValue;
					dcc = int.MaxValue;
				}
			}

			// Final
			VelocityX = VelocityX.MoveTowards(speed, acc, dcc);
		}


		private void MovementUpdate_VelocityY () {

			GravityScale = IsGrounded || VelocityY <= 0 ? 1000 : (int)JumpRiseGravityRate;

			switch (MovementState) {

				// Swim
				case CharacterMovementState.SwimIdle:
				case CharacterMovementState.SwimMove:
					if (IntendedY != 0) {
						VelocityY = VelocityY.MoveTowards(
							IntendedY * SwimSpeed, SwimAcceleration, SwimDeceleration
						);
						GravityScale = 0;
					} else {
						GravityScale = 1000;
					}
					break;

				// Climb
				case CharacterMovementState.Climb:
					VelocityY = (IntendedY <= 0 || ClimbCheck(true) ? IntendedY : 0) * ClimbSpeedY;
					GravityScale = 0;
					break;

				// Pound
				case CharacterMovementState.Pound:
					GravityScale = 0;
					VelocityY = -PoundSpeed;
					break;

				// Fly
				case CharacterMovementState.Fly:
					GravityScale = VelocityY > 0 ? FlyGravityRiseRate : FlyGravityFallRate;
					VelocityY = Mathf.Max(VelocityY, -FlyFallSpeed);
					break;

				// Slide
				case CharacterMovementState.Slide:
					if (VelocityY < -SlideDropSpeed) {
						VelocityY = -SlideDropSpeed;
						GravityScale = 0;
					}
					break;

				// Grab Top
				case CharacterMovementState.GrabTop:
					GravityScale = 0;
					VelocityY = 0;
					break;

				// Grab Side
				case CharacterMovementState.GrabSide:
					GravityScale = 0;
					VelocityY = IntendedY <= 0 || AllowGrabSideMoveUp ? IntendedY * GrabMoveSpeedY : 0;
					break;

				// Grab Flip
				case CharacterMovementState.GrabFlip:
					GravityScale = 0;
					VelocityY = (MovementHeight + Const.CEL + 12) / Mathf.Max(GrabFlipThroughDuration, 1) + 1;
					if (!IsGrabFlippingUp) VelocityY *= -1;
					break;

			}
		}


		private void MovementUpdate_ClipCorrect () {

			if (IsGrounded || VelocityY <= 0) return;

			var rect = Rect;
			int size = VelocityY + 1;

			// Clip Left
			if (CheckCorrect(
				new RectInt(rect.xMin, rect.yMax, 1, size),
				new RectInt(rect.xMin + CLIP_CORRECT_TOLERANCE, rect.yMax, rect.width - CLIP_CORRECT_TOLERANCE, size),
				out var hitRect
			)) {
				PerformMove(hitRect.xMax - rect.xMin, 0);
			}

			// Clip Right
			if (CheckCorrect(
				new RectInt(rect.xMin + Width, rect.yMax, 1, size),
				new RectInt(rect.xMin, rect.yMax, rect.width - CLIP_CORRECT_TOLERANCE, size),
				out hitRect
			)) {
				PerformMove(hitRect.xMin - rect.xMax, 0);
			}

			// Func
			bool CheckCorrect (RectInt trueRect, RectInt falseRect, out RectInt hitRect) {
				if (
					CellPhysics.Overlap(CollisionMask, trueRect, out var hit, this) &&
					!CellPhysics.Overlap(CollisionMask, falseRect, this)
				) {
					hitRect = hit.Rect;
					return true;
				} else {
					hitRect = default;
					return false;
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


		public void Jump () {
			IntendedJump = true;
			RequireJumpFrame = Game.GlobalFrame;
			if (CancelAttackOnJump) LastAttackFrame = int.MinValue;
		}


		public void Dash () => IntendedDash = true;
		public void Pound () => IntendedPound = true;
		public void Rush () => IntendedRush = true;
		public void Crash () => LastCrashFrame = Game.GlobalFrame;


		public void LockFacingRight (bool facingRight, int duration = 1) {
			LockedFacingFrame = Game.GlobalFrame + duration;
			LockedFacingRight = facingRight;
		}


		#endregion




		#region --- LGC ---


		private void MoveLogic (int x, int y) {
			if (IntendedX != 0 && x == 0) LastEndMoveFrame = Game.GlobalFrame;
			if (IntendedX == 0 && x != 0) LastStartMoveFrame = Game.GlobalFrame;
			if (x != 0) RunningAccumulateFrame++;
			if (x == 0 && Game.GlobalFrame > LastEndMoveFrame + RUN_BREAK_FRAME) RunningAccumulateFrame = 0;
			IntendedX = x;
			IntendedY = y;
			if (x != 0 || y != 0) {
				LastMoveDirection = new(IntendedX, IntendedY);
			}
		}


		private int GetCurrentHeight () {
			int growingHeight = GrowingHeight;
			if (IsSquatting) return growingHeight * SquatHeightAmount / 1000;
			if (IsRolling) return growingHeight * RollingHeightAmount / 1000;
			if (IsDashing) return growingHeight * DashHeightAmount / 1000;
			if (IsRushing) return growingHeight * RushHeightAmount / 1000;
			if (InWater) return growingHeight * SwimHeightAmount / 1000;
			if (IsFlying) return growingHeight * FlyHeightAmount / 1000;
			if (IsGrabbingTop) return growingHeight * GrabTopHeightAmount / 1000;
			if (IsGrabbingSide) return growingHeight * GrabSideHeightAmount / 1000;
			return growingHeight;
		}


		private CharacterMovementState GetCurrentMovementState () =>
			IsCrashing ? CharacterMovementState.Crash :
			IsFlying ? CharacterMovementState.Fly :
			IsClimbing ? CharacterMovementState.Climb :
			IsPounding ? CharacterMovementState.Pound :
			IsSliding ? CharacterMovementState.Slide :
			IsGrabFlipping ? CharacterMovementState.GrabFlip :
			IsGrabbingTop ? CharacterMovementState.GrabTop :
			IsGrabbingSide ? CharacterMovementState.GrabSide :
			IsRushing ? CharacterMovementState.Rush :
			IsDashing ? CharacterMovementState.Dash :
			IsSquatting ? (IsMoving ? CharacterMovementState.SquatMove : CharacterMovementState.SquatIdle) :
			InWater && !IsGrounded ? (IsMoving ? CharacterMovementState.SwimMove : CharacterMovementState.SwimIdle) :
			!IsGrounded && !InWater && !InSand && !IsClimbing ? (VelocityY > 0 ? CharacterMovementState.JumpUp : CharacterMovementState.JumpDown) :
			IsMoving ? ReadyForRun && !IsInsideGround ? CharacterMovementState.Run : CharacterMovementState.Walk :
			CharacterMovementState.Idle;


		// Check
		private bool ForceSquatCheck () {

			if (IsInsideGround) return false;

			int GAP = Width / 8;
			var rect = new RectInt(
				X + OffsetX + GAP,
				Y + OffsetY + MovementHeight / 2,
				Width - GAP * 2,
				MovementHeight / 2
			);

			// Oneway Check
			if ((IsSquatting || IsDashing) && !CellPhysics.RoomCheckOneway(
				PhysicsMask.LEVEL, rect, this, Direction4.Up, false
			)) return true;

			// Overlap Check
			return CellPhysics.Overlap(PhysicsMask.MAP, rect, null);
		}


		private bool ClimbCheck (bool up = false) {
			if (IsInsideGround) return false;
			if (CellPhysics.Overlap(
				PhysicsMask.MAP,
				up ? Rect.Shift(0, ClimbSpeedY) : Rect,
				this,
				OperationMode.TriggerOnly,
				Const.CLIMB_TAG
			)) {
				return true;
			}
			if (CellPhysics.Overlap(
				PhysicsMask.MAP,
				up ? Rect.Shift(0, ClimbSpeedY) : Rect,
				out var info,
				this,
				OperationMode.TriggerOnly,
				Const.CLIMB_STABLE_TAG
			)) {
				ClimbPositionCorrect = info.Rect.CenterInt().x;
				return true;
			}
			return false;
		}


		private bool SlideCheck () {
			if (
				!SlideAvailable || IsGrounded || IsClimbing || IsDashing || IsGrabbingTop || IsGrabbingSide ||
				InWater || IsSquatting || IsPounding || IsCrashing ||
				Game.GlobalFrame < LastJumpFrame + SLIDE_JUMP_CANCEL ||
				IntendedX == 0 || VelocityY > -SlideDropSpeed
			) return false;
			var rect = new RectInt(
				IntendedX > 0 ? Hitbox.xMax : Hitbox.xMin - 1,
				Hitbox.y + Hitbox.height / 2,
				1, 1
			);
			if (SlideOnAnyBlock) {
				var hits = CellPhysics.OverlapAll(PhysicsMask.MAP, rect, out int count, this, OperationMode.ColliderOnly);
				for (int i = 0; i < count; i++) {
					var hit = hits[i];
					if (hit.Tag == Const.NO_SLIDE_TAG) continue;
					if (hit.Tag == Const.GRAB_TOP_TAG) continue;
					if (hit.Tag == Const.GRAB_SIDE_TAG) continue;
					return true;
				}
				return false;
			} else {
				return CellPhysics.Overlap(
					PhysicsMask.MAP, rect, this, OperationMode.ColliderOnly, Const.SLIDE_TAG
				);
			}
		}


		private bool GrabTopCheck (out int grabbingY) {
			grabbingY = 0;
			if (
				!GrabTopAvailable || IsInsideGround || IsGrounded || IsCrashing ||
				IsClimbing || IsDashing || InWater || IsSquatting || IsGrabFlipping
			) return false;
			if (Game.GlobalFrame < LastGrabCancelFrame + GRAB_DROP_CANCEL) return false;
			int height = MovementHeight;
			var rect = new RectInt(
				Hitbox.xMin,
				Y + height / 2,
				Hitbox.width,
				height / 2 + GRAB_TOP_CHECK_GAP
			);
			if (CellPhysics.Overlap(
				PhysicsMask.MAP, rect, out var hit, this,
				OperationMode.ColliderOnly, Const.GRAB_TOP_TAG
			) || CellPhysics.Overlap(
				PhysicsMask.MAP, rect, out hit, this,
				OperationMode.ColliderOnly, Const.GRAB_TAG
			)) {
				grabbingY = hit.Rect.yMin - (GrowingHeight * GrabTopHeightAmount / 1000);
				return true;
			}
			return false;
		}


		private bool GrabSideCheck (out bool allowMoveUp) {
			allowMoveUp = false;
			if (
				!GrabSideAvailable || IsInsideGround || IsGrounded || IsClimbing || IsDashing || IsCrashing ||
				InWater || IsSquatting || IsGrabFlipping ||
				Game.GlobalFrame < LastJumpFrame + GRAB_JUMP_CANCEL
			) return false;
			if (!IsGrabbingSide && VelocityY > GrabMoveSpeedY / 2) return false;
			var rectD = new RectInt(
				FacingRight ? Hitbox.xMax : Hitbox.xMin - 1,
				Hitbox.yMin + Hitbox.height / 4,
				1,
				Hitbox.height / 4
			);
			var rectU = new RectInt(
				FacingRight ? Hitbox.xMax : Hitbox.xMin - 1,
				Hitbox.yMax - Hitbox.height / 4,
				1,
				Hitbox.height / 4
			);
			bool allowGrab =
				(AllowCheck(rectD, Const.GRAB_SIDE_TAG) || AllowCheck(rectD, Const.GRAB_TAG)) &&
				(AllowCheck(rectU, Const.GRAB_SIDE_TAG) || AllowCheck(rectU, Const.GRAB_TAG));
			if (allowGrab) {
				allowMoveUp = CellPhysics.Overlap(
					PhysicsMask.MAP, rectU.Shift(0, rectU.height), this, OperationMode.ColliderOnly, Const.GRAB_SIDE_TAG
				) || CellPhysics.Overlap(
					PhysicsMask.MAP, rectU.Shift(0, rectU.height), this, OperationMode.ColliderOnly, Const.GRAB_TAG
				);
			}
			return allowGrab;
			// Func
			bool AllowCheck (RectInt rect, int tag) => CellPhysics.Overlap(PhysicsMask.MAP, rect, this, OperationMode.ColliderOnly, tag);
		}


		private bool JumpThoughOnewayCheck () {
			var rect = new RectInt(Hitbox.xMin, Hitbox.yMin + 4 - Const.CEL / 4, Hitbox.width, Const.CEL / 4);
			if (CellPhysics.Overlap(PhysicsMask.MAP, rect, this)) return false;
			var hits = CellPhysics.OverlapAll(
				PhysicsMask.MAP, rect, out int count, this,
				OperationMode.TriggerOnly, Const.ONEWAY_UP_TAG
			);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Rect.yMax <= Hitbox.y + 16) return true;
			}
			return false;
		}


		private bool GrabFlipCheck (bool flipUp) {
			int x = X - Width / 4;
			int width = Width / 2;
			if (flipUp) {
				// Up
				// No Block Above
				if (CellPhysics.Overlap(
					PhysicsMask.MAP,
					new RectInt(x, Y + (GrowingHeight * GrabTopHeightAmount / 1000) + Const.CEL + Const.HALF, width, 1),
					this
				)) return false;
				return true;
			} else {
				// Down
				// No Block Below
				if (CellPhysics.Overlap(
					PhysicsMask.MAP,
					new RectInt(x, Y - Const.CEL - Const.HALF, width, 1),
					this
				)) return false;
				// Standing on Grab-Top Block
				var hits = CellPhysics.OverlapAll(
					PhysicsMask.MAP,
					new RectInt(x, Y + 4 - Const.CEL / 4, width, Const.CEL / 4), out int count,
					this, OperationMode.ColliderOnly, Const.GRAB_TOP_TAG
				);
				for (int i = 0; i < count; i++) {
					var hit = hits[i];
					if (hit.Rect.yMax <= Hitbox.y + 16) return true;
				}
			}
			return false;
		}


		private bool RushEnvironmentCheck () =>
			(RushWhenSquat || !IsSquatting) &&
			(RushInWater || !InWater) &&
			(RushInAir || IsGrounded) &&
			(RushWhenClimb || !IsClimbing);


		#endregion




	}
}