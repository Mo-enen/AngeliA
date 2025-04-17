namespace AngeliA;


/// <summary>
/// Pants or skirt for a pose-style character. Include hip and legs (no foot)
/// </summary>
public abstract class HipCloth : Cloth {

	// SUB
	public enum HipClothType { None, Pants, Skirt, }

	// VAR
	public sealed override ClothType ClothType => ClothType.Hip;
	public override bool SpriteLoaded => SpriteHip.IsValid;
	/// <summary>
	/// True if the pants renders on front of lengs
	/// </summary>
	protected virtual bool CoverLegs => true;

	private HipClothType HipType = HipClothType.None;
	private OrientedSprite SpriteHip;
	private OrientedSprite SpriteClothTail;
	private OrientedSprite SpriteUpperLegLeft;
	private OrientedSprite SpriteUpperLegRight;
	private OrientedSprite SpriteLowerLegLeft;
	private OrientedSprite SpriteLowerLegRight;

	// API
	public override bool FillFromSheet (string name) {
		base.FillFromSheet(name);
		SpriteHip = new OrientedSprite(name, "HipSuit", "SkirtSuit");
		SpriteClothTail = new OrientedSprite(name, "TailSuit");
		SpriteUpperLegLeft = new OrientedSprite(name, "UpperLegSuitLeft", "UpperLegSuit");
		SpriteUpperLegRight = new OrientedSprite(name, "UpperLegSuitRight", "UpperLegSuit");
		SpriteLowerLegLeft = new OrientedSprite(name, "LowerLegSuitLeft", "LowerLegSuit");
		SpriteLowerLegRight = new OrientedSprite(name, "LowerLegSuitRight", "LowerLegSuit");
		HipType = SpriteHip.AttachmentName switch {
			"HipSuit" => HipClothType.Pants,
			"SkirtSuit" => HipClothType.Skirt,
			_ => HipClothType.None,
		};
		return SpriteLoaded;
	}

	/// <summary>
	/// Draw pants/skirt for given character from system pool
	/// </summary>
	public static void DrawClothFromPool (PoseCharacterRenderer rendering) {
		if (rendering.SuitHip != 0 && TryGetCloth(rendering.SuitHip, out var cloth)) {
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
		}
		DrawClothForUpperLeg(rendering, SpriteUpperLegLeft, SpriteUpperLegRight);
		DrawClothForLowerLeg(rendering, SpriteLowerLegLeft, SpriteLowerLegRight);
		DrawDoubleClothTailsOnHip(rendering, SpriteClothTail);
	}

	public override void DrawClothGizmos (IRect rect, Color32 tint, int z) {

		int legHeight = rect.height / 4;
		int limbSize = rect.width / 8;
		int limbShift = -limbSize / 8;

		// Hip
		IRect hipRect;
		switch (HipType) {
			default:
			case HipClothType.Pants:
				hipRect = rect.CornerInside(Alignment.TopMid, rect.width - limbSize * 2, rect.height / 3);
				legHeight = rect.height / 3;
				if (!SpriteHip.TryGetSpriteForGizmos(out var hipSP)) break;
				hipRect.x = hipRect.CenterX() - limbSize * 3 / 2;
				hipRect.width = limbSize * 3;
				Renderer.Draw(hipSP, hipRect, tint, z);
				break;
			case HipClothType.Skirt:
				hipRect = rect.CornerInside(Alignment.TopMid, rect.width - limbSize * 2, rect.height / 2);
				if (!SpriteHip.TryGetSpriteGroup(true, true, out var group)) break;
				int maxWidth = 0;
				for (int i = 0; i < group.Count.LessOrEquel(3); i++) {
					maxWidth = Util.Max(maxWidth, group.Sprites[i].GlobalWidth);
				}
				for (int i = 0; i < group.Count.LessOrEquel(3); i++) {
					var sp = group.Sprites[i];
					var _rect = hipRect.PartVertical(2 - i, 3);
					int x = _rect.CenterX();
					_rect.width = _rect.width * sp.GlobalWidth / maxWidth;
					Renderer.Draw(
						sp,
						x, _rect.y, 500, 0, 0, _rect.width, _rect.height,
						tint, z
					);
				}
				break;
		}

		// Upper Leg
		var uLegRectL = hipRect.CornerOutside(Alignment.BottomMid, limbSize, legHeight);
		var uLegRectR = hipRect.CornerOutside(Alignment.BottomMid, limbSize, legHeight);
		uLegRectL.x -= limbSize + limbShift;
		uLegRectR.x += limbSize + limbShift;
		if (SpriteUpperLegLeft.TryGetSpriteForGizmos(out var uLegSpL)) {
			Renderer.Draw(uLegSpL, uLegRectL, tint, z);
		}
		if (SpriteUpperLegRight.TryGetSpriteForGizmos(out var uLegSpR)) {
			Renderer.Draw(uLegSpR, uLegRectR.GetFlipHorizontal(), tint, z);
		}

		// Lower Arm
		var lLegRectL = uLegRectL.EdgeOutsideDown(legHeight);
		var lLegRectR = uLegRectR.EdgeOutsideDown(legHeight);
		lLegRectL.x -= limbShift * 2;
		lLegRectR.x += limbShift * 2;
		if (SpriteLowerLegLeft.TryGetSpriteForGizmos(out var lLegSpL)) {
			Renderer.Draw(lLegSpL, lLegRectL, tint, z);
		}
		if (SpriteLowerLegRight.TryGetSpriteForGizmos(out var lLegSpR)) {
			Renderer.Draw(lLegSpR, lLegRectR.GetFlipHorizontal(), tint, z);
		}

	}

