using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public static class CellRenderer {




		#region --- SUB ---



		private struct Cell {
			public int ID;
			public int X;
			public int Y;
			public int Scale;
			public int Rotation;
			public Color32 Color;
		}



		private class Layer {
			public Rect[] UVs;
			public Material Material;
			public Cell[] Cells;
		}



		#endregion




		#region --- VAR ---


		// Const
		public const int UNIT_MULT = 16 * 42;
		public const float MAX_CELL_WIDTH = 48;
		public const float TARGET_CELL_HEIGHT = 16;

		// Api
		public static int LayerCount => Layers.Length;
		public static float CellWidth { get; private set; } = 24f;
		public static float CellHeight { get; private set; } = 16f;

		// Data
		private static Layer[] Layers = new Layer[0];
		private static Layer FocusedLayer = null;


		#endregion




		#region --- MSG ---


		[RuntimeInitializeOnLoadMethod]
		private static void Init () {
			Camera.onPostRender -= OnPostRender;
			Camera.onPostRender += OnPostRender;
		}


		private static void OnPostRender (Camera camera) {

#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying) {
				Camera.onPostRender -= OnPostRender;
				return;
			}
#endif

			// Ratio
			float ratio = camera.aspect;
			if (ratio < MAX_CELL_WIDTH / TARGET_CELL_HEIGHT) {
				CellWidth = TARGET_CELL_HEIGHT * ratio;
				CellHeight = TARGET_CELL_HEIGHT;
			} else {
				CellWidth = MAX_CELL_WIDTH;
				CellHeight = MAX_CELL_WIDTH / ratio;
			}

			// Render
			GL.PushMatrix();
			GL.LoadProjectionMatrix(Matrix4x4.TRS(
				new Vector3(-1f, -1f, 0f),
				Quaternion.identity,
				new Vector3(2f / CellWidth / UNIT_MULT, 2f / CellHeight / UNIT_MULT, 1f)
			));
			GL.Begin(GL.QUADS);

			try {
				Material prevMat = null;
				int layerCount = Layers.Length;
				for (int index = 0; index < layerCount; index++) {

					var layer = Layers[index];
					var cells = layer.Cells;
					int len = cells.Length;
					var uvs = layer.UVs;

					if (layer.Material != prevMat) {
						layer.Material.SetPass(0);
						prevMat = layer.Material;
					}

					var a = Vector3.zero;
					var b = Vector3.zero;
					var c = Vector3.zero;
					var d = Vector3.zero;
					var center = Vector3.zero;

					for (int i = 0; i < len; i++) {

						var cell = cells[i];
						if (cell.ID < 0) { break; }
						var uv = uvs[cell.ID];

						// Color
						GL.Color(cell.Color);

						// Position
						a.x = cell.X;
						a.y = cell.Y;
						b.x = cell.X;
						b.y = cell.Y + UNIT_MULT;
						c.x = cell.X + UNIT_MULT;
						c.y = cell.Y + UNIT_MULT;
						d.x = cell.X + UNIT_MULT;
						d.y = cell.Y;

						// Scale
						if (cell.Scale != 1000) {
							center = (a + c) / 2f;
							float t01 = cell.Scale / 1000f;
							a.x = Mathf.LerpUnclamped(center.x, a.x, t01);
							a.y = Mathf.LerpUnclamped(center.y, a.y, t01);
							b.x = Mathf.LerpUnclamped(center.x, b.x, t01);
							b.y = Mathf.LerpUnclamped(center.y, b.y, t01);
							c.x = Mathf.LerpUnclamped(center.x, c.x, t01);
							c.y = Mathf.LerpUnclamped(center.y, c.y, t01);
							d.x = Mathf.LerpUnclamped(center.x, d.x, t01);
							d.y = Mathf.LerpUnclamped(center.y, d.y, t01);
						}

						// Rotation
						if (cell.Rotation != 0) {
							var rot = Quaternion.Euler(0, 0, -cell.Rotation);
							center = (a + c) / 2f;
							a = rot * (a - center) + center;
							b = rot * (b - center) + center;
							c = rot * (c - center) + center;
							d = rot * (d - center) + center;
						}

						// Final
						GL.TexCoord2(uv.xMin, uv.yMin);
						GL.Vertex(a);
						GL.TexCoord2(uv.xMin, uv.yMax);
						GL.Vertex(b);
						GL.TexCoord2(uv.xMax, uv.yMax);
						GL.Vertex(c);
						GL.TexCoord2(uv.xMax, uv.yMin);
						GL.Vertex(d);
					}
				}
			} catch (System.Exception ex) { Debug.LogException(ex); }

			GL.End();
			GL.PopMatrix();
		}


		#endregion




		#region --- API ---


		// Layer
		public static void InitLayers (int layerCount) {
			Layers = new Layer[layerCount];
		}


		public static void SetupLayer (int layerIndex, int cellCapaticy, Material material, Rect[] uvs) => Layers[layerIndex] = new Layer() {
			Cells = new Cell[cellCapaticy],
			Material = material,
			UVs = uvs,
		};


		public static void FocusLayer (int layerIndex) => FocusedLayer = Layers[layerIndex];


		// Cell
		public static void SetCell (
			int cellIndex, int id, int x, int y, int scale, int rotation, Color32 color
		) {
#if UNITY_EDITOR
			if (FocusedLayer == null) {
				Debug.LogWarning("[Cell Renderer] No Layer is Focused.");
			}
#endif
			var cell = FocusedLayer.Cells[cellIndex];
			cell.ID = id;
			cell.X = x;
			cell.Y = y;
			cell.Scale = scale;
			cell.Rotation = rotation;
			cell.Color = color;
			FocusedLayer.Cells[cellIndex] = cell;
		}


		#endregion




	}
}
