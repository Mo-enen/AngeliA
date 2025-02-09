using AngeliA;

namespace AngeliA.Platformer;

public class PoseHandheld_SingleHanded : HandheldPoseAnimation {

	public static readonly int TYPE_ID = typeof(PoseHandheld_SingleHanded).AngeHash();

	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		if (Attackness.IsChargingAttack) {
			PoseAttack_WaveSingleHanded.SmashDown();
			return;
		}
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
		var center = renderer.HandR.GlobalLerp(0.5f, 0.5f);
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
