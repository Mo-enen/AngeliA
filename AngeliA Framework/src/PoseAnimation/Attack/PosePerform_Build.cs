using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class PosePerform_Build : PoseAnimation {
	public static readonly int TYPE_ID = typeof(PosePerform_Build).AngeHash();
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		Attackness.AttackStyleLoop = 1;
		Build();
	}
	public static void Build () {

		float ease01 = AttackEase;

		if (IsChargingAttack) {
			AttackHeadDown(ease01, 100, 800, 100);
			ResetShoulderAndUpperArmPos();
		} else {
			AttackHeadDown(ease01);
			ResetShoulderAndUpperArmPos();
			// Left Side
			if (
				AnimationType == CharacterAnimationType.Idle ||
				AnimationType == CharacterAnimationType.SquatIdle ||
				AnimationType == CharacterAnimationType.SquatMove
			) {
				UpperArmL.LimbRotate(-15 - (int)(ease01 * 48), 500);
				LowerArmL.LimbRotate(-100 + (int)(ease01 * 48));
			}
		}

		// Upper Arm R
		UpperArmR.LimbRotate(FacingSign * (int)Util.LerpUnclamped(-185, -9, ease01));
		UpperArmR.Height += A2G;
		LowerArmR.LimbRotate(0);
		LowerArmR.Height += A2G;

		HandL.LimbRotate(FacingSign);
		HandR.LimbRotate(FacingSign);
		HandR.Width += HandR.Width.Sign() * A2G;
		HandR.Height += HandR.Height.Sign() * A2G;

		// Leg
		AttackLegShake(ease01);

		// Grab
		Rendering.HandGrabRotationL = Rendering.HandGrabRotationR = 0;
		Rendering.HandGrabScaleL = Rendering.HandGrabScaleR = 618;

		// Z
		UpperArmL.Z = UpperArmL.Z.Abs();
		UpperArmR.Z = UpperArmR.Z.Abs();
		LowerArmL.Z = LowerArmL.Z.Abs();
		LowerArmR.Z = LowerArmR.Z.Abs();
		HandR.Z = POSE_Z_HAND;
	}
}
