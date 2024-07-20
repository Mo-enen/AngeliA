namespace AngeliA;

public class PoseHandheld_EachHand : PoseAnimation {
	public override void Animate (PoseCharacter character) {
		base.Animate(character);
		if (Target.IsChargingAttack) {
			float ease01 = 1f - Ease.OutBack(((float)(Game.GlobalFrame - Target.AttackChargeStartFrame.Value) / Util.Max(Target.MinimalChargeAttackDuration * 2, 1)).Clamp01());
			AttackHeadDown(ease01, 100, 800, 100);
			ResetShoulderAndUpperArmPos();

			UpperArmL.LimbRotate(FacingSign * (int)Util.LerpUnclamped(-175, 0, ease01));
			UpperArmR.LimbRotate(FacingSign * (int)Util.LerpUnclamped(-185, -9, ease01));
			UpperArmL.Height += A2G;
			UpperArmR.Height += A2G;

			LowerArmL.LimbRotate(0);
			LowerArmR.LimbRotate(0);
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

			// Z
			UpperArmL.Z = UpperArmL.Z.Abs();
			UpperArmR.Z = UpperArmR.Z.Abs();
			LowerArmL.Z = LowerArmL.Z.Abs();
			LowerArmR.Z = LowerArmR.Z.Abs();
			HandL.Z = FacingSign * POSE_Z_HAND;
			HandR.Z = -FacingSign * POSE_Z_HAND;

			// Grab
			Target.HandGrabRotationL = FacingSign * (int)Util.LerpUnclamped(-70, 110, ease01);
			Target.HandGrabRotationR = FacingSign * (int)Util.LerpUnclamped(-80, 100, ease01);
			Target.HandGrabScaleL = FacingSign * (int)Util.LerpUnclamped(1000, 1300, ease01);
			Target.HandGrabScaleR = FacingSign * (int)Util.LerpUnclamped(1100, 1400, ease01);

		}
	}
}
