namespace AngeliA;

public class PoseAttack_Wave : PoseAnimation {

	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		int style;
		var handheld = Target.EquippingWeaponHeld;
		var weaponType = Target.EquippingWeaponType;
		switch (handheld) {

			// Single Handed
			case WeaponHandheld.SingleHanded:
				Attackness.AttackStyleLoop = 4;
				style =
					Attackness.LastAttackCharged ||
					weaponType == WeaponType.Throwing ||
					weaponType == WeaponType.Flail ||
					Target.EquipingPickWeapon ?
					0 : Attackness.AttackStyleIndex % Attackness.AttackStyleLoop;
				switch (style) {
					default:
						SingleHanded_SmashDown();
						break;
					case 1:
						SingleHanded_SmashUp();
						break;
					case 2:
						SingleHanded_SlashIn();
						break;
					case 3:
						SingleHanded_SlashOut();
						break;
				}
				break;

			// Double Handed
			case WeaponHandheld.DoubleHanded:
				Attackness.AttackStyleLoop = 4;
				style =
					Attackness.LastAttackCharged ||
					weaponType == WeaponType.Throwing ||
					weaponType == WeaponType.Flail ||
					Target.EquipingPickWeapon ?
					0 : Attackness.AttackStyleIndex % Attackness.AttackStyleLoop;
				switch (style) {
					default:
						DoubleHanded_SmashDown();
						break;
					case 1:
						DoubleHanded_SmashUp();
						break;
					case 2:
						DoubleHanded_SlashIn();
						break;
					case 3:
						DoubleHanded_SlashOut();
						break;
				}
				break;

			// Each Hand
			case WeaponHandheld.OneOnEachHand:
				Attackness.AttackStyleLoop = 4;
				style =
					Attackness.LastAttackCharged ||
					Target.EquipingPickWeapon ? 0 :
					Attackness.AttackStyleIndex % Attackness.AttackStyleLoop;
				switch (style) {
					default:
						EachHand_SmashDown();
						break;
					case 1:
						EachHand_SmashUp();
						break;
					case 2:
						EachHand_SlashIn();
						break;
					case 3:
						EachHand_SlashOut();
						break;
				}
				break;

			// Pole
			case WeaponHandheld.Pole:
				Attackness.AttackStyleLoop = 4;
				style =
					Attackness.LastAttackCharged ||
					Target.EquippingWeaponType == WeaponType.Flail ||
					Target.EquipingPickWeapon ? 0 :
					Attackness.AttackStyleIndex % Attackness.AttackStyleLoop;
				switch (style) {
					default:
						Polearm_SmashDown();
						break;
					case 1:
						Polearm_SmashUp();
						break;
					case 2:
						Polearm_SlashIn();
						break;
					case 3:
						Polearm_SlashOut();
						break;
				}
				break;
		}
	}

	public static void SingleHanded_SmashDown () {

		bool isThrowing = Target.EquippingWeaponType == WeaponType.Throwing;
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
	public static void SingleHanded_SmashUp () {

		float ease01 = AttackEase;

		AttackHeadDown(1f - ease01 + 0.5f, -100, 500, 500);
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

		// Upper Arm R
		UpperArmR.LimbRotate(FacingSign * (int)Util.LerpUnclamped(60, -160, ease01));
		UpperArmR.Height += A2G;

		LowerArmR.LimbRotate(0);
		LowerArmR.Height += A2G;

		HandL.LimbRotate(FacingSign);
		HandR.LimbRotate(FacingSign);
		HandR.Width += HandR.Width.Sign() * A2G;
		HandR.Height += HandR.Height.Sign() * A2G;

		// Leg
		AttackLegShake(ease01);

		// Grab
		Rendering.HandGrabRotationL = Rendering.HandGrabRotationR =
			FacingSign * (int)Util.LerpUnclamped(197, 12, ease01);
		Rendering.HandGrabScaleL = Rendering.HandGrabScaleR =
			FacingSign * (int)Util.LerpUnclamped(-1100, -1400, ease01);

		// Z
		UpperArmL.Z = UpperArmL.Z.Abs();
		UpperArmR.Z = UpperArmR.Z.Abs();
		LowerArmL.Z = LowerArmL.Z.Abs();
		LowerArmR.Z = LowerArmR.Z.Abs();
		HandR.Z = POSE_Z_HAND;
	}
	public static void SingleHanded_SlashIn () {

		float frame01 = AttackLerp;
		float ease01 = AttackEase;
		float ease010 = Util.PingPong(ease01 * 2f, 1f);

		AttackHeadDown(ease01, 1400, 500, 500);
		ResetShoulderAndUpperArmPos();

		Rendering.BodyTwist = -FacingSign * (int)(frame01 * 2000 - 1000);
		Rendering.HeadTwist = -FacingSign * (int)(frame01 * 500 - 250);

		// Left Side
		if (
			AnimationType == CharacterAnimationType.Idle ||
			AnimationType == CharacterAnimationType.SquatIdle ||
			AnimationType == CharacterAnimationType.SquatMove
		) {
			UpperArmL.LimbRotate(15 + (int)(ease01 * 48), 500);
			LowerArmL.LimbRotate(-100 + (int)(ease01 * 48));
		}

		// Upper Arm R
		UpperArmR.LimbRotate(FacingSign * (int)Util.LerpUnclamped(-100, 65, ease01));
		UpperArmR.Height = (int)(UpperArmR.Height * Util.LerpUnclamped(1.2f, 0.1f, ease010));
		LowerArmR.LimbRotate(0);
		LowerArmR.Height = (int)(LowerArmR.Height * Util.LerpUnclamped(1.2f, 0.1f, ease010));

		HandL.LimbRotate(FacingSign);
		HandR.LimbRotate(FacingSign);
		HandR.Width += HandR.Width.Sign() * A2G;
		HandR.Height += HandR.Height.Sign() * A2G;

		// Leg
		AttackLegShake(ease01);

		// Grab
		Rendering.HandGrabRotationL = Rendering.HandGrabRotationR =
			FacingSign * (int)Util.LerpUnclamped(67, 224, ease01);
		Rendering.HandGrabScaleL = Rendering.HandGrabScaleR =
			FacingSign * (int)Util.LerpUnclamped(1300, 100, ease010);
		Rendering.HandGrabAttackTwistL = Rendering.HandGrabAttackTwistR =
			(int)Util.LerpUnclamped(600, 200, frame01);

		// Z
		UpperArmL.Z = UpperArmL.Z.Abs();
		LowerArmL.Z = LowerArmL.Z.Abs();
		UpperArmR.Z = (frame01 < 0.5f ? -1 : 1) * UpperArmR.Z.Abs();
		LowerArmR.Z = (frame01 < 0.5f ? -1 : 1) * LowerArmR.Z.Abs();
		HandR.Z = POSE_Z_HAND;
	}
	public static void SingleHanded_SlashOut () {

		float frame01 = AttackLerp;
		float ease01 = AttackEase;
		float ease010 = Util.PingPong(ease01 * 2f, 1f);

		AttackHeadDown(ease01, -500, 500, -1000);
		ResetShoulderAndUpperArmPos();

		Rendering.BodyTwist = FacingSign * (int)(frame01 * 2000 - 1000);
		Rendering.HeadTwist = FacingSign * (int)(frame01 * 500 - 250);

		// Left Side
		if (
			AnimationType == CharacterAnimationType.Idle ||
			AnimationType == CharacterAnimationType.SquatIdle ||
			AnimationType == CharacterAnimationType.SquatMove
		) {
			UpperArmL.LimbRotate(15 + (int)(ease01 * 48), 500);
			LowerArmL.LimbRotate(-100 + (int)(ease01 * 48));
		}

		// Upper Arm R
		UpperArmR.LimbRotate(FacingSign * (int)Util.LerpUnclamped(178, -50, ease01));
		UpperArmR.Height = (int)(UpperArmR.Height * Util.LerpUnclamped(1.2f, 0.1f, ease010));
		LowerArmR.LimbRotate(0);
		LowerArmR.Height = (int)(LowerArmR.Height * Util.LerpUnclamped(1.2f, 0.1f, ease010));

		HandL.LimbRotate(FacingSign);
		HandR.LimbRotate(FacingSign);
		HandR.Width += HandR.Width.Sign() * A2G;
		HandR.Height += HandR.Height.Sign() * A2G;

		// Leg
		AttackLegShake(ease01);

		// Grab
		Rendering.HandGrabRotationL = Rendering.HandGrabRotationR =
			FacingSign * (int)Util.LerpUnclamped(197, 128, ease01);
		Rendering.HandGrabScaleL = Rendering.HandGrabScaleR =
			FacingSign * (int)Util.LerpUnclamped(-1300, -100, ease010);
		Rendering.HandGrabAttackTwistL = Rendering.HandGrabAttackTwistR =
			(int)Util.LerpUnclamped(600, 200, frame01);

		// Z
		UpperArmL.Z = UpperArmL.Z.Abs();
		LowerArmL.Z = LowerArmL.Z.Abs();
		UpperArmR.Z = (frame01 < 0.5f ? -1 : 1) * UpperArmR.Z.Abs();
		LowerArmR.Z = (frame01 < 0.5f ? -1 : 1) * LowerArmR.Z.Abs();
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
	public static void DoubleHanded_SmashUp () {

		float ease01 = AttackEase;

		AttackHeadDown(1f - ease01 + 0.5f, -200, 500, 500);
		ResetShoulderAndUpperArmPos();

		int upperRotA = (int)Util.LerpUnclamped(42, 180, ease01);
		int upperRotB = (int)Util.LerpUnclamped(29, 180, ease01);
		int lowerRotA = (int)Util.LerpUnclamped(28, 0, ease01);
		int lowerRotB = (int)Util.LerpUnclamped(14, -98, ease01);

		UpperArmL.LimbRotate(FacingRight ? -upperRotA : upperRotB);
		UpperArmR.LimbRotate(FacingRight ? -upperRotB : upperRotA);
		UpperArmL.Height = UpperArmL.Height * (FacingRight ? 1000 : 862) / 1000;
		UpperArmR.Height = UpperArmR.Height * (FacingRight ? 862 : 1000) / 1000;

		LowerArmL.LimbRotate(FacingRight ? -lowerRotA : -lowerRotB);
		LowerArmR.LimbRotate(FacingRight ? lowerRotB : lowerRotA);
		LowerArmL.Height = LowerArmL.Height * (FacingRight ? 1000 : 724) / 1000;
		LowerArmR.Height = LowerArmR.Height * (FacingRight ? 724 : 1000) / 1000;

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
			FacingSign * (int)Util.LerpUnclamped(100, -20, ease01);
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
	public static void DoubleHanded_SlashIn () {

		float frame01 = AttackLerp;
		float ease01 = AttackEase;
		float ease010 = Util.PingPong(ease01 * 2f, 1f);

		AttackHeadDown(ease01, 1400, 500, 500);
		ResetShoulderAndUpperArmPos();

		Rendering.BodyTwist = -FacingSign * (int)(frame01 * 2000 - 1000);
		Rendering.HeadTwist = -FacingSign * (int)(frame01 * 500 - 250);

		// Upper Arm
		int upperRotA = (int)Util.LerpUnclamped(-100, 65, ease01);
		int upperRotB = (int)Util.LerpUnclamped(-47, 77, ease01);

		UpperArmL.LimbRotate(FacingRight ? upperRotA : -upperRotB);
		UpperArmR.LimbRotate(FacingRight ? upperRotB : -upperRotA);
		UpperArmL.Height = (int)(UpperArmL.Height * Util.LerpUnclamped(1.2f, 0.1f, ease010));
		UpperArmR.Height = (int)(UpperArmR.Height * Util.LerpUnclamped(1.2f, 0.1f, ease010));

		LowerArmL.LimbRotate(0);
		LowerArmR.LimbRotate(0);
		LowerArmL.Height = (int)(LowerArmL.Height * Util.LerpUnclamped(1.2f, 0.1f, ease010));
		LowerArmR.Height = (int)(LowerArmR.Height * Util.LerpUnclamped(1.2f, 0.1f, ease010));

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
			FacingSign * (int)Util.LerpUnclamped(71, 248, ease01);
		Rendering.HandGrabScaleL = Rendering.HandGrabScaleR =
			FacingSign * (int)Util.LerpUnclamped(1300, 100, ease010);
		Rendering.HandGrabAttackTwistL = Rendering.HandGrabAttackTwistR =
			(int)Util.LerpUnclamped(600, 200, frame01);

		// Z
		UpperArmL.Z = UpperArmL.Z.Abs();
		UpperArmR.Z = UpperArmR.Z.Abs();
		LowerArmL.Z = LowerArmL.Z.Abs();
		LowerArmR.Z = LowerArmR.Z.Abs();
		HandL.Z = POSE_Z_HAND;
		HandR.Z = POSE_Z_HAND;
	}
	public static void DoubleHanded_SlashOut () {

		float frame01 = AttackLerp;
		float ease01 = AttackEase;
		float ease010 = Util.PingPong(ease01 * 2f, 1f);

		AttackHeadDown(ease01, -500, 500, 500);
		ResetShoulderAndUpperArmPos();

		Rendering.BodyTwist = FacingSign * (int)(frame01 * 2000 - 1000);
		Rendering.HeadTwist = FacingSign * (int)(frame01 * 500 - 250);

		int upperRotA = (int)Util.LerpUnclamped(-171, 49, ease01);
		int upperRotB = (int)Util.LerpUnclamped(-100, 39, ease01);

		UpperArmL.LimbRotate(FacingRight ? -upperRotA : upperRotB);
		UpperArmR.LimbRotate(FacingRight ? -upperRotB : upperRotA);
		UpperArmL.Height = (int)(UpperArmL.Height * Util.LerpUnclamped(1.2f, 0.1f, ease010));
		UpperArmR.Height = (int)(UpperArmR.Height * Util.LerpUnclamped(1.2f, 0.1f, ease010));

		LowerArmL.LimbRotate(0);
		LowerArmR.LimbRotate(0);
		LowerArmL.Height = (int)(LowerArmL.Height * Util.LerpUnclamped(1.2f, 0.1f, ease010));
		LowerArmR.Height = (int)(LowerArmR.Height * Util.LerpUnclamped(1.2f, 0.1f, ease010));

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
			FacingSign * (int)Util.LerpUnclamped(297, 128, ease01);
		Rendering.HandGrabScaleL = Rendering.HandGrabScaleR =
			FacingSign * (int)Util.LerpUnclamped(1300, 100, ease010);
		Rendering.HandGrabAttackTwistL = Rendering.HandGrabAttackTwistR =
			(int)Util.LerpUnclamped(600, 200, frame01);

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
	public static void EachHand_SmashUp () {

		float ease01 = AttackEase;
		float easeL = FacingRight ? ease01 : ease01 - 0.1f;
		float easeR = FacingRight ? ease01 - 0.1f : ease01;

		AttackHeadDown(1f - ease01 + 0.5f, -100, 500, 500);
		ResetShoulderAndUpperArmPos();

		// Upper Arm
		UpperArmL.LimbRotate(FacingSign * (int)Util.LerpUnclamped(60, -160, easeL));
		UpperArmR.LimbRotate(FacingSign * (int)Util.LerpUnclamped(60, -160, easeR));
		UpperArmL.Height += A2G;
		UpperArmR.Height += A2G;

		LowerArmL.LimbRotate(FacingSign * (int)Util.LerpUnclamped(-50, 0, easeL));
		LowerArmR.LimbRotate(FacingSign * (int)Util.LerpUnclamped(-50, 0, easeR));
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
		HandR.Z = POSE_Z_HAND;

		// Grab
		Rendering.HandGrabRotationL = FacingSign * (int)Util.LerpUnclamped(197, 12, easeL);
		Rendering.HandGrabRotationR = FacingSign * (int)Util.LerpUnclamped(197, 12, easeR);
		Rendering.HandGrabScaleL = Rendering.HandGrabScaleR = FacingSign * (int)Util.LerpUnclamped(1000, 1300, ease01);

	}
	public static void EachHand_SlashIn () {

		float frame01 = AttackLerp;
		float ease01 = AttackEase;
		float ease010 = Util.PingPong(ease01 * 2f, 1f);

		AttackHeadDown(ease01, 1400, 500, 500);
		ResetShoulderAndUpperArmPos();

		Rendering.BodyTwist = -FacingSign * (int)(frame01 * 2000 - 1000);
		Rendering.HeadTwist = -FacingSign * (int)(frame01 * 500 - 250);

		// Upper Arm
		UpperArmL.LimbRotate(FacingSign * (int)Util.LerpUnclamped(-110, 55, ease01));
		UpperArmR.LimbRotate(FacingSign * (int)Util.LerpUnclamped(-100, 65, ease01));
		UpperArmL.Height = (int)(UpperArmL.Height * Util.LerpUnclamped(1.2f, 0.1f, ease010));
		UpperArmR.Height = (int)(UpperArmR.Height * Util.LerpUnclamped(1.2f, 0.1f, ease010));

		LowerArmL.LimbRotate(0);
		LowerArmR.LimbRotate(0);
		LowerArmL.Height = (int)(LowerArmL.Height * Util.LerpUnclamped(1.2f, 0.1f, ease010));
		LowerArmR.Height = (int)(LowerArmR.Height * Util.LerpUnclamped(1.2f, 0.1f, ease010));

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
		LowerArmL.Z = LowerArmL.Z.Abs();
		UpperArmR.Z = UpperArmR.Z.Abs();
		LowerArmR.Z = LowerArmR.Z.Abs();
		HandR.Z = POSE_Z_HAND;

		// Grab
		Rendering.HandGrabRotationL = Rendering.HandGrabRotationR =
			FacingSign * (int)Util.LerpUnclamped(67, 224, ease01);
		Rendering.HandGrabScaleL = Rendering.HandGrabScaleR =
			FacingSign * (int)Util.LerpUnclamped(1300, 100, ease010);
		Rendering.HandGrabAttackTwistL = Rendering.HandGrabAttackTwistR =
			(int)Util.LerpUnclamped(600, 200, frame01);
	}
	public static void EachHand_SlashOut () {

		float frame01 = AttackLerp;
		float ease01 = AttackEase;
		float ease010 = Util.PingPong(ease01 * 2f, 1f);

		AttackHeadDown(ease01, -500, 500, 500);
		ResetShoulderAndUpperArmPos();

		Rendering.BodyTwist = FacingSign * (int)(frame01 * 2000 - 1000);
		Rendering.HeadTwist = FacingSign * (int)(frame01 * 500 - 250);

		// Upper Arm
		UpperArmL.LimbRotate(FacingSign * (int)Util.LerpUnclamped(168, -40, ease01));
		UpperArmR.LimbRotate(FacingSign * (int)Util.LerpUnclamped(178, -50, ease01));
		UpperArmL.Height = (int)(UpperArmL.Height * Util.LerpUnclamped(1.2f, 0.1f, ease010));
		UpperArmR.Height = (int)(UpperArmR.Height * Util.LerpUnclamped(1.2f, 0.1f, ease010));

		LowerArmL.LimbRotate(0);
		LowerArmR.LimbRotate(0);
		LowerArmL.Height = (int)(LowerArmL.Height * Util.LerpUnclamped(1.2f, 0.1f, ease010));
		LowerArmR.Height = (int)(LowerArmR.Height * Util.LerpUnclamped(1.2f, 0.1f, ease010));

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
		LowerArmL.Z = LowerArmL.Z.Abs();
		UpperArmR.Z = (frame01 < 0.5f ? -1 : 1) * UpperArmR.Z.Abs();
		LowerArmR.Z = (frame01 < 0.5f ? -1 : 1) * LowerArmR.Z.Abs();
		HandR.Z = POSE_Z_HAND;

		// Grab
		Rendering.HandGrabRotationL = Rendering.HandGrabRotationR =
			FacingSign * (int)Util.LerpUnclamped(197, 128, ease01);
		Rendering.HandGrabScaleL = Rendering.HandGrabScaleR =
			FacingSign * (int)Util.LerpUnclamped(-1300, -100, ease010);
		Rendering.HandGrabAttackTwistL = Rendering.HandGrabAttackTwistR =
			(int)Util.LerpUnclamped(600, 200, frame01);

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
	public static void Polearm_SmashUp () {

		float ease01 = AttackEase;

		AttackHeadDown(1f - ease01 + 0.5f, -100, 500, 500);
		ResetShoulderAndUpperArmPos();

		// Upper Arm
		int uRotA = (int)Util.LerpUnclamped(63, -130, ease01);
		int uRotB = (int)Util.LerpUnclamped(43, -79, ease01);
		UpperArmL.LimbRotate(FacingRight ? uRotA : -uRotB);
		UpperArmR.LimbRotate(FacingRight ? uRotB : -uRotA);

		// Lower Arm
		int lRotA = (int)Util.LerpUnclamped(-75, 0, ease01);
		int lRotB = (int)Util.LerpUnclamped(0, -98, ease01);
		LowerArmL.LimbRotate(FacingRight ? lRotA : -lRotB);
		LowerArmR.LimbRotate(FacingRight ? lRotB : -lRotA);

		// Hand
		HandL.LimbRotate(FacingSign);
		HandR.LimbRotate(FacingSign);

		// Leg
		AttackLegShake(ease01);

		// Grab
		Rendering.HandGrabRotationL = Rendering.HandGrabRotationR =
			FacingSign * (int)Util.LerpUnclamped(130, 10, ease01);
		Rendering.HandGrabScaleL = Rendering.HandGrabScaleR =
			FacingSign * (int)Util.LerpUnclamped(1000, 1300, ease01);

		// Z
		UpperArmL.Z = UpperArmL.Z.Abs();
		LowerArmL.Z = LowerArmL.Z.Abs();
		UpperArmR.Z = UpperArmR.Z.Abs();
		LowerArmR.Z = LowerArmR.Z.Abs();
		HandR.Z = POSE_Z_HAND;

	}
	public static void Polearm_SlashIn () => DoubleHanded_SlashIn();
	public static void Polearm_SlashOut () => DoubleHanded_SlashOut();

}
