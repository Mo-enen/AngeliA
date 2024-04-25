using System;
using System.Collections;
using System.Collections.Generic;
using AngeliA;
using Raylib_cs;

namespace AngeliaRuntime;

public class FontData {

	private readonly Dictionary<char, (Image image, Texture2D texture)> Pool = new();
	private unsafe byte* PrioritizedData = null;
	private unsafe byte* FullsetData = null;
	private int PrioritizedByteSize = 0;
	private int FullsetByteSize = 0;

	public Texture2D? this[char c] => Pool.TryGetValue(c, out var result) ? result.texture : null;
	public string Name;
	public int LayerIndex;
	public int PrioritizedSize;
	public float PrioritizedScale = 1f;
	public int FullsetSize;
	public float FullsetScale = 1f;

	// API
	public static FontData[] LoadFromFile (string fontRoot) {
		var fontList = new List<FontData>(8);
		foreach (var fontPath in Util.EnumerateFiles(fontRoot, true, "*.ttf")) {
			string name = Util.GetNameWithoutExtension(fontPath);
			if (!Util.TryGetIntFromString(name, 0, out int layerIndex, out _)) continue;
			var targetData = fontList.Find(data => data.LayerIndex == layerIndex);
			if (targetData == null) {
				int hashIndex = name.IndexOf('#');
				fontList.Add(targetData = new FontData() {
					Name = (hashIndex >= 0 ? name[..hashIndex] : name).TrimStart_Numbers(),
					LayerIndex = layerIndex,
					FullsetSize = 42,
					FullsetScale = 1f,
					PrioritizedSize = 42,
					PrioritizedScale = 1f,
				});
			}
			bool isFullset = name.Contains("#fullset", StringComparison.OrdinalIgnoreCase);
			// Size
			int sizeTagIndex = name.IndexOf("#size=", StringComparison.OrdinalIgnoreCase);
			if (sizeTagIndex >= 0 && Util.TryGetIntFromString(name, sizeTagIndex + 6, out int size, out _)) {
				if (isFullset) {
					targetData.FullsetSize = Util.Max(42, size);
				} else {
					targetData.PrioritizedSize = Util.Max(42, size);
				}
			}
			// Scale
			int scaleTagIndex = name.IndexOf("#scale=", StringComparison.OrdinalIgnoreCase);
			if (scaleTagIndex >= 0 && Util.TryGetIntFromString(name, scaleTagIndex + 7, out int scale, out _)) {
				if (isFullset) {
					targetData.FullsetScale = (scale / 100f).Clamp(0.01f, 10f);
				} else {
					targetData.PrioritizedScale = (scale / 100f).Clamp(0.01f, 10f);
				}
			}
			// Data
			targetData.LoadData(fontPath, !isFullset);
		}
		fontList.Sort((a, b) => a.LayerIndex.CompareTo(b.LayerIndex));
		return fontList.ToArray();
	}

	public unsafe bool TryGetCharData (char c, out GlyphInfo info, out Texture2D texture) {

		info = default;
		texture = default;

		if (PrioritizedByteSize == 0 && FullsetByteSize == 0) return false;

		bool usingFullset = FullsetByteSize != 0 && c >= 256;
		var data = usingFullset ? FullsetData : PrioritizedData;
		int dataSize = usingFullset ? FullsetByteSize : PrioritizedByteSize;
		int charInt = c;
		var infoPtr = Raylib.LoadFontData(
			data, dataSize, usingFullset ? FullsetSize : PrioritizedSize, &charInt, 1, FontType.Default
		);
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
			Pool.TryAdd(c, (img, texture));
			return true;
		}
		return false;
	}

	// LGC
	private unsafe void LoadData (string filePath, bool isPrioritized) {
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

	~FontData () {
		foreach (var (_, (image, texture)) in Pool) {
			Raylib.UnloadImage(image);
			Raylib.UnloadTexture(texture);
		}
	}


}