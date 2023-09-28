using System.Collections;
using System.Collections.Generic;
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
		public virtual bool SuitAvailable => true;
		public virtual int Size => 1000;


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
		public static bool TryGetBoob (int id, out Boob boob) => Pool.TryGetValue(id, out boob);


		public static void Draw (Character character) {
			if (
				character.BoobID != 0 &&
				character.Body.FrontSide &&
				Pool.TryGetValue(character.BoobID, out var boob)
			) {
				boob.DrawBoob(character);
			}
		}


		public static Vector2Int GetBoobPosition (Character character, bool motion = true) {
			var body = character.Body;
			int boobOffsetX = 0;
			int boobOffsetY = 0;
			if (motion) {
				boobOffsetX += character.PoseTwist * 10 / 1000;
				int basicRootY = character.BasicRootY;
				switch (character.AnimatedPoseType) {
					case CharacterPoseAnimationType.Walk:
						if (character.PoseRootY < basicRootY + A2G / 4) {
							boobOffsetY = 0;
						} else if (character.PoseRootY < basicRootY + A2G / 2) {
							boobOffsetY = -15;
						} else {
							boobOffsetY = 15;
						}
						break;
					case CharacterPoseAnimationType.Run:
						if (character.PoseRootY < basicRootY + A2G / 2) {
							boobOffsetY = 0;
						} else if (character.PoseRootY < basicRootY + A2G) {
							boobOffsetY = -15;
						} else {
							boobOffsetY = 15;
						}
						break;
				}
			}
			return new Vector2Int(
				body.GlobalX + boobOffsetX,
				body.GlobalY + body.Height.Abs() * 570 / 1000 + boobOffsetY
			);
		}


		public static bool TryGetDefaultBoobID (int characterID, out int hornID) => DefaultPool.TryGetValue(characterID, out hornID);


		protected abstract void DrawBoob (Character character);


		// UTL
		protected static void DrawSprite (Character character, int spriteID, Color32 skinColor) {
			if (spriteID == 0 || !character.Body.FrontSide) return;
			if (CellRenderer.TryGetSprite(spriteID, out var sprite)) {
				var pos = GetBoobPosition(character, true);
				if (sprite.GlobalBorder.IsZero) {
					CellRenderer.Draw(
						spriteID,
						pos.x, pos.y, 500, 1000, 0,
						sprite.GlobalWidth, sprite.GlobalHeight
						, skinColor, character.Body.Z + 1
					);
				} else {
					CellRenderer.Draw_9Slice(
						spriteID,
						pos.x, pos.y, 500, 1000, 0,
						sprite.GlobalWidth, sprite.GlobalHeight,
						skinColor, character.Body.Z + 1
					);
				}
			}
		}


	}
}
