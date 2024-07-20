namespace AngeliA;

public class PoseHandheld_MagicPole : PoseAnimation {
	public override void Animate (PoseCharacter character) {
		base.Animate(character);
		if (!Target.IsChargingAttack) {
			// Normal

			ResetShoulderAndUpperArmPos();

			int twistShift = Target.BodyTwist / 50;
			UpperArmL.LimbRotate((FacingRight ? -42 : 29) - twistShift);
			UpperArmR.LimbRotate((FacingRight ? -29 : 42) - twistShift);
			UpperArmL.Height = UpperArmL.Height * (FacingRight ? 1306 : 862) / 1000;
			UpperArmR.Height = UpperArmR.Height * (FacingRight ? 862 : 1306) / 1000;

			LowerArmL.LimbRotate((FacingRight ? -28 : -48) + twistShift / 2);
			LowerArmR.LimbRotate((FacingRight ? 48 : 28) + twistShift / 2);
			LowerArmL.Height = LowerArmL.Height * (FacingRight ? 1592 : 724) / 1000;
			LowerArmR.Height = LowerArmR.Height * (FacingRight ? 724 : 1592) / 1000;

			HandL.LimbRotate(FacingSign);
			HandR.LimbRotate(FacingSign);

			// Z
			int signZ = Body.FrontSide ? 1 : -1;
			UpperArmL.Z = LowerArmL.Z = signZ * UpperArmL.Z.Abs();
			UpperArmR.Z = LowerArmR.Z = signZ * UpperArmR.Z.Abs();
			HandL.Z = HandR.Z = signZ * POSE_Z_HAND;

			// Grab Rotation
			Target.HandGrabScaleL = Target.HandGrabScaleR = FacingSign * 1000;
			Target.HandGrabRotationL = Target.HandGrabRotationR = FacingSign * (
				30 - CurrentAnimationFrame.PingPong(120) / 30
				+ Target.DeltaPositionY.Clamp(-24, 24) / 5
			) - Target.DeltaPositionX.Clamp(-24, 24) / 4;

		} else {
			// Charge
			float ease01 = 1f - Ease.OutBack(((float)(Game.GlobalFrame - Target.AttackChargeStartFrame.Value) / Util.Max(Target.MinimalChargeAttackDuration * 2, 1)).Clamp01());
			AttackHeadDown(ease01, 100, 800, 100);
			ResetShoulderAndUpperArmPos();

			// Upper Arm
			float armGrowAmount = 1f - ease01;
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
}
