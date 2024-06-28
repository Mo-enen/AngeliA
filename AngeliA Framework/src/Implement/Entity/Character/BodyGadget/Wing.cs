using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class AngelWing : Wing { protected override int Scale => 600; }
public class DevilWing : Wing { protected override int Scale => 600; }
public class PropellerWing : Wing { }


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


	public static void DrawGadgetFromPool (PoseCharacter character) {
		if (character.WingID != 0 && TryGetGadget(character.WingID, out var wing)) {
			wing.DrawGadget(character);
		}
	}


	public override void DrawGadget (PoseCharacter character) {

		if (!SpriteLoaded) return;
		using var _ = new SheetIndexScope(SheetIndex);

		DrawSpriteAsWing(character, SpriteGroupID, IsPropeller, Scale);
		if (IsPropeller && character.AnimationType == CharacterAnimationType.Fly) {
			character.TailID.Override(0, 1);
		}
	}


	public static void DrawSpriteAsWing (PoseCharacter character, int spriteGroupID, bool isPropeller, int scale = 1000) {
		if (
			spriteGroupID == 0 ||
			!Renderer.HasSpriteGroup(spriteGroupID, out int groupCount) ||
			!Renderer.TryGetSpriteFromGroup(spriteGroupID, 0, out var firstSprite, false, true)
		) return;
		int z = character.Body.FrontSide ? -33 : 33;
		int xLeft = character.UpperLegL.GlobalX;
		int yLeft = character.UpperLegL.GlobalY;
		int xRight = character.UpperLegR.GlobalX;
		int yRight = character.UpperLegR.GlobalY;
		int spriteHeight = firstSprite.GlobalHeight * character.Body.Height.Sign() * scale / 1000;
		var animatedPoseType = character.AnimationType;
		if (
			animatedPoseType != CharacterAnimationType.Sleep &&
			animatedPoseType != CharacterAnimationType.PassOut &&
			animatedPoseType != CharacterAnimationType.Fly
		) {
			var bodyRect = character.Body.GetGlobalRect();
			xLeft = bodyRect.xMin;
			yLeft = bodyRect.y;
			xRight = bodyRect.xMax;
			yRight = bodyRect.y;
		}
		if (animatedPoseType == CharacterAnimationType.Fly) {
			// Flying
			if (isPropeller) {
				// Propeller
				if (Renderer.TryGetSpriteFromGroup(
					spriteGroupID, character.CurrentAnimationFrame.UMod(groupCount),
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
				if (Renderer.TryGetSpriteFromGroup(spriteGroupID, (character.CurrentAnimationFrame / 6).UMod(groupCount), out var sprite, true, true)) {
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
			Renderer.Draw(
				firstSprite,
				xLeft, yLeft, firstSprite.PivotX, firstSprite.PivotY, -rot,
				firstSprite.GlobalWidth * scale / 1000,
				spriteHeight,
				z
			);
			Renderer.Draw(
				firstSprite,
				xRight, yRight, firstSprite.PivotX, firstSprite.PivotY, rot,
				-firstSprite.GlobalWidth * scale / 1000,
				spriteHeight,
				z
			);
		}
	}


	public static bool IsPropellerWing (int wingID) => wingID != 0 && TryGetGadget(wingID, out var gadget) && gadget is Wing wing && wing.IsPropeller;


}
