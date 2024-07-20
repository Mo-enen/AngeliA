namespace AngeliA;

public class PoseHandheld_Pole : PoseAnimation {
	public override void Animate (PoseCharacter character) {
		base.Animate(character);

		if (!Target.IsChargingAttack) {
			// Holding
			bool dashing = AnimationType == CharacterAnimationType.Dash;

			ResetShoulderAndUpperArmPos();

			// Upper Arm
			int twistDelta = Target.BodyTwist / 26;
			UpperArmL.LimbRotate((FacingRight ? -2 : 14) - twistDelta);
			UpperArmR.LimbRotate((FacingRight ? -14 : 2) - twistDelta);
			if (dashing) {
				UpperArmL.Height /= 3;
				UpperArmR.Height /= 3;
			} else {
				int deltaY = (Target.DeltaPositionY / 5).Clamp(-20, 20);
				UpperArmL.Height += deltaY;
				UpperArmR.Height += deltaY;
			}

			// Lower Arm
			LowerArmL.LimbRotate((FacingRight ? -24 : 43) + twistDelta);
			LowerArmR.LimbRotate((FacingRight ? -43 : 24) + twistDelta);
			if (dashing) {
				LowerArmL.Height /= 3;
				LowerArmR.Height /= 3;
			} else {
				int deltaY = (Target.DeltaPositionY / 10).Clamp(-20, 20);
				LowerArmL.Height += deltaY;
				LowerArmR.Height += deltaY;
			}

			HandL.LimbRotate(FacingSign);
			HandR.LimbRotate(FacingSign);

			// Z
			HandL.Z = FrontSign * POSE_Z_HAND;
			HandR.Z = FrontSign * POSE_Z_HAND;

			// Grab
			int deltaRot = (Target.DeltaPositionY / 10).Clamp(-10, 10);
			Target.HandGrabRotationL = Target.HandGrabRotationR = FacingSign * (80 + deltaRot);
			Target.HandGrabScaleL = Target.HandGrabScaleR = FacingSign * 1000;

		} else {
			// Charging
			float ease01 = 1f - Ease.OutBack(((float)(Game.GlobalFrame - Target.AttackChargeStartFrame.Value) / Util.Max(Target.MinimalChargeAttackDuration * 2, 1)).Clamp01());

			AttackHeadDown(ease01, 100, 800, 100);
			ResetShoulderAndUpperArmPos();

			// Upper Arm
			int uRotA = (int)Util.LerpUnclamped(-130, 63, ease01);
			int uRotB = (int)Util.LerpUnclamped(-79, 43, ease01);
			UpperArmL.LimbRotate(FacingRight ? uRotA : -uRotB);
			UpperArmR.LimbRotate(FacingRight ? uRotB : -uRotA);

			// Lower Arm
			int lRotA = (int)Util.LerpUnclamped(0, -75, ease01);
			int lRotB = (int)Util.LerpUnclamped(-98, 0, ease01);
			LowerArmL.LimbRotate(FacingRight ? lRotA : -lRotB);
			LowerArmR.LimbRotate(FacingRight ? lRotB : -lRotA);

			// Hand
			HandL.LimbRotate(FacingSign);
			HandR.LimbRotate(FacingSign);

			// Leg
			AttackLegShake(ease01);

			// Grab
			Target.HandGrabRotationL = Target.HandGrabRotationR =
				FacingSign * (int)Util.LerpUnclamped(-58, 107, ease01);
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * (int)Util.LerpUnclamped(1100, 1400, ease01);

			// Z
			UpperArmL.Z = UpperArmL.Z.Abs();
			LowerArmL.Z = LowerArmL.Z.Abs();
			UpperArmR.Z = UpperArmR.Z.Abs();
			LowerArmR.Z = LowerArmR.Z.Abs();
			HandR.Z = POSE_Z_HAND;
		}
	}
}
