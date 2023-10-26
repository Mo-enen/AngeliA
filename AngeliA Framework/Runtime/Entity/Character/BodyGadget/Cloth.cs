using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	public enum ClothType { Head, Body, Hand, Hip, Foot, }

	public enum FrontMode { Front, Back, AlwaysFront, AlwaysBack, }


	public abstract class AutoSpriteCloth : Cloth {

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
			character.DrawClothForHead(SpriteID0, Front);
		}
		protected override void DrawBodyShoulderArmArm (Character character) {
			if (ClothType != ClothType.Body) return;
			if (SpriteID0 != 0) {
				// Body
				if (SpriteID4 == 0 || character.Body.Width < 0) {
					character.DrawClothForBody(SpriteID0, SpriteID4 == 0);
				} else {
					character.DrawClothForBody(SpriteID4, false);
				}
			}
			if (SpriteID1 != 0) {
				// Shoulder
				if (CellRenderer.HasSpriteGroup(SpriteID1)) {
					if (CellRenderer.TryGetSpriteFromGroup(SpriteID1, character.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
						character.CoverClothOn(character.ShoulderL, spriteL.GlobalID, character.ShoulderL.Z + 1, Const.WHITE);
					}
					if (CellRenderer.TryGetSpriteFromGroup(SpriteID1, character.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
						character.CoverClothOn(character.ShoulderR, spriteR.GlobalID, character.ShoulderR.Z + 1, Const.WHITE);
					}
				} else {
					character.CoverClothOn(character.ShoulderL, SpriteID1, character.ShoulderL.Z + 1, Const.WHITE);
					character.CoverClothOn(character.ShoulderR, SpriteID1, character.ShoulderR.Z + 1, Const.WHITE);
				}
			}
			if (SpriteID2 != 0) {
				// Upper Arm
				if (CellRenderer.HasSpriteGroup(SpriteID2)) {
					if (CellRenderer.TryGetSpriteFromGroup(SpriteID2, character.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
						character.CoverClothOn(character.UpperArmL, spriteL.GlobalID, character.UpperArmL.Z + 1, Const.WHITE);
					}
					if (CellRenderer.TryGetSpriteFromGroup(SpriteID2, character.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
						character.CoverClothOn(character.UpperArmR, spriteR.GlobalID, character.UpperArmR.Z + 1, Const.WHITE);
					}
				} else {
					character.CoverClothOn(character.UpperArmL, SpriteID2, character.UpperArmL.Z + 1, Const.WHITE);
					character.CoverClothOn(character.UpperArmR, SpriteID2, character.UpperArmR.Z + 1, Const.WHITE);
				}
			}
			if (SpriteID3 != 0) {
				// Lower Arm
				if (CellRenderer.HasSpriteGroup(SpriteID3)) {
					if (CellRenderer.TryGetSpriteFromGroup(SpriteID3, character.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
						character.CoverClothOn(character.LowerArmL, spriteL.GlobalID, character.LowerArmL.Z + 1, Const.WHITE);
					}
					if (CellRenderer.TryGetSpriteFromGroup(SpriteID3, character.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
						character.CoverClothOn(character.LowerArmR, spriteR.GlobalID, character.LowerArmR.Z + 1, Const.WHITE);
					}
				} else {
					character.CoverClothOn(character.LowerArmL, SpriteID3, character.LowerArmL.Z + 1, Const.WHITE);
					character.CoverClothOn(character.LowerArmR, SpriteID3, character.LowerArmR.Z + 1, Const.WHITE);
				}
			}
		}
		protected override void DrawHand (Character character) {
			if (ClothType != ClothType.Hand) return;
			if (SpriteID0 != 0) {
				character.CoverClothOn(character.HandL, SpriteID0, character.HandL.Z + 1, Const.WHITE);
				character.CoverClothOn(character.HandR, SpriteID0, character.HandR.Z + 1, Const.WHITE);
			}
		}
		protected override void DrawHipSkirtLegLeg (Character character) {

			if (ClothType != ClothType.Hip) return;

			if (SpriteID0 != 0) {
				// Pants
				character.DrawClothForHip(SpriteID0);
			}
			if (SpriteID1 != 0) {
				// Skirt
				character.DrawClothForSkirt(SpriteID1);
			}
			if (SpriteID2 != 0) {
				// Upper Leg
				if (CellRenderer.HasSpriteGroup(SpriteID2)) {
					if (CellRenderer.TryGetSpriteFromGroup(SpriteID2, character.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
						character.CoverClothOn(character.UpperLegL, spriteL.GlobalID, character.UpperLegL.Z + 1, Const.WHITE);
					}
					if (CellRenderer.TryGetSpriteFromGroup(SpriteID2, character.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
						character.CoverClothOn(character.UpperLegR, spriteR.GlobalID, character.UpperLegR.Z + 1, Const.WHITE);
					}
				} else {
					character.CoverClothOn(character.UpperLegL, SpriteID2, character.UpperLegL.Z + 1, Const.WHITE);
					character.CoverClothOn(character.UpperLegR, SpriteID2, character.UpperLegR.Z + 1, Const.WHITE);
				}
			}
			if (SpriteID3 != 0) {
				// Lower Leg
				if (CellRenderer.HasSpriteGroup(SpriteID3)) {
					if (CellRenderer.TryGetSpriteFromGroup(SpriteID3, character.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
						character.CoverClothOn(character.LowerLegL, spriteL.GlobalID, character.LowerLegL.Z + 1, Const.WHITE);
					}
					if (CellRenderer.TryGetSpriteFromGroup(SpriteID3, character.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
						character.CoverClothOn(character.LowerLegR, spriteR.GlobalID, character.LowerLegR.Z + 1, Const.WHITE);
					}
				} else {
					character.CoverClothOn(character.LowerLegL, SpriteID3, character.LowerLegL.Z + 1, Const.WHITE);
					character.CoverClothOn(character.LowerLegR, SpriteID3, character.LowerLegR.Z + 1, Const.WHITE);
				}
			}
		}
		protected override void DrawFoot (Character character) {
			if (ClothType != ClothType.Foot) return;
			if (SpriteID0 != 0) {
				if (CellRenderer.HasSpriteGroup(SpriteID0)) {
					if (CellRenderer.TryGetSpriteFromGroup(SpriteID0, character.Body.FrontSide ? 0 : 1, out var spriteL, false, true)) {
						character.DrawClothForFoot(character.FootL, spriteL.GlobalID);
					}
					if (CellRenderer.TryGetSpriteFromGroup(SpriteID0, character.Body.FrontSide ? 1 : 0, out var spriteR, false, true)) {
						character.DrawClothForFoot(character.FootR, spriteR.GlobalID);
					}
				} else {
					character.DrawClothForFoot(character.FootL, SpriteID0);
					character.DrawClothForFoot(character.FootR, SpriteID0);
				}
			}
		}


	}



	public abstract class Cloth {


		// Api
		public int TypeID { get; init; }
		protected abstract ClothType ClothType { get; }
		protected virtual FrontMode Front => FrontMode.Front;

		// Data
		private static readonly Dictionary<int, Cloth> Pool = new();
		private static readonly Dictionary<int, int[]> DefaultPool = new();
		private static readonly int ClothTypeCount = typeof(ClothType).EnumLength();


		// MSG
		[OnGameInitialize(-128)]
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


	}
}
