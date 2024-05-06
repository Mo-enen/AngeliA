using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using AngeliA;
using Task = System.Threading.Tasks.Task;

namespace AngeliaEngine;

public static class EngineUtil {


	// Const
	private const int ERROR_PROJECT_OBJECT_IS_NULL = -100;
	private const int ERROR_PROJECT_FOLDER_INVALID = -101;
	private const int ERROR_PUBLISH_DIR_INVALID = -102;
	private const int ERROR_PROJECT_FOLDER_NOT_EXISTS = -103;
	private const int ERROR_PRODUCT_NAME_INVALID = -104;
	private const int ERROR_DEV_NAME_INVALID = -105;
	private const int ERROR_RESULT_DLL_NOT_FOUND = -106;
	private const int ERROR_RUNTIME_FILE_NOT_FOUND = -107;
	private const int ERROR_UNIVERSE_FOLDER_NOT_FOUND = -108;
	private const int ERROR_EXE_FOR_RUN_NOT_FOUND = -109;
	private const int ERROR_DOTNET_SDK_NOT_FOUND = -110;
	private const int ERROR_ENTRY_PROJECT_NOT_FOUND = -111;
	private const int ERROR_ENTRY_RESULT_NOT_FOUND = -112;

	// Api
	public static string DotnetSdkPath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "dotnet", "dotnet.exe");
	public static string ProjectTemplatePath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "ProjectTemplate");
	public static string EntryExePath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Runtime", "Debug", "AngeliA Entry.exe");
	public static string RiggedExePath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Runtime", "Debug", "AngeliA Rigged.exe");
	public static string EntryProjectFolder => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Runtime", "Release");
	public static bool BuildingProjectInBackground => BuildProjectTask != null && BuildProjectTask.Status == TaskStatus.Running;
	public static long LastBackgroundBuildSrcModifyDate { get; private set; }
	public static int LastBackgroundBuildReturnCode { get; private set; }

	// Cache
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


	// API
	public static int BuildDotnetProject (
		string projectFolder, string sdkPath, bool publish, bool debug, bool logMessage, bool logError,
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

		return Util.ExecuteCommand(
			projectFolder, CacheBuilder.ToString(),
			logMessage: logMessage,
			logError: logError
		);
	}


	public static int BuildAngeliaProject (Project project, bool runAfterBuild) {
		if (project == null) return ERROR_PROJECT_OBJECT_IS_NULL;
		if (!Util.IsValidForFileName(project.Universe.Info.DeveloperName)) return ERROR_DEV_NAME_INVALID;
		var info = project.Universe.Info;
		string verStr = $"{info.MajorVersion}.{info.MinorVersion}.{info.PatchVersion}";
		return BuildAngeliaProjectLogic(
			project.ProjectPath, info.ProductName, project.BuildLibraryPath, verStr,
			project.TempBuildPath, project.BuildPath, project.TempPublishPath, project.TempRoot,
			project.IconPath, project.UniversePath,
			"", publish: false, runAfterBuild: runAfterBuild, logMessage: true, logError: true
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
			publishDir, publish: true, runAfterBuild: false, logMessage: true, logError: true
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
		LastBackgroundBuildSrcModifyDate = srcModifyDate;

		// Task
		BuildProjectTask = Task.Run(BuildFromCache);

		return true;

		// Func
		static void BuildFromCache () {
			LastBackgroundBuildReturnCode = BuildAngeliaProjectLogic(
				c_ProjectPath, c_ProductName, c_BuildLibraryPath, c_VersionString,
				c_TempBuildPath, c_BuildPath, "", c_TempRoot,
				"", c_UniversePath,
				"", publish: false, runAfterBuild: false,
				logMessage: false, logError: false
			);
		}
	}


	private static int BuildAngeliaProjectLogic (
		string projectPath, string productName, string buildLibraryPath, string versionStr,
		string tempBuildPath, string buildPath, string tempPublishPath, string tempRoot,
		string iconPath, string universePath,
		string publishDir, bool publish, bool runAfterBuild, bool logMessage, bool logError
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
			logMessage: logMessage,
			logError: logError
		);

		if (returnCode != 0) return returnCode;

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
				Util.ExecuteCommand(buildPath, exePath, logMessage: false, wait: false);
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
				logMessage: logMessage,
				logError: logError
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
					Util.ExecuteCommand(publishDir, exePath, logMessage: false, wait: false);
				} else {
					return ERROR_EXE_FOR_RUN_NOT_FOUND;
				}
			}

		}

		// Delete Temp Folder
		Util.DeleteFolder(tempRoot);

		return 0;
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


}