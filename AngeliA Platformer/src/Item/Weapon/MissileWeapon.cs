using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliA.Platformer;

public abstract class MissileWeapon<B> : Weapon<B> where B : MissileBullet {

	public override ToolType ToolType => ToolType.Ranged;
	public override ToolHandheld Handheld => ToolHandheld.Shooting;
	protected override WeaponValidDirection ValidDirection => WeaponValidDirection.Two;
	public override int Duration => 16;
	public override int Cooldown => 32;
	public override int? DefaultMovementSpeedRateOnUse => 1000;
	public override int? WalkingMovementSpeedRateOnUse => 1000;
	public override int? RunningMovementSpeedRateOnUse => 1000;
	public override bool AvailableInAir => true;
	public override bool AvailableInWater => true;
	public override bool AvailableWhenWalking => true;
	public override bool AvailableWhenRunning => true;
	public override bool AvailableWhenClimbing => true;
	public override bool AvailableWhenFlying => true;
	public override bool AvailableWhenRolling => true;
	public override bool AvailableWhenSquatting => true;
	public override bool AvailableWhenDashing => true;
	public override bool AvailableWhenSliding => true;
	public override bool AvailableWhenGrabbing => true;
	public override bool AvailableWhenRushing => true;
	public override bool AvailableWhenPounding => true;

	// MSG
	public override void BeforeItemUpdate_FromEquipment (Character holder) {
		base.BeforeItemUpdate_FromEquipment(holder);
		var attackness = holder.Attackness;
		attackness.IgnoreAimingDirection(Direction8.Bottom);
		attackness.IgnoreAimingDirection(Direction8.Top);
		attackness.IgnoreAimingDirection(Direction8.TopLeft);
		attackness.IgnoreAimingDirection(Direction8.TopRight);
		attackness.IgnoreAimingDirection(Direction8.BottomLeft);
		attackness.IgnoreAimingDirection(Direction8.BottomRight);
	}

	public override Bullet SpawnBullet (Character sender) {
		var bullet = base.SpawnBullet(sender);
		// Shift Bullet Position
		if (bullet != null) {
			bullet.X += sender.FacingRight ? Const.CEL : -Const.CEL;
		}
		return bullet;
	}

}