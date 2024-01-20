using System.Collections.Generic;
using System.Collections;
using System;


namespace AngeliaFramework {
	public static class AngePath {

		public const string COMBINATION_FILE_NAME = "Item Combination.txt";

		// Ext
		public const string MAP_FILE_EXT = "ibb";
		public const string CONVERSATION_FILE_EXT = "txt";
		public const string EDITABLE_CONVERSATION_FILE_EXT = "conversation";
		public const string LANGUAGE_FILE_EXT = "txt";
		public const string SHEET_FILE_EXT = "sheet";

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

		// Framework
		public static string WorkspaceRoot => _WorkspaceRoot ??= Util.CombinePaths(PersistentDataPath, "Workspace");
		private static string _WorkspaceRoot = null;

		public static string DownloadRoot => _DownloadRoot ??= Util.CombinePaths(PersistentDataPath, "Download");
		private static string _DownloadRoot = null;

		public static string ProjectTemplateRoot => _ProjectTemplateRoot ??= Util.CombinePaths(ApplicationDataPath, "Project Template");
		private static string _ProjectTemplateRoot = null;

		public static string BuiltInUniverseRoot => _BuiltInUniverseRoot ??= Util.CombinePaths(ApplicationDataPath, "Universe");
		private static string _BuiltInUniverseRoot = null;

		public static string BuiltInSavingRoot => _BuiltInSavingRoot ??= Util.CombinePaths(PersistentDataPath, "Built In Saving");
		private static string _BuiltInSavingRoot = null;


		// Temp
		public static string ProcedureMapTempRoot => _ProcedureMapTempRoot ??= Util.CombinePaths(TempDataPath, "Generating Map");
		private static string _ProcedureMapTempRoot = null;


	}
}
