using System.Collections;
using System.Collections.Generic;


using AngeliA;

namespace AngeliA.Platformer;


public abstract class Axe : MeleeWeapon {
	public override ToolHandheld Handheld => ToolHandheld.SingleHanded;
	public sealed override ToolType ToolType => ToolType.Axe;
	public override int Duration => 12;
	public override int Cooldown => 2;
	public override int RangeXLeft => 275;
	public override int RangeXRight => 384;
	public override int RangeY => 500;
	public override int Damage => 5;
}
