namespace AngeliA;

public class PoseAnimation_JumpUp : PoseAnimation {
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);

		bool alt = CurrentAnimationFrame.UMod(8) >= 4;

		Rendering.PoseRootY += A2G;
		Rendering.BodyTwist = FacingRight ? -400 : 400;

		if (alt) {
			Body.Height += A2G / 4;
			Head.Y += A2G / 4;
			UpperArmL.Y += A2G / 4;
			UpperArmR.Y += A2G / 4;
		}
		Head.Height += A2G;

		int deltaRot = Target.DeltaPositionX.Clamp(-42, 42) / 2;
		
		// Arm
		int motionDelta = -Target.DeltaPositionX.Clamp(-12, 12);
		UpperArmL.LimbRotate((alt ? 65 : 55) + motionDelta);
		UpperArmR.LimbRotate((alt ? -65 : -55) + motionDelta);

		LowerArmL.Z = LowerArmL.Z.Abs();
		LowerArmL.LimbRotate(alt ? -55 : -45, 500);

		LowerArmR.Z = LowerArmR.Z.Abs();
		LowerArmR.LimbRotate(alt ? 55 : 45, 500);

		HandL.Z = HandL.Z.Abs();
		HandL.LimbRotate(0);

		HandR.Z = HandR.Z.Abs();
		HandR.LimbRotate(1);

		// Leg
		UpperLegL.X += FacingRight ? -A2G / 2 : A2G / 2;
		UpperLegL.LimbRotate((FacingRight ? 0 : 20) + deltaRot);

		UpperLegR.X += FacingRight ? -A2G / 2 : A2G / 2;
		UpperLegR.LimbRotate((FacingRight ? -20 : 0) + deltaRot);

		LowerLegL.LimbRotate(FacingRight ? 0 : -45);
		LowerLegL.Height = FacingRight ? LowerLegL.SizeY * 3 / 4 : LowerLegL.SizeY;
		LowerLegL.Z += 2;

		LowerLegR.LimbRotate(FacingRight ? 45 : 0);
		LowerLegR.Height = FacingRight ? LowerLegR.SizeY : LowerLegR.SizeY * 3 / 4;
		LowerLegR.Z += 2;

		FootL.LimbRotate(FacingRight ? 0 : 1);
		FootL.Z += 2;

		FootR.LimbRotate(FacingRight ? 0 : 1);
		FootR.Z += 2;

		// Final
		Rendering.HandGrabRotationL.Override( LowerArmL.Rotation + FacingSign * 90);
		Rendering.HandGrabRotationR.Override( LowerArmR.Rotation + FacingSign * 90);
		Rendering.HandGrabScaleL.Override( FacingSign * 1000);
		Rendering.HandGrabScaleR.Override( FacingSign * 1000);
	}
}
