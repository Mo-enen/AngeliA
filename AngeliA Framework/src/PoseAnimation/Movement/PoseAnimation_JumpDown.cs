namespace AngeliA;

public class PoseAnimation_JumpDown : PoseAnimation {

	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);

		bool alt = CurrentAnimationFrame.UMod(8) >= 4;

		Rendering.PoseRootY -= A2G;
		Rendering.BodyTwist = FacingRight ? -400 : 400;

		if (alt) {
			Body.Height += A2G / 4;
			Head.Y += A2G / 4;
			UpperArmL.Y += A2G / 4;
			UpperArmR.Y += A2G / 4;
		}
		Head.Height -= A2G;

		int deltaRot = Target.DeltaPositionX.Clamp(-42, 42) / 2;
		Head.Rotation = deltaRot * 3 / 2;
		Body.Rotation = -deltaRot;

		// Arm
		UpperArmL.LimbRotate((alt ? 135 : 125) + deltaRot * 2);
		UpperArmR.LimbRotate((alt ? -125 : -135) + deltaRot * 2);

		LowerArmL.Z = LowerArmL.Z.Abs();
		LowerArmL.LimbRotate(alt ? 35 : 45);

		LowerArmR.Z = LowerArmR.Z.Abs();
		LowerArmR.LimbRotate(alt ? -45 : -35);

		HandL.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);
		HandL.LimbRotate(1);

		HandR.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);
		HandR.LimbRotate(-1);

		// Leg
		UpperLegL.X += FacingRight ? A2G / 2 : -A2G / 2;
		UpperLegL.LimbRotate(FacingRight ? 0 : 25);

		UpperLegR.X += FacingRight ? A2G / 2 : -A2G / 2;
		UpperLegR.LimbRotate(FacingRight ? -25 : 0);

		LowerLegL.LimbRotate(FacingRight ? 3 : -15);
		LowerLegL.Height = LowerLegL.SizeY * 3 / 4;
		LowerLegL.Z += 2;

		LowerLegR.LimbRotate(FacingRight ? 15 : -3);
		LowerLegR.Height = LowerLegR.SizeY * 3 / 4;
		LowerLegR.Z += 2;

		FootL.LimbRotate(FacingRight ? 0 : 1);
		FootL.Z += 2;

		FootR.LimbRotate(FacingRight ? 0 : 1);
		FootR.Z += 2;

		// Final
		Rendering.HandGrabRotationL.Override(LowerArmL.Rotation - FacingSign * 90);
		Rendering.HandGrabRotationR.Override(LowerArmR.Rotation - FacingSign * 90);
		Rendering.HandGrabScaleL.Override(FacingSign * 1000);
		Rendering.HandGrabScaleR.Override(FacingSign * 1000);
	}

}
