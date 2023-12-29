using System;

namespace AngeliaFramework {
	public static class AngePath {


		public const string UNIVERSE_NAME = "Universe";
		public const string COMBINATION_FILE_NAME = "Item Combination.txt";
		public const int SAVE_SLOT_COUNT = 4;

		// Ext
		public const string MAP_FILE_EXT = "ibb";
		public const string CONVERSATION_FILE_EXT = "txt";
		public const string EDITABLE_CONVERSATION_FILE_EXT = "conversation";
		public const string LANGUAGE_FILE_EXT = "txt";

		// System
		public static string ApplicationDataPath => _ApplicationDataPath ??= Environment.CurrentDirectory;
		private static string _ApplicationDataPath = null;

		public static string PersistentDataPath => _PersistentDataPath ??= Util.CombinePaths(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			AngeliaGameDeveloperAttribute.GetDeveloper(),
			AngeliaGameTitleAttribute.GetTitle()
		);
		private static string _PersistentDataPath = null;

		public static string TempDataPath => _TempDataPath ??= Util.CombinePaths(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"Temp",
			AngeliaGameDeveloperAttribute.GetDeveloper(),
			AngeliaGameTitleAttribute.GetTitle()
		);
		private static string _TempDataPath = null;

		// Universe
		public static string UniverseRoot => _UniverseRoot ??= Util.CombinePaths(ApplicationDataPath, UNIVERSE_NAME);
		private static string _UniverseRoot = null;

		public static string SheetRoot => _SheetRoot ??= Util.CombinePaths(UniverseRoot, "Sheet");
		private static string _SheetRoot = null;

		public static string DialogueRoot => _DialogueRoot ??= Util.CombinePaths(UniverseRoot, "Dialogue");
		private static string _DialogueRoot = null;

		public static string SheetTexturePath => _SheetTexturePath ??= Util.CombinePaths(SheetRoot, "Texture.png");
		private static string _SheetTexturePath = null;

		public static string MetaRoot => _MetaRoot ??= Util.CombinePaths(UniverseRoot, "Meta");
		private static string _MetaRoot = null;

		public static string BuiltInMapRoot => _BuiltInMapRoot ??= Util.CombinePaths(UniverseRoot, "Map");
		private static string _BuiltInMapRoot = null;

		public static string LanguageRoot => _LanguageRoot ??= Util.CombinePaths(UniverseRoot, "Language");
		private static string _LanguageRoot = null;

		// Persistent
		public static string ItemSaveDataRoot => _ItemSaveDataRoot ??= Util.CombinePaths(PersistentDataPath, "Item Customization");
		private static string _ItemSaveDataRoot = null;

		// Slot
		public static int CurrentSaveSlot {
			get => _CurrentSaveSlot;
			internal set {
				if (value != _CurrentSaveSlot) {
					_CurrentSaveSlot = value;
					_SaveSlotRoot = null;
					_UserMapRoot = null;
					_UserDataRoot = null;
					_ProcedureMapRoot = null;
					_DownloadMapRoot = null;
				}
			}
		}
		private static int _CurrentSaveSlot = 0;

		public static string SaveSlotRoot => _SaveSlotRoot ??= Util.CombinePaths(
			PersistentDataPath,
			$"Save Slot {(char)(CurrentSaveSlot + 'A')}"
		);
		private static string _SaveSlotRoot = null;

		public static string UserDataRoot => _UserDataRoot ??= Util.CombinePaths(SaveSlotRoot, "Data");
		private static string _UserDataRoot = null;

		public static string UserMapRoot => _UserMapRoot ??= Util.CombinePaths(SaveSlotRoot, "Map", "User Map");
		private static string _UserMapRoot = null;

		public static string ProcedureMapRoot => _ProcedureMapRoot ??= Util.CombinePaths(SaveSlotRoot, "Map", "Procedure Map");
		private static string _ProcedureMapRoot = null;

		public static string DownloadMapRoot => _DownloadMapRoot ??= Util.CombinePaths(SaveSlotRoot, "Map", "Download Map");
		private static string _DownloadMapRoot = null;

		// Temp
		public static string ProcedureMapTempRoot => _ProcedureMapTempRoot ??= Util.CombinePaths(TempDataPath, "Generating Map");
		private static string _ProcedureMapTempRoot = null;


	}
}
