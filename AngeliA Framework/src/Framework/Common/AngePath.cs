using System.Collections.Generic;
using System.Collections;
using System;

namespace AngeliA;

public static class AngePath {

	// Ext  
	public const string MAP_FILE_EXT = "ibb";
	public const string CONVERSATION_FILE_EXT = "txt";
	public const string EDITABLE_CONVERSATION_FILE_EXT = "conversation";
	public const string LANGUAGE_FILE_EXT = "txt";
	public const string SHEET_FILE_EXT = "sheet";


	// System 
	public static string PersistentDataPath { get; private set; }
	public static string TempDataPath { get; private set; }

	// Framework
	public static string BuiltInUniverseRoot { get; internal set; }

	public static string WorkspaceRoot => _WorkspaceRoot ??= Util.CombinePaths(PersistentDataPath, "Workspace");
	private static string _WorkspaceRoot = null;

	public static string DownloadRoot => _DownloadRoot ??= Util.CombinePaths(PersistentDataPath, "Download");
	private static string _DownloadRoot = null;

	public static string LanguageRoot => _LanguageRoot ??= Util.CombinePaths(BuiltInUniverseRoot, "Language");
	private static string _LanguageRoot = null;


	// Temp
	public static string ProcedureMapTempRoot => _ProcedureMapTempRoot ??= Util.CombinePaths(TempDataPath, "Generating Map");
	private static string _ProcedureMapTempRoot = null;


	// Sys
	public static void SetCurrentUserPath (string devName, string productName) {
		PersistentDataPath = GetPersistentDataPath(devName, productName);
		TempDataPath = GetTempDataPath(devName, productName);
	}


	public static string GetPersistentDataPath (string devName, string productName) => Util.CombinePaths(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
		devName ?? "(No Developer)",
		productName ?? "(No Title)"
	);


	public static string GetTempDataPath (string devName, string productName) => Util.CombinePaths(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
		"Temp",
		devName ?? "(No Developer)",
		productName ?? "(No Title)"
	);


	// Universe
	public static string GetUniverseRoot (string projectFolder) => Util.CombinePaths(projectFolder, "Universe");
	public static string GetSheetPath (string universeFolder) => Util.CombinePaths(universeFolder, "Sheet.sheet");
	public static string GetAtlasRoot (string universeFolder) => Util.CombinePaths(universeFolder, "Atlas");
	public static string GetConversationRoot (string universeFolder) => Util.CombinePaths(universeFolder, "Conversation");
	public static string GetEditableConversationRoot (string universeFolder) => Util.CombinePaths(universeFolder, "Editable Conversation");
	public static string GetUniverseMetaRoot (string universeFolder) => Util.CombinePaths(universeFolder, "Meta");
	public static string GetUniverseMusicRoot (string universeFolder) => Util.CombinePaths(universeFolder, "Audio", "Music");
	public static string GetUniverseSoundRoot (string universeFolder) => Util.CombinePaths(universeFolder, "Audio", "Sound");
	public static string GetUniverseFontRoot (string universeFolder) => Util.CombinePaths(universeFolder, "Fonts");
	public static string GetMapRoot (string universeFolder) => Util.CombinePaths(universeFolder, "Map");
	public static string GetUniverseCoverPath (string universeFolder) => Util.CombinePaths(universeFolder, "Cover.png");
	public static string GetAsepriteRoot (string universeFolder) => Util.CombinePaths(universeFolder, "Aseprite");
	public static string GetLanguageRoot (string universeFolder) => Util.CombinePaths(universeFolder, "Language");
	public static string GetUniverseInfoPath (string universeFolder) => Util.CombinePaths(universeFolder, "Info.json");
	public static string GetCharacterAnimationRoot (string universeFolder) => Util.CombinePaths(universeFolder, "Meta", "Character Animation");
	public static string GetCharacterMovementConfigRoot (string universeFolder) => Util.CombinePaths(universeFolder, "Meta", "Character Movement");

	// Saving
	public static string GetSavingMetaRoot (string savingFolder) => Util.CombinePaths(savingFolder, "Meta");
	public static string GetSavingMetaCharacterConfigRoot (string savingFolder) => Util.CombinePaths(savingFolder, "Meta", "Character Rendering");
	public static string GetProcedureMapRoot (string savingFolder) => Util.CombinePaths(savingFolder, "Procedure Map");

}
