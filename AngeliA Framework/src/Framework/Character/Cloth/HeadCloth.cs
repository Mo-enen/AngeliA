using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngeliA;


public enum HatFrontMode { FrontOfHead, BackOfHead, AlwaysFrontOfHead, AlwaysBackOfHead, }


public sealed class ModularHeadSuit : HeadCloth, IModularCloth { }


public abstract class HeadCloth : Cloth {

	protected sealed override ClothType ClothType => ClothType.Head;
	public override bool SpriteLoaded => SpriteID != 0;
	protected virtual HatFrontMode Front => HatFrontMode.FrontOfHead;
	protected virtual bool PixelShiftForLeft => true;
	private int SpriteID;

	public override bool FillFromSheet (string name) {
		base.FillFromSheet(name);
		SpriteID = $"{name}.HeadSuit".AngeHash();
		if (!Renderer.HasSprite(SpriteID) && !Renderer.HasSpriteGroup(SpriteID)) SpriteID = 0;
		return SpriteLoaded;
	}

	public static void DrawClothFromPool (PoseCharacter character) {
		if (character.SuitHead != 0 && character.CharacterState != CharacterState.Sleep && Pool.TryGetValue(character.SuitHead, out var cloth)) {
			cloth.DrawCloth(character);
		}
	}

	public override void DrawCloth (PoseCharacter character) {
		if (!SpriteLoaded) return;
		using var _ = new SheetIndexScope(SheetIndex);
		DrawClothForHead(character, SpriteID, Front, PixelShiftForLeft);
	}

	public static void DrawClothForHead (PoseCharacter character, int spriteOrGroupID, HatFrontMode frontMode, bool pixelShiftForLeft) {

		var head = character.Head;
		if (spriteOrGroupID == 0 || head.IsFullCovered) return;
		bool hideHead = false;
		bool showEar = false;

		// Width Amount
		int widthAmount = 1000;
		if (character.HeadTwist != 0) widthAmount -= character.HeadTwist.Abs() / 2;
		if (head.Height < 0) widthAmount = -widthAmount;

		// Draw
		Cell[] cells = null;
		if (Renderer.HasSpriteGroup(spriteOrGroupID)) {
			// Group
			if (head.FrontSide) {
				// Front
				bool front = frontMode != HatFrontMode.AlwaysBackOfHead && frontMode != HatFrontMode.BackOfHead;
				if (Renderer.TryGetSpriteFromGroup(spriteOrGroupID, 0, out var sprite, false, true)) {
					bool usePixelShift = pixelShiftForLeft && head.FrontSide && head.Width < 0;
					cells = AttachClothOn(
						head, sprite, 500, 1000,
						(front ? 34 : -34) - head.Z, widthAmount, 1000, 0,
						usePixelShift ? (front ? -16 : 16) : 0, 0
					);
					hideHead = sprite.Tag.HasAll(Tag.HideLimb);
					showEar = sprite.Tag.HasAll(Tag.ShowLimb);
				}
			} else {
				// Back
				if (Renderer.TryGetSpriteFromGroup(spriteOrGroupID, 1, out var sprite, false, true)) {
					bool front = frontMode != HatFrontMode.AlwaysBackOfHead && frontMode != HatFrontMode.FrontOfHead;
					bool usePixelShift = pixelShiftForLeft && head.FrontSide && head.Width < 0;
					cells = AttachClothOn(
						head, sprite, 500, 1000,
						(front ? 34 : -34) - head.Z, widthAmount, 1000, 0,
						usePixelShift ? (front ? -16 : 16) : 0, 0
					);
					hideHead = sprite.Tag.HasAll(Tag.HideLimb);
					showEar = sprite.Tag.HasAll(Tag.ShowLimb);
				}
			}
		} else if (Renderer.TryGetSprite(spriteOrGroupID, out var sprite)) {
			// Single Sprite
			bool front = frontMode != HatFrontMode.AlwaysBackOfHead && (
				frontMode == HatFrontMode.AlwaysFrontOfHead ||
				frontMode == HatFrontMode.FrontOfHead == head.FrontSide
			);
			bool usePixelShift = pixelShiftForLeft && head.FrontSide && head.Width < 0;
			cells = AttachClothOn(
				head, sprite, 500, 1000,
				(front ? 34 : -34) - head.Z, widthAmount, 1000, 0,
				usePixelShift ? (front ? -16 : 16) : 0, 0
			);
			hideHead = sprite.Tag.HasAll(Tag.HideLimb);
			showEar = sprite.Tag.HasAll(Tag.ShowLimb);
		}

		// Head Rotate
		if (cells != null && character.Head.Rotation != 0) {
			int offsetY = character.Head.Height.Abs() * character.Head.Rotation.Abs() / 360;
			foreach (var cell in cells) {
				cell.RotateAround(character.Head.Rotation, character.Body.GlobalX, character.Body.GlobalY + character.Body.Height);
				cell.Y -= offsetY;
			}
		}

		// Show/Hide Limb
		if (!showEar) {
			character.EarID.Override(0, 1, 4096);
		}
		if (hideHead) {
			character.HairID.Override(0, 1, 4096);
			character.Head.Covered = BodyPart.CoverMode.FullCovered;
		}

	}

}