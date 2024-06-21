using System;
using System.Collections.Generic;

namespace AngeliA;



public sealed class ModularHandSuit : HandCloth, IModularCloth {}


public abstract class HandCloth : Cloth {

	protected sealed override ClothType ClothType => ClothType.Hand;
	public override bool SpriteLoaded => SpriteID != 0;
	private int SpriteID;

	public override bool FillFromSheet (string name) {
		SpriteID = $"{name}.HandSuit".AngeHash();
		if (!Renderer.HasSprite(SpriteID) && !Renderer.HasSpriteGroup(SpriteID)) SpriteID = 0;
		return SpriteLoaded;
	}

	public static void DrawClothFromPool (PoseCharacter character) {
		if (character.SuitHand != 0 && character.CharacterState != CharacterState.Sleep && Pool.TryGetValue(character.SuitHand, out var cloth)) {
			cloth.DrawCloth(character);
		}
	}

	public override void DrawCloth (PoseCharacter character) {
		if (!SpriteLoaded) return;
		DrawClothForHand(character, SpriteID);
	}

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
