using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public static class YayaUtil {


		public static void DrawSegmentHealthBar (
			int x, int y,
			int heartLeftCode, int heartRightCode, int emptyHeartLeftCode, int emptyHeartRightCode,
			int hp, int maxHP, int prevHP = int.MinValue
		) {

			const int SIZE = Const.CEL / 2;
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


	}
}