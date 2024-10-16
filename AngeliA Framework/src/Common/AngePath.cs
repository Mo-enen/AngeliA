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
	public const string MAP_SEED_NAME = "Seed.txt";


	// System 
	public static string PersistentDataPath { get; private set; }
	public static string TempDataPath { get; private set; }
	public static string BuiltInUniverseRoot { get; internal set; }


	// Sys
	public static void SetCurrentUserPath (string devName, string productName) {
		PersistentDataPath = GetPersistentDataPath(devName, productName);
		TempDataPath = GetTempDataPath(devName, productName);
	}


	public static string GetPersistentDataPath (string devName, string productName) => Util.CombinePaths(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
		string.IsNullOrEmpty(devName) ? "(No Developer)" : devName,
		string.IsNullOrEmpty(productName) ? "(No Title)" : productName
	);


	public static string GetTempDataPath (string devName, string productName) => Util.CombinePaths(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
		"Temp",
		devName ?? "(No Developer)",
		productName ?? "(No Title)"
	);


	// Universe
	public static string GetUniverseRoot (string projectFolder) => Util.CombinePaths(projectFolder, "Universe");
	public static string GetSheetRoot (string universeFolder) => Util.CombinePaths(universeFolder, "Sheet");
	public static string GetBuiltInSheetPath (string universeFolder) => Util.CombinePaths(universeFolder, "Sheet", "Built-In Sheet.sheet");
	public static string GetGameSheetPath (string universeFolder) => Util.CombinePaths(universeFolder, "Sheet", "Game Sheet.sheet");
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
	public static string GetCharacterMovementConfigRoot (string universeFolder) => Util.CombinePaths(universeFolder, "Meta", "Character Movement");

	// Saving
	public static string GetSlotRoot (string savingFolder, int slot) => Util.CombinePaths(savingFolder, $"Slot {slot}");
	public static string GetSlotMetaRoot (string savingFolder, int slot) => Util.CombinePaths(savingFolder, $"Slot {slot}", "Meta");
	public static string GetSlotInventoryRoot (string savingFolder, int slot) => Util.CombinePaths(savingFolder, $"Slot {slot}", "Meta", "Inventory");
	public static string GetSlotMetaCharacterConfigRoot (string savingFolder, int slot) => Util.CombinePaths(savingFolder, $"Slot {slot}", "Meta", "Character Rendering");
	public static string GetSlotUserMapRoot (string savingFolder, int slot) => Util.CombinePaths(savingFolder, $"Slot {slot}", "User Map");

}
