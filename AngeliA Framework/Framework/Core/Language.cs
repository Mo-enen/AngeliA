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
			foreach (var path in Util.EnumerateFiles(AngePath.LanguageRoot, true, $"*.{AngePath.LANGUAGE_FILE_EXT}")) {
				allLanguages.Add(Util.GetNameWithoutExtension(path));
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


		public static string Get (int id, string failback = "") => Map.TryGetValue(id, out string value) && !string.IsNullOrEmpty(value) ? value : failback;
		public static string Get (LanguageCode code, string failback = "") => Map.TryGetValue(code.ID, out string value) && !string.IsNullOrEmpty(value) ? value : failback;

		// Error
		//public static string Get (SpriteCode code, string failback = "") => throw new System.Exception("Using sprite code for language.");


		public static string GetLanguageAt (int index) => AllLanguages[index];


		public static bool SetLanguage (string language) {
			if (LoadFromDisk(AngePath.LanguageRoot, language)) {
				_LoadedLanguage.Value = language;
				CurrentLanguageDisplayName = Util.TryGetLanguageDisplayName(language, out string disName) ? disName : language;
				OnLanguageChanged();
				return true;
			}
			return false;
		}


		private static bool LoadFromDisk (string languageRoot, string language) {
			Map.Clear();
			string key, value;
			foreach (var line in Util.ForAllLines(Util.CombinePaths(languageRoot, $"{language}.{AngePath.LANGUAGE_FILE_EXT}"), Encoding.UTF8)) {
				if (string.IsNullOrWhiteSpace(line)) continue;
				int colon = line.IndexOf(':');
				if (colon <= 0) continue;
				key = line[..colon];
				value = colon + 1 < line.Length ? line[(colon + 1)..] : "";
				if (string.IsNullOrWhiteSpace(key)) continue;
				Map.TryAdd(key.AngeHash(), value.Replace("\\n", "\n"));
			}
			return true;
		}


	}
}