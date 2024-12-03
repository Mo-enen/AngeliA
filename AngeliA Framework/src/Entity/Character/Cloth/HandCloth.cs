using System;
using System.Collections.Generic;

namespace AngeliA;

public sealed class ModularHandSuit : HandCloth, IModularCloth { }

public abstract class HandCloth : Cloth {

	protected sealed override ClothType ClothType => ClothType.Hand;
	public override bool SpriteLoaded => SpriteHand.IsValid;
	private OrientedSprite SpriteHand;

	public override bool FillFromSheet (string name) {
		base.FillFromSheet(name);
		SpriteHand = new OrientedSprite(name, "HandSuit");
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
		DrawClothForHand(renderer, SpriteHand);
	}

	public static void DrawClothForHand (PoseCharacterRenderer renderer, OrientedSprite sprite, int localZ = 1) {
		if (!sprite.IsValid) return;
		CoverClothOn(renderer.HandL, sprite.GetSpriteID(renderer.HandL.FrontSide, renderer.HandL.Width > 0), localZ);
		CoverClothOn(renderer.HandR, sprite.GetSpriteID(renderer.HandR.FrontSide, renderer.HandR.Width > 0), localZ);
	}

}
