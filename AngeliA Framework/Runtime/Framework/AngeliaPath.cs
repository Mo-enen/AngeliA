using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public static class AngePath {

		public const string UNIVERSE_NAME = "Universe";
		public const string MANIFEST_NAME = "MANIFEST";
		public const int DATA_SLOT_COUNT = 4;

		// Universe
		public static string UniverseRoot => _UniverseRoot ??= Util.CombinePaths(Application.streamingAssetsPath, UNIVERSE_NAME);
		private static string _UniverseRoot = null;

		public static string SheetRoot => _SheetRoot ??= Util.CombinePaths(UniverseRoot, "Sheet");
		private static string _SheetRoot = null;

		public static string MetaRoot => _MetaRoot ??= Util.CombinePaths(UniverseRoot, "Meta");
		private static string _MetaRoot = null;

		public static string BuiltInMapRoot => _BuiltInMapRoot ??= Util.CombinePaths(UniverseRoot, "Map");
		private static string _BuiltInMapRoot = null;

		public static string LanguageRoot => _LanguageRoot ??= Util.CombinePaths(UniverseRoot, "Language");
		private static string _LanguageRoot = null;

		// Persistent
		public static int CurrentDataSlot {
			get => _CurrentDataSlot;
			internal set {
				if (value != _CurrentDataSlot) {
					_CurrentDataSlot = value;
					_UserDataRoot = null;
					_UserMapRoot = null;
					_ProcedureMapRoot = null;
					_DownloadMapRoot = null;
					_PlayerDataRoot = null;
				}
			}
		}
		private static int _CurrentDataSlot = 0;

		public static string UserDataRoot => _UserDataRoot ??= Util.CombinePaths(Application.persistentDataPath, $"Data Slot {CurrentDataSlot}");
		private static string _UserDataRoot = null;

		public static string UserMapRoot => _UserMapRoot ??= Util.CombinePaths(UserDataRoot, "User Map");
		private static string _UserMapRoot = null;

		public static string ProcedureMapRoot => _ProcedureMapRoot ??= Util.CombinePaths(UserDataRoot, "Procedure Map");
		private static string _ProcedureMapRoot = null;

		public static string DownloadMapRoot => _DownloadMapRoot ??= Util.CombinePaths(UserDataRoot, "Download Map");
		private static string _DownloadMapRoot = null;

		public static string PlayerDataRoot => _PlayerDataRoot ??= Util.CombinePaths(UserDataRoot, "Player Data");
		private static string _PlayerDataRoot = null;

	}
}
