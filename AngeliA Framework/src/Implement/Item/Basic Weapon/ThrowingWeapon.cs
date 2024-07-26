using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class ThrowingWeapon<B> : ProjectileWeapon<B> where B : MovableBullet {
	public sealed override WeaponType WeaponType => WeaponType.Throwing;
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	public override int MaxStackCount => 128;
	public override Bullet SpawnBullet (Character sender) {
		var bullet = base.SpawnBullet(sender);
		if (bullet != null) {
			int eqID = Inventory.GetEquipment(sender.TypeID, EquipmentType.Weapon, out int eqCount);
			if (eqID != 0) {
				int newEqCount = (eqCount - BulletCountInOneShot).GreaterOrEquelThanZero();
				if (newEqCount == 0) eqID = 0;
				Inventory.SetEquipment(sender.TypeID, EquipmentType.Weapon, eqID, newEqCount);
			}
		}
		return bullet;
	}
}
