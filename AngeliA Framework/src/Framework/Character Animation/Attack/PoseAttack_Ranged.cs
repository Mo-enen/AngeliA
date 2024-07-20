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

		AttackLegShake(ease01);
		AttackHeadDown(ease01, 0, 200, -1000, 0);

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
				rotUpperR = -45;
				rotLowerL = 0;
				rotLowerR = 180;
				grabRot = 45;
				grabScl = -1000;
				break;
			}
			case Direction8.TopRight: {
				// TR
				rotUpperL = 45;
				rotUpperR = -135;
				rotLowerL = -180;
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

		AttackHeadDown(ease01, 0, 200, -1000, 0);
		ResetShoulderAndUpperArmPos();

		// Upper Arm
		int rotUA = FacingRight ? UpperArmL.Rotation : -UpperArmR.Rotation;
		int rotUB = FacingRight ? UpperArmR.Rotation : -UpperArmL.Rotation;
		rotUA = (int)Util.LerpUnclamped(rotUA, -90, ease01);
		rotUB = (int)Util.LerpUnclamped(rotUB, -90, ease01);
		UpperArmL.LimbRotate(FacingRight ? rotUA : -rotUB);
		UpperArmR.LimbRotate(FacingRight ? rotUB : -rotUA);
		UpperArmL.Height += FacingRight ? 2 * A2G : 2 * -A2G;
		UpperArmR.Height += FacingRight ? 2 * -A2G : 2 * A2G;

		int rotLA = -90 - rotUA;
		int rotLB = (int)Util.LerpUnclamped(0, 0, ease01);
		LowerArmL.LimbRotate(FacingRight ? rotLA : -rotLB);
		LowerArmR.LimbRotate(FacingRight ? rotLB : -rotLA);
		LowerArmL.Height += FacingRight ? A2G : -A2G;
		LowerArmR.Height += FacingRight ? -A2G : A2G;

		HandL.LimbRotate(FacingSign);
		HandR.LimbRotate(FacingSign);

		// Leg
		AttackLegShake(ease01);

		// Z
		UpperArmL.Z = UpperArmR.Z = FrontSign * (POSE_Z_HAND - 2);
		LowerArmL.Z = LowerArmR.Z = FrontSign * (POSE_Z_HAND - 1);
		HandL.Z = HandR.Z = FrontSign * POSE_Z_HAND;

		// Grab
		Target.HandGrabRotationL = Target.HandGrabRotationR = FacingRight ? 0 : 180;
		Target.HandGrabScaleL = Target.HandGrabScaleR = 1000;

	}

}
