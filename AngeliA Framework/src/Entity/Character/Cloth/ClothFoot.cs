using System;
using System.Collections.Generic;

namespace AngeliA;


/// <summary>
/// Shoes of a pose-style character
/// </summary>
public abstract class FootCloth : Cloth {

	// VAR
	public sealed override ClothType ClothType => ClothType.Foot;
	public override bool SpriteLoaded => SpriteFootLeft.IsValid || SpriteFootRight.IsValid;
	private OrientedSprite SpriteFootLeft;
	private OrientedSprite SpriteFootRight;

	// MSG
	public override bool FillFromSheet (string name) {
		base.FillFromSheet(name);
		SpriteFootLeft = new OrientedSprite(name, "FootSuitLeft", "FootSuit");
		SpriteFootRight = new OrientedSprite(name, "FootSuitRight", "FootSuit");
		return SpriteLoaded;
	}

	/// <summary>
	/// Draw shoes for given character from system pool
	/// </summary>
	public static void DrawClothFromPool (PoseCharacterRenderer renderer) {
		if (renderer.SuitFoot != 0 && renderer.TargetCharacter.CharacterState != CharacterState.Sleep && TryGetCloth(renderer.SuitFoot, out var cloth)) {
			cloth.DrawCloth(renderer);
		}
	}

	public override void DrawCloth (PoseCharacterRenderer renderer) {
		if (!SpriteLoaded) return;
		using var _ = new SheetIndexScope(SheetIndex);
		DrawClothForFoot(renderer, SpriteFootLeft, SpriteFootRight);
	}

	public override void DrawClothGizmos (IRect rect, Color32 tint, int z) {
		if (SpriteFootRight.TryGetSpriteForGizmos(out var footSP)) {
			Renderer.Draw(footSP, rect.Fit(footSP), tint, z);
		}
	}

	/// <summary>
	/// Draw given artwork sprite as shoes for given character
	/// </summary>
	/// <param name="renderer">Target character</param>
	/// <param name="spriteLeft">Artwork sprite for left shoe</param>
	/// <param name="spriteRight">Artwork sprite for right shoe</param>
	/// <param name="localZ">Z value for sorting rendering cells</param>
	public static void DrawClothForFoot (PoseCharacterRenderer renderer, OrientedSprite spriteLeft, OrientedSprite spriteRight, int localZ = 1) {
		if (spriteLeft.IsValid) {
			spriteLeft.TryGetSprite(renderer.FootL.FrontSide, renderer.FootL.Width > 0, renderer.CurrentAnimationFrame, out var sprite);
			DrawClothForFootLogic(renderer.FootL, sprite, localZ);
		}
		if (spriteRight.IsValid) {
			spriteRight.TryGetSprite(renderer.FootR.FrontSide, renderer.FootR.Width > 0, renderer.CurrentAnimationFrame, out var sprite);
			DrawClothForFootLogic(renderer.FootR, sprite, localZ);
		}
		// Func
		static void DrawClothForFootLogic (BodyPart foot, AngeSprite sprite, int localZ) {
			if (sprite == null || foot.IsFullCovered) return;
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

			foot.Covered = !sprite.Tag.HasAll(Tag.ShowLimb) ?
				CoverMode.FullCovered : CoverMode.Covered;

		}
	}

}