using System.Collections;
using System.Collections.Generic;
using Raylib_cs;
using AngeliA;
using System.Runtime.InteropServices;

namespace AngeliaRaylib;

public static unsafe class RayUtil {


	// VAR
	private static readonly Color[] FillPixelCache = new Color[512 * 512];
	public static object EMPTY_TEXTURE_OBJ => EMPTY_TEXTURE;
	public static Texture2D EMPTY_TEXTURE { get; private set; }


	// API
	[OnGameInitialize(int.MinValue)]
	internal static void OnGameInitialize () {
		EMPTY_TEXTURE = (Texture2D)GetTextureFromPixels([Color32.CLEAR], 1, 1);
	}


	[OnGameQuitting]
	internal static void OnGameQuitting () {
		Raylib.UnloadTexture(EMPTY_TEXTURE);
	}


	public static void InitWindowForRiggedGame (bool onlyLogError = true) {
		Raylib.SetTraceLogLevel(onlyLogError ? TraceLogLevel.Error : TraceLogLevel.Trace);
		Raylib.SetWindowState(ConfigFlags.HiddenWindow);
		Raylib.InitWindow(1, 1, "");
	}


	// General
	public static string GetClipboardText () => Raylib.GetClipboardText_();

	public static void SetClipboardText (string text) => Raylib.SetClipboardText(text);

	public static void SetBlendFactorsForGeneral () {
		Rlgl.SetBlendFactorsSeparate(
			Rlgl.SRC_ALPHA,
			Rlgl.ONE_MINUS_SRC_ALPHA,
			Rlgl.ONE,
			Rlgl.ONE_MINUS_SRC_ALPHA,
			Rlgl.FUNC_ADD,
			Rlgl.FUNC_ADD
		);
	}

	public static void GetSlicedUvBorder (AngeSprite sprite, Alignment alignment, out Float2 bl, out Float2 br, out Float2 tl, out Float2 tr) {

		bl = new(0f, 0f);
		br = new(1f, 0f);
		tl = new(0f, 1f);
		tr = new(1f, 1f);

		// Y
		switch (alignment) {
			case Alignment.TopLeft:
			case Alignment.TopMid:
			case Alignment.TopRight:
				bl.y = br.y = (sprite.GlobalHeight - sprite.GlobalBorder.up) / (float)sprite.GlobalHeight;
				break;
			case Alignment.MidLeft:
			case Alignment.MidMid:
			case Alignment.MidRight:
				tl.y = tr.y = (sprite.GlobalHeight - sprite.GlobalBorder.up) / (float)sprite.GlobalHeight;
				bl.y = br.y = sprite.GlobalBorder.down / (float)sprite.GlobalHeight;
				break;
			case Alignment.BottomLeft:
			case Alignment.BottomMid:
			case Alignment.BottomRight:
				tl.y = tr.y = sprite.GlobalBorder.down / (float)sprite.GlobalHeight;
				break;
		}
		// X
		switch (alignment) {
			case Alignment.TopLeft:
			case Alignment.MidLeft:
			case Alignment.BottomLeft:
				br.x = tr.x = sprite.GlobalBorder.left / (float)sprite.GlobalWidth;
				break;
			case Alignment.TopMid:
			case Alignment.MidMid:
			case Alignment.BottomMid:
				br.x = tr.x = (sprite.GlobalWidth - sprite.GlobalBorder.right) / (float)sprite.GlobalWidth;
				bl.x = tl.x = sprite.GlobalBorder.left / (float)sprite.GlobalWidth;
				break;
			case Alignment.TopRight:
			case Alignment.MidRight:
			case Alignment.BottomRight:
				bl.x = tl.x = (sprite.GlobalWidth - sprite.GlobalBorder.right) / (float)sprite.GlobalWidth;
				break;
		}
	}

	// Texture
	public static object GetTextureFromPixels (Color32[] pixels, int width, int height) {
		var result = GetTextureFromPixelsLogic(pixels, width, height);
		if (result.HasValue) {
			Raylib.SetTextureFilter(result.Value, TextureFilter.Point);
			Raylib.SetTextureWrap(result.Value, TextureWrap.Clamp);
			return result.Value;
		} else {
			return EMPTY_TEXTURE;
		}
	}

