using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


public abstract class MagicWeapon<B> : ProjectileWeapon<B> where B : MovableBullet {
	public sealed override WeaponType WeaponType => WeaponType.Magic;
	public override int? DefaultSpeedRateOnAttack => 1000;
	public override int? WalkingSpeedRateOnAttack => 1000;
	public override int? RunningSpeedRateOnAttack => 1000;
}
