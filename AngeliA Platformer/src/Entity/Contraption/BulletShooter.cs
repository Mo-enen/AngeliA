using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;


/// <summary>
/// Entity that shoot given bullet repeatedly
/// </summary>
/// <typeparam name="B">Type of the bullet</typeparam>
public abstract class BulletShooter<B> : BulletShooter where B : Bullet {
	protected override int BulletID => _BulletID;
	private int _BulletID { get; init; }
	public BulletShooter () => _BulletID = typeof(B).AngeHash();
}


/// <summary>
/// Entity that shoot given bullet repeatedly
/// </summary>
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[EntityAttribute.MapEditorGroup("Contraption")]
public abstract class BulletShooter : Entity, IBlockEntity {

	// VAR
	/// <summary>
	/// How many frame does it takes to shoot another bullet
	/// </summary>
	protected virtual int ShootFrequency => 60;
	/// <summary>
	/// Bullet starting position offset in forward of shooting direction
	/// </summary>
	protected virtual int ShootOffsetForward => Const.HALF;
	/// <summary>
	/// Bullet starting position offset in side of shooting direction
	/// </summary>
	protected virtual int ShootOffsetSide => 0;
	/// <summary>
	/// Which teams should be attack by the bullet
	/// </summary>
	protected virtual int AttackTargetTeam => Const.TEAM_ALL;
	/// <summary>
	/// Type ID of the bullet entity
	/// </summary>
	protected abstract int BulletID { get; }
	/// <summary>
	/// Which direction to shoot
	/// </summary>
	protected abstract Direction4 ShootDirection { get; }
	protected int LastShootFrame { get; private set; } = int.MinValue;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		LastShootFrame = int.MinValue;
	}

	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this);
	}

	public override void BeforeUpdate () {
		base.BeforeUpdate();
		// Auto Shoot
		if (Game.GlobalFrame > LastShootFrame + ShootFrequency) {
			Shoot();
		}
	}

	// API
	/// <summary>
	/// Perform a shoot
	/// </summary>
	public void Shoot () {
		LastShootFrame = Game.GlobalFrame;
		var forward = ShootDirection.Normal();
		var side = ShootDirection.Clockwise().Normal();
		var offset = forward * ShootOffsetForward + side * ShootOffsetSide;
		var center = Rect.CenterInt() + offset;
		if (Stage.SpawnEntity(BulletID, center.x, center.y) is Bullet bullet) {
			bullet.Sender = this;
			bullet.FailbackTargetTeam = AttackTargetTeam;
			bullet.X -= bullet.Width / 2;
			bullet.Y -= bullet.Height / 2;
			if (bullet is MovableBullet mBullet) {
				mBullet.StartMove(ShootDirection.ToDirection8(), mBullet.SpeedForward, mBullet.SpeedSide);
			}
			OnBulletShoot(bullet);
		}
	}

	/// <summary>
	/// This function is called when a bullet shoot
	/// </summary>
	protected virtual void OnBulletShoot (Bullet bullet) { }

}
