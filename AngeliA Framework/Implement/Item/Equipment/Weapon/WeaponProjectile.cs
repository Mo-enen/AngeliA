using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public abstract class ProjectileWeapon<B> : ProjectileWeapon where B : MovableBullet {
		public ProjectileWeapon () => BulletID = typeof(B).AngeHash();
	}
	public abstract class ProjectileWeapon : Weapon {
		protected virtual int BulletPivotY => 500;
		public override Bullet SpawnBullet (Character sender) {
			if (base.SpawnBullet(sender) is not MovableBullet bullet) return null;
			bullet.X = sender.FacingRight ? sender.Rect.xMax : sender.Rect.xMin - bullet.Width;
			bullet.Y = sender.Y + sender.Height * BulletPivotY / 1000 - bullet.Height / 2;
			bullet.Y = Util.Max(bullet.Y, sender.Y + 1);
			bullet.StartMove(sender.FacingRight);
			return bullet;
		}
	}
}