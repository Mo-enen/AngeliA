using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public static class Language {


	// Api
	public static int LanguageCount => AllLanguages.Length;
	public static string CurrentLanguage => _LoadedLanguage.Value;
	public static bool PoolReady { get; private set; } = false;

	// Data
	private static event System.Action OnLanguageChanged;
	private static readonly Dictionary<int, string> Pool = [];
	private static string[] AllLanguages = [];

	// Saving
	private static readonly SavingString _LoadedLanguage = new("Game.Language", "", SavingLocation.Global);


	// API
	[OnGameInitialize(-128)]
	public static TaskResult Initialize () {

		if (!SavingSystem.PoolReady) return TaskResult.Continue;

		Util.LinkEventWithAttribute<OnLanguageChangedAttribute>(typeof(Language), nameof(OnLanguageChanged));

		// Get All Language from Disk
		var allLanguages = new List<string>();
		foreach (var path in Util.EnumerateFiles(Universe.BuiltIn.LanguageRoot, true, $"*.{AngePath.LANGUAGE_FILE_EXT}")) {
			allLanguages.Add(Util.GetNameWithoutExtension(path));
		}
		AllLanguages = allLanguages.ToArray();

		// Load Current Language
		var targetLanguage = string.IsNullOrEmpty(_LoadedLanguage.Value) ? LanguageUtil.GetSystemLanguageISO() : _LoadedLanguage.Value;
		if (!SetLanguage(targetLanguage)) {
			SetLanguage("en");
		}

		// End
		PoolReady = true;
		return TaskResult.End;
	}


	public static string Get (int id, string failback = "") => Pool.TryGetValue(id, out string value) && !string.IsNullOrEmpty(value) ? value : failback;


	public static string GetLanguageAt (int index) => AllLanguages[index];


	public static bool SetLanguage (string language) {
		Pool.Clear();
		string path = LanguageUtil.GetLanguageFilePath(Universe.BuiltIn.LanguageRoot, language);
		if (!Util.FileExists(path)) return false;
		foreach (var (key, value) in LanguageUtil.LoadAllPairsFromDiskAtPath(path)) {
			Pool.TryAdd(key.AngeHash(), value);
		}
		_LoadedLanguage.Value = language;
		OnLanguageChanged?.Invoke();
		return true;
	}


}