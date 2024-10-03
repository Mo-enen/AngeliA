using System.Collections;
using System.Collections.Generic;

using AngeliA;namespace AngeliA.Platformer;

public class RideableMovement : VehicleMovement {
	public RideableMovement (Rigidbody rig) : base(rig) { }
	protected override void InitializeMeta () {
		base.InitializeMeta();
		WalkAvailable.BaseValue = true;
		JumpCount.BaseValue = 1;
		DashWithRoll.BaseValue = false;
		FirstJumpWithRoll.BaseValue = false;
		SubsequentJumpWithRoll.BaseValue = false;
		GrowJumpCountWhenFallOffEdge.BaseValue = false;
	}

}
