using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


public enum CharacterMovementState {
	Idle = 0,
	Walk, Run, JumpUp, JumpDown,
	SwimIdle, SwimMove, SquatIdle, SquatMove,
	Dash, Rush, Crash, Pound, Climb, Fly, Slide, GrabTop, GrabSide, GrabFlip,
}


public partial class CharacterMovement {




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
	public Character TargetCharacter { get; init; }
	public Int2 LastMoveDirection { get; private set; } = default;
	public int IntendedX { get; private set; } = 0;
	public int IntendedY { get; private set; } = 0;
	public int CurrentJumpCount { get; private set; } = 0;
	public bool FacingRight {
		get => TargetCharacter.LockFacingOnAttack && TargetCharacter.IsAttacking ? TargetCharacter.AttackStartFacingRight : BasicFacingRight;
		set => BasicFacingRight = value;
	}
	public bool BasicFacingRight { get; private set; } = true;
	public bool FacingFront { get; private set; } = true;
	public int FacingSign => FacingRight ? 1 : -1;
	public virtual int FinalCharacterHeight => MovementHeight;
	public virtual bool SpinOnGroundPound => false;

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
	public CharacterMovementState MovementState { get; set; } = CharacterMovementState.Idle;
	public bool ReadyForRun => RunningAccumulateFrame >= WalkToRunAccumulation;
	public bool IsGrabFlipping => IsGrabFlippingUp || IsGrabFlippingDown;
	public bool IsGrabFlippingUp => Game.GlobalFrame < LastGrabFlipUpFrame + Util.Max(GrabFlipThroughDuration, 1);
	public bool IsGrabFlippingDown => Game.GlobalFrame < LastGrabFlipDownFrame + Util.Max(GrabFlipThroughDuration, 1);
	public bool IsMoving => IntendedX != 0;
	public bool IsWalking => IntendedX != 0 && !ReadyForRun;
	public bool IsRunning => IntendedX != 0 && ReadyForRun;
	public bool IsRolling { get; private set; } = false;
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

	// Short
	private IRect Rect => TargetCharacter.Rect;
	private int X { get => TargetCharacter.X; set => TargetCharacter.X = value; }
	private int Y { get => TargetCharacter.Y; set => TargetCharacter.Y = value; }
	private int Width { get => TargetCharacter.Width; set => TargetCharacter.Width = value; }
	private int Height { get => TargetCharacter.Height; set => TargetCharacter.Height = value; }
	private int OffsetX { get => TargetCharacter.OffsetX; set => TargetCharacter.OffsetX = value; }
	private int OffsetY { get => TargetCharacter.OffsetY; set => TargetCharacter.OffsetY = value; }
	private int VelocityX { get => TargetCharacter.VelocityX; set => TargetCharacter.VelocityX = value; }
	private int VelocityY { get => TargetCharacter.VelocityY; set => TargetCharacter.VelocityY = value; }
	private int GravityScale { get => TargetCharacter.GravityScale; set => TargetCharacter.GravityScale = value; }
	private bool IsInsideGround => TargetCharacter.IsInsideGround;
	private bool InWater => TargetCharacter.InWater;
	private bool IsGrounded => TargetCharacter.IsGrounded;
	private bool OnSlippy => TargetCharacter.OnSlippy;
	private int CollisionMask => TargetCharacter.CollisionMask;

	// Data
	private static readonly Dictionary<int, CharacterMovementConfig> ConfigPool_Movement = new();
	private static int MovementConfigGlobalVersion = -1;
	private int LocalMovementConfigVersion = int.MinValue;
	private IRect Hitbox = default;
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


	[OnGameInitialize(-128)]
	internal static void OnGameInitializeMovement () {
		ReloadAllCharacterMovementConfigFromFile();
	}


	public CharacterMovement (Character character) => TargetCharacter = character;


	public virtual void OnCharacterActivated () {
		Width = MovementWidth;
		Height = MovementHeight;
		OffsetX = -MovementWidth / 2;
		OffsetY = 0;
		IsFlying = false;
		Hitbox = new IRect(X, Y, MovementWidth, MovementHeight);
		RequireJumpFrame = int.MinValue;
	}


