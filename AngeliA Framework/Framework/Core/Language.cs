using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Globalization;

[assembly: AngeliaFramework.RequireGlobalLanguage("Language.Creator")]


namespace AngeliaFramework {


	[System.AttributeUsage(System.AttributeTargets.Method)]
	public class OnLanguageChangedAttribute : System.Attribute { }


	public static class Language {


		// Api
		public static int LanguageCount => AllLanguages.Length;
		public static string CurrentLanguage => _LoadedLanguage.Value;

		// Data
		private static event System.Action OnLanguageChanged;
		private static readonly Dictionary<int, string> Pool = new();
		private static readonly StringBuilder CacheBuilder = new();
		private static string[] AllLanguages = new string[0];

		// Saving
		private static readonly SavingString _LoadedLanguage = new("Game.Language", "");


		// API
		[OnGameInitialize(-128)]
		public static void Initialize () => Util.LinkEventWithAttribute<OnLanguageChangedAttribute>(typeof(Language), nameof(OnLanguageChanged));


		[OnProjectOpen]
		public static void OnProjectOpen () {

			// Get All Language from Disk
			var allLanguages = new List<string>();
			foreach (var path in Util.EnumerateFiles(ProjectSystem.CurrentProject.LanguageRoot, true, $"*.{AngePath.LANGUAGE_FILE_EXT}")) {
				allLanguages.Add(Util.GetNameWithoutExtension(path));
			}
			AllLanguages = allLanguages.ToArray();

			// Load Current Language
			var targetLanguage = string.IsNullOrEmpty(_LoadedLanguage.Value) ? GetSystemLanguageISO() : _LoadedLanguage.Value;
			if (!SetLanguage(targetLanguage)) {
				SetLanguage("en");
			}
		}


		public static string Get (int id, string failback = "") => Pool.TryGetValue(id, out string value) && !string.IsNullOrEmpty(value) ? value : failback;


		public static string GetLanguageAt (int index) => AllLanguages[index];


		public static bool SetLanguage (string language) {
			Pool.Clear();
			string path = GetLanguageFilePath(ProjectSystem.CurrentProject.LanguageRoot, language);
			if (!Util.FileExists(path)) return false;
			foreach (var (key, value) in LoadAllPairsFromDiskAtPath(path)) {
				Pool.TryAdd(key.AngeHash(), value);
			}
			_LoadedLanguage.Value = language;
			OnLanguageChanged();
			return true;
		}


		public static IEnumerable<KeyValuePair<string, string>> LoadAllPairsFromDisk (string languageRoot, string language) => LoadAllPairsFromDiskAtPath(GetLanguageFilePath(languageRoot, language));
		public static IEnumerable<KeyValuePair<string, string>> LoadAllPairsFromDiskAtPath (string path) {
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