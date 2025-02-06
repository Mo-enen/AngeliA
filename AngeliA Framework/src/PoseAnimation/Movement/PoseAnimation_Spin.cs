using System.Collections;
using System.Collections.Generic;


namespace AngeliA;

public class PoseAnimation_Spin : PoseAnimation {
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);

		int aFrame = CurrentAnimationFrame.UMod(8);
		int pingpong = aFrame < 4 ? aFrame : 8 - aFrame; // 01234321
		int pingpong2 = pingpong < 2 ? pingpong : 4 - pingpong; // 01210121
		bool facingFront = aFrame < 4;

		if (!facingFront) {
			FootL.Z -= 2;
			FootR.Z -= 2;
		}

		Head.Rotation = (pingpong - 2) * -10;
		Body.Rotation = (pingpong - 2) * 8;

		Rendering.BodyTwist = (pingpong - 2) * 1000;
		Rendering.HeadTwist = (pingpong - 2) * -300;

		Head.FrontSide = facingFront;
		Body.FrontSide = facingFront;
		Hip.FrontSide = facingFront;

		Body.Width = FacingSign * (facingFront ? 1 : -1) * (Body.SizeX - pingpong2 * A2G);
		Body.Height += A2G;

		Head.Width = FacingSign * (facingFront ? 1 : -1) * (Head.SizeX - pingpong2 * A2G);
		Head.Height += A2G;
		Head.X += (pingpong - 2) * A2G / 2;
		Head.Y = Body.Y + Body.Height;

		UpperArmL.Z = LowerArmL.Z;
		UpperArmR.Z = LowerArmR.Z;

		// Arm
		ShoulderL.X += pingpong * (Body.SizeX / 5);
		ShoulderR.X -= pingpong * (Body.SizeX / 5);
		UpperArmL.X += pingpong * (Body.SizeX / 5);
		UpperArmR.X -= pingpong * (Body.SizeX / 5);
		UpperArmL.LimbRotate(FacingSign * (-180 + aFrame / 2 * 12));
		UpperArmR.LimbRotate(FacingSign * (180 + aFrame / 2 * -12));

		LowerArmL.LimbRotate(0);
		LowerArmR.LimbRotate(0);

		HandL.LimbRotate(FacingSign);
		HandR.LimbRotate(FacingSign);
		HandL.Width += HandL.Width.Sign() * A2G;
		HandL.Height += HandL.Height.Sign() * A2G;
		HandR.Width += HandR.Width.Sign() * A2G;
		HandR.Height += HandR.Height.Sign() * A2G;

		// Leg
		UpperLegL.X += pingpong * UpperLegL.SizeX / 4;
		UpperLegR.X -= pingpong * UpperLegR.SizeX / 4;
		UpperLegL.LimbRotate(0, 0);
		UpperLegR.LimbRotate(0, 0);

		LowerLegL.LimbRotate((2 - pingpong) * -10, 0);
		LowerLegR.LimbRotate((2 - pingpong) * 10, 0);

		FootL.LimbRotate(facingFront ? -FacingSign : FacingSign);
		FootR.LimbRotate(facingFront ? -FacingSign : FacingSign);

		// Final
		Rendering.HandGrabRotationL = LowerArmL.Rotation - 90;
		Rendering.HandGrabRotationR = LowerArmR.Rotation + 90;
	}
}