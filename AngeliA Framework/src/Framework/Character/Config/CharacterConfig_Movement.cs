using System.Collections.Generic;
using System.Reflection;
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
	public bool WalkAvailable = true;
	[PropVisibility(nameof(WalkAvailable))] public int WalkSpeed = 20;
	[PropVisibility(nameof(WalkAvailable))] public int WalkAcceleration = 3;
	[PropVisibility(nameof(WalkAvailable))] public int WalkBrakeAcceleration = 30;
	[PropVisibility(nameof(WalkAvailable))] public int WalkDeceleration = 4;
	[PropVisibility(nameof(WalkAvailable))] public int WalkToRunAccumulation = 0;

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
	[PropVisibility(nameof(RushAvailable))] public bool DashPutoutFire = true;

	[PropGroup("Rush")]
	public bool RushAvailable = true;
	[PropVisibility(nameof(RushAvailable))] public bool RushInAir = false;
	[PropVisibility(nameof(RushAvailable))] public bool RushInWater = true;
	[PropVisibility(nameof(RushAvailable))] public bool RushWhenClimb = false;
	[PropVisibility(nameof(RushAvailable))] public bool RushWhenSquat = false;
	[PropVisibility(nameof(RushAvailable))] public bool RushPutoutFire = true;
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
	[PropVisibility(nameof(SlideAvailable))] public int SlideSideJumpSpeed = 56;
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
	[PropVisibility(nameof(GrabSideAvailable))] public int GrabSideJumpSpeed = 56;

	// Data
	private static readonly List<(FieldInfo movement, FieldInfo config)> MetaMapper = [];
	private static FieldInfo BuffIntBaseValueField;
	private static FieldInfo BuffBoolBaseValueField;


	// MSG
	[OnGameInitialize]
	private static void OnGameInitialize () {
		MetaMapper.Clear();
		var typeM = typeof(CharacterMovement);
		var typeC = typeof(CharacterMovementConfig);
		var intType = typeof(int);
		var boolType = typeof(bool);
		foreach (var field in typeM.ForAllFields<FrameBasedInt>(BindingFlags.Public | BindingFlags.Instance)) {
			if (
				typeC.GetField(field.Name, BindingFlags.Public | BindingFlags.Instance) is not FieldInfo cField ||
				cField.FieldType != intType
			) continue;
			MetaMapper.Add((field, cField));
		}
		foreach (var field in typeM.ForAllFields<FrameBasedBool>(BindingFlags.Public | BindingFlags.Instance)) {
			if (
				typeC.GetField(field.Name, BindingFlags.Public | BindingFlags.Instance) is not FieldInfo cField ||
				cField.FieldType != boolType
			) continue;
			MetaMapper.Add((field, cField));
		}
		BuffIntBaseValueField = typeof(FrameBasedInt).GetField("BaseValue", BindingFlags.Public | BindingFlags.Instance);
		BuffBoolBaseValueField = typeof(FrameBasedBool).GetField("BaseValue", BindingFlags.Public | BindingFlags.Instance);
	}


	// API
	public void LoadToCharacter (Character character) {
		foreach (var (mField, cField) in MetaMapper) {
			object objValue = mField.GetValue(character.NativeMovement);
			if (objValue is FrameBasedInt buffInt) {
				if (cField.GetValue(this) is int intValue) {
					buffInt.BaseValue = intValue;
				}
			} else if (objValue is FrameBasedBool buffBool) {
				if (cField.GetValue(this) is bool boolValue) {
					buffBool.BaseValue = boolValue;
				}
			}
		}
	}

}
