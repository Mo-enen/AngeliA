namespace AngeliA;

public class PoseAttack_WaveDoubleHanded_SmashOnly : PoseAttack_WaveDoubleHanded {
	public static new readonly int TYPE_ID = typeof(PoseAttack_WaveDoubleHanded_SmashOnly).AngeHash();
	public override int StyleIndex => 0;
}

public class PoseAttack_WaveDoubleHanded : PoseAnimation {

	public static readonly int TYPE_ID = typeof(PoseAttack_WaveDoubleHanded).AngeHash();
	public virtual int StyleIndex => Attackness.LastAttackCharged ? 0 : Attackness.AttackStyleIndex % Attackness.AttackStyleLoop;

	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		Attackness.AttackStyleLoop = 4;
		switch (StyleIndex) {
			default:
				SmashDown();
				break;
			case 1:
				SmashUp();
				break;
			case 2:
				SlashIn();
				break;
			case 3:
				SlashOut();
				break;
		}
	}

	public static void SmashDown () {

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
	public static void SmashUp () {

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
	public static void SlashIn () {

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
	public static void SlashOut () {

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

}
