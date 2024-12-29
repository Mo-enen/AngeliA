using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliA.Platformer;

public abstract class Enemy : Character, IDamageReceiver {

	// VAR
	int IDamageReceiver.Team => Const.TEAM_ENEMY;
	bool IDamageReceiver.TakeDamageFromLevel => false;

	// MSG
	public override void TakeDamage (Damage damage) {
		base.TakeDamage(damage);
		// Big Knockback



	}

}
