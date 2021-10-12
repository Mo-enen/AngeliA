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
			public int PivotX;
			public int PivotY;
			public Color32 Color;
		}



		private class Layer {
			public Rect[] UVs;
			public Material Material;
			public Cell[] Cells;
			public int UVCount;
		}



		#endregion




		#region --- VAR ---


		// Const
		public const int UNIT_MULT = 512;
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

					for (int i = 0; i < len; i++) {

						var cell = cells[i];
						if (cell.ID < 0) { break; }
						var uv = uvs[cell.ID];

						// Color
						GL.Color(cell.Color);

						// Position
						float pX = UNIT_MULT * cell.PivotX / 1000f;
						float pY = UNIT_MULT * cell.PivotY / 1000f;
						a.x = -pX;
						a.y = -pY;
						b.x = -pX;
						b.y = UNIT_MULT - pY;
						c.x = UNIT_MULT - pX;
						c.y = UNIT_MULT - pY;
						d.x = UNIT_MULT - pX;
						d.y = -pY;

						// Scale
						if (cell.Scale != 1000) {
							float t01 = cell.Scale / 1000f;
							a.x *= t01;
							a.y *= t01;
							b.x *= t01;
							b.y *= t01;
							c.x *= t01;
							c.y *= t01;
							d.x *= t01;
							d.y *= t01;
						}

						// Rotation
						if (cell.Rotation != 0) {
							var rot = Quaternion.Euler(0, 0, -cell.Rotation);
							a = rot * a;
							b = rot * b;
							c = rot * c;
							d = rot * d;
						}

						a.x += cell.X;
						a.y += cell.Y;
						b.x += cell.X;
						b.y += cell.Y;
						c.x += cell.X;
						c.y += cell.Y;
						d.x += cell.X;
						d.y += cell.Y;

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
		public static void InitLayers (int layerCount) => Layers = new Layer[layerCount];


		public static void SetupLayer (int layerIndex, int cellCapaticy, Material material, Rect[] uvs) => Layers[layerIndex] = new Layer() {
			Cells = new Cell[cellCapaticy],
			Material = material,
			UVs = uvs,
		};


		public static void FocusLayer (int layerIndex) => FocusedLayer = Layers[layerIndex];


		// Cell
		public static void SetCell (
			int index, int id,
			int x, int y,
			int pivotX, int pivotY,
			int rotation, int scale,
			Color32 color
		) {
			if (id >= FocusedLayer.UVCount) { return; }
			var cell = FocusedLayer.Cells[index];
			cell.ID = id;
			cell.X = x;
			cell.Y = y;
			cell.Scale = scale;
			cell.Rotation = rotation;
			cell.PivotX = pivotX;
			cell.PivotY = pivotY;
			cell.Color = color;
			FocusedLayer.Cells[index] = cell;
		}


		public static void MarkAsRoadblock (int index) {
			if (index >= 0 && index < FocusedLayer.Cells.Length) {
				FocusedLayer.Cells[index].ID = -1;
			}
		}

		#endregion




	}
}
