namespace AngeliA;

public class PoseAnimation_Walk : PoseAnimation {
	private static readonly float[] EASE = [0f, 0.03125f, 0.125f, 0.28125f, 0.5f, 0.71875f, 0.875f, 0.96875f, 1f, 0.96875f, 0.875f, 0.71875f, 0.5f, 0.28125f, 0.125f, 0.03125f, 0f, 0.04081633f, 0.1632653f, 0.3673469f, 0.6326531f, 0.8367347f, 0.9591837f, 1f, 0f, 0.04081633f, 0.1632653f, 0.3673469f, 0.6326531f, 0.8367347f, 0.9591837f, 1f,];
	private static readonly int[,] ROTS = { { -20, 20, 25, -25, 0, 0, }, { -17, 17, 21, -25, 0, 0, }, { -15, 15, 17, -27, 30, 20, }, { -7, 7, 17, -15, 45, 10, }, { 0, 0, -5, -5, 60, 0, }, { 7, -7, -5, 7, 75, 0, }, { 15, -15, -27, 17, 90, 0, }, { 17, -17, -26, 21, 45, 0, }, { 20, -20, -25, 25, 0, 0, }, { 17, -17, -26, 21, 10, 15, }, { 15, -15, -27, 17, 20, 30, }, { 7, -7, -15, 7, 10, 45, }, { 0, 0, -5, -5, 0, 60, }, { -7, 7, 5, -10, 0, 75, }, { -15, 15, 17, -27, 0, 90, }, { -17, 17, 21, -26, 0, 45, }, };
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);

		const int FRAME_LENGTH = 16;

		int runSpeed = Movement.WalkSpeed.FinalValue.Abs().GreaterOrEquel(1);
		int arrFrame = Util.RemapUnclamped(
			0, FRAME_LENGTH,
			0, FRAME_LENGTH * runSpeed / 47,
			CurrentAnimationFrame
		).UMod(FRAME_LENGTH);

		float ease = EASE[arrFrame];
		float easeDouble = EASE[arrFrame + FRAME_LENGTH];
		int legOffsetX = (int)Util.LerpUnclamped(
			0f, (Body.SizeX - Body.Border.horizontal - UpperLegL.SizeX) * 0.7f,
			FacingRight ? ease : 1f - ease
		);

		Rendering.PoseRootY += (int)(easeDouble * A2G);

		// Arm
		UpperArmL.LimbRotate(ROTS[arrFrame, 0] * FacingSign);
		UpperArmR.LimbRotate(ROTS[arrFrame, 1] * FacingSign);

		LowerArmL.LimbRotate(0);
		LowerArmR.LimbRotate(0);

		HandL.LimbRotate(0);
		HandR.LimbRotate(1);

		// Leg
		UpperLegL.X += legOffsetX;
		UpperLegL.LimbRotate(ROTS[arrFrame, 2] * FacingSign);

		UpperLegR.X -= legOffsetX;
		UpperLegR.LimbRotate(ROTS[arrFrame, 3] * FacingSign);

		LowerLegL.LimbRotate(ROTS[arrFrame, 4] * FacingSign);
		LowerLegR.LimbRotate(ROTS[arrFrame, 5] * FacingSign);

		FootL.LimbRotate(FacingRight ? 0 : 1);
		FootR.LimbRotate(FacingRight ? 0 : 1);

		// Final
		Rendering.HandGrabRotationL = LowerArmL.Rotation + FacingSign * 90;
		Rendering.HandGrabRotationR = LowerArmR.Rotation + FacingSign * 90;
	}
}
