namespace AngeliA;


public class PoseAnimation_Animation_TakingDamage : PoseAnimation {
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);

		bool alt = CurrentAnimationFrame.UMod(8) >= 4;
		int halfA2G = A2G / 2;

		if (alt) {
			Body.Height += A2G / 4;
			Head.Y += A2G / 4;
			Head.Width += FacingRight ? halfA2G : -halfA2G;
			Head.Height += halfA2G;
			UpperArmL.Y += A2G / 4;
			UpperArmR.Y += A2G / 4;
		}

		Head.Rotation = FacingSign * -6;
		Body.Rotation = FacingSign * 12;

		// Arm
		UpperArmL.LimbRotate(alt ? 85 : 65);
		UpperArmR.LimbRotate(alt ? -85 : -65);

		LowerArmL.Z = LowerArmL.Z.Abs();
		LowerArmL.LimbRotate(alt ? -75 : -55, 500);

		LowerArmR.Z = LowerArmR.Z.Abs();
		LowerArmR.LimbRotate(alt ? 75 : 55, 500);

		HandL.Z = HandL.Z.Abs();
		HandL.LimbRotate(0);

		HandR.Z = HandR.Z.Abs();
		HandR.LimbRotate(1);

		// Leg
		UpperLegL.LimbRotate((FacingRight ? -20 : 70) + (alt ? 5 : 0));
		UpperLegR.LimbRotate((FacingRight ? -70 : 20) + (alt ? -5 : 0));

		LowerLegL.LimbRotate((FacingRight ? 20 : -65) - (alt ? 5 : 0), 500);
		LowerLegL.Height = FacingRight ? LowerLegL.SizeY / 2 : LowerLegL.SizeY * 2 / 3;
		LowerLegL.Z += 2;

		LowerLegR.LimbRotate((FacingRight ? 65 : -20) - (alt ? -5 : 0), 500);
		LowerLegR.Height = !FacingRight ? LowerLegR.SizeY / 2 : LowerLegR.SizeY * 2 / 3;
		LowerLegR.Z += 2;

		FootL.LimbRotate(FacingRight ? 0 : 1);
		FootL.Z += 2;

		FootR.LimbRotate(FacingRight ? 0 : 1);
		FootR.Z += 2;
	}
}
