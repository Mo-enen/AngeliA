namespace AngeliA;

public class PoseAnimation_GrabTop : PoseAnimation {
	public override void Animate (PoseCharacter character) {
		base.Animate(character);
		int loop = Util.Max((700 / Target.GrabMoveSpeedX.FinalValue.Clamp(1, 1024)) / 4 * 4, 1);
		int arrFrame = (CurrentAnimationFrame.UMod(loop) / (loop / 4)) % 4;// 0123
		int pingpong = arrFrame == 3 ? 1 : arrFrame; // 0121
		int pingpongAlt = arrFrame == 2 ? 1 : arrFrame == 3 ? 0 : arrFrame + 1; // 1210

		UpperArmL.Z = UpperArmR.Z = (FacingFront ? 34 : -34);
		LowerArmL.Z = LowerArmR.Z = (FacingFront ? 35 : -35);
		HandL.Z = HandR.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);

		Target.PoseRootY += pingpongAlt * A2G;

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
		UpperLegL.X -= pingpongAlt * A2G / 4;
		UpperLegR.X += pingpongAlt * A2G / 4;

		LowerLegL.X -= pingpongAlt * A2G / 3;
		LowerLegR.X += pingpongAlt * A2G / 3;

		FootL.X -= pingpongAlt * A2G / 2;
		FootR.X += pingpongAlt * A2G / 2;

		// Final
		Target.HandGrabRotationL = LowerArmL.Rotation + FacingSign * 90;
		Target.HandGrabRotationR = LowerArmR.Rotation + FacingSign * 90;
	}
}
