using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace AngeliaFramework {
	public static class Language {


		// Api
		public static int LanguageCount => AllLanguages.Length;
		public static SystemLanguage CurrentLanguage => (SystemLanguage)_LanguageID.Value;

		// Data
		private static readonly Dictionary<int, string> Map = new();
		private static SystemLanguage[] AllLanguages = new SystemLanguage[0];

		// Saving
		private static readonly SavingInt _LanguageID = new("Game.LanguageID", -1);


		// API
		[OnGameInitialize(-128)]
		public static void Initialize () {

			var allLanguages = new List<SystemLanguage>();
			foreach (var filePath in Util.EnumerateFiles(AngePath.LanguageRoot, true, $"*.{Const.LANGUAGE_FILE_EXT}")) {
				if (System.Enum.TryParse<SystemLanguage>(
					Util.GetNameWithoutExtension(filePath),
					out var language)
				) {
					allLanguages.Add(language);
				}
			}
			AllLanguages = allLanguages.ToArray();

			// Load Current Language
			var targetLanguage = _LanguageID.Value < 0 ? Application.systemLanguage : (SystemLanguage)_LanguageID.Value;
			if (!SetLanguage(targetLanguage)) {
				// Failback
				switch (targetLanguage) {
					case SystemLanguage.Chinese:
					case SystemLanguage.ChineseTraditional:
						if (!SetLanguage(SystemLanguage.ChineseSimplified)) {
							SetLanguage(SystemLanguage.English);
						}
						break;
					case SystemLanguage.ChineseSimplified:
						if (!SetLanguage(SystemLanguage.ChineseTraditional)) {
							SetLanguage(SystemLanguage.English);
						}
						break;
					default:
						SetLanguage(SystemLanguage.English);
						break;
				}
			}
		}


		public static string Get (int id, string failback = "") => Map.TryGetValue(id, out string value) ? value : failback;


		public static bool Has (int id) => Map.ContainsKey(id);


		public static SystemLanguage GetLanguageAt (int index) => AllLanguages[index];


		public static bool SetLanguage (SystemLanguage language) {
			if (LoadFromDisk(AngePath.LanguageRoot, language)) {
				_LanguageID.Value = (int)language;
				return true;
			}
			return false;
		}


		// LGC
		private static bool LoadFromDisk (string languageRoot, SystemLanguage language) {
			string path = Util.CombinePaths(languageRoot, $"{language}.{Const.LANGUAGE_FILE_EXT}");
			if (!Util.FileExists(path)) return false;
			Map.Clear();
			string key, value;
			foreach (var line in Util.ForAllLines(path, Encoding.UTF8)) {
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