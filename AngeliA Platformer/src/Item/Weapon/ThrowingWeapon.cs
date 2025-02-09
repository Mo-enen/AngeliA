using System.Collections;
using System.Collections.Generic;

using AngeliA;
namespace AngeliA.Platformer;

public abstract class ThrowingWeapon<B> : ProjectileWeapon<B> where B : MovableBullet {
	public override int MaxStackCount => 128;
	public override int HandheldPoseAnimationID => PoseHandheld_Throwing.TYPE_ID;
	public override int PerformPoseAnimationID => PoseAttack_WaveSingleHanded_SmashOnly.TYPE_ID;
	public override Bullet SpawnBullet (Character sender) {
		var bullet = base.SpawnBullet(sender);
		if (bullet != null) {
			Inventory.ReduceEquipmentCount(sender.InventoryID, 1, EquipmentType.HandTool);
		}
		return bullet;
	}
}
