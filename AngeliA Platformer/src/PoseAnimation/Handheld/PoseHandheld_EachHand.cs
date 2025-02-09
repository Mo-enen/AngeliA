using AngeliA;

namespace AngeliA.Platformer;

public class PoseHandheld_EachHand : HandheldPoseAnimation {

	public static readonly int TYPE_ID = typeof(PoseHandheld_EachHand).AngeHash();

	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		if (Attackness.IsChargingAttack) {
			PoseAttack_WaveEachHand.SmashDown();
		}
	}

	public override void DrawTool (HandTool tool, PoseCharacterRenderer renderer) {

		if (
			!Renderer.TryGetSprite(tool.SpriteID, out var sprite, true) &&
			!Renderer.TryGetSpriteFromGroup(tool.SpriteID, 0, out sprite)
		) return;

		bool attacking = renderer.TargetCharacter.Attackness.IsAttacking;
		bool squatting = renderer.TargetCharacter.Movement.IsSquatting;
		int grabScaleL = renderer.HandGrabScaleL;
		int grabScaleR = renderer.HandGrabScaleR;
		bool facingR = renderer.TargetCharacter.FacingRight;
		int grabRotL = squatting ? (facingR ? 80 : -100) : renderer.HandGrabRotationL;
		int grabRotR = squatting ? (facingR ? 100 : -80) : renderer.HandGrabRotationR;
		int twistL = attacking && !tool.IgnoreGrabTwist ? renderer.HandGrabAttackTwistL : 1000;
		int twistR = attacking && !tool.IgnoreGrabTwist ? renderer.HandGrabAttackTwistR : 1000;
		int zLeft = renderer.HandL.Z - 1;
		int zRight = renderer.HandR.Z - 1;
		var centerL = renderer.HandL.GlobalLerp(0.5f, 0.5f);
		var centerR = renderer.HandR.GlobalLerp(0.5f, 0.5f);
		tool.OnToolSpriteRendered(
			renderer,
			centerL.x, centerL.y,
			sprite.GlobalWidth * twistL / 1000,
			sprite.GlobalHeight,
			grabRotL,
			grabScaleL, sprite,
			zLeft
		);
		tool.OnToolSpriteRendered(
			renderer,
			centerR.x, centerR.y,
			sprite.GlobalWidth * twistR / 1000,
			sprite.GlobalHeight,
			grabRotR,
			grabScaleR, sprite,
			zRight
		);

	}

}
