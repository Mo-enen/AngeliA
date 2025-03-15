using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

public class Muncher : Enemy, IAutoTrackWalker {

	public override int CollisionMask => PhysicsMask.SOLID;
	protected override bool AllowPlayerStepOn => false;
	int IAutoTrackWalker.LastWalkingFrame { get; set; }
	int IAutoTrackWalker.WalkStartFrame { get; set; }
	Direction8 IRouteWalker.CurrentDirection { get; set; }
	Int2 IRouteWalker.TargetPosition { get; set; }

	public override void LateUpdate () {
		base.LateUpdate();
		Draw();
	}

	public override void OnDamaged (Damage damage) { }

}
