using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


/// <summary>
/// Tail body gadget for pose style character
/// </summary>
public abstract class Tail : BodyGadget {





	#region --- VAR ---


	// Api
	public sealed override BodyGadgetType GadgetType => BodyGadgetType.Tail;
	public override bool SpriteLoaded => SpriteTail.IsValid;
	/// <summary>
	/// Size-growing amount for limb rotation (0 means 0%, 1000 means 100%)
	/// </summary>
	protected virtual int LimbGrow => 1000;
	/// <summary>
	/// Total rotating angle amount for limb rotation (0 means 0%, 1000 means 100%)
	/// </summary>
	protected virtual int AngleAmountRoot => 1000;
	/// <summary>
	/// Subsequent sugment's rotating angle amount for limb rotation (0 means 0%, 1000 means 100%)
	/// </summary>
	protected virtual int AngleAmountSubsequent => 1000;
	/// <summary>
	/// Total offset angle for the root sugment
	/// </summary>
	protected virtual int AngleOffset => 0;
	/// <summary>
	/// Motion frequency for the limb rotation, smaller means faster
	/// </summary>
	protected virtual int Frequency => 113;
	/// <summary>
	/// Motion frequency for the less influential part limb rotation, smaller means faster
	/// </summary>
	protected virtual int FrequencyAlt => 277;
	/// <summary>
	/// Looping duration for the limb rotation
	/// </summary>
	protected virtual int FrameLen => 219;
	/// <summary>
	/// Looping duration for the less influential part limb rotation
	/// </summary>
	protected virtual int FrameDelta => 37;
	/// <summary>
	/// Total position offset X
	/// </summary>
	protected virtual int OffsetX => 0;
	/// <summary>
	/// Total position offset Y
	/// </summary>
	protected virtual int OffsetY => 0;
	private OrientedSprite SpriteTail;


	#endregion




	#region --- MSG ---


	public override void DrawGadget (PoseCharacterRenderer renderer) {

		if (!SpriteLoaded) return;
		if (
			renderer.TargetCharacter.AnimationType == CharacterAnimationType.Fly &&
			renderer.WingID != 0 &&
			Wing.IsPropellerWing(renderer.WingID)
		) return;

		using var __ = new SheetIndexScope(SheetIndex);
		var animatedPoseType = renderer.TargetCharacter.AnimationType;
		bool flying = animatedPoseType == CharacterAnimationType.Fly;
		bool lyingDown = animatedPoseType.IsLyingDown();
		int x, y;
		if (flying) {
			x = (renderer.UpperLegL.GlobalX + renderer.UpperLegR.GlobalX) / 2;
			y = (renderer.UpperLegL.GlobalY + renderer.UpperLegR.GlobalY) / 2;
		} else if (lyingDown) {
			x = (renderer.UpperLegL.GlobalX + renderer.UpperLegR.GlobalX) / 2;
			y = (renderer.UpperLegL.GlobalY + renderer.UpperLegR.GlobalY) / 2;
		} else {
			x = renderer.Body.GlobalX;
			y = renderer.Hip.GlobalY + renderer.Hip.Height / 2;
		}
		bool front = renderer.Body.FrontSide;
		bool right = renderer.Body.Width > 0;
		bool up = renderer.Body.Height > 0;
		if (SpriteTail.TryGetSpriteGroup(front, right, out _)) {
			DrawSpriteAsWhipTail(
				SpriteTail, x, y, front, right, up,
				Frequency, FrequencyAlt, FrameLen, FrameDelta,
				AngleAmountRoot, AngleAmountSubsequent, AngleOffset, LimbGrow, OffsetX, OffsetY,
				flying, frameOffset: renderer.TargetCharacter.TypeID // ※ Intended ※
			);
		} else {
			DrawSpriteAsSimpleTail(SpriteTail, x, y, front, right, up);
		}

	}


