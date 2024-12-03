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

		if (!oSprite.IsValid) return;
		AngeSprite singleSprite = null;
		if (!Renderer.TryGetAnimationGroup(oSprite.GroupID, out var group) || group.Count == 0) {
			if (oSprite.TryGetSprite(renderer.Body.FrontSide, renderer.Body.Width > 0, out singleSprite)) {
				group = null;
			} else {
				return;
			}
		}

		var aniType = renderer.TargetCharacter.AnimationType;
		int z = renderer.Body.FrontSide ? -33 : 33;
		int signY = aniType == CharacterAnimationType.Rolling && !renderer.Body.FrontSide ? -1 : 1;

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
				var sprite = singleSprite;
				if (group != null) {
					Renderer.CurrentSheet.TryGetSpriteFromAnimationFrame(group, renderer.CurrentAnimationFrame, out sprite);
				}
				if (sprite != null) {
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
				var sprite = singleSprite;
				if (group != null) {
					Renderer.CurrentSheet.TryGetSpriteFromAnimationFrame(
						group, renderer.CurrentAnimationFrame, out sprite
					);
				}
				if (sprite != null) {
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
		} else if (!isPropeller) {
			var sprite = group != null ? group.Sprites[0] : singleSprite;
			if (sprite != null) {
				// Not Flying with Wing
				int rot = (Game.GlobalFrame.PingPong(120) - 60) / 12;
				int signW = renderer.Body.Width.Sign3();
				int facingScaleL = 1000;
				int facingScaleR = 1000;
				if (renderer.Body.FrontSide) {
					facingScaleL = 1000 + signW * 300;
					facingScaleR = 1000 - signW * 300;
				}
				// L
				Renderer.Draw(
					sprite,
					xLeft, yLeft, sprite.PivotX, sprite.PivotY, -rot,
					sprite.GlobalWidth * scale / 1000 * facingScaleL / 1000,
					signY * sprite.GlobalHeight * scale / 1000,
					z
				);
				// R
				Renderer.Draw(
					sprite,
					xRight, yRight, sprite.PivotX, sprite.PivotY, rot,
					-sprite.GlobalWidth * scale / 1000 * facingScaleR / 1000,
					signY * sprite.GlobalHeight * scale / 1000,
					z

				);
			}
		}
	}


	public static bool IsPropellerWing (int wingID) => wingID != 0 && TryGetGadget(wingID, out var gadget) && gadget is Wing wing && wing.IsPropeller;


}
