using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public partial class CharacterHealth {

	public readonly BuffInt MaxHP = new(1);
	public readonly BuffInt InvincibleDuration = new(120);
	public readonly BuffInt DamageStunDuration = new(24);
	public readonly BuffInt KnockBackSpeed = new(64);
	public readonly BuffInt KnockbackDeceleration = new(16);
	public readonly BuffBool InvincibleOnDash = new(false);
	public readonly BuffBool InvincibleOnRush = new(false);

}
