using AngeliA;

namespace AngeliA.Platformer;


public class PoseAnimation_SlidingSitHigh : PoseAnimation_SlidingSit {
	public new static readonly int TYPE_ID = typeof(PoseAnimation_SlidingSitHigh).AngeHash();
	public override void Animate (PoseCharacterRenderer renderer) {
		BaseAnimate(renderer);
		SlidingSit(true);
	}
}


public class PoseAnimation_SlidingSit : PoseAnimation {

	public static readonly int TYPE_ID = typeof(PoseAnimation_SlidingSit).AngeHash();

	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		SlidingSit(false);
	}

	public void BaseAnimate (PoseCharacterRenderer renderer) => base.Animate(renderer);

	public static void SlidingSit (bool highPos) {

		float deltaX = Target.DeltaPositionX.Clamp(-30, 30) / 30f;
		float deltaY = Target.DeltaPositionY.Clamp(-20, 20) / 20f;

		Rendering.PoseRootX = (int)(-FacingSign * A2G + deltaX * 30);
		Rendering.PoseRootY = highPos ? Rendering.PoseRootY : 0;
		Rendering.PoseRootY += (CurrentAnimationFrame.PingPong(10) - 5) * 3;
		Body.X += FacingSign * A2G;
		Head.X += FacingSign * A2G * 2;
		Hip.X = Body.X;
		UpperLegL.X += FacingSign * A2G;
		UpperLegR.X += FacingSign * A2G;
		Head.Rotation = FacingSign * (int)(10 - deltaX.Abs() * 10);

		Body.Height = Body.SizeY - (int)(deltaX * 16).Abs();
		Head.Y = Body.Y + Body.Height - (int)(deltaX * 24).Abs();
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
		UpperLegL.LimbRotate((int)(FacingSign * (-80 + deltaY * 15)));
		UpperLegR.LimbRotate((int)(FacingSign * (-80 + deltaY * 15)));
		LowerLegL.LimbRotate((int)(FacingSign * (120 + deltaY * 8)));
		LowerLegR.LimbRotate((int)(FacingSign * (120 + deltaY * 8)));
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
