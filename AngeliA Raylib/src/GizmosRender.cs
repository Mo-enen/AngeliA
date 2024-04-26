using System.Collections;
using System.Collections.Generic;
using AngeliA;
using Raylib_cs;

namespace AngeliaRaylib;


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


	public static bool HasInverseGizmos => InverseTextureCount > 0;

	private static readonly GLRect[] GLRects = new GLRect[256 * 256].FillWithNewValue();
	private static readonly GLTexture[] GLTextures = new GLTexture[1024].FillWithNewValue();
	private static readonly GLTexture[] InverseTextures = new GLTexture[16].FillWithNewValue();
	private static int GLRectCount = 0;
	private static int GLTextureCount = 0;
	private static int InverseTextureCount = 0;


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
				rTexture,
				uv.ShrinkRectangle(0.001f),
				rect.ExpandRectangle(0.001f),
				new(0, 0), 0, Color.White
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


	public static void UpdateInverse () {
		if (InverseTextureCount <= 0) return;
		for (int i = 0; i < InverseTextureCount; i++) {
			var glTexture = InverseTextures[i];
			var rTexture = glTexture.Texture;
			var rect = glTexture.Rect;
			var uv = glTexture.UV;
			float yMin = rTexture.Height - (uv.Y + uv.Height);
			float yMax = rTexture.Height - uv.Y;
			uv.Y = yMin;
			uv.Height = yMax - yMin;
			Raylib.DrawTexturePro(
				rTexture,
				uv.ShrinkRectangle(0.001f),
				rect.ExpandRectangle(0.001f),
				new(0, 0), 0, Color.White
			);
		}
		InverseTextureCount = 0;
	}


	public static void DrawGizmosRect (Rectangle rect, Color32 color) {
		if (GLRectCount >= GLRects.Length) return;
		var glRect = GLRects[GLRectCount];
		glRect.Rect = rect;
		glRect.Color = color.ToRaylib();
		GLRectCount++;
	}


	public static void DrawGizmosTexture (Rectangle rect, Rectangle uv, Texture2D texture, bool inverse = false) {
		if (inverse) {
			if (InverseTextureCount >= InverseTextures.Length) return;
			var glTexture = InverseTextures[InverseTextureCount];
			glTexture.Rect = rect;
			glTexture.Texture = texture;
			glTexture.UV = uv;
			InverseTextureCount++;
		} else {
			if (GLTextureCount >= GLTextures.Length) return;
			var glTexture = GLTextures[GLTextureCount];
			glTexture.Rect = rect;
			glTexture.Texture = texture;
			glTexture.UV = uv;
			GLTextureCount++;
		}
	}


}