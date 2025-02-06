namespace AngeliA;

public class PoseAnimation_PassOut : PoseAnimation {

	protected override bool ValidHeadPosition => false;

	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);

		Rendering.PoseRootY = 0;

		Body.Height = Body.SizeY / 4;

		Head.X = FacingRight ? A2G * 1 : A2G * -1;
		Head.Y = 0;

		// Arm
		ShoulderL.X = UpperArmL.X = FacingRight ? Body.X - Body.Width / 2 : Body.X + Body.Width / 2;
		ShoulderL.Y = ShoulderL.Height;
		UpperArmL.Y = 0;
		UpperArmL.LimbRotate(FacingRight ? -90 : 90, 0);
		
		ShoulderR.X = UpperArmR.X = FacingRight ? Body.X + Body.Width / 2 : Body.X - Body.Width / 2;
		ShoulderR.Y = ShoulderR.Height;
		UpperArmR.Y = 0;
		UpperArmR.LimbRotate(FacingRight ? -90 : 90, 0);
		
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

		// Z
		UpperArmL.Z = UpperArmL.Z.Abs();
		UpperArmR.Z = UpperArmR.Z.Abs();
		LowerArmL.Z = LowerArmL.Z.Abs();
		LowerArmR.Z = LowerArmR.Z.Abs();
		HandL.Z = HandL.Z.Abs();
		HandR.Z = HandR.Z.Abs();

	}

}
