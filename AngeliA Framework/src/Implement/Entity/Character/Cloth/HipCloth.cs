namespace AngeliA;


public sealed class DefaultHipSuit : HipCloth {
	public static readonly int TYPE_ID = typeof(DefaultHipSuit).AngeHash();
}


public abstract class HipCloth : Cloth {

	protected sealed override ClothType ClothType => ClothType.Hip;
	protected virtual bool CoverLegs => true;
	private int SpriteIdHip { get; init; }
	private int SpriteIdSkirt { get; init; }
	private int SpriteIdUpperLeg { get; init; }
	private int SpriteIdLowerLeg { get; init; }

	public HipCloth () {
		string name = (GetType().DeclaringType ?? GetType()).AngeName();
		SpriteIdHip = $"{name}.HipSuit".AngeHash();
		SpriteIdSkirt = $"{name}.SkirtSuit".AngeHash();
		SpriteIdUpperLeg = $"{name}.UpperLegSuit".AngeHash();
		SpriteIdLowerLeg = $"{name}.LowerLegSuit".AngeHash();
		if (!Renderer.HasSprite(SpriteIdHip) && !Renderer.HasSpriteGroup(SpriteIdHip)) SpriteIdHip = 0;
		if (!Renderer.HasSprite(SpriteIdSkirt) && !Renderer.HasSpriteGroup(SpriteIdSkirt)) SpriteIdSkirt = 0;
		if (!Renderer.HasSprite(SpriteIdUpperLeg) && !Renderer.HasSpriteGroup(SpriteIdUpperLeg)) SpriteIdUpperLeg = 0;
		if (!Renderer.HasSprite(SpriteIdLowerLeg) && !Renderer.HasSpriteGroup(SpriteIdLowerLeg)) SpriteIdLowerLeg = 0;
	}

	public static void DrawClothFromPool (PoseCharacter character) {
		if (character.Suit_Hip != 0 && Pool.TryGetValue(character.Suit_Hip, out var cloth)) {
			cloth.Draw(character);
		}
	}

	public override void Draw (PoseCharacter character) {
		DrawClothForHip(character, SpriteIdHip, CoverLegs ? 4 : 1);
		DrawClothForSkirt(character, SpriteIdSkirt, CoverLegs ? 6 : 1);
		DrawClothForUpperLeg(character, SpriteIdUpperLeg);
		DrawClothForLowerLeg(character, SpriteIdLowerLeg);
	}

	public static void DrawClothForHip (PoseCharacter character, int spriteID, int localZ = 1) {

		var hip = character.Hip;
		if (spriteID == 0 || hip.IsFullCovered) return;
		if (
			!Renderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 0 : 1, out var sprite, false, true) &&
			!Renderer.TryGetSprite(spriteID, out sprite)
		) return;

		var rect = hip.GetGlobalRect();
		if (!sprite.GlobalBorder.IsZero) {
			if (hip.Width > 0) {
				rect = rect.Expand(
					sprite.GlobalBorder.left,
					sprite.GlobalBorder.right,
					sprite.GlobalBorder.down,
					sprite.GlobalBorder.up
				);
			} else {
				rect = rect.Expand(
					sprite.GlobalBorder.right,
					sprite.GlobalBorder.left,
					sprite.GlobalBorder.down,
					sprite.GlobalBorder.up
				);
			}
		}

		// Limb
		hip.Covered = sprite.Tag == SpriteTag.HIDE_LIMB_TAG ?
			 BodyPart.CoverMode.FullCovered : BodyPart.CoverMode.Covered;

