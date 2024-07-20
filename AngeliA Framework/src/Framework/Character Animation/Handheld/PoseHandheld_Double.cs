namespace AngeliA;

public class PoseHandheld_Double : PoseAnimation {
	public override void Animate (PoseCharacter character) {
		base.Animate(character);
		if (!Target.IsChargingAttack) {
			// Holding
			ResetShoulderAndUpperArmPos();

			int twistShift = Target.BodyTwist / 50;
			UpperArmL.LimbRotate((FacingRight ? -42 : 29) - twistShift);
			UpperArmR.LimbRotate((FacingRight ? -29 : 42) - twistShift);
			UpperArmL.Height = UpperArmL.Height * (FacingRight ? 1306 : 862) / 1000;
			UpperArmR.Height = UpperArmR.Height * (FacingRight ? 862 : 1306) / 1000;

			LowerArmL.Height = LowerArmL.SizeY;
			LowerArmR.Height = LowerArmR.SizeY;
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
			// Charging
			float ease01 = 1f - Ease.OutBack(
				((float)(Game.GlobalFrame - Target.AttackChargeStartFrame.Value) / Util.Max(Target.MinimalChargeAttackDuration * 2, 1)).Clamp01()
			);
			AttackHeadDown(ease01, 100, 800, 100);
			ResetShoulderAndUpperArmPos();

			int upperRotA = (int)Util.LerpUnclamped(180, 42, ease01);
			int upperRotB = (int)Util.LerpUnclamped(180, 29, ease01);
			int lowerRotA = (int)Util.LerpUnclamped(0, 28, ease01);
			int lowerRotB = (int)Util.LerpUnclamped(-98, 14, ease01);

			UpperArmL.LimbRotate(FacingRight ? -upperRotA : upperRotB);
			UpperArmR.LimbRotate(FacingRight ? -upperRotB : upperRotA);
			UpperArmL.Height = UpperArmL.Height * (FacingRight ? 1306 : 862) / 1000;
			UpperArmR.Height = UpperArmR.Height * (FacingRight ? 862 : 1306) / 1000;

			LowerArmL.LimbRotate(FacingRight ? -lowerRotA : -lowerRotB);
			LowerArmR.LimbRotate(FacingRight ? lowerRotB : lowerRotA);
			LowerArmL.Height = LowerArmL.Height * (FacingRight ? 1592 : 724) / 1000;
			LowerArmR.Height = LowerArmR.Height * (FacingRight ? 724 : 1592) / 1000;

			// Upper Arm
			UpperArmL.Height += A2G;
			UpperArmR.Height += A2G;
			LowerArmL.Height += A2G;
			LowerArmR.Height += A2G;

			HandL.LimbRotate(FacingSign);
			HandL.Width += HandL.Width.Sign() * A2G;
			HandL.Height += HandL.Height.Sign() * A2G;

			HandR.LimbRotate(FacingSign);
			HandR.Width += HandR.Width.Sign() * A2G;
			HandR.Height += HandR.Height.Sign() * A2G;

			// Leg
			AttackLegShake(ease01);

			// Grab Rotation
			Target.HandGrabRotationL = Target.HandGrabRotationR =
				FacingSign * (int)Util.LerpUnclamped(-37, 100, ease01);
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * (int)Util.LerpUnclamped(1100, 1400, ease01);

			// Z
			UpperArmL.Z = UpperArmL.Z.Abs();
			UpperArmR.Z = UpperArmR.Z.Abs();
			LowerArmL.Z = LowerArmL.Z.Abs();
			LowerArmR.Z = LowerArmR.Z.Abs();
			HandL.Z = POSE_Z_HAND;
			HandR.Z = POSE_Z_HAND;
		}
	}
}
