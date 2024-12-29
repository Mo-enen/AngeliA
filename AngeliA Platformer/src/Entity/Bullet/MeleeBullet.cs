using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

public class MeleeBullet : Bullet {

	// Const
	public static readonly int TYPE_ID = typeof(MeleeBullet).AngeHash();

	// Api
	public override int Duration => 5;
	protected sealed override int EnvironmentHitCount => int.MaxValue;
	protected override int ReceiverHitCount => 4;
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
		X = character.Movement.FacingRight ? characterRect.xMin : characterRect.xMax - Width;
		Y = character.Y - 1;
	}

}