using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework.World {
	public class WorldSquad {




		#region --- VAR ---


		// Data
		private WorldData[,] Worlds = new WorldData[3, 3] {
			{ new(Const.WORLD_MAP_SIZE), new(Const.WORLD_MAP_SIZE), new(Const.WORLD_MAP_SIZE) },
			{ new(Const.WORLD_MAP_SIZE), new(Const.WORLD_MAP_SIZE), new(Const.WORLD_MAP_SIZE) },
			{ new(Const.WORLD_MAP_SIZE), new(Const.WORLD_MAP_SIZE), new(Const.WORLD_MAP_SIZE) },
		};


		#endregion




		#region --- API ---


		public void Init () => FillSquad(0, 0);


		public void Update (Vector2Int center) {

			


		}


		#endregion




		#region --- LGC ---


		private void Shift (Direction2 deltaX, Direction2 deltaY) {
			if (deltaX == Direction2.Positive) {
				// R
				for (int y = 0; y < 2; y++) {
					(Worlds[0, y], Worlds[1, y]) = (Worlds[1, y], Worlds[0, y]);
					(Worlds[0, y], Worlds[2, y]) = (Worlds[2, y], Worlds[0, y]);
				}
			} else {
				// L
				for (int y = 0; y < 2; y++) {
					(Worlds[0, y], Worlds[2, y]) = (Worlds[2, y], Worlds[0, y]);
					(Worlds[0, y], Worlds[1, y]) = (Worlds[1, y], Worlds[0, y]);
				}
			}
			if (deltaY == Direction2.Positive) {
				// U
				for (int x = 0; x < 2; x++) {
					(Worlds[x, 0], Worlds[x, 1]) = (Worlds[x, 1], Worlds[x, 0]);
					(Worlds[x, 0], Worlds[x, 2]) = (Worlds[x, 2], Worlds[x, 0]);
				}
			} else {
				// D
				for (int x = 0; x < 2; x++) {
					(Worlds[x, 0], Worlds[x, 2]) = (Worlds[x, 2], Worlds[x, 0]);
					(Worlds[x, 0], Worlds[x, 1]) = (Worlds[x, 1], Worlds[x, 0]);
				}
			}
		}


		private void FillSquadAsync (int x, int y) {

			Worlds[0, 0].FillAsync(new Vector2Int(x - 1, y - 1));
			Worlds[0, 1].FillAsync(new Vector2Int(x - 1, y + 0));
			Worlds[0, 2].FillAsync(new Vector2Int(x - 1, y + 1));

			Worlds[1, 0].FillAsync(new Vector2Int(x + 0, y - 1));
			Worlds[1, 1].FillAsync(new Vector2Int(x + 0, y + 0));
			Worlds[1, 2].FillAsync(new Vector2Int(x + 0, y + 1));

			Worlds[2, 0].FillAsync(new Vector2Int(x + 1, y - 1));
			Worlds[2, 1].FillAsync(new Vector2Int(x + 1, y + 0));
			Worlds[2, 2].FillAsync(new Vector2Int(x + 1, y + 1));

		}


		private void FillSquad (int x, int y) {

			Worlds[0, 0].Fill(new Vector2Int(x - 1, y - 1));
			Worlds[0, 1].Fill(new Vector2Int(x - 1, y + 0));
			Worlds[0, 2].Fill(new Vector2Int(x - 1, y + 1));

			Worlds[1, 0].Fill(new Vector2Int(x + 0, y - 1));
			Worlds[1, 1].Fill(new Vector2Int(x + 0, y + 0));
			Worlds[1, 2].Fill(new Vector2Int(x + 0, y + 1));

			Worlds[2, 0].Fill(new Vector2Int(x + 1, y - 1));
			Worlds[2, 1].Fill(new Vector2Int(x + 1, y + 0));
			Worlds[2, 2].Fill(new Vector2Int(x + 1, y + 1));

		}


		#endregion




	}
}