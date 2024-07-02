using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace AngeliA;


[StructLayout(LayoutKind.Sequential)]
public class CharacterMovementConfig {

	[Group("Move")]
	public int MovementWidth = 150;
	public int MovementHeight = 384; // Global Height when Character is 160cm

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

	[Group("Push")]
	public bool PushAvailable = true;
	public int PushSpeed = 10;

	[Group("Jump")]
	public int JumpSpeed = 73;
	public int JumpCount = 2;
	public int JumpReleaseLoseRate = 700;
	public int JumpRiseGravityRate = 600;
	public bool EdgeFallGrowJumpCount = true;
	public bool JumpDownThoughOneway = false;

	[Group("Roll")]
	public int RollingHeightAmount = 521;
	public bool FirstJumpWithRoll = false;
	public bool SubsequentJumpWithRoll = true;
	public bool DashWithRoll = false;

	[Group("Dash")]
	public bool DashAvailable = true;
	public int DashHeightAmount = 521;
	public int DashSpeed = 42;
	public int DashDuration = 20;
	public int DashCooldown = 4;
	public int DashAcceleration = 24;
	public int DashCancelLoseRate = 300;

	[Group("Rush")]
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
	public int RushHeightAmount = 1000;

	// Slip & Crash
	[Group("SlipCrash")]
	public bool CrashWhenSlippy = true;
	public int CrashDuration = 30;
	public int CrashRunDurationRequire = 42;
	public int CrashDeceleration = 1;
	public int SlipAcceleration = 2;
	public int SlipDeceleration = 1;

	// Squat
	[Group("Squat")]
	public bool SquatAvailable = true;
	public int SquatSpeed = 14;
	public int SquatAcceleration = 48;
	public int SquatDeceleration = 48;
	public int SquatHeightAmount = 521;

	// Pound
	[Group("Pound")]
	public bool PoundAvailable = true;
	public int PoundSpeed = 96;

	// Swim
	[Group("Swim")]
	public int SwimWidth = 200;
	public int InWaterSpeedLoseRate = 500;
	public int SwimSpeed = 42;
	public int SwimJumpSpeed = 128;
	public int SwimAcceleration = 4;
	public int SwimDeceleration = 4;
	public int SwimHeightAmount = 1000;

	// Climb
	[Group("Climb")]
	public bool ClimbAvailable = true;
	public bool JumpWhenClimbAvailable = true;
	public int ClimbSpeedX = 12;
	public int ClimbSpeedY = 18;

	// Fly
	[Group("Fly")]
	public int FlyCooldown = 24;
	public int FlyRiseSpeed = 96;
	public int FlyGravityRiseRate = 800;
	public int FlyGravityFallRate = 200;
	public int FlyFallSpeed = 12;
	public int FlyMoveSpeed = 32;
	public int FlyAcceleration = 2;
	public int FlyDeceleration = 1;
	public int FlyHeightAmount = 521;

	// Slide
	[Group("Slide")]
	public bool SlideAvailable = false;
	public bool SlideOnAnyBlock = false;
	public bool ResetJumpCountWhenSlide = true;
	public int SlideDropSpeed = 4;

	// Grab
	[Group("Grab")]
	public bool GrabTopAvailable = true;
	public bool GrabSideAvailable = true;
	public bool ResetJumpCountWhenGrab = true;
	public bool GrabFlipDownAvailable = true;
	public bool GrabFlipUpAvailable = true;
	public int GrabFlipDuration = 18;
	public int GrabMoveSpeedX = 24;
	public int GrabMoveSpeedY = 24;
	public int GrabTopHeightAmount = 947;
	public int GrabSideHeightAmount = 947;

