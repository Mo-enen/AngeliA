namespace AngeliA;

public class PoseAnimation_Fly : PoseAnimation {
	public override void Animate (PoseCharacter character) {
		base.Animate(character);

		int frame = CurrentAnimationFrame.UMod(16) / 2;
		int pingpong = frame < 4 ? frame : 8 - frame;
		int motionDelta = (Target.DeltaPositionX * 2).Clamp(-30, 30);

		Target.PoseRootY = (frame < 6 ? frame / 2 : 8 - frame) * -A2G + 2 * A2G;

		Head.Y = 0;

		Body.Height = A2G * 2;
		Hip.Y = Head.Y - Body.Height;
		Body.Y = Hip.Y + Hip.Height;

		// Arm
		ShoulderL.Y = Body.Y + Body.Height;
		ShoulderR.Y = Body.Y + Body.Height;

		UpperArmL.X -= pingpong * A2G / 4;
		UpperArmL.Y = Head.Y;
		UpperArmL.LimbRotate(motionDelta);
		UpperArmL.Height -= pingpong * A2G / 6;

		UpperArmR.X += pingpong * A2G / 4;
		UpperArmR.Y = Head.Y;
		UpperArmR.LimbRotate(motionDelta);
		UpperArmR.Height -= pingpong * A2G / 6;

		LowerArmL.LimbRotate(0);
		LowerArmR.LimbRotate(0);

		HandL.LimbRotate(0);
		HandR.LimbRotate(1);

		// Leg
		UpperLegL.X -= Body.Width / 2 + FacingSign * A2G;
		UpperLegR.X -= Body.Width / 2 + FacingSign * A2G;
		UpperLegL.Y = Head.Y + A2G * 5;
		UpperLegR.Y = Head.Y + A2G * 5;
		UpperLegL.Z = -34;
		UpperLegR.Z = -34;
		UpperLegL.LimbRotate(20 + motionDelta / 2);
		UpperLegR.LimbRotate(-20 + motionDelta / 2);

		LowerLegL.LimbRotate(pingpong * 4 - 20);
		LowerLegR.LimbRotate(-pingpong * 4 + 20);
		LowerLegL.Z = -33;
		LowerLegR.Z = -33;

		FootL.LimbRotate(-FacingSign);
		FootR.LimbRotate(-FacingSign);
		FootL.Z = -32;
		FootR.Z = -32;

		// Final
		Target.HandGrabRotationL = LowerArmL.Rotation + FacingSign * 60;
		Target.HandGrabRotationR = LowerArmR.Rotation + FacingSign * 60;
	}
}
