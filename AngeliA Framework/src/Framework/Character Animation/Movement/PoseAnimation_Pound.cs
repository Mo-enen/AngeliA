namespace AngeliA;

public class PoseAnimation_Pound : PoseAnimation {
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);

		bool alt = CurrentAnimationFrame % 8 < 4;

		HandL.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);
		HandR.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);
		UpperLegL.Z = -3;
		UpperLegR.Z = -3;
		LowerLegL.Z = FacingFront ? 4 : -4;
		LowerLegR.Z = FacingFront ? 4 : -4;
		FootL.Z = FacingFront ? 5 : -5;
		FootR.Z = FacingFront ? 5 : -5;

		Rendering.PoseRootY = A2G;

		Head.X += FacingSign * A2G;
		Head.Y -= A2G;

		// Arm
		UpperArmL.LimbRotate(135 + (alt ? 15 : 0), 500);
		UpperArmR.LimbRotate(-135 + (alt ? 15 : 0), 500);

		LowerArmL.LimbRotate(-60 + (alt ? -15 : 0), 500);
		LowerArmR.LimbRotate(60 + (alt ? -15 : 0), 500);

		HandL.LimbRotate(FacingSign);
		HandR.LimbRotate(FacingSign);

		// Leg
		UpperLegL.LimbRotate((FacingRight ? -125 : 155) + (alt ? 10 : 0), FacingRight ? 1000 : 500);
		UpperLegR.LimbRotate((FacingRight ? -155 : 125) + (alt ? 10 : 0), FacingRight ? 500 : 1000);

		LowerLegL.LimbRotate((FacingRight ? 100 : -120) + (alt ? -5 : 0));
		LowerLegR.LimbRotate((FacingRight ? 120 : -100) + (alt ? -5 : 0));

		FootL.LimbRotate(-FacingSign);
		FootR.LimbRotate(-FacingSign);

		// Final
		Rendering.HandGrabRotationL = LowerArmL.Rotation - 90;
		Rendering.HandGrabRotationR = LowerArmR.Rotation + 90;
	}
}
