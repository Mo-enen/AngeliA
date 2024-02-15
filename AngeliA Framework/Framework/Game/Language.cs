using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Globalization;

[assembly: AngeliA.Framework.RequireGlobalLanguage("Language.Creator")]


namespace AngeliA.Framework; 


[System.AttributeUsage(System.AttributeTargets.Method)]
public class OnLanguageChangedAttribute : System.Attribute { }


public static class Language {


	// Api
	public static int LanguageCount => AllLanguages.Length;
	public static string CurrentLanguage => _LoadedLanguage.Value;

	// Data
	private static event System.Action OnLanguageChanged;
	private static readonly Dictionary<int, string> Pool = new();
	private static string[] AllLanguages = new string[0];

	// Saving
	private static readonly SavingString _LoadedLanguage = new("Game.Language", "");


	// API
	[OnGameInitialize(-128)]
	public static void Initialize () {

		Util.LinkEventWithAttribute<OnLanguageChangedAttribute>(typeof(Language), nameof(OnLanguageChanged));

		// Get All Language from Disk
		var allLanguages = new List<string>();
		foreach (var path in Util.EnumerateFiles(AngePath.LanguageRoot, true, $"*.{AngePath.LANGUAGE_FILE_EXT}")) {
			allLanguages.Add(Util.GetNameWithoutExtension(path));
		}
		AllLanguages = allLanguages.ToArray();

		// Load Current Language
		var targetLanguage = string.IsNullOrEmpty(_LoadedLanguage.Value) ? LanguageUtil.GetSystemLanguageISO() : _LoadedLanguage.Value;
		if (!SetLanguage(targetLanguage)) {
			SetLanguage("en");
		}

	}


	public static string Get (int id, string failback = "") => Pool.TryGetValue(id, out string value) && !string.IsNullOrEmpty(value) ? value : failback;


	public static string GetLanguageAt (int index) => AllLanguages[index];


	public static bool SetLanguage (string language) {
		Pool.Clear();
		string path = LanguageUtil.GetLanguageFilePath(AngePath.LanguageRoot, language);
		if (!Util.FileExists(path)) return false;
		foreach (var (key, value) in LanguageUtil.LoadAllPairsFromDiskAtPath(path)) {
			Pool.TryAdd(key.AngeHash(), value);
		}
		_LoadedLanguage.Value = language;
		OnLanguageChanged();
		return true;
	}


}