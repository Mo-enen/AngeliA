using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public abstract class ProjectileWeapon<B> : ProjectileWeapon where B : MovableBullet {
		public ProjectileWeapon () => BulletID = typeof(B).AngeHash();
	}


	public abstract class ProjectileWeapon : Weapon {
		public override Bullet SpawnBullet (Character sender) {
			if (base.SpawnBullet(sender) is not MovableBullet bullet) return null;
			bullet.X = sender.FacingRight ? sender.X : sender.X - bullet.Width;
			bullet.Y = sender.Y + sender.Height / 2;
			bullet.StartMove(sender.FacingRight);
			return bullet;
		}
	}
}