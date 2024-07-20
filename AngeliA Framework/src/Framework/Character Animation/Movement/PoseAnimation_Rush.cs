namespace AngeliA;

public class PoseAnimation_Rush : PoseAnimation {
	public override void Animate (PoseCharacter character) {
		base.Animate(character);

		int aFrame = CurrentAnimationFrame.Abs() / 3;
		bool alt = aFrame % 2 == 0;

		Target.PoseRootY -= aFrame.Clamp(0, 2) * A2G - A2G * 2;
		Target.BodyTwist = -FacingSign * 1000;
		Head.Height -= A2G;
		Body.X -= FacingSign * A2G / 2;

		UpperArmL.Z = UpperArmL.Z.Abs();
		UpperArmR.Z = UpperArmR.Z.Abs();
		LowerArmL.Z = LowerArmL.Z.Abs();
		LowerArmR.Z = LowerArmR.Z.Abs();
		HandL.Z = (FacingFront ? 1 : -1) * HandL.Z.Abs();
		HandR.Z = (FacingFront ? 1 : -1) * HandR.Z.Abs();
		UpperLegL.Z += FacingSign;
		UpperLegR.Z -= FacingSign;
		LowerLegL.Z += FacingSign;
		LowerLegR.Z -= FacingSign;
		FootL.Z += FacingSign;
		FootR.Z -= FacingSign;
		UpperLegL.X -= FacingSign * A2G + (alt ? 3 : 0);
		UpperLegR.X -= FacingSign * A2G + (alt ? 3 : 0);

		if (aFrame == 0) {
			// 0
			UpperArmL.LimbRotate((FacingRight ? 40 : 0));
			UpperArmR.LimbRotate((FacingRight ? 0 : -40));

			LowerArmL.LimbRotate(FacingRight ? 0 : -40);
			LowerArmR.LimbRotate(FacingRight ? 40 : 0);

			UpperLegL.LimbRotate(FacingRight ? 5 : 9);
			UpperLegR.LimbRotate(FacingRight ? -9 : -5);

			LowerLegL.LimbRotate(FacingSign * (FacingRight ? 25 : 35) - UpperLegL.Rotation);
			LowerLegR.LimbRotate(FacingSign * (FacingRight ? 35 : 25) - UpperLegR.Rotation);

		} else if (aFrame == 1) {
			// 1
			UpperArmL.LimbRotate((FacingRight ? 70 : 0));
			UpperArmR.LimbRotate((FacingRight ? 0 : -70));

			LowerArmL.LimbRotate(FacingRight ? 0 : -70);
			LowerArmR.LimbRotate(FacingRight ? 70 : 0);

			UpperLegL.LimbRotate(FacingRight ? 15 : 25);
			UpperLegR.LimbRotate(FacingRight ? -25 : -15);

			LowerLegL.LimbRotate(FacingSign * (FacingRight ? 45 : 65) - UpperLegL.Rotation);
			LowerLegR.LimbRotate(FacingSign * (FacingRight ? 65 : 45) - UpperLegR.Rotation);

		} else {
			// 2..
			UpperArmL.LimbRotate((FacingRight ? 90 : 0) + (alt ? 3 : 0));
			UpperArmR.LimbRotate((FacingRight ? 0 : -90) + (alt ? -3 : 0));

			LowerArmL.LimbRotate((FacingRight ? 0 : -90) + (alt ? 2 : 0));
			LowerArmR.LimbRotate((FacingRight ? 90 : 0) + (alt ? -2 : 0));

			UpperLegL.LimbRotate((FacingRight ? 20 : 30) + (alt ? 3 : 0));
			UpperLegR.LimbRotate((FacingRight ? -30 : -20) + (alt ? -2 : 0));

			LowerLegL.LimbRotate((FacingSign * (FacingRight ? 65 : 85) - UpperLegL.Rotation) + (alt ? 2 : 0));
			LowerLegR.LimbRotate((FacingSign * (FacingRight ? 85 : 65) - UpperLegR.Rotation) + (alt ? -3 : 0));
		}

		HandL.LimbRotate(FacingSign);
		HandR.LimbRotate(FacingSign);
		FootL.LimbRotate(-FacingSign);
		FootR.LimbRotate(-FacingSign);

		// Final
		Target.HandGrabRotationL = LowerArmL.Rotation + FacingSign * 90;
		Target.HandGrabRotationR = LowerArmR.Rotation + FacingSign * 90;
	}
}
