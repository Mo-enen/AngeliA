using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }


namespace Yaya {


	public static class YayaUtil {


		private static readonly Dictionary<int, int> HintCodePool = new();


		[AfterGameInitialize]
		public static void Initialize () {
			HintCodePool.Clear();
			var BOTTOM_TYPE = typeof(ActionEntity);
			foreach (var type in typeof(ActionEntity).AllChildClass()) {
				var _type = type;
				while (_type != null && _type != BOTTOM_TYPE) {
					string name = _type.Name;
					if (name[0] == 'e') name = name[1..];
					int code = $"ActionHint.{name}".AngeHash();
					if (Language.Has(code)) {
						HintCodePool.TryAdd(type.AngeHash(), code);
						break;
					}
					_type = _type.BaseType;
				}
			}
		}


		public static int GetHintLanguageCode (int typeID) => HintCodePool.TryGetValue(typeID, out int result) ? result : WORD.HINT_USE;


		public static void DrawSegmentHealthBar (
			int x, int y,
			int heartLeftCode, int heartRightCode, int emptyHeartLeftCode, int emptyHeartRightCode,
			int hp, int maxHP, int prevHP = int.MinValue
		) {

			const int SIZE = Const.HALF;
			const int COLUMN = 4;
			const int MAX = 8;

			int maxHp = Mathf.Min(maxHP, MAX);
			int left = x - SIZE * COLUMN / 4;

			// Draw Hearts
			var rect = new RectInt(0, 0, SIZE / 2, SIZE);
			bool isLeft = true;
			for (int i = 0; i < maxHp; i++) {
				rect.x = left + (i % COLUMN) * SIZE / 2;
				rect.y = y - (i / COLUMN + 1) * SIZE;
				if (i < hp) {
					// Heart
					CellRenderer.Draw(isLeft ? heartLeftCode : heartRightCode, rect).Z = 0;
				} else {
					// Empty Heart
					CellRenderer.Draw(isLeft ? emptyHeartLeftCode : emptyHeartRightCode, rect).Z = 0;
					// Spawn Drop Particle
					if (i < prevHP) {
						eYayaDroppingHeart heart;
						if (isLeft) {
							heart = Game.Current.SpawnEntity<eYayaDroppingHeartLeft>(rect.x, rect.y);
						} else {
							heart = Game.Current.SpawnEntity<eYayaDroppingHeartRight>(rect.x, rect.y);
						}
						if (heart != null) {
							heart.Width = rect.width + 8;
							heart.Height = rect.height + 16;
						}
					}
				}
				isLeft = !isLeft;
			}
		}


		public static Vector2Int GetFlyingFormation (Vector2Int pos, int column, int instanceIndex) {

			int sign = instanceIndex % 2 == 0 ? -1 : 1;
			int _row = instanceIndex / 2 / column;
			int _column = (instanceIndex / 2 % column + 1) * sign;
			int rowSign = (_row % 2 == 0) == (sign == 1) ? 1 : -1;

			int instanceOffsetX = _column * Const.CEL * 3 / 2 + rowSign * Const.HALF / 2;
			int instanceOffsetY = _row * Const.CEL + Const.CEL - _column.Abs() * Const.HALF / 3;

			// Result
			return new(pos.x + instanceOffsetX, pos.y + instanceOffsetY);
		}


	}
}