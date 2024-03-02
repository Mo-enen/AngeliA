using System.Collections.Generic;
using System.Collections;
using System;

namespace AngeliA;

public static class AngePath {

	public const string COMBINATION_FILE_NAME = "Item Combination.txt";

	// Ext
	public const string MAP_FILE_EXT = "ibb";
	public const string CONVERSATION_FILE_EXT = "txt";
	public const string EDITABLE_CONVERSATION_FILE_EXT = "conversation";
	public const string LANGUAGE_FILE_EXT = "txt";
	public const string SHEET_FILE_EXT = "sheet";

	// System
	public static string ApplicationDataPath {
		get {
			if (!string.IsNullOrEmpty(_ApplicationDataPath)) return _ApplicationDataPath;
#if DEBUG
			string path = Util.GetParentPath(Environment.CurrentDirectory);
			for (int safe = 0; safe < 12; safe++) {
				foreach (var filePath in Util.EnumerateFiles(path, true, "*.csproj")) {
					return _ApplicationDataPath = path;
				}
				path = Util.GetParentPath(path);
				if (string.IsNullOrEmpty(path)) break;
			}
#endif
			return _ApplicationDataPath = Environment.CurrentDirectory;
		}
	}
	private static string _ApplicationDataPath = null;

	public static string PersistentDataPath => _PersistentDataPath ??= Util.CombinePaths(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
		Util.TryGetAttributeFromAllAssemblies<AngeliaGameDeveloperAttribute>(out var dev) ? dev.Developer : "(No Developer)",
		Util.TryGetAttributeFromAllAssemblies<AngeliaGameTitleAttribute>(out var tit) ? tit.Title : "(No Title)"
	);
	private static string _PersistentDataPath = null;

	public static string TempDataPath => _TempDataPath ??= Util.CombinePaths(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
		"Temp",
		Util.TryGetAttributeFromAllAssemblies<AngeliaGameDeveloperAttribute>(out var dev) ? dev.Developer : "(No Developer)",
		Util.TryGetAttributeFromAllAssemblies<AngeliaGameTitleAttribute>(out var tit) ? tit.Title : "(No Title)"
	);
	private static string _TempDataPath = null;

	// Framework
	public static string BuiltInUniverseRoot => _BuiltInUniverseRoot ??= Util.CombinePaths(ApplicationDataPath, "Universe");
	private static string _BuiltInUniverseRoot = null;

	public static string WorkspaceRoot => _WorkspaceRoot ??= Util.CombinePaths(PersistentDataPath, "Workspace");
	private static string _WorkspaceRoot = null;

	public static string DownloadRoot => _DownloadRoot ??= Util.CombinePaths(PersistentDataPath, "Download");
	private static string _DownloadRoot = null;

	public static string BuiltInSavingRoot => _BuiltInSavingRoot ??= Util.CombinePaths(PersistentDataPath, "Built In Saving");
	private static string _BuiltInSavingRoot = null;

	public static string LanguageRoot => _LanguageRoot ??= Util.CombinePaths(BuiltInUniverseRoot, "Language");
	private static string _LanguageRoot = null;

	// Temp
	public static string ProcedureMapTempRoot => _ProcedureMapTempRoot ??= Util.CombinePaths(TempDataPath, "Generating Map");
	private static string _ProcedureMapTempRoot = null;

	// Universe
	public static string GetUniverseRoot (string universeFolder) => Util.CombinePaths(universeFolder, "Universe");
	public static string GetSheetPath (string universeFolder) => Util.CombinePaths(universeFolder, "Sheet.sheet");
	public static string GetAtlasRoot (string universeFolder) => Util.CombinePaths(universeFolder, "Atlas");
	public static string GetConversationRoot (string universeFolder) => Util.CombinePaths(universeFolder, "Conversation");
	public static string GetEditableConversationRoot (string universeFolder) => Util.CombinePaths(universeFolder, "Editable Conversation");
	public static string GetUniverseMetaRoot (string universeFolder) => Util.CombinePaths(universeFolder, "Meta");
	public static string GetMapRoot (string universeFolder) => Util.CombinePaths(universeFolder, "Map");
	public static string GetItemCustomizationRoot (string savingFolder) => Util.CombinePaths(savingFolder, "Item Customization");
	public static string GetSavingMetaRoot (string savingFolder) => Util.CombinePaths(savingFolder, "Meta");
	public static string GetProcedureMapRoot (string savingFolder) => Util.CombinePaths(savingFolder, "Procedure Map");
	public static string GetUniverseCoverPath (string universeFolder) => Util.CombinePaths(universeFolder, "Cover.png");
	public static string GetAsepriteRoot (string universeFolder) => Util.CombinePaths(universeFolder, "Aseprite");

}
