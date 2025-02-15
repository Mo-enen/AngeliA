namespace AngeliA;

public class PoseAnimation_SwimIdle : PoseAnimation {
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);

		int frame0121 = CurrentAnimationFrame.UMod(64) / 16;

		// Arm
		if (frame0121 == 3) frame0121 = 1;
		int easeArm = frame0121 * A2G / 2;

		UpperArmL.LimbRotate(FacingSign * -90);
		UpperArmL.Height = FacingRight ?
			UpperArmL.SizeY + 2 * A2G - easeArm :
			UpperArmL.SizeY / 2 + easeArm;

		UpperArmR.LimbRotate(FacingSign * -90);
		UpperArmR.Height = FacingRight ?
			UpperArmR.SizeY / 2 + easeArm :
			UpperArmR.SizeY + 2 * A2G - easeArm;

		LowerArmL.LimbRotate(0);
		LowerArmR.LimbRotate(0);

		HandL.LimbRotate(-FacingSign);
		HandL.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);
		HandL.Height = HandL.SizeY - frame0121 * A2G / 3;

		HandR.LimbRotate(-FacingSign);
		HandR.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);
		HandR.Height = HandR.SizeY - frame0121 * A2G / 3;

		// Leg
		UpperLegL.LimbRotate((frame0121 - 1) * 10);
		UpperLegR.LimbRotate((frame0121 - 1) * -10);

		LowerLegL.LimbRotate((frame0121 - 1) * -10);
		LowerLegR.LimbRotate((frame0121 - 1) * 10);
		FootL.LimbRotate(-FacingSign);
		FootR.LimbRotate(-FacingSign);

		// Final
		Rendering.HandGrabRotationL.Override( LowerArmL.Rotation + FacingSign * 90);
		Rendering.HandGrabRotationR.Override( LowerArmR.Rotation + FacingSign * 90);
	}
}
