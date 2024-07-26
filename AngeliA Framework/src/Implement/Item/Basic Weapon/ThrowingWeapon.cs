using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AngeliA;


public abstract class ThrowingWeapon<B> : ProjectileWeapon<B> where B : MovableBullet {
	public sealed override WeaponType WeaponType => WeaponType.Throwing;
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
}

