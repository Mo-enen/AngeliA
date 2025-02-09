using AngeliA;

namespace AngeliA.Platformer;

public class PoseAttack_WavePolearm_SmashOnly : PoseAttack_WavePolearm {
	public static new readonly int TYPE_ID = typeof(PoseAttack_WavePolearm_SmashOnly).AngeHash();
	public override int StyleIndex => 0;
}

public class PoseAttack_WavePolearm : PoseAnimation {

	public static readonly int TYPE_ID = typeof(PoseAttack_WavePolearm).AngeHash();
	public virtual int StyleIndex => Attackness.LastAttackCharged ? 0 : Attackness.AttackStyleIndex;

	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		switch (StyleIndex % 4) {
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
	public static void SmashUp () {

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
