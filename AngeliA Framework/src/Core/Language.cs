using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public static class Language {




	#region --- VAR ---


	// Api
	public static int LanguageCount => AllLanguages.Length;
	public static string CurrentLanguage => _LoadedLanguage.Value;
	public static bool PoolReady { get; private set; } = false;

	// Data
	[OnLanguageChanged] internal static System.Action OnLanguageChanged;
	private static readonly Dictionary<int, string> Pool = [];
	private static string[] AllLanguages = [];

	// Saving
	private static readonly SavingString _LoadedLanguage = new("Game.Language", "", SavingLocation.Global);


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-128)]
	internal static TaskResult Initialize () {

		if (!SavingSystem.PoolReady) return TaskResult.Continue;

		// Get All Language from Disk
		var allLanguages = new List<string>();
		foreach (var path in Util.EnumerateFolders(Universe.BuiltIn.LanguageRoot, true)) {
			if (!Util.HasFileIn(path, true)) continue;
			allLanguages.Add(Util.GetNameWithoutExtension(path));
		}
		AllLanguages = [.. allLanguages];

		// Load Current Language
		var targetLanguage = string.IsNullOrEmpty(_LoadedLanguage.Value) ? LanguageUtil.GetSystemLanguageISO() : _LoadedLanguage.Value;
		if (!SetLanguage(targetLanguage)) {
			SetLanguage("en");
		}

		// End
		PoolReady = true;
		return TaskResult.End;
	}


#if DEBUG
	[OnGameFocused]
	internal static void OnGameFocused () => SetLanguage(CurrentLanguage);
#endif


	#endregion




	#region --- API ---


	public static string Get (int id, string failback = "") => Pool.TryGetValue(id, out string value) && !string.IsNullOrEmpty(value) ? value : failback;


	public static string GetLanguageAt (int index) => AllLanguages[index];


	public static bool SetLanguage (string language) {
		Pool.Clear();
		foreach (var (key, value) in LanguageUtil.LoadAllPairsFromFolder(Universe.BuiltIn.LanguageRoot, language)) {
			Pool.TryAdd(key.AngeHash(), value);
		}
		_LoadedLanguage.Value = language;
		OnLanguageChanged?.Invoke();
		return true;
	}


	#endregion




}