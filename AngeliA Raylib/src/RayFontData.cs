using System;
using System.Collections;
using System.Collections.Generic;
using AngeliA;
using Raylib_cs;

namespace AngeliaRaylib;

public class RayFontData : FontData {

	private unsafe byte* Data = null;
	private int ByteSize = 0;

	public unsafe bool TryGetCharData (char c, out GlyphInfo info, out Texture2D texture) {

		info = default;
		texture = default;

		if (ByteSize == 0) return false;

		var data = Data;
		int dataSize = ByteSize;
		int charInt = c;
		var infoPtr = Raylib.LoadFontData(data, dataSize, Size, &charInt, 1, FontType.Default);
		if (infoPtr == null) return false;

		info = infoPtr[0];
		var img = info.Image;
		int imgLen = img.Width * img.Height;
		if (imgLen != 0) {
			Raylib.ImageFormat(ref img, PixelFormat.UncompressedR32G32B32A32);
			Raylib.ImageColorReplace(ref img, Color.Black, Color.Blank);
			texture = Raylib.LoadTextureFromImage(img);
			Raylib.SetTextureFilter(texture, TextureFilter.Bilinear);
			Raylib.SetTextureWrap(texture, TextureWrap.Clamp);
			return true;
		}
		return false;
	}

	public override bool TryGetCharSprite (char c, out CharSprite result) {
		result = null;
		if (!TryGetCharData(c, out var info, out var texture)) return true;
		float finalFontSize = Size / Scale;
		result = new CharSprite {
			Char = c,
			Advance = info.AdvanceX / finalFontSize,
			Offset = c == ' ' ? new FRect(0.5f, 0.5f, 0.001f, 0.001f) : FRect.MinMaxRect(
				xmin: info.OffsetX / finalFontSize,
				ymin: (finalFontSize - info.OffsetY - info.Image.Height) / finalFontSize,
				xmax: (info.OffsetX + info.Image.Width) / finalFontSize,
				ymax: (finalFontSize - info.OffsetY) / finalFontSize
			),
			Texture = texture,
		};
		return true;
	}

	public unsafe override void LoadData (string filePath) {
		uint fileSize = 0;
		Data = Raylib.LoadFileData(filePath, ref fileSize);
		ByteSize = (int)fileSize;
	}

	public unsafe override void Unload () {
		if (Data == null) return;
		try {
			Raylib.UnloadFileData(Data);
			Data = null;
			ByteSize = 0;
		} catch (Exception ex) { Debug.LogException(ex); }
	}

}