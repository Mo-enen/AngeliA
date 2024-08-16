namespace AngeliA;

public class PoseAttack_Scratch : PoseAnimation {
	public override void Animate (PoseCharacter character) {
		base.Animate(character);
		if (Attackness.LastAttackCharged) {
			PoseAttack_Wave.EachHand_SmashDown();
			return;
		}
		Attackness.AttackStyleLoop = 3;
		int style = Attackness.AttackStyleIndex % Attackness.AttackStyleLoop;
		switch (style) {
			default:
				ScratchIn();
				break;
			case 1:
				ScratchOut();
				break;
			case 2:
				ScratchUp();
				break;
		}
	}
	public static void ScratchIn () {

		float ease01 = AttackEase;
		AttackHeadDown(ease01, 1000, 500, 500);
		ResetShoulderAndUpperArmPos();

		// Arm
		int rotUA = (int)Util.LerpUnclamped(6, 137, ease01);
		int rotUB = (int)Util.LerpUnclamped(-46, 90, ease01);
		UpperArmL.LimbRotate(FacingRight ? rotUA : -rotUB);
		UpperArmR.LimbRotate(FacingRight ? rotUB : -rotUA);
		UpperArmL.Height = UpperArmL.Height * (FacingRight ? 500 : 1200) / 1000;
		UpperArmR.Height = UpperArmR.Height * (FacingRight ? 1200 : 500) / 1000;

		int rotLA = (int)Util.LerpUnclamped(-12, -180, ease01);
		int rotLB = (int)Util.LerpUnclamped(-24, 27, ease01);
		LowerArmL.LimbRotate(FacingRight ? rotLA : -rotLB);
		LowerArmR.LimbRotate(FacingRight ? rotLB : -rotLA);
		LowerArmL.Height = LowerArmL.Height * (FacingRight ? 500 : 1200) / 1000;
		LowerArmR.Height = LowerArmR.Height * (FacingRight ? 1200 : 500) / 1000;
		HandL.LimbRotate(FacingSign);
		HandR.LimbRotate(FacingSign);

		// Leg
		AttackLegShake(ease01);

		// Grab Rotation
		int rotA = (int)Util.LerpUnclamped(101, 69, ease01);
		int rotB = (int)Util.LerpUnclamped(54, 94, ease01);

		Target.HandGrabRotationL = LowerArmL.Rotation + (FacingRight ? rotA : -rotB);
		Target.HandGrabRotationR = LowerArmR.Rotation + (FacingRight ? rotB : -rotA);
		Target.HandGrabScaleL = FacingRight ? 700 : -1300;
		Target.HandGrabScaleR = FacingRight ? 1300 : -700;

		// Z
		UpperArmL.Z = -FacingSign * FrontSign * UpperArmL.Z.Abs();
		UpperArmR.Z = FacingSign * FrontSign * UpperArmR.Z.Abs();
		LowerArmL.Z = -FacingSign * FrontSign * LowerArmL.Z.Abs();
		LowerArmR.Z = FacingSign * FrontSign * LowerArmR.Z.Abs();
		HandL.Z = -FacingSign * POSE_Z_HAND;
		HandR.Z = FacingSign * POSE_Z_HAND;

	}
	public static void ScratchOut () {

		float ease01 = AttackEase;
		AttackHeadDown(ease01, 1000, 500, 500);
		ResetShoulderAndUpperArmPos();

		// Arm
		int rotUA = (int)Util.LerpUnclamped(137, -46, ease01);
		int rotUB = (int)Util.LerpUnclamped(-44, 35, ease01);
		UpperArmL.LimbRotate(FacingRight ? rotUA : -rotUB);
		UpperArmR.LimbRotate(FacingRight ? rotUB : -rotUA);
		UpperArmL.Height = UpperArmL.Height * (FacingRight ? 1200 : 500) / 1000;
		UpperArmR.Height = UpperArmR.Height * (FacingRight ? 500 : 1200) / 1000;

		int rotLA = (int)Util.LerpUnclamped(-180, -45, ease01);
		int rotLB = (int)Util.LerpUnclamped(23, 46, ease01);
		LowerArmL.LimbRotate(FacingRight ? rotLA : -rotLB);
		LowerArmR.LimbRotate(FacingRight ? rotLB : -rotLA);
		LowerArmL.Height = LowerArmL.Height * (FacingRight ? 1200 : 500) / 1000;
		LowerArmR.Height = LowerArmR.Height * (FacingRight ? 500 : 1200) / 1000;

		HandL.LimbRotate(FacingSign);
		HandR.LimbRotate(FacingSign);

		// Leg
		AttackLegShake(ease01);

		// Grab Rotation
		int rotA = (int)Util.LerpUnclamped(93, 72, ease01);
		int rotB = (int)Util.LerpUnclamped(94, 54, ease01);
		Target.HandGrabRotationL = LowerArmL.Rotation + (FacingRight ? rotA : -rotB);
		Target.HandGrabRotationR = LowerArmR.Rotation + (FacingRight ? rotB : -rotA);
		Target.HandGrabScaleL = FacingRight ? 1300 : -700;
		Target.HandGrabScaleR = FacingRight ? 700 : -1300;

		// Z
		UpperArmL.Z = FacingSign * FrontSign * UpperArmL.Z.Abs();
		UpperArmR.Z = -FacingSign * FrontSign * UpperArmR.Z.Abs();
		LowerArmL.Z = FacingSign * FrontSign * LowerArmL.Z.Abs();
		LowerArmR.Z = -FacingSign * FrontSign * LowerArmR.Z.Abs();
		HandL.Z = FacingSign * POSE_Z_HAND;
		HandR.Z = -FacingSign * POSE_Z_HAND;

	}
	public static void ScratchUp () {

		float ease01 = AttackEase;
		AttackHeadDown(1f - ease01 + 0.5f, -500, 500, 500);
		ResetShoulderAndUpperArmPos();

		// Arm
		int rotUA = (int)Util.LerpUnclamped(-82, 137, ease01);
		int rotUB = (int)Util.LerpUnclamped(-69, 142, ease01);
		UpperArmL.LimbRotate(FacingRight ? rotUA : -rotUB);
		UpperArmR.LimbRotate(FacingRight ? rotUB : -rotUA);
		UpperArmL.Height = UpperArmL.Height * (FacingRight ? 1200 : 500) / 1000;
		UpperArmR.Height = UpperArmR.Height * (FacingRight ? 500 : 1200) / 1000;

		int rotLA = (int)Util.LerpUnclamped(-36, 0, ease01);
		int rotLB = (int)Util.LerpUnclamped(-39, 0, ease01);
		LowerArmL.LimbRotate(FacingRight ? rotLA : -rotLB);
		LowerArmR.LimbRotate(FacingRight ? rotLB : -rotLA);
		LowerArmL.Height = LowerArmL.Height * (FacingRight ? 1200 : 500) / 1000;
		LowerArmR.Height = LowerArmR.Height * (FacingRight ? 500 : 1200) / 1000;

		HandL.LimbRotate(FacingSign);
		HandR.LimbRotate(FacingSign);

		// Leg
		AttackLegShake(ease01);

		// Grab
		Target.HandGrabRotationL = UpperArmL.Rotation + FacingSign * 90;
		Target.HandGrabRotationR = UpperArmR.Rotation + FacingSign * 90;
		Target.HandGrabScaleL = FacingSign * 1300;
		Target.HandGrabScaleR = FacingSign * 1300;

		// Z
		UpperArmL.Z = FacingSign * FrontSign * UpperArmL.Z.Abs();
		UpperArmR.Z = -FacingSign * FrontSign * UpperArmR.Z.Abs();
		LowerArmL.Z = FacingSign * FrontSign * LowerArmL.Z.Abs();
		LowerArmR.Z = -FacingSign * FrontSign * LowerArmR.Z.Abs();
		HandL.Z = FacingSign * POSE_Z_HAND;
		HandR.Z = -FacingSign * POSE_Z_HAND;

	}
}
