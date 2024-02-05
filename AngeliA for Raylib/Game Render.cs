using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AngeliaFramework;
using Raylib_cs;

namespace AngeliaForRaylib;

public partial class GameForRaylib {


	// Data
	private Texture2D Texture;
	private IRect ScreenRect;


	// Render
	protected override void _OnRenderingLayerCreated (int index, string name, int sortingOrder, int capacity) { }

	protected override void _OnCameraUpdate () {
		ScreenRect = CameraScreenRect.x.AlmostZero() ?
			new IRect(0, 0, ScreenWidth, ScreenHeight) :
			new IRect(
				Util.LerpUnclamped(0, ScreenWidth, CameraScreenRect.x).RoundToInt(), 0,
				(ScreenWidth * CameraScreenRect.width).RoundToInt(), ScreenHeight
			);
	}

	protected override void _OnLayerUpdate (int layerIndex, bool isUiLayer, bool isTextLayer, Cell[] cells, int cellCount) {

		var cameraRect = CellRenderer.CameraRect;
		int cameraL = cameraRect.x;
		int cameraR = cameraRect.xMax;
		int cameraD = cameraRect.y;
		int cameraU = cameraRect.yMax;
		int screenL = ScreenRect.x;
		int screenR = ScreenRect.xMax;
		int screenD = ScreenRect.y;
		int screenU = ScreenRect.yMax;
		int textureWidth = Texture.Width;
		int textureHeight = Texture.Height;

		for (int i = 0; i < cellCount; i++) {

			var cell = cells[isUiLayer ? cellCount - i - 1 : i];

			if (!isTextLayer) {

				// Cell
				if (cell.Sprite == null || cell.Width == 0 || cell.Height == 0 || cell.Color.a == 0) continue;

				float pivotX = cell.Width > 0 ? cell.PivotX : 1f - cell.PivotX;
				float pivotY = cell.Height > 0 ? 1f - cell.PivotY : cell.PivotY;

				// UV
				float sourceL, sourceR, sourceD, sourceU;
				if (cell.BorderSide == Alignment.Full) {
					var uvRect = cell.Sprite.UvRect;
					sourceL = uvRect.x * textureWidth;
					sourceR = uvRect.xMax * textureWidth;
					sourceD = textureHeight - uvRect.yMax * textureHeight;
					sourceU = textureHeight - uvRect.y * textureHeight;
				} else {
					AngeUtil.GetSlicedUvBorder(cell.Sprite, cell.BorderSide, out var bl, out _, out _, out var tr);
					sourceL = bl.x * textureWidth;
					sourceR = tr.x * textureWidth;
					sourceD = textureHeight - tr.y * textureHeight;
					sourceU = textureHeight - bl.y * textureHeight;
				}
				var source = new Rectangle(
					sourceL,
					sourceD,
					sourceR - sourceL,
					sourceU - sourceD
				);

				// Pos
				var dest = new Rectangle(
					Util.RemapUnclamped(cameraL, cameraR, screenL, screenR, cell.X),
					Util.RemapUnclamped(cameraD, cameraU, screenU, screenD, cell.Y),
					cell.Width.Abs() * ScreenRect.width / (float)cameraRect.width,
					cell.Height.Abs() * ScreenRect.height / (float)cameraRect.height
				);

				// Shift
				if (true || !cell.Shift.IsZero) {

					if (cell.Shift.horizontal >= cell.Width.Abs()) continue;
					if (cell.Shift.vertical >= cell.Height.Abs()) continue;

					float shiftL = ((float)cell.Shift.left / cell.Width.Abs()).Clamp01();
					float shiftR = ((float)cell.Shift.right / cell.Width.Abs()).Clamp01();
					float shiftD = ((float)cell.Shift.down / cell.Height.Abs()).Clamp01();
					float shiftU = ((float)cell.Shift.up / cell.Height.Abs()).Clamp01();

					// Shift Dest/Source
					var newDest = dest;
					var newSource = source;
					// L
					if (cell.Width != 0) {
						float shift = dest.Width * shiftL;
						newDest.X -= cell.Width < 0 ? shift : 0;
						newDest.Width -= shift;
						shift = source.Width * shiftL;
						newSource.X += shift;
						newSource.Width -= shift;
					}

					// R
					if (cell.Width != 0) {
						float shift = dest.Width * shiftR;
						newDest.X += cell.Width < 0 ? shift : 0;
						newDest.Width -= shift;
						newSource.Width -= source.Width * shiftR;
					}

					// D
					if (cell.Height != 0) {
						float shift = dest.Height * shiftD;
						newDest.Y += cell.Height < 0 ? shift : 0;
						newDest.Height -= dest.Height * shiftD;
						newSource.Height -= source.Height * shiftD;
					}

					// U
					if (cell.Height != 0) {
						float shift = dest.Height * shiftU;
						newDest.Y -= cell.Height < 0 ? shift : 0;
						newDest.Height -= shift;
						shift = source.Height * shiftU;
						newSource.Y += shift;
						newSource.Height -= shift;
					}

					if (newDest.Width.AlmostZero() || newDest.Height.AlmostZero()) continue;

					// Shift Pivot
					pivotX = (pivotX - shiftL) * dest.Width / newDest.Width;
					pivotY = (pivotY - shiftU) * dest.Height / newDest.Height;
					dest = newDest;
					source = newSource;

				}

				source.Width *= cell.Width.Sign();
				source.Height *= cell.Height.Sign();

				// Draw
				Raylib.DrawTexturePro(
					Texture, source, dest,
					new(
						pivotX * dest.Width,
						pivotY * dest.Height
					), cell.Rotation, cell.Color.ToRaylib()
				);

			} else {
				// Text
				if (cell.TextSprite == null) continue;




			}

		}
	}

