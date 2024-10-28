namespace AngeliA;

public class PoseAttack_Smash : PoseAnimation {

	public static readonly int TYPE_ID = typeof(PoseAttack_Smash).AngeHash();

	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		switch (Target.EquippingToolHeld) {
			default:
				SingleHanded_SmashDown();
				break;
			case ToolHandheld.DoubleHanded:
				DoubleHanded_SmashDown();
				break;
			case ToolHandheld.OneOnEachHand:
				EachHand_SmashDown();
				break;
			case ToolHandheld.Pole:
				Polearm_SmashDown();
				break;
		}
	}

	public static void SingleHanded_SmashDown () {

		bool isThrowing = Target.EquippingToolType == ToolType.Throwing;
		float ease01 = AttackEase;

		if (IsChargingAttack) {
			AttackHeadDown(ease01, 100, 800, 100);
			ResetShoulderAndUpperArmPos();
		} else {
			AttackHeadDown(ease01);
			ResetShoulderAndUpperArmPos();
			// Left Side
			if (
				AnimationType == CharacterAnimationType.Idle ||
				AnimationType == CharacterAnimationType.SquatIdle ||
				AnimationType == CharacterAnimationType.SquatMove
			) {
				UpperArmL.LimbRotate(-15 - (int)(ease01 * 48), 500);
				LowerArmL.LimbRotate(-100 + (int)(ease01 * 48));
			}
		}

		// Upper Arm R
		const int STEP = 55;
		(int from, int to) = Attackness.AimingDirection switch {
			Direction8.Bottom => (-185 + STEP + STEP, -9 + STEP + STEP),
			Direction8.BottomLeft or Direction8.BottomRight => (-185 + STEP, -9 + STEP),
			Direction8.Left or Direction8.Right => (-185, -9),
			Direction8.TopLeft or Direction8.TopRight => (-185 - STEP, -9 - STEP),
			Direction8.Top => (-185 - STEP - STEP, -9 - STEP - STEP),
			_ => (-185, -9),
		};
		UpperArmR.LimbRotate(FacingSign * (int)Util.LerpUnclamped(from, to, ease01));
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
		Rendering.HandGrabRotationL = Rendering.HandGrabRotationR =
			FacingSign * (int)Util.LerpUnclamped(-80, 100, ease01);
		Rendering.HandGrabScaleL = Rendering.HandGrabScaleR =
			FacingSign * (int)Util.LerpUnclamped(1100, 1400, ease01);

		// Z
		UpperArmL.Z = UpperArmL.Z.Abs();
		UpperArmR.Z = UpperArmR.Z.Abs();
		LowerArmL.Z = LowerArmL.Z.Abs();
		LowerArmR.Z = LowerArmR.Z.Abs();
		HandR.Z = POSE_Z_HAND;
	}

	public static void DoubleHanded_SmashDown () {

		float ease01 = AttackEase;

		if (IsChargingAttack) {
			AttackHeadDown(ease01, 100, 800, 100);
		} else {
			AttackHeadDown(ease01);
		}
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
		Rendering.HandGrabRotationL = Rendering.HandGrabRotationR =
			FacingSign * (int)Util.LerpUnclamped(-37, 100, ease01);
		Rendering.HandGrabScaleL = Rendering.HandGrabScaleR =
			FacingSign * (int)Util.LerpUnclamped(1100, 1400, ease01);

		// Z
		UpperArmL.Z = UpperArmL.Z.Abs();
		UpperArmR.Z = UpperArmR.Z.Abs();
		LowerArmL.Z = LowerArmL.Z.Abs();
		LowerArmR.Z = LowerArmR.Z.Abs();
		HandL.Z = POSE_Z_HAND;
		HandR.Z = POSE_Z_HAND;
	}

	public static void EachHand_SmashDown () {

		float ease01 = AttackEase;
		if (IsChargingAttack) {
			AttackHeadDown(ease01, 100, 800, 100);
		} else {
			AttackHeadDown(ease01);
		}
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
		if (IsChargingAttack) {
			HandL.Z = FacingSign * POSE_Z_HAND;
			HandR.Z = -FacingSign * POSE_Z_HAND;
		} else {
			HandL.Z = POSE_Z_HAND;
			HandR.Z = POSE_Z_HAND;
		}

		// Grab
		Rendering.HandGrabRotationL = FacingSign * (int)Util.LerpUnclamped(-70, 110, ease01);
		Rendering.HandGrabScaleL = FacingSign * (int)Util.LerpUnclamped(1000, 1300, ease01);
		Rendering.HandGrabRotationR = FacingSign * (int)Util.LerpUnclamped(-80, 100, ease01);
		Rendering.HandGrabScaleR = FacingSign * (int)Util.LerpUnclamped(1100, 1400, ease01);

	}

	public static void Polearm_SmashDown () {

		float ease01 = AttackEase;

		if (IsChargingAttack) {
			AttackHeadDown(ease01, 100, 800, 100);
		} else {
			AttackHeadDown(ease01);
		}
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
		Rendering.HandGrabRotationL = Rendering.HandGrabRotationR =
			FacingSign * (int)Util.LerpUnclamped(-58, 107, ease01);
		Rendering.HandGrabScaleL = Rendering.HandGrabScaleR =
			FacingSign * (int)Util.LerpUnclamped(1100, 1400, ease01);

		// Z
		UpperArmL.Z = UpperArmL.Z.Abs();
		LowerArmL.Z = LowerArmL.Z.Abs();
		UpperArmR.Z = UpperArmR.Z.Abs();
		LowerArmR.Z = LowerArmR.Z.Abs();
		HandR.Z = POSE_Z_HAND;

	}

}
