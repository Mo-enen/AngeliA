namespace AngeliA;

public class PoseAnimation_Brake : PoseAnimation {
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);

		const float DURATION = 8f;
		float lerp01 = (CurrentAnimationFrame.Abs() / DURATION).Clamp01();

		Body.Rotation = FacingSign * 10;

		// Arm
		UpperArmL.LimbRotate(UpperArmL.Rotation.LerpTo(FacingRight ? 130 : 20, lerp01));
		UpperArmR.LimbRotate(UpperArmR.Rotation.LerpTo(FacingRight ? -20 : -130, lerp01));
		LowerArmL.LimbRotate(LowerArmL.Rotation.LerpTo(FacingRight ? -120 : -135, lerp01));
		LowerArmR.LimbRotate(LowerArmR.Rotation.LerpTo(FacingRight ? 135 : 120, lerp01));
		HandL.LimbRotate(0);
		HandR.LimbRotate(0);

		// Leg
		UpperLegL.LimbRotate(UpperLegL.Rotation.LerpTo(FacingRight ? 20 : -100, lerp01));
		UpperLegR.LimbRotate(UpperLegR.Rotation.LerpTo(FacingRight ? 100 : -20, lerp01));
		LowerLegL.LimbRotate(LowerLegL.Rotation.LerpTo(FacingRight ? 0 : 150, lerp01));
		LowerLegR.LimbRotate(LowerLegR.Rotation.LerpTo(FacingRight ? -150 : 0, lerp01));
		FootL.LimbRotate(FacingSign);
		FootR.LimbRotate(FacingSign);

		// Final
		HandL.Z = 33;
		HandR.Z = 33;
		Rendering.BodyTwist = FacingRight ? -1000 : 1000;
		Rendering.HandGrabRotationL.Override(LowerArmL.Rotation + FacingSign * 90);
		Rendering.HandGrabRotationR.Override(LowerArmR.Rotation + FacingSign * 90);
	}
}
