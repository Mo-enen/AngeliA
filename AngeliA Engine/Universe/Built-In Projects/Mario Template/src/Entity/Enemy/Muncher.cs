using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

public class Muncher : Enemy, IAutoTrackWalker {

	// VAR
	public override int PhysicalLayer => PhysicsLayer.ENVIRONMENT;
	public override int SelfCollisionMask => PhysicsMask.MAP;
	protected override bool AllowPlayerStepOn => false;
	protected override bool AttackOnTouchPlayer => true;
	int IAutoTrackWalker.LastWalkingFrame { get; set; }
	int IAutoTrackWalker.WalkStartFrame { get; set; }
	Direction8 IRouteWalker.CurrentDirection { get; set; }
	Int2 IRouteWalker.TargetPosition { get; set; }

	// MSG
	public override void LateUpdate () {
		base.LateUpdate();
		Draw();
	}

	public override void OnDamaged (Damage damage) {
		if (!damage.Type.HasAll(Tag.MagicalDamage)) return;
		base.OnDamaged(damage);
	}

}
