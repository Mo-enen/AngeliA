using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace AngeliA;


public enum CharacterMovementState {
	Idle = 0,
	Walk, Run, JumpUp, JumpDown,
	SwimIdle, SwimMove, SquatIdle, SquatMove,
	Dash, Rush, Crash, Pound, Climb, Fly, Slide, GrabTop, GrabSide, GrabFlip,
}


[StructLayout(LayoutKind.Sequential)]
public class CharacterMovement (Rigidbody rig) {




	#region --- MET ---


	[PropGroup("Size")]
	public readonly FrameBasedInt MovementWidth = new(150);
	public readonly FrameBasedInt MovementHeight = new(384); // Height when Character is 160cm

	[PropGroup("Walk")]
	public readonly FrameBasedBool WalkAvailable = new(true);
	[PropVisibility(nameof(WalkAvailable))] public readonly FrameBasedInt WalkSpeed = new(20);
	[PropVisibility(nameof(WalkAvailable))] public readonly FrameBasedInt WalkAcceleration = new(3);
	[PropVisibility(nameof(WalkAvailable))] public readonly FrameBasedInt WalkBrakeAcceleration = new(30);
	[PropVisibility(nameof(WalkAvailable))] public readonly FrameBasedInt WalkDeceleration = new(4);

	[PropGroup("Run")]
	public readonly FrameBasedBool RunAvailable = new(true);
	[PropVisibility(nameof(RunAvailable))] public readonly FrameBasedInt RunSpeed = new(32);
	[PropVisibility(nameof(RunAvailable))] public readonly FrameBasedInt RunAcceleration = new(3);
	[PropVisibility(nameof(RunAvailable))] public readonly FrameBasedInt RunBrakeAcceleration = new(30);
	[PropVisibility(nameof(RunAvailable))] public readonly FrameBasedInt RunDeceleration = new(4);

	[PropGroup("Jump")]
	public readonly FrameBasedInt JumpCount = new(2);
	[PropVisibility(nameof(JumpCount), CompareMode.GreaterThan, 0)] public readonly FrameBasedInt JumpSpeed = new(73);
	[PropVisibility(nameof(JumpCount), CompareMode.GreaterThan, 0)] public readonly FrameBasedInt JumpReleaseSpeedRate = new(700);
	[PropVisibility(nameof(JumpCount), CompareMode.GreaterThan, 0)] public readonly FrameBasedInt JumpRiseGravityRate = new(600);
	[PropVisibility(nameof(JumpCount), CompareMode.GreaterThan, 0)] public readonly FrameBasedBool FirstJumpWithRoll = new(false);
	[PropVisibility(nameof(JumpCount), CompareMode.GreaterThan, 1)] public readonly FrameBasedBool SubsequentJumpWithRoll = new(true);
	[PropVisibility(nameof(JumpCount), CompareMode.GreaterThan, 0)] public readonly FrameBasedBool JumpBreakRush = new(false);
	[PropVisibility(nameof(JumpCount), CompareMode.GreaterThan, 0)] public readonly FrameBasedBool JumpBreakDash = new(true);
	public readonly FrameBasedBool JumpDownThoughOneway = new(false);

	[PropGroup("Squat")]
	public readonly FrameBasedBool SquatAvailable = new(true);
	[PropVisibility(nameof(SquatAvailable))] public readonly FrameBasedInt SquatHeightAmount = new(521);
	[PropVisibility(nameof(SquatAvailable))] public readonly FrameBasedInt SquatMoveSpeed = new(14);
	[PropVisibility(nameof(SquatAvailable))] public readonly FrameBasedInt SquatAcceleration = new(48);
	[PropVisibility(nameof(SquatAvailable))] public readonly FrameBasedInt SquatDeceleration = new(48);

	[PropGroup("Dash")]
	public readonly FrameBasedBool DashAvailable = new(true);
	[PropVisibility(nameof(DashAvailable))] public readonly FrameBasedInt DashHeightAmount = new(521);
	[PropVisibility(nameof(DashAvailable))] public readonly FrameBasedBool DashWithRoll = new(false);
	[PropVisibility(nameof(DashAvailable))] public readonly FrameBasedBool DashPutoutFire = new(true);
	[PropVisibility(nameof(DashAvailable))] public readonly FrameBasedInt DashSpeed = new(42);
	[PropVisibility(nameof(DashAvailable))] public readonly FrameBasedInt DashDuration = new(20);
	[PropVisibility(nameof(DashAvailable))] public readonly FrameBasedInt DashCooldown = new(4);
	[PropVisibility(nameof(DashAvailable))] public readonly FrameBasedInt DashAcceleration = new(24);
	[PropVisibility(nameof(DashAvailable))] public readonly FrameBasedInt DashCancelLoseRate = new(300);

