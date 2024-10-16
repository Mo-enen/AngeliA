using System.Collections;
using System.Collections.Generic;

using AngeliA;
namespace AngeliA.Platformer;


public abstract class Hammer : MeleeWeapon {
	public sealed override ToolType ToolType => ToolType.Hammer;
	public override ToolHandheld Handheld => ToolHandheld.SingleHanded;
	public override bool IgnoreGrabTwist => true;
	public override int RangeXLeft => 275;
	public override int RangeXRight => 275;
	public override int RangeY => 432;

}