	protected override void _SetSkyboxTint (Byte4 top, Byte4 bottom) { }

	protected override void _SetTextureForRenderer (object texture) {
		if (texture == null) return;
		Texture = (Texture2D)texture;
		Raylib.SetTextureWrap(Texture, TextureWrap.Clamp);
	}


	// Effect
	protected override bool _GetEffectEnable (int effectIndex) {
		// TODO

		return false;
	}
	protected override void _SetEffectEnable (int effectIndex, bool enable) {
		// TODO
	}
	protected override void _Effect_SetDarkenParams (float amount, float step) {
		// TODO
	}
	protected override void _Effect_SetLightenParams (float amount, float step) {
		// TODO
	}
	protected override void _Effect_SetTintParams (Byte4 color) {
		// TODO
	}
	protected override void _Effect_SetVignetteParams (float radius, float feather, float offsetX, float offsetY, float round) {
		// TODO
	}


	// Texture
	protected override object _GetTextureFromPixels (Byte4[] pixels, int width, int height) {
		if (pixels == null || pixels.Length == 0) return null;
		unsafe {
			int len = pixels.Length;
			var bytes = new byte[pixels.Length * 4];
			for (int i = 0; i < bytes.Length; i += 4) {
				var p = pixels[i / 4];
				bytes[i + 0] = p.r;
				bytes[i + 1] = p.g;
				bytes[i + 2] = p.b;
				bytes[i + 3] = p.a;
			}
			fixed (void* data = bytes) {
				return Raylib.LoadTextureFromImage(new Image() {
					Data = data,
					Format = PixelFormat.UncompressedR8G8B8A8,
					Width = width,
					Height = height,
					Mipmaps = 1,
				});
			}
		}
	}

	protected override Byte4[] _GetPixelsFromTexture (object texture) {
		if (texture is not Texture2D rTexture) return System.Array.Empty<Byte4>();
		var image = Raylib.LoadImageFromTexture(rTexture);
		unsafe {
			int len = image.Width * image.Height;
			var result = new Byte4[len];
			var colors = Raylib.LoadImageColors(image);
			for (int i = 0; i < len; i++) {
				result[i] = colors[i].ToAngelia();
			}
			Raylib.UnloadImageColors(colors);
			return result;
		}
	}

	protected override void _FillPixelsIntoTexture (Byte4[] pixels, object texture) {
		if (texture is not Texture2D rTexture) return;
		Raylib.UpdateTexture(rTexture, pixels.ToRaylib());
	}

	protected override Int2 _GetTextureSize (object texture) => texture is Texture2D rTexture ? new Int2(rTexture.Width, rTexture.Height) : default;

	protected override object _PngBytesToTexture (byte[] bytes) {
		if (bytes == null || bytes.Length == 0) return null;
		var image = Raylib.LoadImageFromMemory(".png", bytes);
		return Raylib.LoadTextureFromImage(image);
	}

	protected override byte[] _TextureToPngBytes (object texture) {
		if (texture is not Texture2D rTexture) return System.Array.Empty<byte>();
		unsafe {
			var fileType = Marshal.StringToHGlobalAnsi(".png");
			int fileSize = 0;
			char* result = Raylib.ExportImageToMemory(
				Raylib.LoadImageFromTexture(rTexture),
				(sbyte*)fileType.ToPointer(),
				&fileSize
			);
			if (fileSize == 0) return System.Array.Empty<byte>();
			var resultBytes = new byte[fileSize];
			Marshal.Copy((nint)result, resultBytes, 0, fileSize);
			Marshal.FreeCoTaskMem((System.IntPtr)result);
			Marshal.FreeCoTaskMem(fileType);
			return resultBytes;
		}
	}


	// GL Gizmos
	protected override void _DrawGizmosRect (IRect rect, Byte4 color) {
		if (GLRectCount >= GLRects.Length) return;
		var glRect = GLRects[GLRectCount];
		glRect.RaylibRect = Angelia_to_Raylib_Rect(rect);
		glRect.Color = color.ToRaylib();
		GLRectCount++;
	}

	protected override void _DrawGizmosTexture (IRect rect, FRect uv, object texture) {
		if (texture is not Texture2D rTexture) return;
		if (GLTextureCount >= GLTextures.Length) return;
		var glTexture = GLTextures[GLTextureCount];
		glTexture.RaylibRect = Angelia_to_Raylib_Rect(rect);
		glTexture.Texture = rTexture;
		GLTextureCount++;
	}


	// Text
	protected override void _OnTextLayerCreated (int index, string name, int sortingOrder, int capacity) {

	}

	protected override int _GetTextLayerCount () => Fonts.Length;

	protected override string _GetTextLayerName (int index) => Fonts[index].Name;

	protected override int _GetFontSize (int index) => Fonts[index].Font.BaseSize;

	protected override CharSprite _FillCharSprite (int layerIndex, char c, int textSize, CharSprite charSprite, out bool filled) {

		//charSprite.GlobalID = c;


		filled = charSprite != null;
		return charSprite;
	}

	protected override void _RequestStringForFont (int layerIndex, int textSize, char[] content) { }
	protected override void _RequestStringForFont (int layerIndex, int textSize, string content) { }

	protected override string _GetClipboardText () => Raylib.GetClipboardText_();

	protected override void _SetClipboardText (string text) => Raylib.SetClipboardText(text);

	protected override void _SetImeCompositionMode (bool on) {

	}


}