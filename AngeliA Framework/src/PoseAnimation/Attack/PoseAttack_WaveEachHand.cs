namespace AngeliA;

public class PoseAttack_WaveEachHand_SmashOnly : PoseAttack_WaveEachHand {
	public static new readonly int TYPE_ID = typeof(PoseAttack_WaveEachHand_SmashOnly).AngeHash();
	public override int StyleIndex => 0;
}

public class PoseAttack_WaveEachHand : PoseAnimation {

	public static readonly int TYPE_ID = typeof(PoseAttack_WaveEachHand).AngeHash();
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
	public static void SmashUp () {

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
	public static void SlashIn () {

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
	public static void SlashOut () {

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

}