	[PropGroup("Rush")]
	public readonly FrameBasedBool RushAvailable = new(true);
	[PropVisibility(nameof(RushAvailable))] public readonly FrameBasedInt RushHeightAmount = new(1000);
	[PropVisibility(nameof(RushAvailable))] public readonly FrameBasedBool RushInAir = new(false);
	[PropVisibility(nameof(RushAvailable))] public readonly FrameBasedBool RushInWater = new(true);
	[PropVisibility(nameof(RushAvailable))] public readonly FrameBasedBool RushWhenClimb = new(false);
	[PropVisibility(nameof(RushAvailable))] public readonly FrameBasedBool RushWhenSquat = new(false);
	[PropVisibility(nameof(RushAvailable))] public readonly FrameBasedBool RushPutoutFire = new(true);
	[PropVisibility(nameof(RushAvailable))] public readonly FrameBasedInt RushSpeed = new(72);
	[PropVisibility(nameof(RushAvailable))] public readonly FrameBasedInt RushStopSpeed = new(8);
	[PropVisibility(nameof(RushAvailable))] public readonly FrameBasedInt RushDuration = new(8);
	[PropVisibility(nameof(RushAvailable))] public readonly FrameBasedInt RushStiff = new(10);
	[PropVisibility(nameof(RushAvailable))] public readonly FrameBasedInt RushCooldown = new(2);
	[PropVisibility(nameof(RushAvailable))] public readonly FrameBasedInt RushAcceleration = new(12);
	[PropVisibility(nameof(RushAvailable))] public readonly FrameBasedInt RushDeceleration = new(4);

	[PropGroup("Slip")]
	public readonly FrameBasedBool SlipAvailable = new(true);
	[PropVisibility(nameof(SlipAvailable))] public readonly FrameBasedInt SlipAcceleration = new(2);
	[PropVisibility(nameof(SlipAvailable))] public readonly FrameBasedInt SlipDeceleration = new(1);

	[PropGroup("Pound")]
	public readonly FrameBasedBool PoundAvailable = new(true);
	[PropVisibility(nameof(PoundAvailable))] public readonly FrameBasedBool PoundPutoutFire = new(true);
	[PropVisibility(nameof(PoundAvailable))] public readonly FrameBasedInt PoundSpeed = new(96);

	[PropGroup("Swim")]
	public readonly FrameBasedBool SwimAvailable = new(true);
	public readonly FrameBasedInt InWaterSpeedRate = new(500);
	[PropVisibility(nameof(SwimAvailable))] public readonly FrameBasedInt SwimWidthAmount = new(1333);
	[PropVisibility(nameof(SwimAvailable))] public readonly FrameBasedInt SwimHeightAmount = new(1000);
	[PropVisibility(nameof(SwimAvailable))] public readonly FrameBasedInt SwimSpeed = new(42);
	[PropVisibility(nameof(SwimAvailable))] public readonly FrameBasedInt SwimJumpSpeed = new(128);
	[PropVisibility(nameof(SwimAvailable))] public readonly FrameBasedInt SwimAcceleration = new(4);
	[PropVisibility(nameof(SwimAvailable))] public readonly FrameBasedInt SwimDeceleration = new(4);

	[PropGroup("Climb")]
	public readonly FrameBasedBool ClimbAvailable = new(true);
	[PropVisibility(nameof(ClimbAvailable))] public readonly FrameBasedBool AllowJumpWhenClimbing = new(true);
	[PropVisibility(nameof(ClimbAvailable))] public readonly FrameBasedInt ClimbSpeedX = new(12);
	[PropVisibility(nameof(ClimbAvailable))] public readonly FrameBasedInt ClimbSpeedY = new(18);

