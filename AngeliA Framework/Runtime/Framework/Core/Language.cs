using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Globalization;


namespace AngeliaFramework {


	[System.AttributeUsage(System.AttributeTargets.Method)]
	public class OnLanguageChangedAttribute : System.Attribute { }


	public static class Language {


		// Api
		public static int LanguageCount => AllLanguages.Length;
		public static string CurrentLanguage => _LoadedLanguage.Value;
		public static string CurrentLanguageDisplayName { get; private set; } = "";

		// Data
		private static event System.Action OnLanguageChanged;
		private static readonly Dictionary<int, string> Map = new();
		private static string[] AllLanguages = new string[0];

		// Saving
		private static readonly SavingString _LoadedLanguage = new("Game.Language", "");


		// API
		[OnGameInitialize(-128)]
		public static void Initialize () {

			Util.LinkEventWithAttribute<OnLanguageChangedAttribute>(typeof(Language), nameof(OnLanguageChanged));

			// Get All Language from Disk
			var allLanguages = new List<string>();
			foreach (var folderPath in Util.EnumerateFolders(AngePath.LanguageRoot, true, "*")) {
				allLanguages.Add(Util.GetNameWithoutExtension(folderPath));
			}
			AllLanguages = allLanguages.ToArray();

			// Load Current Language
			var targetLanguage = string.IsNullOrEmpty(_LoadedLanguage.Value) ?
				CultureInfo.InstalledUICulture.TwoLetterISOLanguageName :
				_LoadedLanguage.Value;
			if (!SetLanguage(targetLanguage)) {
				SetLanguage("en");
			}
		}


		public static string Get (int id, string failback = "") => Map.TryGetValue(id, out string value) ? value : failback;


		public static bool Has (int id) => Map.ContainsKey(id);


		public static string GetLanguageAt (int index) => AllLanguages[index];


		public static bool SetLanguage (string language) {
			if (LoadFromDisk(AngePath.LanguageRoot, language)) {
				_LoadedLanguage.Value = language;
				CurrentLanguageDisplayName = Util.GetLanguageDisplayName(language, out string disName) ? disName : language;
				OnLanguageChanged();
				return true;
			}
			return false;
		}


		// LGC
		private static bool LoadFromDisk (string languageRoot, string language) {
			string rootPath = Util.CombinePaths(languageRoot, language);
			Map.Clear();
			string key, value;
			foreach (var path in Util.EnumerateFiles(rootPath, true, $"*.{AngePath.LANGUAGE_FILE_EXT}")) {
				foreach (var line in Util.ForAllLines(path, Encoding.UTF8)) {
					if (string.IsNullOrWhiteSpace(line)) continue;
					int colon = line.IndexOf(':');
					if (colon <= 0) continue;
					key = line[..colon];
					value = colon + 1 < line.Length ? line[(colon + 1)..] : "";
					if (string.IsNullOrWhiteSpace(key)) continue;
					Map.TryAdd(key.AngeHash(), value.Replace("\\n", "\n"));
				}
			}
			return true;
		}


	}
}