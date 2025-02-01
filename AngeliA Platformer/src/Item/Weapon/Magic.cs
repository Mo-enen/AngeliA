using System.Collections;
using System.Collections.Generic;


using AngeliA;

namespace AngeliA.Platformer;


public abstract class MagicWeapon<B> : ProjectileWeapon<B> where B : MovableBullet {
	public sealed override ToolType ToolType => ToolType.Magic;
	public override int? DefaultMovementSpeedRateOnUse => 1000;
	public override int? WalkingMovementSpeedRateOnUse => 1000;
	public override int? RunningMovementSpeedRateOnUse => 1000;
}
