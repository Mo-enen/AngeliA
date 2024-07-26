using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


public abstract class MagicWeapon<B> : ProjectileWeapon<B> where B : MovableBullet {
	public sealed override WeaponType WeaponType => WeaponType.Magic;
	public override int? DefaultSpeedLoseOnAttack => 1000;
	public override int? WalkingSpeedLoseOnAttack => 1000;
	public override int? RunningSpeedLoseOnAttack => 1000;
}
