using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[EntityAttribute.Capacity(1, 1)]
[EntityAttribute.StageOrder(4096)]
[EntityAttribute.SpawnWithCheatCode]
public class MeowMeowYaya : PlayableCharacter {


	// VAR
	public override int DefaultCharacterHeight => 157;
	protected override bool UseMarioStyleMovement => false;

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

}
