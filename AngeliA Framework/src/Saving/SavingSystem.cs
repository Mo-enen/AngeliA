using System.Collections.Generic;
using System.Text;

namespace AngeliA;

/// <summary>
/// Core system for handle data that auto keeps inside disk.
/// </summary>
public static class SavingSystem {


	// SUB
	internal struct SavingLine (string key, string value, bool global) {
		public string Key = key;
		public string Value = value;
		public bool Global = global;
	}

	// Api
	internal static readonly Dictionary<int, SavingLine> Pool = [];
	/// <summary>
	/// True if the internal pool is loaded from file
	/// </summary>
	public static bool FileLoaded { get; private set; } = false;
	/// <summary>
	/// True if there's any unsaved change
	/// </summary>
	public static bool IsDirty { get; set; } = true;
	/// <summary>
	/// Data version of the internal pool
	/// </summary>
	public static int PoolVersion { get; private set; } = 0;
	/// <summary>
	/// True if the internal pool is ready to use
	/// </summary>
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


	[BeforeSavingSlotChanged]
	internal static void BeforeSavingSlotChanged () {
		FileLoaded = false;
		PoolReady = false;
	}


	[OnSavingSlotChanged]
	internal static void OnSavingSlotChanged () {
		SlotSavingPath = Util.CombinePaths(Universe.BuiltIn.SlotMetaRoot, "Saving.txt");
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
	/// <summary>
	/// True if the given key exists in the internal pool
	/// </summary>
	public static bool HasKey (Saving saving) => Pool.ContainsKey(saving.ID);


	/// <summary>
	/// True if the given key exists in the internal pool
	/// </summary>
	public static bool HasKey (int id) => Pool.ContainsKey(id);


	internal static void LoadFromFile () {
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


	internal static void SaveToFile () {
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
