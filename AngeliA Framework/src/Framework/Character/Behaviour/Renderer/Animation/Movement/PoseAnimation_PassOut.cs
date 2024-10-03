namespace AngeliA;

public class PoseAnimation_PassOut : PoseAnimation {

	protected override bool ValidHeadPosition => false;

	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);

		Rendering.PoseRootY = 0;

		Body.Height = Body.SizeY / 4;

		Head.X = FacingRight ? A2G * 4 : A2G * -4;
		Head.Y = 0;

		// Arm
		ShoulderL.X = UpperArmL.X = FacingRight ? Head.X - Head.Width / 2 : Head.X + Head.Width / 2;
		ShoulderL.Y = ShoulderL.Height;
		UpperArmL.Y = 0;
		UpperArmL.LimbRotate(FacingRight ? -90 : 90, 0);
		UpperArmL.Height /= 2;

		ShoulderR.X = UpperArmR.X = FacingRight ? Head.X + Head.Width / 2 : Head.X - Head.Width / 2;
		ShoulderR.Y = ShoulderR.Height;
		UpperArmR.Y = 0;
		UpperArmR.LimbRotate(FacingRight ? -90 : 90, 0);
		UpperArmR.Height /= 2;

		LowerArmL.LimbRotate(0, 0);
		LowerArmR.LimbRotate(0, 0);
		LowerArmL.Height /= 2;
		LowerArmR.Height /= 2;

		HandL.LimbRotate(FacingRight ? 0 : 1);
		HandR.LimbRotate(FacingRight ? 0 : 1);

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
