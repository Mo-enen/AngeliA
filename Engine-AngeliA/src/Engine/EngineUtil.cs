using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using AngeliA;
using Task = System.Threading.Tasks.Task;

namespace AngeliaEngine;

[System.AttributeUsage(System.AttributeTargets.Method)] public class OnProjectBuiltInBackgroundAttribute : OrderedAttribute { public OnProjectBuiltInBackgroundAttribute (int order = 0) : base(order) { } }

public static class EngineUtil {




	#region --- VAR ---


	// Const
	public const int ERROR_PROJECT_OBJECT_IS_NULL = -100;
	public const int ERROR_PROJECT_FOLDER_INVALID = -101;
	public const int ERROR_PUBLISH_DIR_INVALID = -102;
	public const int ERROR_PROJECT_FOLDER_NOT_EXISTS = -103;
	public const int ERROR_PRODUCT_NAME_INVALID = -104;
	public const int ERROR_DEV_NAME_INVALID = -105;
	public const int ERROR_RESULT_DLL_NOT_FOUND = -106;
	public const int ERROR_RUNTIME_FILE_NOT_FOUND = -107;
	public const int ERROR_UNIVERSE_FOLDER_NOT_FOUND = -108;
	public const int ERROR_EXE_FOR_RUN_NOT_FOUND = -109;
	public const int ERROR_DOTNET_SDK_NOT_FOUND = -110;
	public const int ERROR_ENTRY_PROJECT_NOT_FOUND = -111;
	public const int ERROR_ENTRY_RESULT_NOT_FOUND = -112;
	public const int ERROR_USER_CODE_COMPILE_ERROR = -113;
	private const int BACK_GROUND_BUILD_LOG_ID = 102735648;