	[PropGroup("Fly")]
	public readonly FrameBasedBool FlyAvailable = new(true);
	[PropVisibility(nameof(FlyAvailable))] public readonly FrameBasedInt FlyHeightAmount = new(521);
	[PropVisibility(nameof(FlyAvailable))] public readonly FrameBasedBool GlideOnFlying = new(false);
	[PropVisibility(nameof(FlyAvailable))] public readonly FrameBasedInt FlyCooldown = new(24);
	[PropVisibility(nameof(FlyAvailable))] public readonly FrameBasedInt FlyRiseSpeed = new(96);
	[PropVisibility(nameof(FlyAvailable))] public readonly FrameBasedInt FlyGravityRiseRate = new(800);
	[PropVisibility(nameof(FlyAvailable))] public readonly FrameBasedInt FlyGravityFallRate = new(200);
	[PropVisibility(nameof(FlyAvailable))] public readonly FrameBasedInt FlyFallSpeed = new(16);
	[PropVisibility(nameof(FlyAvailable))] public readonly FrameBasedInt FlyMoveSpeed = new(32);
	[PropVisibility(nameof(FlyAvailable))] public readonly FrameBasedInt FlyAcceleration = new(2);
	[PropVisibility(nameof(FlyAvailable))] public readonly FrameBasedInt FlyDeceleration = new(1);

	[PropGroup("Slide")]
	public readonly FrameBasedBool SlideAvailable = new(false);
	[PropVisibility(nameof(SlideAvailable))] public readonly FrameBasedBool SlideOnAnyBlock = new(false);
	[PropVisibility(nameof(SlideAvailable))] public readonly FrameBasedBool ResetJumpCountWhenSlide = new(true);
	[PropVisibility(nameof(SlideAvailable))] public readonly FrameBasedInt SlideJumpKickSpeed = new(56);
	[PropVisibility(nameof(SlideAvailable))] public readonly FrameBasedInt SlideDropSpeed = new(4);

	[PropGroup("Grab")]
	public readonly FrameBasedBool GrabTopAvailable = new(true);
	public readonly FrameBasedBool GrabSideAvailable = new(true);
	[PropVisibility(nameof(GrabTopAvailable))] public readonly FrameBasedInt GrabTopHeightAmount = new(947);
	[PropVisibility(nameof(GrabSideAvailable))] public readonly FrameBasedInt GrabSideHeightAmount = new(947);
	[PropVisibility(nameof(GrabTopAvailable), CompareMode.Or, nameof(GrabSideAvailable))] public readonly FrameBasedBool ResetJumpCountWhenGrab = new(true);
	[PropVisibility(nameof(GrabTopAvailable))] public readonly FrameBasedBool GrabFlipThroughDownAvailable = new(true);
	[PropVisibility(nameof(GrabTopAvailable))] public readonly FrameBasedBool GrabFlipThroughUpAvailable = new(true);
	[PropVisibility(nameof(GrabTopAvailable))] public readonly FrameBasedInt GrabFlipThroughDuration = new(18);
	[PropVisibility(nameof(GrabTopAvailable))] public readonly FrameBasedInt GrabMoveSpeedX = new(24);
	[PropVisibility(nameof(GrabSideAvailable))] public readonly FrameBasedInt GrabMoveSpeedY = new(24);
	[PropVisibility(nameof(GrabSideAvailable))] public readonly FrameBasedInt GrabSideJumpKickSpeed = new(56);

	[PropGroup("Crash")]
	public readonly FrameBasedBool CrashAvailable = new(true);
	[PropVisibility(nameof(CrashAvailable))] public readonly FrameBasedBool CrashWhenSlippy = new(true);
	[PropVisibility(nameof(CrashAvailable))] public readonly FrameBasedInt CrashDuration = new(30);
	[PropVisibility(nameof(CrashAvailable))] public readonly FrameBasedInt CrashRunDurationRequire = new(42);
	[PropVisibility(nameof(CrashAvailable))] public readonly FrameBasedInt CrashDeceleration = new(1);

	[PropGroup("Push")]
	public readonly FrameBasedBool PushAvailable = new(true);
	[PropVisibility(nameof(PushAvailable))] public readonly FrameBasedInt PushSpeed = new(10);


	#endregion




	#region --- VAR ---


	// Const
	private const int JUMP_TOLERANCE = 4;
	private const int JUMP_REQUIRE_TOLERANCE = 6;
	private const int CLIMB_CORRECT_DELTA = 36;
	private const int SLIDE_JUMP_CANCEL = 2;
	private const int SLIDE_GROUND_TOLERANCE = 12;
	private const int GRAB_GROUND_TOLERANCE = 12;
	private const int CLIMB_GROUND_TOLERANCE = 12;
	private const int GRAB_JUMP_CANCEL = 2;
	private const int GRAB_DROP_CANCEL = 12;
	private const int GRAB_TOP_CHECK_GAP = 128;
	private const int CLIP_CORRECT_TOLERANCE = Const.CEL / 4;

