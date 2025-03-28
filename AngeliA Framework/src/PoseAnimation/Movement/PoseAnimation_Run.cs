﻿namespace AngeliA;

public class PoseAnimation_Run : PoseAnimation {

	public static readonly int TYPE_ID = typeof(PoseAnimation_Run).AngeHash();
	private static readonly float[] EASE = [0f, 0.03125f, 0.125f, 0.28125f, 0.5f, 0.71875f, 0.875f, 0.96875f, 1f, 0.96875f, 0.875f, 0.71875f, 0.5f, 0.28125f, 0.125f, 0.03125f, 0f, 0.04081633f, 0.1632653f, 0.3673469f, 0.6326531f, 0.8367347f, 0.9591837f, 1f, 0f, 0.04081633f, 0.1632653f, 0.3673469f, 0.6326531f, 0.8367347f, 0.9591837f, 1f,];
	private static readonly int[,] ROTS = { { -10, 80, -65, -90, 45, -55, 80, 60, }, { -10, 80, -65, -90, 45, -55, 80, 60, }, { 1, 68, -68, -86, 32, -42, 90, 29, }, { 1, 68, -68, -86, 32, -42, 90, 29, }, { 35, 35, -77, -77, -5, -5, 90, 0, }, { 35, 35, -77, -77, -5, -5, 90, 0, }, { 68, 1, -86, -68, -42, 32, 90, 0, }, { 68, 1, -86, -68, -42, 32, 90, 0, }, { 80, -10, -90, -65, -55, 45, 60, 80, }, { 80, -10, -90, -65, -55, 45, 60, 80, }, { 68, 1, -86, -68, -42, 32, 29, 90, }, { 68, 1, -86, -68, -42, 32, 29, 90, }, { 35, 35, -77, -77, -5, -5, 0, 90, }, { 35, 35, -77, -77, -5, -5, 0, 90, }, { 1, 68, -68, -86, 32, -42, 0, 90, }, { 1, 68, -68, -86, 32, -42, 0, 90, }, };

	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);

		const int FRAME_LENGTH = 16;

		int runSpeed = Movement.RunSpeed.FinalValue.Abs().GreaterOrEquel(1);
		int arrFrame = Util.RemapUnclamped(
			0, FRAME_LENGTH,
			0, FRAME_LENGTH * runSpeed / 57,
			CurrentAnimationFrame
		).UMod(FRAME_LENGTH);

		float ease = EASE[arrFrame];
		float easeDouble = EASE[arrFrame + FRAME_LENGTH];
		int legOffsetX = (int)Util.LerpUnclamped(
			0f, (Body.SizeX - Body.Border.horizontal - UpperLegL.SizeX) * 0.9f,
			FacingRight ? ease : 1f - ease
		);
		float frame01 = (float)arrFrame / FRAME_LENGTH;

		Rendering.PoseRootY += (int)((1f - easeDouble) * A2G * 2);
		Rendering.BodyTwist = (int)Util.LerpUnclamped(1000f, -1000f, frame01 < 0.5f ? frame01 * 2f : 2f - 2f * frame01);

		int bodyRot = Target.DeltaPositionX.Clamp(-42, 42) * 2 / 3;
		int localTurningFrame = Game.GlobalFrame - Movement.LastFacingChangeFrame;
		if (localTurningFrame < 24) {
			bodyRot = bodyRot * localTurningFrame / 24;
		}
		Head.Rotation = -bodyRot * 2 / 3;
		Body.Rotation = bodyRot;

		// Arm
		UpperArmL.LimbRotate(ROTS[arrFrame, 0] * FacingSign - bodyRot / 2);
		UpperArmR.LimbRotate(ROTS[arrFrame, 1] * FacingSign - bodyRot / 2);

		LowerArmL.LimbRotate(ROTS[arrFrame, 2] * FacingSign, 500);
		LowerArmR.LimbRotate(ROTS[arrFrame, 3] * FacingSign, 500);

		HandL.LimbRotate(0);
		HandR.LimbRotate(1);

		// Leg
		UpperLegL.X += legOffsetX;
		UpperLegL.Z = 1;
		UpperLegL.LimbRotate(ROTS[arrFrame, 4] * FacingSign);

		UpperLegR.X -= legOffsetX;
		UpperLegR.Z = 1;
		UpperLegR.LimbRotate(ROTS[arrFrame, 5] * FacingSign);

		LowerLegL.LimbRotate(ROTS[arrFrame, 6] * FacingSign);
		LowerLegL.Z = 2;

		LowerLegR.LimbRotate(ROTS[arrFrame, 7] * FacingSign);
		LowerLegR.Z = 2;

		FootL.LimbRotate(-FacingSign);
		FootL.Z = 3;

		FootR.LimbRotate(-FacingSign);
		FootR.Z = 3;

		// Final
		Rendering.HandGrabRotationL.Override(LowerArmL.Rotation + FacingSign * 90);
		Rendering.HandGrabRotationR.Override(LowerArmR.Rotation + FacingSign * 90);

	}

}
