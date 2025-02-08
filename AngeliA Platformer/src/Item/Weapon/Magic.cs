using System.Collections;
using System.Collections.Generic;


using AngeliA;

namespace AngeliA.Platformer;


public abstract class MagicWeapon<B> : ProjectileWeapon<B> where B : MovableBullet {

	public override int? DefaultMovementSpeedRateOnUse => 1000;
	public override int? WalkingMovementSpeedRateOnUse => 1000;
	public override int? RunningMovementSpeedRateOnUse => 1000;

	public override void OnCharacterAttack_FromEquipment (Character character, Bullet bullet) {
		base.OnCharacterAttack_FromEquipment(character, bullet);

		// Face Expression
		if (character.Rendering is PoseCharacterRenderer pRendering) {
			pRendering.ForceFaceExpressionIndex.Override(
				(int)CharacterFaceExpression.Normal, Duration - PerformDelayFrame
			);
		}

	}

}
