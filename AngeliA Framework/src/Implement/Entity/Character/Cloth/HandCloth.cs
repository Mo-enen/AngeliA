using System;
using System.Collections.Generic;

namespace AngeliA;


public class HandCloth : Cloth {

	protected sealed override ClothType ClothType => ClothType.Hand;
	private int SpriteID;

	protected override bool FillFromSheet (string name) {
		SpriteID = $"{name}.HandSuit".AngeHash();
		if (!Renderer.HasSprite(SpriteID) && !Renderer.HasSpriteGroup(SpriteID)) SpriteID = 0;
		return SpriteID != 0;
	}

	public static void DrawClothFromPool (PoseCharacter character) {
		if (character.Suit_Hand != 0 && character.CharacterState != CharacterState.Sleep && Pool.TryGetValue(character.Suit_Hand, out var cloth)) {
			cloth.Draw(character);
		}
	}

	public override void Draw (PoseCharacter character) => DrawClothForHand(character, SpriteID);

	public static void DrawClothForHand (PoseCharacter character, int spriteID, int localZ = 1) {
		if (spriteID == 0) return;
		if (Renderer.HasSpriteGroup(spriteID)) {
			if (Renderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
				CoverClothOn(character.HandL, spriteL.ID, localZ);
			}
			if (Renderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
				CoverClothOn(character.HandR, spriteR.ID, localZ);
			}
		} else {
			CoverClothOn(character.HandL, spriteID, localZ);
			CoverClothOn(character.HandR, spriteID, localZ);
		}
	}

}
