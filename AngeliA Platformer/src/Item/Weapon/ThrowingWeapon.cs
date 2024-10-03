using System.Collections;
using System.Collections.Generic;

using AngeliA;namespace AngeliA.Platformer;

public abstract class ThrowingWeapon<B> : ProjectileWeapon<B> where B : MovableBullet {
	public sealed override WeaponType WeaponType => WeaponType.Throwing;
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	public override int MaxStackCount => 128;
	public override Bullet SpawnBullet (Character sender) {
		var bullet = base.SpawnBullet(sender);
		if (bullet != null) {
			Inventory.ReduceEquipmentCount(sender.InventoryID, 1, EquipmentType.Weapon);
		}
		return bullet;
	}
}
