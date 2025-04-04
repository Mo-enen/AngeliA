using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace AngeliA.Platformer;

/// <summary>
/// Weapon that shoot bullet to heal friendly forces
/// </summary>
/// <typeparam name="B">Type of bullet to spawn</typeparam>
public abstract class HealWeapon<B> : Weapon<B> where B : HealBullet {

	// VAR
	/// <summary>
	/// Frames it takes to auto attack again. Set to 0 for no auto attack
	/// </summary>
	protected virtual int AutoAttackFrequency => 0;
	/// <summary>
	/// True if this weapon allows manually attack from character
	/// </summary>
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
