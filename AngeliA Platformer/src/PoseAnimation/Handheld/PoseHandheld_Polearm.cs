using AngeliA;

namespace AngeliA.Platformer;

public class PoseHandheld_Polearm : HandheldPoseAnimation {

	public static readonly int TYPE_ID = typeof(PoseHandheld_Polearm).AngeHash();

	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);

		// Charging
		if (Attackness.IsChargingAttack) {
			PoseAttack_Polearm.SmashDown();
			return;
		}

		// Holding
		bool dashing = AnimationType == CharacterAnimationType.Dash;

		ResetShoulderAndUpperArmPos();

		// Upper Arm
		int twistDelta = Rendering.BodyTwist / 26;
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
		Rendering.HandGrabRotationL.Override(FacingSign * (80 + deltaRot));
		Rendering.HandGrabRotationR.Override(FacingSign * (80 + deltaRot));
		Rendering.HandGrabScaleL.Override(FacingSign * 1000);
		Rendering.HandGrabScaleR.Override(FacingSign * 1000);

	}

	public override void DrawTool (HandTool tool, PoseCharacterRenderer renderer) {

		if (
			!Renderer.TryGetSprite(tool.SpriteID, out var sprite, true) &&
			!Renderer.TryGetSpriteFromGroup(tool.SpriteID, 0, out sprite)
		) return;

		var centerL = renderer.HandL.GlobalLerp(0.5f, 0.5f);
		var centerR = renderer.HandR.GlobalLerp(0.5f, 0.5f);
		int twistR = renderer.TargetCharacter.Attackness.IsAttacking && !tool.IgnoreGrabTwist ? renderer.HandGrabAttackTwistR : 1000;
		tool.OnToolSpriteRendered(
			renderer,
			(centerL.x + centerR.x) / 2,
			(centerL.y + centerR.y) / 2,
			sprite.GlobalWidth * twistR / 1000,
			sprite.GlobalHeight,
			renderer.HandGrabRotationR,
			renderer.HandGrabScaleR,
			sprite,
			renderer.HandR.Z - 1
		);
	}

}
