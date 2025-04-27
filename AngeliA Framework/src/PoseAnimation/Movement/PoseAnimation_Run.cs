namespace AngeliA;

public class PoseAnimation_Run : PoseAnimation {

	public static readonly int TYPE_ID = typeof(PoseAnimation_Run).AngeHash();

	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);

		const int FRAME_LENGTH = 16;

		int runSpeed = Movement.RunSpeed.FinalValue.Abs().GreaterOrEquel(1);
		float arrFrameF = CurrentAnimationFrame * runSpeed / 57f;

		float frame01 = (arrFrameF / FRAME_LENGTH).UMod(1f);
		float frame010 = Util.PingPong(frame01 * 2f, 1f);
		float frame101 = 1f - frame010;

		float frame01Alt = ((arrFrameF / FRAME_LENGTH) - 0.3f).UMod(1f);
		float frame010Alt = Util.PingPong(frame01Alt * 2f, 1f);
		float frame101Alt = 1f - frame010Alt;

		float easeDouble = Ease.InCubic((frame01 * 2f) % 1f);
		int legOffsetX = (int)Util.LerpUnclamped(
			0f, (Body.SizeX - Body.Border.horizontal - UpperLegL.SizeX) * 0.9f,
			FacingRight ? frame010 : 1f - frame010
		);

		int twist = (int)Util.LerpUnclamped(1000f, -1000f, frame01 < 0.5f ? frame01 * 2f : 2f - 2f * frame01);
		Rendering.PoseRootY += (int)((1f - easeDouble) * A2G * 2) - A2G;
		Rendering.BodyTwist = twist;

		int bodyRot = Target.DeltaPositionX.Clamp(-42, 42) * 2 / 3;
		int localTurningFrame = Game.GlobalFrame - Movement.LastFacingChangeFrame;
		if (localTurningFrame < 24) {
			bodyRot = bodyRot * localTurningFrame / 24;
		}
		Head.Rotation = (twist + FacingSign * 500) * bodyRot / -12000 - bodyRot * 2 / 3;
		Body.Rotation = bodyRot;

		// Arm
		UpperArmL.LimbRotate(Util.LerpUnclamped(-10f, 80f, frame010).RoundToInt() * FacingSign - bodyRot / 2);
		UpperArmR.LimbRotate(Util.LerpUnclamped(-10f, 80f, frame101).RoundToInt() * FacingSign - bodyRot / 2);
		var forwardUpperArm = (FacingRight ? UpperArmR : UpperArmL);
		forwardUpperArm.Height -= forwardUpperArm.Height * bodyRot.Abs() / 42;

		LowerArmL.LimbRotate(Util.LerpUnclamped(-65f, -90f, frame010).RoundToInt() * FacingSign, 500);
		LowerArmR.LimbRotate(Util.LerpUnclamped(-65f, -90f, frame101).RoundToInt() * FacingSign, 500);
		var forwardLowerArm = (FacingRight ? LowerArmR : LowerArmL);
		forwardLowerArm.Height -= forwardLowerArm.Height * bodyRot.Abs() / 42;

		HandL.LimbRotate(-FacingSign);
		HandR.LimbRotate(-FacingSign);

		// Leg
		UpperLegL.X += legOffsetX;
		UpperLegR.X -= legOffsetX;

		UpperLegL.LimbRotate(
			Util.LerpUnclamped(55f, -65f, frame010).RoundToInt() * FacingSign + bodyRot / 3
		);
		UpperLegR.LimbRotate(
			Util.LerpUnclamped(55f, -65f, frame101).RoundToInt() * FacingSign + bodyRot / 3
		);
		LowerLegL.LimbRotate(
			Util.LerpUnclamped(90f, 0f, Ease.InOutBack(frame010Alt)).RoundToInt() * FacingSign
		);
		LowerLegR.LimbRotate(
			Util.LerpUnclamped(90f, 0f, Ease.InOutBack(frame101Alt)).RoundToInt() * FacingSign
		);
		FootL.LimbRotate(-FacingSign);
		FootR.LimbRotate(-FacingSign);

		// Z
		UpperLegL.Z = 1;
		UpperLegR.Z = 1;
		LowerLegL.Z = 2;
		LowerLegR.Z = 2;
		FootL.Z = 3;
		FootR.Z = 3;

		// Final
		Rendering.HandGrabRotationL.Override(LowerArmL.Rotation + FacingSign * 90);
		Rendering.HandGrabRotationR.Override(LowerArmR.Rotation + FacingSign * 90);

	}

}
