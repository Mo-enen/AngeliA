namespace AngeliA;

public class PoseAnimation_Slide : PoseAnimation {
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);

		bool alt = (CurrentAnimationFrame / 4) % 2 == 0;

		Head.X -= FacingSign * 2 * A2G;
		Head.Width *= -1;

		Body.X -= FacingSign * A2G;

		if (FacingRight) {
			UpperLegL.X += A2G;
		} else {
			UpperLegR.X -= A2G;
		}

		Head.Rotation = FacingSign * -6;
		Body.Rotation = FacingSign * 8;

		// Arm
		UpperArmL.Z = UpperArmL.Z.Abs();
		UpperArmR.Z = UpperArmR.Z.Abs();
		LowerArmL.Z = LowerArmL.Z.Abs();
		LowerArmR.Z = LowerArmR.Z.Abs();
		HandL.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);
		HandR.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);

		UpperArmL.LimbRotate(FacingRight ? 70 : 175);
		UpperArmR.LimbRotate(FacingRight ? -175 : -70);

		if (FacingRight) {
			UpperArmR.Height = UpperArmR.Height * 3 / 2 + (alt ? A2G / 5 : 0);
		} else {
			UpperArmL.Height = UpperArmL.Height * 3 / 2 + (alt ? A2G / 5 : 0);
		}

		LowerArmL.LimbRotate(FacingRight ? -65 : 0);
		LowerArmR.LimbRotate(FacingRight ? 0 : 65);

		if (FacingRight) {
			LowerArmR.Height = LowerArmR.Height * 3 / 2 + (alt ? A2G / 5 : 0);
		} else {
			LowerArmL.Height = LowerArmL.Height * 3 / 2 + (alt ? A2G / 5 : 0);
		}

		HandL.LimbRotate(-FacingSign);
		HandR.LimbRotate(-FacingSign);

		// Leg
		UpperLegL.LimbRotate((FacingRight ? -15 : 30) + (alt ? 1 : 0), FacingRight ? 1000 : 500);
		UpperLegR.LimbRotate((FacingRight ? -30 : 15) + (alt ? 0 : 1), FacingRight ? 500 : 1000);

		LowerLegL.LimbRotate(FacingRight ? -30 : -20, FacingRight ? 1000 : 300);
		LowerLegR.LimbRotate(FacingRight ? 20 : 30, FacingRight ? 300 : 1000);

		FootL.LimbRotate(-FacingSign);
		FootR.LimbRotate(-FacingSign);

		// Final
		Rendering.HandGrabRotationL = LowerArmL.Rotation + FacingSign * 90;
		Rendering.HandGrabRotationR = LowerArmR.Rotation + FacingSign * 90;
	}
}