	// Api
	public readonly Rigidbody Target = rig;
	public readonly Character TargetCharacter = rig as Character;
	public Int2 LastMoveDirection { get; private set; } = default;
	public int IntendedX { get; private set; } = 0;
	public int IntendedY { get; private set; } = 0;
	public int CurrentJumpCount { get; set; } = 0;
	public int SpeedRateX { get; private set; } = 1000;
	public bool FacingRight { get; set; } = true;
	public bool FacingFront { get; set; } = true;
	public bool ShouldRun { get; private set; } = true;
	public virtual bool SyncFromConfigFile => true;

	// Frame Cache
	public int LastGroundFrame { get; private set; } = int.MinValue;
	public int LastGroundingFrame { get; private set; } = int.MinValue;
	public int LastStartMoveFrame { get; private set; } = int.MinValue;
	public int LastEndMoveFrame { get; private set; } = int.MinValue;
	public int LastJumpFrame { get; private set; } = int.MinValue;
	public int LastClimbFrame { get; private set; } = int.MinValue;
	public int LastDashFrame { get; private set; } = int.MinValue;
	public int LastRushStartFrame { get; private set; } = int.MinValue;
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
	public bool IsGrabFlipping => IsGrabFlippingUp || IsGrabFlippingDown;
	public bool IsGrabFlippingUp => Game.GlobalFrame < LastGrabFlipUpFrame + Util.Max(GrabFlipThroughDuration, 1);
	public bool IsGrabFlippingDown => Game.GlobalFrame < LastGrabFlipDownFrame + Util.Max(GrabFlipThroughDuration, 1);
	public bool IsMoving => IntendedX != 0;
	public bool IsWalking => WalkAvailable && IntendedX != 0 && !ShouldRun;
	public bool IsRunning => RunAvailable && IntendedX != 0 && ShouldRun;
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
	protected IRect Rect => Target.Rect;
	protected int X { get => Target.X; set => Target.X = value; }
	protected int Y { get => Target.Y; set => Target.Y = value; }
	protected int Width { get => Target.Width; set => Target.Width = value; }
	protected int Height { get => Target.Height; set => Target.Height = value; }
	protected int OffsetX { get => Target.OffsetX; set => Target.OffsetX = value; }
	protected int OffsetY { get => Target.OffsetY; set => Target.OffsetY = value; }
	protected int VelocityX { get => Target.VelocityX; set => Target.VelocityX = value; }
	protected int VelocityY { get => Target.VelocityY; set => Target.VelocityY = value; }
	protected bool IsInsideGround => Target.IsInsideGround;
	protected bool InWater => Target.InWater;
	protected bool IsGrounded => Target.IsGrounded;
	protected int CollisionMask => Target.CollisionMask;

