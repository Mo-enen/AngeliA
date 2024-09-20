using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class MeleeBullet : Bullet {

	// Const
	public static readonly int TYPE_ID = typeof(MeleeBullet).AngeHash();

	// Api
	protected override int Duration => 10;
	protected override int Damage => 1;
	protected sealed override int EnvironmentHitCount => int.MaxValue;
	protected sealed override int ReceiverHitCount => int.MaxValue;
	public virtual int SmokeParticleID => 0;

	// MSG
	public override void Update () {
		FollowSender();
		base.Update();
	}

	// API
	public void FollowSender () {
		if (Sender is not Character character) return;
		var characterRect = character.Rect;
		X = character.Movement.FacingRight ? characterRect.xMax : characterRect.xMin - Width;
		Y = character.Y - 1;
	}

}