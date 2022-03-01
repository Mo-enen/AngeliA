using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace AngeliaFramework {
	public class WorldSquad {




		#region --- VAR ---


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


		public void Initialize () => LoadSquadFromDisk(0, 0);


		public void FrameUpdate (Vector2Int viewPos) {
			var midZone = new RectInt(
				Worlds[1, 1].WorldPosition * Const.WORLD_MAP_SIZE * Const.CELL_SIZE,
				new Vector2Int(Const.WORLD_MAP_SIZE * Const.CELL_SIZE, Const.WORLD_MAP_SIZE * Const.CELL_SIZE)
			).Expand(
				Const.WORLD_MAP_SIZE * Const.CELL_SIZE / 2
			);
			if (!midZone.Contains(viewPos)) {
				int x = viewPos.x.AltDivide(Const.WORLD_MAP_SIZE * Const.CELL_SIZE);
				int y = viewPos.y.AltDivide(Const.WORLD_MAP_SIZE * Const.CELL_SIZE);
				Shift(x, y);
				LoadSquadFromDisk(x, y);
			}
		}


		// Iter
		public void DrawBlocksInside (RectInt globalUnitRect, bool level) {
			var rect = new RectInt(0, 0, Const.CELL_SIZE, Const.CELL_SIZE);
			for (int worldI = 0; worldI <= 2; worldI++) {
				for (int worldJ = 0; worldJ <= 2; worldJ++) {
					var world = Worlds[worldI, worldJ];
					var worldUnitRect = world.WorldUnitRect;
					if (!worldUnitRect.Overlaps(globalUnitRect)) continue;
					int unitL = Mathf.Max(globalUnitRect.x, worldUnitRect.x);
					int unitR = Mathf.Min(globalUnitRect.xMax, worldUnitRect.xMax);
					int unitD = Mathf.Max(globalUnitRect.y, worldUnitRect.y);
					int unitU = Mathf.Min(globalUnitRect.yMax, worldUnitRect.yMax);
					for (int j = unitD; j < unitU; j++) {
						for (int i = unitL; i < unitR; i++) {
							if (level) {
								var block = world.GetLevelBlock(i - worldUnitRect.x, j - worldUnitRect.y);
								if (block.TypeID == 0) continue;
								rect.x = i * Const.CELL_SIZE;
								rect.y = j * Const.CELL_SIZE;
								if (block.HasCollider) {
									CellPhysics.FillBlock(
										PhysicsLayer.Level,
										rect.Shrink(block.ColliderBorder.Left, block.ColliderBorder.Right, block.ColliderBorder.Down, block.ColliderBorder.Up),
										block.IsTrigger,
										block.Tag
									);
								}
								CellRenderer.Draw(block.TypeID, rect, new Color32(255, 255, 255, 255));
							} else {
								var block = world.GetBackgroundBlock(i - worldUnitRect.x, j - worldUnitRect.y);
								if (block.TypeID == 0) continue;
								rect.x = i * Const.CELL_SIZE;
								rect.y = j * Const.CELL_SIZE;
								CellRenderer.Draw(block.TypeID, rect, new Color32(255, 255, 255, 255));
							}
						}
					}
				}
			}
		}


		public void SpawnEntitiesInside (RectInt globalUnitRect, Game game) {
			for (int worldI = 0; worldI <= 2; worldI++) {
				for (int worldJ = 0; worldJ <= 2; worldJ++) {
					var world = Worlds[worldI, worldJ];
					var worldUnitRect = world.WorldUnitRect;
					if (!worldUnitRect.Overlaps(globalUnitRect)) continue;
					int unitL = Mathf.Max(globalUnitRect.x, worldUnitRect.x);
					int unitR = Mathf.Min(globalUnitRect.xMax, worldUnitRect.xMax);
					int unitD = Mathf.Max(globalUnitRect.y, worldUnitRect.y);
					int unitU = Mathf.Min(globalUnitRect.yMax, worldUnitRect.yMax);
					for (int j = unitD; j < unitU; j++) {
						for (int i = unitL; i < unitR; i++) {
							var entity = world.GetEntity(i - worldUnitRect.x, j - worldUnitRect.y);
							if (entity.TypeID == 0) continue;
							game.TrySpawnEntity(
								globalUnitRect, entity, i * Const.CELL_SIZE, j * Const.CELL_SIZE
							);
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
					int localX = world.WorldPosition.x - x;
					int localY = world.WorldPosition.y - y;
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


		private void LoadSquadFromDisk (int x, int y) {
			for (int j = 0; j <= 2; j++) {
				for (int i = 0; i <= 2; i++) {
					var world = Worlds[i, j];
					var pos = new Vector2Int(x + i - 1, y + j - 1);
					if (world.WorldPosition != pos) {
						world.LoadFromDisk(pos.x, pos.y);
					}
				}
			}
		}


		#endregion




	}
}