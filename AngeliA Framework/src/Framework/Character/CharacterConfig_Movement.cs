using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace AngeliA;


[StructLayout(LayoutKind.Sequential)]
public class CharacterMovementConfig {

	[PropGroup("Move")]
	public int MovementWidth = 150;
	public int MovementHeight = 384; // Global Height when Character is 160cm

	// Walk
	[PropSeparator]
	public int WalkSpeed = 20;
	public int WalkAcceleration = 3;
	public int WalkBrakeAcceleration = 30;
	public int WalkDeceleration = 4;
	public int WalkToRunAccumulation = 0;

	// Run
	public int RunSpeed = 32;
	public int RunAcceleration = 3;
	public int RunBrakeAcceleration = 30;
	public int RunDeceleration = 4;

	[PropGroup("Squat")]
	public bool SquatAvailable = true;
	[PropVisibility(nameof(SquatAvailable))] public int SquatSpeed = 14;
	[PropVisibility(nameof(SquatAvailable))] public int SquatAcceleration = 48;
	[PropVisibility(nameof(SquatAvailable))] public int SquatDeceleration = 48;
	[PropVisibility(nameof(SquatAvailable))] public int SquatHeightAmount = 521;

	[PropGroup("Jump")]
	public int JumpCount = 2;
	[PropVisibility(nameof(JumpCount), CompareMode.GreaterThan, 0)] public int JumpSpeed = 73;
	[PropVisibility(nameof(JumpCount), CompareMode.GreaterThan, 0)] public int JumpReleaseLoseRate = 700;
	[PropVisibility(nameof(JumpCount), CompareMode.GreaterThan, 0)] public int JumpRiseGravityRate = 600;
	[PropVisibility(nameof(JumpCount), CompareMode.GreaterThan, 0)] public bool EdgeFallGrowJumpCount = true;
	[PropVisibility(nameof(JumpCount), CompareMode.GreaterThan, 0)] public bool FirstJumpWithRoll = false;
	[PropVisibility(nameof(JumpCount), CompareMode.GreaterThan, 0)] public bool SubsequentJumpWithRoll = true;
	public bool JumpDownThoughOneway = false;

	[PropGroup("Dash")]
	public bool DashAvailable = true;
	[PropVisibility(nameof(DashAvailable))] public bool DashWithRoll = false;
	[PropVisibility(nameof(DashAvailable))] public int DashSpeed = 42;
	[PropVisibility(nameof(DashAvailable))] public int DashDuration = 20;
	[PropVisibility(nameof(DashAvailable))] public int DashCooldown = 4;
	[PropVisibility(nameof(DashAvailable))] public int DashAcceleration = 24;
	[PropVisibility(nameof(DashAvailable))] public int DashCancelLoseRate = 300;
	[PropVisibility(nameof(DashAvailable))] public int DashHeightAmount = 521;

	[PropGroup("Rush")]
	public bool RushAvailable = true;
	[PropVisibility(nameof(RushAvailable))] public bool RushInAir = false;
	[PropVisibility(nameof(RushAvailable))] public bool RushInWater = true;
	[PropVisibility(nameof(RushAvailable))] public bool RushWhenClimb = false;
	[PropVisibility(nameof(RushAvailable))] public bool RushWhenSquat = false;
	[PropVisibility(nameof(RushAvailable))] public int RushSpeed = 72;
	[PropVisibility(nameof(RushAvailable))] public int RushStopSpeed = 8;
	[PropVisibility(nameof(RushAvailable))] public int RushDuration = 8;
	[PropVisibility(nameof(RushAvailable))] public int RushStiff = 10;
	[PropVisibility(nameof(RushAvailable))] public int RushCooldown = 2;
	[PropVisibility(nameof(RushAvailable))] public int RushAcceleration = 12;
	[PropVisibility(nameof(RushAvailable))] public int RushDeceleration = 4;
	[PropVisibility(nameof(RushAvailable))] public int RushHeightAmount = 1000;

	[PropGroup("Crash")]
	public bool CrashAvailable = true;
	[PropVisibility(nameof(CrashAvailable))] public bool CrashWhenSlippy = true;
	[PropVisibility(nameof(CrashAvailable))] public int CrashDuration = 30;
	[PropVisibility(nameof(CrashAvailable))] public int CrashRunDurationRequire = 42;
	[PropVisibility(nameof(CrashAvailable))] public int CrashDeceleration = 1;

	[PropGroup("Slip")]
	public bool SlipAvailable = true;
	[PropVisibility(nameof(SlipAvailable))] public int SlipAcceleration = 2;
	[PropVisibility(nameof(SlipAvailable))] public int SlipDeceleration = 1;

	[PropGroup("Push")]
	public bool PushAvailable = true;
	public bool PoundPutoutFire = true;
	[PropVisibility(nameof(PushAvailable))] public int PushSpeed = 10;