	public override void DrawGadgetGizmos (IRect rect, Color32 tint, int z) {
		using var _ = new DynamicClampCellScope(rect);
		DrawSpriteAsWhipTail(SpriteTail, rect.CenterX(), rect.y, true, true, true, z: z);
	}


	#endregion




	#region --- API ---


	public override bool FillFromSheet (string name) {
		base.FillFromSheet(name);
		SpriteTail = new OrientedSprite(name, "Tail");
		return SpriteLoaded;
	}


	public static void DrawGadgetFromPool (PoseCharacterRenderer renderer) {
		if (renderer.TailID != 0 && TryGetGadget(renderer.TailID, out var tail)) {
			tail.DrawGadget(renderer);
		}
	}


	/// <summary>
	/// Draw given artwork sprite as tail at given position
	/// </summary>
	/// <param name="oSprite">Artwork sprite</param>
	/// <param name="x">Position X in global space</param>
	/// <param name="y">Position Y in global space</param>
	/// <param name="facingFront">Is character facing front</param>
	/// <param name="facingRight">Is character facing right</param>
	/// <param name="facingUp">Is character facing up</param>
	/// <param name="frequency">Motion frequency for the limb rotation, smaller means faster</param>
	/// <param name="frequencyAlt">Motion frequency for the less influential part limb rotation, smaller means faster</param>
	/// <param name="frameLen">Looping duration for the limb rotation</param>
	/// <param name="frameDelta">Looping duration for the less influential part limb rotation</param>
	/// <param name="angleAmountRoot">Total rotating angle amount for limb rotation (0 means 0%, 1000 means 100%)</param>
	/// <param name="angleAmountSubsequent">Subsequent sugment's rotating angle amount for limb rotation (0 means 0%, 1000 means 100%)</param>
	/// <param name="angleOffset">Total offset angle for the root sugment</param>
	/// <param name="limbGrow">Size-growing amount for limb rotation (0 means 0%, 1000 means 100%)</param>
	/// <param name="offsetX">Total position offset X</param>
	/// <param name="offsetY">Total position offset Y</param>
	/// <param name="isFlying">Is character flying</param>
	/// <param name="frameOffset">Current animation frame</param>
	/// <param name="z">Z value for sorting rendering cell</param>
	public static void DrawSpriteAsWhipTail (
		OrientedSprite oSprite, int x, int y, bool facingFront, bool facingRight, bool facingUp,
		int frequency = 113, int frequencyAlt = 277, int frameLen = 219, int frameDelta = 37,
		int angleAmountRoot = 1000, int angleAmountSubsequent = 1000, int angleOffset = 0, int limbGrow = 1000,
		int offsetX = 0, int offsetY = 0,
		bool isFlying = false, int frameOffset = 0, int z = int.MinValue
	) {

		if (!oSprite.IsValid) return;
		if (frequency <= 0) frequency = 1;
		if (frequencyAlt <= 0) frequencyAlt = 1;
		if (frameLen <= 0) frameLen = 1;
		if (
			!oSprite.TryGetSpriteGroup(facingFront, facingRight, out var group) ||
			group.Count == 0
		) return;

		int count = group.Count;
		z = z == int.MinValue ? (facingFront ? -33 : 33) : z;
		int facingSign = facingRight || !facingFront ? 1 : -1;
		int prevX = 0;
		int prevY = 0;
		int prevW = 0;
		int prevH = 0;
		int prevR = 0;
		int w = 0;
		int h = 0;
		int r = 0;
		int px = 0;
		int py = 0;
		int animationFrame = (frameOffset + Game.GlobalFrame).Abs();
		for (int i = 0; i < count; i++) {

			var sprite = group.Sprites[i];
			w = sprite.GlobalWidth;
			h = sprite.GlobalHeight;
			px = 0;

			if (isFlying) {
				// Flying
				h = sprite.GlobalHeight * 2 / 3;
				if (i == 0) {
					FrameworkUtil.LimbRotate(
						ref x, ref y, ref px, ref py, ref r, ref w, ref h,
						facingSign * 60, false, limbGrow
					);
				} else {
					FrameworkUtil.LimbRotate(
						ref x, ref y, ref px, ref py, ref r, ref w, ref h,
						prevX, prevY, prevR, prevW, prevH,
						facingSign * -5, false, limbGrow
					);
				}
			} else {
				int ANGLE_DELTA = (int)Util.LerpUnclamped(
					-42, -3, Game.GlobalFrame.PingPong(frequencyAlt) / (float)frequencyAlt
				);
				if (i == 0) {
					// First
					int minAngle = facingUp ? 123 : 63;
					int maxAngle = facingUp ? 142 : 82;
					minAngle = minAngle * angleAmountRoot / 1000;
					maxAngle = maxAngle * angleAmountRoot / 1000;
					ANGLE_DELTA = ANGLE_DELTA * angleAmountRoot / 1000;
					int angle = (Util.Remap(0, count - 1, minAngle, maxAngle, i) + angleOffset) * facingSign;
					int targetRot = (int)Util.LerpUnclamped(
						-angle + ANGLE_DELTA * facingSign,
						angle + ANGLE_DELTA * facingSign,
						Ease.InOutQuart(
							(animationFrame - frequency + i * frameDelta).PingPong(frameLen).Clamp(0, frequency) / (float)frequency
						)
					);
					FrameworkUtil.LimbRotate(
						ref x, ref y, ref px, ref py, ref r, ref w, ref h,
						targetRot, false, limbGrow
					);
				} else {
					// Subsequent
					int MIN_ANGLE = 43 * angleAmountSubsequent / 1000;
					int MAX_ANGLE = 62 * angleAmountSubsequent / 1000;
					ANGLE_DELTA = ANGLE_DELTA * angleAmountSubsequent / 1000;
					int angle = Util.Remap(0, count - 1, MIN_ANGLE, MAX_ANGLE, i) * facingSign;
					int targetRot = (int)Util.LerpUnclamped(
						-angle + ANGLE_DELTA * facingSign,
						angle + ANGLE_DELTA * facingSign,
						Ease.InOutQuart(
							(animationFrame - frequency + i * frameDelta).PingPong(frameLen).Clamp(0, frequency) / (float)frequency
						)
					);
					FrameworkUtil.LimbRotate(
						ref x, ref y, ref px, ref py, ref r, ref w, ref h,
						prevX, prevY, prevR, prevW, prevH,
						targetRot, false, limbGrow
					);
				}
			}

			// Draw
			if (sprite.GlobalBorder.IsZero) {
				Renderer.Draw(sprite, x + offsetX, y + offsetY, px, py, r, w, h, z);
			} else {
				Renderer.DrawSlice(sprite, x + offsetX, y + offsetY, px, py, r, w, h, z);
			}

			// to Next
			prevX = x;
			prevY = y;
			prevW = w;
			prevH = h;
			prevR = r;
		}

	}


	/// <summary>
	/// Draw given artwork sprite as tail with only one segment
	/// </summary>
	/// <param name="oSprite">Artwork sprite</param>
	/// <param name="x">Position X in global space</param>
	/// <param name="y">Position Y in global space</param>
	/// <param name="facingFront">Is character facing front</param>
	/// <param name="facingRight">Is character facing right</param>
	/// <param name="facingUp">Is character facing up</param>
	/// <param name="z">Z value for sorting rendering cell</param>
	public static void DrawSpriteAsSimpleTail (OrientedSprite oSprite, int x, int y, bool facingFront, bool facingRight, bool facingUp, int z = int.MinValue) {
		if (!oSprite.IsValid) return;
		if (!oSprite.TryGetSprite(facingFront, facingRight, Game.GlobalFrame, out var sprite)) return;
		Renderer.Draw(
			sprite, x, y,
			sprite.PivotX, sprite.PivotY, 0,
			sprite.GlobalWidth, facingUp ? sprite.GlobalHeight : -sprite.GlobalHeight,
			z == int.MinValue ? (facingFront ? -33 : 33) : z
		);
	}


	#endregion




}
