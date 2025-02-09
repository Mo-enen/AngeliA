using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace AngeliA.Platformer;

public abstract class HealWeapon<B> : Weapon<B> where B : HealBullet {

	// VAR
	protected virtual int AutoAttackFrequency => 0;
	protected virtual bool AllowManuallyAttack => true;

	// MSG
	public override void BeforeItemUpdate_FromEquipment (Character holder) {
		base.BeforeItemUpdate_FromEquipment(holder);
		// Auto Attack
		if (AutoAttackFrequency > 0 && Game.GlobalFrame % AutoAttackFrequency == 0) {
			SpawnBullet(holder);
		}
	}
	public override Bullet SpawnBullet (Character sender) {
		if (!AllowManuallyAttack) return null;
		var bullet = base.SpawnBullet(sender);
		// Shift Bullet Position
		if (bullet != null) {
			bullet.X += sender.FacingRight ? Const.CEL : -Const.CEL;
		}
		return bullet;
	}

}
