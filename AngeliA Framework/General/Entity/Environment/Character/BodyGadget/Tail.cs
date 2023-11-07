using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class AutoSpriteTail : Tail {
		protected int SpriteGroupID { get; init; }
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
		public AutoSpriteTail () {
			string name = (GetType().DeclaringType ?? GetType()).AngeName();
			SpriteGroupID = $"{name}.Tail".AngeHash();
			if (!CellRenderer.HasSpriteGroup(SpriteGroupID)) SpriteGroupID = 0;
		}
		protected override void DrawTail (Character character) {
			if (
				character.AnimatedPoseType == CharacterPoseAnimationType.Fly &&
				character.WingID != 0 &&
				Wing.IsPropellerWing(character.WingID)
			) return;
			DrawSegmentTail(
				character, SpriteGroupID, Frequency, FrequencyAlt, FrameLen, FrameDelta,
				AngleAmountRoot, AngleAmountSubsequent, AngleOffset, LimbGrow, OffsetX, OffsetY
			);
		}
	}


	public abstract class Tail : BodyGadget {


		// Data
		private static readonly Dictionary<int, Tail> Pool = new();
		private static readonly Dictionary<int, int> DefaultPool = new();
		protected override string BaseTypeName => nameof(Tail);


		// MSG
		[OnGameInitialize(-127)]
		public static void BeforeGameInitialize () {
			Pool.Clear();
			var charType = typeof(Character);
			foreach (var type in typeof(Tail).AllChildClass()) {
				if (System.Activator.CreateInstance(type) is not Tail tail) continue;
				int id = type.AngeHash();
				Pool.TryAdd(id, tail);
				// Default
				var dType = type.DeclaringType;
				if (dType != null && dType.IsSubclassOf(charType)) {
					DefaultPool.TryAdd(dType.AngeHash(), id);
				}
			}
		}


		// API
		public static void Draw (Character character) => Draw(character, out _);
		public static void Draw (Character character, out Tail tail) {
			tail = null;
			if (character.TailID != 0 &&
				character.ShowingTail &&
				Pool.TryGetValue(character.TailID, out tail)
			) {
				tail.DrawTail(character);
			}
		}


		public static bool TryGetDefaultTailID (int characterID, out int tailID) => DefaultPool.TryGetValue(characterID, out tailID);
		public static bool TryGetTail (int tailID, out Tail tail) => Pool.TryGetValue(tailID, out tail);

		protected abstract void DrawTail (Character character);


		// UTL
		public static void DrawSegmentTail (
			Character character, int spriteGroupID,
			int frequency, int frequencyAlt, int frameLen, int frameDelta,
			int angleAmountRoot, int angleAmountSubsequent, int angleOffset, int limbGrow,
			int offsetX, int offsetY
		) {

			if (spriteGroupID == 0 || !CellRenderer.HasSpriteGroup(spriteGroupID, out int count)) return;
			if (frequency <= 0) frequency = 1;
			if (frequencyAlt <= 0) frequencyAlt = 1;
			if (frameLen <= 0) frameLen = 1;

			int z = character.Body.FrontSide ? -33 : 33;
			int facingSign = character.FacingRight || character.AnimatedPoseType == CharacterPoseAnimationType.Climb ? 1 : -1;
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

				if (!CellRenderer.TryGetSpriteFromGroup(spriteGroupID, i, out var sprite, false, true)) break;
				w = sprite.GlobalWidth;
				h = sprite.GlobalHeight;
				px = 0;

				var animatedPoseType = character.AnimatedPoseType;

				if (animatedPoseType == CharacterPoseAnimationType.Fly) {
					// Flying
					h = sprite.GlobalHeight * 2 / 3;
					if (i == 0) {
						x = (character.UpperLegL.GlobalX + character.UpperLegR.GlobalX) / 2;
						y = (character.UpperLegL.GlobalY + character.UpperLegR.GlobalY) / 2;
						AngeUtil.LimbRotate(
							ref x, ref y, ref px, ref py, ref r, ref w, ref h,
							facingSign * 60, false, limbGrow
						);
					} else {
						AngeUtil.LimbRotate(
							ref x, ref y, ref px, ref py, ref r, ref w, ref h,
							prevX, prevY, prevR, prevW, prevH,
							facingSign * -5, false, limbGrow
						);
					}
				} else {
					int ANGLE_DELTA = (int)Mathf.LerpUnclamped(
						-42, -3, Game.GlobalFrame.PingPong(frequencyAlt) / (float)frequencyAlt
					);
					if (i == 0) {
						// First
						if (
							animatedPoseType == CharacterPoseAnimationType.Sleep ||
							animatedPoseType == CharacterPoseAnimationType.PassOut
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
						int targetRot = (int)Mathf.LerpUnclamped(
							-angle + ANGLE_DELTA * facingSign,
							angle + ANGLE_DELTA * facingSign,
							Ease.InOutQuart(
								(animationFrame - frequency + i * frameDelta).PingPong(frameLen).Clamp(0, frequency) / (float)frequency
							)
						);
						AngeUtil.LimbRotate(
							ref x, ref y, ref px, ref py, ref r, ref w, ref h,
							targetRot, false, limbGrow
						);
					} else {
						// Subsequent
						int MIN_ANGLE = 43 * angleAmountSubsequent / 1000;
						int MAX_ANGLE = 62 * angleAmountSubsequent / 1000;
						ANGLE_DELTA = ANGLE_DELTA * angleAmountSubsequent / 1000;
						int angle = Util.Remap(0, count - 1, MIN_ANGLE, MAX_ANGLE, i) * facingSign;
						int targetRot = (int)Mathf.LerpUnclamped(
							-angle + ANGLE_DELTA * facingSign,
							angle + ANGLE_DELTA * facingSign,
							Ease.InOutQuart(
								(animationFrame - frequency + i * frameDelta).PingPong(frameLen).Clamp(0, frequency) / (float)frequency
							)
						);
						AngeUtil.LimbRotate(
							ref x, ref y, ref px, ref py, ref r, ref w, ref h,
							prevX, prevY, prevR, prevW, prevH,
							targetRot, false, limbGrow
						);
					}
				}

				// Draw
				if (sprite.GlobalBorder.IsZero) {
					CellRenderer.Draw(sprite.GlobalID, x + offsetX, y + offsetY, px, py, r, w, h, z);
				} else {
					CellRenderer.Draw_9Slice(sprite.GlobalID, x + offsetX, y + offsetY, px, py, r, w, h, z);
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
}
