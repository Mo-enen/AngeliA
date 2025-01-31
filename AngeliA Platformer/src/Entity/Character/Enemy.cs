using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliA.Platformer;

[EntityAttribute.SpawnWithCheatCode]
[EntityAttribute.RepositionWhenInactive]
public abstract class Enemy : Character {

	// VAR
	public override int Team => Const.TEAM_ENEMY;
	public override int AttackTargetTeam => Const.TEAM_PLAYER | Const.TEAM_ENVIRONMENT;
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
			Physics.FillBlock(PhysicsLayer.DAMAGE, TypeID, Rect, tag: Tag.PhysicalDamage);
		}
	}

	public override void BeforeUpdate () {
		base.BeforeUpdate();
		IgnoreDamageFromLevel(1);
	}

	public override void OnDamaged (Damage damage) {
		base.OnDamaged(damage);


	}

}
