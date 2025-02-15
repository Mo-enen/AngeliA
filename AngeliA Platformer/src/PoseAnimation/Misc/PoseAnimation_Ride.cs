using AngeliA;

namespace AngeliA.Platformer;

public class PoseAnimation_Ride : PoseAnimation {

	public static readonly int TYPE_ID = typeof(PoseAnimation_Ride).AngeHash();

	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		Ride();
	}

	private static void Ride () {

		int bodyDeltaY = 2 * A2G;
		int velX = Movement.Target.VelocityX;

		Rendering.BasicRootY = Rendering.PoseRootY = -bodyDeltaY;

		UpperLegL.Y -= bodyDeltaY / 2;
		UpperLegR.Y -= bodyDeltaY / 2;

		Body.Rotation = FacingSign * 20;
		Head.Rotation = FacingSign * -16;
		Head.X = Body.X + FacingSign * A2G + velX.Clamp(-A2G * 3, A2G * 3);
		Body.Height /= 2;
		Head.Y = Body.Y + Body.Height;

		ResetShoulderAndUpperArmPos();

		int armLimbDeltaX = velX.Clamp(-A2G * 2, A2G * 2);
		ShoulderL.X += armLimbDeltaX;
		ShoulderR.X += armLimbDeltaX;
		UpperArmL.X += armLimbDeltaX;
		UpperArmR.X += armLimbDeltaX;

		// Arm
		UpperArmL.LimbRotate(FacingSign * -45);
		UpperArmR.LimbRotate(FacingSign * -45);
		LowerArmL.LimbRotate(0);
		LowerArmR.LimbRotate(0);
		HandL.LimbRotate(0);
		HandR.LimbRotate(0);

		// Leg
		int legAngle = velX.Abs().Clamp(30, 60);
		UpperLegL.LimbRotate(FacingSign * -legAngle);
		UpperLegR.LimbRotate(FacingSign * -legAngle);
		LowerLegL.LimbRotate(FacingSign * 2 * legAngle);
		LowerLegR.LimbRotate(FacingSign * 2 * legAngle);
		FootL.LimbRotate(0);
		FootR.LimbRotate(0);

		// Grab Rot
		Rendering.HandGrabRotationL.Override(LowerArmL.Rotation + FacingSign * 90);
		Rendering.HandGrabRotationR.Override(LowerArmR.Rotation + FacingSign * 90);
		Rendering.HandGrabScaleL.Override(FacingSign * 1000, 0, 64);
		Rendering.HandGrabScaleR.Override(FacingSign * 1000, 0, 64);

		// Z
		Body.Z = 1;
		Hip.Z = 1;
		UpperArmL.Z = FacingSign * 22;
		LowerArmL.Z = FacingSign * 28;
		HandL.Z = FacingSign * 32;
		UpperArmR.Z = FacingSign * -22;
		LowerArmR.Z = FacingSign * -28;
		HandR.Z = FacingSign * -32;
		UpperLegL.Z = FacingRight ? 2 : -22;
		LowerLegL.Z = FacingRight ? 1 : -21;
		FootL.Z = FacingRight ? 2 : -22;
		UpperLegR.Z = FacingRight ? -22 : 2;
		LowerLegR.Z = FacingRight ? -21 : 1;
		FootR.Z = FacingRight ? -22 : 2;

	}

}
