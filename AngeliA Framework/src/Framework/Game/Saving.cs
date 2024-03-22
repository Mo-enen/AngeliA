using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace AngeliA;


public static class SavingSystem {

	public static readonly Dictionary<int, (string key, string value)> Pool = new();
	private static readonly StringBuilder Builder = new();
	private static string SavingPath = "";
	public static bool FileLoaded = false;
	public static bool IsDirty = true;
	public static int PoolVersion = 0;


	[OnGameInitialize(int.MinValue + 1)]
	public static void OnGameInitialize () {
		SavingPath = Util.CombinePaths(UniverseSystem.CurrentUniverse.SavingMetaRoot, "Saving.txt");
		FileLoaded = false;
		LoadFromFile();
	}


	[OnUniverseOpen(int.MinValue + 1)]
	public static void OnUniverseOpen () {
		if (Game.GlobalFrame == 0) return;
		OnGameInitialize();
	}


	[OnGameQuitting]
	public static void OnGameQuitting () {
		if (FileLoaded && IsDirty) SaveToFile();
	}


	[OnGameUpdate]
	public static void OnGameUpdate () {
		if (!FileLoaded) LoadFromFile();
		if (IsDirty) SaveToFile();
	}


	public static void LoadFromFile () {
		FileLoaded = true;
		Pool.Clear();
		PoolVersion++;
		foreach (string line in Util.ForAllLines(SavingPath, Encoding.ASCII)) {
			int midIndex = line.IndexOf(':');
			if (midIndex <= 0 || midIndex > line.Length) continue;
			string key = line[..midIndex];
			string value = line[(midIndex + 1)..];
			Pool.TryAdd(key.AngeHash(), (key, value));
		}
	}


	public static void SaveToFile () {
		IsDirty = false;
		Builder.Clear();
		foreach (var (_, value) in Pool) {
			Builder.Append(value.key);
			Builder.Append(':');
			Builder.Append(value.value);
			Builder.Append('\n');
		}
		Util.TextToFile(Builder.ToString(), SavingPath, Encoding.ASCII);
	}

}


public abstract class Saving<T> {

	public string Key { get; init; }
	public int ID { get; init; }
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