namespace AngeliA;

public class PoseAnimation_GrabTop : PoseAnimation {
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		int loop = Util.Max((700 / Movement.GrabMoveSpeedX.FinalValue.Clamp(1, 1024)) / 4 * 4, 1);
		int arrFrame = (CurrentAnimationFrame.UMod(loop) / (loop / 4)) % 4;// 0123
		int pingpong = arrFrame == 3 ? 1 : arrFrame; // 0121
		int pingpongAlt = arrFrame == 2 ? 1 : arrFrame == 3 ? 0 : arrFrame + 1; // 1210

		UpperArmL.Z = UpperArmR.Z = (FacingFront ? 34 : -34);
		LowerArmL.Z = LowerArmR.Z = (FacingFront ? 35 : -35);
		HandL.Z = HandR.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);
		Body.Rotation = (pingpong - 1) * 5;
		Rendering.PoseRootY += pingpongAlt * A2G;

		// Arm
		UpperArmL.LimbRotate(165 - pingpong * 6, 1500);
		UpperArmR.LimbRotate(-165 + pingpong * 6, 1500);

		UpperArmL.Height += A2G;
		UpperArmR.Height += A2G;

		LowerArmL.LimbRotate(180 - pingpong * 6 - UpperArmL.Rotation);
		LowerArmR.LimbRotate(-180 + pingpong * 6 - UpperArmR.Rotation);

		LowerArmL.Height += 2 * A2G;
		LowerArmR.Height += 2 * A2G;

		HandL.LimbRotate(1);
		HandR.LimbRotate(0);

		// Leg
		UpperLegL.LimbRotate(pingpong * 5);
		UpperLegR.LimbRotate(pingpong * -5);

		LowerLegL.LimbRotate(-3);
		LowerLegR.LimbRotate(3);

		FootL.LimbRotate(-FacingSign);
		FootR.LimbRotate(-FacingSign);

		// Final
		Rendering.HandGrabRotationL = LowerArmL.Rotation + FacingSign * 90;
		Rendering.HandGrabRotationR = LowerArmR.Rotation + FacingSign * 90;
	}
}
