using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class CharacterMovementConfig {

	public int MovementWidth = 150;
	public int MovementHeight = 384; // Height when Character is 160cm

	// Height Amount
	public int RollingHeightAmount = 521;
	public int DashHeightAmount = 521;
	public int RushHeightAmount = 1000;
	public int SquatHeightAmount = 521;
	public int SwimHeightAmount = 1000;
	public int FlyHeightAmount = 521;
	public int GrabTopHeightAmount = 947;
	public int GrabSideHeightAmount = 947;

	// Walk
	public int WalkSpeed = 20;
	public int WalkAcceleration = 3;
	public int WalkBrakeAcceleration = 30;
	public int WalkDeceleration = 4;

	// Run
	public int WalkToRunAccumulation = 0;
	public int RunSpeed = 32;
	public int RunAcceleration = 3;
	public int RunBrakeAcceleration = 30;
	public int RunDeceleration = 4;

	// Push
	public bool PushAvailable = true;
	public int PushSpeed = 10;

	// Jump
	public int JumpSpeed = 73;
	public int JumpCount = 2;
	public int JumpReleaseLoseRate = 700;
	public int JumpRiseGravityRate = 600;
	public bool GrowJumpCountWhenFallOffEdge = true;
	public bool FirstJumpWithRoll = false;
	public bool SubsequentJumpWithRoll = true;
	public bool JumpDownThoughOneway = false;

	// Dash
	public bool DashAvailable = true;
	public bool DashWithRoll = false;
	public int DashSpeed = 42;
	public int DashDuration = 20;
	public int DashCooldown = 4;
	public int DashAcceleration = 24;
	public int DashCancelLoseRate = 300;

	// Rush
	public bool RushAvailable = true;
	public bool RushInAir = false;
	public bool RushInWater = true;
	public bool RushWhenClimb = false;
	public bool RushWhenSquat = false;
	public int RushSpeed = 72;
	public int RushStopSpeed = 8;
	public int RushDuration = 8;
	public int RushStiff = 10;
	public int RushCooldown = 2;
	public int RushAcceleration = 12;
	public int RushDeceleration = 4;

	// Slip & Crash
	public bool CrashWhenSlippy = true;
	public int CrashDuration = 30;
	public int CrashRunDurationRequire = 42;
	public int CrashDeceleration = 1;
	public int SlipAcceleration = 2;
	public int SlipDeceleration = 1;

	// Squat
	public bool SquatAvailable = true;
	public int SquatSpeed = 14;
	public int SquatAcceleration = 48;
	public int SquatDeceleration = 48;

	// Pound
	public bool PoundAvailable = true;
	public int PoundSpeed = 96;

	// Swim
	public int SwimWidth = 200;
	public int InWaterSpeedLoseRate = 500;
	public int SwimSpeed = 42;
	public int SwimJumpSpeed = 128;
	public int SwimAcceleration = 4;
	public int SwimDeceleration = 4;

	// Climb
	public bool ClimbAvailable = true;
	public bool JumpWhenClimbAvailable = true;
	public int ClimbSpeedX = 12;
	public int ClimbSpeedY = 18;

	// Fly
	public int FlyCooldown = 24;
	public int FlyRiseSpeed = 96;
	public int FlyGravityRiseRate = 800;
	public int FlyGravityFallRate = 200;
	public int FlyFallSpeed = 12;
	public int FlyMoveSpeed = 32;
	public int FlyAcceleration = 2;
	public int FlyDeceleration = 1;

	// Slide
	public bool SlideAvailable = false;
	public bool SlideOnAnyBlock = false;
	public bool ResetJumpCountWhenSlide = true;
	public int SlideDropSpeed = 4;

	// Grab
	public bool GrabTopAvailable = true;
	public bool GrabSideAvailable = true;
	public bool ResetJumpCountWhenGrab = true;
	public bool GrabFlipThroughDownAvailable = true;
	public bool GrabFlipThroughUpAvailable = true;
	public int GrabFlipThroughDuration = 18;
	public int GrabMoveSpeedX = 24;
	public int GrabMoveSpeedY = 24;

}
