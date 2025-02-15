using AngeliA;

namespace AngeliA.Platformer;

public class PoseHandheld_MagicPole : HandheldPoseAnimation {

	public static readonly int TYPE_ID = typeof(PoseHandheld_MagicPole).AngeHash();

	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		if (Attackness.IsChargingAttack) {
			// Charge
			PoseAttack_MagicPole.Wave();
			return;
		}

		// Normal
		ResetShoulderAndUpperArmPos();

		int twistShift = Rendering.BodyTwist / 50;
		UpperArmL.LimbRotate((FacingRight ? -42 : 29) - twistShift);
		UpperArmR.LimbRotate((FacingRight ? -29 : 42) - twistShift);
		UpperArmL.Height = UpperArmL.Height * (FacingRight ? 1306 : 862) / 1000;
		UpperArmR.Height = UpperArmR.Height * (FacingRight ? 862 : 1306) / 1000;

		LowerArmL.LimbRotate((FacingRight ? -28 : -48) + twistShift / 2);
		LowerArmR.LimbRotate((FacingRight ? 48 : 28) + twistShift / 2);
		LowerArmL.Height = LowerArmL.Height * (FacingRight ? 1592 : 724) / 1000;
		LowerArmR.Height = LowerArmR.Height * (FacingRight ? 724 : 1592) / 1000;

		HandL.LimbRotate(FacingSign);
		HandR.LimbRotate(FacingSign);

		// Z
		int signZ = Body.FrontSide ? 1 : -1;
		UpperArmL.Z = LowerArmL.Z = signZ * UpperArmL.Z.Abs();
		UpperArmR.Z = LowerArmR.Z = signZ * UpperArmR.Z.Abs();
		HandL.Z = HandR.Z = signZ * POSE_Z_HAND;

		// Grab Rotation
		int gRot = FacingSign * (
			30 - CurrentAnimationFrame.PingPong(120) / 30
			+ Target.DeltaPositionY.Clamp(-24, 24) / 5
		) - Target.DeltaPositionX.Clamp(-24, 24) / 4;
		Rendering.HandGrabScaleL.Override(FacingSign * 1000);
		Rendering.HandGrabScaleR.Override(FacingSign * 1000);
		Rendering.HandGrabRotationL.Override(gRot);
		Rendering.HandGrabRotationR.Override(gRot);

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
