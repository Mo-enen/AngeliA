namespace AngeliA;

public class PoseAnimation_Idle : PoseAnimation {

	public static readonly int TYPE_ID = typeof(PoseAnimation_Idle).AngeHash();

	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);

		int frameLoop = Target.TypeID.UMod(128) + 96;
		float lerp01 = (CurrentAnimationFrame + Target.TypeID).UMod(frameLoop) * 2f / frameLoop;
		float ease010 = Ease.InOutCirc(lerp01 < 1f ? lerp01 : 2f - lerp01);

		int oneEase = (int)(ease010 * A2G);
		int halfEase = (int)(ease010 * A2G / 2);
		int bodyBorderL = FacingRight ? Body.Border.left : Body.Border.right;
		int bodyBorderR = FacingRight ? Body.Border.right : Body.Border.left;

		Body.Width += FacingSign * halfEase;
		Body.Height -= oneEase;
		Head.Y -= oneEase;

		int ARM_SHIFT_XL = FacingRight ? A2G * 2 / 3 : 0;
		int ARM_SHIFT_XR = FacingRight ? 0 : A2G * 2 / 3;

		ShoulderL.X = Body.X - Body.Width.Abs() / 2 + bodyBorderL;
		ShoulderL.Y -= oneEase;
		ShoulderR.X = Body.X + Body.Width.Abs() / 2 - bodyBorderR;
		ShoulderR.Y -= oneEase;

		// Arm
		UpperArmL.X = ShoulderL.X;
		UpperArmL.Y -= oneEase;

		UpperArmR.X = ShoulderR.X;
		UpperArmR.Y -= oneEase;

		LowerArmL.X = UpperArmL.X - ARM_SHIFT_XL;
		LowerArmL.Y -= oneEase;

		LowerArmR.X = UpperArmR.X + ARM_SHIFT_XR;
		LowerArmR.Y -= oneEase;

		// Hand
		HandL.X -= halfEase / 2 + ARM_SHIFT_XL;
		HandL.Y -= oneEase;

		HandR.X += halfEase / 2 + ARM_SHIFT_XR;
		HandR.Y -= oneEase;

		// Leg
		LowerLegL.Y += 1;
		LowerLegR.Y += 1;
		LowerLegL.Height += 1;
		LowerLegR.Height += 1;

		// Final
		Rendering.HandGrabRotationL.Override( LowerArmL.Rotation + FacingSign * 90);
		Rendering.HandGrabRotationR.Override( LowerArmR.Rotation + FacingSign * 90);

	}
}
