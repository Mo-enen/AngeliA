using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public sealed class DefaultWing : Wing {
	public static readonly int TYPE_ID = typeof(DefaultWing).AngeHash();
	protected override int Scale => 600;
}


public sealed class DefaultPropellerWing : Wing {
	public static readonly int TYPE_ID = typeof(DefaultPropellerWing).AngeHash();
}


public sealed class ModularWing : Wing, IModularBodyGadget { }


public abstract class Wing : BodyGadget {


	protected sealed override BodyGadgetType GadgetType => BodyGadgetType.Wing;
	public override bool SpriteLoaded => SpriteGroupID != 0;
	private int SpriteGroupID;
	public bool IsPropeller { get; private set; } = false;
	protected virtual int Scale => 1000;


	// API
	public override bool FillFromSheet (string name) {
		base.FillFromSheet(name);
		SpriteGroupID = $"{name}.Wing".AngeHash();
		if (!Renderer.HasSpriteGroup(SpriteGroupID)) SpriteGroupID = 0;
		if (
			SpriteGroupID != 0 &&
			Renderer.TryGetSpriteFromGroup(SpriteGroupID, 0, out var sprite)
		) {
			IsPropeller = sprite.IsTrigger;
		}
		return SpriteLoaded;
	}


	public static void DrawGadgetFromPool (PoseCharacterRenderer renderer) {
		if (renderer.WingID != 0 && TryGetGadget(renderer.WingID, out var wing)) {
			wing.DrawGadget(renderer);
		}
	}


	public override void DrawGadget (PoseCharacterRenderer renderer) {

		if (!SpriteLoaded) return;
		using var _ = new SheetIndexScope(SheetIndex);

		DrawSpriteAsWing(renderer, SpriteGroupID, IsPropeller, Scale);
		if (IsPropeller && renderer.TargetCharacter.AnimationType == CharacterAnimationType.Fly) {
			renderer.TailID.Override(0, 1, priority: 4096);
		}
	}


	public static void DrawSpriteAsWing (PoseCharacterRenderer renderer, int spriteGroupID, bool isPropeller, int scale = 1000) {
		if (
			spriteGroupID == 0 ||
			!Renderer.HasSpriteGroup(spriteGroupID, out int groupCount) ||
			!Renderer.TryGetSpriteFromGroup(spriteGroupID, 0, out var firstSprite, false, true)
		) return;
		var aniType = renderer.TargetCharacter.AnimationType;
		int z = renderer.Body.FrontSide ? -33 : 33;
		int spriteHeight = firstSprite.GlobalHeight * scale / 1000;
		if (aniType == CharacterAnimationType.Rolling && !renderer.Body.FrontSide) {
			spriteHeight = -spriteHeight;
		}

		// Get Wing Position
		int xLeft;
		int yLeft;
		int xRight;
		int yRight;
		if (
			aniType != CharacterAnimationType.Sleep &&
			aniType != CharacterAnimationType.PassOut &&
			aniType != CharacterAnimationType.Fly
		) {
			// Standing Up
			var bodyRect = renderer.Body.GetGlobalRect();
			xLeft = bodyRect.xMin;
			yLeft = bodyRect.y;
			xRight = bodyRect.xMax;
			yRight = bodyRect.y;
		} else {
			// Lying Down
			xLeft = renderer.UpperLegL.GlobalX;
			yLeft = renderer.UpperLegL.GlobalY;
			xRight = renderer.UpperLegR.GlobalX;
			yRight = renderer.UpperLegR.GlobalY;
		}

		if (aniType == CharacterAnimationType.Fly) {
			// Flying
			if (isPropeller) {
				// Propeller
				if (Renderer.TryGetSpriteFromGroup(
					spriteGroupID, renderer.CurrentAnimationFrame.UMod(groupCount),
					out var sprite, true, true
				)) {
					Renderer.Draw(
						sprite,
						(xLeft + xRight) / 2,
						(yLeft + yRight) / 2,
						firstSprite.PivotX, firstSprite.PivotY, 0,
						firstSprite.GlobalWidth * scale / 1000,
						firstSprite.GlobalHeight * scale / 1000,
						z
					);
				}
			} else {
				// Wings
				if (Renderer.TryGetSpriteFromGroup(spriteGroupID, (renderer.CurrentAnimationFrame / 6).UMod(groupCount), out var sprite, true, true)) {
					Renderer.Draw(
						sprite,
						xLeft, yLeft, firstSprite.PivotX, firstSprite.PivotY, 0,
						firstSprite.GlobalWidth * scale / 1000,
						spriteHeight,
						z
					);
					Renderer.Draw(
						sprite,
						xRight, yRight, firstSprite.PivotX, firstSprite.PivotY, 0,
						-firstSprite.GlobalWidth * scale / 1000,
						spriteHeight,
						z
					);
				}
			}
		} else if (!isPropeller && firstSprite != null) {
			// Not Flying
			int rot = Game.GlobalFrame.PingPong(120) - 60;
			rot /= 12;
			int facingScaleL = 1000 + renderer.Body.Width.Sign3() * 300;
			int facingScaleR = 1000 - renderer.Body.Width.Sign3() * 300;
			// L
			Renderer.Draw(
				firstSprite,
				xLeft, yLeft, firstSprite.PivotX, firstSprite.PivotY, -rot,
				firstSprite.GlobalWidth * scale / 1000 * facingScaleL / 1000,
				spriteHeight,
				z
			);
			// R
			Renderer.Draw(
				firstSprite,
				xRight, yRight, firstSprite.PivotX, firstSprite.PivotY, rot,
				-firstSprite.GlobalWidth * scale / 1000 * facingScaleR / 1000,
				spriteHeight,
				z
			);
		}
	}


	public static bool IsPropellerWing (int wingID) => wingID != 0 && TryGetGadget(wingID, out var gadget) && gadget is Wing wing && wing.IsPropeller;


}
