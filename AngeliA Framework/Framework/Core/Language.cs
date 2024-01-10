using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Globalization;


namespace AngeliaFramework {


	[System.AttributeUsage(System.AttributeTargets.Method)]
	public class OnLanguageChangedAttribute : System.Attribute { }


	[RequireGlobalLanguage("Language.Creator")]
	public static class Language {


		// Api
		public static int LanguageCount => AllLanguages.Length;
		public static int BuiltInLanguageCount => AllBuiltInLanguages.Length;
		public static int UserLanguageCount => AllUserLanguages.Length;
		public static string CurrentLanguage => _LoadedLanguage.Value;
		public static string CurrentLanguageDisplayName { get; private set; } = "";

		// Data
		private static event System.Action OnLanguageChanged;
		private static readonly Dictionary<int, string> Pool = new();
		private static readonly StringBuilder CacheBuilder = new();
		private static string[] AllLanguages = new string[0];
		private static string[] AllBuiltInLanguages = new string[0];
		private static string[] AllUserLanguages = new string[0];

		// Saving
		private static readonly SavingString _LoadedLanguage = new("Game.Language", "");


		// API
		[OnGameInitialize(-128)]
		public static void Initialize () {

			Util.LinkEventWithAttribute<OnLanguageChangedAttribute>(typeof(Language), nameof(OnLanguageChanged));

			// Get All Language from Disk
			var allBuiltInLanguages = new List<string>();
			var allUserLanguages = new List<string>();
			foreach (var path in Util.EnumerateFiles(AngePath.BuiltInLanguageRoot, true, $"*.{AngePath.LANGUAGE_FILE_EXT}")) {
				allBuiltInLanguages.Add(Util.GetNameWithoutExtension(path));
			}
			foreach (var path in Util.EnumerateFiles(AngePath.UserLanguageRoot, true, $"*.{AngePath.LANGUAGE_FILE_EXT}")) {
				allUserLanguages.Add(Util.GetNameWithoutExtension(path));
			}
			AllBuiltInLanguages = allBuiltInLanguages.ToArray();
			AllUserLanguages = allUserLanguages.ToArray();
			AllLanguages = allBuiltInLanguages.Concat(allUserLanguages).Distinct().ToArray();

			// Load Current Language
			var targetLanguage = string.IsNullOrEmpty(_LoadedLanguage.Value) ? GetSystemLanguageISO() : _LoadedLanguage.Value;
			if (!SetLanguage(targetLanguage)) {
				SetLanguage("en");
			}
		}


		public static string Get (int id, string failback = "") => Pool.TryGetValue(id, out string value) && !string.IsNullOrEmpty(value) ? value : failback;
		public static string Get (LanguageCode code, string failback = "") => Pool.TryGetValue(code.ID, out string value) && !string.IsNullOrEmpty(value) ? value : failback;

		// Error
		//public static string Get (SpriteCode code, string failback = "") => throw new System.Exception("Using sprite code for language.");


		public static string GetLanguageAt (int index) => AllLanguages[index];
		public static string GetBuiltInLanguageAt (int index) => AllBuiltInLanguages[index];
		public static string GetUserLanguageAt (int index) => AllUserLanguages[index];


		public static bool SetLanguage (string language) {
			Pool.Clear();
			foreach (var (key, value) in LoadAllPairsFromDisk(AngePath.UserLanguageRoot, language)) {
				Pool.TryAdd(key.AngeHash(), value);
			}
			foreach (var (key, value) in LoadAllPairsFromDisk(AngePath.BuiltInLanguageRoot, language)) {
				Pool.TryAdd(key.AngeHash(), value);
			}
			if (Pool.Count <= 0) return false;
			_LoadedLanguage.Value = language;
			CurrentLanguageDisplayName = Util.TryGetLanguageDisplayName(language, out string disName) ? disName : language;
			OnLanguageChanged();
			return true;
		}


		public static IEnumerable<KeyValuePair<string, string>> LoadAllPairsFromDisk (string languageRoot, string language) {
			string path = GetLanguageFilePath(languageRoot, language);
			foreach (var line in Util.ForAllLines(path, Encoding.UTF8)) {
				if (string.IsNullOrWhiteSpace(line)) continue;
				int colon = line.IndexOf(':');
				if (colon <= 0) continue;
				string key = line[..colon];
				if (string.IsNullOrWhiteSpace(key)) continue;
				string value = colon + 1 < line.Length ? line[(colon + 1)..] : "";
				yield return new(key, value.Replace("\\n", "\n"));
			}
		}


		public static void SaveAllPairsToDisk (string languageRoot, string language, IEnumerable<KeyValuePair<string, string>> pairs) {
			string path = GetLanguageFilePath(languageRoot, language);
			CacheBuilder.Clear();
			foreach (var pair in pairs) {
				CacheBuilder.Append(pair.Key);
				CacheBuilder.Append(':');
				CacheBuilder.Append(pair.Value);
				CacheBuilder.Append('\n');
			}
			Util.TextToFile(CacheBuilder.ToString(), path, Encoding.UTF8);
			CacheBuilder.Clear();
		}


		public static string GetLanguageFilePath (string languageRoot, string language) => Util.CombinePaths(languageRoot, $"{language}.{AngePath.LANGUAGE_FILE_EXT}");


		private static string GetSystemLanguageISO () {
			string iso = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
			if (iso == "zh") {
				return CultureInfo.InstalledUICulture.ThreeLetterWindowsLanguageName == "CHT" ? "zht" : "zhs";
			}
			return iso;
		}


	}
}