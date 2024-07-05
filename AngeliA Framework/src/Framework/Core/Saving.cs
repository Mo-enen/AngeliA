using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace AngeliA;

public static class SavingSystem {


	// Api
	public static readonly Dictionary<int, (string key, string value)> Pool = new();
	public static bool FileLoaded { get; private set; } = false;
	public static bool IsDirty { get; set; } = true;
	public static int PoolVersion { get; private set; } = 0;
	public static bool PoolReady { get; private set; } = false;

	// Data
	private static readonly StringBuilder CacheBuilder = new();
	private static string SavingPath = "";


	// MSG
	[OnGameInitialize(int.MinValue + 1)]
	internal static void OnGameInitialize () {
		SavingPath = Util.CombinePaths(Universe.BuiltIn.SavingMetaRoot, "Saving.txt");
		FileLoaded = false;
		LoadFromFile();
		PoolReady = true;
	}


	[OnGameQuitting(4096)]
	internal static void OnGameQuitting () => SaveToFile();


	[OnGameUpdate]
	internal static void OnGameUpdate () {
		if (!FileLoaded) LoadFromFile();
		if (IsDirty) SaveToFile();
	}


	// API
	public static void LoadFromFile () {
		FileLoaded = true;
		Pool.Clear();
		PoolVersion++;
		foreach (string line in Util.ForAllLinesInFile(SavingPath, Encoding.Unicode)) {
			int midIndex = line.IndexOf(':');
			if (midIndex <= 0 || midIndex > line.Length) continue;
			string key = line[..midIndex];
			string value = line[(midIndex + 1)..];
			int id = key.AngeHash();
			Pool.TryAdd(id, (key, value));
		}
	}


	public static void SaveToFile () {
		IsDirty = false;
		CacheBuilder.Clear();
		if (!FileLoaded) return;
		foreach (var (_, value) in Pool) {
			CacheBuilder.Append(value.key);
			CacheBuilder.Append(':');
			CacheBuilder.Append(value.value);
			CacheBuilder.Append('\n');
		}
		Util.TextToFile(CacheBuilder.ToString(), SavingPath, Encoding.Unicode);
	}


}


public abstract class Saving {
	public string Key { get; init; }
	public int ID { get; init; }
}


public abstract class Saving<T> : Saving {

	public T Value {
		get {
			if (!SavingSystem.FileLoaded) {
				SavingSystem.LoadFromFile();
			}
			if (PoolVersion != SavingSystem.PoolVersion) {
				PoolVersion = SavingSystem.PoolVersion;
				if (SavingSystem.Pool.TryGetValue(ID, out var strValue)) {
					_Value = StringToValue(strValue.value);
				} else {
					_Value = DefaultValue;
				}
			}
			return _Value;
		}
		set {
			if (
				PoolVersion != SavingSystem.PoolVersion ||
				(_Value != null && !_Value.Equals(value)) ||
				(_Value == null && value != null)
			) {
				_Value = value;
				PoolVersion = SavingSystem.PoolVersion;
				SavingSystem.IsDirty = true;
				string newString = ValueToString(value);
				SavingSystem.Pool[ID] = (Key, newString);
			}
		}
	}
	public T DefaultValue { get; init; }
	private T _Value;
	private int PoolVersion;

	public Saving (string key, T defaultValue = default) {
		Key = key;
		ID = key.AngeHash();
		DefaultValue = defaultValue;
		_Value = defaultValue;
		PoolVersion = -1;
	}

	protected abstract T StringToValue (string str);
	protected abstract string ValueToString (T value);

}


public class SavingColor32 : Saving<Color32> {
	public SavingColor32 (string key, Color32 defaultValue = default) : base(key, defaultValue) { }
	protected override string ValueToString (Color32 value) => Util.ColorToInt(value).ToString();
	protected override Color32 StringToValue (string str) => int.TryParse(str, out int value) ? Util.IntToColor(value) : Color32.CLEAR;
}


public class SavingInt : Saving<int> {
	public SavingInt (string key, int defaultValue = default) : base(key, defaultValue) { }
	protected override string ValueToString (int value) => value.ToString();
	protected override int StringToValue (string str) => int.TryParse(str, out int value) ? value : 0;
}



public class SavingBool : Saving<bool> {
	public SavingBool (string key, bool defaultValue = default) : base(key, defaultValue) { }
	protected override string ValueToString (bool value) => value ? "1" : "0";
	protected override bool StringToValue (string str) => str == "1";
}



public class SavingString : Saving<string> {
	public SavingString (string key, string defaultValue = default) : base(key, defaultValue) { }
	protected override string ValueToString (string value) => value;
	protected override string StringToValue (string str) => str;
}


public class SavingHotkey : Saving<Hotkey> {
	public SavingHotkey (string key, Hotkey defaultValue = default) : base(key, defaultValue) { }
	protected override string ValueToString (Hotkey value) => value.GetStringData();
	protected override Hotkey StringToValue (string str) => new(str);
}