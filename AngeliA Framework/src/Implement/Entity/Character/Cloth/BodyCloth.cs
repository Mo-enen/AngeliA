using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngeliA;


public sealed class DefaultBodySuit : BodyCloth {
	public static readonly int TYPE_ID = typeof(DefaultBodySuit).AngeHash();
	public DefaultBodySuit () => FillFromSheet(GetType().AngeName());
}


public class BodyCloth : Cloth {

	protected sealed override ClothType ClothType => ClothType.Body;
	private int SpriteIdFrontL;
	private int SpriteIdFrontR;
	private int SpriteIdShoulder;
	private int SpriteIdUpperArm;
	private int SpriteIdLowerArm;
	protected virtual int TwistShiftTopAmount => 300;
	protected virtual int LocalZ => 7;

	protected override bool FillFromSheet (string name) {
		SpriteIdFrontL = SpriteIdFrontR = $"{name}.BodySuit".AngeHash();
		if (!Renderer.HasSprite(SpriteIdFrontL) && !Renderer.HasSpriteGroup(SpriteIdFrontL)) SpriteIdFrontL = 0;
		if (SpriteIdFrontL == 0) {
			SpriteIdFrontL = $"{name}.BodySuitL".AngeHash();
			SpriteIdFrontR = $"{name}.BodySuitR".AngeHash();
			if (!Renderer.HasSprite(SpriteIdFrontL) && !Renderer.HasSpriteGroup(SpriteIdFrontL)) SpriteIdFrontL = 0;
			if (!Renderer.HasSprite(SpriteIdFrontR) && !Renderer.HasSpriteGroup(SpriteIdFrontR)) SpriteIdFrontR = SpriteIdFrontL;
		}
		SpriteIdShoulder = $"{name}.ShoulderSuit".AngeHash();
		SpriteIdUpperArm = $"{name}.UpperArmSuit".AngeHash();
		SpriteIdLowerArm = $"{name}.LowerArmSuit".AngeHash();
		if (!Renderer.HasSprite(SpriteIdShoulder) && !Renderer.HasSpriteGroup(SpriteIdShoulder)) SpriteIdShoulder = 0;
		if (!Renderer.HasSprite(SpriteIdUpperArm) && !Renderer.HasSpriteGroup(SpriteIdUpperArm)) SpriteIdUpperArm = 0;
		if (!Renderer.HasSprite(SpriteIdLowerArm) && !Renderer.HasSpriteGroup(SpriteIdLowerArm)) SpriteIdLowerArm = 0;
		return SpriteIdFrontL != 0 || SpriteIdFrontR != 0;
	}

	public static void DrawClothFromPool (PoseCharacter character) {
		if (character.SuitBody != 0 && Pool.TryGetValue(character.SuitBody, out var cloth)) {
			cloth.Draw(character);
		}
	}

	public override void Draw (PoseCharacter character) {
		DrawClothForBody(character, SpriteIdFrontL, SpriteIdFrontR, LocalZ, TwistShiftTopAmount);
		DrawCape(character, TypeID);
		DrawClothForShoulder(character, SpriteIdShoulder);
		DrawClothForUpperArm(character, SpriteIdUpperArm);
		DrawClothForLowerArm(character, SpriteIdLowerArm);
	}

	public static void DrawClothForBody (PoseCharacter character, int spriteIdFrontL, int spriteIdFrontR, int localZ, int twistShiftTopAmount) {

		if (spriteIdFrontL == 0 && spriteIdFrontR == 0) return;

		var body = character.Body;
		bool facingRight = body.Width > 0;
		bool separatedSprite = spriteIdFrontL != spriteIdFrontR;
		int spriteGroupId = facingRight ? spriteIdFrontR : spriteIdFrontL;
		if (spriteGroupId == 0 || body.IsFullCovered) return;

		var hip = character.Hip;
		int poseTwist = character.BodyTwist;
		int groupIndex = body.FrontSide ? 0 : 1;
		if (!Renderer.TryGetSpriteFromGroup(spriteGroupId, groupIndex, out var suitSprite, false, true)) return;

		var rect = new IRect(
			body.GlobalX - body.Width / 2,
			hip.GlobalY,
			body.Width,
			body.Height + hip.Height
		);

		// Border
		if (!suitSprite.GlobalBorder.IsZero) {
			if (rect.width > 0) {
				rect = rect.Expand(
					suitSprite.GlobalBorder.left,
					suitSprite.GlobalBorder.right,
					suitSprite.GlobalBorder.down,
					suitSprite.GlobalBorder.up
				);
			} else {
				rect = rect.Expand(
					-suitSprite.GlobalBorder.left,
					-suitSprite.GlobalBorder.right,
					suitSprite.GlobalBorder.down,
					suitSprite.GlobalBorder.up
				);
			}
		}

		// Flip
		bool flipX = separatedSprite && !facingRight;
		if (flipX) rect.FlipHorizontal();

		// Draw
		var cell = Renderer.Draw(suitSprite, rect, body.Z + localZ);

		// Twist
		if (poseTwist != 0 && body.FrontSide && body.Height > 0) {
			if (flipX) poseTwist = -poseTwist;
			int shiftTop = body.Height * twistShiftTopAmount / 1000;
			int shiftX = poseTwist * cell.Width / 2500;
			var cellL = Renderer.DrawPixel(default);
			cellL.CopyFrom(cell);
			var cellR = Renderer.DrawPixel(default);
			cellR.CopyFrom(cell);
			cellL.Shift.up = cellR.Shift.up = shiftTop;
			cellL.Width += body.Width.Sign() * shiftX;
			cellL.Shift.right = cellL.Width.Abs() / 2;
			cellR.Width -= body.Width.Sign() * shiftX;
			cellR.X = cellL.X + cellL.Width / 2 - cellR.Width / 2;
			cellR.Shift.left = cellR.Width.Abs() / 2;
			cell.Shift.down = cell.Height - shiftTop;
		}

		// Hide Limb
		body.Covered = suitSprite.Tag.HasTag(Tag.HideLimb) ?
			 BodyPart.CoverMode.FullCovered : BodyPart.CoverMode.Covered;

	}

