using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliA.Platformer;

[EntityAttribute.SpawnWithCheatCode]
public abstract class Enemy : Character, IDamageReceiver {

	// VAR
	int IDamageReceiver.Team => Const.TEAM_ENEMY;
	bool IDamageReceiver.TakeDamageFromLevel => false;
	protected virtual bool DamageOnTouch => true;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		Movement.MovementWidth.BaseValue = 256;
		Movement.MovementHeight.BaseValue = 256;
		Health.MaxHP.BaseValue = Health.HP = 3;
		Health.InvincibleDuration.BaseValue = 24;
		Health.KnockbackSpeed.BaseValue = 48;
		Health.KnockbackDeceleration.BaseValue = 4;
	}

	public override void FirstUpdate () {
		base.FirstUpdate();
		if (DamageOnTouch && CharacterState == CharacterState.GamePlay && !Health.TakingDamage) {
			Physics.FillBlock(PhysicsLayer.DAMAGE, TypeID, Rect);
		}
	}

	public override void TakeDamage (Damage damage) {
		base.TakeDamage(damage);


	}

}
