using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace AngeliaFramework {
	public static class CellPhysics {




		#region --- SUB ---


		private struct Cell {
			public RectInt Rect;
			public Entity Entity;
			public uint Frame;
			public Cell (RectInt rect, Entity entity, uint frame) {
				Rect = rect;
				Entity = entity;
				Frame = frame;
#if UNITY_EDITOR
				if (entity == null) {
					Debug.LogError("Entity of a physics cell is null");
				}
#endif
			}
		}


		private class PhysicsLayer {
			public Cell this[int x, int y, int z] {
				get => Cells[x, y, z];
				set => Cells[x, y, z] = value;
			}
			private Cell[,,] Cells = null;
			public PhysicsLayer (int width, int height) {
				Cells = new Cell[width, height, CELL_DEPTH];
				for (int i = 0; i < width; i++) {
					for (int j = 0; j < height; j++) {
						for (int z = 0; z < CELL_DEPTH; z++) {
							Cells[i, j, z].Frame = uint.MinValue;
						}
					}
				}
			}
		}


		#endregion




		#region --- VAR ---


		// Const
		private const int CELL_DEPTH = 8;

		// Api
		public static int Width { get; private set; } = 0;
		public static int Height { get; private set; } = 0;
		public static int PositionX { get; set; } = 0;
		public static int PositionY { get; set; } = 0;

		// Data
		private static PhysicsLayer[] Layers = null;
		private static PhysicsLayer CurrentLayer = null;
		private static uint CurrentFrame = uint.MinValue;


		#endregion




		#region --- API ---


		// Setup
		public static void Init (int width, int height, int layerCount) {
			Width = width;
			Height = height;
			Layers = new PhysicsLayer[layerCount];
		}


		public static void SetupLayer (int index) => Layers[index] = new PhysicsLayer(Width, Height);


		// Fill
		public static bool HasLayer (int layer) => Layers[layer] != null;


		public static void BeginFill (int layer, uint frame) {
			CurrentLayer = Layers[layer];
			CurrentFrame = frame;
		}


		public static void Fill (RectInt globalRect, Entity entity) {
			int i = globalRect.x.GetCellIndexX();
			int j = globalRect.y.GetCellIndexY();
			if (i < 0 || j < 0 || i >= Width || j >= Height) { return; }
#if UNITY_EDITOR
			if (globalRect.width > Const.CELL_SIZE || globalRect.height > Const.CELL_SIZE) {
				Debug.LogWarning("[CellPhysics] Rect size too large.");
			}
#endif
			for (int dep = 0; dep < CELL_DEPTH; dep++) {
				var cell = CurrentLayer[i, j, dep];
				if (cell.Frame != CurrentFrame) {
					cell.Rect = globalRect;
					cell.Entity = entity;
					cell.Frame = CurrentFrame;
					CurrentLayer[i, j, dep] = cell;
					return;
				}
			}
		}


		// Overlap
		public static bool Overlap (Layer layer, RectInt globalRect, Entity ignore, out RectInt hitRect, out Entity result) {
			result = null;
			hitRect = default;
			var layerItem = Layers[(int)layer];
			if (layerItem == null) { return false; }
			int l = Mathf.Max(globalRect.xMin.GetCellIndexX() - 1, 0);
			int d = Mathf.Max(globalRect.yMin.GetCellIndexY() - 1, 0);
			int r = Mathf.Min((globalRect.xMax - 1).GetCellIndexX() + 1, Width - 1);
			int u = Mathf.Min((globalRect.yMax - 1).GetCellIndexY() + 1, Height - 1);
			for (int j = d; j <= u; j++) {
				for (int i = l; i <= r; i++) {
					for (int dep = 0; dep < CELL_DEPTH; dep++) {
						var cell = layerItem[i, j, dep];
						if (cell.Frame != CurrentFrame) { break; }
						if (cell.Entity == ignore) { continue; }
						if (cell.Rect.Overlaps(globalRect)) {
							result = cell.Entity;
							hitRect = cell.Rect;
							return true;
						}
					}
				}
			}
			return false;
		}


		public static void ForAllOverlaps (Layer layer, RectInt globalRect, System.Func<RectInt, Entity, bool> func) {
			var layerItem = Layers[(int)layer];
			if (layerItem == null) { return; }
			int l = Mathf.Max(globalRect.xMin.GetCellIndexX() - 1, 0);
			int d = Mathf.Max(globalRect.yMin.GetCellIndexY() - 1, 0);
			int r = Mathf.Min((globalRect.xMax - 1).GetCellIndexX() + 1, Width - 1);
			int u = Mathf.Min((globalRect.yMax - 1).GetCellIndexY() + 1, Height - 1);
			for (int j = d; j <= u; j++) {
				for (int i = l; i <= r; i++) {
					for (int dep = 0; dep < CELL_DEPTH; dep++) {
						var cell = layerItem[i, j, dep];
						if (cell.Frame != CurrentFrame) { break; }
						if (cell.Rect.Overlaps(globalRect)) {
							if (!func(cell.Rect, cell.Entity)) { return; }
						}
					}
				}
			}
		}


		// Raycast




		#endregion




		#region --- LGC ---


		private static int GetCellIndexX (this int x) => (x - PositionX) / Const.CELL_SIZE;
		private static int GetCellIndexY (this int y) => (y - PositionY) / Const.CELL_SIZE;


		#endregion




	}
}
