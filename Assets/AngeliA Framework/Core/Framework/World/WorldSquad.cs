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
				if (viewPos.x < 0) x--;
				if (viewPos.y < 0) y--;
				Shift(x, y);
				FillSquad(x, y);
				//FillSquadAsync(x, y);
			}
		}


		public IEnumerable<(RectInt, WorldData.Block)> ForAllBlocksInsideAllLayers (RectInt globalUnitRect) {
			for (int layer = 0; layer < Const.BLOCK_LAYER_COUNT; layer++) {
				foreach (var pair in ForAllBlocksInside(globalUnitRect, (BlockLayer)layer)) {
					yield return pair;
				}
			}
		}


		public IEnumerable<(RectInt, WorldData.Block)> ForAllBlocksInside (RectInt globalUnitRect, BlockLayer layer) {
			int layerIndex = (int)layer;
			for (int worldI = 0; worldI <= 2; worldI++) {
				for (int worldJ = 0; worldJ <= 2; worldJ++) {
					var world = Worlds[worldI, worldJ];
					if (world.IsFilling) continue;
					var worldUnitRect = world.FilledUnitRect;
					if (!worldUnitRect.Overlaps(globalUnitRect)) continue;
					int unitL = Mathf.Max(globalUnitRect.x, worldUnitRect.x);
					int unitR = Mathf.Min(globalUnitRect.xMax, worldUnitRect.xMax);
					int unitD = Mathf.Max(globalUnitRect.y, worldUnitRect.y);
					int unitU = Mathf.Min(globalUnitRect.yMax, worldUnitRect.yMax);
					for (int j = unitD; j < unitU; j++) {
						for (int i = unitL; i < unitR; i++) {
							var block = world.Blocks[i - worldUnitRect.x, j - worldUnitRect.y, layerIndex];
							if (block.TypeID == 0) continue;
							var rect = new RectInt(
								i * Const.CELL_SIZE, j * Const.CELL_SIZE,
								Const.CELL_SIZE, Const.CELL_SIZE
							);
							yield return (rect, block);
						}
					}
				}
			}
		}


		public IEnumerable<(WorldData.Entity entity, int globalX, int globalY, EntityLayer layer)> ForAllEntitiesInsideAllLayers (RectInt globalUnitRect) {
			for (int layer = 0; layer < Const.ENTITY_LAYER_COUNT; layer++) {
				foreach (var entity in ForAllEntitiesInside(globalUnitRect, (EntityLayer)layer)) {
					yield return entity;
				}
			}
		}


		public IEnumerable<(WorldData.Entity entity, int globalX, int globalY, EntityLayer layer)> ForAllEntitiesInside (RectInt globalUnitRect, EntityLayer layer) {
			int layerIndex = (int)layer;
			for (int worldI = 0; worldI <= 2; worldI++) {
				for (int worldJ = 0; worldJ <= 2; worldJ++) {
					var world = Worlds[worldI, worldJ];
					if (world.IsFilling) continue;
					var worldUnitRect = world.FilledUnitRect;
					if (!worldUnitRect.Overlaps(globalUnitRect)) continue;
					int unitL = Mathf.Max(globalUnitRect.x, worldUnitRect.x);
					int unitR = Mathf.Min(globalUnitRect.xMax, worldUnitRect.xMax);
					int unitD = Mathf.Max(globalUnitRect.y, worldUnitRect.y);
					int unitU = Mathf.Min(globalUnitRect.yMax, worldUnitRect.yMax);
					for (int j = unitD; j < unitU; j++) {
						for (int i = unitL; i < unitR; i++) {
							var entity = world.Entities[i - worldUnitRect.x, j - worldUnitRect.y, layerIndex];
							if (entity.TypeID == 0) continue;
							yield return (entity, i * Const.CELL_SIZE, j * Const.CELL_SIZE, layer);
						}
					}
				}
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