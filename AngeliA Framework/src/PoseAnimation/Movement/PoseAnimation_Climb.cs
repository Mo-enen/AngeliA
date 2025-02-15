namespace AngeliA;

public class PoseAnimation_Climb : PoseAnimation {
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);

		int frameRate = Util.Max(560 / Movement.ClimbSpeedY.FinalValue.Clamp(1, 1024) / 8, 1);
		int aFrame = CurrentAnimationFrame.UMod(frameRate * 10 - 1) / frameRate;

		int delayFrame = (aFrame + 1) % 10;
		if (aFrame >= 5) aFrame = 8 - aFrame;
		if (delayFrame >= 5) delayFrame = 8 - delayFrame;
		aFrame = aFrame.Clamp(0, 4); // 0 1 2 3 4 3 2 1 0

		Rendering.PoseRootY -= (aFrame - 2).Abs() * A2G;
		Body.Rotation = aFrame * 2 - 4;

		// Arm
		UpperArmL.LimbRotate(((3 - delayFrame) * -35 + 135).Clamp(45, 135), 1000);
		UpperArmR.LimbRotate((delayFrame * 35 - 135).Clamp(-135, -45), 1000);

		LowerArmL.LimbRotate(180 - UpperArmL.Rotation, 1000);
		LowerArmR.LimbRotate(-180 - UpperArmR.Rotation, 1000);

		HandL.LimbRotate(1);
		HandR.LimbRotate(0);
		HandL.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);
		HandR.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);

		// Leg
		UpperLegL.LimbRotate((aFrame * 35).Clamp(0, 60), 500);
		UpperLegR.LimbRotate(((3 - aFrame) * -35).Clamp(-60, 0), 500);

		LowerLegL.LimbRotate(-UpperLegL.Rotation - 5, 1000);
		LowerLegR.LimbRotate(-UpperLegR.Rotation + 5, 1000);

		FootL.LimbRotate(-1);
		FootR.LimbRotate(1);

		// Final
		Rendering.HandGrabRotationL.Override(LowerArmL.Rotation + 90);
		Rendering.HandGrabRotationR.Override(LowerArmR.Rotation + 90);
		Rendering.HandGrabScaleL.Override(1000);
		Rendering.HandGrabScaleR.Override(1000);
	}
}
