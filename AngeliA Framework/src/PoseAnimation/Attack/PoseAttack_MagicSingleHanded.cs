using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class PoseAttack_MagicSingleHanded : PoseAnimation {
	public static readonly int TYPE_ID = typeof(PoseAttack_MagicSingleHanded).AngeHash();
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		Attackness.AttackStyleLoop = 1;
		Wave();
	}
	public static void Wave () {

		float ease01 = AttackEase;

		AttackHeadDown(ease01, 200, 300, 200);
		ResetShoulderAndUpperArmPos();

		// Arm L
		UpperArmL.LimbRotate((int)(ease01 * 20f));
		LowerArmL.LimbRotate((int)(ease01 * -5f));
		HandL.LimbRotate(FacingSign);

		// Arm R
		UpperArmR.LimbRotate(FacingSign * (int)Util.LerpUnclamped(-165, -80, ease01));
		LowerArmR.LimbRotate(0);
		HandR.LimbRotate(FacingSign);

		// Z
		UpperArmR.Z = LowerArmR.Z = UpperArmR.Z.Abs();
		HandR.Z = POSE_Z_HAND;

		// Grab
		Rendering.HandGrabRotationL = Rendering.HandGrabRotationR =
			FacingSign * (int)Util.LerpUnclamped(-60, 90, ease01);
		Rendering.HandGrabScaleL = Rendering.HandGrabScaleR =
			FacingSign * (int)Util.LerpUnclamped(1000, 1200, ease01);

		// Leg
		AttackLegShake(ease01);
	}
}