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
			public int Width;
			public int Height;
			public int Rotation;
			public float PivotX;
			public float PivotY;
			public Color32 Color;
		}



		private class Layer {
			public Rect[] UVs;
			public Material Material;
			public Cell[] Cells;
			public int CellCount;
			public int UVCount;
		}



		#endregion




		#region --- VAR ---


		// Const
		public const int MAX_CELL_WIDTH = 48 * 512;
		public const int TARGET_CELL_HEIGHT = 16 * 512;

		// Api
		public static int LayerCount => Layers.Length;
		public static int Width { get; private set; } = 24 * 512;
		public static int Height { get; private set; } = 16 * 512;

		// Data
		private static Layer[] Layers = new Layer[0];
		private static Layer FocusedLayer = null;
		private static int FocusedCell = 0;


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
				Width = Mathf.CeilToInt(TARGET_CELL_HEIGHT * ratio);
				Height = TARGET_CELL_HEIGHT;
			} else {
				Width = MAX_CELL_WIDTH;
				Height = Mathf.CeilToInt(MAX_CELL_WIDTH / ratio);
			}

			// Render
			GL.PushMatrix();
			GL.LoadProjectionMatrix(Matrix4x4.TRS(
				new Vector3(-1f, -1f, 0f),
				Quaternion.identity,
				new Vector3(2f / Width, 2f / Height, 1f)
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
						float pX = cell.Width * cell.PivotX;
						float pY = cell.Height * cell.PivotY;
						a.x = -pX;
						a.y = -pY;
						b.x = -pX;
						b.y = cell.Height - pY;
						c.x = cell.Width - pX;
						c.y = cell.Height - pY;
						d.x = cell.Width - pX;
						d.y = -pY;

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


		public static void SetupLayer (int layerIndex, int cellCapaticy, Material material, Rect[] uvs) => Layers[layerIndex] = FocusedLayer = new Layer() {
			Cells = new Cell[cellCapaticy],
			Material = material,
			UVs = uvs,
			UVCount = uvs.Length,
			CellCount = cellCapaticy,
		};


		// Draw
		public static void BeginDraw (int layerIndex) {
			FocusedLayer = Layers[layerIndex];
			FocusedCell = 0;
		}


		public static void Draw (int id, int x, int y, float pivotX, float pivotY, int rotation, int width, int height, Color32 color) {
			if (id >= FocusedLayer.UVCount || FocusedCell < 0) { return; }
			var cell = FocusedLayer.Cells[FocusedCell];
			cell.ID = id;
			cell.X = x;
			cell.Y = y;
			cell.Width = width;
			cell.Height = height;
			cell.Rotation = rotation;
			cell.PivotX = pivotX;
			cell.PivotY = pivotY;
			cell.Color = color;
			FocusedLayer.Cells[FocusedCell] = cell;
			FocusedCell++;
			if (FocusedCell >= FocusedLayer.CellCount) {
				FocusedCell = -1;
			}
		}


		public static void EndDraw () {
			if (FocusedCell >= 0 && FocusedCell < FocusedLayer.CellCount) {
				FocusedLayer.Cells[FocusedCell].ID = -1;
			}
		}


		#endregion




	}
}
