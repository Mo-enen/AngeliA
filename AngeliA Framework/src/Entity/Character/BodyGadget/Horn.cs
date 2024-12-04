using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public sealed class ModularHorn : Horn, IModularBodyGadget { }


public abstract class Horn : BodyGadget {

	// VAR
	protected sealed override BodyGadgetType GadgetType => BodyGadgetType.Horn;
	public override bool SpriteLoaded => SpriteHornLeft.IsValid || SpriteHornRight.IsValid;
	protected virtual bool AnchorOnFace => false;
	public OrientedSprite SpriteHornLeft { get; private set; }
	public OrientedSprite SpriteHornRight { get; private set; }


	// MSG
	public override bool FillFromSheet (string name) {
		base.FillFromSheet(name);
		SpriteHornLeft = new OrientedSprite(name, "HornLeft", "Horn");
		SpriteHornRight = new OrientedSprite(name, "HornRight", "Horn");
		return SpriteLoaded;
	}


	// API
	public static void DrawGadgetFromPool (PoseCharacterRenderer renderer) {
		if (renderer.HornID != 0 && TryGetGadget(renderer.HornID, out var horn)) {
			horn.DrawGadget(renderer);
		}
	}


	public override void DrawGadget (PoseCharacterRenderer renderer) {
		if (!SpriteLoaded) return;
		using var _ = new SheetIndexScope(SheetIndex);
		DrawSpriteAsHorn(
			renderer,
			SpriteHornLeft, SpriteHornRight,
			FrontOfHeadL(renderer),
			FrontOfHeadR(renderer),
			AnchorOnFace
		);
	}


	protected virtual bool FrontOfHeadL (PoseCharacterRenderer renderer) => true;
	protected virtual bool FrontOfHeadR (PoseCharacterRenderer renderer) => true;


	public static void DrawSpriteAsHorn (
		PoseCharacterRenderer renderer, OrientedSprite spriteLeft, OrientedSprite spriteRight,
		bool frontOfHeadL = true, bool frontOfHeadR = true, bool onFace = false
	) {

		if (!spriteLeft.IsValid && !spriteRight.IsValid) return;
		var head = renderer.Head;
		if (head.Tint.a == 0) return;

		var headRect = head.GetGlobalRect();
		if (onFace) headRect = headRect.Shrink(head.Border);

		bool flipLR = !head.FrontSide && head.Height > 0;
		spriteLeft.TryGetSprite(head.FrontSide, flipLR, renderer.CurrentAnimationFrame, out var spriteL);
		spriteRight.TryGetSprite(head.FrontSide, !flipLR, renderer.CurrentAnimationFrame, out var spriteR);

		// Twist
		int twist = renderer.HeadTwist;
		int twistWidth = 0;
		if (twist != 0) {
			twistWidth -= 16 * twist.Abs() / 500;
		}

		if (spriteL != null) {
			var cell = Renderer.Draw(
				spriteL,
				headRect.xMin,
				head.Height > 0 ? headRect.yMax : headRect.yMin,
				spriteL.PivotX, spriteL.PivotY, 0,
				spriteL.GlobalWidth * (flipLR ? -1 : 1) + twistWidth,
				head.Height.Sign3() * spriteL.GlobalHeight,
				head.Z + (head.FrontSide == frontOfHeadL ? 34 : -34)
			);
			if (renderer.Head.Rotation != 0) {
				cell.RotateAround(renderer.Head.Rotation, renderer.Body.GlobalX, renderer.Body.GlobalY + renderer.Body.Height);
				cell.Y -= renderer.Head.Height.Abs() * renderer.Head.Rotation.Abs() / 360;
			}
		}

		if (spriteR != null) {
			var cell = Renderer.Draw(
				spriteR,
				headRect.xMax,
				head.Height > 0 ? headRect.yMax : headRect.yMin,
				spriteR.PivotX, spriteR.PivotY, 0,
				spriteR.GlobalWidth * (flipLR ? -1 : 1) + twistWidth,
				head.Height.Sign3() * spriteR.GlobalHeight,
				head.Z + (head.FrontSide == frontOfHeadR ? 34 : -34)
			);
			if (renderer.Head.Rotation != 0) {
				cell.RotateAround(renderer.Head.Rotation, renderer.Body.GlobalX, renderer.Body.GlobalY + renderer.Body.Height);
				cell.Y -= renderer.Head.Height.Abs() * renderer.Head.Rotation.Abs() / 360;
			}
		}

	}


}