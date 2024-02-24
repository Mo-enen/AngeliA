using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AngeliA;
using Raylib_cs;

namespace AngeliaToRaylib;

public static class TextureUtil {

	public static Texture2D? GetTextureFromPixels (Color32[] pixels, int width, int height) {
		int len = width * height;
		if (len == 0) return null;
		unsafe {
			Texture2D textureResult;
			var image = new Image() {
				Format = PixelFormat.UncompressedR8G8B8A8,
				Width = width,
				Height = height,
				Mipmaps = 1,
			};
			if (pixels != null && pixels.Length == len) {
				var bytes = new byte[pixels.Length * 4];
				int index = 0;
				for (int y = 0; y < height; y++) {
					for (int x = 0; x < width; x++) {
						int i = (height - y - 1) * width + x;
						var p = pixels[i];
						bytes[index * 4 + 0] = p.r;
						bytes[index * 4 + 1] = p.g;
						bytes[index * 4 + 2] = p.b;
						bytes[index * 4 + 3] = p.a;
						index++;
					}
				}
				fixed (void* data = bytes) {
					image.Data = data;
					textureResult = Raylib.LoadTextureFromImage(image);
				}
			} else {
				textureResult = Raylib.LoadTextureFromImage(image);
			}
			Raylib.SetTextureFilter(textureResult, TextureFilter.Point);
			return textureResult;
		}
	}

	public static Color32[] GetPixelsFromTexture (Texture2D texture) {
		var image = Raylib.LoadImageFromTexture(texture);
		unsafe {
			int width = image.Width;
			int height = image.Height;
			var result = new Color32[width * height];
			var colors = Raylib.LoadImageColors(image);
			int index = 0;
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					int i = (height - y - 1) * width + x;
					result[index] = colors[i].ToAngelia();
					index++;
				}
			}
			Raylib.UnloadImageColors(colors);
			return result;
		}
	}

	public static void FillPixelsIntoTexture (Color32[] pixels, Texture2D texture) {
		if (pixels == null) return;
		int width = texture.Width;
		int height = texture.Height;
		if (pixels.Length != width * height) return;
		var colors = new Color[pixels.Length];
		int index = 0;
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				int i = (height - y - 1) * width + x;
				colors[index] = pixels[i].ToRaylib();
				index++;
			}
		}
		Raylib.UpdateTexture(texture, colors);
	}

	public static Texture2D? PngBytesToTexture (byte[] bytes) {
		if (bytes == null || bytes.Length == 0) return null;
		var image = Raylib.LoadImageFromMemory(".png", bytes);
		var result = Raylib.LoadTextureFromImage(image);
		Raylib.SetTextureFilter(result, TextureFilter.Point);
		return result;
	}

	public static unsafe byte[] TextureToPngBytes (Texture2D texture) {
		var fileType = Marshal.StringToHGlobalAnsi(".png");
		int fileSize = 0;
		char* result = Raylib.ExportImageToMemory(
			Raylib.LoadImageFromTexture(texture),
			(sbyte*)fileType.ToPointer(),
			&fileSize
		);
		if (fileSize == 0) return System.Array.Empty<byte>();
		var resultBytes = new byte[fileSize];
		Marshal.Copy((nint)result, resultBytes, 0, fileSize);
		Marshal.FreeHGlobal((System.IntPtr)result);
		Marshal.FreeHGlobal(fileType);
		return resultBytes;
	}

	public static void FillSheetIntoTexturePool (Sheet sheet, Dictionary<int, Texture2D> TexturePool) {
		foreach (var sprite in sheet.Sprites) {
			if (TexturePool.ContainsKey(sprite.GlobalID)) continue;
			var texture = GetTextureFromPixels(sprite.Pixels, sprite.PixelWidth, sprite.PixelHeight);
			if (!texture.HasValue) continue;
			Raylib.SetTextureFilter(texture.Value, TextureFilter.Point);
			Raylib.SetTextureWrap(texture.Value, TextureWrap.Clamp);
			TexturePool.Add(sprite.GlobalID, texture.Value);
		}
	}

}
