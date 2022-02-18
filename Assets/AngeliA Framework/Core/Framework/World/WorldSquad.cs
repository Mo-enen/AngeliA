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
		public readonly World[,] Worlds = new World[3, 3] {
			{ new(), new(), new() },
			{ new(), new(), new() },
			{ new(), new(), new() },
		};
		private World[,] WorldBuffer = new World[3, 3];
		private World[] WorldBufferAlt = new World[9];


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
				int x = viewPos.x.Divide(Const.WORLD_MAP_SIZE * Const.CELL_SIZE);
				int y = viewPos.y.Divide(Const.WORLD_MAP_SIZE * Const.CELL_SIZE);
				Shift(x, y);
				FillSquad(x, y);
				//FillSquadAsync(x, y);
			}
		}


		// Iter
		public IEnumerable<(World.Block block, int globalX, int globalY, BlockLayer layer)> ForAllBlocksInsideAllLayers (RectInt globalUnitRect) {
			for (int layer = 0; layer < Const.BLOCK_LAYER_COUNT; layer++) {
				foreach (var pair in ForAllBlocksInside(globalUnitRect, (BlockLayer)layer)) {
					yield return pair;
				}
			}
		}


		public IEnumerable<(World.Block block, int globalX, int globalY, BlockLayer layer)> ForAllBlocksInside (RectInt globalUnitRect, BlockLayer layer) {
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
							var block = world.GetBlock(i - worldUnitRect.x, j - worldUnitRect.y, layerIndex);
							if (block.TypeID == 0) continue;
							yield return (block, i * Const.CELL_SIZE, j * Const.CELL_SIZE, layer);
						}
					}
				}
			}
		}


		public IEnumerable<(World.Entity entity, int globalX, int globalY)> ForAllEntitiesInside (RectInt globalUnitRect) {
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
							var entity = world.GetEntity(i - worldUnitRect.x, j - worldUnitRect.y);
							if (entity.TypeID == 0) continue;
							yield return (entity, i * Const.CELL_SIZE, j * Const.CELL_SIZE);
						}
					}
				}
			}
		}


		// Get
		public bool GetBlockAt (int globalX, int globalY, out World.Block block, out BlockLayer layer) {
			for (int i = Const.BLOCK_LAYER_COUNT - 1; i >= 0; i--) {
				layer = (BlockLayer)i;
				if (GetBlockAt(globalX, globalY, layer, out block)) {
					return true;
				}
			}
			block = default;
			layer = default;
			return false;
		}


		public bool GetBlockAt (int globalX, int globalY, BlockLayer layer, out World.Block block) {
			int unitX = globalX.Divide(Const.CELL_SIZE);
			int unitY = globalY.Divide(Const.CELL_SIZE);
			var allWorldUnitRect = new RectInt(
				Worlds[0, 0].FilledPosition * Const.WORLD_MAP_SIZE,
				Vector2Int.one * (3 * Const.WORLD_MAP_SIZE)
			);
			if (allWorldUnitRect.Contains(unitX, unitY)) {
				for (int worldJ = 0; worldJ <= 2; worldJ++) {
					for (int worldI = 0; worldI <= 2; worldI++) {
						var world = Worlds[worldI, worldJ];
						if (world.FilledUnitRect.Contains(unitX, unitY)) {
							var _block = world.GetBlock(
								unitX - world.FilledPosition.x * Const.WORLD_MAP_SIZE,
								unitY - world.FilledPosition.y * Const.WORLD_MAP_SIZE,
								(int)layer
							);
							if (_block.TypeID == 0) continue;
							block = _block;
							return true;
						}
					}
				}
			}
			block = default;
			return false;
		}


		public bool GetEntityAt (int globalX, int globalY, out World.Entity entity) {
			int unitX = globalX.Divide(Const.CELL_SIZE);
			int unitY = globalY.Divide(Const.CELL_SIZE);
			var allWorldUnitRect = new RectInt(Worlds[0, 0].FilledPosition * Const.WORLD_MAP_SIZE, Vector2Int.one * (3 * Const.WORLD_MAP_SIZE));
			if (allWorldUnitRect.Contains(unitX, unitY)) {
				for (int worldJ = 0; worldJ <= 2; worldJ++) {
					for (int worldI = 0; worldI <= 2; worldI++) {
						var world = Worlds[worldI, worldJ];
						if (world.FilledUnitRect.Contains(unitX, unitY)) {
							var _entity = world.GetEntity(
								unitX - world.FilledPosition.x * Const.WORLD_MAP_SIZE,
								unitY - world.FilledPosition.y * Const.WORLD_MAP_SIZE
							);
							if (_entity.TypeID == 0) continue;
							entity = _entity;
							return true;
						}
					}
				}
			}
			entity = default;
			return false;
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

			// Clear Buffer
			for (int i = 0; i < 9; i++) {
				WorldBuffer[i / 3, i % 3] = null;
				WorldBufferAlt[i] = null;
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