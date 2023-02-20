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
		Dash, Rush, Pound, Climb, Fly, Slide, GrabTop, GrabSide, GrabFlip,
	}


	public abstract partial class eCharacter {




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
		private const int GRAB_DROP_CANCEL = 6;
		private const int GRAB_TOP_CHECK_GAP = 128;
		private const int CLIP_CORRECT_TOLERANCE = Const.CEL / 4;

		// Api
		public Vector2Int LastMoveDirection { get; private set; } = default;
		public int IntendedX { get; private set; } = 0;
		public int IntendedY { get; private set; } = 0;
		public int CurrentJumpCount { get; private set; } = 0;
		public bool UseFreeStyleSwim => SwimInFreeStyle;
		public bool FacingRight { get; private set; } = true;

		// Frame Cache
		public int RunningAccumulateFrame { get; private set; } = 0;
		public int LastGroundFrame { get; private set; } = int.MinValue;
		public int LastGroundingFrame { get; private set; } = int.MinValue;
		public int LastStartMoveFrame { get; private set; } = int.MinValue;
		public int LastEndMoveFrame { get; private set; } = int.MinValue;
		public int LastJumpFrame { get; private set; } = int.MinValue;
		public int LastClimbFrame { get; private set; } = int.MinValue;
		public int LastDashFrame { get; private set; } = int.MinValue;
		public int LastRushFrame { get; private set; } = int.MinValue;
		public int LastSquatFrame { get; private set; } = int.MinValue;
		public int LastSquatingFrame { get; private set; } = int.MinValue;
		public int LastPoundingFrame { get; private set; } = int.MinValue;
		public int LastSlidingFrame { get; private set; } = int.MinValue;
		public int LastGrabingFrame { get; private set; } = int.MinValue;
		public int LastFlyFrame { get; private set; } = int.MinValue;
		public int LastGrabFlipFrame { get; private set; } = int.MinValue;
		public int LastGrabTopDropFrame { get; private set; } = int.MinValue;

		// Movement State
		public bool ReadyForRun => RunningAccumulateFrame >= RunAccumulation;
		public bool IsRolling => !InWater && !IsPounding && !IsFlying && !IsRushing && ((JumpWithRoll && CurrentJumpCount > 0) || (SecondJumpWithRoll && CurrentJumpCount > 1));
		public bool IsGrabFliping => Game.GlobalFrame < LastGrabFlipFrame + Mathf.Max(GrabFlipThroughDuration, 1);
		public MovementState MoveState { get; set; } = MovementState.Idle;
		public bool IsDashing { get; private set; } = false;
		public bool IsRushing { get; private set; } = false;
		public bool IsSquating { get; private set; } = false;
		public bool IsPounding { get; private set; } = false;
		public bool IsClimbing { get; private set; } = false;
		public bool IsFlying { get; private set; } = false;
		public bool IsSliding { get; private set; } = false;
		public bool IsGrabingTop { get; private set; } = false;
		public bool IsGrabingSide { get; private set; } = false;

		// Short
		private int CurrentDashDuration => InWater && SwimInFreeStyle ? FreeSwimDashDuration : DashDuration;
		private int CurrentDashCooldown => InWater && SwimInFreeStyle ? FreeSwimDashCooldown : DashCooldown;

		// Data
		private static readonly PhysicsCell[] c_SlideCheck = new PhysicsCell[8];
		private static readonly PhysicsCell[] c_GroundThoughCheck = new PhysicsCell[8];
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
		private int? ClimbPositionCorrect = null;
		private int LockedFacingFrame = int.MinValue;
		private int RequireJumpFrame = int.MinValue;


		#endregion




		#region --- MSG ---


		private void OnActived_Movement () {
			Width = MovementWidth;
			Height = MovementHeight;
			OffsetX = -MovementWidth / 2;
			OffsetY = 0;
			IsFlying = false;
			Hitbox = new RectInt(X, Y, MovementWidth, MovementHeight);
			RequireJumpFrame = int.MinValue;
		}


		private void PhysicsUpdate_GamePlay_Movement () {
			MovementUpdate_Cache();
			MovementUpdate_Jump();
			MovementUpdate_ResetJumpCount();
			MovementUpdate_Dash();
			MoveState = GetCurrentMovementState();
			if (!IsInsideGround) {
				if (PhysicsEnable) {
					MovementUpdate_VelocityX();
					MovementUpdate_VelocityY();
				}
			} else {
				VelocityX = IntendedX * WalkSpeed;
				VelocityY = VelocityY.MoveTowards(0, 2);
				if (IntendedJump) {
					VelocityY = WalkSpeed;
					RenderBounce();
				}
			}
			MovementUpdate_ClipCorrect();
		}


		private void PhysicsUpdate_Movement () {
			IntendedJump = false;
			IntendedDash = false;
			IntendedPound = false;
			IntendedRush = false;
			PrevHoldingJump = HoldingJump;
		}


		private void MovementUpdate_Cache () {

			int frame = Game.GlobalFrame;

			// Ground
			if (IsGrounded) LastGroundingFrame = frame;
			if (!PrevGrounded && IsGrounded) LastGroundFrame = frame;
			PrevGrounded = IsGrounded;

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
			if (ClimbAvailable && !IsRushing) {
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
			if (IsClimbing) LastClimbFrame = frame;

			// Rush
			if (
				IntendedRush && RushAvailable &&
				frame > LastRushFrame + RushDuration + RushStiff + RushCooldown &&
				RushEnvironmentCheck()
			) {
				IsRushing = true;
				IsClimbing = false;
				LastRushFrame = frame;
			}

			if (
				IsRushing &&
				//(frame > LastRushFrame + RushDuration + RushStiff || IntendedX == 0 || FacingRight != IntendedX > 0)
				frame > LastRushFrame + RushDuration + RushStiff
			) {
				IsRushing = false;
				VelocityX = FacingRight ? RushStopSpeed.Value : -RushStopSpeed.Value;
			}

			// Dash
			if (InWater && SwimInFreeStyle) {
				IsDashing =
					DashAvailable && FreeSwimDashSpeed > 0 &&
					!IsClimbing && !IsInsideGround && !IsRushing && frame < LastDashFrame + FreeSwimDashDuration;
			} else {
				IsDashing =
					DashAvailable && DashSpeed > 0 &&
					!IsClimbing && !IsInsideGround && !IsRushing && frame < LastDashFrame + CurrentDashDuration;
				if (IsDashing && IntendedY != -1) {
					// Stop when Dashing Without Holding Down
					LastDashFrame = int.MinValue;
					IsDashing = false;
					VelocityX = VelocityX * DashCancelLoseRate / 1000;
				}
			}

			// Squat
			bool squating =
				SquatAvailable && IsGrounded && !IsClimbing && !InSand && !IsInsideGround &&
				((!IsDashing && !IsRushing && IntendedY < 0) || ForceSquatCheck());
			if (!IsSquating && squating) LastSquatFrame = frame;
			if (squating) LastSquatingFrame = frame;
			IsSquating = squating;

			// Pound
			IsPounding =
				PoundAvailable && !IsGrounded && !IsGrabingSide && !IsGrabingTop &&
				!IsClimbing && !InWater && !IsDashing && !IsRushing && !IsInsideGround &&
				(IsPounding ? IntendedY < 0 : IntendedPound);
			if (IsPounding) LastPoundingFrame = frame;

			// Grab
			bool prevGrabingTop = IsGrabingTop;
			bool prevGrabingSide = !IsGrabingTop && IsGrabingSide;
			IsGrabingTop = GrabTopCheck(out int grabingY);
			IsGrabingSide = GrabSideCheck(out AllowGrabSideMoveUp);
			if (IsGrabingTop && IsGrabingSide) {
				IsGrabingTop = prevGrabingTop;
				IsGrabingSide = prevGrabingSide;
			}
			if (IsGrabingTop) {
				Y = grabingY;
				Height = GrabTopHeight;
			}
			if (IsGrabingTop || IsGrabingSide) LastGrabingFrame = frame;

			// Grab Lock
			if (!IsGrabingTop && IntendedY == 1) GrabFlipUpLock = true;
			if (IsGrabingTop && IntendedY != 1) GrabFlipUpLock = false;
			if (!IsGrabingTop && IntendedY == -1) GrabDropLock = true;
			if (IsGrabingTop && IntendedY != -1) GrabDropLock = false;

			// Fly
			if (
				(!HoldingJump && frame > LastFlyFrame + FlyCooldown) ||
				IsGrounded || InWater || IsClimbing || IsDashing || IsRushing ||
				IsInsideGround || IsPounding || IsGrabingSide || IsGrabingTop
			) {
				IsFlying = false;
			}

			// Slide
			IsSliding = SlideCheck();
			if (IsSliding) LastSlidingFrame = frame;

			// Facing Right
			if (Game.GlobalFrame <= LockedFacingFrame && !IsSliding && !IsGrabingSide) {
				FacingRight = LockedFacingRight;
			} else if (IntendedX != 0) {
				FacingRight = IntendedX > 0;
			}

			// Physics
			int width = InWater ? SwimWidth : MovementWidth;
			int height = GetCurrentHeight();
			Hitbox = new(X - width / 2, Y, width, Hitbox.height.MoveTowards(height, Const.CEL / 8, Const.CEL));
			Width = Hitbox.width;
			Height = Hitbox.height;
			OffsetX = -width / 2;
			OffsetY = 0;

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
				frame <= LastGrabingFrame + GRAB_GROUND_TOLERANCE
			) {
				CurrentJumpCount = 0;
				return;
			}
		}


		private void MovementUpdate_Jump () {

			int frame = Game.GlobalFrame;

			// Perform Jump/Fly
			if (
				!IsSquating && !IsGrabingTop && !IsInsideGround && !IsRushing &&
				(!IsClimbing || JumpWhenClimbAvailable)
			) {
				// Jump
				if (CurrentJumpCount < JumpCount) {
					// Jump
					if (IntendedJump || frame < RequireJumpFrame + JUMP_REQUIRE_TOLERANCE) {
						if (InWater && SwimInFreeStyle) {
							// Free Dash in Water
							LastDashFrame = frame;
							IsDashing = true;
							VelocityX = 0;
							VelocityY = 0;
						} else {
							// Perform Jump
							CurrentJumpCount++;
							VelocityY = Mathf.Max(InWater ? SwimJumpSpeed : JumpSpeed, VelocityY);
							if (IsGrabingSide) {
								X += FacingRight ? -6 : 6;
							} else if (IsGrabingTop) {
								VelocityY = 0;
								Y -= 3;
							}
							LastDashFrame = int.MinValue;
							IsDashing = false;
							IsSliding = false;
							IsGrabingSide = false;
							IsGrabingTop = false;
							LastJumpFrame = frame;
						}
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
			if (PrevHoldingJump && !HoldingJump) {
				// Lose Speed if Raising
				if (!IsGrounded && CurrentJumpCount <= JumpCount && VelocityY > 0) {
					VelocityY = VelocityY * JumpReleaseLoseRate / 1000;
				}
			}
		}


		private void MovementUpdate_Dash () {

			if (!IntendedDash || !IsGrounded || InSand) return;

			// Jump Though Oneway
			if (JumpThoughOneway.Value && JumpThoughOnewayCheck()) {
				PerformMove(0, -Const.HALF, ignoreOneway: true);
				VelocityY = 0;
				return;
			}

			// Drop Though Grab Top
			if (GrabFlipThroughDown && GrabFlipThoughDownCheck()) {
				LastGrabFlipFrame = Game.GlobalFrame;
				VelocityY = -(MovementHeight + Const.CEL + 12) / Mathf.Max(GrabFlipThroughDuration, 1);
				return;
			}

			// Dash
			if (!DashAvailable || Game.GlobalFrame <= LastDashFrame + CurrentDashDuration + CurrentDashCooldown) return;
			LastDashFrame = Game.GlobalFrame;
			IsDashing = true;
			VelocityY = 0;
		}


		private void MovementUpdate_VelocityX () {

			int speed;
			int acc = int.MaxValue;
			int dcc = int.MaxValue;

			switch (MoveState) {

				// Walk and Run
				default:
					bool running = ReadyForRun;
					speed = IntendedX * (running ? RunSpeed : WalkSpeed);
					bool braking = (speed > 0 && VelocityX < 0) || (speed < 0 && VelocityX > 0);
					acc = running ?
						(braking ? RunBrakeAcceleration : RunAcceleration) :
						(braking ? WalkBrakeAcceleration : WalkAcceleration);
					dcc = running ? RunDecceleration : WalkDecceleration;
					break;

				// Squat
				case MovementState.SquatIdle:
				case MovementState.SquatMove:
					speed = IntendedX * SquatSpeed;
					acc = SquatAcceleration;
					dcc = SquatDecceleration;
					break;

				// Swim
				case MovementState.SwimMove:
					if (SwimInFreeStyle) {
						// Free Swim
						speed = IntendedX * FreeSwimSpeed;
						acc = FreeSwimAcceleration;
						dcc = FreeSwimDecceleration;
					} else {
						// Normal Swim
						speed = IntendedX * SwimSpeed;
						acc = SwimAcceleration;
						dcc = SwimDecceleration;
					}
					break;

				// Stop
				case MovementState.Slide:
				case MovementState.GrabSide:
				case MovementState.GrabFlip:
					speed = 0;
					break;

				// Dash
				case MovementState.Dash:
				case MovementState.SwimDash:
					if (InWater && SwimInFreeStyle && !IsGrounded) {
						// Free Water Dash
						speed = LastMoveDirection.x * FreeSwimDashSpeed;
						acc = FreeSwimDashAcceleration;
					} else {
						// Normal Dash
						speed = FacingRight ? DashSpeed : -DashSpeed;
						acc = DashAcceleration;
					}
					break;

				// Rush
				case MovementState.Rush:
					speed =
						Game.GlobalFrame > LastRushFrame + RushDuration ? 0 :
						FacingRight ? RushSpeed.Value : -RushSpeed.Value;
					acc = RushAcceleration.Value;
					dcc = RushDecceleration.Value;
					break;

				// Climb
				case MovementState.Climb:
					speed = ClimbPositionCorrect.HasValue ? 0 : IntendedX * ClimbSpeedX;
					if (ClimbPositionCorrect.HasValue) X = X.MoveTowards(ClimbPositionCorrect.Value, CLIMB_CORRECT_DELTA);
					break;

				// Fly
				case MovementState.Fly: // Glide
					if (FlyGlideAvailable.Value) {
						speed = FacingRight ? FlyMoveSpeed : -FlyMoveSpeed;
						acc = FlyAcceleration;
						dcc = FlyDecceleration;
					} else {
						speed = IntendedX * FlyMoveSpeed;
						acc = FlyAcceleration;
						dcc = FlyDecceleration;
					}
					break;

				// Grab Top
				case MovementState.GrabTop:
					speed = IntendedX * GrabMoveSpeedX;
					break;

			}

			VelocityX = VelocityX.MoveTowards(speed, acc, dcc);
		}


		private void MovementUpdate_VelocityY () {

			GravityScale = IsGrounded || VelocityY <= 0 ? 1000 : (int)JumpRiseGravityRate;

			switch (MoveState) {

				// Swim
				case MovementState.SwimIdle:
				case MovementState.SwimMove:
					if (SwimInFreeStyle) {
						VelocityY = VelocityY.MoveTowards(
							IntendedY * FreeSwimSpeed, FreeSwimAcceleration, FreeSwimDecceleration
						);
						GravityScale = 0;
					} else {
						if (IntendedY != 0) {
							VelocityY = VelocityY.MoveTowards(
								IntendedY * SwimSpeed, SwimAcceleration, SwimDecceleration
							);
							GravityScale = 0;
						} else {
							GravityScale = 1000;
						}
					}
					break;

				case MovementState.SwimDash:
					VelocityY = VelocityY.MoveTowards(
						LastMoveDirection.y * FreeSwimDashSpeed, FreeSwimDashAcceleration, int.MaxValue
					);
					GravityScale = 0;
					break;

				// Rush
				case MovementState.Rush:
					if (Game.GlobalFrame < LastRushFrame + RushDuration) {
						VelocityY = 0;
					}
					break;

				// Climb
				case MovementState.Climb:
					VelocityY = (IntendedY <= 0 || ClimbCheck(true) ? IntendedY : 0) * ClimbSpeedY;
					GravityScale = 0;
					break;

				// Pound
				case MovementState.Pound:
					GravityScale = 0;
					VelocityY = -PoundSpeed;
					break;

				// Fly
				case MovementState.Fly:
					GravityScale = VelocityY > 0 ? FlyGravityRiseRate : FlyGravityFallRate;
					VelocityY = Mathf.Max(VelocityY, -FlyFallSpeed);
					break;

				// Slide
				case MovementState.Slide:
					if (VelocityY < -SlideDropSpeed) {
						VelocityY = -SlideDropSpeed;
						GravityScale = 0;
					}
					break;

				// Grab Top
				case MovementState.GrabTop:
					GravityScale = 0;
					VelocityY = 0;
					// Flip Through Up
					if (IntendedY > 0 && GrabFlipThroughUp && !GrabFlipUpLock) {
						LastGrabFlipFrame = Game.GlobalFrame;
						VelocityY = (MovementHeight + Const.CEL + 12) / Mathf.Max(GrabFlipThroughDuration, 1);
					}
					// Grab Drop
					if (!GrabDropLock && IntendedY < 0) {
						if (!GrabSideCheck(out _)) {
							Y -= GRAB_TOP_CHECK_GAP;
							Hitbox.y = Y;
						}
						IsGrabingTop = false;
						LastGrabTopDropFrame = Game.GlobalFrame;
					}
					break;

				// Grab Side
				case MovementState.GrabSide:
					GravityScale = 0;
					VelocityY = IntendedY <= 0 || AllowGrabSideMoveUp ? IntendedY * GrabMoveSpeedY : 0;
					break;

				// Grab Flip
				case MovementState.GrabFlip:
					GravityScale = 0;
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
			if (CancelAttackOnJump) {
				CancelAttack();
			}
		}


		public void Dash () => IntendedDash = true;


		public void Pound () => IntendedPound = true;


		public void Rush () => IntendedRush = true;


		public void LockFacingRight (bool facingRight, int duration = 0) {
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
			if (IsSquating) return SquatHeight;
			if (IsRolling) return RollingHeight;
			if (IsDashing) return DashHeight;
			if (IsRushing) return RushHeight;
			if (InWater) return SwimHeight;
			if (IsFlying) return FlyHeight;
			if (IsGrabingTop) return GrabTopHeight;
			if (IsGrabingSide) return GrabSideHeight;
			return MovementHeight;
		}


		private MovementState GetCurrentMovementState () =>
			IsFlying ? MovementState.Fly :
			IsClimbing ? MovementState.Climb :
			IsPounding ? MovementState.Pound :
			IsSliding ? MovementState.Slide :
			IsGrabFliping ? MovementState.GrabFlip :
			IsGrabingTop ? MovementState.GrabTop :
			IsGrabingSide ? MovementState.GrabSide :
			IsRushing ? MovementState.Rush :
			IsDashing ? (!IsGrounded && InWater ? MovementState.SwimDash : MovementState.Dash) :
			IsSquating ? (IntendedX != 0 ? MovementState.SquatMove : MovementState.SquatIdle) :
			InWater && (SwimInFreeStyle || !IsGrounded) ? (IntendedX != 0 ? MovementState.SwimMove : MovementState.SwimIdle) :
			!IsGrounded && !InWater && !InSand && !IsClimbing ? (VelocityY > 0 ? MovementState.JumpUp : MovementState.JumpDown) :
			IntendedX != 0 ? ReadyForRun && !IsInsideGround ? MovementState.Run : MovementState.Walk :
			MovementState.Idle;


		// Check
		private bool ForceSquatCheck () {

			if (IsInsideGround) return false;

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
			if (IsInsideGround) return false;
			if (CellPhysics.Overlap(
				YayaConst.MASK_MAP,
				up ? Rect.Shift(0, ClimbSpeedY) : Rect,
				this,
				OperationMode.TriggerOnly,
				YayaConst.CLIMB_TAG
			)) {
				return true;
			}
			if (CellPhysics.Overlap(
				YayaConst.MASK_MAP,
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
				!SlideAvailable || IsGrounded || IsClimbing || IsDashing || IsGrabingTop || IsGrabingSide ||
				InWater || IsSquating || IsPounding ||
				Game.GlobalFrame < LastJumpFrame + SLIDE_JUMP_CANCEL ||
				IntendedX == 0 || VelocityY > -SlideDropSpeed
			) return false;
			var rect = new RectInt(
				IntendedX > 0 ? Hitbox.xMax : Hitbox.xMin - 1,
				Hitbox.y + Hitbox.height / 2,
				1, 1
			);
			if (SlideOnAnyBlock) {
				int count = CellPhysics.OverlapAll(c_SlideCheck, YayaConst.MASK_MAP, rect, this, OperationMode.ColliderOnly);
				for (int i = 0; i < count; i++) {
					var hit = c_SlideCheck[i];
					if (hit.Tag == YayaConst.NO_SLIDE_TAG) continue;
					if (hit.Tag == YayaConst.GRAB_TOP_TAG) continue;
					if (hit.Tag == YayaConst.GRAB_SIDE_TAG) continue;
					return true;
				}
				return false;
			} else {
				return CellPhysics.Overlap(
					YayaConst.MASK_MAP, rect, this, OperationMode.ColliderOnly, YayaConst.SLIDE_TAG
				);
			}
		}


		private bool GrabTopCheck (out int grabingY) {
			grabingY = 0;
			if (
				!GrabTopAvailable || IsInsideGround || IsGrounded || IsClimbing || IsDashing ||
				InWater || IsSquating || IsGrabFliping
			) return false;
			if (Game.GlobalFrame < LastGrabTopDropFrame + GRAB_DROP_CANCEL) return false;
			var rect = new RectInt(
				Hitbox.xMin, Hitbox.yMin + Hitbox.height / 2,
				Hitbox.width, Hitbox.height / 2 + GRAB_TOP_CHECK_GAP
			);
			if (CellPhysics.Overlap(
				YayaConst.MASK_MAP, rect, out var hit, this, OperationMode.ColliderOnly, YayaConst.GRAB_TOP_TAG
			)) {
				grabingY = hit.Rect.yMin - GrabTopHeight;
				return true;
			}
			return false;
		}


		private bool GrabSideCheck (out bool allowMoveUp) {
			allowMoveUp = false;
			if (
				!GrabSideAvailable || IsInsideGround || IsGrounded || IsClimbing || IsDashing ||
				InWater || IsSquating || IsGrabFliping ||
				Game.GlobalFrame < LastJumpFrame + GRAB_JUMP_CANCEL
			) return false;
			if (!IsGrabingSide && VelocityY > GrabMoveSpeedY / 2) return false;
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
			bool allowGrab = CellPhysics.Overlap(
				YayaConst.MASK_MAP, rectD, this, OperationMode.ColliderOnly, YayaConst.GRAB_SIDE_TAG
			) && CellPhysics.Overlap(
				YayaConst.MASK_MAP, rectU, this, OperationMode.ColliderOnly, YayaConst.GRAB_SIDE_TAG
			);
			if (allowGrab) {
				allowMoveUp = CellPhysics.Overlap(
					YayaConst.MASK_MAP, rectU.Shift(0, rectU.height), this, OperationMode.ColliderOnly, YayaConst.GRAB_SIDE_TAG
				);
			}
			return allowGrab;
		}


		private bool JumpThoughOnewayCheck () {
			var rect = new RectInt(Hitbox.xMin, Hitbox.yMin + 4 - Const.CEL / 4, Hitbox.width, Const.CEL / 4);
			if (CellPhysics.Overlap(YayaConst.MASK_MAP, rect, this)) return false;
			int count = CellPhysics.OverlapAll(
				c_GroundThoughCheck,
				YayaConst.MASK_MAP, rect, this,
				OperationMode.TriggerOnly, Const.ONEWAY_UP_TAG
			);
			for (int i = 0; i < count; i++) {
				var hit = c_GroundThoughCheck[i];
				if (hit.Rect.yMax <= Hitbox.y + 16) return true;
			}
			return false;
		}


		private bool GrabFlipThoughDownCheck () {
			int count = CellPhysics.OverlapAll(
				c_GroundThoughCheck,
				YayaConst.MASK_MAP,
				new RectInt(Hitbox.xMin + Hitbox.width / 2, Hitbox.yMin + 4 - Const.CEL / 4, 1, Const.CEL / 4),
				this, OperationMode.ColliderOnly, YayaConst.GRAB_TOP_TAG
			);
			for (int i = 0; i < count; i++) {
				var hit = c_GroundThoughCheck[i];
				if (hit.Rect.yMax <= Hitbox.y + 16) return true;
			}
			return false;
		}


		private bool RushEnvironmentCheck () =>
			(RushWhenSquat || !IsSquating) &&
			(RushInWater || !InWater) &&
			(RushInAir || IsGrounded) &&
			(RushWhenClimb || !IsClimbing);


		#endregion




	}
}