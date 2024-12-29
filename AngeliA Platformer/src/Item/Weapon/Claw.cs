using System.Collections;
using System.Collections.Generic;

using AngeliA;namespace AngeliA.Platformer;


public abstract class Claw : MeleeWeapon {
	public sealed override ToolType ToolType => ToolType.Claw;
	public sealed override ToolHandheld Handheld => ToolHandheld.OneOnEachHand;
	public override int Duration => 10;
	public override int Cooldown => 0;
	public override int? DefaultMovementSpeedRateOnUse => 1000;
	public override int? WalkingMovementSpeedRateOnUse => 1000;
	public override int? RunningMovementSpeedRateOnUse => 1000;
	public override int RangeXLeft => 375;
	public override int RangeXRight => 375;
	public override int RangeY => 432;
}
