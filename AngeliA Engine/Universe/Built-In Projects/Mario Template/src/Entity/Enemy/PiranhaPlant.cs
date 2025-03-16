using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

public class PiranhaPlant : Enemy, IAutoTrackWalker {

	public override int CollisionMask => PhysicsMask.MAP;
	protected override bool AllowPlayerStepOn => false;
	protected override bool AttackOnTouchPlayer => true;
	int IAutoTrackWalker.LastWalkingFrame { get; set; }
	int IAutoTrackWalker.WalkStartFrame { get; set; }
	Direction8 IRouteWalker.CurrentDirection { get; set; }
	Int2 IRouteWalker.TargetPosition { get; set; }

	public override void LateUpdate () {
		base.LateUpdate();
		Renderer.Draw(TypeID, X + Width / 2, Y, 500, 0, 0, Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE);
	}

}
