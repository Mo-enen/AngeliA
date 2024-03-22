using System.Collections;
using System.Collections.Generic;
using AngeliA;
using Raylib_cs;

namespace AngeliaRuntime;


public class GLRect {
	public Rectangle Rect;
	public Color Color;
}


public class GLTexture {
	public Rectangle Rect;
	public Texture2D Texture;
	public Rectangle UV;
}


public static class GizmosRender {


	private static readonly GLRect[] GLRects = new GLRect[256 * 256].FillWithNewValue();
	private static readonly GLTexture[] GLTextures = new GLTexture[1024].FillWithNewValue();
	private static int GLRectCount = 0;
	private static int GLTextureCount = 0;


	public static void UpdateGizmos () {

		// Texture
		for (int i = 0; i < GLTextureCount; i++) {
			var glTexture = GLTextures[i];
			var rTexture = glTexture.Texture;
			var rect = glTexture.Rect;
			var uv = glTexture.UV;
			float yMin = rTexture.Height - (uv.Y + uv.Height);
			float yMax = rTexture.Height - uv.Y;
			uv.Y = yMin;
			uv.Height = yMax - yMin;
			Raylib.DrawTexturePro(
				rTexture, uv.ShrinkRectangle(0.01f), rect.ExpandRectangle(0.5f), new(0, 0), 0, Color.White
			);
		}
		GLTextureCount = 0;

		// Rect
		for (int i = 0; i < GLRectCount; i++) {
			var glRect = GLRects[i];
			var rect = glRect.Rect;
			Raylib.DrawRectangle(
				rect.X.RoundToInt(),
				rect.Y.RoundToInt(),
				rect.Width.RoundToInt(),
				rect.Height.RoundToInt(),
				glRect.Color
			);
		}
		GLRectCount = 0;
	}


	public static void DrawGizmosRect (Rectangle rect, Color32 color) {
		if (GLRectCount >= GLRects.Length) return;
		var glRect = GLRects[GLRectCount];
		glRect.Rect = rect;
		glRect.Color = color.ToRaylib();
		GLRectCount++;
	}


	public static void DrawGizmosTexture (Rectangle rect, Rectangle uv, Texture2D texture) {
		if (GLTextureCount >= GLTextures.Length) return;
		var glTexture = GLTextures[GLTextureCount];
		glTexture.Rect = rect;
		glTexture.Texture = texture;
		glTexture.UV = uv;
		GLTextureCount++;
	}



}