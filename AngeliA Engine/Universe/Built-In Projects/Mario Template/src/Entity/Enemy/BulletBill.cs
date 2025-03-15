using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;


public class BulletBillRed : BulletBill {
	public static readonly int TYPE_ID = typeof(BulletBillRed).AngeHash();
	protected override int Speed => 36;
}


public class BulletBillBlack : BulletBill {
	public static readonly int TYPE_ID = typeof(BulletBillBlack).AngeHash();
	protected override int Speed => 22;
}


public abstract class BulletBill : Enemy {

	// VAR
	public override int CollisionMask => 0;
	protected override bool DelayPassoutOnStep => false;
	protected abstract int Speed { get; }
	public bool MovingRight { get; set; }

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		MovingRight = true;
	}

	public override void FirstUpdate () {
		FillAsTrigger(1);
		IgnoreGravity.True(1);
		IgnoreMomentum.True(1);
		base.FirstUpdate();
	}

	public override void Update () {
		base.Update();
		// Fly Movement
		VelocityX = MovingRight ? Speed : -Speed;
	}

	public override void LateUpdate () {
		base.LateUpdate();
		Renderer.Draw(TypeID, X + Width / 2, Y, 500, 0, 0, MovingRight ? Width : -Width, Height);
	}

}
