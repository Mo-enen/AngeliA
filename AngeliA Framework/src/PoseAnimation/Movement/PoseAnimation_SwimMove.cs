namespace AngeliA;

public class PoseAnimation_SwimMove : PoseAnimation {
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);

		int loop = Util.Max((1200 / Movement.SwimSpeed.FinalValue.GreaterOrEquel(1)).Clamp(1, 128) / 4 * 4, 4);
		int frame = CurrentAnimationFrame.UMod(loop) / (loop / 4);

		int frame0121 = frame == 3 ? 1 : frame;
		int easeArm = frame * A2G;

		Rendering.BodyTwist = FacingRight ? 0 : 1000;

		Head.X = FacingSign * A2G;

		int deltaX = Target.DeltaPositionX.Clamp(-24, 24) / 2;
		Head.Rotation = -deltaX;
		Body.Rotation = deltaX;

		// Arm
		if (FacingRight) {
			UpperArmL.LimbRotate(FacingSign * -90);
			UpperArmL.Height += 2 * A2G - easeArm;

			LowerArmL.LimbRotate(0);

			HandL.LimbRotate(-FacingSign);
			HandL.Height -= frame0121 * A2G / 3;

			UpperArmR.Imitate(UpperArmL);
			LowerArmR.Imitate(LowerArmL);
			HandR.Imitate(HandL);

		} else {
			UpperArmR.LimbRotate(FacingSign * -90);
			UpperArmR.Height += 2 * A2G - easeArm;

			LowerArmR.LimbRotate(0);

			HandR.LimbRotate(-FacingSign);
			HandR.Height -= frame0121 * A2G / 3;

			UpperArmL.Imitate(UpperArmR);
			LowerArmL.Imitate(LowerArmR);
			HandL.Imitate(HandR);
		}

		// Leg
		UpperLegL.Z += FacingSign * 2;
		UpperLegL.LimbRotate(FacingSign * (25 + FacingSign * frame0121 * 15));
		UpperLegL.Height = UpperLegL.SizeY + A2G - FacingSign * frame0121 * A2G / 2;

		UpperLegR.Z -= FacingSign * 2;
		UpperLegR.LimbRotate(FacingSign * (25 - FacingSign * frame0121 * 15));
		UpperLegR.Height = UpperLegR.SizeY + A2G + FacingSign * frame0121 * A2G / 2;

		LowerLegL.Z += FacingSign * 2;
		LowerLegL.LimbRotate(FacingSign * 85 - UpperLegL.Rotation);

		LowerLegR.Z -= FacingSign * 2;
		LowerLegR.LimbRotate(FacingSign * 85 - UpperLegR.Rotation);

		FootL.Z += FacingSign * 2;
		FootL.LimbRotate(-FacingSign);

		FootR.Z -= FacingSign * 2;
		FootR.LimbRotate(-FacingSign);

		// Final
		Rendering.HandGrabRotationL.Override(LowerArmL.Rotation + FacingSign * 90);
		Rendering.HandGrabRotationR.Override(LowerArmR.Rotation + FacingSign * 90);
	}
}