	// Api
	public static string DotnetSdkPath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "dotnet", "dotnet.exe");
	public static string ProjectTemplatePath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "ProjectTemplate");
	public static string TemplateFrameworkDllFolder => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "ProjectTemplate", "lib");
	public static string EntryExePath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Runtime", "Debug", "AngeliA Entry.exe");
	public static string RiggedExePath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Runtime", "Debug", "AngeliA Rigged.exe");
	public static string EntryProjectFolder => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Runtime", "Release");
	public static bool BuildingProjectInBackground => BuildProjectTask != null && BuildProjectTask.Status == TaskStatus.Running;
	public static long LastBackgroundBuildModifyDate { get; private set; }
	public static int LastBackgroundBuildReturnCode { get; private set; }
	public static Queue<string> BackgroundBuildMessages { get; } = new(capacity: 32);

	// Cache
	private static event System.Action<int> OnProjectBuiltInBackgroundHandler;
	private static string c_ProjectPath;
	private static string c_ProductName;
	private static string c_BuildLibraryPath;
	private static string c_VersionString;
	private static string c_TempBuildPath;
	private static string c_BuildPath;
	private static string c_TempRoot;
	private static string c_UniversePath;


	// Data
	private static readonly StringBuilder CacheBuilder = new();
	private static Task BuildProjectTask = null;


	#endregion




	#region --- MSG ---


	[OnGameInitializeLater]
	internal static void OnGameInitializeLater () {
		Util.LinkEventWithAttribute<OnProjectBuiltInBackgroundAttribute>(typeof(EngineUtil), nameof(OnProjectBuiltInBackgroundHandler));
		Debug.OnLogInternal += OnLogMessage;
		Debug.OnLogErrorInternal += OnLogMessage;
	}


	#endregion




	#region --- API ---


	// Build
	public static int BuildAngeliaProject (Project project, bool runAfterBuild) {
		if (project == null) return ERROR_PROJECT_OBJECT_IS_NULL;
		if (!Util.IsValidForFileName(project.Universe.Info.DeveloperName)) return ERROR_DEV_NAME_INVALID;
		var info = project.Universe.Info;
		string verStr = $"{info.MajorVersion}.{info.MinorVersion}.{info.PatchVersion}";
		return BuildAngeliaProjectLogic(
			project.ProjectPath, info.ProductName, project.BuildLibraryPath, verStr,
			project.TempBuildPath, project.BuildPath, project.TempPublishPath, project.TempRoot,
			project.IconPath, project.UniversePath,
			"", publish: false, runAfterBuild: runAfterBuild, logID: 0
		);
	}


	public static int PublishAngeliaProject (Project project, string publishDir) {
		if (project == null) return ERROR_PROJECT_OBJECT_IS_NULL;
		if (!Util.IsValidForFileName(project.Universe.Info.DeveloperName)) return ERROR_DEV_NAME_INVALID;
		var info = project.Universe.Info;
		string verStr = $"{info.MajorVersion}.{info.MinorVersion}.{info.PatchVersion}";
		return BuildAngeliaProjectLogic(
			project.ProjectPath, info.ProductName, project.BuildLibraryPath, verStr,
			project.TempBuildPath, project.BuildPath, project.TempPublishPath, project.TempRoot,
			project.IconPath, project.UniversePath,
			publishDir, publish: true, runAfterBuild: false, logID: 0
		);
	}


	public static bool BuildAngeliaProjectInBackground (Project project, long srcModifyDate) {

		// Gate
		if (project == null) return false;
		if (!Util.IsValidForFileName(project.Universe.Info.DeveloperName)) return false;
		if (BuildingProjectInBackground) return false;

		// Project >> Cache
		var info = project.Universe.Info;
		c_ProjectPath = project.ProjectPath;
		c_ProductName = info.ProductName;
		c_VersionString = $"{info.MajorVersion}.{info.MinorVersion}.{info.PatchVersion}";
		c_BuildLibraryPath = project.BuildLibraryPath;
		c_TempBuildPath = project.TempBuildPath;
		c_BuildPath = project.BuildPath;
		c_TempRoot = project.TempRoot;
		c_UniversePath = project.UniversePath;
		LastBackgroundBuildReturnCode = int.MinValue;
		LastBackgroundBuildModifyDate = srcModifyDate;

		// Task
		BuildProjectTask = Task.Run(BuildFromCache);

		return true;

		// Func
		static void BuildFromCache () {
			try {
				BackgroundBuildMessages.Clear();
				LastBackgroundBuildReturnCode = BuildAngeliaProjectLogic(
					c_ProjectPath, c_ProductName, c_BuildLibraryPath, c_VersionString,
					c_TempBuildPath, c_BuildPath, "", c_TempRoot,
					"", c_UniversePath,
					"", publish: false, runAfterBuild: false
				);
				OnProjectBuiltInBackgroundHandler?.Invoke(LastBackgroundBuildReturnCode);
			} catch (System.Exception ex) {
				System.Console.WriteLine(ex.Message + "\n" + ex.Source);
			}
		}
	}


	// Modyfy Date
	public static long GetScriptModifyDate (Project project) {
		if (project == null || !Util.FolderExists(project.SourceCodePath)) return 0;
		long result = 0;
		foreach (var path in Util.EnumerateFiles(project.SourceCodePath, false, "*.cs")) {
			result = System.Math.Max(result, Util.GetFileModifyDate(path));
		}
		return result;
	}


	public static long GetBuildLibraryModifyDate (Project project) {
		if (project == null || !Util.FolderExists(project.BuildLibraryPath)) return 0;
		long result = 0;
		foreach (string path in Util.EnumerateFiles(project.BuildLibraryPath, true, "*.dll")) {
			result = System.Math.Max(result, Util.GetFileModifyDate(path));
		}
		return result;
	}


	#endregion




	#region --- LGC ---


	private static int BuildDotnetProject (
		string projectFolder, string sdkPath, bool publish, bool debug, int logID,
		string assemblyName = "", string version = "", string outputPath = "",
		string publishDir = "", string iconPath = ""
	) {

		if (!Util.FolderExists(projectFolder)) return ERROR_PROJECT_FOLDER_NOT_EXISTS;

		CacheBuilder.Clear();

		// SDK
		CacheBuilder.AppendWithDoubleQuotes(sdkPath);

		// Build
		CacheBuilder.Append(publish ? " publish " : " build ");

		// Project Folder
		CacheBuilder.AppendWithDoubleQuotes(projectFolder);

		// Config
		CacheBuilder.Append(debug ? " -c debug" : " -c release");

		// Prop
		if (!string.IsNullOrWhiteSpace(assemblyName)) {
			CacheBuilder.Append($" -p:AssemblyName=\"{assemblyName}\"");
		}
		if (!string.IsNullOrWhiteSpace(version)) {
			CacheBuilder.Append($" -p:Version={version}");
		}
		if (!string.IsNullOrWhiteSpace(outputPath)) {
			CacheBuilder.Append($" -p:OutputPath=\"{outputPath}\"");
		}
		if (!string.IsNullOrWhiteSpace(publishDir)) {
			CacheBuilder.Append($" -p:PublishDir=\"{publishDir}\"");
		}
		if (!string.IsNullOrWhiteSpace(iconPath) && Util.FileExists(iconPath)) {
			CacheBuilder.Append($" -p:ApplicationIcon=\"{iconPath}\"");
		}

		return Util.ExecuteCommand(projectFolder, CacheBuilder.ToString(), logID: logID);
	}


	private static int BuildAngeliaProjectLogic (
		string projectPath, string productName, string buildLibraryPath, string versionStr,
		string tempBuildPath, string buildPath, string tempPublishPath, string tempRoot,
		string iconPath, string universePath,
		string publishDir, bool publish, bool runAfterBuild, int logID = BACK_GROUND_BUILD_LOG_ID
	) {

		if (!Util.IsPathValid(projectPath)) return ERROR_PROJECT_FOLDER_INVALID;
		if (publish && !Util.IsPathValid(publishDir)) return ERROR_PUBLISH_DIR_INVALID;
		if (publish && !Util.FolderExists(EntryProjectFolder)) return ERROR_ENTRY_PROJECT_NOT_FOUND;
		if (!Util.FolderExists(projectPath)) return ERROR_PROJECT_FOLDER_NOT_EXISTS;
		if (!Util.FileExists(DotnetSdkPath)) return ERROR_DOTNET_SDK_NOT_FOUND;
		if (!Util.IsValidForFileName(productName)) return ERROR_PRODUCT_NAME_INVALID;

		string libAssemblyName = $"lib.{productName}";

		// ===== Build =====

		// Delete Build Library Folder
		Util.DeleteFolder(buildLibraryPath);

		// Build Dotnet Project
		int returnCode = BuildDotnetProject(
			projectPath,
			DotnetSdkPath,
			publish: false,
			debug: !publish,
			assemblyName: libAssemblyName,
			version: versionStr,
			outputPath: tempBuildPath,
			logID: logID
		);

		if (returnCode != 0) return ERROR_USER_CODE_COMPILE_ERROR;

		string resultDllPath = Util.CombinePaths(tempBuildPath, $"{libAssemblyName}.dll");
		if (!Util.FileExists(resultDllPath)) return ERROR_RESULT_DLL_NOT_FOUND;

		// Result Dll to Lib Folder
		string gameLibDllName = Util.GetNameWithExtension(resultDllPath);
		string gameLibBuildPath = Util.CombinePaths(buildLibraryPath, gameLibDllName);
		Util.CreateFolder(buildLibraryPath);
		Util.CopyFile(resultDllPath, gameLibBuildPath);

		// Copy Runtime into Build Folder
		string entrySourcePath = EntryExePath;
		string entryBuildName = Util.GetNameWithExtension(entrySourcePath);
		string entryBuildPath = Util.CombinePaths(buildPath, entryBuildName);
		if (!Util.FileExists(entrySourcePath)) return ERROR_RUNTIME_FILE_NOT_FOUND;
		if (!Util.FileExists(entryBuildPath)) {
			Util.CopyFile(entrySourcePath, entryBuildPath);
		}

		// Run
		if (!publish && runAfterBuild) {
			string exePath = entryBuildPath;
			if (Util.FileExists(exePath)) {
				Util.ExecuteCommand(buildPath, exePath, wait: false);
			} else {
				return ERROR_EXE_FOR_RUN_NOT_FOUND;
			}
		}

		// ===== Publish =====

		if (publish) {

			// Build Entry Exe for Publish
			int pubReturnCode = BuildDotnetProject(
				EntryProjectFolder,
				DotnetSdkPath,
				publish: true,
				debug: false,
				assemblyName: productName,
				version: versionStr,
				outputPath: tempBuildPath,
				publishDir: tempPublishPath,
				iconPath: iconPath,
				logID: -1
			);
			if (pubReturnCode != 0) return returnCode;

			// Move Result Exe to Publish Folder
			string resultExePath = Util.CombinePaths(tempPublishPath, $"{productName}.exe");
			if (!Util.FileExists(resultExePath)) return ERROR_ENTRY_RESULT_NOT_FOUND;
			string pubResultExePath = Util.CombinePaths(publishDir, $"{productName}.exe");
			Util.CopyFile(resultExePath, pubResultExePath);

			// Copy Game Libs to Publish Folder
			if (!Util.FileExists(gameLibBuildPath)) return ERROR_RESULT_DLL_NOT_FOUND;
			Util.CopyFile(gameLibBuildPath, Util.CombinePaths(publishDir, "Library", gameLibDllName));

			// Copy Universe to Publish Folder
			if (!Util.FolderExists(universePath)) return ERROR_UNIVERSE_FOLDER_NOT_FOUND;
			Util.CreateFolder(publishDir);
			string universePublishFolderPath = Util.CombinePaths(publishDir, "Universe");
			Util.CopyFolder(universePath, universePublishFolderPath, true, true);

			// Run
			if (runAfterBuild) {
				string exePath = pubResultExePath;
				if (Util.FileExists(exePath)) {
					Util.ExecuteCommand(publishDir, exePath, wait: false);
				} else {
					return ERROR_EXE_FOR_RUN_NOT_FOUND;
				}
			}

		}

		// Delete Temp Folder
		Util.DeleteFolder(tempRoot);

		return 0;
	}


	private static void OnLogMessage (int id, string message) {
		if (id != BACK_GROUND_BUILD_LOG_ID) return;
		BackgroundBuildMessages.Enqueue(message);
	}


	#endregion




}