using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngeliA;

public static partial class LanguageUtil {

	// Data
	private static readonly StringBuilder CacheBuilder = new();

	// Api
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

	public static string GetSystemLanguageISO () {
		string iso = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
		if (iso == "zh") {
			return CultureInfo.InstalledUICulture.ThreeLetterWindowsLanguageName == "CHT" ? "zht" : "zhs";
		}
		return iso;
	}

}