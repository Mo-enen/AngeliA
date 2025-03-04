using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class PoseHandheld_Block : HandheldPoseAnimation {

	public static readonly int TYPE_ID = typeof(PoseHandheld_Block).AngeHash();

	public override void DrawTool (HandTool tool, PoseCharacterRenderer renderer) {

		if (
			!Renderer.TryGetSprite(tool.SpriteID, out var sprite, true) &&
			!Renderer.TryGetSpriteFromGroup(tool.SpriteID, 0, out sprite)
		) return;

		bool attacking = renderer.TargetCharacter.Attackness.IsAttacking;
		int facingSign = renderer.TargetCharacter.Movement.FacingRight ? 1 : -1;
		int grabScale = renderer.HandGrabScaleR;
		int grabRotation = renderer.HandGrabRotationR;

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
		Renderer.Draw(
			sprite, center.x, center.y, 500, 500, grabRotation,
			sprite.GlobalWidth * grabScale / 1500,
			sprite.GlobalHeight * grabScale / 1500,
			renderer.HandR.Z + 3
		);

	}

}
