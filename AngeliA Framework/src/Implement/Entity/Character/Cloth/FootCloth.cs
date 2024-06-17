using System;
using System.Collections.Generic;

namespace AngeliA;

public sealed class DefaultFootSuit : FootCloth {
	public static readonly int TYPE_ID = typeof(DefaultFootSuit).AngeHash();
	public DefaultFootSuit () => FillFromSheet(GetType().AngeName());
}

public class FootCloth : Cloth {

	protected sealed override ClothType ClothType => ClothType.Foot;
	private int SpriteID;

	protected override bool FillFromSheet (string name) {
		SpriteID = $"{name}.FootSuit".AngeHash();
		if (!Renderer.HasSprite(SpriteID) && !Renderer.HasSpriteGroup(SpriteID)) SpriteID = 0;
		return SpriteID != 0;
	}

	public static void DrawClothFromPool (PoseCharacter character) {
		if (character.SuitFoot != 0 && character.CharacterState != CharacterState.Sleep && Pool.TryGetValue(character.SuitFoot, out var cloth)) {
			cloth.Draw(character);
		}
	}

	public override void Draw (PoseCharacter character) => DrawClothForFoot(character, SpriteID);

	public static void DrawClothForFoot (PoseCharacter character, int spriteID, int localZ = 1) {
		if (spriteID == 0) return;
		if (Renderer.HasSpriteGroup(spriteID)) {
			if (Renderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
				DrawClothForFootLogic(character.FootL, spriteL.ID, localZ);
			}
			if (Renderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
				DrawClothForFootLogic(character.FootR, spriteR.ID, localZ);
			}
		} else {
			DrawClothForFootLogic(character.FootL, spriteID, localZ);
			DrawClothForFootLogic(character.FootR, spriteID, localZ);
		}
		// Func
		static void DrawClothForFootLogic (BodyPart foot, int spriteID, int localZ) {
			if (spriteID == 0 || foot.IsFullCovered) return;
			if (!Renderer.TryGetSprite(spriteID, out var sprite)) return;
			var location = foot.GlobalLerp(0f, 0f);
			int width = Util.Max(foot.Width, sprite.GlobalWidth);
			if (sprite.GlobalBorder.IsZero) {
				Renderer.Draw(
					sprite, location.x, location.y,
					0, 0, foot.Rotation,
					foot.Width.Sign() * width, sprite.GlobalHeight,
					foot.Z + localZ
				);
			} else {
				Renderer.DrawSlice(
					sprite, location.x, location.y,
					0, 0, foot.Rotation,
					foot.Width.Sign() * width, sprite.GlobalHeight,
					foot.Z + localZ
				);
			}

			foot.Covered = sprite.Tag != SpriteTag.SHOW_LIMB_TAG ?
				BodyPart.CoverMode.FullCovered : BodyPart.CoverMode.Covered;

		}
	}

}