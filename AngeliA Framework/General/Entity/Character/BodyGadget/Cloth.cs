using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	public enum ClothType { Head, Body, Hand, Hip, Foot, }

	public enum FrontMode { Front, Back, AlwaysFront, AlwaysBack, }


	public abstract class AutoSpriteCloth : Cloth {

		private static readonly int HEAD_SUIT_SUFFIX = "UI.SuitSuffix.Head".AngeHash();
		private static readonly int BODY_SUIT_SUFFIX = "UI.SuitSuffix.Body".AngeHash();
		private static readonly int HIP_SUIT_SUFFIX = "UI.SuitSuffix.Hip".AngeHash();
		private static readonly int SKIRT_SUIT_SUFFIX = "UI.SuitSuffix.Skirt".AngeHash();
		private static readonly int HAND_SUIT_SUFFIX = "UI.SuitSuffix.Hand".AngeHash();
		private static readonly int FOOT_SUIT_SUFFIX = "UI.SuitSuffix.Foot".AngeHash();

		private int SpriteID0 { get; init; }
		private int SpriteID1 { get; init; }
		private int SpriteID2 { get; init; }
		private int SpriteID3 { get; init; }
		private int SpriteID4 { get; init; }

		public AutoSpriteCloth () {
			string name = (GetType().DeclaringType ?? GetType()).AngeName();
			SpriteID0 = $"{name}.{ClothType}Suit".AngeHash();
			SpriteID4 = 0;
			if (!CellRenderer.HasSprite(SpriteID0) && !CellRenderer.HasSpriteGroup(SpriteID0)) SpriteID0 = 0;
			if (ClothType == ClothType.Body) {
				if (SpriteID0 == 0) {
					SpriteID0 = $"{name}.BodySuitL".AngeHash();
					SpriteID4 = $"{name}.BodySuitR".AngeHash();
					if (!CellRenderer.HasSprite(SpriteID0) && !CellRenderer.HasSpriteGroup(SpriteID0)) SpriteID0 = 0;
					if (!CellRenderer.HasSprite(SpriteID4) && !CellRenderer.HasSpriteGroup(SpriteID4)) SpriteID4 = 0;
				}
				SpriteID1 = $"{name}.ShoulderSuit".AngeHash();
				SpriteID2 = $"{name}.UpperArmSuit".AngeHash();
				SpriteID3 = $"{name}.LowerArmSuit".AngeHash();
				if (!CellRenderer.HasSprite(SpriteID1) && !CellRenderer.HasSpriteGroup(SpriteID1)) SpriteID1 = 0;
				if (!CellRenderer.HasSprite(SpriteID2) && !CellRenderer.HasSpriteGroup(SpriteID2)) SpriteID2 = 0;
				if (!CellRenderer.HasSprite(SpriteID3) && !CellRenderer.HasSpriteGroup(SpriteID3)) SpriteID3 = 0;
			} else if (ClothType == ClothType.Hip) {
				SpriteID1 = $"{name}.SkirtSuit".AngeHash();
				SpriteID2 = $"{name}.UpperLegSuit".AngeHash();
				SpriteID3 = $"{name}.LowerLegSuit".AngeHash();
				if (!CellRenderer.HasSprite(SpriteID1) && !CellRenderer.HasSpriteGroup(SpriteID1)) SpriteID1 = 0;
				if (!CellRenderer.HasSprite(SpriteID2) && !CellRenderer.HasSpriteGroup(SpriteID2)) SpriteID2 = 0;
				if (!CellRenderer.HasSprite(SpriteID3) && !CellRenderer.HasSpriteGroup(SpriteID3)) SpriteID3 = 0;
			}
		}

		protected override void DrawHead (Character character) {
			if (ClothType != ClothType.Head) return;
			DrawClothForHead(character, SpriteID0, Front);
		}
		protected override void DrawBodyShoulderArmArm (Character character) {
			if (ClothType != ClothType.Body) return;
			if (SpriteID0 != 0) {
				// Body
				if (SpriteID4 == 0 || character.Body.Width < 0) {
					DrawClothForBody(character, SpriteID0, SpriteID4 == 0);
				} else {
					DrawClothForBody(character, SpriteID4, false);
				}
			}
			if (SpriteID1 != 0) {
				// Shoulder
				if (CellRenderer.HasSpriteGroup(SpriteID1)) {
					if (CellRenderer.TryGetSpriteFromGroup(SpriteID1, character.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
						CoverClothOn(character.ShoulderL, spriteL.GlobalID);
					}
					if (CellRenderer.TryGetSpriteFromGroup(SpriteID1, character.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
						CoverClothOn(character.ShoulderR, spriteR.GlobalID);
					}
				} else {
					CoverClothOn(character.ShoulderL, SpriteID1);
					CoverClothOn(character.ShoulderR, SpriteID1);
				}
			}
			if (SpriteID2 != 0) {
				// Upper Arm
				if (CellRenderer.HasSpriteGroup(SpriteID2)) {
					if (CellRenderer.TryGetSpriteFromGroup(SpriteID2, character.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
						CoverClothOn(character.UpperArmL, spriteL.GlobalID);
					}
					if (CellRenderer.TryGetSpriteFromGroup(SpriteID2, character.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
						CoverClothOn(character.UpperArmR, spriteR.GlobalID);
					}
				} else {
					CoverClothOn(character.UpperArmL, SpriteID2);
					CoverClothOn(character.UpperArmR, SpriteID2);
				}
			}
			if (SpriteID3 != 0) {
				// Lower Arm
				if (CellRenderer.HasSpriteGroup(SpriteID3)) {
					if (CellRenderer.TryGetSpriteFromGroup(SpriteID3, character.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
						CoverClothOn(character.LowerArmL, spriteL.GlobalID);
					}
					if (CellRenderer.TryGetSpriteFromGroup(SpriteID3, character.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
						CoverClothOn(character.LowerArmR, spriteR.GlobalID);
					}
				} else {
					CoverClothOn(character.LowerArmL, SpriteID3);
					CoverClothOn(character.LowerArmR, SpriteID3);
				}
			}
		}
		protected override void DrawHand (Character character) {
			if (ClothType != ClothType.Hand) return;
			if (SpriteID0 != 0) {
				if (CellRenderer.HasSpriteGroup(SpriteID0)) {
					if (CellRenderer.TryGetSpriteFromGroup(SpriteID0, character.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
						CoverClothOn(character.HandL, spriteL.GlobalID);
					}
					if (CellRenderer.TryGetSpriteFromGroup(SpriteID0, character.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
						CoverClothOn(character.HandR, spriteR.GlobalID);
					}
				} else {
					CoverClothOn(character.HandL, SpriteID0);
					CoverClothOn(character.HandR, SpriteID0);
				}

			}
		}
		protected override void DrawHipSkirtLegLeg (Character character) {

			if (ClothType != ClothType.Hip) return;

			if (SpriteID0 != 0) {
				// Pants
				DrawClothForHip(character, SpriteID0);
			}
			if (SpriteID1 != 0) {
				// Skirt
				DrawClothForSkirt(character, SpriteID1);
			}
			if (SpriteID2 != 0) {
				// Upper Leg
				if (CellRenderer.HasSpriteGroup(SpriteID2)) {
					if (CellRenderer.TryGetSpriteFromGroup(SpriteID2, character.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
						CoverClothOn(character.UpperLegL, spriteL.GlobalID);
					}
					if (CellRenderer.TryGetSpriteFromGroup(SpriteID2, character.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
						CoverClothOn(character.UpperLegR, spriteR.GlobalID);
					}
				} else {
					CoverClothOn(character.UpperLegL, SpriteID2);
					CoverClothOn(character.UpperLegR, SpriteID2);
				}
			}
			if (SpriteID3 != 0) {
				// Lower Leg
				if (CellRenderer.HasSpriteGroup(SpriteID3)) {
					if (CellRenderer.TryGetSpriteFromGroup(SpriteID3, character.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
						CoverClothOn(character.LowerLegL, spriteL.GlobalID);
					}
					if (CellRenderer.TryGetSpriteFromGroup(SpriteID3, character.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
						CoverClothOn(character.LowerLegR, spriteR.GlobalID);
					}
				} else {
					CoverClothOn(character.LowerLegL, SpriteID3);
					CoverClothOn(character.LowerLegR, SpriteID3);
				}
			}
		}
		protected override void DrawFoot (Character character) {
			if (ClothType != ClothType.Foot) return;
			if (SpriteID0 != 0) {
				if (CellRenderer.HasSpriteGroup(SpriteID0)) {
					if (CellRenderer.TryGetSpriteFromGroup(SpriteID0, character.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
						DrawClothForFoot(character.FootL, spriteL.GlobalID);
					}
					if (CellRenderer.TryGetSpriteFromGroup(SpriteID0, character.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
						DrawClothForFoot(character.FootR, spriteR.GlobalID);
					}
				} else {
					DrawClothForFoot(character.FootL, SpriteID0);
					DrawClothForFoot(character.FootR, SpriteID0);
				}
			}
		}

		public string GetDisplayName () {

			string typeName = GetType().AngeName();

			switch (ClothType) {
				case ClothType.Head:
					if (typeName.EndsWith("HeadSuit", System.StringComparison.OrdinalIgnoreCase)) {
						string smartName = typeName.Length > 8 ? typeName[..^8] : typeName;
						string name = Language.Get($"Pat.{smartName}".AngeHash(), Util.GetDisplayName(smartName));
						string suffix = Language.Get(HEAD_SUIT_SUFFIX, " Hat");
						return $"{name}{suffix}";
					}
					break;
				case ClothType.Body:
					if (typeName.EndsWith("BodySuit", System.StringComparison.OrdinalIgnoreCase)) {
						string smartName = typeName.Length > 8 ? typeName[..^8] : typeName;
						string name = Language.Get($"Pat.{smartName}".AngeHash(), Util.GetDisplayName(smartName));
						string suffix = Language.Get(BODY_SUIT_SUFFIX, " Cloth");
						return $"{name}{suffix}";
					}
					break;
				case ClothType.Hand:
					if (typeName.EndsWith("HandSuit", System.StringComparison.OrdinalIgnoreCase)) {
						string smartName = typeName.Length > 8 ? typeName[..^8] : typeName;
						string name = Language.Get($"Pat.{smartName}".AngeHash(), Util.GetDisplayName(smartName));
						string suffix = Language.Get(HAND_SUIT_SUFFIX, " Gloves");
						return $"{name}{suffix}";
					}
					break;
				case ClothType.Hip:
					if (typeName.EndsWith("HipSuit", System.StringComparison.OrdinalIgnoreCase)) {
						string smartName = typeName.Length > 7 ? typeName[..^7] : typeName;
						string name = Language.Get($"Pat.{smartName}".AngeHash(), Util.GetDisplayName(smartName));
						string suffix = SpriteID0 != 0 ?
							Language.Get(HIP_SUIT_SUFFIX, " Pants") :
							Language.Get(SKIRT_SUIT_SUFFIX, " Skirt");
						return $"{name}{suffix}";
					}
					break;
				case ClothType.Foot:
					if (typeName.EndsWith("FootSuit", System.StringComparison.OrdinalIgnoreCase)) {
						string smartName = typeName.Length > 8 ? typeName[..^8] : typeName;
						string name = Language.Get($"Pat.{smartName}".AngeHash(), Util.GetDisplayName(smartName));
						string suffix = Language.Get(FOOT_SUIT_SUFFIX, " Shoes");
						return $"{name}{suffix}";
					}
					break;
			}
			return $"{Language.Get($"Pat.{typeName}".AngeHash(), Util.GetDisplayName(typeName))}";

		}

	}



	public abstract class Cloth {


		// Const
		private static readonly Cell[] SINGLE_CELL = { CellRenderer.EMPTY_CELL };

		// Api
		public int TypeID { get; init; }
		protected abstract ClothType ClothType { get; }
		protected virtual FrontMode Front => FrontMode.Front;

		// Data
		private static readonly Dictionary<int, Cloth> Pool = new();
		private static readonly Dictionary<int, int[]> DefaultPool = new();
		private static readonly int ClothTypeCount = typeof(ClothType).EnumLength();


		// MSG
		[OnGameInitialize(-127)]
		public static void BeforeGameInitialize () {
			Pool.Clear();
			var charType = typeof(Character);
			foreach (var type in typeof(Cloth).AllChildClass()) {
				if (System.Activator.CreateInstance(type) is not Cloth cloth) continue;
				int suitID = type.AngeHash();
				Pool.TryAdd(suitID, cloth);
			}
			// Default Suit
			foreach (var (suitID, cloth) in Pool) {
				var dType = cloth.GetType().DeclaringType;
				if (dType == null || !dType.IsSubclassOf(charType)) continue;
				int charID = dType.AngeHash();
				if (DefaultPool.TryGetValue(charID, out var suitArray)) {
					suitArray[(int)cloth.ClothType] = suitID;
				} else {
					var arr = new int[ClothTypeCount];
					arr[(int)cloth.ClothType] = suitID;
					DefaultPool.TryAdd(charID, arr);
				}
			}
		}

		public Cloth () => TypeID = GetType().AngeHash();

		// API
		public static void DrawHeadSuit (Character character) => DrawHeadSuit(character, out _);
		public static void DrawBodySuit (Character character) => DrawBodySuit(character, out _);
		public static void DrawHipSuit (Character character) => DrawHipSuit(character, out _);
		public static void DrawHandSuit (Character character) => DrawHandSuit(character, out _);
		public static void DrawFootSuit (Character character) => DrawFootSuit(character, out _);

		public static void DrawHeadSuit (Character character, out Cloth cloth) {
			cloth = null;
			if (
				character.Suit_Head != 0 &&
				character.CharacterState != CharacterState.Sleep &&
				Pool.TryGetValue(character.Suit_Head, out cloth)
			) {
				cloth.DrawHead(character);
			}
		}
		public static void DrawBodySuit (Character character, out Cloth cloth) {
			cloth = null;
			if (
				character.Suit_Body != 0 &&
				Pool.TryGetValue(character.Suit_Body, out cloth)
			) {
				cloth.DrawBodyShoulderArmArm(character);
			}
		}
		public static void DrawHipSuit (Character character, out Cloth cloth) {
			cloth = null;
			if (
				character.Suit_Hip != 0 &&
				Pool.TryGetValue(character.Suit_Hip, out cloth)
			) {
				cloth.DrawHipSkirtLegLeg(character);
			}
		}
		public static void DrawHandSuit (Character character, out Cloth cloth) {
			cloth = null;
			if (
				character.Suit_Hand != 0 &&
				Pool.TryGetValue(character.Suit_Hand, out cloth)
			) {
				cloth.DrawHand(character);
			}
		}
		public static void DrawFootSuit (Character character, out Cloth cloth) {
			cloth = null;
			if (
				character.Suit_Foot != 0 &&
				character.CharacterState != CharacterState.Sleep &&
				Pool.TryGetValue(character.Suit_Foot, out cloth)
			) {
				cloth.DrawFoot(character);
			}
		}


		protected abstract void DrawHead (Character character);
		protected abstract void DrawBodyShoulderArmArm (Character character);
		protected abstract void DrawHand (Character character);
		protected abstract void DrawHipSkirtLegLeg (Character character);
		protected abstract void DrawFoot (Character character);


		// Pool
		public static bool HasCloth (int clothID) => Pool.ContainsKey(clothID);
		public static bool TryGetCloth (int clothID, out Cloth cloth) => Pool.TryGetValue(clothID, out cloth);


		public static bool TryGetDefaultSuitID (int characterID, ClothType suitType, out int suitID) {
			if (DefaultPool.TryGetValue(characterID, out var suitArray)) {
				suitID = suitArray[(int)suitType];
				return true;
			}
			suitID = 0;
			return false;
		}


		// Draw
		public static void DrawClothForHead (Character character, int spriteID, FrontMode frontMode) {

			var head = character.Head;
			if (spriteID == 0 || head.FullyCovered) return;

			// Width Amount
			int widthAmount = 1000;
			if (character.HeadTwist != 0) widthAmount -= character.HeadTwist.Abs() / 2;
			if (head.Height < 0) widthAmount = -widthAmount;

			// Draw
			Cell[] cells = null;
			CellRenderer.TryGetMeta(spriteID, out var meta);
			if (CellRenderer.HasSpriteGroup(spriteID)) {
				if (head.FrontSide) {
					// Front
					bool front = frontMode != FrontMode.AlwaysBack && frontMode != FrontMode.Back;
					if (CellRenderer.TryGetSpriteFromGroup(spriteID, 0, out var sprite, false, true)) {
						bool usePixelShift = head.FrontSide && head.Width < 0;
						if (usePixelShift && meta != null && meta.IsTrigger) {
							usePixelShift = false;
						}
						cells = AttachClothOn(
							head, sprite, 500, 1000,
							(front ? 34 : -34) - head.Z, widthAmount, 1000, 0,
							usePixelShift ? (front ? -16 : 16) : 0, 0
						);
					}
				} else {
					// Back
					if (CellRenderer.TryGetSpriteFromGroup(spriteID, 1, out var sprite, false, true)) {
						bool front = frontMode != FrontMode.AlwaysBack && frontMode != FrontMode.Front;
						bool usePixelShift = head.FrontSide && head.Width < 0;
						if (usePixelShift && meta != null && meta.IsTrigger) {
							usePixelShift = false;
						}
						cells = AttachClothOn(
							head, sprite, 500, 1000,
							(front ? 34 : -34) - head.Z, widthAmount, 1000, 0,
							usePixelShift ? (front ? -16 : 16) : 0, 0
						);
					}
				}
			} else if (CellRenderer.TryGetSprite(spriteID, out var sprite)) {
				// Single Sprite
				bool front = frontMode != FrontMode.AlwaysBack && (
					frontMode == FrontMode.AlwaysFront ||
					frontMode == FrontMode.Front == head.FrontSide
				);
				bool usePixelShift = head.FrontSide && head.Width < 0;
				if (usePixelShift && meta != null && meta.IsTrigger) {
					usePixelShift = false;
				}
				cells = AttachClothOn(
					head, sprite, 500, 1000,
					(front ? 34 : -34) - head.Z, widthAmount, 1000, 0,
					usePixelShift ? (front ? -16 : 16) : 0, 0
				);
			}
			// Head Rotate
			if (cells != null && character.HeadRotation != 0) {
				int offsetY = character.Head.Height.Abs() * character.HeadRotation.Abs() / 360;
				foreach (var cell in cells) {
					cell.RotateAround(character.HeadRotation, character.Body.GlobalX, character.Body.GlobalY + character.Body.Height);
					cell.Y -= offsetY;
				}
			}
		}


		public static void DrawClothForBody (Character character, int spriteGroupId, bool flipWithBody = true) => DrawClothForBody(character, spriteGroupId, Const.WHITE, flipWithBody);
		public static void DrawClothForBody (Character character, int spriteGroupId, Color32 tint, bool flipWithBody = true) {

			var body = character.Body;
			if (spriteGroupId == 0 || body.FullyCovered) return;

			var hip = character.Hip;
			int poseTwist = character.PoseTwist;

			int groupIndex = !body.FrontSide ? 3 :
				poseTwist.Abs() < 333 ? 0 :
				(flipWithBody || body.Width > 0) == (body.Width > 0 == (poseTwist < 0)) ? 1 :
				2;
			if (!CellRenderer.TryGetSpriteFromGroup(spriteGroupId, groupIndex, out var suitSprite, false, true)) return;

			var rect = new RectInt(
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
			if (!flipWithBody && body.Width < 0) rect.FlipHorizontal();

			// Draw
			CellRenderer.Draw(suitSprite.GlobalID, rect, tint, body.Z + 7);

			// Hide Limb
			if (CellRenderer.TryGetMeta(suitSprite.GlobalID, out var meta) && meta.Tag == Const.HIDE_LIMB_TAG) {
				body.FullyCovered = true;
			}
		}


		public static void DrawClothForHip (Character character, int spriteID) => DrawClothForHip(character, spriteID, Const.WHITE);
		public static void DrawClothForHip (Character character, int spriteID, Color32 tint) {

			var hip = character.Hip;
			if (spriteID == 0 || hip.FullyCovered) return;
			if (
				!CellRenderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 0 : 1, out var sprite, false, true) &&
				!CellRenderer.TryGetSprite(spriteID, out sprite)
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

			// Draw
			CellRenderer.Draw(
				sprite.GlobalID, rect, tint,
				CellRenderer.TryGetMeta(spriteID, out var meta) && meta.IsTrigger ? hip.Z + 4 : hip.Z + 1
			);

		}


		public static void DrawClothForSkirt (Character character, int spriteID) => DrawClothForSkirt(character, spriteID, Const.WHITE);
		public static void DrawClothForSkirt (Character character, int spriteID, Color32 tint) {

			var hip = character.Hip;
			if (spriteID == 0 || hip.FullyCovered) return;
			if (
				!CellRenderer.TryGetSpriteFromGroup(spriteID, character.Body.FrontSide ? 0 : 1, out var sprite, false, true) &&
				!CellRenderer.TryGetSprite(spriteID, out sprite)
			) return;

			var body = character.Body;
			var upperLegL = character.UpperLegL;
			var upperLegR = character.UpperLegR;
			var animatedPoseType = character.AnimatedPoseType;
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
				animatedPoseType != CharacterPoseAnimationType.GrabSide &&
				animatedPoseType != CharacterPoseAnimationType.Dash &&
				animatedPoseType != CharacterPoseAnimationType.Idle;
			int width = Mathf.Max(
				(right - left).Abs(), bodyWidthAbs - body.Border.left - body.Border.right
			);
			width += sprite.GlobalBorder.horizontal;
			if (stretch) width += Stretch(upperLegL.Rotation, upperLegR.Rotation);
			width += animatedPoseType switch {
				CharacterPoseAnimationType.JumpUp or CharacterPoseAnimationType.JumpDown => 2 * A2G,
				CharacterPoseAnimationType.Run => A2G / 2,
				_ => 0,
			};
			int shiftY = animatedPoseType switch {
				CharacterPoseAnimationType.Dash => A2G,
				_ => 0,
			};
			int offsetY = sprite.GlobalHeight * (1000 - sprite.PivotY) / 1000 + shiftY;
			int z = CellRenderer.TryGetMeta(spriteID, out var meta) && meta.IsTrigger ? hip.Z + 1 : hip.Z + 6;
			CellRenderer.Draw(
				sprite.GlobalID,
				centerX,
				body.Height > 0 ? Mathf.Max(centerY + offsetY, character.Y + sprite.GlobalHeight) : centerY - offsetY,
				500, 1000, 0,
				width,
				body.Height > 0 ? sprite.GlobalHeight : -sprite.GlobalHeight,
				tint, z
			);

			// Func
			static int Stretch (int rotL, int rotR) {
				int result = 0;
				if (rotL > 0) result += rotL / 2;
				if (rotR < 0) result += rotR / -2;
				return result;
			}
		}


		public static void DrawClothForFoot (BodyPart foot, int spriteID) => DrawClothForFoot(foot, spriteID, Const.WHITE);
		public static void DrawClothForFoot (BodyPart foot, int spriteID, Color32 tint) {
			if (spriteID == 0 || foot.FullyCovered) return;
			if (!CellRenderer.TryGetSprite(spriteID, out var sprite)) return;
			var location = foot.GlobalLerp(0f, 1f);
			if (sprite.GlobalBorder.IsZero) {
				CellRenderer.Draw(
					spriteID, location.x, location.y,
					0, 1000, foot.Rotation,
					foot.Width.Sign() * sprite.GlobalWidth, sprite.GlobalHeight,
					tint, foot.Z + 1
				);
			} else {
				CellRenderer.Draw_9Slice(
					spriteID, location.x, location.y,
					0, 1000, foot.Rotation,
					foot.Width.Sign() * sprite.GlobalWidth, sprite.GlobalHeight,
					tint, foot.Z + 1
				);
			}
			if (!CellRenderer.TryGetMeta(sprite.GlobalID, out var meta) || meta.Tag != Const.SHOW_LIMB_TAG) {
				foot.FullyCovered = true;
			}
		}


		public static Cell[] AttachClothOn (
			BodyPart bodyPart, AngeSprite sprite, int locationX, int locationY, int localZ,
			int widthAmount = 1000, int heightAmount = 1000,
			int localRotation = 0, int shiftPixelX = 0, int shiftPixelY = 0, bool defaultHideLimb = true
		) {
			var location = bodyPart.GlobalLerp(locationX / 1000f, locationY / 1000f);
			location.x += shiftPixelX;
			location.y += shiftPixelY;
			Cell[] result;
			if (sprite.GlobalBorder.IsZero) {
				var cell = CellRenderer.Draw(
					sprite.GlobalID,
					location.x,
					location.y,
					sprite.PivotX, sprite.PivotY, bodyPart.Rotation + localRotation,
					(bodyPart.Width > 0 ? sprite.GlobalWidth : -sprite.GlobalWidth) * widthAmount / 1000,
					(bodyPart.Height > 0 ? sprite.GlobalHeight : -sprite.GlobalHeight) * heightAmount / 1000,
					bodyPart.Z + localZ
				);
				result = SINGLE_CELL;
				result[0] = cell;
			} else {
				result = CellRenderer.Draw_9Slice(
					sprite.GlobalID,
					location.x,
					location.y,
					sprite.PivotX, sprite.PivotY, bodyPart.Rotation + localRotation,
					(bodyPart.Width > 0 ? sprite.GlobalWidth : -sprite.GlobalWidth) * widthAmount / 1000,
					(bodyPart.Height > 0 ? sprite.GlobalHeight : -sprite.GlobalHeight) * heightAmount / 1000,
					bodyPart.Z + localZ
				);
			}
			if (defaultHideLimb) {
				if (!CellRenderer.TryGetMeta(sprite.GlobalID, out var meta) || meta.Tag != Const.SHOW_LIMB_TAG) {
					bodyPart.FullyCovered = true;
				}
			} else {
				if (CellRenderer.TryGetMeta(sprite.GlobalID, out var meta) && meta.Tag == Const.HIDE_LIMB_TAG) {
					bodyPart.FullyCovered = true;
				}
			}
			return result;
		}


		public static Cell[] CoverClothOn (BodyPart bodyPart, int spriteID) => CoverClothOn(bodyPart, spriteID, 1, Const.WHITE, true);
		public static Cell[] CoverClothOn (BodyPart bodyPart, int spriteID, int localZ) => CoverClothOn(bodyPart, spriteID, localZ, Const.WHITE, true);
		public static Cell[] CoverClothOn (BodyPart bodyPart, int spriteID, int localZ, Color32 tint, bool defaultHideLimb = true) {
			if (spriteID == 0 || bodyPart.FullyCovered || !CellRenderer.TryGetSprite(spriteID, out var sprite)) return null;
			Cell[] result;
			if (sprite.GlobalBorder.IsZero) {
				SINGLE_CELL[0] = CellRenderer.Draw(
					spriteID, bodyPart.GlobalX, bodyPart.GlobalY,
					bodyPart.PivotX, bodyPart.PivotY, bodyPart.Rotation,
					bodyPart.Width, bodyPart.Height, tint, bodyPart.Z + localZ
				);
				result = SINGLE_CELL;
			} else {
				result = CellRenderer.Draw_9Slice(
					spriteID, bodyPart.GlobalX, bodyPart.GlobalY,
					bodyPart.PivotX, bodyPart.PivotY, bodyPart.Rotation,
					bodyPart.Width, bodyPart.Height, tint, bodyPart.Z + localZ
				);
			}
			if (defaultHideLimb) {
				if (!CellRenderer.TryGetMeta(sprite.GlobalID, out var meta) || meta.Tag != Const.SHOW_LIMB_TAG) {
					bodyPart.FullyCovered = true;
				}
			} else {
				if (CellRenderer.TryGetMeta(sprite.GlobalID, out var meta) && meta.Tag == Const.HIDE_LIMB_TAG) {
					bodyPart.FullyCovered = true;
				}
			}
			return result;
		}


		public static void DrawDoubleClothTailsOnHip (Character character, int spriteIdLeft, int spriteIdRight, bool drawOnAllPose = false) {

			var animatedPoseType = character.AnimatedPoseType;
			var hip = character.Hip;
			var body = character.Body;
			if (
				!drawOnAllPose && (
					animatedPoseType == CharacterPoseAnimationType.Rolling ||
					animatedPoseType == CharacterPoseAnimationType.Sleep ||
					animatedPoseType == CharacterPoseAnimationType.PassOut ||
					animatedPoseType == CharacterPoseAnimationType.Fly
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

			if (animatedPoseType == CharacterPoseAnimationType.Dash) scaleY = 500;

			DrawClothTail(character, spriteIdLeft, hipRect.x + 16, hipRect.y, z, rotL, scaleX, scaleY);
			DrawClothTail(character, spriteIdRight, hipRect.xMax - 16, hipRect.y, z, rotR, scaleX, scaleY);

		}


		public static void DrawClothTail (Character character, int spriteID, int globalX, int globalY, int z, int rotation, int scaleX = 1000, int scaleY = 1000, int motionAmount = 1000) {

			if (!CellRenderer.TryGetSprite(spriteID, out var sprite)) return;

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
			CellRenderer.Draw(
				spriteID,
				globalX, globalY,
				sprite.PivotX, sprite.PivotY, rotation + rot,
				sprite.GlobalWidth * scaleX / 1000,
				sprite.GlobalHeight * scaleY / 1000,
				z
			);

		}


		public static void DrawCape (Character character, int groupID, int motionAmount = 1000) {

			var animatedPoseType = character.AnimatedPoseType;
			var body = character.Body;
			if (
				animatedPoseType == CharacterPoseAnimationType.SquatIdle ||
				animatedPoseType == CharacterPoseAnimationType.SquatMove ||
				animatedPoseType == CharacterPoseAnimationType.Dash ||
				animatedPoseType == CharacterPoseAnimationType.Rolling ||
				animatedPoseType == CharacterPoseAnimationType.Fly ||
				animatedPoseType == CharacterPoseAnimationType.Sleep ||
				animatedPoseType == CharacterPoseAnimationType.PassOut ||
				groupID == 0 ||
				!CellRenderer.TryGetSpriteFromGroup(groupID, body.FrontSide ? 0 : 1, out var sprite, false, true)
			) return;

			// Draw
			int height = sprite.GlobalHeight + body.Height.Abs() - body.SizeY;
			var cells = CellRenderer.Draw_9Slice(
				sprite.GlobalID,
				body.GlobalX, body.GlobalY + body.Height,
				500, 1000, 0,
				sprite.GlobalWidth,
				body.Height.Sign() * height,
				Const.WHITE, body.FrontSide ? -31 : 31
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
