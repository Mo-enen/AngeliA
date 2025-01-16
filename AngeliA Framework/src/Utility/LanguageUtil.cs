using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AngeliA;

public static partial class LanguageUtil {

	public const int ADD_KEYS_FOR_ALL_LANGUAGE_CODE_SETTING_ID = 914528783;

	// Data
	private static readonly StringBuilder CacheBuilder = new();

	// Api
	public static IEnumerable<KeyValuePair<string, string>> LoadAllPairsFromFolder (string languageRoot, string language, bool keepEscapeCharacters = false) {
		string lanFolder = GetLanguageFolderPath(languageRoot, language);
		foreach (var filePath in Util.EnumerateFiles(lanFolder, true, AngePath.LANGUAGE_SEARCH_PATTERN)) {
			foreach (var pair in LoadAllPairsFromDiskAtPath(filePath, keepEscapeCharacters)) {
				yield return pair;
			}
		}
	}

	public static IEnumerable<KeyValuePair<string, string>> LoadAllPairsFromDiskAtPath (string path, bool keepEscapeCharacters = false) {
		foreach (var line in Util.ForAllLinesInFile(path, Encoding.UTF8)) {
			if (string.IsNullOrWhiteSpace(line)) continue;
			int colon = line.IndexOf(':');
			if (colon <= 0) continue;
			string key = line[..colon];
			if (string.IsNullOrWhiteSpace(key)) continue;
			string value = colon + 1 < line.Length ? line[(colon + 1)..] : "";
			yield return new(key, keepEscapeCharacters ? value : value.Replace("\\n", "\n"));
		}
	}

	public static void SaveAllPairsToDisk (string filePath, IEnumerable<KeyValuePair<string, string>> pairs) {
		CacheBuilder.Clear();
		foreach (var pair in pairs) {
			CacheBuilder.Append(pair.Key);
			CacheBuilder.Append(':');
			CacheBuilder.Append(pair.Value.Replace("\n", "\\n"));
			CacheBuilder.Append('\n');
		}
		Util.TextToFile(CacheBuilder.ToString(), filePath, Encoding.UTF8);
		CacheBuilder.Clear();
	}

	public static string GetLanguageFolderPath (string languageRoot, string language) => Util.CombinePaths(languageRoot, language);

	public static string GetSystemLanguageISO () {
		string iso = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
		if (iso == "zh") {
			return CultureInfo.InstalledUICulture.ThreeLetterWindowsLanguageName == "CHT" ? "zht" : "zhs";
		}
		return iso;
	}

	public static void AddKeysForAllLanguageCode (string languageRoot) {
		var fieldBinding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
		foreach (string folderPath in Util.EnumerateFolders(languageRoot, true)) {
			string lan = Util.GetNameWithoutExtension(folderPath);
			string filePath = Util.CombinePaths(folderPath, $"{lan}.{AngePath.LANGUAGE_FILE_EXT}");
			if (!Util.FileExists(filePath)) continue;
			var pairs = new List<KeyValuePair<string, string>>();
			pairs.AddRange(LoadAllPairsFromDiskAtPath(filePath));
			bool loadDef = lan == "en";
			// All Language Code
			foreach (var type in Util.GetAllTypeSpan()) {
				foreach (var lanCode in type.ForAllStaticFieldValue<LanguageCode>(fieldBinding, inherited: false)) {
					if (lanCode == null) continue;
					pairs.Add(new(lanCode.Name, loadDef ? lanCode.DefaultValue : ""));
				}
				foreach (var lanCodes in type.ForAllStaticFieldValue<LanguageCode[]>(fieldBinding, inherited: false)) {
					if (lanCodes == null || lanCodes.Length == 0) continue;
					foreach (var lanCode in lanCodes) {
						pairs.Add(new(lanCode.Name, loadDef ? lanCode.DefaultValue : ""));
					}
				}
			}
			// All Items
			foreach (var type in typeof(Item).AllChildClass()) {
				string angeName = type.AngeName();
				pairs.Add(new($"iName.{angeName}", loadDef ? Util.GetDisplayName(angeName) : ""));
				pairs.Add(new($"iDes.{angeName}", ""));
			}
			// All Buffs
			foreach (var type in typeof(Buff).AllChildClass()) {
				string angeName = type.AngeName();
				pairs.Add(new($"iName.{angeName}", loadDef ? Util.GetDisplayName(angeName) : ""));
				pairs.Add(new($"iDes.{angeName}", ""));
			}
			// Save to Disk
			SaveAllPairsToDisk(filePath, pairs);
		}
	}

}