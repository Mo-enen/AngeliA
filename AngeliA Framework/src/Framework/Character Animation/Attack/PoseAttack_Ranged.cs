namespace AngeliA;

public class PoseAttack_Ranged : PoseAnimation {

	public override void Animate (PoseCharacter character) {
		base.Animate(character);
		character.AttackStyleLoop = 1;
		if (character.EquippingWeaponHeld == WeaponHandheld.Bow) {
			Bow();
		} else {
			Shooting();
		}
	}

	public static void Bow () {

		bool isCharging = Target.IsChargingAttack && Target.AttackChargeStartFrame.HasValue;
		float ease01 = isCharging ?
			Ease.OutBack(((float)(Game.GlobalFrame - Target.AttackChargeStartFrame.Value) / Util.Max(Target.MinimalChargeAttackDuration * 2, 1)).Clamp01()) :
			Target.LastAttackCharged ? 1f : Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

		if (!isCharging) {
			AttackLegShake(ease01);
			AttackHeadDown(ease01, 0, 200, 0);
		}
		ResetShoulderAndUpperArmPos();

		// Arm
		int rotUpperL = 0;
		int rotUpperR = 0;
		int rotLowerL = 0;
		int rotLowerR = 0;
		int grabRot = 0;
		int grabScl = 1000;

		switch (Target.AimingDirection) {


			case Direction8.Left: {
				// L
				rotUpperL = 90;
				rotUpperR = -90;
				rotLowerL = 0;
				rotLowerR = 180;
				grabRot = 0;
				grabScl = -1000;
				break;
			}
			case Direction8.Right: {
				// R
				rotUpperL = 90;
				rotUpperR = -90;
				rotLowerL = -180;
				rotLowerR = 0;
				grabRot = 0;
				grabScl = 1000;
				break;
			}
			case Direction8.Top: {
				// T
				rotUpperL = -20;
				rotUpperR = 20;
				rotLowerL = FacingRight ? 0 : -140;
				rotLowerR = FacingRight ? 140 : 0;
				grabRot = -90;
				grabScl = 1000;
				break;
			}
			case Direction8.Bottom: {
				// B
				rotUpperL = -20;
				rotUpperR = 20;
				rotLowerL = FacingRight ? -140 : 0;
				rotLowerR = FacingRight ? 0 : 140;
				grabRot = 90;
				grabScl = 1000;
				break;
			}

			case Direction8.TopLeft: {
				// TL
				rotUpperL = 135;
				rotUpperR = -45 + 10;
				rotLowerL = 0;
				rotLowerR = 180 - 20;
				grabRot = 45;
				grabScl = -1000;
				break;
			}
			case Direction8.TopRight: {
				// TR
				rotUpperL = 45 - 10;
				rotUpperR = -135;
				rotLowerL = -180 + 20;
				rotLowerR = 0;
				grabRot = -45;
				grabScl = 1000;
				break;
			}
			case Direction8.BottomLeft: {
				// BL
				rotUpperL = 45;
				rotUpperR = -135;
				rotLowerL = 0;
				rotLowerR = 180;
				grabRot = -45;
				grabScl = -1000;
				break;
			}
			case Direction8.BottomRight: {
				// BR
				rotUpperL = 135;
				rotUpperR = -45;
				rotLowerL = -180;
				rotLowerR = 0;
				grabRot = 45;
				grabScl = 1000;
				break;
			}

		}

		// Arm Hand
		UpperArmL.LimbRotate((int)Util.LerpUnclamped(UpperArmL.Rotation, rotUpperL, ease01));
		UpperArmR.LimbRotate((int)Util.LerpUnclamped(UpperArmR.Rotation, rotUpperR, ease01));
		LowerArmL.LimbRotate((int)Util.LerpUnclamped(LowerArmL.Rotation, rotLowerL, ease01));
		LowerArmR.LimbRotate((int)Util.LerpUnclamped(LowerArmR.Rotation, rotLowerR, ease01));
		HandL.LimbRotate(FacingSign);
		HandR.LimbRotate(FacingSign);

		// Z
		ShoulderL.Z = ShoulderR.Z = FrontSign * (POSE_Z_HAND - 3);
		UpperArmL.Z = UpperArmR.Z = FrontSign * (POSE_Z_HAND - 2);
		LowerArmL.Z = LowerArmR.Z = FrontSign * (POSE_Z_HAND - 1);
		HandL.Z = HandR.Z = FrontSign * POSE_Z_HAND;

		// Grab
		Target.HandGrabRotationL = Target.HandGrabRotationR = grabRot;
		Target.HandGrabScaleL = Target.HandGrabScaleR = grabScl;

	}

