namespace AngeliA;

public class PoseAnimation_Sleep : PoseAnimation {
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		bool alt = CurrentAnimationFrame.UMod(120) >= 60;

		Rendering.PoseRootY = 0;

		Body.Height = Body.SizeY / 4;

		Head.X = FacingRight ? A2G * 4 : A2G * -4;
		Head.Y = 0;
		Head.Height -= alt ? A2G : 0;

		// Arm
		ShoulderL.X = UpperArmL.X = Head.X - Head.SizeX / 2 + UpperArmL.SizeX;
		ShoulderL.Y = ShoulderL.Height;
		UpperArmL.Y = 0;

		ShoulderR.X = UpperArmR.X = Head.X + Head.SizeX / 2 - UpperArmR.SizeX;
		ShoulderR.Y = ShoulderR.Height;
		UpperArmR.Y = 0;

		if (FacingRight) {
			UpperArmR.X -= A2G;
		} else {
			UpperArmL.X += A2G;
		}

		UpperArmL.LimbRotate(FacingRight ? 0 : -90, 0);
		UpperArmR.LimbRotate(FacingRight ? 90 : 0, 0);

		LowerArmL.LimbRotate(0);
		LowerArmL.Height = alt ? LowerArmL.SizeY + A2G : LowerArmL.SizeY;
		LowerArmR.LimbRotate(0);
		LowerArmR.Height = alt ? LowerArmR.SizeY + A2G : LowerArmR.SizeY;

		HandL.LimbRotate(0);
		HandR.LimbRotate(1);

		// Leg
		int legX = FacingRight ?
			Body.X - Body.SizeX / 2 + Body.Border.left - A2G * 2 :
			Body.X + Body.SizeX / 2 - Body.Border.right + A2G * 2;
		UpperLegL.X = legX - A2G;
		UpperLegL.Y = 0;
		UpperLegL.LimbRotate(FacingRight ? 90 : -90);
		UpperLegL.Height = UpperLegL.SizeY / 2;

		UpperLegR.X = legX + A2G;
		UpperLegR.Y = 0;
		UpperLegR.LimbRotate(FacingRight ? 90 : -90);
		UpperLegR.Height = UpperLegR.SizeY / 2;

		LowerLegL.LimbRotate(0);
		LowerLegR.LimbRotate(0);

		FootL.LimbRotate(FacingRight ? 0 : 1);
		FootR.LimbRotate(FacingRight ? 0 : 1);
	}
}
