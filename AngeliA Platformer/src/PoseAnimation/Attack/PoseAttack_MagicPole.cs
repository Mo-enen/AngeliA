using AngeliA;

namespace AngeliA.Platformer;

public class PoseAttack_MagicPole : PoseAnimation {
	public static readonly int TYPE_ID = typeof(PoseAttack_MagicPole).AngeHash();
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		Wave();
	}
	public static void Wave () {

		float ease01 = AttackEase;

		if (IsChargingAttack) {
			AttackHeadDown(ease01, 100, 800, 100);
		} else {
			AttackHeadDown(ease01, 200, 300, 200);
		}
		ResetShoulderAndUpperArmPos();

		// Upper Arm
		float armGrowAmount = IsChargingAttack ? 1f - ease01 : ease01;
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
		int gRot = FacingSign * (int)Util.LerpUnclamped(-5, 60, ease01);
		int gScl = FacingSign * (int)Util.LerpUnclamped(1000, 1100, ease01);
		Rendering.HandGrabRotationL.Override(gRot);
		Rendering.HandGrabRotationR.Override(gRot);
		Rendering.HandGrabScaleL.Override(gScl);
		Rendering.HandGrabScaleR.Override(gScl);

		// Z
		UpperArmL.Z = LowerArmL.Z = FrontSign * UpperArmL.Z.Abs();
		UpperArmR.Z = LowerArmR.Z = FrontSign * UpperArmR.Z.Abs();
		HandL.Z = FrontSign * POSE_Z_HAND;
		HandR.Z = FrontSign * POSE_Z_HAND;

		// Leg
		AttackLegShake(ease01);

	}
}