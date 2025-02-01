using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;


public abstract class BulletShooter<B> : BulletShooter where B : Bullet {
	protected override int BulletID => _BulletID;
	private int _BulletID { get; init; }
	public BulletShooter () => _BulletID = typeof(B).AngeHash();
}


[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[EntityAttribute.MapEditorGroup("Contraption")]
public abstract class BulletShooter : Entity, IBlockEntity {

	// VAR
	protected virtual int ShootFrequency => 60;
	protected virtual int ShootOffsetForward => Const.HALF;
	protected virtual int ShootOffsetSide => 0;
	protected virtual int AttackTargetTeam => Const.TEAM_ALL;
	protected abstract int BulletID { get; }
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

	protected virtual void OnBulletShoot (Bullet bullet) { }

}
