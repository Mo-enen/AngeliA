using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class AutoSpriteHorn : Horn {
		private int SpriteIdL { get; init; }
		private int SpriteIdR { get; init; }
		private int SpriteIdLBack { get; init; }
		private int SpriteIdRBack { get; init; }
		protected virtual bool AnchorOnFace => false;
		protected virtual int FacingLeftOffsetX => 0;
		public AutoSpriteHorn () {
			string name = (GetType().DeclaringType ?? GetType()).AngeName();
			SpriteIdL = $"{name}.HornL".AngeHash();
			SpriteIdR = $"{name}.HornR".AngeHash();
			SpriteIdLBack = $"{name}.HornLB".AngeHash();
			SpriteIdRBack = $"{name}.HornRB".AngeHash();
			if (!CellRenderer.HasSprite(SpriteIdL)) SpriteIdL = 0;
			if (!CellRenderer.HasSprite(SpriteIdR)) SpriteIdR = 0;
			if (!CellRenderer.HasSprite(SpriteIdLBack)) SpriteIdLBack = SpriteIdL;
			if (!CellRenderer.HasSprite(SpriteIdRBack)) SpriteIdRBack = SpriteIdR;
		}
		protected override void DrawHorn (Character character) {
			int idL = character.FacingFront ? SpriteIdL : SpriteIdLBack;
			int idR = character.FacingFront ? SpriteIdR : SpriteIdRBack;
			DrawHornSprite(
				character, idL, idR,
				FrontOfHeadL(character), FrontOfHeadR(character),
				AnchorOnFace,
				character.FacingFront == character.FacingRight ? 0 : FacingLeftOffsetX
			);
		}
		protected virtual bool FrontOfHeadL (Character character) => true;
		protected virtual bool FrontOfHeadR (Character character) => true;
	}


	public abstract class Horn {


		// VAR
		private static readonly Dictionary<int, Horn> Pool = new();
		private static readonly Dictionary<int, int> DefaultPool = new();


		// MSG
		[OnGameInitialize(-128)]
		public static void BeforeGameInitialize () {
			Pool.Clear();
			var charType = typeof(Character);
			foreach (var type in typeof(Horn).AllChildClass()) {
				if (System.Activator.CreateInstance(type) is not Horn horn) continue;
				int id = type.AngeHash();
				Pool.TryAdd(id, horn);
				// Default
				var dType = type.DeclaringType;
				if (dType != null && dType.IsSubclassOf(charType)) {
					DefaultPool.TryAdd(dType.AngeHash(), id);
				}
			}
		}


		// API
		public static void Draw (Character character) => Draw(character, out _);
		public static void Draw (Character character, out Horn horn) {
			horn = null;
			if (
				character.HornID != 0 &&
				Pool.TryGetValue(character.HornID, out horn)
			) {
				horn.DrawHorn(character);
			}
		}


		public static bool TryGetDefaultHornID (int characterID, out int hornID) => DefaultPool.TryGetValue(characterID, out hornID);


		protected abstract void DrawHorn (Character character);


		// UTL
		protected static void DrawHornSprite (
			Character character, int spriteIdLeft, int spriteIdRight,
			bool frontOfHeadL = true, bool frontOfHeadR = true, bool onFace = false, int offsetX = 0
		) {

			if (spriteIdLeft == 0 && spriteIdRight == 0) return;
			var head = character.Head;
			if (head.Tint.a == 0) return;

			var headRect = head.GetGlobalRect();
			if (onFace) headRect = headRect.Shrink(head.Border);

			bool flipLR = !head.FrontSide && head.Height > 0;
			if (flipLR) {
				(spriteIdLeft, spriteIdRight) = (spriteIdRight, spriteIdLeft);
				offsetX = -offsetX;
			}

			// Twist
			int twist = character.HeadTwist;
			int twistWidth = 0;
			if (twist != 0) {
				twistWidth -= 16 * twist.Abs() / 500;
			}

			if (CellRenderer.TryGetSprite(spriteIdLeft, out var sprite)) {
				CellRenderer.Draw(
					spriteIdLeft,
					headRect.xMin + offsetX,
					head.Height > 0 ? headRect.yMax : headRect.yMin,
					sprite.PivotX, sprite.PivotY, 0,
					sprite.GlobalWidth * (flipLR ? -1 : 1) + twistWidth,
					head.Height.Sign3() * sprite.GlobalHeight,
					head.Z + (head.FrontSide == frontOfHeadL ? 34 : -34)
				);
			}

			if (CellRenderer.TryGetSprite(spriteIdRight, out sprite)) {
				CellRenderer.Draw(
					spriteIdRight,
					headRect.xMax + offsetX,
					head.Height > 0 ? headRect.yMax : headRect.yMin,
					sprite.PivotX, sprite.PivotY, 0,
					sprite.GlobalWidth * (flipLR ? -1 : 1) + twistWidth,
					head.Height.Sign3() * sprite.GlobalHeight,
					head.Z + (head.FrontSide == frontOfHeadR ? 34 : -34)
				);
			}

		}


	}
}