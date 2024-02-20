using System.Collections;
using System.Collections.Generic;
using Raylib_cs;

namespace AngeliaToRaylib;

public class FontData {

	private readonly Dictionary<char, (Image image, Texture2D texture)> Pool = new();
	private unsafe byte* PrioritizedData = null;
	private unsafe byte* FullsetData = null;
	private int PrioritizedByteSize = 0;
	private int FullsetByteSize = 0;

	public string Name;
	public int LayerIndex;
	public int Size;
	public float Scale = 1f;

	public unsafe void LoadData (string filePath, bool isPrioritized) {
		uint fileSize = 0;
		byte* fileData = Raylib.LoadFileData(filePath, ref fileSize);
		if (isPrioritized) {
			PrioritizedData = fileData;
			PrioritizedByteSize = (int)fileSize;
		} else {
			FullsetData = fileData;
			FullsetByteSize = (int)fileSize;
		}
	}

	public unsafe bool TryGetCharData (char c, out GlyphInfo info, out Texture2D texture) {

		info = default;
		texture = default;

		if (PrioritizedByteSize == 0 && FullsetByteSize == 0) return false;

		bool usingFullset = FullsetByteSize != 0 && (int)c >= 256;
		var data = usingFullset ? FullsetData : PrioritizedData;
		int dataSize = usingFullset ? FullsetByteSize : PrioritizedByteSize;
		int charInt = c;
		var infoPtr = Raylib.LoadFontData(data, dataSize, Size, &charInt, 1, FontType.Default);
		if (infoPtr == null) return false;

		info = infoPtr[0];
		var img = info.Image;
		if (img.Width * img.Height != 0) {
			texture = Raylib.LoadTextureFromImage(img);
			Raylib.SetTextureFilter(texture, TextureFilter.Bilinear);
			Pool.TryAdd(c, (img, texture));
			return true;
		}
		return false;
	}

	public void Unload () {
		foreach (var (_, (image, texture)) in Pool) {
			Raylib.UnloadImage(image);
			Raylib.UnloadTexture(texture);
		}
	}

	public bool TryGetTexture (char c, out Texture2D texture) {
		if (Pool.TryGetValue(c, out var result)) {
			texture = result.texture;
			return true;
		} else {
			texture = default;
			return false;
		}
	}

}