	public static void Shooting () {

		bool isCharging = Target.IsChargingAttack && Target.AttackChargeStartFrame.HasValue;
		float ease01 = isCharging ?
			Ease.OutBack(((float)(Game.GlobalFrame - Target.AttackChargeStartFrame.Value) / Util.Max(Target.MinimalChargeAttackDuration * 2, 1)).Clamp01()) :
			Target.LastAttackCharged ? 1f : Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

		if (!isCharging) {
			AttackHeadDown(ease01, 0, 200, 0);
			AttackLegShake(ease01);
		}
		ResetShoulderAndUpperArmPos();

		// Arm
		int rotUpperL = 0;
		int rotUpperR = 0;
		int rotLowerL = 0;
		int rotLowerR = 0;
		int grabRot = 0;
		int grabScl = 1000;
		switch (Target.AimingDirection) {


			case Direction8.Left: {
				// L
				rotUpperL = 90;
				rotUpperR = 90;
				rotLowerL = 0;
				rotLowerR = 0;
				grabRot = 0;
				grabScl = -1000;
				break;
			}
			case Direction8.Right: {
				// R
				rotUpperL = -90;
				rotUpperR = -90;
				rotLowerL = 0;
				rotLowerR = 0;
				grabRot = 0;
				grabScl = 1000;
				break;
			}
			case Direction8.Top: {
				// T
				rotUpperL = -20;
				rotUpperR = 20;
				rotLowerL = FacingRight ? 0 : -140;
				rotLowerR = FacingRight ? 140 : 0;
				grabRot = -90;
				grabScl = 1000;
				break;
			}
			case Direction8.Bottom: {
				// B
				rotUpperL = -20;
				rotUpperR = 20;
				rotLowerL = FacingRight ? -140 : 0;
				rotLowerR = FacingRight ? 0 : 140;
				grabRot = 90;
				grabScl = 1000;
				break;
			}

			case Direction8.TopLeft: {
				// TL
				rotUpperL = 145;
				rotUpperR = 125;
				rotLowerL = 0;
				rotLowerR = 0;
				grabRot = 45;
				grabScl = -1000;
				break;
			}
			case Direction8.TopRight: {
				// TR
				rotUpperL = -125;
				rotUpperR = -145;
				rotLowerL = 0;
				rotLowerR = 0;
				grabRot = -45;
				grabScl = 1000;
				break;
			}
			case Direction8.BottomLeft: {
				// BL
				rotUpperL = 35;
				rotUpperR = 55;
				rotLowerL = 0;
				rotLowerR = 0;
				grabRot = -45;
				grabScl = -1000;
				break;
			}
			case Direction8.BottomRight: {
				// BR
				rotUpperL = -55;
				rotUpperR = -35;
				rotLowerL = 0;
				rotLowerR = 0;
				grabRot = 45;
				grabScl = 1000;
				break;
			}

		}

		// Arm Hand
		UpperArmL.LimbRotate((int)Util.LerpUnclamped(UpperArmL.Rotation, rotUpperL, ease01));
		UpperArmR.LimbRotate((int)Util.LerpUnclamped(UpperArmR.Rotation, rotUpperR, ease01));
		LowerArmL.LimbRotate((int)Util.LerpUnclamped(LowerArmL.Rotation, rotLowerL, ease01));
		LowerArmR.LimbRotate((int)Util.LerpUnclamped(LowerArmR.Rotation, rotLowerR, ease01));
		HandL.LimbRotate(FacingSign);
		HandR.LimbRotate(FacingSign);

		// Z
		ShoulderL.Z = ShoulderR.Z = FrontSign * (POSE_Z_HAND - 3);
		UpperArmL.Z = UpperArmR.Z = FrontSign * (POSE_Z_HAND - 2);
		LowerArmL.Z = LowerArmR.Z = FrontSign * (POSE_Z_HAND - 1);
		HandL.Z = HandR.Z = FrontSign * POSE_Z_HAND;

		// Grab
		Target.HandGrabRotationL = Target.HandGrabRotationR = grabRot;
		Target.HandGrabScaleL = Target.HandGrabScaleR = grabScl;
		Target.HandGrabAttackTwistL = Target.HandGrabAttackTwistR = 1000;

		// Leg
		AttackLegShake(ease01);

	}

}
