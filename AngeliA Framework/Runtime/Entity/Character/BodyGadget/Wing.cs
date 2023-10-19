using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace AngeliaFramework {


	public class AngelWing : AutoSpriteWing { protected override int Scale => 600; }
	public class DevilWing : AutoSpriteWing { protected override int Scale => 600; }
	public class PropellerWing : AutoSpriteWing { }


	public abstract class AutoSpriteWing : Wing {
		private int SpriteGroupID { get; init; }
		public override bool IsPropeller => _IsPropeller;
		protected virtual int Scale => 1000;
		private bool _IsPropeller { get; init; } = false;
		public AutoSpriteWing () {
			string name = (GetType().DeclaringType ?? GetType()).AngeName();
			SpriteGroupID = $"{name}.Wing".AngeHash();
			if (!CellRenderer.HasSpriteGroup(SpriteGroupID)) SpriteGroupID = 0;
			if (
				SpriteGroupID != 0 &&
				CellRenderer.TryGetSpriteFromGroup(SpriteGroupID, 0, out var sprite) &&
				CellRenderer.TryGetMeta(sprite.GlobalID, out var meta)
			) {
				_IsPropeller = meta.IsTrigger;
			}
		}
		protected override void DrawWing (Character character) => DrawSprite(character, SpriteGroupID, IsPropeller, Scale);
	}


	public abstract class Wing {


		// Api
		public virtual bool IsPropeller => false;

		// Data
		private static readonly Dictionary<int, Wing> Pool = new();
		private static readonly Dictionary<int, int> DefaultPool = new();


		// MSG
		[OnGameInitialize(-128)]
		public static void BeforeGameInitialize () {
			Pool.Clear();
			var charType = typeof(Character);
			foreach (var type in typeof(Wing).AllChildClass()) {
				if (System.Activator.CreateInstance(type) is not Wing wing) continue;
				int id = type.AngeHash();
				Pool.TryAdd(id, wing);
				// Default
				var dType = type.DeclaringType;
				if (dType != null && dType.IsSubclassOf(charType)) {
					DefaultPool.TryAdd(dType.AngeHash(), id);
				}
			}
		}


		// API
		public static void Draw (Character character) => Draw(character, out _);
		public static void Draw (Character character, out Wing wing) {
			wing = null;
			if (
				character.WingID != 0 &&
				Pool.TryGetValue(character.WingID, out wing)
			) {
				wing.DrawWing(character);
			}
		}


		public static bool TryGetDefaultWingID (int characterID, out int wingID) => DefaultPool.TryGetValue(characterID, out wingID);


		protected abstract void DrawWing (Character character);


		// UTL
		public static void DrawSprite (Character character, int spriteGroupID, bool isPropeller, int scale = 1000) {
			if (
				spriteGroupID == 0 ||
				!CellRenderer.HasSpriteGroup(spriteGroupID, out int groupCount) ||
				!CellRenderer.TryGetSpriteFromGroup(spriteGroupID, 0, out var firstSprite, false, true)
			) return;
			int z = character.Body.FrontSide ? -33 : 33;
			int xLeft = character.UpperLegL.GlobalX;
			int yLeft = character.UpperLegL.GlobalY;
			int xRight = character.UpperLegR.GlobalX;
			int yRight = character.UpperLegR.GlobalY;
			int spriteHeight = firstSprite.GlobalHeight * character.Body.Height.Sign() * scale / 1000;
			var animatedPoseType = character.AnimatedPoseType;
			if (
				animatedPoseType != CharacterPoseAnimationType.Sleep &&
				animatedPoseType != CharacterPoseAnimationType.PassOut &&
				animatedPoseType != CharacterPoseAnimationType.Fly
			) {
				var bodyRect = character.Body.GetGlobalRect();
				xLeft = bodyRect.xMin;
				yLeft = bodyRect.y;
				xRight = bodyRect.xMax;
				yRight = bodyRect.y;
			}
			if (animatedPoseType == CharacterPoseAnimationType.Fly) {
				// Flying
				if (isPropeller) {
					// Propeller
					if (CellRenderer.TryGetSpriteFromGroup(
						spriteGroupID, character.CurrentAnimationFrame.UMod(groupCount),
						out var sprite, true, true
					)) {
						CellRenderer.Draw(
							sprite.GlobalID,
							(xLeft + xRight) / 2,
							(yLeft + yRight) / 2,
							firstSprite.PivotX, firstSprite.PivotY, 0,
							firstSprite.GlobalWidth * scale / 1000,
							firstSprite.GlobalHeight * scale / 1000,
							z
						);
					}
				} else {
					// Wings
					if (CellRenderer.TryGetSpriteFromGroup(spriteGroupID, (character.CurrentAnimationFrame / 6).UMod(groupCount), out var sprite, true, true)) {
						CellRenderer.Draw(
							sprite.GlobalID,
							xLeft, yLeft, firstSprite.PivotX, firstSprite.PivotY, 0,
							firstSprite.GlobalWidth * scale / 1000,
							spriteHeight,
							z
						);
						CellRenderer.Draw(
							sprite.GlobalID,
							xRight, yRight, firstSprite.PivotX, firstSprite.PivotY, 0,
							-firstSprite.GlobalWidth * scale / 1000,
							spriteHeight,
							z
						);
					}
				}
			} else if (!isPropeller && firstSprite != null) {
				// Not Flying
				int rot = Game.GlobalFrame.PingPong(120) - 60;
				rot /= 12;
				CellRenderer.Draw(
					firstSprite.GlobalID,
					xLeft, yLeft, firstSprite.PivotX, firstSprite.PivotY, -rot,
					firstSprite.GlobalWidth * scale / 1000,
					spriteHeight,
					z
				);
				CellRenderer.Draw(
					firstSprite.GlobalID,
					xRight, yRight, firstSprite.PivotX, firstSprite.PivotY, rot,
					-firstSprite.GlobalWidth * scale / 1000,
					spriteHeight,
					z
				);
			}
		}


		public static bool IsPropellerWing (int wingID) => wingID != 0 && Pool.TryGetValue(wingID, out var wing) && wing.IsPropeller;


	}
}