	public static void DrawClothForShoulder (PoseCharacter character, int spriteID) {
		if (spriteID == 0) return;
		if (Renderer.HasSpriteGroup(spriteID)) {
			if (Renderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
				CoverClothOn(character.ShoulderL, spriteL.ID);
			}
			if (Renderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
				CoverClothOn(character.ShoulderR, spriteR.ID);
			}
		} else {
			CoverClothOn(character.ShoulderL, spriteID);
			CoverClothOn(character.ShoulderR, spriteID);
		}
	}

	public static void DrawClothForUpperArm (PoseCharacter character, int spriteID, int localZ = 1) {
		if (spriteID == 0) return;
		if (Renderer.HasSpriteGroup(spriteID)) {
			if (Renderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
				CoverClothOn(character.UpperArmL, spriteL.ID, localZ);
			}
			if (Renderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
				CoverClothOn(character.UpperArmR, spriteR.ID, localZ);
			}
		} else {
			CoverClothOn(character.UpperArmL, spriteID, localZ);
			CoverClothOn(character.UpperArmR, spriteID, localZ);
		}
	}

	public static void DrawClothForLowerArm (PoseCharacter character, int spriteID, int localZ = 1) {
		if (spriteID == 0) return;
		if (Renderer.HasSpriteGroup(spriteID)) {
			if (Renderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
				CoverClothOn(character.LowerArmL, spriteL.ID, localZ);
			}
			if (Renderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
				CoverClothOn(character.LowerArmR, spriteR.ID, localZ);
			}
		} else {
			CoverClothOn(character.LowerArmL, spriteID, localZ);
			CoverClothOn(character.LowerArmR, spriteID, localZ);
		}
	}

	public static void DrawCape (PoseCharacter character, int capeID, int motionAmount = 1000) {
		if (capeID == 0) return;
		if (!Renderer.TryGetSpriteGroup(capeID, out var group) || group.Count < 4) return;
		if (!Renderer.TryGetSpriteFromGroup(capeID, character.Body.FrontSide ? 2 : 3, out var sprite, false, false)) return;
		var animatedPoseType = character.AnimationType;
		if (
			animatedPoseType == CharacterAnimationType.SquatIdle ||
			animatedPoseType == CharacterAnimationType.SquatMove ||
			animatedPoseType == CharacterAnimationType.Dash ||
			animatedPoseType == CharacterAnimationType.Rolling ||
			animatedPoseType == CharacterAnimationType.Spin ||
			animatedPoseType == CharacterAnimationType.Fly ||
			animatedPoseType == CharacterAnimationType.Sleep ||
			animatedPoseType == CharacterAnimationType.Crash ||
			animatedPoseType == CharacterAnimationType.PassOut
		) return;
		DrawCape(character, sprite, motionAmount);
		// Func
		static void DrawCape (PoseCharacter character, AngeSprite sprite, int motionAmount = 1000) {

			var body = character.Body;

			// Draw
			int height = sprite.GlobalHeight + body.Height.Abs() - body.SizeY;
			var cells = Renderer.DrawSlice(
				sprite,
				body.GlobalX, body.GlobalY + body.Height,
				500, 1000, 0,
				sprite.GlobalWidth,
				body.Height.Sign() * height, Color32.WHITE, body.FrontSide ? -31 : 31
			);

			// Flow Motion
			if (motionAmount != 0) {
				// X
				int maxX = 30 * motionAmount / 1000;
				int offsetX = (-character.DeltaPositionX * motionAmount / 1000).Clamp(-maxX, maxX);
				cells[3].X += offsetX / 2;
				cells[4].X += offsetX / 2;
				cells[5].X += offsetX / 2;
				cells[6].X += offsetX;
				cells[7].X += offsetX;
				cells[8].X += offsetX;
				// Y
				int maxY = 20 * motionAmount / 1000;
				int offsetAmountY = 1000 + (character.DeltaPositionY * motionAmount / 10000).Clamp(-maxY, maxY) * 1000 / 20;
				offsetAmountY = offsetAmountY.Clamp(800, 1200);
				cells[0].Height = cells[0].Height * offsetAmountY / 1000;
				cells[1].Height = cells[1].Height * offsetAmountY / 1000;
				cells[2].Height = cells[2].Height * offsetAmountY / 1000;
				cells[3].Height = cells[3].Height * offsetAmountY / 1000;
				cells[4].Height = cells[4].Height * offsetAmountY / 1000;
				cells[5].Height = cells[5].Height * offsetAmountY / 1000;
				cells[6].Height = cells[6].Height * offsetAmountY / 1000;
				cells[7].Height = cells[7].Height * offsetAmountY / 1000;
				cells[8].Height = cells[8].Height * offsetAmountY / 1000;
			}
		}
	}

}