		// Draw
		Renderer.Draw(sprite, rect, hip.Z + localZ);

	}

	public static void DrawClothForSkirt (PoseCharacter character, int spriteID, int localZ = 6) {

		var hip = character.Hip;
		if (spriteID == 0 || hip.IsFullCovered) return;
		if (
			!Renderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 0 : 1, out var sprite, false, true) &&
			!Renderer.TryGetSprite(spriteID, out sprite)
		) return;

		var body = character.Body;
		var upperLegL = character.UpperLegL;
		var upperLegR = character.UpperLegR;
		var animatedPoseType = character.AnimationType;
		const int A2G = 16;

		// Skirt
		int bodyWidthAbs = body.Width.Abs();
		var legTopL = upperLegL.GlobalLerp(0.5f, 1f);
		var legTopR = upperLegR.GlobalLerp(0.5f, 1f);
		int left = legTopL.x - upperLegL.SizeX / 2;
		int right = legTopR.x + upperLegR.SizeX / 2;
		int centerX = (left + right) / 2;
		int centerY = (legTopL.y + legTopR.y) / 2;
		bool stretch =
			animatedPoseType != CharacterAnimationType.GrabSide &&
			animatedPoseType != CharacterAnimationType.Dash &&
			animatedPoseType != CharacterAnimationType.Idle;
		int width = Util.Max(
			(right - left).Abs(), bodyWidthAbs - body.Border.left - body.Border.right
		);
		width += sprite.GlobalBorder.horizontal;
		if (stretch) width += Stretch(upperLegL.Rotation, upperLegR.Rotation);
		width += animatedPoseType switch {
			CharacterAnimationType.JumpUp or CharacterAnimationType.JumpDown => 2 * A2G,
			CharacterAnimationType.Run => A2G / 2,
			_ => 0,
		};
		int shiftY = animatedPoseType switch {
			CharacterAnimationType.Dash => A2G,
			_ => 0,
		};
		int offsetY = sprite.GlobalHeight * (1000 - sprite.PivotY) / 1000 + shiftY;
		Renderer.Draw(
			sprite,
			centerX,
			body.Height > 0 ? Util.Max(centerY + offsetY, character.Y + sprite.GlobalHeight) : centerY - offsetY,
			500, 1000, 0,
			width,
			body.Height > 0 ? sprite.GlobalHeight : -sprite.GlobalHeight,
			hip.Z + localZ
		);

		// Limb
		hip.Covered = sprite.Tag == SpriteTag.HIDE_LIMB_TAG ?
			BodyPart.CoverMode.FullCovered : BodyPart.CoverMode.Covered;

		// Func
		static int Stretch (int rotL, int rotR) {
			int result = 0;
			if (rotL > 0) result += rotL / 2;
			if (rotR < 0) result += rotR / -2;
			return result;
		}
	}

	public static void DrawClothForUpperLeg (PoseCharacter character, int spriteID, int localZ = 1) {
		if (spriteID == 0) return;
		if (Renderer.HasSpriteGroup(spriteID)) {
			if (Renderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
				CoverClothOn(character.UpperLegL, spriteL.ID, localZ);
			}
			if (Renderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
				CoverClothOn(character.UpperLegR, spriteR.ID, localZ);
			}
		} else {
			CoverClothOn(character.UpperLegL, spriteID, localZ);
			CoverClothOn(character.UpperLegR, spriteID, localZ);
		}
	}

	public static void DrawClothForLowerLeg (PoseCharacter character, int spriteID, int localZ = 1) {
		if (spriteID == 0) return;
		if (Renderer.HasSpriteGroup(spriteID)) {
			if (Renderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
				CoverClothOn(character.LowerLegL, spriteL.ID, localZ);
			}
			if (Renderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
				CoverClothOn(character.LowerLegR, spriteR.ID, localZ);
			}
		} else {
			CoverClothOn(character.LowerLegL, spriteID, localZ);
			CoverClothOn(character.LowerLegR, spriteID, localZ);
		}
	}

	public static void DrawDoubleClothTailsOnHip (PoseCharacter character, int spriteIdLeft, int spriteIdRight, bool drawOnAllPose = false) {

		var animatedPoseType = character.AnimationType;
		var hip = character.Hip;
		var body = character.Body;
		if (
			!drawOnAllPose && (
				animatedPoseType == CharacterAnimationType.Rolling ||
				animatedPoseType == CharacterAnimationType.Sleep ||
				animatedPoseType == CharacterAnimationType.PassOut ||
				animatedPoseType == CharacterAnimationType.Fly
			)
		) return;

		var hipRect = hip.GetGlobalRect();
		int z = body.FrontSide ? -39 : 39;
		bool facingRight = body.Width > 0;
		int rotL = facingRight ? 30 : 18;
		int rotR = facingRight ? -18 : -30;
		int scaleX = 1000;
		int scaleY = 1000;

		if (body.Height < 0) {
			rotL = 180 - rotL;
			rotR = -180 + rotR;
			z = -z;
		}

		if (animatedPoseType == CharacterAnimationType.Dash) scaleY = 500;

		DrawClothTail(character, spriteIdLeft, hipRect.x + 16, hipRect.y, z, rotL, scaleX, scaleY);
		DrawClothTail(character, spriteIdRight, hipRect.xMax - 16, hipRect.y, z, rotR, scaleX, scaleY);

	}

	public static void DrawClothTail (PoseCharacter character, int spriteID, int globalX, int globalY, int z, int rotation, int scaleX = 1000, int scaleY = 1000, int motionAmount = 1000) {

		if (!Renderer.TryGetSprite(spriteID, out var sprite)) return;

		int rot = 0;

		// Motion
		if (motionAmount != 0) {
			// Idle Rot
			int animationFrame = (character.TypeID + Game.GlobalFrame).Abs(); // ※ Intended ※
			rot += rotation.Sign() * (animationFrame.PingPong(180) / 10 - 9);
			// Delta Y >> Rot
			int deltaY = character.DeltaPositionY;
			rot -= rotation.Sign() * (deltaY * 2 / 3).Clamp(-20, 20);
		}

		// Draw
		Renderer.Draw(
			sprite,
			globalX, globalY,
			sprite.PivotX, sprite.PivotY, rotation + rot,
			sprite.GlobalWidth * scaleX / 1000,
			sprite.GlobalHeight * scaleY / 1000,
			z
		);

	}

}