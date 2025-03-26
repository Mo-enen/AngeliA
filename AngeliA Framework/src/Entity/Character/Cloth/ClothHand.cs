using System;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Gloves for a pose-style character
/// </summary>
public abstract class HandCloth : Cloth {

	// AVR
	public sealed override ClothType ClothType => ClothType.Hand;
	public override bool SpriteLoaded => SpriteHandLeft.IsValid || SpriteHandRight.IsValid;
	private OrientedSprite SpriteHandLeft;
	private OrientedSprite SpriteHandRight;

	// MSG
	public override bool FillFromSheet (string name) {
		base.FillFromSheet(name);
		SpriteHandLeft = new OrientedSprite(name, "HandSuitLeft", "HandSuit");
		SpriteHandRight = new OrientedSprite(name, "HandSuitRight", "HandSuit");
		return SpriteLoaded;
	}

	/// <summary>
	/// Draw gloves for given character from system pool
	/// </summary>
	public static void DrawClothFromPool (PoseCharacterRenderer renderer) {
		if (renderer.SuitHand != 0 && renderer.TargetCharacter.CharacterState != CharacterState.Sleep && TryGetCloth(renderer.SuitHand, out var cloth)) {
			cloth.DrawCloth(renderer);
		}
	}

	public override void DrawCloth (PoseCharacterRenderer renderer) {
		if (!SpriteLoaded) return;
		using var _ = new SheetIndexScope(SheetIndex);
		DrawClothForHand(renderer, SpriteHandLeft, SpriteHandRight);
	}

	public override void DrawClothGizmos (IRect rect, Color32 tint, int z) {
		if (SpriteHandRight.TryGetSpriteForGizmos(out var handSP)) {
			Renderer.Draw(handSP, rect.Fit(handSP), tint, z);
		}
	}

	/// <summary>
	/// Draw artwork sprite as gloves for given character
	/// </summary>
	/// <param name="renderer">Target character</param>
	/// <param name="spriteLeft">Artwork for left glove</param>
	/// <param name="spriteRight">Artwork for right glove</param>
	/// <param name="localZ">Z value for sort rendering cells</param>
	public static void DrawClothForHand (PoseCharacterRenderer renderer, OrientedSprite spriteLeft, OrientedSprite spriteRight, int localZ = 1) {
		var body = renderer.Body;
		using var _ = new RotateCellScope(body.Rotation, body.GlobalX, body.GlobalY);
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
