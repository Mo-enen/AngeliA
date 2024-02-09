using System.Collections;
using System.Collections.Generic;


[assembly: AngeliaFramework.RequireGlobalSprite(atlas: "Character",
	"DefaultCharacterFace",
	"DefaultCharacterFace.Face.Normal",
	"DefaultCharacterFace.Face.Blink",
	"DefaultCharacterFace.Face.Sleep",
	"DefaultCharacterFace.Face.Attack",
	"DefaultCharacterFace.Face.Suffer",
	"DefaultCharacterFace.Face.PassOut",
	"DefaultCharacterFace.Face.Damage"
)]


namespace AngeliaFramework {


	public enum CharacterFaceType {
		Normal = 0,
		Blink = 1,
		Sleep = 2,
		Damage = 3,
		PassOut = 4,
		Attack = 5,
		Suffer = 6,
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


	[RequireSprite("{1}.Face.Attack", "{1}.Face.Blink", "{1}.Face.Damage", "{1}.Face.Normal", "{1}.Face.PassOut", "{1}.Face.Sleep", "{1}.Face.Suffer")]
	[RequireLanguage("{1}.Face")]
	public abstract class Face : BodyGadget {


		// VAR
		protected sealed override BodyGadgetType GadgetType => BodyGadgetType.Face;
		private FaceSpriteID SpriteID { get; init; }
		private static readonly FaceSpriteID DefaultSpriteID = new("");


		// API
		public Face () => SpriteID = new((GetType().DeclaringType ?? GetType()).AngeName());


		public static void DrawGadgetFromPool (PoseCharacter character) {
			if (character.FaceID != 0 && TryGetGadget(character.FaceID, out var face)) {
				face.DrawGadget(character);
			} else {
				DrawSpriteAsFace(character, DefaultSpriteID[GetCurrentFaceType(character)]);
			}
		}


		public override void DrawGadget (PoseCharacter character) => DrawSpriteAsFace(character, SpriteID[GetCurrentFaceType(character)]);


		public static void DrawSpriteAsFace (PoseCharacter character, int spriteGroupID, Int4 borderOffset = default) {

			var head = character.Head;
			if (spriteGroupID == 0 || head.IsFullCovered || !head.FrontSide) return;

			bool attacking =
				character.IsAttacking &&
				character.EquippingWeaponType != WeaponType.Magic &&
				character.EquippingWeaponType != WeaponType.Ranged;
			if (!CellRenderer.TryGetSpriteFromGroup(
				spriteGroupID,
				attacking ? (Game.GlobalFrame - character.LastAttackFrame) * 2 / character.AttackDuration : character.CurrentAnimationFrame / 5,
				out var sprite,
				loopIndex: !attacking,
				clampIndex: true
			)) return;

			int bounce = character.CurrentRenderingBounce;
			var headRect = head.GetGlobalRect();
			if (bounce.Abs() != 1000) {
				const int A2G = Const.CEL / Const.ART_CEL;
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
				border.left = head.Border.right;
				border.right = head.Border.left;
			}
			if (!border.IsZero) {
				if (head.Height < 0) {
					(border.down, border.up) = (border.up, border.down);
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


			// Draw
			var cells = CellRenderer.Draw_9Slice(
				sprite,
				faceRect.CenterX(), faceRect.y, 500, 0, 0, faceRect.width, faceRect.height,
				Const.WHITE, 33
			);

			if (cells != null) {

				// Twist
				int twist = character.HeadTwist;
				if (twist != 0) {
					int offsetX = faceRect.width * twist / 2000;
					foreach (var cell in cells) {
						cell.X += offsetX;
						cell.Width -= offsetX.Abs() / 2;
					}
					CellRenderer.ClampCells(cells, headRect);
				}

				// Rotate
				int headRot = character.HeadRotation;
				if (headRot != 0) {
					var body = character.Body;
					int offsetY = character.Head.Height.Abs() * headRot.Abs() / 360;
					foreach (var cell in cells) {
						cell.RotateAround(headRot, body.GlobalX, body.GlobalY + body.Height);
						cell.Y -= offsetY;
					}
				}
			}
		}


		public static void DrawSpriteAsHumanEar (PoseCharacter character, int spriteL, int spriteR, int offsetXL = -32, int offsetXR = 0) {

			// Get Face Rect
			var head = character.Head;
			var headRect = head.GetGlobalRect();
			bool facingRight = head.Width > 0;
			var faceRect = headRect;
			var border = head.Border;
			if (!facingRight) {
				border.left = head.Border.right;
				border.right = head.Border.left;
			}
			if (!border.IsZero) {
				if (head.Height < 0) {
					(border.down, border.up) = (border.up, border.down);
				}
				faceRect = headRect.Shrink(border);
			}

			// Draw Ears
			if (!facingRight) (offsetXL, offsetXR) = (-offsetXR, -offsetXL);
			var cellL = CellRenderer.Draw(
				spriteL,
				faceRect.x + offsetXL, faceRect.yMax, 1000, 1000, 0,
				Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE, character.SkinColor,
				facingRight ? 33 : -33
			);
			var cellR = CellRenderer.Draw(
				spriteR,
				faceRect.xMax + offsetXR, faceRect.yMax, 0, 1000, 0,
				Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE, character.SkinColor,
				facingRight ? -33 : 33
			);

			// Rotate
			int headRot = character.HeadRotation;
			if (headRot != 0) {
				var body = character.Body;
				int offsetY = character.Head.Height.Abs() * headRot.Abs() / 360;

				cellL.RotateAround(headRot, body.GlobalX, body.GlobalY + body.Height);
				cellL.Y -= offsetY;
				cellR.RotateAround(headRot, body.GlobalX, body.GlobalY + body.Height);
				cellR.Y -= offsetY;

			}
		}


		public static CharacterFaceType GetCurrentFaceType (PoseCharacter character) {

			// Attack
			if (
				character.IsAttacking &&
				character.EquippingWeaponType != WeaponType.Magic &&
				character.EquippingWeaponType != WeaponType.Ranged
			) return CharacterFaceType.Attack;

			// Blink
			if (
				(Game.GlobalFrame + character.TypeID).UMod(360) <= 8 &&
				character.AnimationType != CharacterAnimationType.Sleep &&
				character.AnimationType != CharacterAnimationType.PassOut
			) return CharacterFaceType.Blink;

			// Other
			return character.AnimationType switch {
				CharacterAnimationType.Sleep => CharacterFaceType.Sleep,
				CharacterAnimationType.PassOut => CharacterFaceType.PassOut,
				CharacterAnimationType.Crash => CharacterFaceType.Suffer,
				CharacterAnimationType.TakingDamage => CharacterFaceType.Damage,
				_ => CharacterFaceType.Normal,
			};
		}


	}
}
