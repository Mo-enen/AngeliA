using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace AngeliaFramework {
	public static class CellPhysics {




		#region --- SUB ---


		private class Cell {

			public bool HasNext => Next != null;

			public RectInt Rect = default;
			public Entity Entity = null;
			public Cell Next = null;

			public Cell (RectInt rect, Entity entity) {
				Rect = rect;
				Entity = entity;
				Next = null;
#if UNITY_EDITOR
				if (entity == null) {
					Debug.LogError("Entity of a physics cell is null");
				}
#endif
			}

		}


		private class PhysicsLayer {

			// Api
			public Cell this[int x, int y] {
				get => Cells[x, y];
				set => Cells[x, y] = value;
			}

			// Data
			private Cell[,] Cells = null;

			// API
			public PhysicsLayer (int width, int height) {
				Cells = new Cell[width, height];
			}

			public void Clear () => System.Array.Clear(Cells, 0, Cells.Length);

		}


		#endregion




		#region --- VAR ---


		// Api
		public static int Width { get; private set; } = 0;
		public static int Height { get; private set; } = 0;
		public static int PositionX { get; set; } = 0;
		public static int PositionY { get; set; } = 0;

		// Data
		private static PhysicsLayer[] Layers = null;
		private static PhysicsLayer CurrentLayer = null;


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
		public static bool HasLayer (Layer layer) => Layers[(int)layer] != null;


		public static void BeginFill (Layer layer) {
			CurrentLayer = Layers[(int)layer];
			CurrentLayer?.Clear();
		}


		public static void Fill (RectInt globalRect, Entity entity) {
			var (i, j) = globalRect.GetCellPosition();
			if (i < 0 || i >= Width || j < 0 || j >= Height) { return; }
			globalRect.width = Mathf.Clamp(globalRect.width, 0, Const.CELL_SIZE);
			globalRect.height = Mathf.Clamp(globalRect.height, 0, Const.CELL_SIZE);
			var cell = new Cell(globalRect, entity);
			var basicCell = CurrentLayer[i, j];
			if (basicCell != null) {
				while (basicCell.HasNext) {
					basicCell = basicCell.Next;
				}
				basicCell.Next = cell;
			} else {
				CurrentLayer[i, j] = cell;
			}
		}


		// Overlap
		public static bool Overlap (Layer layer, RectInt globalRect, Entity ignore, out Entity result) {
			result = null;
			var layerItem = Layers[(int)layer];
			if (layerItem == null) { return false; }
			var (i, j) = globalRect.GetCellPosition();
			int l = Mathf.Max(i - 1, 0);
			int r = Mathf.Min(i + 1, Width - 1);
			int d = Mathf.Max(j - 1, 0);
			int u = Mathf.Min(j + 1, Height - 1);
			for (int y = d; y <= u; y++) {
				for (int x = l; x <= r; x++) {
					var cell = layerItem[x, y];
					while (cell != null) {
						if (cell.Entity != ignore && cell.Rect.Overlaps(globalRect)) {
							result = cell.Entity;
							return true;
						}
						cell = cell.Next;
					}
				}
			}
			return false;
		}


		public static void ForAllOverlaps (Layer layer, RectInt globalRect, System.Func<RectInt, Entity, bool> func) {
			var layerItem = Layers[(int)layer];
			if (layerItem == null) { return; }
			var (i, j) = globalRect.GetCellPosition();
			int l = Mathf.Max(i - 1, 0);
			int r = Mathf.Min(i + 1, Width - 1);
			int d = Mathf.Max(j - 1, 0);
			int u = Mathf.Min(j + 1, Height - 1);
			for (int y = d; y <= u; y++) {
				for (int x = l; x <= r; x++) {
					var cell = layerItem[x, y];
					while (cell != null) {
						if (cell.Rect.Overlaps(globalRect)) {
							if (!func(cell.Rect, cell.Entity)) { return; }
						}
						cell = cell.Next;
					}
				}
			}
		}


		// Slove
		public static void Slove () {




		}



		#endregion




		#region --- LGC ---


		private static (int, int) GetCellPosition (this RectInt globalRect) => (
			(globalRect.x - PositionX) / Const.CELL_SIZE,
			(globalRect.y - PositionY) / Const.CELL_SIZE
		);



		#endregion




	}
}
