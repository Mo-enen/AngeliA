using AngeliA;

namespace AngeliA.Platformer;

public class PoseHandheld_DoubleHanded : HandheldPoseAnimation {

	public static readonly int TYPE_ID = typeof(PoseHandheld_DoubleHanded).AngeHash();

	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);

		if (Attackness.IsChargingAttack) {
			// Charging
			PoseAttack_WaveDoubleHanded.SmashDown();
			return;
		}

		// Holding
		ResetShoulderAndUpperArmPos();

		int twistShift = Rendering.BodyTwist / 50;
		UpperArmL.LimbRotate((FacingRight ? -42 : 29) - twistShift);
		UpperArmR.LimbRotate((FacingRight ? -29 : 42) - twistShift);
		UpperArmL.Height = UpperArmL.Height * (FacingRight ? 1306 : 862) / 1000;
		UpperArmR.Height = UpperArmR.Height * (FacingRight ? 862 : 1306) / 1000;

		LowerArmL.Height = LowerArmL.SizeY;
		LowerArmR.Height = LowerArmR.SizeY;
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
		Rendering.HandGrabScaleL = Rendering.HandGrabScaleR = FacingSign * 1000;
		Rendering.HandGrabRotationL = Rendering.HandGrabRotationR = FacingSign * (
			30 - CurrentAnimationFrame.PingPong(120) / 30
			+ Target.DeltaPositionY.Clamp(-24, 24) / 5
		) - Target.DeltaPositionX.Clamp(-24, 24) / 4;

	}

	public override void DrawTool (HandTool tool, PoseCharacterRenderer renderer) {

		if (
			!Renderer.TryGetSprite(tool.SpriteID, out var sprite, true) &&
			!Renderer.TryGetSpriteFromGroup(tool.SpriteID, 0, out sprite)
		) return;

		bool attacking = renderer.TargetCharacter.Attackness.IsAttacking;
		int twistR = attacking && !tool.IgnoreGrabTwist ? renderer.HandGrabAttackTwistR : 1000;
		int facingSign = renderer.TargetCharacter.Movement.FacingRight ? 1 : -1;
		int grabScale = renderer.HandGrabScaleR;
		int grabRotation = renderer.HandGrabRotationR;
		int z = renderer.HandR.Z - 1;

		// Fix Rotation
		if (sprite.IsTrigger) {
			if (!attacking) {
				grabRotation = 0;
			} else {
				grabRotation = Util.RemapUnclamped(
					0, tool.Duration,
					facingSign * 90, 0,
					Game.GlobalFrame - renderer.TargetCharacter.Attackness.LastAttackFrame
				);
			}
		}

		// Draw
		var center = (renderer.HandL.GlobalLerp(0.5f, 0.5f) + renderer.HandR.GlobalLerp(0.5f, 0.5f)) / 2;
		tool.OnToolSpriteRendered(
			renderer,
			center.x, center.y,
			sprite.GlobalWidth * twistR / 1000,
			sprite.GlobalHeight,
			grabRotation, grabScale,
			sprite, z
		);

	}

}
