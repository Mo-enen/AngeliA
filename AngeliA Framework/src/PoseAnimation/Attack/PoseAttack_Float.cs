using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class PoseAttack_Float : PoseAnimation {
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		Attackness.AttackStyleLoop = 1;
		WaveDown();
	}
	public static void WaveDown () {

		float ease01 = AttackEase;

		Body.Rotation = FacingSign * (int)((ease01 - 0.3f) * 15);
		Head.Rotation = -Body.Rotation / 2;

		if (IsChargingAttack) {
			AttackHeadDown(ease01, 100, 800, 100);
		} else {
			AttackHeadDown(ease01, 200, 300, 300);
		}
		ResetShoulderAndUpperArmPos(!FacingRight, FacingRight);

		var uArmB = FacingRight ? UpperArmR : UpperArmL;
		var lArmB = FacingRight ? LowerArmR : LowerArmL;
		var handB = FacingRight ? HandR : HandL;

		// Arm Right
		uArmB.LimbRotate((int)Util.LerpUnclamped(FacingSign * -180, FacingSign * -90, ease01));
		lArmB.LimbRotate((int)((1f - ease01) * -10 * FacingSign));
		handB.LimbRotate(FacingSign);

		// Z
		handB.Z = POSE_Z_HAND;

		// Grab
		Rendering.HandGrabRotationL = Rendering.HandGrabRotationR = 0;
		Rendering.HandGrabScaleL = Rendering.HandGrabScaleR = (int)Util.LerpUnclamped(700, 800, ease01);

		// Leg
		AttackLegShake(ease01);
	}
}