	// API
	public void LoadToCharacter (Character character) {

		character.MovementWidth.BaseValue = MovementWidth;
		character.MovementHeight.BaseValue = MovementHeight;

		// Height Amount
		character.RollingHeightAmount.BaseValue = RollingHeightAmount;
		character.DashHeightAmount.BaseValue = DashHeightAmount;
		character.RushHeightAmount.BaseValue = RushHeightAmount;
		character.SquatHeightAmount.BaseValue = SquatHeightAmount;
		character.SwimHeightAmount.BaseValue = SwimHeightAmount;
		character.FlyHeightAmount.BaseValue = FlyHeightAmount;
		character.GrabTopHeightAmount.BaseValue = GrabTopHeightAmount;
		character.GrabSideHeightAmount.BaseValue = GrabSideHeightAmount;

		// Walk
		character.WalkSpeed.BaseValue = WalkSpeed;
		character.WalkAcceleration.BaseValue = WalkAcceleration;
		character.WalkBrakeAcceleration.BaseValue = WalkBrakeAcceleration;
		character.WalkDeceleration.BaseValue = WalkDeceleration;

		// Run
		character.WalkToRunAccumulation.BaseValue = WalkToRunAccumulation;
		character.RunSpeed.BaseValue = RunSpeed;
		character.RunAcceleration.BaseValue = RunAcceleration;
		character.RunBrakeAcceleration.BaseValue = RunBrakeAcceleration;
		character.RunDeceleration.BaseValue = RunDeceleration;

		// Push
		character.PushAvailable.BaseValue = PushAvailable;
		character.PushSpeed.BaseValue = PushSpeed;

		// Jump
		character.JumpSpeed.BaseValue = JumpSpeed;
		character.JumpCount.BaseValue = JumpCount;
		character.JumpReleaseLoseRate.BaseValue = JumpReleaseLoseRate;
		character.JumpRiseGravityRate.BaseValue = JumpRiseGravityRate;
		character.GrowJumpCountWhenFallOffEdge.BaseValue = EdgeFallGrowJumpCount;
		character.FirstJumpWithRoll.BaseValue = FirstJumpWithRoll;
		character.SubsequentJumpWithRoll.BaseValue = SubsequentJumpWithRoll;
		character.JumpDownThoughOneway.BaseValue = JumpDownThoughOneway;

		// Dash
		character.DashAvailable.BaseValue = DashAvailable;
		character.DashWithRoll.BaseValue = DashWithRoll;
		character.DashSpeed.BaseValue = DashSpeed;
		character.DashDuration.BaseValue = DashDuration;
		character.DashCooldown.BaseValue = DashCooldown;
		character.DashAcceleration.BaseValue = DashAcceleration;
		character.DashCancelLoseRate.BaseValue = DashCancelLoseRate;

		// Rush
		character.RushAvailable.BaseValue = RushAvailable;
		character.RushInAir.BaseValue = RushInAir;
		character.RushInWater.BaseValue = RushInWater;
		character.RushWhenClimb.BaseValue = RushWhenClimb;
		character.RushWhenSquat.BaseValue = RushWhenSquat;
		character.RushSpeed.BaseValue = RushSpeed;
		character.RushStopSpeed.BaseValue = RushStopSpeed;
		character.RushDuration.BaseValue = RushDuration;
		character.RushStiff.BaseValue = RushStiff;
		character.RushCooldown.BaseValue = RushCooldown;
		character.RushAcceleration.BaseValue = RushAcceleration;
		character.RushDeceleration.BaseValue = RushDeceleration;

		// Slip & Crash
		character.CrashWhenSlippy.BaseValue = CrashWhenSlippy;
		character.CrashDuration.BaseValue = CrashDuration;
		character.CrashRunDurationRequire.BaseValue = CrashRunDurationRequire;
		character.CrashDeceleration.BaseValue = CrashDeceleration;
		character.SlipAcceleration.BaseValue = SlipAcceleration;
		character.SlipDeceleration.BaseValue = SlipDeceleration;

		// Squat
		character.SquatAvailable.BaseValue = SquatAvailable;
		character.SquatSpeed.BaseValue = SquatSpeed;
		character.SquatAcceleration.BaseValue = SquatAcceleration;
		character.SquatDeceleration.BaseValue = SquatDeceleration;

		// Pound
		character.PoundAvailable.BaseValue = PoundAvailable;
		character.PoundSpeed.BaseValue = PoundSpeed;

		// Swim
		character.SwimWidth.BaseValue = SwimWidth;
		character.InWaterSpeedLoseRate.BaseValue = InWaterSpeedLoseRate;
		character.SwimSpeed.BaseValue = SwimSpeed;
		character.SwimJumpSpeed.BaseValue = SwimJumpSpeed;
		character.SwimAcceleration.BaseValue = SwimAcceleration;
		character.SwimDeceleration.BaseValue = SwimDeceleration;

		// Climb
		character.ClimbAvailable.BaseValue = ClimbAvailable;
		character.JumpWhenClimbAvailable.BaseValue = JumpWhenClimbAvailable;
		character.ClimbSpeedX.BaseValue = ClimbSpeedX;
		character.ClimbSpeedY.BaseValue = ClimbSpeedY;

		// Fly
		character.FlyCooldown.BaseValue = FlyCooldown;
		character.FlyRiseSpeed.BaseValue = FlyRiseSpeed;
		character.FlyGravityRiseRate.BaseValue = FlyGravityRiseRate;
		character.FlyGravityFallRate.BaseValue = FlyGravityFallRate;
		character.FlyFallSpeed.BaseValue = FlyFallSpeed;
		character.FlyMoveSpeed.BaseValue = FlyMoveSpeed;
		character.FlyAcceleration.BaseValue = FlyAcceleration;
		character.FlyDeceleration.BaseValue = FlyDeceleration;

		// Slide
		character.SlideAvailable.BaseValue = SlideAvailable;
		character.SlideOnAnyBlock.BaseValue = SlideOnAnyBlock;
		character.ResetJumpCountWhenSlide.BaseValue = ResetJumpCountWhenSlide;
		character.SlideDropSpeed.BaseValue = SlideDropSpeed;

		// Grab
		character.GrabTopAvailable.BaseValue = GrabTopAvailable;
		character.GrabSideAvailable.BaseValue = GrabSideAvailable;
		character.ResetJumpCountWhenGrab.BaseValue = ResetJumpCountWhenGrab;
		character.GrabFlipThroughDownAvailable.BaseValue = GrabFlipDownAvailable;
		character.GrabFlipThroughUpAvailable.BaseValue = GrabFlipUpAvailable;
		character.GrabFlipThroughDuration.BaseValue = GrabFlipDuration;
		character.GrabMoveSpeedX.BaseValue = GrabMoveSpeedX;
		character.GrabMoveSpeedY.BaseValue = GrabMoveSpeedY;

	}

}
