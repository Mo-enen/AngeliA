using AngeliA;

namespace AngeliA.Platformer;

public class PoseAnimation_PhotoPose : PoseAnimation {

	public static readonly int TYPE_ID = typeof(PoseAnimation_PhotoPose).AngeHash();

	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		switch (Attackness.AttackStyleIndex.UMod(1)) {
			case 0:
				Pose0();
				break;

		}
	}

	private void Pose0 () {

		Head.Rotation = -5;
		int bodyOffsetX = A2G;
		Head.X -= bodyOffsetX;
		Body.X -= bodyOffsetX;
		Hip.X -= bodyOffsetX;
		UpperLegL.X -= bodyOffsetX / 2;

		ResetShoulderAndUpperArmPos();

		// Arm
		UpperArmL.Height = UpperArmL.SizeY;
		UpperArmR.Height = UpperArmR.SizeY;
		LowerArmL.Height = LowerArmL.SizeY;
		LowerArmR.Height = LowerArmR.SizeY;
		UpperArmL.LimbRotate(60);
		UpperArmR.LimbRotate(-5);
		LowerArmL.LimbRotate(120);
		LowerArmR.LimbRotate(0);
		HandL.LimbRotate(0);
		HandR.LimbRotate(1);

		// Leg
		UpperLegL.LimbRotate(10);
		LowerLegL.LimbRotate(0);
		FootL.LimbRotate(-FacingSign);

		// Grab Rot
		Rendering.HandGrabScaleL.Override(0);
		Rendering.HandGrabScaleR.Override(0);

		// Z
		UpperArmL.Z = 30;
		UpperArmR.Z = 30;
		LowerArmL.Z = 31;
		LowerArmR.Z = 31;
		HandL.Z = 32;
		HandR.Z = 32;

	}

}
