using System.Collections;
using System.Collections.Generic;


using AngeliA;

namespace AngeliA.Platformer;

public abstract class Hook : MeleeWeapon {
	public override int RangeXLeft => 275;
	public override int RangeXRight => 275;
	public override int RangeY => 432;
	public override int PerformPoseAnimationID => PoseAttack_WaveSingleHanded.TYPE_ID;
	public override int HandheldPoseAnimationID => PoseHandheld_SingleHanded.TYPE_ID;
}

