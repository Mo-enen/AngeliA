namespace AngeliA;

public class PoseAnimation_Dash : PoseAnimation {
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);

		int aFrame = CurrentAnimationFrame.UMod(8) / 4;
		bool alt = aFrame == 1;

		HandL.Z = HandR.Z = POSE_Z_HAND;

		Rendering.PoseRootY = 0;
		Rendering.BodyTwist = -1000;

		Head.X += FacingSign * A2G;
		Body.X += FacingSign * 2 * A2G;
		Hip.X += FacingSign * 2 * A2G;
		UpperLegL.X += FacingSign * 2 * A2G + Body.Width / 4;
		UpperLegR.X += FacingSign * 2 * A2G + Body.Width / 4;
		ShoulderL.X += FacingSign * 2 * A2G;
		ShoulderR.X += FacingSign * 2 * A2G;
		UpperArmL.X += FacingSign * 2 * A2G;
		UpperArmR.X += FacingSign * 2 * A2G;

		Body.Rotation += -FacingSign * 40;
		Head.Rotation += FacingSign * 20;

		Hip.Y = UpperLegL.SizeX;
		Body.Y = Hip.Y + Hip.Height;
		Body.Height = Body.SizeY - A2G;
		Head.Y = Body.Y + Body.Height;
		Head.Height -= A2G;

		if (FacingRight) {
			LowerLegR.Z = 31;
			FootR.Z = 32;
		} else {
			LowerLegL.Z = 31;
			FootL.Z = 32;
		}

		// Arm
		ShoulderL.Y = Body.Y + Body.Height;
		ShoulderR.Y = Body.Y + Body.Height;

		UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.down;
		UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.down;

		if (FacingRight) {
			UpperArmR.Y += A2G;
		} else {
			UpperArmL.Y += A2G;
		}
		UpperArmL.LimbRotate((FacingRight ? 60 : 135) + (alt ? -2 : 2));
		UpperArmR.LimbRotate((FacingRight ? -135 : -60) + (alt ? 2 : -2));
		if (FacingRight) {
			UpperArmR.Height = UpperArmR.SizeY + A2G;
		} else {
			UpperArmL.Height = UpperArmL.SizeY + A2G;
		}

		LowerArmL.LimbRotate((FacingRight ? -60 : -45) + (alt ? -2 : 2));
		LowerArmR.LimbRotate((FacingRight ? 45 : 60) + (alt ? 2 : -2));
		if (FacingRight) {
			LowerArmL.Height = LowerArmL.SizeY + A2G;
		} else {
			LowerArmR.Height = LowerArmR.SizeY + A2G;
		}

		HandL.LimbRotate(0);
		HandR.LimbRotate(1);

		// Leg
		UpperLegL.X += FacingRight ? 0 : Body.Width.Abs() / 3;
		UpperLegR.X += FacingRight ? -Body.Width.Abs() / 3 : 0;
		UpperLegL.Y += FacingRight ? 0 : 2 * A2G;
		UpperLegR.Y += FacingRight ? 2 * A2G : 0;
		UpperLegL.LimbRotate((FacingRight ? -90 : 135));
		UpperLegR.LimbRotate((FacingRight ? -135 : 90));

		LowerLegL.LimbRotate((FacingRight ? 0 : -90) + (alt ? 2 : 0));
		LowerLegR.LimbRotate((FacingRight ? 90 : 0) + (alt ? -2 : 0));
		if (FacingRight) {
			LowerLegL.Height = LowerLegL.SizeY + A2G;
		} else {
			LowerLegR.Height = LowerLegR.SizeY + A2G;
		}

		FootL.LimbRotate(-FacingSign);
		FootR.LimbRotate(-FacingSign);

		// Final
		Rendering.HandGrabRotationL.Override(FacingRight ? 90 : 0);
		Rendering.HandGrabRotationR.Override(FacingRight ? 0 : -90);
	}
}