	public virtual void PhysicsUpdateGamePlay () {
		Update_Cache();
		Update_GrabFlip();
		Update_ResetJumpCount();
		Update_Jump();
		Update_Dash();
		MovementState = GetCurrentMovementState();
		if (!IsInsideGround) {
			// General
			Update_VelocityX();
			Update_VelocityY();
		} else {
			// Inside Ground
			VelocityX = IntendedX * WalkSpeed;
			VelocityY = VelocityY.MoveTowards(0, 2);
			if (IntendedJump) {
				VelocityY = WalkSpeed;
				TargetCharacter.Bounce();
			}
		}
		Update_ClipCorrect();
	}


	public virtual void PhysicsUpdateLater () {
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


	private void Update_Cache () {

		int frame = Game.GlobalFrame;
		bool requirePutoutFire = false;

		// Ground
		if (IsGrounded) LastGroundingFrame = frame;
		if (!PrevGrounded && IsGrounded) LastGroundFrame = frame;
		PrevGrounded = IsGrounded;

		// Slip
		if (OnSlippy && IsGrounded && !IsCrashing && IsRunning && !IsSquatting) {
			if (LastSlippyMoveStartFrame < 0) {
				LastSlippyMoveStartFrame = Game.GlobalFrame;
			}
		} else {
			LastSlippyMoveStartFrame = int.MinValue;
		}

		// Crash
		if (CrashAvailable) {
			if (
				CrashWhenSlippy &&
				((LastSlippyMoveStartFrame >= 0 && frame > LastSlippyMoveStartFrame + CrashRunDurationRequire) ||
				(OnSlippy && IsGrounded && IsRushing))
			) {
				LastCrashFrame = frame;
				IsRushing = false;
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
		if (IsRushing && RushPutoutFire) requirePutoutFire = true;

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
		if (IsDashing && DashPutoutFire) requirePutoutFire = true;

		// Squat
		bool squatting =
			SquatAvailable && IsGrounded && !IsClimbing && !IsInsideGround && !IsCrashing &&
			((!IsDashing && !IsRushing && IntendedY < 0) || ForceSquatCheck());
		if (!IsSquatting && squatting) LastSquatFrame = frame;
		if (squatting) LastSquattingFrame = frame;
		IsSquatting = squatting;

		// Pound
		bool prevPounding = IsPounding;
		IsPounding =
			PoundAvailable && !IsGrounded && !IsGrabbingSide && !IsGrabbingTop && !IsCrashing &&
			!IsClimbing && !InWater && !IsDashing && !IsRushing && !IsInsideGround &&
			(IsPounding ? IntendedY < 0 : IntendedPound);
		if (IsPounding) LastPoundingFrame = frame;
		if (prevPounding && !IsPounding && IsGrounded && PoundPutoutFire) {
			requirePutoutFire = true;
		}

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
			Height = FinalCharacterHeight * GrabTopHeightAmount / 1000;
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

		// Roll
		IsRolling =
			!IsPounding && !IsFlying && !IsRushing && (
				(FirstJumpWithRoll && CurrentJumpCount == 1) ||
				(SubsequentJumpWithRoll && CurrentJumpCount > 1) ||
				(DashWithRoll && IsDashing)
			);

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
		FacingFront = !IsClimbing && (!TargetCharacter.Teleporting || TargetCharacter.TeleportEndFrame > 0);

		// Physics
		int width = InWater ? SwimWidth : MovementWidth;
		int height = GetCurrentHeight();
		Hitbox = new IRect(
			X - width / 2,
			Y,
			width,
			Hitbox.height.MoveTowards(height, Const.CEL / 8, Const.CEL)
		);
		Width = Hitbox.width;
		Height = Hitbox.height;
		OffsetX = -width / 2;
		OffsetY = 0;

		// Putout Fire
		if (requirePutoutFire) {
			Fire.PutoutFire(Rect);
		}

	}


	private void Update_GrabFlip () {

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
			IntendedDash && IsGrounded && GrabFlipThroughDownAvailable &&
			GrabFlipCheck(false)
		) {
			LastGrabFlipDownFrame = Game.GlobalFrame;
			LastGrabCancelFrame = Game.GlobalFrame;
			LastDashFrame = int.MinValue;
			IsDashing = false;
		}

	}


	private void Update_ResetJumpCount () {

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


	private void Update_Jump () {

		int frame = Game.GlobalFrame;

		bool movementAllowJump = !IsSquatting && !IsGrabbingTop && !IsInsideGround && !IsRushing && !IsGrabFlipping && !IsCrashing;

		// Perform Jump/Fly
		if (movementAllowJump && (!IsClimbing || JumpWhenClimbAvailable)) {
			// Jump
			if (CurrentJumpCount < JumpCount) {
				// Jump
				if (IntendedJump || frame < RequireJumpFrame + JUMP_REQUIRE_TOLERANCE) {
					// Perform Jump
					CurrentJumpCount++;
					VelocityY = Util.Max(InWater ? SwimJumpSpeed : JumpSpeed, VelocityY);
					if (InWater) TargetCharacter.Bounce();
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
							VelocityY = Util.Max(FlyRiseSpeed, VelocityY);
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


	private void Update_Dash () {

		if (!IntendedDash || !IsGrounded || IsGrabFlipping) return;

		// Jump Though Oneway
		if (JumpDownThoughOneway && JumpThoughOnewayCheck()) {
			TargetCharacter.PerformMove(0, -Const.HALF, ignoreOneway: true);
			VelocityY = 0;
			return;
		}

		// Dash
		if (!DashAvailable || Game.GlobalFrame <= LastDashFrame + DashDuration + DashCooldown) return;
		LastDashFrame = Game.GlobalFrame;
		IsDashing = true;
		VelocityY = 0;
	}


	private void Update_VelocityX () {

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
				if (GlideOnFlying) {
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

		// Slip
		if (OnSlippy && SlipAvailable) {
			acc = SlipAcceleration;
			dcc = SlipDeceleration;
		}

		// Speed Lose on Attack
		if (TargetCharacter.IsAttacking && speed != 0) {
			int loseRate = TargetCharacter.CurrentSpeedLoseOnAttack;
			if (loseRate != 1000) {
				speed = speed * loseRate / 1000;
			}
		}

		// Push
		if (PushAvailable && !IsCrashing && IntendedX != 0 && speed != 0) {
			var hits = Physics.OverlapAll(
			PhysicsMask.ENVIRONMENT,
			Rect.Shrink(0, 0, 4, 4).EdgeOutside(IntendedX < 0 ? Direction4.Left : Direction4.Right),
			out int count, TargetCharacter
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


	private void Update_VelocityY () {

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
				VelocityY = Util.Max(VelocityY, -FlyFallSpeed);
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
				VelocityY = (MovementHeight + Const.CEL + 12) / Util.Max(GrabFlipThroughDuration, 1) + 1;
				if (!IsGrabFlippingUp) VelocityY *= -1;
				break;

		}
	}


	private void Update_ClipCorrect () {

		if (IsGrounded || VelocityY <= 0) return;

		var rect = Rect;
		int size = VelocityY + 1;

		// Clip Left
		if (CheckCorrect(
			new IRect(rect.xMin, rect.yMax, 1, size),
			new IRect(rect.xMin + CLIP_CORRECT_TOLERANCE, rect.yMax, rect.width - CLIP_CORRECT_TOLERANCE, size),
			out var hitRect
		)) {
			TargetCharacter.PerformMove(hitRect.xMax - rect.xMin, 0);
		}

		// Clip Right
		if (CheckCorrect(
			new IRect(rect.xMin + Width, rect.yMax, 1, size),
			new IRect(rect.xMin, rect.yMax, rect.width - CLIP_CORRECT_TOLERANCE, size),
			out hitRect
		)) {
			TargetCharacter.PerformMove(hitRect.xMin - rect.xMax, 0);
		}

		// Func
		bool CheckCorrect (IRect trueRect, IRect falseRect, out IRect hitRect) {
			if (
				Physics.Overlap(CollisionMask, trueRect, out var hit, TargetCharacter) &&
				!Physics.Overlap(CollisionMask, falseRect, TargetCharacter)
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


	// Config
	public static void ReloadMovementConfigFromFile (string characterName) {
		int id = characterName.AngeHash();
		string path = Util.CombinePaths(Universe.BuiltIn.CharacterMovementConfigRoot, $"{characterName}.json");
		var config = JsonUtil.LoadOrCreateJsonFromPath<CharacterMovementConfig>(path);
		ConfigPool_Movement[id] = config;
		MovementConfigGlobalVersion++;
	}


	public static void ReloadAllCharacterMovementConfigFromFile () {
		MovementConfigGlobalVersion++;
		ConfigPool_Movement.Clear();
		string movementRoot = Universe.BuiltIn.CharacterMovementConfigRoot;
		foreach (var type in typeof(Character).AllChildClass()) {
			int typeID = type.AngeHash();
			string name = type.Name;
			// Movement
			string path = Util.CombinePaths(movementRoot, $"{name}.json");
			var config = JsonUtil.LoadJsonFromPath<CharacterMovementConfig>(path);
			// Create Default Config
			if (config == null) {
				config = new CharacterMovementConfig();
				JsonUtil.SaveJsonToPath(config, path, prettyPrint: true);
			}
			// Add to Pool
			ConfigPool_Movement.Add(typeID, config);
		}
	}


	public void SyncConfigFromPool () {
		if (LocalMovementConfigVersion == MovementConfigGlobalVersion) return;
		LocalMovementConfigVersion = MovementConfigGlobalVersion;
		if (ConfigPool_Movement.TryGetValue(TargetCharacter.TypeID, out var mConfig)) {
			mConfig.LoadToCharacter(TargetCharacter);
		}
	}


	// Movement Logic
	public virtual void Move (Direction3 x, Direction3 y) => MoveLogic((int)x, (int)y);


	public virtual void Stop () {
		MoveLogic(0, 0);
		VelocityX = 0;
	}


	public virtual void HoldJump (bool holding) => HoldingJump = holding;


	public virtual void Jump () {
		IntendedJump = true;
		RequireJumpFrame = Game.GlobalFrame;
		if (TargetCharacter.CancelAttackOnJump) TargetCharacter.CancelAttack();
	}


	public virtual void Dash () => IntendedDash = true;


	public virtual void Pound () => IntendedPound = true;


	public virtual void Rush () => IntendedRush = true;


	public virtual void Crash () => LastCrashFrame = Game.GlobalFrame;


	public void LockFacingRight (bool facingRight, int duration = 1) {
		LockedFacingFrame = Game.GlobalFrame + duration;
		LockedFacingRight = facingRight;
	}


	public void ClearRunningAccumulate () => RunningAccumulateFrame = -1;


	#endregion




	#region --- LGC ---


	// Move
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
		int growingHeight = FinalCharacterHeight;
		if (IsSquatting) return growingHeight * SquatHeightAmount / 1000;
		if (IsRolling) return growingHeight * SquatHeightAmount / 1000;
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
		!IsGrounded && !InWater && !IsClimbing ? (VelocityY > 0 ? CharacterMovementState.JumpUp : CharacterMovementState.JumpDown) :
		IsMoving && (ReadyForRun ? RunSpeed : WalkSpeed) != 0 ? (ReadyForRun && !IsInsideGround ? CharacterMovementState.Run : CharacterMovementState.Walk) :
		CharacterMovementState.Idle;


	// Check
	private bool ForceSquatCheck () {

		if (IsInsideGround) return false;

		int GAP = Width / 8;
		var rect = new IRect(
			X + OffsetX + GAP,
			Y + OffsetY + MovementHeight / 2,
			Width - GAP * 2,
			MovementHeight / 2
		);

		// Oneway Check
		if ((IsSquatting || IsDashing) && !Physics.RoomCheckOneway(
			PhysicsMask.LEVEL, rect, TargetCharacter, Direction4.Up, false
		)) return true;

		// Overlap Check
		return Physics.Overlap(PhysicsMask.MAP, rect, null);
	}


	private bool ClimbCheck (bool up = false) {
		if (IsInsideGround) return false;
		if (Physics.Overlap(
			PhysicsMask.MAP,
			up ? Rect.Shift(0, ClimbSpeedY) : Rect,
			TargetCharacter,
			OperationMode.TriggerOnly,
			Tag.Climb
		)) {
			return true;
		}
		if (Physics.Overlap(
			PhysicsMask.MAP,
			up ? Rect.Shift(0, ClimbSpeedY) : Rect,
			out var info,
			TargetCharacter,
			OperationMode.TriggerOnly,
			Tag.ClimbStable
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
		var rect = new IRect(
			IntendedX > 0 ? Hitbox.xMax : Hitbox.xMin - 1,
			Hitbox.y + Hitbox.height / 2,
			1, 1
		);
		if (SlideOnAnyBlock) {
			var hits = Physics.OverlapAll(PhysicsMask.MAP, rect, out int count, TargetCharacter, OperationMode.ColliderOnly);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Tag.HasAny(Tag.NoSlide | Tag.GrabTop | Tag.GrabSide)) continue;
				return true;
			}
			return false;
		} else {
			return Physics.Overlap(
				PhysicsMask.MAP, rect, TargetCharacter, OperationMode.ColliderOnly, Tag.Slide
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
		var rect = new IRect(
			Hitbox.xMin,
			Y + height / 2,
			Hitbox.width,
			height / 2 + GRAB_TOP_CHECK_GAP
		);
		if (Physics.Overlap(
			PhysicsMask.MAP, rect, out var hit, TargetCharacter,
			OperationMode.ColliderOnly, Tag.GrabTop
		) || Physics.Overlap(
			PhysicsMask.MAP, rect, out hit, TargetCharacter,
			OperationMode.ColliderOnly, Tag.Grab
		)) {
			grabbingY = hit.Rect.yMin - (FinalCharacterHeight * GrabTopHeightAmount / 1000);
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
		var rectD = new IRect(
			FacingRight ? Hitbox.xMax : Hitbox.xMin - 1,
			Hitbox.yMin + Hitbox.height / 4,
			1,
			Hitbox.height / 4
		);
		var rectU = new IRect(
			FacingRight ? Hitbox.xMax : Hitbox.xMin - 1,
			Hitbox.yMax - Hitbox.height / 4,
			1,
			Hitbox.height / 4
		);
		bool allowGrab =
			(AllowCheck(rectD, Tag.GrabSide) || AllowCheck(rectD, Tag.Grab)) &&
			(AllowCheck(rectU, Tag.GrabSide) || AllowCheck(rectU, Tag.Grab));
		if (allowGrab) {
			allowMoveUp = Physics.Overlap(
				PhysicsMask.MAP, rectU.Shift(0, rectU.height), TargetCharacter, OperationMode.ColliderOnly, Tag.GrabSide
			) || Physics.Overlap(
				PhysicsMask.MAP, rectU.Shift(0, rectU.height), TargetCharacter, OperationMode.ColliderOnly, Tag.Grab
			);
		}
		return allowGrab;
		// Func
		bool AllowCheck (IRect rect, Tag tag) => Physics.Overlap(PhysicsMask.MAP, rect, TargetCharacter, OperationMode.ColliderOnly, tag);
	}


	private bool JumpThoughOnewayCheck () {
		var rect = new IRect(Hitbox.xMin, Hitbox.yMin + 4 - Const.CEL / 4, Hitbox.width, Const.CEL / 4);
		if (Physics.Overlap(PhysicsMask.MAP, rect, TargetCharacter)) return false;
		var hits = Physics.OverlapAll(
			PhysicsMask.MAP, rect, out int count, TargetCharacter,
			OperationMode.TriggerOnly, Tag.OnewayUp
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
			if (Physics.Overlap(
				PhysicsMask.MAP,
				new IRect(x, Y + (FinalCharacterHeight * GrabTopHeightAmount / 1000) + Const.CEL + Const.HALF, width, 1),
				TargetCharacter
			)) return false;
			return true;
		} else {
			// Down
			// No Block Below
			if (Physics.Overlap(
				PhysicsMask.MAP,
				new IRect(x, Y - Const.CEL - Const.HALF, width, 1),
				TargetCharacter
			)) return false;
			// Standing on Grab-Top Block
			var hits = Physics.OverlapAll(
				PhysicsMask.MAP,
				new IRect(x, Y + 4 - Const.CEL / 4, width, Const.CEL / 4), out int count,
				TargetCharacter, OperationMode.ColliderOnly, Tag.GrabTop
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
