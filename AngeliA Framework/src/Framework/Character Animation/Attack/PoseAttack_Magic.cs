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
				Float();
				break;
			case WeaponHandheld.SingleHanded:
				SingleHanded();
				break;
			case WeaponHandheld.Pole:
				Pole();
				break;
		}
	}
	private static void Float () {

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
	private static void SingleHanded () {

		float ease01 = Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

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
	private static void Pole () {

		bool isCharging = Target.IsChargingAttack && Target.AttackChargeStartFrame.HasValue;
		float ease01 = isCharging ?
			1f - Ease.OutBack(((float)(Game.GlobalFrame - Target.AttackChargeStartFrame.Value) / Util.Max(Target.MinimalChargeAttackDuration * 2, 1)).Clamp01()) :
			Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

		if (isCharging) {
			AttackHeadDown(ease01, 100, 800, 100);
		} else {
			AttackHeadDown(ease01, 200, 300, 200);
		}
		ResetShoulderAndUpperArmPos();

		// Upper Arm
		float armGrowAmount = isCharging ? 1f - ease01 : ease01;
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