using System;
using System.Collections.Generic;

namespace AngeliA;

public abstract class FontData {

	public string Name;
	public string FilePath;
	public long FileModifyDate;
	public int LocalLayerIndex;
	public int Size = 42;
	public float Scale = 1f;
	public bool BuiltIn;

	public abstract bool TryGetCharSprite (char c, out CharSprite result);

	public abstract void LoadData (string filePath);

	public abstract void Unload ();

	public bool LoadFromFile (string fontPath, bool builtIn) {

		string name = Util.GetNameWithoutExtension(fontPath);
		if (!Util.TryGetIntFromString(name, 0, out int layerIndex, out _)) return false;

		int hashIndex = name.IndexOf('#');

		Name = (hashIndex >= 0 ? name[..hashIndex] : name).TrimStart_Numbers();
		FilePath = fontPath;
		FileModifyDate = Util.GetFileModifyDate(fontPath);
		LocalLayerIndex = layerIndex;
		Size = 42;
		Scale = 1f;
		BuiltIn = builtIn;

		// Size
		int sizeTagIndex = name.IndexOf("#size=", StringComparison.OrdinalIgnoreCase);
		if (sizeTagIndex >= 0 && Util.TryGetIntFromString(name, sizeTagIndex + 6, out int size, out _)) {
			Size = Util.Max(42, size);
		}

		// Scale
		int scaleTagIndex = name.IndexOf("#scale=", StringComparison.OrdinalIgnoreCase);
		if (scaleTagIndex >= 0 && Util.TryGetIntFromString(name, scaleTagIndex + 7, out int scale, out _)) {
			Scale = (scale / 100f).Clamp(0.01f, 10f);
		}

		// Data
		LoadData(fontPath);

		return true;
	}

}