using System;
using System.Collections.Generic;

namespace AngeliA;

public abstract class HandCloth : Cloth {

	public sealed override ClothType ClothType => ClothType.Hand;
	public override bool SpriteLoaded => SpriteHandLeft.IsValid || SpriteHandRight.IsValid;
	private OrientedSprite SpriteHandLeft;
	private OrientedSprite SpriteHandRight;

	public override bool FillFromSheet (string name) {
		base.FillFromSheet(name);
		SpriteHandLeft = new OrientedSprite(name, "HandSuitLeft", "HandSuit");
		SpriteHandRight = new OrientedSprite(name, "HandSuitRight", "HandSuit");
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
		DrawClothForHand(renderer, SpriteHandLeft, SpriteHandRight);
	}

	public static void DrawClothForHand (PoseCharacterRenderer renderer, OrientedSprite spriteLeft, OrientedSprite spriteRight, int localZ = 1) {
		if (spriteLeft.IsValid) {
			spriteLeft.TryGetSprite(renderer.HandL.FrontSide, renderer.HandL.Width > 0, renderer.CurrentAnimationFrame, out var sprite);
			CoverClothOn(renderer.HandL, sprite, localZ);
		}
		if (spriteRight.IsValid) {
			spriteRight.TryGetSprite(renderer.HandR.FrontSide, renderer.HandR.Width > 0, renderer.CurrentAnimationFrame, out var sprite);
			CoverClothOn(renderer.HandR, sprite, localZ);
		}
	}

}
