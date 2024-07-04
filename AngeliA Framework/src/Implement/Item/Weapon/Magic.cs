using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


public abstract class MagicWeapon<B> : ProjectileWeapon<B> where B : MovableBullet {
	public sealed override WeaponType WeaponType => WeaponType.Magic;
}
