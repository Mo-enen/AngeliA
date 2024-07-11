using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA;

public abstract class ProjectileWeapon<B> : Weapon<B> where B : MovableBullet {
	protected virtual int BulletPivotY => 500;
	protected virtual int AdditionalBulletSpeedX => 0;
	protected virtual int AdditionalBulletSpeedY => 0;
	public override Bullet SpawnBullet (Character sender) {
		if (base.SpawnBullet(sender) is not MovableBullet bullet) return null;
		bullet.X = sender.FacingRight ? sender.Rect.xMax : sender.Rect.xMin - bullet.Width;
		bullet.Y = sender.Y + sender.Height * BulletPivotY / 1000 - bullet.Height / 2;
		bullet.Y = Util.Max(bullet.Y, sender.Y + 1);
		bullet.StartMove(sender.FacingRight, AdditionalBulletSpeedX, AdditionalBulletSpeedY);
		return bullet;
	}
}
