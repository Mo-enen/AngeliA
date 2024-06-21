using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public sealed class ModularTail : Tail, IModularBodyGadget { }


public abstract class Tail : BodyGadget {


	// Data
	protected sealed override BodyGadgetType GadgetType => BodyGadgetType.Tail;
	public override bool SpriteLoaded => SpriteGroupID != 0;
	protected int SpriteGroupID { get; private set; }
	protected virtual int LimbGrow => 1000;
	protected virtual int AngleAmountRoot => 1000;
	protected virtual int AngleAmountSubsequent => 1000;
	protected virtual int AngleOffset => 0;
	protected virtual int Frequency => 113;
	protected virtual int FrequencyAlt => 277;
	protected virtual int FrameLen => 219;
	protected virtual int FrameDelta => 37;
	protected virtual int OffsetX => 0;
	protected virtual int OffsetY => 0;


	public override bool FillFromSheet (string name) {
		SpriteGroupID = $"{name}.Tail".AngeHash();
		if (!Renderer.HasSpriteGroup(SpriteGroupID)) SpriteGroupID = 0;
		return SpriteLoaded;
	}


	public static void DrawGadgetFromPool (PoseCharacter character) {
		if (character.TailID != 0 && TryGetGadget(character.TailID, out var tail)) {
			tail.DrawGadget(character);
		}
	}


	public override void DrawGadget (PoseCharacter character) {
		if (!SpriteLoaded) return;
		if (
			character.AnimationType == CharacterAnimationType.Fly &&
			character.WingID != 0 &&
			Wing.IsPropellerWing(character.WingID)
		) return;
		DrawSpriteAsTail(
			character, SpriteGroupID, Frequency, FrequencyAlt, FrameLen, FrameDelta,
			AngleAmountRoot, AngleAmountSubsequent, AngleOffset, LimbGrow, OffsetX, OffsetY
		);
	}


	public static void DrawSpriteAsTail (
		PoseCharacter character, int spriteGroupID,
		int frequency, int frequencyAlt, int frameLen, int frameDelta,
		int angleAmountRoot, int angleAmountSubsequent, int angleOffset, int limbGrow,
		int offsetX, int offsetY
	) {

		if (spriteGroupID == 0 || !Renderer.HasSpriteGroup(spriteGroupID, out int count)) return;
		if (frequency <= 0) frequency = 1;
		if (frequencyAlt <= 0) frequencyAlt = 1;
		if (frameLen <= 0) frameLen = 1;

		int z = character.Body.FrontSide ? -33 : 33;
		int facingSign = character.FacingRight || character.AnimationType == CharacterAnimationType.Climb ? 1 : -1;
		int prevX = 0;
		int prevY = 0;
		int prevW = 0;
		int prevH = 0;
		int prevR = 0;
		int x = 0;
		int y = 0;
		int w = 0;
		int h = 0;
		int r = 0;
		int px = 0;
		int py = 0;
		int animationFrame = (character.TypeID + Game.GlobalFrame).Abs(); // ※ Intended ※
		for (int i = 0; i < count; i++) {

			if (!Renderer.TryGetSpriteFromGroup(spriteGroupID, i, out var sprite, false, true)) break;
			w = sprite.GlobalWidth;
			h = sprite.GlobalHeight;
			px = 0;

			var animatedPoseType = character.AnimationType;

			if (animatedPoseType == CharacterAnimationType.Fly) {
				// Flying
				h = sprite.GlobalHeight * 2 / 3;
				if (i == 0) {
					x = (character.UpperLegL.GlobalX + character.UpperLegR.GlobalX) / 2;
					y = (character.UpperLegL.GlobalY + character.UpperLegR.GlobalY) / 2;
					Util.LimbRotate(
						ref x, ref y, ref px, ref py, ref r, ref w, ref h,
						facingSign * 60, false, limbGrow
					);
				} else {
					Util.LimbRotate(
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
					if (
						animatedPoseType == CharacterAnimationType.Sleep ||
						animatedPoseType == CharacterAnimationType.PassOut
					) {
						x = (character.UpperLegL.GlobalX + character.UpperLegR.GlobalX) / 2;
						y = (character.UpperLegL.GlobalY + character.UpperLegR.GlobalY) / 2;
					} else {
						x = character.Body.GlobalX - w / 2;
						y = character.Hip.GlobalY + character.Hip.Height / 2;
					}
					int MIN_ANGLE = character.Body.Height > 0 ? 123 : 63;
					int MAX_ANGLE = character.Body.Height > 0 ? 142 : 82;
					MIN_ANGLE = MIN_ANGLE * angleAmountRoot / 1000;
					MAX_ANGLE = MAX_ANGLE * angleAmountRoot / 1000;
					ANGLE_DELTA = ANGLE_DELTA * angleAmountRoot / 1000;
					int angle = (Util.Remap(0, count - 1, MIN_ANGLE, MAX_ANGLE, i) + angleOffset) * facingSign;
					int targetRot = (int)Util.LerpUnclamped(
						-angle + ANGLE_DELTA * facingSign,
						angle + ANGLE_DELTA * facingSign,
						Ease.InOutQuart(
							(animationFrame - frequency + i * frameDelta).PingPong(frameLen).Clamp(0, frequency) / (float)frequency
						)
					);
					Util.LimbRotate(
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
					Util.LimbRotate(
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


}
