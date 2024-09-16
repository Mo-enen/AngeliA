using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public partial class CharacterMovement {

	public readonly FrameBasedInt MovementWidth = new(150);
	public readonly FrameBasedInt MovementHeight = new(384); // Height when Character is 160cm

	// Height
	public readonly FrameBasedInt DashHeightAmount = new(521);
	public readonly FrameBasedInt RushHeightAmount = new(1000);
	public readonly FrameBasedInt SquatHeightAmount = new(521);
	public readonly FrameBasedInt SwimHeightAmount = new(1000);
	public readonly FrameBasedInt FlyHeightAmount = new(521);
	public readonly FrameBasedInt GrabTopHeightAmount = new(947);
	public readonly FrameBasedInt GrabSideHeightAmount = new(947);

	// Walk
	public readonly FrameBasedBool WalkAvailable = new(true);
	public readonly FrameBasedInt WalkSpeed = new(20);
	public readonly FrameBasedInt WalkAcceleration = new(3);
	public readonly FrameBasedInt WalkBrakeAcceleration = new(30);
	public readonly FrameBasedInt WalkDeceleration = new(4);

	// Run
	public readonly FrameBasedBool RunAvailable = new(true);
	public readonly FrameBasedInt RunSpeed = new(32);
	public readonly FrameBasedInt RunAcceleration = new(3);
	public readonly FrameBasedInt RunBrakeAcceleration = new(30);
	public readonly FrameBasedInt RunDeceleration = new(4);

	// Push
	public readonly FrameBasedBool PushAvailable = new(true);
	public readonly FrameBasedInt PushSpeed = new(10);

	// Jump
	public readonly FrameBasedInt JumpSpeed = new(73);
	public readonly FrameBasedInt JumpCount = new(2);
	public readonly FrameBasedInt JumpReleaseSpeedRate = new(700);
	public readonly FrameBasedInt JumpRiseGravityRate = new(600);
	public readonly FrameBasedBool GrowJumpCountWhenFallOffEdge = new(true);
	public readonly FrameBasedBool FirstJumpWithRoll = new(false);
	public readonly FrameBasedBool SubsequentJumpWithRoll = new(true);
	public readonly FrameBasedBool JumpDownThoughOneway = new(false);

	// Dash
	public readonly FrameBasedBool DashAvailable = new(true);
	public readonly FrameBasedBool DashWithRoll = new(false);
	public readonly FrameBasedBool DashPutoutFire = new(true);
	public readonly FrameBasedInt DashSpeed = new(42);
	public readonly FrameBasedInt DashDuration = new(20);
	public readonly FrameBasedInt DashCooldown = new(4);
	public readonly FrameBasedInt DashAcceleration = new(24);
	public readonly FrameBasedInt DashCancelLoseRate = new(300);

	// Rush
	public readonly FrameBasedBool RushAvailable = new(true);
	public readonly FrameBasedBool RushInAir = new(false);
	public readonly FrameBasedBool RushInWater = new(true);
	public readonly FrameBasedBool RushWhenClimb = new(false);
	public readonly FrameBasedBool RushWhenSquat = new(false);
	public readonly FrameBasedBool RushPutoutFire = new(true);
	public readonly FrameBasedInt RushSpeed = new(72);
	public readonly FrameBasedInt RushStopSpeed = new(8);
	public readonly FrameBasedInt RushDuration = new(8);
	public readonly FrameBasedInt RushStiff = new(10);
	public readonly FrameBasedInt RushCooldown = new(2);
	public readonly FrameBasedInt RushAcceleration = new(12);
	public readonly FrameBasedInt RushDeceleration = new(4);

	// Crash
	public readonly FrameBasedBool CrashAvailable = new(true);
	public readonly FrameBasedBool CrashWhenSlippy = new(true);
	public readonly FrameBasedInt CrashDuration = new(30);
	public readonly FrameBasedInt CrashRunDurationRequire = new(42);
	public readonly FrameBasedInt CrashDeceleration = new(1);

	// Slip
	public readonly FrameBasedBool SlipAvailable = new(true);
	public readonly FrameBasedInt SlipAcceleration = new(2);
	public readonly FrameBasedInt SlipDeceleration = new(1);

	// Squat
	public readonly FrameBasedBool SquatAvailable = new(true);
	public readonly FrameBasedInt SquatSpeed = new(14);
	public readonly FrameBasedInt SquatAcceleration = new(48);
	public readonly FrameBasedInt SquatDeceleration = new(48);

	// Pound
	public readonly FrameBasedBool PoundAvailable = new(true);
	public readonly FrameBasedBool PoundPutoutFire = new(true);
	public readonly FrameBasedInt PoundSpeed = new(96);

	// Swim
	public readonly FrameBasedInt SwimWidth = new(200);
	public readonly FrameBasedInt InWaterSpeedRate = new(500);
	public readonly FrameBasedInt SwimSpeed = new(42);
	public readonly FrameBasedInt SwimJumpSpeed = new(128);
	public readonly FrameBasedInt SwimAcceleration = new(4);
	public readonly FrameBasedInt SwimDeceleration = new(4);

	// Climb
	public readonly FrameBasedBool ClimbAvailable = new(true);
	public readonly FrameBasedBool JumpWhenClimbAvailable = new(true);
	public readonly FrameBasedInt ClimbSpeedX = new(12);
	public readonly FrameBasedInt ClimbSpeedY = new(18);

	// Fly
	public readonly FrameBasedBool FlyAvailable = new(true);
	public readonly FrameBasedBool GlideOnFlying = new(false);
	public readonly FrameBasedInt FlyCooldown = new(24);
	public readonly FrameBasedInt FlyRiseSpeed = new(96);
	public readonly FrameBasedInt FlyGravityRiseRate = new(800);
	public readonly FrameBasedInt FlyGravityFallRate = new(200);
	public readonly FrameBasedInt FlyFallSpeed = new(12);
	public readonly FrameBasedInt FlyMoveSpeed = new(32);
	public readonly FrameBasedInt FlyAcceleration = new(2);
	public readonly FrameBasedInt FlyDeceleration = new(1);

	// Slide
	public readonly FrameBasedBool SlideAvailable = new(false);
	public readonly FrameBasedBool SlideOnAnyBlock = new(false);
	public readonly FrameBasedBool ResetJumpCountWhenSlide = new(true);
	public readonly FrameBasedInt SlideSideJumpSpeed = new(56);
	public readonly FrameBasedInt SlideDropSpeed = new(4);

	// Grab
	public readonly FrameBasedBool GrabTopAvailable = new(true);
	public readonly FrameBasedBool GrabSideAvailable = new(true);
	public readonly FrameBasedBool ResetJumpCountWhenGrab = new(true);
	public readonly FrameBasedBool GrabFlipThroughDownAvailable = new(true);
	public readonly FrameBasedBool GrabFlipThroughUpAvailable = new(true);
	public readonly FrameBasedInt GrabFlipThroughDuration = new(18);
	public readonly FrameBasedInt GrabMoveSpeedX = new(24);
	public readonly FrameBasedInt GrabMoveSpeedY = new(24);
	public readonly FrameBasedInt GrabSideJumpSpeed = new(56);

}
