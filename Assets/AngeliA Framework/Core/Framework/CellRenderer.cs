using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public static class CellRenderer {




		#region --- SUB ---



		public struct Cell {
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



		public class Layer {
			public Rect[] UVs;
			public Material Material;
			public Cell[] Cells;
			public int CellCount;
			public int UVCount;
		}



		#endregion




		#region --- VAR ---


		// Const
		public const int MAX_CELL_WIDTH = 32 * Const.CELL_SIZE;
		public const int CELL_HEIGHT = 16 * Const.CELL_SIZE;

		// Api
		public static int LayerCount => Layers.Length;
		public static int Width { get; private set; } = 0;
		public static int Height { get; private set; } = 0;
		public static int ViewPositionX { get; set; } = 0;
		public static int ViewPositionY { get; set; } = 0;
		public static float Zoom { get; set; } = 1f;
		public static Layer DebugLayer { get; set; } = null;

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
			float ratio = (float)Screen.width / Screen.height;
			float maxRatio = (float)MAX_CELL_WIDTH / CELL_HEIGHT;
			var rect = new Rect(0f, 0f, 1f, 1f);
			if (ratio > maxRatio) {
				rect = new Rect(0.5f - 0.5f * maxRatio / ratio, 0f, maxRatio / ratio, 1f);
			}
			if (camera.rect.NotAlmost(rect)) {
				camera.rect = rect;
			}

			// Size
			(Width, Height) = GetCameraSize();

			// Render
			GL.PushMatrix();
			GL.LoadProjectionMatrix(Matrix4x4.TRS(
				new Vector3(-Zoom, -Zoom, 0f),
				Quaternion.identity,
				new Vector3(2f * Zoom / Width, 2f * Zoom / Height, 1f)
			));

			try {
				int layerCount = Layers.Length;
				for (int index = 0; index < layerCount; index++) {
					DrawLayer(Layers[index]);
				}
#if UNITY_EDITOR
				if (DebugLayer != null) { DrawLayer(DebugLayer); }
#endif
			} catch (System.Exception ex) { Debug.LogException(ex); }

			GL.PopMatrix();
		}


		private static void DrawLayer (Layer layer) {

			var cells = layer.Cells;
			int cellCount = cells.Length;
			var uvs = layer.UVs;

			layer.Material.SetPass(0);
			GL.Begin(GL.QUADS);

			var a = Vector3.zero;
			var b = Vector3.zero;
			var c = Vector3.zero;
			var d = Vector3.zero;

			for (int i = 0; i < cellCount; i++) {

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
				a.z = 0f;
				b.x = -pX;
				b.y = cell.Height - pY;
				b.z = 0f;
				c.x = cell.Width - pX;
				c.y = cell.Height - pY;
				c.z = 0f;
				d.x = cell.Width - pX;
				d.y = -pY;
				d.z = 0f;

				// Rotation
				if (cell.Rotation != 0) {
					var rot = Quaternion.Euler(0, 0, -cell.Rotation);
					a = rot * a;
					b = rot * b;
					c = rot * c;
					d = rot * d;
				}

				a.x += cell.X - ViewPositionX;
				a.y += cell.Y - ViewPositionY;
				b.x += cell.X - ViewPositionX;
				b.y += cell.Y - ViewPositionY;
				c.x += cell.X - ViewPositionX;
				c.y += cell.Y - ViewPositionY;
				d.x += cell.X - ViewPositionX;
				d.y += cell.Y - ViewPositionY;

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
			GL.End();
		}


		#endregion




		#region --- API ---


		// Layer
		public static void InitLayers (int layerCount) => Layers = new Layer[layerCount];


		public static void SetupLayer (int layerIndex, int cellCapaticy, Material material, Rect[] uvs) {
			var cells = new Cell[cellCapaticy];
			for (int i = 0; i < cellCapaticy; i++) {
				cells[i] = new Cell() { ID = -1 };
			}
			Layers[layerIndex] = new Layer() {
				Cells = cells,
				Material = material,
				UVs = uvs,
				UVCount = uvs.Length,
				CellCount = cellCapaticy,
			};
		}


		// Draw
		public static void BeginDraw (int layerIndex) {
			FocusedLayer = Layers[layerIndex];
			FocusedCell = 0;
		}


		public static void Draw (int id, int x, int y, float pivotX, float pivotY, int rotation, int width, int height, Color32 color) {
			if (id >= FocusedLayer.UVCount || FocusedCell < 0) { return; }
			var cell = new Cell {
				ID = id,
				X = x,
				Y = y,
				Width = width,
				Height = height,
				Rotation = rotation,
				PivotX = pivotX,
				PivotY = pivotY,
				Color = color
			};
			FocusedLayer.Cells[FocusedCell] = cell;
			FocusedCell++;
			if (FocusedCell >= FocusedLayer.CellCount) {
				FocusedCell = -1;
			}
		}


		public static void EndDraw () {
			if (FocusedCell >= 0 && FocusedCell < FocusedLayer.CellCount) {
				FocusedLayer.Cells[FocusedCell] = new Cell() { ID = -1 };
			}
		}


		// Camera
		public static (int width, int height) GetCameraSize () => (
			Mathf.CeilToInt(CELL_HEIGHT * Mathf.Min((float)Screen.width / Screen.height, (float)MAX_CELL_WIDTH / CELL_HEIGHT)),
			CELL_HEIGHT
		);



		#endregion




	}
}
