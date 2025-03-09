using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace AngeliaGame;

[EntityAttribute.Capacity(1, 1)]
[EntityAttribute.StageOrder(4096)]
[EntityAttribute.SpawnWithCheatCode]
public class MeowMeowYaya : PoseCharacter, IPlayable, IActionTarget {


	// VAR
	public override int DefaultCharacterHeight => 157;

	// MSG
	public override void OnActivated () {
		base.OnActivated();


		if (Rendering is PoseCharacterRenderer pRender) {
			pRender.SpinOnGroundPound = true;
		}

		NativeMovement.JumpDownThoughOneway.BaseValue = true;
		NativeMovement.DashAvailable.BaseValue = true;
		NativeMovement.RushAvailable.BaseValue = true;
		NativeMovement.PoundAvailable.BaseValue = true;
		NativeMovement.RushInAir.BaseValue = true;
		NativeMovement.RushInWater.BaseValue = true;
		NativeMovement.SlideOnAnyBlock.BaseValue = true;
		NativeMovement.SlideAvailable.BaseValue = true;
		NativeMovement.JumpDownThoughOneway.BaseValue = true;

	}

	bool IActionTarget.Invoke () {
		if (PlayerSystem.Selecting == this) return false;
		PlayerSystem.SetCharacterAsPlayer(this);
		return true;
	}

	bool IActionTarget.AllowInvoke () => PlayerSystem.Selecting != this;

}
