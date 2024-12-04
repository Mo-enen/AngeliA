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


public sealed class ModularBodySuit : BodyCloth, IModularCloth { }


public abstract class BodyCloth : Cloth {

	protected sealed override ClothType ClothType => ClothType.Body;
	public override bool SpriteLoaded => SpriteBody.IsValid;
	private OrientedSprite SpriteBody;
	private OrientedSprite SpriteCape;
	private OrientedSprite SpriteShoulderLeft;
	private OrientedSprite SpriteShoulderRight;
	private OrientedSprite SpriteUpperArmLeft;
	private OrientedSprite SpriteUpperArmRight;
	private OrientedSprite SpriteLowerArmLeft;
	private OrientedSprite SpriteLowerArmRight;
	protected virtual int TwistShiftTopAmount => 300;
	protected virtual int LocalZ => 7;

	public override bool FillFromSheet (string name) {
		base.FillFromSheet(name);
		SpriteBody = new OrientedSprite(name, "BodySuit");
		SpriteCape = new OrientedSprite(name, "CapeSuit");
		SpriteShoulderLeft = new OrientedSprite(name, "ShoulderSuitLeft", "ShoulderSuit");
		SpriteShoulderRight = new OrientedSprite(name, "ShoulderSuitRight", "ShoulderSuit");
		SpriteUpperArmLeft = new OrientedSprite(name, "UpperArmSuitLeft", "UpperArmSuit");
		SpriteUpperArmRight = new OrientedSprite(name, "UpperArmSuitRight", "UpperArmSuit");
		SpriteLowerArmLeft = new OrientedSprite(name, "LowerArmSuitLeft", "LowerArmSuit");
		SpriteLowerArmRight = new OrientedSprite(name, "LowerArmSuitRight", "LowerArmSuit");
		return SpriteLoaded;
	}

	public static void DrawClothFromPool (PoseCharacterRenderer renderer) {
		if (renderer.SuitBody != 0 && Pool.TryGetValue(renderer.SuitBody, out var cloth)) {
			cloth.DrawCloth(renderer);
		}
	}

	public override void DrawCloth (PoseCharacterRenderer renderer) {
		if (!SpriteLoaded) return;
		using var _ = new SheetIndexScope(SheetIndex);
		DrawClothForBody(renderer, SpriteBody, LocalZ, TwistShiftTopAmount);
		DrawCape(renderer, SpriteCape);
		DrawClothForShoulder(renderer, SpriteShoulderLeft, SpriteShoulderRight);
		DrawClothForUpperArm(renderer, SpriteUpperArmLeft, SpriteUpperArmRight);
		DrawClothForLowerArm(renderer, SpriteLowerArmLeft, SpriteLowerArmRight);
	}

	public static void DrawClothForBody (PoseCharacterRenderer renderer, OrientedSprite clothSprite, int localZ, int twistShiftTopAmount) {

		if (!clothSprite.IsValid) return;
		if (renderer.Body.IsFullCovered) return;


		var body = renderer.Body;
		bool facingRight = body.Width > 0;
		bool facingFront = body.FrontSide;
		if (!clothSprite.TryGetSprite(
			facingFront, facingRight, renderer.CurrentAnimationFrame, out var suitSprite
		)) return;

		bool separatedSprite = clothSprite.SpriteID_FL != clothSprite.SpriteID_FR;
		var hip = renderer.Hip;
		int poseTwist = renderer.BodyTwist;
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
		body.Covered = suitSprite.Tag.HasAll(Tag.HideLimb) ?
			 BodyPart.CoverMode.FullCovered : BodyPart.CoverMode.Covered;

	}

	public static void DrawClothForShoulder (PoseCharacterRenderer renderer, OrientedSprite spriteLeft, OrientedSprite spriteRight, int localZ = 1) {
		bool facingRight = renderer.Body.Width > 0;
		if (spriteLeft.IsValid) {
			spriteLeft.TryGetSprite(
				renderer.ShoulderL.FrontSide, facingRight, renderer.CurrentAnimationFrame, out var sprite
			);
			CoverClothOn(renderer.ShoulderL, sprite, localZ);
		}
		if (spriteRight.IsValid) {
			spriteRight.TryGetSprite(
				renderer.ShoulderR.FrontSide, facingRight, renderer.CurrentAnimationFrame, out var sprite
			);
			CoverClothOn(renderer.ShoulderR, sprite, localZ);
		}
	}

	public static void DrawClothForUpperArm (PoseCharacterRenderer renderer, OrientedSprite spriteLeft, OrientedSprite spriteRight, int localZ = 1) {
		bool facingRight = renderer.Body.Width > 0;
		if (spriteLeft.IsValid) {
			spriteLeft.TryGetSprite(
				renderer.UpperArmL.FrontSide, facingRight, renderer.CurrentAnimationFrame, out var sprite
			);
			CoverClothOn(renderer.UpperArmL, sprite, localZ);
		}
		if (spriteRight.IsValid) {
			spriteRight.TryGetSprite(
				renderer.UpperArmR.FrontSide, facingRight, renderer.CurrentAnimationFrame, out var sprite
			);
			CoverClothOn(renderer.UpperArmR, sprite, localZ);
		}
	}

	public static void DrawClothForLowerArm (PoseCharacterRenderer renderer, OrientedSprite spriteLeft, OrientedSprite spriteRight, int localZ = 1) {
		bool facingRight = renderer.Body.Width > 0;
		if (spriteLeft.IsValid) {
			spriteLeft.TryGetSprite(
				renderer.LowerArmL.FrontSide, facingRight, renderer.CurrentAnimationFrame, out var sprite
			);
			CoverClothOn(renderer.LowerArmL, sprite, localZ);
		}
		if (spriteRight.IsValid) {
			spriteRight.TryGetSprite(
				renderer.LowerArmR.FrontSide, facingRight, renderer.CurrentAnimationFrame, out var sprite
			);
			CoverClothOn(renderer.LowerArmR, sprite, localZ);
		}
	}

	public static void DrawCape (PoseCharacterRenderer renderer, OrientedSprite clothSprite, int motionAmount = 1000) {

		if (!clothSprite.IsValid) return;
		var body = renderer.Body;
		bool facingRight = body.Width > 0;
		bool facingFront = body.FrontSide;
		if (!clothSprite.TryGetSprite(facingFront, facingRight, renderer.CurrentAnimationFrame, out var sprite)) return;

		var animatedPoseType = renderer.TargetCharacter.AnimationType;

		if (
			animatedPoseType == CharacterAnimationType.SquatIdle ||
			animatedPoseType == CharacterAnimationType.SquatMove ||
			animatedPoseType == CharacterAnimationType.Dash ||
			animatedPoseType == CharacterAnimationType.Rolling ||
			animatedPoseType == CharacterAnimationType.Spin ||
			animatedPoseType == CharacterAnimationType.Fly ||
			animatedPoseType.IsLyingDown()
		) return;

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
			int offsetX = (-renderer.TargetCharacter.DeltaPositionX * motionAmount / 1000).Clamp(-maxX, maxX);
			cells[3].X += offsetX / 2;
			cells[4].X += offsetX / 2;
			cells[5].X += offsetX / 2;
			cells[6].X += offsetX;
			cells[7].X += offsetX;
			cells[8].X += offsetX;
			// Y
			int maxY = 20 * motionAmount / 1000;
			int offsetAmountY = 1000 + (renderer.TargetCharacter.DeltaPositionY * motionAmount / 10000).Clamp(-maxY, maxY) * 1000 / 20;
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