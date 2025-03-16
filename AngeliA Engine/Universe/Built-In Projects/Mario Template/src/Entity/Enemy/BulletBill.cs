using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;


public class BulletBillRedLeft : BulletBill {
	public static readonly int TYPE_ID = typeof(BulletBillRedLeft).AngeHash();
	protected override int Speed => -36;
}


public class BulletBillBlackLeft : BulletBill {
	public static readonly int TYPE_ID = typeof(BulletBillBlackLeft).AngeHash();
	protected override int Speed => -22;
}


public class BulletBillRedRight : BulletBill {
	public static readonly int TYPE_ID = typeof(BulletBillRedRight).AngeHash();
	protected override int Speed => 36;
}


public class BulletBillBlackRight : BulletBill {
	public static readonly int TYPE_ID = typeof(BulletBillBlackRight).AngeHash();
	protected override int Speed => 22;
}


public abstract class BulletBill : Enemy {

	// VAR
	public override int CollisionMask => 0;
	protected override bool DelayPassoutOnStep => false;
	protected override bool AllowPlayerStepOn => true;
	protected override bool AttackOnTouchPlayer => true;
	protected abstract int Speed { get; }
	public bool MovingRight { get; set; }

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		MovingRight = Speed > 0;
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
		int speed = Speed.Abs();
		VelocityX = MovingRight ? speed : -speed;
	}

	public override void LateUpdate () {
		base.LateUpdate();
		Renderer.Draw(TypeID, X + Width / 2, Y, 500, 0, 0, MovingRight == Speed > 0 ? Width : -Width, Height);
	}

}
