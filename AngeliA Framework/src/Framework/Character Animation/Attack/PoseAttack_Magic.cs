using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class PoseAttack_Magic : PoseAnimation {
	public override void Animate (PoseCharacter character) {
		base.Animate(character);
		character.AttackStyleLoop = 1;
		switch (character.EquippingWeaponHeld) {
			default:
			case WeaponHandheld.Float:
				PoseAttack_Float.WaveDown();
				break;
			case WeaponHandheld.SingleHanded:
				SingleHanded();
				break;
			case WeaponHandheld.Pole:
				Pole();
				break;
		}
	}
	public static void SingleHanded () {

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
		Target.HandGrabRotationL = Target.HandGrabRotationR =
			FacingSign * (int)Util.LerpUnclamped(-60, 90, ease01);
		Target.HandGrabScaleL = Target.HandGrabScaleR =
			FacingSign * (int)Util.LerpUnclamped(1000, 1200, ease01);

		// Leg
		AttackLegShake(ease01);
	}
	public static void Pole () {

		float ease01 = AttackEase;

		if (IsChargingAttack) {
			AttackHeadDown(ease01, 100, 800, 100);
		} else {
			AttackHeadDown(ease01, 200, 300, 200);
		}
		ResetShoulderAndUpperArmPos();

		// Upper Arm
		float armGrowAmount = IsChargingAttack ? 1f - ease01 : ease01;
		int uRotA = (int)Util.LerpUnclamped(-90, 13, ease01);
		int uRotB = (int)Util.LerpUnclamped(-69, 33, ease01);
		UpperArmL.LimbRotate(FacingRight ? uRotA : -uRotB);
		UpperArmR.LimbRotate(FacingRight ? uRotB : -uRotA);
		UpperArmL.Height = (int)(UpperArmL.Height * armGrowAmount * (FacingRight ? 1f : 0.2f));
		UpperArmR.Height = (int)(UpperArmR.Height * armGrowAmount * (FacingRight ? 0.2f : 1f));

		// Lower Arm
		int lRotA = (int)Util.LerpUnclamped(0, -25, ease01);
		int lRotB = (int)Util.LerpUnclamped(-32, 0, ease01);
		LowerArmL.LimbRotate(FacingRight ? lRotA : -lRotB);
		LowerArmR.LimbRotate(FacingRight ? lRotB : -lRotA);
		LowerArmL.Height = (int)(LowerArmL.Height * armGrowAmount * (FacingRight ? 1f : 0.2f));
		LowerArmR.Height = (int)(LowerArmR.Height * armGrowAmount * (FacingRight ? 0.2f : 1f));

		// Hand
		HandL.LimbRotate(FacingSign);
		HandR.LimbRotate(FacingSign);

		// Grab
		Target.HandGrabRotationL = Target.HandGrabRotationR =
			FacingSign * (int)Util.LerpUnclamped(-5, 60, ease01);
		Target.HandGrabScaleL = Target.HandGrabScaleR =
			FacingSign * (int)Util.LerpUnclamped(1000, 1100, ease01);

		// Z
		UpperArmL.Z = LowerArmL.Z = FrontSign * UpperArmL.Z.Abs();
		UpperArmR.Z = LowerArmR.Z = FrontSign * UpperArmR.Z.Abs();
		HandL.Z = FrontSign * POSE_Z_HAND;
		HandR.Z = FrontSign * POSE_Z_HAND;

		// Leg
		AttackLegShake(ease01);

	}
}