	// Data
	private static readonly Dictionary<int, List<(string name, int value)>> ConfigPool = [];
	private int LocalMovementConfigVersion = -2;
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
	private bool OnSlippy;
	private int? ClimbPositionCorrect = null;
	private int LockedFacingFrame = int.MinValue;
	private int RequireJumpFrame = int.MinValue;
	private int SpeedRateFrame = int.MinValue;


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-128)]
	internal static void InitializeConfigPool () {

		string movementRoot = Universe.BuiltIn.CharacterMovementConfigRoot;

		// Load Config from File
		foreach (var type in typeof(Character).AllChildClass()) {
			string name = type.AngeName();
			// Movement
			string path = Util.CombinePaths(movementRoot, $"{name}.txt");
			// Load List from Config File
			var list = new List<(string, int)>() { ("", 0) };
			bool loaded = FrameworkUtil.NameAndIntFile_to_List(list, path);
			if (!loaded) {
				Util.TextToFile("", path);
			}
			ConfigPool.Add(name.AngeHash(), list);
		}
		ConfigPool.TrimExcess();

		// Remove Files Not in Pool
		foreach (string path in Util.EnumerateFiles(movementRoot, true, AngePath.MOVEMENT_CONFIG_SEARCH_PATTERN)) {
			string name = Util.GetNameWithoutExtension(path);
			int id = name.AngeHash();
			if (!ConfigPool.ContainsKey(id)) {
				Util.DeleteFile(path);
			}
		}

	}


	public virtual void OnActivated () {

		Width = MovementWidth;
		Height = MovementHeight;
		OffsetX = -MovementWidth / 2;
		OffsetY = 0;
		IsFlying = false;
		Hitbox = new IRect(X, Y, MovementWidth, MovementHeight);
		RequireJumpFrame = int.MinValue;
		FacingRight = true;
		FacingFront = true;
		LastRushStartFrame = int.MinValue;
		LastDashFrame = int.MinValue;
		OnSlippy = false;

		// Sync Movement Config from Pool
		if (
			SyncFromConfigFile &&
			ConfigPool.TryGetValue(Target.TypeID, out var configList)
		) {
			int poolVersion = configList[0].value;
			if (LocalMovementConfigVersion != poolVersion) {
				LocalMovementConfigVersion = poolVersion;
				FrameworkUtil.List_to_FrameBasedFields(configList, this);
			}
		}
	}


	public virtual void PhysicsUpdateGamePlay () {
		if (Target == null) return;
		Update_Cache();
		Update_GrabFlip();
		Update_ResetJumpCount();
		Update_Jump();
		Update_Dash();
		MovementState = GetMovementState();
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
				TargetCharacter?.Bounce();
			}
		}
		Update_ClipCorrect();
	}


	public virtual void UpdateLater () {
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
		OnSlippy = !InWater && Physics.Overlap(
			CollisionMask, Rect.EdgeOutsideDown(1),
			TargetCharacter, OperationMode.ColliderAndTrigger, Tag.Slip
		);
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
				VelocityY = VelocityY * InWaterSpeedRate / 1000;
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
			frame > LastRushStartFrame + RushDuration + RushStiff + RushCooldown &&
			(RushWhenSquat || !IsSquatting) &&
			(RushInWater || !InWater) &&
			(RushInAir || IsGrounded) &&
			(RushWhenClimb || !IsClimbing)
		) {
			IsRushing = true;
			IsClimbing = false;
			LastRushStartFrame = frame;
			VelocityY = 0;
		}
		if (IsRushing && frame > LastRushStartFrame + RushDuration + RushStiff) {
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
			Height = TargetCharacter.FinalCharacterHeight * GrabTopHeightAmount / 1000;
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
		}
		if (FacingRight != oldFacingRight) {
			LastFacingChangeFrame = frame;
		}

		// Facing Front
		FacingFront = !IsClimbing;

		// Physics
		int growingHeight = TargetCharacter != null ? TargetCharacter.FinalCharacterHeight : MovementHeight;
		int width = InWater ? MovementWidth * SwimWidthAmount / 1000 : MovementWidth;
		int height =
			IsSquatting ? growingHeight * SquatHeightAmount / 1000 :
			IsRolling ? growingHeight * SquatHeightAmount / 1000 :
			IsDashing ? growingHeight * DashHeightAmount / 1000 :
			IsRushing ? growingHeight * RushHeightAmount / 1000 :
			InWater ? growingHeight * SwimHeightAmount / 1000 :
			IsFlying ? growingHeight * FlyHeightAmount / 1000 :
			IsGrabbingTop ? growingHeight * GrabTopHeightAmount / 1000 :
			IsGrabbingSide ? growingHeight * GrabSideHeightAmount / 1000 :
			growingHeight;
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
			IFire.PutoutFire(Rect.Expand(Const.HALF));
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
		if (frame > LastJumpFrame + 1) {
			if (IsGrounded || InWater) {
				CurrentJumpCount = 0;
				return;
			} else if (VelocityY < 0 && Target.PerformGroundCheck(Rect.Shift(0, VelocityY), out _)) {
				CurrentJumpCount = 0;
				LastGroundingFrame = frame;
				return;
			}
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

		bool movementAllowJump =
			!IsSquatting && !IsGrabbingTop && !IsInsideGround &&
			(JumpBreakRush || !IsRushing) &&
			(JumpBreakDash || !IsDashing) &&
			!IsGrabFlipping && !IsCrashing;

		// Perform Jump/Fly
		if (movementAllowJump && (!IsClimbing || AllowJumpWhenClimbing)) {
			// Jump
			if (CurrentJumpCount < JumpCount) {
				// Jump
				if (IntendedJump || frame < RequireJumpFrame + JUMP_REQUIRE_TOLERANCE) {
					// Perform Jump
					CurrentJumpCount++;
					VelocityY = Util.Max(InWater ? SwimJumpSpeed : JumpSpeed, VelocityY);
					if (InWater) {
						TargetCharacter?.Bounce();
					}
					if (IsGrabbingSide) {
						X += FacingRight ? -6 : 6;
					} else if (IsGrabbingTop) {
						VelocityY = 0;
						Y -= 3;
					}
					if (IsSliding) {
						VelocityX += FacingRight ? -SlideJumpKickSpeed : SlideJumpKickSpeed;
					} else if (IsGrabbingSide) {
						VelocityX += FacingRight ? -GrabSideJumpKickSpeed : GrabSideJumpKickSpeed;
					}
					if (JumpBreakRush) LastRushStartFrame = int.MinValue;
					if (JumpBreakDash) LastDashFrame = int.MinValue;
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
			CurrentJumpCount == 0 &&
			!IsGrounded && !InWater && !IsClimbing &&
			frame > LastGroundingFrame
		) {
			// Grow Jump Count
			if (frame > LastGroundingFrame + JUMP_TOLERANCE) {
				CurrentJumpCount++;
			}
		}

		// Jump Release
		if (movementAllowJump && PrevHoldingJump && !HoldingJump) {
			// Lose Speed if Raising
			if (!IsGrounded && CurrentJumpCount <= JumpCount && VelocityY > 0) {
				VelocityY = VelocityY * JumpReleaseSpeedRate / 1000;
			}
		}
	}


	private void Update_Dash () {

		if (!IntendedDash || !IsGrounded || IsGrabFlipping) return;

		// Jump Though Oneway
		if (JumpDownThoughOneway && JumpThoughOnewayCheck()) {
			Target.IgnoreOneway.True(2);
			Target.PerformMove(0, -Const.HALF);
			//Target.CancelIgnoreOneway();
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
				bool running = ShouldRun;
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
				speed = IntendedX * SquatMoveSpeed;
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
					Game.GlobalFrame > LastRushStartFrame + RushDuration ? 0 :
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
		if (Game.GlobalFrame <= SpeedRateFrame && SpeedRateX != 1000) {
			speed = speed * SpeedRateX / 1000;
		}

		// Push
		if (PushAvailable && !IsCrashing && IntendedX != 0 && speed != 0) {
			var hits = Physics.OverlapAll(
				PhysicsMask.ENVIRONMENT,
				Rect.Shrink(0, 0, 4, 4).EdgeOutside(IntendedX < 0 ? Direction4.Left : Direction4.Right),
				out int count, Target
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

		bool ignoreGravity = false;
		Target.RisingGravityScale.Override(JumpRiseGravityRate);

		switch (MovementState) {

			// Swim
			case CharacterMovementState.SwimIdle:
			case CharacterMovementState.SwimMove:
				if (IntendedY != 0) {
					VelocityY = VelocityY.MoveTowards(
						IntendedY * SwimSpeed, SwimAcceleration, SwimDeceleration
					);
					ignoreGravity = true;
				}
				break;

			// Climb
			case CharacterMovementState.Climb:
				VelocityY = (IntendedY <= 0 || ClimbCheck(true) ? IntendedY : 0) * ClimbSpeedY;
				ignoreGravity = true;
				break;

			// Pound
			case CharacterMovementState.Pound:
				ignoreGravity = true;
				VelocityY = -PoundSpeed;
				break;

			// Fly
			case CharacterMovementState.Fly:
				Target.RisingGravityScale.Override(FlyGravityRiseRate);
				Target.FallingGravityScale.Override(FlyGravityFallRate);
				VelocityY = Util.Max(VelocityY, -FlyFallSpeed);
				break;

			// Slide
			case CharacterMovementState.Slide:
				if (VelocityY < -SlideDropSpeed) {
					VelocityY = -SlideDropSpeed;
					ignoreGravity = true;
				}
				break;

			// Grab Top
			case CharacterMovementState.GrabTop:
				ignoreGravity = true;
				VelocityY = 0;
				break;

			// Grab Side
			case CharacterMovementState.GrabSide:
				ignoreGravity = true;
				VelocityY = IntendedY <= 0 || AllowGrabSideMoveUp ? IntendedY * GrabMoveSpeedY : 0;
				break;

			// Grab Flip
			case CharacterMovementState.GrabFlip:
				ignoreGravity = true;
				VelocityY = (MovementHeight + Const.CEL + 12) / Util.Max(GrabFlipThroughDuration, 1) + 1;
				if (!IsGrabFlippingUp) VelocityY *= -1;
				break;

		}
		if (ignoreGravity) {
			Target.RisingGravityScale.Override(0);
			Target.FallingGravityScale.Override(0);
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
			Target.PerformMove(hitRect.xMax - rect.xMin, 0);
		}

		// Clip Right
		if (CheckCorrect(
			new IRect(rect.xMin + Width, rect.yMax, 1, size),
			new IRect(rect.xMin, rect.yMax, rect.width - CLIP_CORRECT_TOLERANCE, size),
			out hitRect
		)) {
			Target.PerformMove(hitRect.xMin - rect.xMax, 0);
		}

		// Func
		bool CheckCorrect (IRect trueRect, IRect falseRect, out IRect hitRect) {
			if (
				Physics.Overlap(CollisionMask, trueRect, out var hit, Target) &&
				!Physics.Overlap(CollisionMask, falseRect, Target)
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
	public void ReloadMovementConfigFromFile () {
		string cName = Target.GetType().AngeName();
		int id = cName.AngeHash();
		string path = Util.CombinePaths(Universe.BuiltIn.CharacterMovementConfigRoot, $"{cName}.txt");
		List<(string name, int value)> configList = [("", 0)];
		ConfigPool[id] = configList;
		bool loaded = FrameworkUtil.NameAndIntFile_to_List(configList, path);
		if (!loaded) {
			Util.TextToFile("", path);
		}
		LocalMovementConfigVersion = configList[0].value;
		FrameworkUtil.List_to_FrameBasedFields(configList, this);
	}


	// Movement Logic
	public virtual void Move (Direction3 x, Direction3 y, bool walk = false) => MoveLogic((int)x, (int)y, walk);


	public virtual void Stop () {
		MoveLogic(0, 0);
		VelocityX = 0;
	}


	public virtual void HoldJump (bool holding) => HoldingJump = holding;


	public virtual void Jump () {
		IntendedJump = true;
		RequireJumpFrame = Game.GlobalFrame;
		Target.CancelMakeGrounded();
	}


	public virtual void Dash () => IntendedDash = true;
	public void StopDash () => LastDashFrame = int.MinValue;


	public virtual void Pound () => IntendedPound = true;


	public virtual void Rush () => IntendedRush = true;
	public void StopRush () => LastRushStartFrame = int.MinValue;


	public virtual void Crash () => LastCrashFrame = Game.GlobalFrame;
	public void StopCrash () => LastCrashFrame = int.MinValue;


	public void LockFacingRight (bool facingRight, int duration = 1) {
		LockedFacingFrame = Game.GlobalFrame + duration;
		LockedFacingRight = facingRight;
	}


	public void SetSpeedRate (int newRate, int duration = 1) {
		SpeedRateFrame = Game.GlobalFrame + duration;
		SpeedRateX = newRate;
	}


	// Movement State
	public static CharacterMovementState CalculateMovementState (CharacterMovement movement) {
		return movement.IsCrashing ? CharacterMovementState.Crash :
		movement.IsFlying ? CharacterMovementState.Fly :
		movement.IsClimbing ? CharacterMovementState.Climb :
		movement.IsPounding ? CharacterMovementState.Pound :
		movement.IsSliding ? CharacterMovementState.Slide :
		movement.IsGrabFlipping ? CharacterMovementState.GrabFlip :
		movement.IsGrabbingTop ? CharacterMovementState.GrabTop :
		movement.IsGrabbingSide ? CharacterMovementState.GrabSide :
		movement.IsRushing ? CharacterMovementState.Rush :
		movement.IsDashing ? CharacterMovementState.Dash :
		movement.IsSquatting ? (movement.IsMoving ? CharacterMovementState.SquatMove : CharacterMovementState.SquatIdle) :
		movement.SwimAvailable && movement.InWater && !movement.IsGrounded ? (movement.IsMoving ? CharacterMovementState.SwimMove : CharacterMovementState.SwimIdle) :
		!movement.IsGrounded && !movement.InWater && !movement.IsClimbing ? (movement.VelocityY > 0 ? CharacterMovementState.JumpUp : CharacterMovementState.JumpDown) :
		movement.IsMoving && (movement.ShouldRun ? movement.RunSpeed : movement.WalkSpeed) != 0 ? (movement.ShouldRun && !movement.IsInsideGround ? CharacterMovementState.Run : CharacterMovementState.Walk) :
		CharacterMovementState.Idle;
	}


	protected virtual CharacterMovementState GetMovementState () => CalculateMovementState(this);


	#endregion




	#region --- LGC ---


	// Move
	protected void MoveLogic (int x, int y, bool walk = false) {
		if (IntendedX != 0 && x == 0) LastEndMoveFrame = Game.GlobalFrame;
		if (IntendedX == 0 && x != 0) {
			LastStartMoveFrame = Game.GlobalFrame;
		}
		if (IntendedX != x && x != 0) {
			FacingRight = x > 0;
		}
		IntendedX = x;
		IntendedY = y;
		walk &= WalkAvailable;
		ShouldRun = RunAvailable && !walk;
		if (x != 0 || y != 0) {
			LastMoveDirection = new(IntendedX, IntendedY);
		}
	}


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
			PhysicsMask.MAP, rect, Target, Direction4.Up, false
		)) return true;

		// Overlap Check
		return Physics.Overlap(PhysicsMask.MAP, rect, null);
	}


	private bool ClimbCheck (bool up = false) {
		if (IsInsideGround) return false;
		if (Physics.Overlap(
			PhysicsMask.MAP,
			up ? Rect.Shift(0, ClimbSpeedY) : Rect,
			Target,
			OperationMode.TriggerOnly,
			Tag.Climb
		)) {
			return true;
		}
		if (Physics.Overlap(
			PhysicsMask.MAP,
			up ? Rect.Shift(0, ClimbSpeedY) : Rect,
			out var info,
			Target,
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
			var hits = Physics.OverlapAll(PhysicsMask.MAP, rect, out int count, Target, OperationMode.ColliderAndTrigger);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.IsTrigger && !hit.Tag.HasAny(Tag.OnewayLeft | Tag.OnewayRight)) continue;
				if (hit.Tag.HasAny(Tag.NoSlide | Tag.GrabTop | Tag.GrabSide)) continue;
				return true;
			}
			return false;
		} else {
			return Physics.Overlap(
				PhysicsMask.MAP, rect, Target, OperationMode.ColliderOnly, Tag.Slide
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
			PhysicsMask.MAP, rect, out var hit, Target,
			OperationMode.ColliderOnly, Tag.GrabTop
		) || Physics.Overlap(
			PhysicsMask.MAP, rect, out hit, Target,
			OperationMode.ColliderOnly, Tag.Grab
		)) {
			grabbingY = hit.Rect.yMin - (TargetCharacter.FinalCharacterHeight * GrabTopHeightAmount / 1000);
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
				PhysicsMask.MAP, rectU.Shift(0, rectU.height), Target, OperationMode.ColliderOnly, Tag.GrabSide
			) || Physics.Overlap(
				PhysicsMask.MAP, rectU.Shift(0, rectU.height), Target, OperationMode.ColliderOnly, Tag.Grab
			);
		}
		return allowGrab;
		// Func
		bool AllowCheck (IRect rect, Tag tag) => Physics.Overlap(PhysicsMask.MAP, rect, Target, OperationMode.ColliderOnly, tag);
	}


	private bool JumpThoughOnewayCheck () {
		var rect = new IRect(Hitbox.xMin, Hitbox.yMin + 4 - Const.CEL / 4, Hitbox.width, Const.CEL / 4);
		if (Physics.Overlap(PhysicsMask.MAP, rect, Target)) return false;
		var hits = Physics.OverlapAll(
			PhysicsMask.MAP, rect, out int count, Target,
			OperationMode.TriggerOnly, Tag.OnewayUp
		);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Tag.HasAll(Tag.Mark)) continue;
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
				new IRect(x, Y + (TargetCharacter.FinalCharacterHeight * GrabTopHeightAmount / 1000) + Const.CEL + Const.HALF, width, 1),
				Target
			)) return false;
			return true;
		} else {
			// Down
			// No Block Below
			if (Physics.Overlap(
				PhysicsMask.MAP,
				new IRect(x, Y - Const.CEL - Const.HALF, width, 1),
				Target
			)) return false;
			// Standing on Grab-Top Block
			var hits = Physics.OverlapAll(
				PhysicsMask.MAP,
				new IRect(x, Y + 4 - Const.CEL / 4, width, Const.CEL / 4), out int count,
				Target, OperationMode.ColliderOnly, Tag.GrabTop
			);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Rect.yMax <= Hitbox.y + 16) return true;
			}
		}
		return false;
	}


	#endregion




}
