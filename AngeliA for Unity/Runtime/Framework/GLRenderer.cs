using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaForUnity {
	public class GLRenderer : MonoBehaviour {

		public struct RectCell {
			public RectInt Rect;
			public Color32 Color;
		}
		public struct TextureCell {
			public RectInt Rect;
			public Texture2D Texture;
		}

		private Camera Camera;
		private Material RectMaterial;
		private static readonly RectCell[] Rects = new RectCell[4096];
		private static readonly TextureCell[] Textures = new TextureCell[256];
		private static int RectCount = 0;
		private static int TextureCount = 0;


		private void Awake () {
			Camera = GetComponent<Camera>();
			RectMaterial = new Material(Shader.Find("Angelia/Vertex"));
		}


		[OnGameUpdate(-4096)]
		public static void OnGameUpdate () {
			RectCount = 0;
			TextureCount = 0;
			for (int i = 0; i < Textures.Length; i++) {
				Textures[i].Texture = null;
			}
		}


		private void OnPostRender () {

			if (Camera == null) return;
			var cameraRect = CellRenderer.CameraRect;

			// Rects
			if (RectCount > 0) {
				RectMaterial.SetPass(0);
				GL.LoadOrtho();
				GL.Begin(GL.QUADS);
				var rect = new Rect();
				for (int i = 0; i < RectCount; i++) {
					try {
						var cell = Rects[i];
						GL.Color(cell.Color);

						rect.x = Util.InverseLerp(cameraRect.x, cameraRect.xMax, cell.Rect.x);
						rect.y = Util.InverseLerp(cameraRect.y, cameraRect.yMax, cell.Rect.y);
						rect.width = Util.InverseLerp(0, cameraRect.width, cell.Rect.width);
						rect.height = Util.InverseLerp(0, cameraRect.height, cell.Rect.height);

						GL.Vertex3(rect.x, rect.y, 0.5f);
						GL.Vertex3(rect.x, rect.yMax, 0.5f);
						GL.Vertex3(rect.xMax, rect.yMax, 0.5f);
						GL.Vertex3(rect.xMax, rect.y, 0.5f);

					} catch (System.Exception ex) { Debug.LogException(ex); }
				}
				GL.End();
			}

			// Textures
			if (TextureCount > 0) {
				int screenWidth = Screen.width;
				int screenHeight = Screen.height;
				var targetCameraRect = Camera.rect;
				targetCameraRect.x *= screenWidth;
				targetCameraRect.width *= screenWidth;
				targetCameraRect.height *= screenHeight;
				GL.LoadPixelMatrix();
				var rect = new Rect();
				for (int i = 0; i < TextureCount; i++) {
					var cell = Textures[i];
					rect.width = Util.RemapUnclamped(
						0, cameraRect.width, 0, targetCameraRect.width, cell.Rect.width
					);
					rect.height = Util.RemapUnclamped(
						0, cameraRect.height, 0, targetCameraRect.height, cell.Rect.height
					);
					rect.x = Util.RemapUnclamped(
						cameraRect.x, cameraRect.xMax, targetCameraRect.x, targetCameraRect.xMax, cell.Rect.x
					);
					rect.y = Util.RemapUnclamped(
						 cameraRect.y, cameraRect.yMax, targetCameraRect.y, targetCameraRect.yMax, cell.Rect.y
					);

					Graphics.DrawTexture(rect, cell.Texture);

				}
			}

		}


		public static void DrawRect (RectInt rect, Color32 color) {
			if (RectCount >= Rects.Length) return;
			Rects[RectCount] = new RectCell() { Rect = rect, Color = color, };
			RectCount++;
		}


		public static void DrawTexture (RectInt rect, Texture2D texture) {
			if (TextureCount >= Textures.Length) return;
			Textures[TextureCount] = new TextureCell() { Rect = rect, Texture = texture, };
			TextureCount++;
		}


	}
}