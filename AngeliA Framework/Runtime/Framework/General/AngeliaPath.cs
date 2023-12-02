using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public static class AngePath {

		public const string UNIVERSE_NAME = "Universe";
		public const string MANIFEST_NAME = "MANIFEST";
		public const int SAVE_SLOT_COUNT = 4;

		// Universe
		public static string UniverseRoot => _UniverseRoot ??= Util.CombinePaths(Application.streamingAssetsPath, UNIVERSE_NAME);
		private static string _UniverseRoot = null;

		public static string SheetRoot => _SheetRoot ??= Util.CombinePaths(UniverseRoot, "Sheet");
		private static string _SheetRoot = null;

		public static string DialogueRoot => _DialogueRoot ??= Util.CombinePaths(UniverseRoot, "Dialogue");
		private static string _DialogueRoot = null;

		public static string SheetTexturePath => _SheetTexturePath ??= Util.CombinePaths(SheetRoot, "Sheet.png");
		private static string _SheetTexturePath = null;

		public static string MetaRoot => _MetaRoot ??= Util.CombinePaths(UniverseRoot, "Meta");
		private static string _MetaRoot = null;

		public static string BuiltInMapRoot => _BuiltInMapRoot ??= Util.CombinePaths(UniverseRoot, "Map");
		private static string _BuiltInMapRoot = null;

		public static string LanguageRoot => _LanguageRoot ??= Util.CombinePaths(UniverseRoot, "Language");
		private static string _LanguageRoot = null;

		// Persistent
		public static int CurrentSaveSlot {
			get => _CurrentSaveSlot;
			internal set {
				if (value != _CurrentSaveSlot) {
					_CurrentSaveSlot = value;
					_SaveSlotRoot = null;
					_UserMapRoot = null;
					_ProcedureMapRoot = null;
					_DownloadMapRoot = null;
					_PlayerDataRoot = null;
				}
			}
		}
		private static int _CurrentSaveSlot = 0;

		public static string SaveSlotRoot => _SaveSlotRoot ??= Util.CombinePaths(Application.persistentDataPath, $"Save Slot {(char)(CurrentSaveSlot + 'A')}");
		private static string _SaveSlotRoot = null;

		public static string UserMapRoot => _UserMapRoot ??= Util.CombinePaths(SaveSlotRoot, "User Map");
		private static string _UserMapRoot = null;

		public static string PlayerDataRoot => _PlayerDataRoot ??= Util.CombinePaths(SaveSlotRoot, "Player Data");
		private static string _PlayerDataRoot = null;

		public static string ProcedureMapRoot => _ProcedureMapRoot ??= Util.CombinePaths(Application.persistentDataPath, "Procedure Map");
		private static string _ProcedureMapRoot = null;

		public static string DownloadMapRoot => _DownloadMapRoot ??= Util.CombinePaths(Application.persistentDataPath, "Download Map");
		private static string _DownloadMapRoot = null;

		// Temp
		public static string ProcedureMapTempRoot => _ProcedureMapTempRoot ??= Util.CombinePaths(Application.temporaryCachePath, "Generating Map");
		private static string _ProcedureMapTempRoot = null;


	}
}