	/// <summary>
	/// Draw artwork sprite as pants for given character
	/// </summary>
	/// <param name="rendering">Target character</param>
	/// <param name="clothSprite">Artwork sprite</param>
	/// <param name="localZ">Z value for sort rendering cells</param>
	public static void DrawClothAsPants (PoseCharacterRenderer rendering, OrientedSprite clothSprite, int localZ = 1) {

		var hip = rendering.Hip;
		if (!clothSprite.IsValid || hip.IsFullCovered) return;
		if (!clothSprite.TryGetSprite(hip.FrontSide, hip.Width > 0, rendering.CurrentAnimationFrame, out var sprite)) return;

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

	/// <summary>
	/// Draw artwork sprite as skirt for given character
	/// </summary>
	/// <param name="rendering">Target character</param>
	/// <param name="clothSprite">Artwork sprite (should be a sprite group)</param>
	/// <param name="localZ">Z value for sort rendering cells</param>
	/// <param name="motionAmount">How much does the skirt flow with character movement (0 means 0%, 1000 means 100%)</param>
	public static void DrawClothAsSkirt (PoseCharacterRenderer rendering, OrientedSprite clothSprite, int localZ = 6, int motionAmount = 1000) {

		var hip = rendering.Hip;
		if (!clothSprite.IsValid || hip.IsFullCovered) return;
		SpriteGroup group = null;
		if (!clothSprite.TryGetSpriteWithoutAnimation(hip.FrontSide, hip.Width > 0, out var sprite)) {
			if (clothSprite.TryGetSpriteGroup(hip.FrontSide, hip.Width > 0, out group) && group.Count > 0) {
				sprite = group.Sprites[0];
			} else {
				return;
			}
		}

		var body = rendering.Body;
		var upperLegL = rendering.UpperLegL;
		var upperLegR = rendering.UpperLegR;
		var animatedPoseType = rendering.TargetCharacter.AnimationType;
		const int A2G = 16;

		int bodyWidthAbs = body.Width.Abs();
		var legTopL = upperLegL.GlobalLerp(0.5f, 1f);
		var legTopR = upperLegR.GlobalLerp(0.5f, 1f);
		int left = legTopL.x - upperLegL.SizeX / 2;
		int right = legTopR.x + upperLegR.SizeX / 2;
		int centerX = (left + right) / 2;
		int centerY = (legTopL.y + legTopR.y) / 2;
		int width = Util.Max(
			(right - left).Abs(), bodyWidthAbs - body.Border.left - body.Border.right
		);
		width += sprite.GlobalBorder.horizontal;

		// Stretch
		if (animatedPoseType != CharacterAnimationType.GrabSide &&
			animatedPoseType != CharacterAnimationType.Dash &&
			animatedPoseType != CharacterAnimationType.Idle
		) {
			int stretch = 0;
			if (upperLegL.Rotation > 0) stretch += upperLegL.Rotation / 2;
			if (upperLegR.Rotation < 0) stretch += upperLegR.Rotation / -2;
			width += stretch;
		}
		width += animatedPoseType switch {
			CharacterAnimationType.JumpUp or CharacterAnimationType.JumpDown => A2G,
			CharacterAnimationType.Run => A2G / 2,
			CharacterAnimationType.Pound => A2G,
			_ => 0,
		};

		// Shift Y
		int offsetY = sprite.GlobalHeight * (1000 - sprite.PivotY) / 1000;
		if (animatedPoseType == CharacterAnimationType.Dash) offsetY += A2G;
		int height = body.Height > 0 ? sprite.GlobalHeight : -sprite.GlobalHeight;
		if (animatedPoseType.IsLyingDown() && body.Height > 0) {
			height = height.LessOrEquel(centerY + offsetY - rendering.TargetCharacter.Y);
		}
		if (body.Height < 0) offsetY = -offsetY;

		int rot = body.Rotation ;

		if (group != null) {
			// Dress Group

			// Render
			int deltaX = rendering.TargetCharacter.DeltaPositionX.Clamp(-64, 64);
			int deltaY = rendering.TargetCharacter.DeltaPositionY.Clamp(-42, 42);
			int signY = hip.Height.Sign3();
			int currentY = centerY + offsetY;
			for (int i = 0; i < group.Count; i++) {

				if (group.Sprites[i] is not AngeSprite segSprite) continue;

				// Motion
				int motionX = 0;
				int motionY = 0;
				if (motionAmount != 0) {
					float lerp01 = i / (group.Count - 1f);
					motionX = (-deltaX * motionAmount * lerp01 / 1000f).RoundToInt();
					motionY = (-deltaY * motionAmount * lerp01 / 2000f).RoundToInt();
				}
				if (
					animatedPoseType == CharacterAnimationType.Dash ||
					animatedPoseType == CharacterAnimationType.Rush
				) {
					motionX = motionX * 3 / 2;
				}

				// Draw Segment
				int deltaW = width.Sign() * (width.Abs() - sprite.GlobalWidth);
				int deltaH = height.Sign() * (height.Abs() - sprite.GlobalHeight);
				int segWidth = segSprite.GlobalWidth + deltaW + motionX.Abs();
				int segHeight = signY * (segSprite.GlobalHeight + deltaH - motionY);
				currentY -= segHeight;
				Renderer.Draw(
					segSprite,
					centerX + motionX / 2,
					currentY,
					500, 0, rot,
					segWidth, segHeight,
					z: hip.Z + localZ
				);
				rot = (int)Util.LerpUnclamped(rot, 0, 0.5f);

			}

		} else {
			// Single Skirt
			Renderer.Draw(
				sprite,
				centerX,
				centerY + offsetY,
				500, 1000, rot,
				width, height,
				hip.Z + localZ
			);
		}

		// Limb
		hip.Covered = sprite.Tag.HasAll(Tag.HideLimb) ? BodyPart.CoverMode.FullCovered : BodyPart.CoverMode.Covered;

	}

	// Leg
	/// <summary>
	/// Draw artwork sprite as cloth only for upper-leg
	/// </summary>
	/// <param name="rendering">Target character</param>
	/// <param name="spriteLeft">Artwork sprite for left leg</param>
	/// <param name="spriteRight">Artwork sprite for right leg</param>
	/// <param name="localZ">Z value for sort rendering cells</param>
	public static void DrawClothForUpperLeg (PoseCharacterRenderer rendering, OrientedSprite spriteLeft, OrientedSprite spriteRight, int localZ = 1) {
		bool facingRight = rendering.Body.Width > 0;
		if (spriteLeft.IsValid) {
			spriteLeft.TryGetSprite(rendering.UpperLegL.FrontSide, facingRight, rendering.CurrentAnimationFrame, out var sprite);
			CoverClothOn(rendering.UpperLegL, sprite, localZ);
		}
		if (spriteRight.IsValid) {
			spriteRight.TryGetSprite(rendering.UpperLegR.FrontSide, facingRight, rendering.CurrentAnimationFrame, out var sprite);
			CoverClothOn(rendering.UpperLegR, sprite, localZ);
		}
	}

	/// <summary>
	/// Draw artwork sprite as cloth only for lower-leg
	/// </summary>
	/// <param name="rendering">Target character</param>
	/// <param name="spriteLeft">Artwork sprite for left leg</param>
	/// <param name="spriteRight">Artwork sprite for right leg</param>
	/// <param name="localZ">Z value for sort rendering cells</param>
	public static void DrawClothForLowerLeg (PoseCharacterRenderer rendering, OrientedSprite spriteLeft, OrientedSprite spriteRight, int localZ = 1) {
		bool facingRight = rendering.Body.Width > 0;
		if (spriteLeft.IsValid) {
			spriteLeft.TryGetSprite(rendering.LowerLegL.FrontSide, facingRight, rendering.CurrentAnimationFrame, out var sprite);
			CoverClothOn(rendering.LowerLegL, sprite, localZ);
		}
		if (spriteRight.IsValid) {
			spriteRight.TryGetSprite(rendering.LowerLegR.FrontSide, facingRight, rendering.CurrentAnimationFrame, out var sprite);
			CoverClothOn(rendering.LowerLegR, sprite, localZ);
		}
	}

	// Cloth Tail
	/// <summary>
	/// Draw two tails as cloth decoration (like Suisei's standard suit from Hololive)
	/// </summary>
	/// <param name="rendering">Target character</param>
	/// <param name="clothSprite">Artwork sprite</param>
	/// <param name="drawOnAllPose">Draw this tail even when character is Rolling, Sleeping, Passout and Flying</param>
	public static void DrawDoubleClothTailsOnHip (PoseCharacterRenderer rendering, OrientedSprite clothSprite, bool drawOnAllPose = false) {

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

		clothSprite.TryGetSprite(hip.FrontSide, false, rendering.CurrentAnimationFrame, out var spriteL);
		clothSprite.TryGetSprite(hip.FrontSide, true, rendering.CurrentAnimationFrame, out var spriteR);

		int deltaY = rendering.TargetCharacter.DeltaPositionY;
		DrawSingleClothTail(spriteL, hipRect.x + 16, hipRect.y, z, rotL, deltaY, scaleX, scaleY);
		DrawSingleClothTail(spriteR, hipRect.xMax - 16, hipRect.y, z, rotR, deltaY, scaleX, scaleY);

	}

	/// <summary>
	/// Draw a single tail as cloth decoration (like Suisei's standard suit from Hololive)
	/// </summary>
	/// <param name="sprite">Artwork sprite</param>
	/// <param name="globalX">Pivot position X of the tail in global space</param>
	/// <param name="globalY">Pivot position Y of the tail in global space</param>
	/// <param name="z">Z value for sorting rendering cells</param>
	/// <param name="rotation">Rotation of this tail</param>
	/// <param name="deltaY">Character's current movement speed Y</param>
	/// <param name="scaleX">Horizontal size scale (0 means 0%, 1000 means 100%)</param>
	/// <param name="scaleY">Vertical size scale (0 means 0%, 1000 means 100%)</param>
	/// <param name="motionAmount">How much flow motion should apply from characters movement (0 means 0%, 1000 means 100%)</param>
	public static void DrawSingleClothTail (AngeSprite sprite, int globalX, int globalY, int z, int rotation, int deltaY, int scaleX = 1000, int scaleY = 1000, int motionAmount = 1000) {

		if (sprite == null) return;

		int rot = 0;

		// Motion
		if (motionAmount != 0) {
			// Idle Rot
			int animationFrame = (sprite.ID * 62154 + Game.GlobalFrame).Abs(); // ※ Intended ※
			rot += rotation.Sign() * (animationFrame.PingPong(180) / 10 - 9);
			// Delta Y >> Rot
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