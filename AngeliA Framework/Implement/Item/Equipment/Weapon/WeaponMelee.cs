namespace AngeliaFramework {
	public abstract class MeleeWeapon<B> : MeleeWeapon where B : MeleeBullet {
		public MeleeWeapon () => BulletID = typeof(B).AngeHash();
	}
	public abstract class MeleeWeapon : Weapon {
		public abstract int RangeXLeft { get; }
		public abstract int RangeXRight { get; }
		public abstract int RangeY { get; }
		public MeleeWeapon () => BulletID = MeleeBullet.TYPE_ID;
		public override Bullet SpawnBullet (Character sender) {

			if (base.SpawnBullet(sender) is not MeleeBullet bullet) return null;

			// Set Range
			int rangeX = RangeXRight;
			if (!sender.FacingRight) {
				rangeX = RangeXLeft;
			}
			bullet.SetSpawnSize(rangeX, RangeY);

			// Follow
			bullet.FollowSender();

			// Smoke Particle
			if (bullet.SmokeParticleID != 0 && bullet.GroundCheck(out var tint)) {
				if (Stage.SpawnEntity(bullet.SmokeParticleID, bullet.X + bullet.Width / 2, bullet.Y) is Particle particle) {
					particle.Tint = tint;
					particle.Width = !sender.FacingRight ? -1 : 1;
					particle.Height = 1;
				}
			}

			return bullet;
		}
	}
}