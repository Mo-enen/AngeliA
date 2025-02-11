using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngeliA;




public abstract class HeadCloth : Cloth {

	// SUB
	public enum HatFrontMode { FrontOfHead, BackOfHead, AlwaysFrontOfHead, AlwaysBackOfHead, }

	// VAR
	public sealed override ClothType ClothType => ClothType.Head;
	public override bool SpriteLoaded => SpriteHead.IsValid;
	protected virtual HatFrontMode Front => HatFrontMode.FrontOfHead;
	private OrientedSprite SpriteHead;

	// MSG
	public override bool FillFromSheet (string name) {
		base.FillFromSheet(name);
		SpriteHead = new OrientedSprite(name, "HeadSuit");
		return SpriteLoaded;
	}

	public static void DrawClothFromPool (PoseCharacterRenderer renderer) {
		if (renderer.SuitHead != 0 && renderer.TargetCharacter.CharacterState != CharacterState.Sleep && Pool.TryGetValue(renderer.SuitHead, out var cloth)) {
			cloth.DrawCloth(renderer);
		}
	}

	public override void DrawCloth (PoseCharacterRenderer renderer) {
		if (!SpriteLoaded) return;
		using var _ = new SheetIndexScope(SheetIndex);
		DrawClothForHead(renderer, SpriteHead, Front);
	}

	public override void DrawClothGizmos (IRect rect, Color32 tint, int z) {
		if (SpriteHead.TryGetSpriteForGizmos(out var headSP)) {
			Renderer.Draw(headSP, rect.Fit(headSP), tint, z);
		}
	}

	public static void DrawClothForHead (PoseCharacterRenderer renderer, OrientedSprite clothSprite, HatFrontMode frontMode) {

		var head = renderer.Head;
		if (!clothSprite.IsValid || head.IsFullCovered) return;

		// Get Sprite
		bool front = frontMode != HatFrontMode.AlwaysBackOfHead && (
			frontMode == HatFrontMode.AlwaysFrontOfHead ||
			frontMode == HatFrontMode.FrontOfHead == head.FrontSide
		);
		if (!clothSprite.TryGetSprite(front, head.Width > 0, renderer.CurrentAnimationFrame, out var sprite)) return;

		// Width Amount
		int widthAmount = 1000;
		if (renderer.HeadTwist != 0) widthAmount -= renderer.HeadTwist.Abs() / 2;
		if ((head.Width < 0 != head.Height < 0) == head.FrontSide) widthAmount = -widthAmount;

		var body = renderer.Body;
		using var _ = new RotateCellScope(body.Rotation, body.GlobalX, body.GlobalY);

		// Draw
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