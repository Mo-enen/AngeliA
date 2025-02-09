using AngeliA;

namespace AngeliA.Platformer;

public class PoseHandheld_Float : HandheldPoseAnimation {

	public static readonly int TYPE_ID = typeof(PoseHandheld_Float).AngeHash();

	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		// Charging
		if (Attackness.IsChargingAttack) {
			PoseAttack_Float.Wave();
		}
	}

	public override void DrawTool (HandTool tool, PoseCharacterRenderer renderer) {

		if (
			!Renderer.TryGetSprite(tool.SpriteID, out var sprite, true) &&
			!Renderer.TryGetSpriteFromGroup(tool.SpriteID, 0, out sprite)
		) return;

		const int SHIFT_X = 148;
		var character = renderer.TargetCharacter;
		int grabScaleL = character.Attackness.IsAttacking ? renderer.HandGrabScaleL : 700;
		int facingSign = character.Movement.FacingRight ? 1 : -1;
		int moveDeltaX = -character.DeltaPositionX * 2;
		int moveDeltaY = -character.DeltaPositionY;
		int facingFrame = Game.GlobalFrame - character.Movement.LastFacingChangeFrame;
		if (facingFrame < 30) {
			moveDeltaX += (int)Util.LerpUnclamped(
				facingSign * SHIFT_X * 2, 0,
				Ease.OutBack(facingFrame / 30f)
			);
		}
		tool.OnToolSpriteRendered(
			renderer,
			character.X + (facingSign * -SHIFT_X) + moveDeltaX,
			character.Y + Const.CEL * renderer.CharacterHeight / 263 + Game.GlobalFrame.PingPong(240) / 4 + moveDeltaY,
			sprite.GlobalWidth,
			sprite.GlobalHeight,
			0,
			(sprite.IsTrigger ? facingSign : 1) * grabScaleL,
			sprite,
			36
		);

	}

}