	public static Color32[] GetPixelsFromTexture (object texture) {
		if (texture is not Texture2D rTexture) return [];
		var image = Raylib.LoadImageFromTexture(rTexture);
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
		Raylib.UnloadImage(image);
		return result;
	}

	public static void FillPixelsIntoTexture (Color32[] pixels, object texture) {
		if (texture is not Texture2D rTexture) return;
		if (pixels == null) return;
		int width = rTexture.Width;
		int height = rTexture.Height;
		if (pixels.Length != width * height) return;
		var colors = pixels.Length <= FillPixelCache.Length ? FillPixelCache : new Color[pixels.Length];
		int index = 0;
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				int i = (height - y - 1) * width + x;
				colors[index] = pixels[i].ToRaylib();
				index++;
			}
		}
		Raylib.UpdateTexture(rTexture, colors);
	}

	public static Int2 GetTextureSize (object texture) => texture is Texture2D rTexture ? new Int2(rTexture.Width, rTexture.Height) : default;

	public static object PngBytesToTexture (byte[] bytes) {
		if (bytes == null || bytes.Length == 0) return EMPTY_TEXTURE;
		var image = Raylib.LoadImageFromMemory(".png", bytes);
		var result = Raylib.LoadTextureFromImage(image);
		Raylib.SetTextureFilter(result, TextureFilter.Point);
		Raylib.UnloadImage(image);
		return result;
	}

	public static byte[] TextureToPngBytes (object texture) {
		if (texture is not Texture2D rTexture) return [];
		var fileType = Marshal.StringToHGlobalAnsi(".png");
		int fileSize = 0;
		var img = Raylib.LoadImageFromTexture(rTexture);
		char* result = Raylib.ExportImageToMemory(
			img,
			(sbyte*)fileType.ToPointer(),
			&fileSize
		);
		Raylib.UnloadImage(img);
		if (fileSize == 0) return [];
		var resultBytes = new byte[fileSize];
		Marshal.Copy((nint)result, resultBytes, 0, fileSize);
		Marshal.FreeHGlobal((nint)result);
		Marshal.FreeHGlobal(fileType);
		return resultBytes;
	}

	public static void UnloadTexture (object texture) {
		if (texture is not Texture2D rTexture || rTexture.Id == 0) return;
		if (Raylib.IsTextureReady(rTexture)) {
			Raylib.UnloadTexture(rTexture);
		}
	}

	public static uint? GetTextureID (object texture) => texture is Texture2D rTexture ? rTexture.Id : null;

	public static bool IsTextureReady (object texture) => texture is Texture2D rTexture && rTexture.Id != 0 && Raylib.IsTextureReady(rTexture);

	public static object GetResizedTexture (object texture, int newWidth, int newHeight, bool nearestNeighbor = true) {
		if (texture is not Texture2D rTexture) return null;
		var img = Raylib.LoadImageFromTexture(rTexture);
		if (!Raylib.IsImageReady(img)) return null;
		if (nearestNeighbor) {
			Raylib.ImageResizeNN(ref img, newWidth, newHeight);
		} else {
			Raylib.ImageResize(ref img, newWidth, newHeight);
		}
		if (!Raylib.IsImageReady(img)) return null;
		var result = Raylib.LoadTextureFromImage(img);
		Raylib.UnloadImage(img);
		if (!Raylib.IsTextureReady(result)) return null;
		return result;
	}

	public static void FillResizedTexture (object source, object target, bool nearestNeighbor = true) {
		if (source is not Texture2D rSource) return;
		if (target is not Texture2D rTarget) return;
		var img = Raylib.LoadImageFromTexture(rSource);
		if (!Raylib.IsImageReady(img)) return;
		if (nearestNeighbor) {
			Raylib.ImageResizeNN(ref img, rTarget.Width, rTarget.Height);
		} else {
			Raylib.ImageResize(ref img, rTarget.Width, rTarget.Height);
		}
		if (!Raylib.IsImageReady(img)) return;
		Raylib.UpdateTexture(rTarget, img.Data);
		Raylib.UnloadImage(img);
	}


	// LGC
	private static Texture2D? GetTextureFromPixelsLogic (Color32[] pixels, int width, int height) {
		int len = width * height;
		if (len == 0) return null;
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