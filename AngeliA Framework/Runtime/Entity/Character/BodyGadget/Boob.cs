using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class AutoSpriteBoob : Boob {
		protected int SpriteID { get; init; }
		public AutoSpriteBoob () {
			string name = (GetType().DeclaringType ?? GetType()).AngeName();
			SpriteID = $"{name}.Boob".AngeHash();
			if (!CellRenderer.HasSpriteGroup(SpriteID) && !CellRenderer.HasSprite(SpriteID)) SpriteID = 0;
		}
		protected override void DrawBoob (Character character) => DrawSprite(character, SpriteID, character.SkinColor);
	}


	public abstract class Boob {


		// Const
		private const int A2G = Const.CEL / Const.ART_CEL;

		// VAR
		private static readonly Dictionary<int, Boob> Pool = new();
		private static readonly Dictionary<int, int> DefaultPool = new();


		// MSG
		[OnGameInitialize(-128)]
		public static void BeforeGameInitialize () {
			Pool.Clear();
			var charType = typeof(Character);
			foreach (var type in typeof(Boob).AllChildClass()) {
				if (System.Activator.CreateInstance(type) is not Boob boob) continue;
				int id = type.AngeHash();
				Pool.TryAdd(id, boob);
				// Default
				var dType = type.DeclaringType;
				if (dType != null && dType.IsSubclassOf(charType)) {
					DefaultPool.TryAdd(dType.AngeHash(), id);
				}
			}
		}


		// API
		public static void Draw (Character character) => Draw(character, out _);
		public static void Draw (Character character, out Boob boob) {
			boob = null;
			if (
				character.BoobID != 0 &&
				character.Body.FrontSide &&
				Pool.TryGetValue(character.BoobID, out boob)
			) {
				boob.DrawBoob(character);
			}
		}


		public static RectInt GetBoobRect (Character character, bool motion = true) => GetBoobRect(character, character.CharacterBoobSize, motion);
		public static RectInt GetBoobRect (Character character, int boobSize, bool motion = true) {
			var bodyRect = character.Body.GetGlobalRect();
			int bodySizeY = character.Body.SizeY;
			int boobHeight = boobSize * bodySizeY / 1000;
			int boobOffsetX = boobSize * bodyRect.width / 2000;
			int boobOffsetY = 0;
			if (motion) {
				boobOffsetX += (character.FacingRight ? 1 : -1) * character.PoseTwist * 20 / 1000;
				int basicRootY = character.BasicRootY;
				switch (character.AnimatedPoseType) {
					case CharacterPoseAnimationType.Walk:
						if (character.PoseRootY < basicRootY + A2G / 4) {
							boobOffsetY = 0;
						} else if (character.PoseRootY < basicRootY + A2G / 2) {
							boobOffsetY = 10;
						} else {
							boobOffsetY = 20;
						}
						break;
					case CharacterPoseAnimationType.Run:
						if (character.PoseRootY < basicRootY + A2G / 2) {
							boobOffsetY = 0;
						} else if (character.PoseRootY < basicRootY + A2G) {
							boobOffsetY = 10;
						} else {
							boobOffsetY = 20;
						}
						break;
				}
			}
			return new RectInt(
				character.FacingRight ? bodyRect.x : bodyRect.xMax,
				bodyRect.y + bodyRect.height * 618 / 1000 - boobHeight + boobOffsetY,
				character.FacingRight ? bodyRect.width + boobOffsetX : -bodyRect.width - boobOffsetX,
				boobHeight
			);
		}


		public static bool TryGetDefaultBoobID (int characterID, out int hornID) => DefaultPool.TryGetValue(characterID, out hornID);


		protected abstract void DrawBoob (Character character);


		// UTL
		protected static void DrawSprite (Character character, int spriteID, Color32 skinColor) {
			if (spriteID == 0 || !character.Body.FrontSide) return;
			CellRenderer.Draw(
				spriteID, GetBoobRect(character, true), skinColor, character.Body.Z + 1
			);
		}


	}
}
