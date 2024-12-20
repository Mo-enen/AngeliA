
using AngeliA;
namespace AngeliA.Platformer;


public abstract class Polearm : MeleeWeapon {
	public sealed override ToolType ToolType => ToolType.Polearm;
	public sealed override ToolHandheld Handheld => ToolHandheld.Pole;
	public override bool IgnoreGrabTwist => true;
	public override int Duration => 18;
	public override int Cooldown => 2;
	public override int RangeXLeft => 384;
	public override int RangeXRight => 384;
	public override int RangeY => 432;

}
