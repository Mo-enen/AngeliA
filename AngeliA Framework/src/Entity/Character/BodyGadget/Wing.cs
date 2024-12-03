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


	// VAR
	public bool IsPropeller { get; private set; } = false;
	public override bool SpriteLoaded => SpriteWing.IsValid;
	protected sealed override BodyGadgetType GadgetType => BodyGadgetType.Wing;
	protected virtual int Scale => 1000;
	public OrientedSprite SpriteWing { get; private set; }


	// API
	public override bool FillFromSheet (string name) {
		base.FillFromSheet(name);
		SpriteWing = new OrientedSprite(name, "Wing", "Propeller");
		IsPropeller = SpriteWing.AttachmentName == "Propeller";
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

		DrawSpriteAsWing(renderer, SpriteWing, IsPropeller, Scale);
		if (IsPropeller && renderer.TargetCharacter.AnimationType == CharacterAnimationType.Fly) {
			renderer.TailID.Override(0, 1, priority: 4096);
		}
	}


	public static void DrawSpriteAsWing (PoseCharacterRenderer renderer, OrientedSprite oSprite, bool isPropeller, int scale = 1000) {
		if (
			!oSprite.IsValid ||
			!Renderer.HasSpriteGroup(oSprite.GroupID, out int groupCount)
		) return;
		var aniType = renderer.TargetCharacter.AnimationType;
		int z = renderer.Body.FrontSide ? -33 : 33;
		int signY = aniType == CharacterAnimationType.Rolling && !renderer.Body.FrontSide ? 1 : -1;

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
					oSprite.GroupID, renderer.CurrentAnimationFrame.UMod(groupCount),
					out var sprite, true, true
				)) {
					Renderer.Draw(
						sprite,
						(xLeft + xRight) / 2,
						(yLeft + yRight) / 2,
						sprite.PivotX, sprite.PivotY, 0,
						sprite.GlobalWidth * scale / 1000,
						sprite.GlobalHeight * scale / 1000,
						z
					);
				}
			} else {
				// Wings
				if (Renderer.TryGetSpriteFromGroup(oSprite.GroupID, (renderer.CurrentAnimationFrame / 6).UMod(groupCount), out var sprite, true, true)) {
					Renderer.Draw(
						sprite,
						xLeft, yLeft, sprite.PivotX, sprite.PivotY, 0,
						sprite.GlobalWidth * scale / 1000,
						signY * sprite.GlobalHeight * scale / 1000,
						z
					);
					Renderer.Draw(
						sprite,
						xRight, yRight, sprite.PivotX, sprite.PivotY, 0,
						-sprite.GlobalWidth * scale / 1000,
						signY * sprite.GlobalHeight * scale / 1000,
						z
					);
				}
			}
		} else if (!isPropeller && Renderer.TryGetSpriteFromGroup(oSprite.GroupID, 0, out var firstSprite, false, true)) {
			// Not Flying with Wing
			int rot = Game.GlobalFrame.PingPong(120) - 60;
			rot /= 12;
			int facingScaleL = 1000 + renderer.Body.Width.Sign3() * 300;
			int facingScaleR = 1000 - renderer.Body.Width.Sign3() * 300;
			// L
			Renderer.Draw(
				firstSprite,
				xLeft, yLeft, firstSprite.PivotX, firstSprite.PivotY, -rot,
				firstSprite.GlobalWidth * scale / 1000 * facingScaleL / 1000,
				signY * firstSprite.GlobalHeight * scale / 1000,
				z
			);
			// R
			Renderer.Draw(
				firstSprite,
				xRight, yRight, firstSprite.PivotX, firstSprite.PivotY, rot,
				-firstSprite.GlobalWidth * scale / 1000 * facingScaleR / 1000,
				signY * firstSprite.GlobalHeight * scale / 1000,
				z
			);
		}
	}


	public static bool IsPropellerWing (int wingID) => wingID != 0 && TryGetGadget(wingID, out var gadget) && gadget is Wing wing && wing.IsPropeller;


}
