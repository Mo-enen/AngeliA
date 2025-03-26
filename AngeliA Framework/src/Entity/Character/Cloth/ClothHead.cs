using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Hat for a pose style character
/// </summary>
public abstract class HeadCloth : Cloth {

	// VAR
	public sealed override ClothType ClothType => ClothType.Head;
	public override bool SpriteLoaded => SpriteHead.IsValid;
	private OrientedSprite SpriteHead;

	// MSG
	public override bool FillFromSheet (string name) {
		base.FillFromSheet(name);
		SpriteHead = new OrientedSprite(name, "HeadSuit");
		return SpriteLoaded;
	}

	/// <summary>
	/// Draw hat for given character from system pool
	/// </summary>
	public static void DrawClothFromPool (PoseCharacterRenderer renderer) {
		if (
			renderer.SuitHead != 0 &&
			TryGetCloth(renderer.SuitHead, out var cloth)
		) {
			cloth.DrawCloth(renderer);
		}
	}

	public override void DrawCloth (PoseCharacterRenderer renderer) {
		if (!SpriteLoaded) return;
		using var _ = new SheetIndexScope(SheetIndex);
		DrawClothForHead(renderer, SpriteHead);
	}

	public override void DrawClothGizmos (IRect rect, Color32 tint, int z) {
		if (SpriteHead.TryGetSpriteForGizmos(out var headSP)) {
			Renderer.Draw(headSP, rect.Fit(headSP), tint, z);
		}
	}

	/// <summary>
	/// Draw artwork sprite as hat for given character
	/// </summary>
	/// <param name="renderer">Target character</param>
	/// <param name="clothSprite">Artwork sprite</param>
	public static void DrawClothForHead (PoseCharacterRenderer renderer, OrientedSprite clothSprite) {

		var head = renderer.Head;
		if (!clothSprite.IsValid || head.IsFullCovered) return;

		// Get Sprite
		if (!clothSprite.TryGetSprite(head.FrontSide, head.Width > 0, renderer.CurrentAnimationFrame, out var sprite)) return;

		// Hide when Sleeping
		if (renderer.TargetCharacter.CharacterState == CharacterState.Sleep && !sprite.Tag.HasAll(Tag.Mark)) return;

		// Width Amount
		int widthAmount = 1000;
		if (renderer.HeadTwist != 0) widthAmount -= renderer.HeadTwist.Abs() / 2;
		if ((head.Width < 0 != head.Height < 0) == head.FrontSide) widthAmount = -widthAmount;

		var body = renderer.Body;
		using var _ = new RotateCellScope(body.Rotation, body.GlobalX, body.GlobalY);

		// Draw
		bool front = sprite.IsTrigger ? sprite.LocalZ >= 0 : sprite.LocalZ >= 0 == head.FrontSide;
		AttachClothOn(
			head, sprite, 500, 1000,
			(front ? 34 : -34) - head.Z, widthAmount, 1000, 0,
			0, 0
		);
		bool hideHead = sprite.Tag.HasAll(Tag.HideLimb);
		bool showEar = sprite.Tag.HasAll(Tag.ShowLimb);

		// Show/Hide Limb
		if (!showEar) {
			renderer.EarID.Override(0, 1, 4096);
		}
		if (hideHead) {
			renderer.HairID.Override(0, 1, 4096);
			renderer.Head.Covered = BodyPart.CoverMode.FullCovered;
		}

	}

}