	[PropGroup("Pound")]
	public bool PoundAvailable = true;
	[PropVisibility(nameof(PoundAvailable))] public int PoundSpeed = 96;

	[PropGroup("Swim")]
	public int SwimWidth = 200;
	public int InWaterSpeedLoseRate = 500;
	public int SwimSpeed = 42;
	public int SwimJumpSpeed = 128;
	public int SwimAcceleration = 4;
	public int SwimDeceleration = 4;
	public int SwimHeightAmount = 1000;

	[PropGroup("Climb")]
	public bool ClimbAvailable = true;
	[PropVisibility(nameof(ClimbAvailable))] public bool JumpWhenClimbAvailable = true;
	[PropVisibility(nameof(ClimbAvailable))] public int ClimbSpeedX = 12;
	[PropVisibility(nameof(ClimbAvailable))] public int ClimbSpeedY = 18;

	[PropGroup("Fly")]
	public bool FlyAvailable = true;
	[PropVisibility(nameof(FlyAvailable))] public bool GlideOnFlying = false;
	[PropVisibility(nameof(FlyAvailable))] public int FlyCooldown = 24;
	[PropVisibility(nameof(FlyAvailable))] public int FlyRiseSpeed = 96;
	[PropVisibility(nameof(FlyAvailable))] public int FlyGravityRiseRate = 800;
	[PropVisibility(nameof(FlyAvailable))] public int FlyGravityFallRate = 200;
	[PropVisibility(nameof(FlyAvailable))] public int FlyFallSpeed = 12;
	[PropVisibility(nameof(FlyAvailable))] public int FlyMoveSpeed = 32;
	[PropVisibility(nameof(FlyAvailable))] public int FlyAcceleration = 2;
	[PropVisibility(nameof(FlyAvailable))] public int FlyDeceleration = 1;
	[PropVisibility(nameof(FlyAvailable))] public int FlyHeightAmount = 521;

	[PropGroup("Slide")]
	public bool SlideAvailable = false;
	[PropVisibility(nameof(SlideAvailable))] public bool SlideOnAnyBlock = false;
	[PropVisibility(nameof(SlideAvailable))] public bool ResetJumpCountWhenSlide = true;
	[PropVisibility(nameof(SlideAvailable))] public int SlideDropSpeed = 4;

	[PropGroup("Grab")]
	public bool GrabTopAvailable = true;
	public bool GrabSideAvailable = true;
	[PropVisibility(nameof(GrabTopAvailable), CompareMode.Or, nameof(GrabSideAvailable))] public bool ResetJumpCountWhenGrab = true;
	[PropVisibility(nameof(GrabTopAvailable))] public bool GrabFlipDownAvailable = true;
	[PropVisibility(nameof(GrabTopAvailable))] public bool GrabFlipUpAvailable = true;
	[PropVisibility(nameof(GrabTopAvailable))] public int GrabFlipDuration = 18;
	[PropVisibility(nameof(GrabTopAvailable))] public int GrabMoveSpeedX = 24;
	[PropVisibility(nameof(GrabSideAvailable))] public int GrabMoveSpeedY = 24;
	[PropVisibility(nameof(GrabTopAvailable))] public int GrabTopHeightAmount = 947;
	[PropVisibility(nameof(GrabSideAvailable))] public int GrabSideHeightAmount = 947;


	// API
	public void LoadToCharacter (Character character) {

		character.MovementWidth.BaseValue = MovementWidth;
		character.MovementHeight.BaseValue = MovementHeight;

		// Height Amount
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

		// Crash
		character.CrashAvailable.BaseValue = CrashAvailable;
		character.CrashWhenSlippy.BaseValue = CrashWhenSlippy;
		character.CrashDuration.BaseValue = CrashDuration;
		character.CrashRunDurationRequire.BaseValue = CrashRunDurationRequire;
		character.CrashDeceleration.BaseValue = CrashDeceleration;

		// Slip
		character.SlipAvailable.BaseValue = SlipAvailable;
		character.SlipAcceleration.BaseValue = SlipAcceleration;
		character.SlipDeceleration.BaseValue = SlipDeceleration;

		// Squat
		character.SquatAvailable.BaseValue = SquatAvailable;
		character.SquatSpeed.BaseValue = SquatSpeed;
		character.SquatAcceleration.BaseValue = SquatAcceleration;
		character.SquatDeceleration.BaseValue = SquatDeceleration;

		// Pound
		character.PoundAvailable.BaseValue = PoundAvailable;
		character.PoundPutoutFire.BaseValue = PoundPutoutFire;
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
		character.FlyAvailable.BaseValue = FlyAvailable;
		character.GlideOnFlying.BaseValue = GlideOnFlying;
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
