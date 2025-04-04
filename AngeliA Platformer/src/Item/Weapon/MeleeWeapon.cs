using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Weapon that spawn invisible bullet that follows the character
/// </summary>
public abstract class MeleeWeapon : Weapon<MeleeBullet> {
	/// <summary>
	/// Bullet width in global space when attack facing left
	/// </summary>
	public abstract int RangeXLeft { get; }
	/// <summary>
	/// Bullet width in global space when attack facing right
	/// </summary>
	public abstract int RangeXRight { get; }
	/// <summary>
	/// Bullet height in global space
	/// </summary>
	public abstract int RangeY { get; }
	/// <summary>
	/// Damage amount it deals
	/// </summary>
	public virtual int Damage => 1;
	public override Bullet SpawnBullet (Character sender) {

		if (base.SpawnBullet(sender) is not MeleeBullet bullet) return null;

		// Set Range
		int rangeX = RangeXRight;
		if (!sender.Movement.FacingRight) {
			rangeX = RangeXLeft;
		}
		bullet.Width = rangeX;
		bullet.Height = RangeY;
		bullet.Damage.Override(Damage, bullet.Duration + 1);

		// Follow
		bullet.FollowSender();

		// Smoke Particle
		if (bullet.SmokeParticleID != 0 && bullet.GroundCheck(out var tint)) {
			if (Stage.SpawnEntity(bullet.SmokeParticleID, bullet.X + bullet.Width / 2, bullet.Y) is Particle particle) {
				particle.Tint = tint;
				particle.Width = !sender.Movement.FacingRight ? -1 : 1;
				particle.Height = 1;
			}
		}

		return bullet;
	}
}