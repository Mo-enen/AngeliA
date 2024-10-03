using System;
using System.Collections.Generic;

namespace AngeliA;



public sealed class ModularHandSuit : HandCloth, IModularCloth { }


public abstract class HandCloth : Cloth {

	protected sealed override ClothType ClothType => ClothType.Hand;
	public override bool SpriteLoaded => SpriteID != 0;
	private int SpriteID;

	public override bool FillFromSheet (string name) {
		base.FillFromSheet(name);
		SpriteID = $"{name}.HandSuit".AngeHash();
		if (!Renderer.HasSprite(SpriteID) && !Renderer.HasSpriteGroup(SpriteID)) SpriteID = 0;
		return SpriteLoaded;
	}

	public static void DrawClothFromPool (PoseCharacterRenderer renderer) {
		if (renderer.SuitHand != 0 && renderer.TargetCharacter.CharacterState != CharacterState.Sleep && Pool.TryGetValue(renderer.SuitHand, out var cloth)) {
			cloth.DrawCloth(renderer);
		}
	}

	public override void DrawCloth (PoseCharacterRenderer renderer) {
		if (!SpriteLoaded) return;
		using var _ = new SheetIndexScope(SheetIndex);
		DrawClothForHand(renderer, SpriteID);
	}

	public static void DrawClothForHand (PoseCharacterRenderer renderer, int spriteID, int localZ = 1) {
		if (spriteID == 0) return;
		if (Renderer.HasSpriteGroup(spriteID)) {
			if (Renderer.TryGetSpriteFromGroup(spriteID, renderer.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
				CoverClothOn(renderer.HandL, spriteL.ID, localZ);
			}
			if (Renderer.TryGetSpriteFromGroup(spriteID, renderer.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
				CoverClothOn(renderer.HandR, spriteR.ID, localZ);
			}
		} else {
			CoverClothOn(renderer.HandL, spriteID, localZ);
			CoverClothOn(renderer.HandR, spriteID, localZ);
		}
	}

}
