using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngeliA;


public enum HatFrontMode { FrontOfHead, BackOfHead, AlwaysFrontOfHead, AlwaysBackOfHead, }


public class HeadCloth : Cloth {

	protected sealed override ClothType ClothType => ClothType.Head;
	protected virtual HatFrontMode Front => HatFrontMode.FrontOfHead;
	protected virtual bool PixelShiftForLeft => true;
	private int SpriteID;

	protected override bool FillFromSheet (string name) {
		SpriteID = $"{name}.HeadSuit".AngeHash();
		if (!Renderer.HasSprite(SpriteID) && !Renderer.HasSpriteGroup(SpriteID)) SpriteID = 0;
		return SpriteID != 0;
	}

	public static void DrawClothFromPool (PoseCharacter character) {
		if (character.Suit_Head != 0 && character.CharacterState != CharacterState.Sleep && Pool.TryGetValue(character.Suit_Head, out var cloth)) {
			cloth.Draw(character);
		}
	}

	public override void Draw (PoseCharacter character) => DrawClothForHead(character, SpriteID, Front, PixelShiftForLeft);

	public static void DrawClothForHead (PoseCharacter character, int spriteGroupID, HatFrontMode frontMode, bool pixelShiftForLeft) {

		var head = character.Head;
		if (spriteGroupID == 0 || head.IsFullCovered) return;

		// Width Amount
		int widthAmount = 1000;
		if (character.HeadTwist != 0) widthAmount -= character.HeadTwist.Abs() / 2;
		if (head.Height < 0) widthAmount = -widthAmount;

		// Draw
		Cell[] cells = null;
		if (Renderer.HasSpriteGroup(spriteGroupID)) {
			if (head.FrontSide) {
				// Front
				bool front = frontMode != HatFrontMode.AlwaysBackOfHead && frontMode != HatFrontMode.BackOfHead;
				if (Renderer.TryGetSpriteFromGroup(spriteGroupID, 0, out var sprite, false, true)) {
					bool usePixelShift = pixelShiftForLeft && head.FrontSide && head.Width < 0;
					cells = AttachClothOn(
						head, sprite, 500, 1000,
						(front ? 34 : -34) - head.Z, widthAmount, 1000, 0,
						usePixelShift ? (front ? -16 : 16) : 0, 0
					);
				}
			} else {
				// Back
				if (Renderer.TryGetSpriteFromGroup(spriteGroupID, 1, out var sprite, false, true)) {
					bool front = frontMode != HatFrontMode.AlwaysBackOfHead && frontMode != HatFrontMode.FrontOfHead;
					bool usePixelShift = pixelShiftForLeft && head.FrontSide && head.Width < 0;
					cells = AttachClothOn(
						head, sprite, 500, 1000,
						(front ? 34 : -34) - head.Z, widthAmount, 1000, 0,
						usePixelShift ? (front ? -16 : 16) : 0, 0
					);
				}
			}
		} else if (Renderer.TryGetSprite(spriteGroupID, out var sprite)) {
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
		}
		// Head Rotate
		if (cells != null && character.HeadRotation != 0) {
			int offsetY = character.Head.Height.Abs() * character.HeadRotation.Abs() / 360;
			foreach (var cell in cells) {
				cell.RotateAround(character.HeadRotation, character.Body.GlobalX, character.Body.GlobalY + character.Body.Height);
				cell.Y -= offsetY;
			}
		}
	}

}