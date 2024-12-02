namespace AngeliA;


public sealed class DefaultHipSuit : HipCloth {
	public static readonly int TYPE_ID = typeof(DefaultHipSuit).AngeHash();
	public DefaultHipSuit () => FillFromSheet(GetType().AngeName());
}


public sealed class ModularHipSuit : HipCloth, IModularCloth { }


public abstract class HipCloth : Cloth {

	// SUB
	public enum HipClothType { None, Pants, Skirt, Dress, }

	// VAR
	protected sealed override ClothType ClothType => ClothType.Hip;
	public override bool SpriteLoaded => SpriteHip.IsValid;
	protected virtual bool CoverLegs => true;

	private HipClothType HipType = HipClothType.None;
	private ClothSprite SpriteHip;
	private ClothSprite SpriteTail;
	private ClothSprite SpriteUpperLeg;
	private ClothSprite SpriteLowerLeg;

	// API
	public override bool FillFromSheet (string name) {
		base.FillFromSheet(name);
		SpriteHip = new ClothSprite(name, "HipSuit", "SkirtSuit", "DressSuit");
		SpriteTail = new ClothSprite(name, "TailSuit");
		SpriteUpperLeg = new ClothSprite(name, "UpperLegSuit");
		SpriteLowerLeg = new ClothSprite(name, "LowerLegSuit");
		HipType = SpriteHip.SuitName switch {
			"HipSuit" => HipClothType.Pants,
			"SkirtSuit" => HipClothType.Skirt,
			"DressSuit" => HipClothType.Dress,
			_ => HipClothType.None,
		};
		return SpriteLoaded;
	}

	public static void DrawClothFromPool (PoseCharacterRenderer rendering) {
		if (rendering.SuitHip != 0 && Pool.TryGetValue(rendering.SuitHip, out var cloth)) {
			cloth.DrawCloth(rendering);
		}
	}

	public override void DrawCloth (PoseCharacterRenderer rendering) {
		if (!SpriteLoaded) return;
		using var _ = new SheetIndexScope(SheetIndex);
		switch (HipType) {
			case HipClothType.Pants:
				DrawClothAsPants(rendering, SpriteHip, CoverLegs ? 4 : 1);
				break;
			case HipClothType.Skirt:
				DrawClothAsSkirt(rendering, SpriteHip, CoverLegs ? 6 : 1);
				break;
			case HipClothType.Dress:
				DrawClothAsDress(rendering, SpriteHip.GroupID, CoverLegs ? 6 : 1);
				break;
		}
		DrawClothForUpperLeg(rendering, SpriteUpperLeg);
		DrawClothForLowerLeg(rendering, SpriteLowerLeg);
		DrawDoubleClothTailsOnHip(rendering, SpriteTail);
	}

	public static void DrawClothAsPants (PoseCharacterRenderer rendering, ClothSprite clothSprite, int localZ = 1) {

		var hip = rendering.Hip;
		if (!clothSprite.IsValid || hip.IsFullCovered) return;
		if (!clothSprite.TryGetSprite(hip.FrontSide, hip.Width > 0, out var sprite)) return;

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
		hip.Covered = sprite.Tag.HasAll(Tag.HideLimb) ?
			 BodyPart.CoverMode.FullCovered : BodyPart.CoverMode.Covered;

		// Draw
		Renderer.Draw(sprite, rect, hip.Z + localZ);

	}

