namespace AngeliA;

public class PoseHandheld_Pole : PoseAnimation {
	public override void Animate (PoseCharacter character) {
		base.Animate(character);

		// Charging
		if (Target.IsChargingAttack) {
			PoseAttack_Polearm.SmashDown();
			return;
		}

		// Holding
		bool dashing = AnimationType == CharacterAnimationType.Dash;

		ResetShoulderAndUpperArmPos();

		// Upper Arm
		int twistDelta = Target.BodyTwist / 26;
		UpperArmL.LimbRotate((FacingRight ? -2 : 14) - twistDelta);
		UpperArmR.LimbRotate((FacingRight ? -14 : 2) - twistDelta);
		if (dashing) {
			UpperArmL.Height /= 3;
			UpperArmR.Height /= 3;
		} else {
			int deltaY = (Target.DeltaPositionY / 5).Clamp(-20, 20);
			UpperArmL.Height += deltaY;
			UpperArmR.Height += deltaY;
		}

		// Lower Arm
		LowerArmL.LimbRotate((FacingRight ? -24 : 43) + twistDelta);
		LowerArmR.LimbRotate((FacingRight ? -43 : 24) + twistDelta);
		if (dashing) {
			LowerArmL.Height /= 3;
			LowerArmR.Height /= 3;
		} else {
			int deltaY = (Target.DeltaPositionY / 10).Clamp(-20, 20);
			LowerArmL.Height += deltaY;
			LowerArmR.Height += deltaY;
		}

		HandL.LimbRotate(FacingSign);
		HandR.LimbRotate(FacingSign);

		// Z
		HandL.Z = FrontSign * POSE_Z_HAND;
		HandR.Z = FrontSign * POSE_Z_HAND;

		// Grab
		int deltaRot = (Target.DeltaPositionY / 10).Clamp(-10, 10);
		Target.HandGrabRotationL = Target.HandGrabRotationR = FacingSign * (80 + deltaRot);
		Target.HandGrabScaleL = Target.HandGrabScaleR = FacingSign * 1000;

	}
}
