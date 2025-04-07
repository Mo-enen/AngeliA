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



/// <summary>
/// Behavior class that handles movement logic for character
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public class CharacterMovement (Rigidbody rig) {




	#region --- MET ---


	/// <summary>
	/// Default with of the character
	/// </summary>
	[PropGroup("Size")]
	public readonly FrameBasedInt MovementWidth = new(150);
	/// <summary>
	/// Height of the character in global space when character is 160cm
	/// </summary>
	public readonly FrameBasedInt MovementHeight = new(384);

	/// <summary>
	/// Allow character to walk
	/// </summary>
	[PropGroup("Walk")]
	public readonly FrameBasedBool WalkAvailable = new(true);
	/// <summary>
	/// How fast should the character walk
	/// </summary>
	[PropVisibility(nameof(WalkAvailable))] public readonly FrameBasedInt WalkSpeed = new(20);
	/// <summary>
	/// Speed acceleration when character is walking
	/// </summary>
	[PropVisibility(nameof(WalkAvailable))] public readonly FrameBasedInt WalkAcceleration = new(3000);
	/// <summary>
	/// Speed acceleration when character is trying to walk to the opposite direction
	/// </summary>
	[PropVisibility(nameof(WalkAvailable))] public readonly FrameBasedInt WalkBrakeAcceleration = new(30000);
	/// <summary>
	/// Speed deceleration when character is walking
	/// </summary>
	[PropVisibility(nameof(WalkAvailable))] public readonly FrameBasedInt WalkDeceleration = new(4000);

	/// <summary>
	/// Allow character to run
	/// </summary>
	[PropGroup("Run")]
	public readonly FrameBasedBool RunAvailable = new(true);
	/// <summary>
	/// How fast should the character run
	/// </summary>
	[PropVisibility(nameof(RunAvailable))] public readonly FrameBasedInt RunSpeed = new(32);
	/// <summary>
	/// Speed acceleration when character is running
	/// </summary>
	[PropVisibility(nameof(RunAvailable))] public readonly FrameBasedInt RunAcceleration = new(3000);
	/// <summary>
	/// Speed acceleration when character is trying to run to the opposite direction
	/// </summary>
	[PropVisibility(nameof(RunAvailable))] public readonly FrameBasedInt RunBrakeAcceleration = new(30000);
	/// <summary>
	/// Speed deceleration when character is running
	/// </summary>
	[PropVisibility(nameof(RunAvailable))] public readonly FrameBasedInt RunDeceleration = new(4000);

	/// <summary>
	/// How many times can the character jump without touching ground
	/// </summary>
	[PropGroup("Jump")]
	public readonly FrameBasedInt JumpCount = new(2);
	/// <summary>
	/// Initial speed when character start to jump
	/// </summary>
	[PropVisibility(nameof(JumpCount), CompareMode.GreaterThan, 0)] public readonly FrameBasedInt JumpSpeed = new(73);
	/// <summary>
	/// When character stop trying to jump (player release the jump button), and the character is still moving up, then the current speed will be multiply to this rate (0 means 0%, 1000 means 100%)
	/// </summary>
	[PropVisibility(nameof(JumpCount), CompareMode.GreaterThan, 0)] public readonly FrameBasedInt JumpReleaseSpeedRate = new(700);
	/// <summary>
	/// Gravity applys on the character will multiply this rate when character moving up in air (0 means 0%, 1000 means 100%)
	/// </summary>
	[PropVisibility(nameof(JumpCount), CompareMode.GreaterThan, 0)] public readonly FrameBasedInt JumpRiseGravityRate = new(600);
	/// <summary>
	/// When character start jump, if it's moving, the running speed will multiply this rate and add into the initial jump speed. (0 means 0%, 1000 means 100%)
	/// </summary>
	[PropVisibility(nameof(JumpCount), CompareMode.GreaterThan, 0)] public readonly FrameBasedInt JumpBoostFromMoveRate = new(500);
	/// <summary>
	/// When character jump from ground, does it jump with rolling in air
	/// </summary>
	[PropVisibility(nameof(JumpCount), CompareMode.GreaterThan, 0)] public readonly FrameBasedBool FirstJumpWithRoll = new(false);
	/// <summary>
	/// When character jump from air, does it jump with rolling in air
	/// </summary>
	[PropVisibility(nameof(JumpCount), CompareMode.GreaterThan, 1)] public readonly FrameBasedBool SubsequentJumpWithRoll = new(true);
	/// <summary>
	/// Allow character jump when rushing and stop the rush
	/// </summary>
	[PropVisibility(nameof(JumpCount), CompareMode.GreaterThan, 0)] public readonly FrameBasedBool JumpBreakRush = new(false);
	/// <summary>
	/// Allow character jump when dashing and stop the dash
	/// </summary>
	[PropVisibility(nameof(JumpCount), CompareMode.GreaterThan, 0)] public readonly FrameBasedBool JumpBreakDash = new(true);
	/// <summary>
	/// Allow character jump when squatting, and keep squatting when jump in air
	/// </summary>
	[PropVisibility(nameof(JumpCount), CompareMode.GreaterThan, 0)] public readonly FrameBasedBool AllowSquatJump = new(false);
	/// <summary>
	/// Allow character jump down and go through oneway gate (player holding down button and press jump button once)
	/// </summary>
	public readonly FrameBasedBool JumpDownThroughOneway = new(false);

	/// <summary>
	/// Allow character to squat
	/// </summary>
	[PropGroup("Squat")]
	public readonly FrameBasedBool SquatAvailable = new(true);
	/// <summary>
	/// Character hitbox height multiply this rate when squatting (0 means 0%, 1000 means 100%)
	/// </summary>
	[PropVisibility(nameof(SquatAvailable))] public readonly FrameBasedInt SquatHeightAmount = new(521);
	/// <summary>
	/// Move speed when character squatting, set to 0 when not allow squat move
	/// </summary>
	[PropVisibility(nameof(SquatAvailable))] public readonly FrameBasedInt SquatMoveSpeed = new(14);
	/// <summary>
	/// Movement acceleration when squat moving
	/// </summary>
	[PropVisibility(nameof(SquatAvailable))] public readonly FrameBasedInt SquatAcceleration = new(48000);
	/// <summary>
	/// Movement deceleration when squat moving
	/// </summary>
	[PropVisibility(nameof(SquatAvailable))] public readonly FrameBasedInt SquatDeceleration = new(48000);

	/// <summary>
	/// Allow character to dash (player hold down button and press jump button for once)
	/// </summary>
	[PropGroup("Dash")]
	public readonly FrameBasedBool DashAvailable = new(true);
	/// <summary>
	///  Character hitbox height multiply this rate when dashing (0 means 0%, 1000 means 100%)
	/// </summary>
	[PropVisibility(nameof(DashAvailable))] public readonly FrameBasedInt DashHeightAmount = new(521);
	/// <summary>
	/// Character roll when dashing
	/// </summary>
	[PropVisibility(nameof(DashAvailable))] public readonly FrameBasedBool DashWithRoll = new(false);
	/// <summary>
	/// Allow character dash through fire to put it out
	/// </summary>
	[PropVisibility(nameof(DashAvailable))] public readonly FrameBasedBool DashPutoutFire = new(true);
	/// <summary>
	/// Movement speed for dashing
	/// </summary>
	[PropVisibility(nameof(DashAvailable))] public readonly FrameBasedInt DashSpeed = new(42);
	/// <summary>
	/// How many frames does dash last
	/// </summary>
	[PropVisibility(nameof(DashAvailable))] public readonly FrameBasedInt DashDuration = new(20);
	/// <summary>
	/// Frames length between prev dash end and next dash start
	/// </summary>
	[PropVisibility(nameof(DashAvailable))] public readonly FrameBasedInt DashCooldown = new(4);
	/// <summary>
	/// Speed acceleration when dashing
	/// </summary>
	[PropVisibility(nameof(DashAvailable))] public readonly FrameBasedInt DashAcceleration = new(24000);
	/// <summary>
	/// Speed multiply this rate when dash being cancel (0 means 0%, 1000 means 100%)
	/// </summary>
	[PropVisibility(nameof(DashAvailable))] public readonly FrameBasedInt DashCancelLoseRate = new(300);

	/// <summary>
	/// Allow character to rush
	/// </summary>
	[PropGroup("Rush")]
	public readonly FrameBasedBool RushAvailable = new(false);
	/// <summary>
	///  Character hitbox height multiply this rate when rushing (0 means 0%, 1000 means 100%)
	/// </summary>
	[PropVisibility(nameof(RushAvailable))] public readonly FrameBasedInt RushHeightAmount = new(1000);
	/// <summary>
	/// Allow character rush when not grounded
	/// </summary>
	[PropVisibility(nameof(RushAvailable))] public readonly FrameBasedBool RushInAir = new(false);
	/// <summary>
	/// Allow character rush when inside water
	/// </summary>
	[PropVisibility(nameof(RushAvailable))] public readonly FrameBasedBool RushInWater = new(true);
	/// <summary>
	/// character rush when climb
	/// </summary>
	[PropVisibility(nameof(RushAvailable))] public readonly FrameBasedBool RushWhenClimb = new(false);
	/// <summary>
	/// character rush when squat
	/// </summary>
	[PropVisibility(nameof(RushAvailable))] public readonly FrameBasedBool RushWhenSquat = new(false);
	/// <summary>
	/// Allow character rush through fire to put it out
	/// </summary>
	[PropVisibility(nameof(RushAvailable))] public readonly FrameBasedBool RushPutoutFire = new(true);
	/// <summary>
	/// Movement speed when rushing
	/// </summary>
	[PropVisibility(nameof(RushAvailable))] public readonly FrameBasedInt RushSpeed = new(72);
	/// <summary>
	/// Movement speed when rush end
	/// </summary>
	[PropVisibility(nameof(RushAvailable))] public readonly FrameBasedInt RushStopSpeed = new(8);
	/// <summary>
	/// How many frames does rush last
	/// </summary>
	[PropVisibility(nameof(RushAvailable))] public readonly FrameBasedInt RushDuration = new(8);
	/// <summary>
	/// How many frames does character not allow to move after rush end
	/// </summary>
	[PropVisibility(nameof(RushAvailable))] public readonly FrameBasedInt RushStiff = new(10);
	/// <summary>
	/// How many frames does character has to wair after prev rush end to rush again
	/// </summary>
	[PropVisibility(nameof(RushAvailable))] public readonly FrameBasedInt RushCooldown = new(2);
	/// <summary>
	/// Speed acceleration when rushing
	/// </summary>
	[PropVisibility(nameof(RushAvailable))] public readonly FrameBasedInt RushAcceleration = new(12000);
	/// <summary>
	/// Speed deceleration when rushing
	/// </summary>
	[PropVisibility(nameof(RushAvailable))] public readonly FrameBasedInt RushDeceleration = new(4000);

	/// <summary>
	/// Allow character slip when run on slippery ground
	/// </summary>
	[PropGroup("Slip")]
	public readonly FrameBasedBool SlipAvailable = new(true);
	/// <summary>
	/// Speed acceleration when moving on slippery ground
	/// </summary>
	[PropVisibility(nameof(SlipAvailable))] public readonly FrameBasedInt SlipAcceleration = new(2000);
	/// <summary>
	/// Speed deceleration when moving on slippery ground
	/// </summary>
	[PropVisibility(nameof(SlipAvailable))] public readonly FrameBasedInt SlipDeceleration = new(1000);

	/// <summary>
	/// Allow character ground pound in air (player press down button in air)
	/// </summary>
	[PropGroup("Pound")]
	public readonly FrameBasedBool PoundAvailable = new(false);
	/// <summary>
	/// Allow character pound on fire to put it out
	/// </summary>
	[PropVisibility(nameof(PoundAvailable))] public readonly FrameBasedBool PoundPutoutFire = new(true);
	/// <summary>
	/// Movement speed when pounding
	/// </summary>
	[PropVisibility(nameof(PoundAvailable))] public readonly FrameBasedInt PoundSpeed = new(96);

	/// <summary>
	/// Allow character swim in water
	/// </summary>
	[PropGroup("Swim")]
	public readonly FrameBasedBool SwimAvailable = new(true);
	/// <summary>
	/// When character inside water, the movement speed will multiply this rate (0 means 0%, 1000 means 100%)
	/// </summary>
	public readonly FrameBasedInt InWaterSpeedRate = new(500);
	/// <summary>
	///  Character hitbox width multiply this rate when swimming (0 means 0%, 1000 means 100%)
	/// </summary>
	[PropVisibility(nameof(SwimAvailable))] public readonly FrameBasedInt SwimWidthAmount = new(1333);
	/// <summary>
	///  Character hitbox height multiply this rate when swimming (0 means 0%, 1000 means 100%)
	/// </summary>
	[PropVisibility(nameof(SwimAvailable))] public readonly FrameBasedInt SwimHeightAmount = new(1000);
	/// <summary>
	/// Movement speed when character swimming
	/// </summary>
	[PropVisibility(nameof(SwimAvailable))] public readonly FrameBasedInt SwimSpeed = new(42);
	/// <summary>
	/// Movement speed when character jump inside water
	/// </summary>
	[PropVisibility(nameof(SwimAvailable))] public readonly FrameBasedInt SwimJumpSpeed = new(128);
	/// <summary>
	/// Speed acceleration when chracter inside water
	/// </summary>
	[PropVisibility(nameof(SwimAvailable))] public readonly FrameBasedInt SwimAcceleration = new(4000);
	/// <summary>
	/// Speed deceleration when chracter inside water
	/// </summary>
	[PropVisibility(nameof(SwimAvailable))] public readonly FrameBasedInt SwimDeceleration = new(4000);

	/// <summary>
	/// Allow character to climb
	/// </summary>
	[PropGroup("Climb")]
	public readonly FrameBasedBool ClimbAvailable = new(true);
	/// <summary>
	/// Allow character jump when climbing
	/// </summary>
	[PropVisibility(nameof(ClimbAvailable))] public readonly FrameBasedBool AllowJumpWhenClimbing = new(true);
	/// <summary>
	/// Horizontal speed when climbing
	/// </summary>
	[PropVisibility(nameof(ClimbAvailable))] public readonly FrameBasedInt ClimbSpeedX = new(12);
	/// <summary>
	/// Vertical speed when climbing
	/// </summary>
	[PropVisibility(nameof(ClimbAvailable))] public readonly FrameBasedInt ClimbSpeedY = new(18);

	/// <summary>
	/// Allow character to fly (player press jump button when no jump count left)
	/// </summary>
	[PropGroup("Fly")]
	public readonly FrameBasedBool FlyAvailable = new(false);
	/// <summary>
	/// Character hitbox height multiply this rate when flying (0 means 0%, 1000 means 100%)
	/// </summary>
	[PropVisibility(nameof(FlyAvailable))] public readonly FrameBasedInt FlyHeightAmount = new(521);
	/// <summary>
	/// When glide flying, character always move to the direction it facing
	/// </summary>
	[PropVisibility(nameof(FlyAvailable))] public readonly FrameBasedBool GlideOnFlying = new(false);
	/// <summary>
	/// Character has to wait this many frames to fly again
	/// </summary>
	[PropVisibility(nameof(FlyAvailable))] public readonly FrameBasedInt FlyCooldown = new(24);
	/// <summary>
	/// Initial speed when fly start
	/// </summary>
	[PropVisibility(nameof(FlyAvailable))] public readonly FrameBasedInt FlyRiseSpeed = new(96);
	/// <summary>
	/// Gravity multiply this rate when flying and moving up (0 means 0%, 1000 means 100%)
	/// </summary>
	[PropVisibility(nameof(FlyAvailable))] public readonly FrameBasedInt FlyGravityRiseRate = new(800);
	/// <summary>
	/// Gravity multiply this rate when flying and moving down (0 means 0%, 1000 means 100%)
	/// </summary>
	[PropVisibility(nameof(FlyAvailable))] public readonly FrameBasedInt FlyGravityFallRate = new(200);
	/// <summary>
	/// Fall down speed shen flying
	/// </summary>
	[PropVisibility(nameof(FlyAvailable))] public readonly FrameBasedInt FlyFallSpeed = new(16);
	/// <summary>
	/// Horizontal move speed when flying
	/// </summary>
	[PropVisibility(nameof(FlyAvailable))] public readonly FrameBasedInt FlyMoveSpeed = new(32);
	/// <summary>
	/// Speed acceleration when flying
	/// </summary>
	[PropVisibility(nameof(FlyAvailable))] public readonly FrameBasedInt FlyAcceleration = new(2000);
	/// <summary>
	/// Speed deceleration when flying
	/// </summary>
	[PropVisibility(nameof(FlyAvailable))] public readonly FrameBasedInt FlyDeceleration = new(1000);

	/// <summary>
	/// Allow character to slide on wall
	/// </summary>
	[PropGroup("Slide")]
	public readonly FrameBasedBool SlideAvailable = new(false);
	/// <summary>
	/// Allow character to slide on all type of blocks
	/// </summary>
	[PropVisibility(nameof(SlideAvailable))] public readonly FrameBasedBool SlideOnAnyBlock = new(false);
	/// <summary>
	/// Set jump count to 0 when slide
	/// </summary>
	[PropVisibility(nameof(SlideAvailable))] public readonly FrameBasedBool ResetJumpCountWhenSlide = new(true);
	/// <summary>
	/// Horizontal initial speed when character jump when slide
	/// </summary>
	[PropVisibility(nameof(SlideAvailable))] public readonly FrameBasedInt SlideJumpKickSpeed = new(56);
	/// <summary>
	/// Drop down speed when sliding
	/// </summary>
	[PropVisibility(nameof(SlideAvailable))] public readonly FrameBasedInt SlideDropSpeed = new(4);

	/// <summary>
	/// Allow character grab on the bottom of blocks with Tag.GrabTop
	/// </summary>
	[PropGroup("Grab")]
	public readonly FrameBasedBool GrabTopAvailable = new(true);
	/// <summary>
	/// Allow character grab on the side of blocks with Tag.GrabSide
	/// </summary>
	public readonly FrameBasedBool GrabSideAvailable = new(true);
	/// <summary>
	/// Character hitbox height multiply this rate when top-grabbing (0 means 0%, 1000 means 100%)
	/// </summary>
	[PropVisibility(nameof(GrabTopAvailable))] public readonly FrameBasedInt GrabTopHeightAmount = new(947);
	/// <summary>
	/// Character hitbox height multiply this rate when side-grabbing (0 means 0%, 1000 means 100%)
	/// </summary>
	[PropVisibility(nameof(GrabSideAvailable))] public readonly FrameBasedInt GrabSideHeightAmount = new(947);
	/// <summary>
	/// Set jump count to 0 when character grabbing
	/// </summary>
	[PropVisibility(nameof(GrabTopAvailable), CompareMode.Or, nameof(GrabSideAvailable))] public readonly FrameBasedBool ResetJumpCountWhenGrab = new(true);
	/// <summary>
	/// Allow character to flip through block downward (player press down when standing on top-grabable blocks)
	/// </summary>
	[PropVisibility(nameof(GrabTopAvailable))] public readonly FrameBasedBool GrabFlipThroughDownAvailable = new(true);
	/// <summary>
	/// Allow character to flip through block upward (player press up when top-grabbing)
	/// </summary>
	[PropVisibility(nameof(GrabTopAvailable))] public readonly FrameBasedBool GrabFlipThroughUpAvailable = new(true);
	/// <summary>
	/// How long does flip through takes in frames
	/// </summary>
	[PropVisibility(nameof(GrabTopAvailable))] public readonly FrameBasedInt GrabFlipThroughDuration = new(18);
	/// <summary>
	/// Horizontal move speed when top-grabbing
	/// </summary>
	[PropVisibility(nameof(GrabTopAvailable))] public readonly FrameBasedInt GrabMoveSpeedX = new(24);
	/// <summary>
	///  Vertical move speed when side-grabbing
	/// </summary>
	[PropVisibility(nameof(GrabSideAvailable))] public readonly FrameBasedInt GrabMoveSpeedY = new(24);
	/// <summary>
	/// Horizontal initial speed when character jump when side-grabbing
	/// </summary>
	[PropVisibility(nameof(GrabSideAvailable))] public readonly FrameBasedInt GrabSideJumpKickSpeed = new(56);

	/// <summary>
	/// Allow character crash
	/// </summary>
	[PropGroup("Crash")]
	public readonly FrameBasedBool CrashAvailable = new(true);
	/// <summary>
	/// Make character crash when running too long on slippery ground or rush on slippery ground
	/// </summary>
	[PropVisibility(nameof(CrashAvailable))] public readonly FrameBasedBool CrashWhenSlippy = new(true);
	/// <summary>
	/// How many frames does it takes for one crash
	/// </summary>
	[PropVisibility(nameof(CrashAvailable))] public readonly FrameBasedInt CrashDuration = new(30);
	/// <summary>
	/// How many frames does it takes to make character crash
	/// </summary>
	[PropVisibility(nameof(CrashAvailable), CompareMode.And, nameof(CrashWhenSlippy))] public readonly FrameBasedInt CrashRunDurationRequire = new(42);
	/// <summary>
	/// Speed deceleration when character crashing
	/// </summary>
	[PropVisibility(nameof(CrashAvailable))] public readonly FrameBasedInt CrashDeceleration = new(1000);

	/// <summary>
	/// Allow character push other rigidbody
	/// </summary>
	[PropGroup("Push")]
	public readonly FrameBasedBool PushAvailable = new(true);
	/// <summary>
	/// Movement speed when character pushing
	/// </summary>
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
	private const int JUMP_BOOST_TOLERANCE = 12;

	// Api

	public readonly Rigidbody Target = rig;

	public readonly Character TargetCharacter = rig as Character;
	/// <summary>
	/// Direction for last movement
	/// </summary>
	public Int2 LastMoveDirection { get; private set; } = default;
	/// <summary>
	/// 1 if character want to move right, -1 for left
	/// </summary>
	public int IntendedX { get; private set; } = 0;
	/// <summary>
	/// 1 if character want to move up, -1 for down
	/// </summary>
	public int IntendedY { get; private set; } = 0;
	/// <summary>
	/// How many times does character jumps after touching ground
	/// </summary>
	public int CurrentJumpCount { get; set; } = 0;
	/// <summary>
	/// True if character currently facing right
	/// </summary>
	public bool FacingRight { get; set; } = true;
	/// <summary>
	/// True if character currently facing front
	/// </summary>
	public bool FacingFront { get; set; } = true;
	/// <summary>
	/// True if character want to run instead of walk when move
	/// </summary>
	public bool ShouldRun { get; private set; } = true;
	/// <summary>
	/// Does movement config sync with json file in game universe folder
	/// </summary>
	public virtual bool SyncFromConfigFile => true;

	// Frame Cache
	/// <summary>
	/// Last frame when character start touching ground
	/// </summary>
	public int LastGroundFrame { get; private set; } = int.MinValue;
	/// <summary>
	/// Last frame when character is touching ground
	/// </summary>
	public int LastGroundingFrame { get; private set; } = int.MinValue;
	/// <summary>
	/// Last frame when character start to move
	/// </summary>
	public int LastStartMoveFrame { get; private set; } = -1;
	/// <summary>
	/// Last frame when character moving ends
	/// </summary>
	public int LastEndMoveFrame { get; private set; } = int.MinValue;
	/// <summary>
	/// Last frame when character start to jump
	/// </summary>
	public int LastJumpFrame { get; private set; } = int.MinValue;
	/// <summary>
	/// Last frame when character climb
	/// </summary>
	public int LastClimbFrame { get; private set; } = int.MinValue;
	/// <summary>
	/// Last frame when character start to dash
	/// </summary>
	public int LastDashFrame { get; private set; } = int.MinValue;
	/// <summary>
	/// Last frame when character start to rush
	/// </summary>
	public int LastRushStartFrame { get; private set; } = int.MinValue;
	/// <summary>
	/// Last frame when character crash
	/// </summary>
	public int LastCrashFrame { get; private set; } = int.MinValue;
	/// <summary>
	/// Last frame when character start to move on slippery ground
	/// </summary>
	public int LastSlippyMoveStartFrame { get; private set; } = int.MinValue;
	/// <summary>
	/// Last frame when character start to squat
	/// </summary>
	public int LastSquatStartFrame { get; private set; } = int.MinValue;
	/// <summary>
	/// Last frame when character is squatting
	/// </summary>
	public int LastSquattingFrame { get; private set; } = int.MinValue;
	/// <summary>
	/// Last frame when character is pounding
	/// </summary>
	public int LastPoundingFrame { get; private set; } = int.MinValue;
	/// <summary>
	/// Last frame when character is sliding on wall
	/// </summary>
	public int LastSlidingFrame { get; private set; } = int.MinValue;
	/// <summary>
	/// Last frame when character is grabbing
	/// </summary>
	public int LastGrabbingFrame { get; private set; } = int.MinValue;
	/// <summary>
	/// Last frame when character start to fly
	/// </summary>
	public int LastFlyFrame { get; private set; } = int.MinValue;
	/// <summary>
	/// Last frame when character start to grab flip upward
	/// </summary>
	public int LastGrabFlipUpFrame { get; private set; } = int.MinValue;
	/// <summary>
	/// Last frame when character start to grab flip downward
	/// </summary>
	public int LastGrabFlipDownFrame { get; private set; } = int.MinValue;
	/// <summary>
	/// Last frame when character stop grabbing
	/// </summary>
	public int LastGrabCancelFrame { get; private set; } = int.MinValue;
	/// <summary>
	/// Last frame when character start to run
	/// </summary>
	public int LastStartRunFrame { get; private set; } = int.MinValue;
	/// <summary>
	/// Last frame when character change horizontal facing
	/// </summary>
	public int LastFacingChangeFrame { get; private set; } = 0;

	// Movement State
	/// <summary>
	/// Current movement type
	/// </summary>
	public CharacterMovementState MovementState { get; set; } = CharacterMovementState.Idle;
	/// <summary>
	/// True if character flipping through blocks
	/// </summary>
	public bool IsGrabFlipping => IsGrabFlippingUp || IsGrabFlippingDown;
	/// <summary>
	/// True if character flipping through blocks upward
	/// </summary>
	public bool IsGrabFlippingUp => Game.GlobalFrame < LastGrabFlipUpFrame + Util.Max(GrabFlipThroughDuration, 1);
	/// <summary>
	/// True if character flipping through blocks downward
	/// </summary>
	public bool IsGrabFlippingDown => Game.GlobalFrame < LastGrabFlipDownFrame + Util.Max(GrabFlipThroughDuration, 1);
	/// <summary>
	/// True if character is moving
	/// </summary>
	public bool IsMoving => IntendedX != 0;
	/// <summary>
	/// True if character is walking
	/// </summary>
	public bool IsWalking => WalkAvailable && IntendedX != 0 && !ShouldRun;
	/// <summary>
	/// True if character is running
	/// </summary>
	public bool IsRunning => RunAvailable && IntendedX != 0 && ShouldRun;
	/// <summary>
	/// True if character is rolling
	/// </summary>
	public bool IsRolling { get; private set; } = false;
	/// <summary>
	/// True if character is dashing
	/// </summary>
	public bool IsDashing { get; private set; } = false;
	/// <summary>
	/// True if character is rushing
	/// </summary>
	public bool IsRushing { get; private set; } = false;
	/// <summary>
	/// True if character is crashing
	/// </summary>
	public bool IsCrashing { get; private set; } = false;
	/// <summary>
	/// True if character is squatting
	/// </summary>
	public bool IsSquatting { get; private set; } = false;
	/// <summary>
	/// True if character is pounding
	/// </summary>
	public bool IsPounding { get; private set; } = false;
	/// <summary>
	/// True if character is climbing
	/// </summary>
	public bool IsClimbing { get; private set; } = false;
	/// <summary>
	/// True if character is flying
	/// </summary>
	public bool IsFlying { get; private set; } = false;
	/// <summary>
	/// True if character is sliding on wall
	/// </summary>
	public bool IsSliding { get; private set; } = false;
	/// <summary>
	/// True if character is top-grabbing
	/// </summary>
	public bool IsGrabbingTop { get; private set; } = false;
	/// <summary>
	/// True if character is side-grabbing
	/// </summary>
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
	private bool SquatJumpping;
	private bool OnSlippy;
	private int? ClimbPositionCorrect = null;
	private int LockedFacingFrame = int.MinValue;
	private int LockedSquatFrame = int.MinValue;
	private int RequireJumpFrame = int.MinValue;
	private int SpeedRateFrame = int.MinValue;
	private int SpeedRateX = 1000;


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
		LockedSquatFrame = int.MinValue;
		LockedFacingFrame = int.MinValue;

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

		// Reset LastJumpIsSquatJump
		if (SquatJumpping && IsGrounded && frame > RequireJumpFrame.GreaterOrEquel(LastJumpFrame) + JUMP_REQUIRE_TOLERANCE) {
			SquatJumpping = false;
		}

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
		bool squatting = frame <= LockedSquatFrame || (
			SquatAvailable && IsGrounded && !IsClimbing && !IsInsideGround && !IsCrashing &&
			((!IsDashing && !IsRushing && IntendedY < 0) || ForceSquatCheck()));
		if (!IsSquatting && squatting) LastSquatStartFrame = frame;
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
			LastSlippyMoveStartFrame = frame;
			LastFacingChangeFrame = frame;
		}

		// Facing Front
		FacingFront = !IsClimbing;

		// Physics
		int growingHeight = TargetCharacter != null ? TargetCharacter.FinalCharacterHeight : MovementHeight;
		int width = InWater ? MovementWidth * SwimWidthAmount / 1000 : MovementWidth;
		int height =
			IsSquatting || SquatJumpping ? growingHeight * SquatHeightAmount / 1000 :
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
			(AllowSquatJump || !IsSquatting) &&
			!IsGrabbingTop && !IsInsideGround &&
			(JumpBreakRush || !IsRushing) &&
			(JumpBreakDash || !IsDashing) &&
			!IsGrabFlipping && !IsCrashing &&
			(!IsClimbing || AllowJumpWhenClimbing);

		// Perform Jump/Fly
		if (movementAllowJump) {
			// Jump
			if (CurrentJumpCount < JumpCount) {
				// Jump
				if (IntendedJump || frame < RequireJumpFrame + JUMP_REQUIRE_TOLERANCE) {
					// Perform Jump
					CurrentJumpCount++;
					int jumpSpeed = InWater ? SwimJumpSpeed : JumpSpeed;
					if (IntendedX != 0) {
						int boostFrame = (frame - LastStartMoveFrame).Clamp(0, JUMP_BOOST_TOLERANCE);
						jumpSpeed += RunSpeed * JumpBoostFromMoveRate * boostFrame / 1000 / JUMP_BOOST_TOLERANCE;
					}
					VelocityY = Util.Max(jumpSpeed, VelocityY);
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
		if (JumpDownThroughOneway && JumpThoughOnewayCheck()) {
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
				if (SquatJumpping) {
					// Squat Jump
					speed = IntendedX * RunSpeed;
					acc = RunAcceleration;
					dcc = RunDeceleration;
				} else {
					// Squat Move
					speed = IntendedX * SquatMoveSpeed;
					acc = SquatAcceleration;
					dcc = SquatDeceleration;
				}
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
		int localFrame = Game.GlobalFrame - LastStartMoveFrame;
		VelocityX = VelocityX.MoveTowards(
			speed,
			FrameworkUtil.GetFrameAmortizedValue(acc, localFrame),
			FrameworkUtil.GetFrameAmortizedValue(dcc, localFrame)
		);
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
						IntendedY * SwimSpeed,
						FrameworkUtil.GetFrameAmortizedValue(SwimAcceleration),
						FrameworkUtil.GetFrameAmortizedValue(SwimDeceleration)
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
	/// <summary>
	/// Load movement data from json file inside game universe folder
	/// </summary>
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
	/// <summary>
	/// Move the character
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <param name="walk">True if character want to walk instead of run</param>
	public virtual void Move (Direction3 x, Direction3 y, bool walk = false) => MoveLogic((int)x, (int)y, walk);


	/// <summary>
	/// Stop current movement
	/// </summary>
	public virtual void Stop () {
		MoveLogic(0, 0);
		VelocityX = 0;
	}


	/// <summary>
	/// Is character holding jump (call this every frame no matter character is jumpping or not)
	/// </summary>
	public virtual void HoldJump (bool holding) => HoldingJump = holding;


	/// <summary>
	/// Perform jump (call this once when jump start)
	/// </summary>
	/// <param name="isSquatJump">Is this jump a squat jump or not</param>
	public virtual void Jump (bool isSquatJump = false) {
		IntendedJump = true;
		SquatJumpping = isSquatJump;
		RequireJumpFrame = Game.GlobalFrame;
		Target.CancelMakeGrounded();
	}


	/// <summary>
	/// Perform dash (call this once when dash start)
	/// </summary>
	public virtual void Dash () => IntendedDash = true;


	/// <summary>
	/// Stop current dashing 
	/// </summary>
	public void StopDash () => LastDashFrame = int.MinValue;


	/// <summary>
	/// Perform pound (call this once when pound start)
	/// </summary>
	public virtual void Pound () => IntendedPound = true;


	/// <summary>
	/// Perform rush (call this once when rush start)
	/// </summary>
	public virtual void Rush () => IntendedRush = true;


	/// <summary>
	/// Stop current rush
	/// </summary>
	public void StopRush () => LastRushStartFrame = int.MinValue;


	/// <summary>
	/// Perform crash (call this once when crash start)
	/// </summary>
	public virtual void Crash () => LastCrashFrame = Game.GlobalFrame;



	/// <summary>
	/// Stop current crash
	/// </summary>
	public void StopCrash () => LastCrashFrame = int.MinValue;


	/// <summary>
	/// Force character facing right or left for given frames
	/// </summary>
	public void LockFacingRight (bool facingRight, int duration = 1) {
		LockedFacingFrame = Game.GlobalFrame + duration;
		LockedFacingRight = facingRight;
	}


	/// <summary>
	/// Force character to squat for given frames
	/// </summary>
	public void LockSquat (int duration = 1) => LockedSquatFrame = Game.GlobalFrame + duration;


	/// <summary>
	/// Force movement speed rate for given frames
	/// </summary>
	/// <param name="newRate">0 means 0%, 1000 means 100%</param>
	/// <param name="duration"></param>
	public void SetSpeedRate (int newRate, int duration = 1) {
		SpeedRateFrame = Game.GlobalFrame + duration;
		SpeedRateX = newRate;
	}


	// Movement State
	/// <summary>
	/// Get current movement type base on current cached data
	/// </summary>
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
		movement.IsSquatting ? (movement.IsMoving && movement.SquatMoveSpeed != 0 ? CharacterMovementState.SquatMove : CharacterMovementState.SquatIdle) :
		movement.SwimAvailable && movement.InWater && !movement.IsGrounded ? (movement.IsMoving ? CharacterMovementState.SwimMove : CharacterMovementState.SwimIdle) :
		movement.SquatJumpping ? CharacterMovementState.SquatIdle : !movement.IsGrounded && !movement.InWater && !movement.IsClimbing ? (movement.VelocityY > 0 ? CharacterMovementState.JumpUp : CharacterMovementState.JumpDown) :
		movement.IsMoving && (movement.ShouldRun ? movement.RunSpeed : movement.WalkSpeed) != 0 ? (movement.ShouldRun && !movement.IsInsideGround ? CharacterMovementState.Run : CharacterMovementState.Walk) :
		CharacterMovementState.Idle;
	}


	/// <summary>
	/// Get current movement type base on current cached data
	/// </summary>
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
			bool oldFacingRight = FacingRight;
			FacingRight = x > 0;
			if (FacingRight != oldFacingRight) {
				LastSlippyMoveStartFrame = Game.GlobalFrame;
				LastFacingChangeFrame = Game.GlobalFrame;
			}
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
		bool right = IntendedX > 0;
		var rect = new IRect(
			right ? Hitbox.xMax : Hitbox.xMin - 1,
			Hitbox.y + Hitbox.height / 2,
			1, 1
		);
		if (SlideOnAnyBlock) {
			var hits = Physics.OverlapAll(PhysicsMask.MAP, rect, out int count, Target, OperationMode.ColliderAndTrigger);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.IsTrigger && !hit.Tag.HasAny(right ? Tag.OnewayLeft : Tag.OnewayRight)) continue;
				if (hit.Tag.HasAny(Tag.NoSlide | Tag.GrabTop | Tag.GrabSide)) continue;
				if (right ? hit.Rect.x < Hitbox.xMax : hit.Rect.xMax > Hitbox.xMin) continue;
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
		bool allowGrab = AllowCheck(rectD, Tag.GrabSide) && AllowCheck(rectU, Tag.GrabSide);
		if (allowGrab) {
			allowMoveUp = Physics.Overlap(
				PhysicsMask.MAP, rectU.Shift(0, rectU.height), Target, OperationMode.ColliderOnly, Tag.GrabSide
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
