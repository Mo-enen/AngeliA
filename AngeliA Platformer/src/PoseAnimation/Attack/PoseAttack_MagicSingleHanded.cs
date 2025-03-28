using AngeliA;

namespace AngeliA.Platformer;

public class PoseAttack_MagicSingleHanded : PoseAnimation {
	public static readonly int TYPE_ID = typeof(PoseAttack_MagicSingleHanded).AngeHash();
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		Wave();
	}
	public static void Wave () {

		float ease01 = AttackEase;

		AttackHeadDown(ease01, 200, 300, 200);
		ResetShoulderAndUpperArmPos();

		// Arm L
		UpperArmL.LimbRotate((int)(ease01 * 20f));
		LowerArmL.LimbRotate((int)(ease01 * -5f));
		HandL.LimbRotate(FacingSign);

		// Arm R
		UpperArmR.LimbRotate(FacingSign * (int)Util.LerpUnclamped(-165, -80, ease01));
		LowerArmR.LimbRotate(0);
		HandR.LimbRotate(FacingSign);

		// Z
		UpperArmR.Z = LowerArmR.Z = UpperArmR.Z.Abs();
		HandR.Z = POSE_Z_HAND;

		// Grab
		int gRot = FacingSign * (int)Util.LerpUnclamped(-60, 90, ease01);
		int gScl = FacingSign * (int)Util.LerpUnclamped(1000, 1200, ease01);
		Rendering.HandGrabRotationL.Override(gRot);
		Rendering.HandGrabRotationR.Override(gRot);
		Rendering.HandGrabScaleL.Override(gScl);
		Rendering.HandGrabScaleR.Override(gScl);

		// Leg
		AttackLegShake(ease01);
	}
}