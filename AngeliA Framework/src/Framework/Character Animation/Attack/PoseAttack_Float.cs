using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class PoseAttack_Float : PoseAnimation {
	public override void Animate (PoseCharacter character) {
		base.Animate(character);
		character.AttackStyleLoop = 1;
		Float();
	}
	public static void Float () {

		bool isCharging = Target.IsChargingAttack && Target.AttackChargeStartFrame.HasValue;
		float ease01 = isCharging ?
			1f - Ease.OutBack(((float)(Game.GlobalFrame - Target.AttackChargeStartFrame.Value) / Util.Max(Target.MinimalChargeAttackDuration * 2, 1)).Clamp01()) :
			Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

		if (isCharging) {
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
		Target.HandGrabRotationL = Target.HandGrabRotationR = 0;
		Target.HandGrabScaleL = Target.HandGrabScaleR = (int)Util.LerpUnclamped(700, 800, ease01);

		// Leg
		AttackLegShake(ease01);
	}
}
