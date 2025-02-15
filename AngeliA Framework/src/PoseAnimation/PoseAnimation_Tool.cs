using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class PosePerform_Tool : PoseAnimation {
	public static readonly int TYPE_ID = typeof(PosePerform_Tool).AngeHash();
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		Perform();
	}
	public static void Perform () {

		int aFrame = (Game.GlobalFrame - Attackness.LastAttackFrame).UDivide(5);
		if (aFrame >= 4) return;

		UpperArmL.Z = FrontSign * UpperArmL.Z.Abs();
		UpperArmR.Z = FrontSign * UpperArmR.Z.Abs();
		LowerArmL.Z = FrontSign * LowerArmL.Z.Abs();
		LowerArmR.Z = FrontSign * LowerArmR.Z.Abs();
		HandL.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);
		HandR.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);

		Head.X += FacingSign * (aFrame == 0 ? -2 : (3 - aFrame) * 2) * A2G;
		Head.Y += A2G * 2 * (aFrame == 0 ? 0 : (3 - aFrame) * -2);

		Body.Y -= aFrame * A2G / 4;
		Hip.Y -= aFrame * A2G / 4;
		Body.Height = Head.Y - Body.Y;

		// Arm
		ShoulderL.X = Body.X - Body.SizeX / 2 + Body.Border.left;
		ShoulderL.Y = Body.Y + Body.Height - Body.Border.up;
		ShoulderR.X = Body.X + Body.SizeX / 2 - Body.Border.right;
		ShoulderR.Y = Body.Y + Body.Height - Body.Border.up;
		ShoulderL.Height = Util.Min(ShoulderL.Height, Body.Height);
		ShoulderR.Height = Util.Min(ShoulderR.Height, Body.Height);
		ShoulderL.PivotX = 1000;
		ShoulderR.PivotX = 1000;

		UpperArmL.PivotX = 0;
		UpperArmR.PivotX = 1000;

		UpperArmL.X = ShoulderL.X;
		UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.down;
		UpperArmR.X = ShoulderR.X;
		UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.down;
		UpperArmL.PivotX = 1000;
		UpperArmR.PivotX = 0;

		if (aFrame == 0) {
			UpperArmL.LimbRotate(FacingRight ? 135 : -175);
			UpperArmR.LimbRotate(FacingRight ? 175 : -135);
		} else if (aFrame == 1) {
			UpperArmL.LimbRotate(FacingRight ? -30 : 40);
			UpperArmR.LimbRotate(FacingRight ? -40 : 30);
		} else if (aFrame == 2) {
			UpperArmL.LimbRotate(FacingRight ? -15 : 20);
			UpperArmR.LimbRotate(FacingRight ? -20 : 15);
		} else if (aFrame == 3) {
			UpperArmL.LimbRotate(FacingRight ? -5 : 9);
			UpperArmR.LimbRotate(FacingRight ? -9 : 5);
		}
		UpperArmL.Height += A2G;
		UpperArmR.Height += A2G;

		LowerArmL.LimbRotate(0);
		LowerArmR.LimbRotate(0);

		HandL.LimbRotate(FacingSign);
		HandR.LimbRotate(FacingSign);
		HandL.Width += HandL.Width.Sign() * A2G;
		HandL.Height += HandL.Height.Sign() * A2G;
		HandR.Width += HandR.Width.Sign() * A2G;
		HandR.Height += HandR.Height.Sign() * A2G;

		// Leg
		if (AnimationType == CharacterAnimationType.Idle) {
			if (aFrame == 0) {
				UpperLegL.X -= A2G;
				UpperLegR.X += A2G;
				LowerLegL.X -= A2G;
				LowerLegR.X += A2G;
				FootL.X -= A2G;
				FootR.X += A2G;
			} else if (FacingRight) {
				LowerLegL.X -= A2G;
				FootL.X -= A2G;
			} else {
				LowerLegR.X += A2G;
				FootR.X += A2G;
			}
		}

		// Final
		Rendering.HandGrabRotationL.Override( LowerArmL.Rotation + FacingSign * 90);
		Rendering.HandGrabRotationR.Override( LowerArmR.Rotation + FacingSign * 90);
	}
}
