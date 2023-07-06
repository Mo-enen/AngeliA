using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class eWallpaperCloud : Wallpaper {


		// VAR
		private const int CLOUD_COUNT = 64;

		private static readonly int CLOUD = "Wallpeper.Cloud".AngeHash();
		private static readonly System.Random Ran = new(284721387);

		private readonly Vector3Int[] Positions = new Vector3Int[CLOUD_COUNT];


		// MSG
		public override void OnActivated () {
			base.OnActivated();
			for (int i = 0; i < CLOUD_COUNT; i++) {
				Positions[i] = new(
					Ran.Next(0, 100000),
					GetRandomY(),
					Ran.Next(0, 100000)
				);
			}
		}


		protected override void DrawBackground (RectInt backgroundRect) {

			int windSpeedMin = (Game.GlobalFrame / 1000).PingPong(1, 2);
			int windSpeedMax = (Game.GlobalFrame / 1000).PingPong(2, 4);
			int minSize = backgroundRect.height * 41 / 100;
			int maxSize = backgroundRect.height * 87 / 100;
			int left = backgroundRect.x - maxSize / 2;
			int right = backgroundRect.xMax + maxSize / 2;
			int bottom = backgroundRect.y;
			int top = backgroundRect.yMax + minSize / 3;
			var tint = GetSkyTint(backgroundRect.CenterY());

			for (int i = 0; i < CLOUD_COUNT; i++) {

				var pos = Positions[i];
				int x = (int)Util.RemapUnclamped(0, 100000, left, right, (float)pos.x);
				int y = (int)Util.RemapUnclamped(0, 100000, bottom, top, (float)pos.y);

				if (x >= left && x <= right) {
					int size = Util.RemapUnclamped(0, 100000, minSize, maxSize, pos.z);
					size = (int)Mathf.LerpUnclamped(size / 9f, size, pos.y / 100000f);
					int targetAlpha = pos.y > 30000 ? 24 : 12;
					tint.a = (byte)Util.RemapUnclamped(0, 1000, 0, targetAlpha, Amount);
					CellRenderer.Draw(
						CLOUD,
						x, y, 500, 500, 0, size, size,
						tint, int.MinValue + pos.y + 1
					);
				}

				pos.x += Util.RemapUnclamped(0, CLOUD_COUNT - 1, windSpeedMin, windSpeedMax, i);
				pos.x += (int)Util.RemapUnclamped(0, 100000, 0, 12, (float)pos.y);
				if (pos.x > 100000 || pos.x < 0) {
					pos.x = 0;
					pos.y = GetRandomY();
					pos.z = Ran.Next(0, 100000);
				}
				Positions[i] = pos;
			}
		}


		private int GetRandomY () => (int)(Ease.InCirc((float)Ran.NextDouble()) * 100000);


	}
}
