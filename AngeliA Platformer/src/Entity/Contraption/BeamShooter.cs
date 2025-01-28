using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class BeamShooter : Entity {

	// VAR
	protected virtual int ShootFrequency => 60;
	protected virtual int ShootOffsetForward => Const.HALF;
	protected virtual int ShootOffsetSide => 0;
	protected abstract int BeamBulletID { get; }
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
		if (Stage.SpawnEntity(BeamBulletID, center.x, center.y) is BeamBullet beam) {
			beam.Sender = this;
			beam.X -= beam.Width / 2;
			beam.Y -= beam.Height / 2;
			beam.StartMove(ShootDirection.ToDirection8(), beam.SpeedForward, beam.SpeedSide);
		}
	}

}
