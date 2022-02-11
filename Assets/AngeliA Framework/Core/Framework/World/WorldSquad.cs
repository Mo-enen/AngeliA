using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace AngeliaFramework {
	public class WorldSquad {




		#region --- VAR ---


		// Handler
		public delegate void VoidHandler ();
		public static VoidHandler BeforeWorldShift { get; set; } = null;

		// Api
		public bool IsReady {
			get {
				for (int i = 0; i < 3; i++) {
					for (int j = 0; j < 3; j++) {
						if (Worlds[i, j].IsFilling) return false;
					}
				}
				return true;
			}
		}

		// Data
		public readonly WorldData[,] Worlds = new WorldData[3, 3] {
			{ new(), new(), new() },
			{ new(), new(), new() },
			{ new(), new(), new() },
		};
		private WorldData[,] WorldBuffer = new WorldData[3, 3];
		private WorldData[] WorldBufferAlt = new WorldData[9];


		#endregion




		#region --- API ---


		public void Init () => FillSquad(0, 0);


		public void FrameUpdate (Vector2Int viewPos) {
			var midZone = new RectInt(
				Worlds[1, 1].FilledPosition * Const.WORLD_MAP_SIZE * Const.CELL_SIZE,
				new Vector2Int(Const.WORLD_MAP_SIZE * Const.CELL_SIZE, Const.WORLD_MAP_SIZE * Const.CELL_SIZE)
			).Expand(
				Const.WORLD_MAP_SIZE * Const.CELL_SIZE / 2
			);
			if (!midZone.Contains(viewPos)) {
				BeforeWorldShift();
				int x = viewPos.x / Const.WORLD_MAP_SIZE / Const.CELL_SIZE;
				int y = viewPos.y / Const.WORLD_MAP_SIZE / Const.CELL_SIZE;
				Shift(x, y);
				//FillSquadAsync(x, y);
				FillSquad(x, y);
			}
		}


		#endregion




		#region --- LGC ---


		private void Shift (int x, int y) {
			// Clear Buffer
			for (int i = 0; i < 9; i++) {
				WorldBuffer[i / 3, i % 3] = null;
				WorldBufferAlt[i] = null;
			}

			// Worlds >> Buffer
			int alt = 0;
			for (int j = 0; j <= 2; j++) {
				for (int i = 0; i <= 2; i++) {
					var world = Worlds[i, j];
					int localX = world.FilledPosition.x - x;
					int localY = world.FilledPosition.y - y;
					if (localX >= -1 && localX <= 1 && localY >= -1 && localY <= 1) {
						WorldBuffer[localX + 1, localY + 1] = world;
					} else {
						WorldBufferAlt[alt] = world;
						alt++;
					}
				}
			}

			// Buffer >> Worlds
			alt = 0;
			for (int j = 0; j <= 2; j++) {
				for (int i = 0; i <= 2; i++) {
					var buffer = WorldBuffer[i, j];
					if (buffer != null) {
						Worlds[i, j] = buffer;
					} else {
						Worlds[i, j] = WorldBufferAlt[alt];
						alt++;
					}
				}
			}
		}


		private void FillSquad (int x, int y) {
			for (int j = 0; j <= 2; j++) {
				for (int i = 0; i <= 2; i++) {
					var world = Worlds[i, j];
					var pos = new Vector2Int(x + i - 1, y + j - 1);
					if (world.FilledPosition != pos) {
						world.Fill(pos);
					}
				}
			}
		}


		private void FillSquadAsync (int x, int y) {
			for (int j = 0; j <= 2; j++) {
				for (int i = 0; i <= 2; i++) {
					var world = Worlds[i, j];
					var pos = new Vector2Int(x + i - 1, y + j - 1);
					if (world.FilledPosition != pos) {
						world.FillAsync(pos);
					}
				}
			}
		}


		#endregion




	}
}