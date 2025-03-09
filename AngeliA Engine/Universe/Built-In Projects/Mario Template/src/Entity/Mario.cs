using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[EntityAttribute.Capacity(1, 1)]
[EntityAttribute.StageOrder(4096)]
[EntityAttribute.SpawnWithCheatCode]
[CharacterAttribute.DefaultSelectedPlayer]
[EntityAttribute.UpdateOutOfRange]
public class Mario : PoseCharacter, IPlayable, IActionTarget {

	// MSG
	public override void OnActivated () {
		base.OnActivated();

		// Movement
		NativeMovement.RunAvailable.BaseValue = true;
		NativeMovement.RunSpeed.BaseValue = 48;
		NativeMovement.RunAcceleration.BaseValue = 1;
		NativeMovement.RunDeceleration.BaseValue = 2;
		NativeMovement.RunBrakeAcceleration.BaseValue = 3;

		NativeMovement.JumpCount.BaseValue = 1;
		NativeMovement.JumpSpeed.BaseValue = 80;
		NativeMovement.JumpReleaseSpeedRate.BaseValue = 700;
		NativeMovement.JumpRiseGravityRate.BaseValue = 600;
		NativeMovement.FirstJumpWithRoll.BaseValue = false;

		NativeMovement.SquatAvailable.BaseValue = true;
		NativeMovement.SquatHeightAmount.BaseValue = 521;
		NativeMovement.SquatMoveSpeed.BaseValue = 0;
		NativeMovement.SquatAcceleration.BaseValue = 1;
		NativeMovement.SquatDeceleration.BaseValue = 1;

		NativeMovement.SlipAvailable.BaseValue = true;
		NativeMovement.SlipAcceleration.BaseValue = 1;
		NativeMovement.SlipAcceleration.BaseValue = 1;

		NativeMovement.SwimAvailable.BaseValue = true;
		NativeMovement.InWaterSpeedRate.BaseValue = 500;
		NativeMovement.SwimWidthAmount.BaseValue = 1333;
		NativeMovement.SwimHeightAmount.BaseValue = 1000;
		NativeMovement.SwimSpeed.BaseValue = 42;
		NativeMovement.SwimJumpSpeed.BaseValue = 128;
		NativeMovement.SwimAcceleration.BaseValue = 2;
		NativeMovement.SwimDeceleration.BaseValue = 2;

		NativeMovement.ClimbAvailable.BaseValue = true;
		NativeMovement.AllowJumpWhenClimbing.BaseValue = true;
		NativeMovement.ClimbSpeedX.BaseValue = 12;
		NativeMovement.ClimbSpeedY.BaseValue = 18;

		NativeMovement.WalkAvailable.BaseValue = false;
		NativeMovement.DashAvailable.BaseValue = false;
		NativeMovement.RushAvailable.BaseValue = false;
		NativeMovement.PoundAvailable.BaseValue = false;
		NativeMovement.FlyAvailable.BaseValue = false;
		NativeMovement.SlideAvailable.BaseValue = false;
		NativeMovement.GrabTopAvailable.BaseValue = false;
		NativeMovement.GrabSideAvailable.BaseValue = false;
		NativeMovement.CrashAvailable.BaseValue = false;
		NativeMovement.PushAvailable.BaseValue = false;

	}

	bool IActionTarget.Invoke () {
		if (PlayerSystem.Selecting == this) return false;
		PlayerSystem.SetCharacterAsPlayer(this);
		return true;
	}

	bool IActionTarget.AllowInvoke () => PlayerSystem.Selecting != this;

}