	public static void DrawClothAsSkirt (PoseCharacterRenderer rendering, ClothSprite clothSprite, int localZ = 6) {

		var hip = rendering.Hip;
		if (!clothSprite.IsValid || hip.IsFullCovered) return;
		if (!clothSprite.TryGetSprite(hip.FrontSide, hip.Width > 0, out var sprite)) return;

		var body = rendering.Body;
		var upperLegL = rendering.UpperLegL;
		var upperLegR = rendering.UpperLegR;
		var animatedPoseType = rendering.TargetCharacter.AnimationType;
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
		int height = body.Height > 0 ? sprite.GlobalHeight : -sprite.GlobalHeight;
		if (animatedPoseType.IsLyingDown() && body.Height > 0) {
			height = height.LessOrEquel(centerY + offsetY - rendering.TargetCharacter.Y);
		}
		Renderer.Draw(
			sprite,
			centerX,
			body.Height > 0 ? centerY + offsetY : centerY - offsetY,
			500, 1000, 0,
			width, height,
			hip.Z + localZ
		);

		// Limb
		hip.Covered = sprite.Tag.HasAll(Tag.HideLimb) ?
			BodyPart.CoverMode.FullCovered : BodyPart.CoverMode.Covered;

		// Func
		static int Stretch (int rotL, int rotR) {
			int result = 0;
			if (rotL > 0) result += rotL / 2;
			if (rotR < 0) result += rotR / -2;
			return result;
		}
	}

	public static void DrawClothAsDress (PoseCharacterRenderer rendering, int dressGroupID, int localZ = 6, int motionAmount = 1000) {

		if (dressGroupID == 0 || !Renderer.TryGetSpriteGroup(dressGroupID, out var group)) return;

		// Render
		var hip = rendering.Hip;
		int z = hip.Z + localZ;
		int deltaX = rendering.TargetCharacter.DeltaPositionX;
		int deltaY = rendering.TargetCharacter.DeltaPositionY;
		for (int i = 0; i < group.Count; i++) {

			// Motion
			int offsetX = 0;
			int offsetY = 0;
			if (motionAmount != 0) {


			}

			// Draw Segment





		}

	}

	// Leg
	public static void DrawClothForUpperLeg (PoseCharacterRenderer rendering, ClothSprite clothSprite, int localZ = 1) {
		if (!clothSprite.IsValid) return;
		CoverClothOn(rendering.UpperLegL, clothSprite.GetSpriteID(rendering.UpperLegL.FrontSide, rendering.UpperLegL.Width > 0), localZ);
		CoverClothOn(rendering.UpperLegR, clothSprite.GetSpriteID(rendering.UpperLegR.FrontSide, rendering.UpperLegR.Width > 0), localZ);
	}

	public static void DrawClothForLowerLeg (PoseCharacterRenderer rendering, ClothSprite clothSprite, int localZ = 1) {
		if (!clothSprite.IsValid) return;
		CoverClothOn(rendering.LowerLegL, clothSprite.GetSpriteID(rendering.LowerLegL.FrontSide, rendering.LowerLegL.Width > 0), localZ);
		CoverClothOn(rendering.LowerLegR, clothSprite.GetSpriteID(rendering.LowerLegR.FrontSide, rendering.LowerLegR.Width > 0), localZ);
	}

	// Cloth Tail
	public static void DrawDoubleClothTailsOnHip (PoseCharacterRenderer rendering, ClothSprite clothSprite, bool drawOnAllPose = false) {

		var animatedPoseType = rendering.TargetCharacter.AnimationType;
		var hip = rendering.Hip;
		var body = rendering.Body;
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

		DrawSingleClothTail(rendering, clothSprite.GetSpriteID(hip.FrontSide, false), hipRect.x + 16, hipRect.y, z, rotL, scaleX, scaleY);
		DrawSingleClothTail(rendering, clothSprite.GetSpriteID(hip.FrontSide, true), hipRect.xMax - 16, hipRect.y, z, rotR, scaleX, scaleY);

	}

	public static void DrawSingleClothTail (PoseCharacterRenderer rendering, int spriteID, int globalX, int globalY, int z, int rotation, int scaleX = 1000, int scaleY = 1000, int motionAmount = 1000) {

		if (!Renderer.TryGetSprite(spriteID, out var sprite)) return;

		int rot = 0;

		// Motion
		if (motionAmount != 0) {
			// Idle Rot
			int animationFrame = (rendering.TargetCharacter.TypeID + Game.GlobalFrame).Abs(); // ※ Intended ※
			rot += rotation.Sign() * (animationFrame.PingPong(180) / 10 - 9);
			// Delta Y >> Rot
			int deltaY = rendering.TargetCharacter.DeltaPositionY;
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