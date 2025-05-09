﻿using System;
using System.Collections.Generic;

namespace AngeliA;


// Font
public abstract class FontData {

	public int ID;
	public string Name;
	public string Path;
	public long FileModifyDate;
	public int Size = 42;
	public float Scale = 1f;
	public bool BuiltIn;

	public abstract bool TryGetCharSprite (char c, out CharSprite result);

	public abstract void LoadData (string filePath);

	public abstract void Unload ();

	public bool LoadFromFile (string fontPath, bool builtIn) {

		string name = Util.GetNameWithoutExtension(fontPath);
		Name = GetFontRealName(name);
		Path = fontPath;
		FileModifyDate = Util.GetFileModifyDate(fontPath);
		ID = Name.AngeHash();
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

	public static string GetFontRealName (string fontNameWithHashTag) {
		int hashIndex = fontNameWithHashTag.IndexOf('#');
		return (hashIndex >= 0 ? fontNameWithHashTag[..hashIndex] : fontNameWithHashTag).TrimStart_Numbers().TrimEnd_NumbersEmpty_();
	}

}


// Audio
public class MusicData : AudioData {

}


public class SoundData : AudioData {

	public object[] SoundObjects;
	public int[] StartFrames;
	public int LastPlayFrame = -1;

}


public abstract class AudioData {

	public int ID;
	public string Name;
	public string Path;

	[OnGameInitializeLater(4096)]
	internal static void OnGameInitializeLater () => Game.SyncAudioPool(Universe.BuiltIn.UniverseRoot);

}