using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public partial class CharacterHealth {

	public readonly FrameBasedInt MaxHP = new(1);
	public readonly FrameBasedInt InvincibleDuration = new(120);
	public readonly FrameBasedInt DamageStunDuration = new(24);
	public readonly FrameBasedInt KnockBackSpeed = new(64);
	public readonly FrameBasedInt KnockbackDeceleration = new(16);
	public readonly FrameBasedBool InvincibleOnDash = new(false);
	public readonly FrameBasedBool InvincibleOnRush = new(false);

}
