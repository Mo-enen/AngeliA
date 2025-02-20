using AngeliA;

namespace AngeliA.Platformer;

public class PoseAnimation_Sit : PoseAnimation {

	public static readonly int TYPE_ID = typeof(PoseAnimation_Sit).AngeHash();

	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		Sit();
	}

	public static void Sit () {

		Rendering.PoseRootY = 0;

		ResetShoulderAndUpperArmPos();

		// Arm
		UpperArmL.Height = UpperArmL.SizeY;
		UpperArmR.Height = UpperArmR.SizeY;
		LowerArmL.Height = LowerArmL.SizeY;
		LowerArmR.Height = LowerArmR.SizeY;
		UpperArmL.LimbRotate(-20);
		UpperArmR.LimbRotate(20);
		LowerArmL.LimbRotate(0);
		LowerArmR.LimbRotate(0);
		HandL.LimbRotate(0);
		HandR.LimbRotate(1);

		// Leg
		UpperLegL.LimbRotate(FacingSign * -80);
		UpperLegR.LimbRotate(FacingSign * -80);
		LowerLegL.LimbRotate(FacingSign * 120);
		LowerLegR.LimbRotate(FacingSign * 120);
		FootL.LimbRotate(-FacingSign);
		FootR.LimbRotate(-FacingSign);

		// Misc
		Rendering.BodyTwist = FacingSign * 1000;
		Rendering.HeadTwist = FacingSign * 100;

		// Grab Rot
		Rendering.HandGrabRotationL.Override(LowerArmL.Rotation + 90);
		Rendering.HandGrabRotationR.Override(LowerArmR.Rotation + 90);
		Rendering.HandGrabScaleL.Override(500);
		Rendering.HandGrabScaleR.Override(500);

		// Z
		UpperLegL.Z = FacingSign * 6;
		UpperLegR.Z = FacingSign * -6;
		LowerLegL.Z = FacingSign * 7;
		LowerLegR.Z = FacingSign * -7;
		FootL.Z = FacingSign * 8;
		FootR.Z = FacingSign * -8;

	}

}
