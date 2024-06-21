using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public sealed class ModularHorn : Horn, IModularBodyGadget { }


public abstract class Horn : BodyGadget {

	// VAR
	protected sealed override BodyGadgetType GadgetType => BodyGadgetType.Horn;
	public override bool SpriteLoaded => SpriteIdL != 0 || SpriteIdR != 0 || SpriteIdLBack != 0 || SpriteIdRBack != 0;
	private int SpriteIdL;
	private int SpriteIdR;
	private int SpriteIdLBack;
	private int SpriteIdRBack;
	protected virtual bool AnchorOnFace => false;
	protected virtual int FacingLeftOffsetX => 0;


	// MSG
	public override bool FillFromSheet (string name) {
		SpriteIdL = $"{name}.HornL".AngeHash();
		SpriteIdR = $"{name}.HornR".AngeHash();
		SpriteIdLBack = $"{name}.HornLB".AngeHash();
		SpriteIdRBack = $"{name}.HornRB".AngeHash();
		if (!Renderer.HasSprite(SpriteIdL)) SpriteIdL = 0;
		if (!Renderer.HasSprite(SpriteIdR)) SpriteIdR = 0;
		if (!Renderer.HasSprite(SpriteIdLBack)) SpriteIdLBack = SpriteIdL;
		if (!Renderer.HasSprite(SpriteIdRBack)) SpriteIdRBack = SpriteIdR;
		return SpriteLoaded;
	}


	// API
	public static void DrawGadgetFromPool (PoseCharacter character) {
		if (character.HornID != 0 && TryGetGadget(character.HornID, out var horn)) {
			horn.DrawGadget(character);
		}
	}


	public override void DrawGadget (PoseCharacter character) {

		if (!SpriteLoaded) return;

		int idL = character.FacingFront ? SpriteIdL : SpriteIdLBack;
		int idR = character.FacingFront ? SpriteIdR : SpriteIdRBack;
		DrawSpriteAsHorn(
			character, idL, idR,
			FrontOfHeadL(character), FrontOfHeadR(character),
			AnchorOnFace,
			character.FacingFront == character.FacingRight ? 0 : FacingLeftOffsetX
		);
	}


	protected virtual bool FrontOfHeadL (PoseCharacter character) => true;
	protected virtual bool FrontOfHeadR (PoseCharacter character) => true;


	public static void DrawSpriteAsHorn (
		PoseCharacter character, int spriteIdLeft, int spriteIdRight,
		bool frontOfHeadL = true, bool frontOfHeadR = true, bool onFace = false, int offsetX = 0
	) {

		if (spriteIdLeft == 0 && spriteIdRight == 0) return;
		var head = character.Head;
		if (head.Tint.a == 0) return;

		var headRect = head.GetGlobalRect();
		if (onFace) headRect = headRect.Shrink(head.Border);

		bool flipLR = !head.FrontSide && head.Height > 0;
		if (flipLR) {
			(spriteIdLeft, spriteIdRight) = (spriteIdRight, spriteIdLeft);
			offsetX = -offsetX;
		}

		// Twist
		int twist = character.HeadTwist;
		int twistWidth = 0;
		if (twist != 0) {
			twistWidth -= 16 * twist.Abs() / 500;
		}

		if (Renderer.TryGetSprite(spriteIdLeft, out var sprite)) {
			var cell = Renderer.Draw(
				sprite,
				headRect.xMin + offsetX,
				head.Height > 0 ? headRect.yMax : headRect.yMin,
				sprite.PivotX, sprite.PivotY, 0,
				sprite.GlobalWidth * (flipLR ? -1 : 1) + twistWidth,
				head.Height.Sign3() * sprite.GlobalHeight,
				head.Z + (head.FrontSide == frontOfHeadL ? 34 : -34)
			);
			if (character.Head.Rotation != 0) {
				cell.RotateAround(character.Head.Rotation, character.Body.GlobalX, character.Body.GlobalY + character.Body.Height);
				cell.Y -= character.Head.Height.Abs() * character.Head.Rotation.Abs() / 360;
			}
		}

		if (Renderer.TryGetSprite(spriteIdRight, out sprite)) {
			var cell = Renderer.Draw(
				sprite,
				headRect.xMax + offsetX,
				head.Height > 0 ? headRect.yMax : headRect.yMin,
				sprite.PivotX, sprite.PivotY, 0,
				sprite.GlobalWidth * (flipLR ? -1 : 1) + twistWidth,
				head.Height.Sign3() * sprite.GlobalHeight,
				head.Z + (head.FrontSide == frontOfHeadR ? 34 : -34)
			);
			if (character.Head.Rotation != 0) {
				cell.RotateAround(character.Head.Rotation, character.Body.GlobalX, character.Body.GlobalY + character.Body.Height);
				cell.Y -= character.Head.Height.Abs() * character.Head.Rotation.Abs() / 360;
			}
		}

	}


}