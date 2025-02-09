
using AngeliA;
namespace AngeliA.Platformer;


public abstract class Polearm : MeleeWeapon {
	public override bool IgnoreGrabTwist => true;
	public override int Duration => 18;
	public override int Cooldown => 2;
	public override int RangeXLeft => 384;
	public override int RangeXRight => 384;
	public override int RangeY => 432;
	public override int PerformPoseAnimationID => PoseAttack_WavePolearm.TYPE_ID;
	public override int HandheldPoseAnimationID => PoseHandheld_Polearm.TYPE_ID;
}
