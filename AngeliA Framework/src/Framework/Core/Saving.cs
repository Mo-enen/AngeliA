using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AngeliA;


public enum SavingLocation { Global, Slot, }


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
				if (SavingSystem.Pool.TryGetValue(ID, out var line)) {
					_Value = StringToValue(line.Value);
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
				SavingSystem.Pool[ID] = new SavingSystem.SavingLine(Key, newString, Location == SavingLocation.Global);
			}
		}
	}
	public T DefaultValue { get; init; }
	private SavingLocation Location { get; init; }
	private T _Value;
	private int PoolVersion;

	public Saving (string key, T defaultValue, SavingLocation location) {
		Key = key;
		ID = key.AngeHash();
		DefaultValue = defaultValue;
		_Value = defaultValue;
		PoolVersion = -1;
		Location = location;
	}

	protected abstract T StringToValue (string str);
	protected abstract string ValueToString (T value);

}


public static class SavingSystem {


	// SUB
	internal struct SavingLine {
		public string Key;
		public string Value;
		public bool Global;
		public SavingLine (string key, string value, bool global) {
			Key = key;
			Value = value;
			Global = global;
		}
	}

	// Api
	internal static readonly Dictionary<int, SavingLine> Pool = new();
	public static bool FileLoaded { get; private set; } = false;
	public static bool IsDirty { get; set; } = true;
	public static int PoolVersion { get; private set; } = 0;
	public static bool PoolReady { get; private set; } = false;

	// Data
	private static readonly StringBuilder SlotCacheBuilder = new();
	private static readonly StringBuilder GlobalCacheBuilder = new();
	private static string SlotSavingPath = "";
	private static string GlobalSavingPath = "";


	// MSG
	[OnGameInitialize(int.MinValue + 1)]
	internal static void OnGameInitialize () {
		SlotSavingPath = Util.CombinePaths(Universe.BuiltIn.SlotMetaRoot, "Saving.txt");
		GlobalSavingPath = Util.CombinePaths(Universe.BuiltIn.SavingRoot, "Saving.txt");
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
		// Slot
		foreach (string line in Util.ForAllLinesInFile(SlotSavingPath, Encoding.UTF8)) {
			int midIndex = line.IndexOf(':');
			if (midIndex <= 0 || midIndex > line.Length) continue;
			string key = line[..midIndex];
			string value = line[(midIndex + 1)..];
			int id = key.AngeHash();
			Pool.TryAdd(id, new SavingLine(key, value, false));
		}
		// Global
		foreach (string line in Util.ForAllLinesInFile(GlobalSavingPath, Encoding.UTF8)) {
			int midIndex = line.IndexOf(':');
			if (midIndex <= 0 || midIndex > line.Length) continue;
			string key = line[..midIndex];
			string value = line[(midIndex + 1)..];
			int id = key.AngeHash();
			Pool.TryAdd(id, new SavingLine(key, value, true));
		}
	}


	public static void SaveToFile () {
		IsDirty = false;
		SlotCacheBuilder.Clear();
		GlobalCacheBuilder.Clear();
		if (!FileLoaded) return;
		foreach (var (_, line) in Pool) {
			var builder = line.Global ? GlobalCacheBuilder : SlotCacheBuilder;
			builder.Append(line.Key);
			builder.Append(':');
			builder.Append(line.Value);
			builder.Append('\n');
		}
		Util.TextToFile(SlotCacheBuilder.ToString(), SlotSavingPath, Encoding.UTF8);
		Util.TextToFile(GlobalCacheBuilder.ToString(), GlobalSavingPath, Encoding.UTF8);
	}


}
