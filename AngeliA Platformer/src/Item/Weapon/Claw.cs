using System.Collections;
using System.Collections.Generic;

using AngeliA;
namespace AngeliA.Platformer;


public abstract class Claw : MeleeWeapon {

	public override int Duration => 10;
	public override int Cooldown => 0;
	public override int? DefaultMovementSpeedRateOnUse => 1000;
	public override int? WalkingMovementSpeedRateOnUse => 1000;
	public override int? RunningMovementSpeedRateOnUse => 1000;
	public override int RangeXLeft => 375;
	public override int RangeXRight => 375;
	public override int RangeY => 432;
	public override int HandheldPoseAnimationID => PoseHandheld_EachHand.TYPE_ID;
	public override int PerformPoseAnimationID => PoseAttack_Scratch.TYPE_ID;

	public override Cell OnToolSpriteRendered (PoseCharacterRenderer renderer, int x, int y, int width, int height, int grabRotation, int grabScale, AngeSprite sprite, int z) {
		grabScale = grabScale * 4 / 5;
		return base.OnToolSpriteRendered(renderer, x, y, width, height, grabRotation, grabScale, sprite, z);
	}
	
}
