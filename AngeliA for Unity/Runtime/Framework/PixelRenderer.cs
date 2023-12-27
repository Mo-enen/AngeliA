using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaForUnity {
	public class PixelRenderer : MonoBehaviour {


		public struct Cell {
			public RectInt Rect;
			public Color32 Color;
		}
		private Camera Camera;
		private Material Material;
		private static readonly Cell[] Cells = new Cell[4096];
		private static int CellCount = 0;


		private void Awake () {
			Camera = GetComponent<Camera>();
			Material = new Material(Shader.Find("Angelia/Vertex"));
		}


		[OnGameUpdate(-4096)]
		public static void OnGameUpdate () => CellCount = 0;


		private void OnPostRender () {
			if (Camera == null || CellCount == 0) return;

			var cameraRect01 = Camera.rect;
			var angeCameraRect = CellRenderer.CameraRect;

			Material.SetPass(0);
			GL.LoadOrtho();
			GL.Begin(GL.QUADS);

			var rect = new Rect();
			for (int i = 0; i < CellCount; i++) {
				try {
					var cell = Cells[i];
					GL.Color(cell.Color);

					rect.x = Util.RemapUnclamped(
						angeCameraRect.x, angeCameraRect.xMax, cameraRect01.x, cameraRect01.xMax, cell.Rect.x
					);
					rect.y = Util.RemapUnclamped(
						angeCameraRect.y, angeCameraRect.yMax, cameraRect01.y, cameraRect01.yMax, cell.Rect.y
					);
					rect.width = Util.RemapUnclamped(
						0, angeCameraRect.width, 0, cameraRect01.width, cell.Rect.width
					);
					rect.height = Util.RemapUnclamped(
						0, angeCameraRect.height, 0, cameraRect01.height, cell.Rect.height
					);

					GL.Vertex3(rect.x, rect.y, 0.5f);
					GL.Vertex3(rect.x, rect.yMax, 0.5f);
					GL.Vertex3(rect.xMax, rect.yMax, 0.5f);
					GL.Vertex3(rect.xMax, rect.y, 0.5f);

				} catch (System.Exception ex) { Debug.LogException(ex); }
			}

			GL.End();
		}


		public static void Draw (RectInt rect, Color32 color) {
			if (CellCount >= Cells.Length) return;
			Cells[CellCount] = new Cell() { Rect = rect, Color = color, };
			CellCount++;
		}


	}
}