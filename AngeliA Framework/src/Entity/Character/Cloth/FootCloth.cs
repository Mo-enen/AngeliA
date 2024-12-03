using System;
using System.Collections.Generic;

namespace AngeliA;

public sealed class DefaultFootSuit : FootCloth {
	public static readonly int TYPE_ID = typeof(DefaultFootSuit).AngeHash();
	public DefaultFootSuit () => FillFromSheet(GetType().AngeName());
}


public sealed class ModularFootSuit : FootCloth, IModularCloth { }


public abstract class FootCloth : Cloth {

	protected sealed override ClothType ClothType => ClothType.Foot;
	public override bool SpriteLoaded => SpriteFoot.IsValid;
	private OrientedSprite SpriteFoot;

	public override bool FillFromSheet (string name) {
		base.FillFromSheet(name);
		SpriteFoot = new OrientedSprite(name, "FootSuit");
		return SpriteLoaded;
	}

	public static void DrawClothFromPool (PoseCharacterRenderer renderer) {
		if (renderer.SuitFoot != 0 && renderer.TargetCharacter.CharacterState != CharacterState.Sleep && Pool.TryGetValue(renderer.SuitFoot, out var cloth)) {
			cloth.DrawCloth(renderer);
		}
	}

	public override void DrawCloth (PoseCharacterRenderer renderer) {
		if (!SpriteLoaded) return;
		using var _ = new SheetIndexScope(SheetIndex);
		DrawClothForFoot(renderer, SpriteFoot);
	}

	public static void DrawClothForFoot (PoseCharacterRenderer renderer, OrientedSprite sprite, int localZ = 1) {
		if (!sprite.IsValid) return;
		DrawClothForFootLogic(renderer.FootL, sprite.GetSpriteID(renderer.FootL.FrontSide, renderer.FootL.Width > 0), localZ);
		DrawClothForFootLogic(renderer.FootR, sprite.GetSpriteID(renderer.FootR.FrontSide, renderer.FootR.Width > 0), localZ);
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

			foot.Covered = !sprite.Tag.HasAll(Tag.ShowLimb) ?
				BodyPart.CoverMode.FullCovered : BodyPart.CoverMode.Covered;

		}
	}

}