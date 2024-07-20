namespace AngeliA;

public class PoseHandheld_Single : PoseAnimation {
	public override void Animate (PoseCharacter character) {
		base.Animate(character);
		if (Target.IsChargingAttack) {
			// Charging
			bool isThrowing = Target.EquippingWeaponType == WeaponType.Throwing;
			float ease01 = 1f - Ease.OutBack(((float)(Game.GlobalFrame - Target.AttackChargeStartFrame.Value) / Util.Max(Target.MinimalChargeAttackDuration * 2, 1)).Clamp01());

			AttackHeadDown(ease01, 100, 800, 100);
			ResetShoulderAndUpperArmPos();

			// Upper Arm R
			UpperArmR.LimbRotate(FacingSign * (int)Util.LerpUnclamped(-185, -9, ease01));
			if (!isThrowing) UpperArmR.Height += A2G;
			LowerArmR.LimbRotate(0);
			if (!isThrowing) LowerArmR.Height += A2G;

			HandL.LimbRotate(FacingSign);
			HandR.LimbRotate(FacingSign);
			if (!isThrowing) {
				HandR.Width += HandR.Width.Sign() * A2G;
				HandR.Height += HandR.Height.Sign() * A2G;
			}

			// Leg
			AttackLegShake(ease01);

			// Grab
			Target.HandGrabRotationL = Target.HandGrabRotationR =
				FacingSign * (int)Util.LerpUnclamped(-80, 100, ease01);
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * (int)Util.LerpUnclamped(1100, 1400, ease01);

			// Z
			UpperArmL.Z = UpperArmL.Z.Abs();
			UpperArmR.Z = UpperArmR.Z.Abs();
			LowerArmL.Z = LowerArmL.Z.Abs();
			LowerArmR.Z = LowerArmR.Z.Abs();
			HandR.Z = POSE_Z_HAND;
		}
	}
}
