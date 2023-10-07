using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace AngeliaFramework {


	public enum CharacterFaceType {
		Normal = 0,
		Blink = 1,
		Sleep = 2,
		Damage = 3,
		PassOut = 4,
	}


	public class FaceSpriteID {

		private static readonly int FACE_TYPE_COUNT = typeof(CharacterFaceType).EnumLength();
		private static int[] DEFAULT_ID = null;

		public int this[CharacterFaceType type] => SpriteIDs[(int)type];

		private int[] SpriteIDs { get; init; }

		public FaceSpriteID (string keyword) {

			// Init Default
			if (DEFAULT_ID == null) {
				DEFAULT_ID = new int[FACE_TYPE_COUNT];
				for (int i = 0; i < DEFAULT_ID.Length; i++) {
					DEFAULT_ID[i] = $"DefaultCharacterFace.Face.{(CharacterFaceType)i}".AngeHash();
				}
			}

			// Set IDs
			SpriteIDs = new int[FACE_TYPE_COUNT];
			if (!string.IsNullOrEmpty(keyword)) {
				for (int i = 0; i < FACE_TYPE_COUNT; i++) {
					int id = $"{keyword}.Face.{(CharacterFaceType)i}".AngeHash();
					if (!CellRenderer.HasSpriteGroup(id) && !CellRenderer.HasSprite(id))
						id = DEFAULT_ID[i];
					SpriteIDs[i] = id;
				}
			} else {
				DEFAULT_ID.CopyTo(SpriteIDs, 0);
			}

		}

	}


	public abstract class AutoSpriteFace : Face {
		private FaceSpriteID SpriteID { get; init; }
		public AutoSpriteFace () => SpriteID = new((GetType().DeclaringType ?? GetType()).AngeName());
		protected override void DrawFace (Character character) => DrawSprite(character, SpriteID[GetFaceType(character)]);
	}



	public class DefaultCharacterFace : AutoSpriteFace { }


	public abstract class Face {


		// VAR
		private const int A2G = Const.CEL / Const.ART_CEL;
		private static readonly Dictionary<int, Face> Pool = new();
		private static readonly Dictionary<int, int> DefaultPool = new();
		private static readonly FaceSpriteID DefaultSpriteID = new("");


		// MSG
		[OnGameInitialize(-128)]
		public static void BeforeGameInitialize () {
			Pool.Clear();
			var CHARACTER_TYPE = typeof(Character);
			foreach (var type in typeof(Face).AllChildClass()) {
				if (System.Activator.CreateInstance(type) is not Face face) continue;
				int id = type.AngeHash();
				Pool.TryAdd(id, face);
				// Default
				var dType = type.DeclaringType;
				if (dType != null && dType.IsSubclassOf(CHARACTER_TYPE)) {
					DefaultPool.TryAdd(dType.AngeHash(), id);
				}
			}
			// Inherit
			foreach (var cType in CHARACTER_TYPE.AllChildClass()) {
				int cID = cType.AngeHash();
				if (DefaultPool.ContainsKey(cID)) continue;
				var _type = cType.BaseType;
				while (_type != CHARACTER_TYPE) {
					int _id = _type.AngeHash(); ;
					if (DefaultPool.TryGetValue(_id, out int _resultValue)) {
						DefaultPool.Add(cID, _resultValue);
						break;
					}
					_type = _type.BaseType;
				}
			}
		}


		// API
		public static void Draw (Character character) => Draw(character, out _);
		public static void Draw (Character character, out Face face) {
			face = null;
			if (
				character.FaceID != 0 &&
				Pool.TryGetValue(character.FaceID, out face)
			) {
				face.DrawFace(character);
			} else {
				DrawSprite(character, DefaultSpriteID[GetFaceType(character)]);
			}
		}


		public static bool TryGetDefaultFaceID (int characterID, out int faceID) => DefaultPool.TryGetValue(characterID, out faceID);


		protected abstract void DrawFace (Character character);


		// UTL
		protected static void DrawSprite (Character character, int spriteGroupID, Int4 borderOffset = default) {

			var head = character.Head;
			if (spriteGroupID == 0 || head.Tint.a == 0 || !head.FrontSide) return;

			if (!CellRenderer.TryGetSpriteFromGroup(
				spriteGroupID,
				character.CurrentAnimationFrame / 5,
				out var sprite, true, true
			)) return;

			int bounce = character.CurrentRenderingBounce;
			var headRect = head.GetGlobalRect();
			if (bounce.Abs() != 1000) {
				bool reverse = bounce < 0;
				bounce = bounce.Abs();
				int newWidth = (reverse ?
					headRect.width * 1000 / bounce :
					headRect.width - headRect.width * (1000 - bounce) / 1000
				).Clamp(headRect.width - A2G * 2, headRect.width + A2G * 2);
				headRect.x -= (newWidth - headRect.width) / 2;
				headRect.width = newWidth;
			}

			// Draw Sprite
			bool facingRight = head.Width > 0;
			var faceRect = headRect;
			var border = head.Border;
			if (!facingRight) {
				border.Left = head.Border.Right;
				border.Right = head.Border.Left;
			}
			if (!border.IsZero) {
				if (head.Height < 0) {
					(border.Down, border.Up) = (border.Up, border.Down);
				}
				faceRect = headRect.Shrink(border);
			}

			if (head.Height > 0) {
				faceRect.y += faceRect.height - sprite.GlobalHeight;
				faceRect.height = sprite.GlobalHeight;
			} else {
				faceRect.y += sprite.GlobalHeight;
				faceRect.height = -sprite.GlobalHeight;
			}

			if (!borderOffset.IsZero) {
				faceRect = faceRect.Expand(borderOffset);
			}

			CellRenderer.Draw_9Slice(sprite.GlobalID, faceRect, Const.WHITE, 33);
		}


		protected static void DrawHumanEar (Character character, int spriteL, int spriteR, int offsetXL = -32, int offsetXR = 0) {

			// Get Face Rect
			var head = character.Head;
			var headRect = head.GetGlobalRect();
			bool facingRight = head.Width > 0;
			var faceRect = headRect;
			var border = head.Border;
			if (!facingRight) {
				border.Left = head.Border.Right;
				border.Right = head.Border.Left;
			}
			if (!border.IsZero) {
				if (head.Height < 0) {
					(border.Down, border.Up) = (border.Up, border.Down);
				}
				faceRect = headRect.Shrink(border);
			}

			// Draw Ears
			if (!facingRight) (offsetXL, offsetXR) = (-offsetXR, -offsetXL);
			CellRenderer.Draw(
				spriteL,
				faceRect.x + offsetXL, faceRect.yMax, 1000, 1000, 0,
				Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE, character.SkinColor,
				facingRight ? 33 : -33
			);
			CellRenderer.Draw(
				spriteR,
				faceRect.xMax + offsetXR, faceRect.yMax, 0, 1000, 0,
				Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE, character.SkinColor,
				facingRight ? -33 : 33
			);

		}


		protected static CharacterFaceType GetFaceType (Character character) {
			bool blinking =
				(Game.GlobalFrame + character.TypeID).UMod(360) <= 8 &&
				character.AnimatedPoseType != CharacterPoseAnimationType.Sleep &&
				character.AnimatedPoseType != CharacterPoseAnimationType.PassOut;
			if (blinking) return CharacterFaceType.Blink;
			return character.AnimatedPoseType switch {
				CharacterPoseAnimationType.Sleep => CharacterFaceType.Sleep,
				CharacterPoseAnimationType.PassOut => CharacterFaceType.PassOut,
				CharacterPoseAnimationType.TakingDamage => CharacterFaceType.Damage,
				_ => CharacterFaceType.Normal,
			};
		}


	}
}
