using System.Collections;
using System.Collections.Generic;
using System.Text;
using AngeliA;

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
	public static string EntryProjectFolder => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Runtime", "Release");


	// Data
	private static readonly StringBuilder CacheBuilder = new();


	// API
	public static int BuildDotnetProject (
		string projectFolder, string sdkPath, bool publish, bool debug,
		string assemblyName = "", string version = "", string outputPath = "", string publishDir = "", string iconPath = ""
	) {

		if (!Util.FolderExists(projectFolder)) return ERROR_PROJECT_FOLDER_NOT_EXISTS;

		CacheBuilder.Clear();

		// SDK
		CacheBuilder.AppendWithDoubleQuotes(sdkPath);

		// Build
		CacheBuilder.Append(publish ? " publish " : " build ");

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

		// Execute
		return Util.ExecuteCommand(projectFolder, CacheBuilder.ToString());
	}


	public static int BuildAngeliaProject (Project project, bool runAfterBuild) => BuildAngeliaProjectLogic(project, "", false, runAfterBuild);
	public static int PublishAngeliaProject (Project project, string publishDir) => BuildAngeliaProjectLogic(project, publishDir, true, false);
	private static int BuildAngeliaProjectLogic (Project project, string publishDir, bool publish, bool runAfterBuild) {

		if (project == null) return ERROR_PROJECT_OBJECT_IS_NULL;
		if (!Util.IsPathValid(project.ProjectPath)) return ERROR_PROJECT_FOLDER_INVALID;
		if (publish && !Util.IsPathValid(publishDir)) return ERROR_PUBLISH_DIR_INVALID;
		if (publish && !Util.FolderExists(EntryProjectFolder)) return ERROR_ENTRY_PROJECT_NOT_FOUND;
		if (!Util.FolderExists(project.ProjectPath)) return ERROR_PROJECT_FOLDER_NOT_EXISTS;
		if (!Util.FileExists(DotnetSdkPath)) return ERROR_DOTNET_SDK_NOT_FOUND;
		if (!Util.IsValidForFileName(project.Universe.Info.ProductName)) return ERROR_PRODUCT_NAME_INVALID;
		if (!Util.IsValidForFileName(project.Universe.Info.DeveloperName)) return ERROR_DEV_NAME_INVALID;

		var info = project.Universe.Info;
		string libAssemblyName = $"lib.{info.ProductName}";

		// ===== Build =====

		// Delete Build Library Folder
		Util.DeleteFolder(Util.CombinePaths(project.BuildPath, "Library"));

		// Build Dotnet Project
		int returnCode = BuildDotnetProject(
			project.ProjectPath,
			DotnetSdkPath,
			publish: false,
			debug: !publish,
			assemblyName: libAssemblyName,
			version: $"{info.MajorVersion}.{info.MinorVersion}.{info.PatchVersion}",
			outputPath: project.TempBuildPath
		);

		if (returnCode != 0) return returnCode;

		string resultDllPath = Util.CombinePaths(project.TempBuildPath, $"{libAssemblyName}.dll");
		if (!Util.FileExists(resultDllPath)) return ERROR_RESULT_DLL_NOT_FOUND;

		// Result Dll to Lib Folder
		string gameLibDllName = Util.GetNameWithExtension(resultDllPath);
		string libFolderBuildPath = Util.CombinePaths(project.BuildPath, "Library");
		string gameLibBuildPath = Util.CombinePaths(libFolderBuildPath, gameLibDllName);
		Util.CreateFolder(libFolderBuildPath);
		Util.CopyFile(resultDllPath, gameLibBuildPath);

		// Copy Runtime into Build Folder
		string entrySourcePath = EntryExePath;
		string entryBuildName = Util.GetNameWithExtension(entrySourcePath);
		string entryBuildPath = Util.CombinePaths(project.BuildPath, entryBuildName);
		if (!Util.FileExists(entrySourcePath)) return ERROR_RUNTIME_FILE_NOT_FOUND;
		if (!Util.FileExists(entryBuildPath)) {
			Util.CopyFile(entrySourcePath, entryBuildPath);
		}

		// Run
		if (!publish && runAfterBuild) {
			string exePath = entryBuildPath;
			if (Util.FileExists(exePath)) {
				Util.ExecuteCommand(project.BuildPath, exePath, logMessage: false, wait: false);
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
				assemblyName: info.ProductName,
				version: $"{info.MajorVersion}.{info.MinorVersion}.{info.PatchVersion}",
				outputPath: project.TempBuildPath,
				publishDir: project.TempPublishPath,
				iconPath: project.IconPath
			);
			if (pubReturnCode != 0) return returnCode;

			// Move Result Exe to Publish Folder
			string resultExePath = Util.CombinePaths(project.TempPublishPath, $"{info.ProductName}.exe");
			if (!Util.FileExists(resultExePath)) return ERROR_ENTRY_RESULT_NOT_FOUND;
			string pubResultExePath = Util.CombinePaths(publishDir, $"{info.ProductName}.exe");
			Util.CopyFile(resultExePath, pubResultExePath);

			// Copy Game Libs to Publish Folder
			if (!Util.FileExists(gameLibBuildPath)) return ERROR_RESULT_DLL_NOT_FOUND;
			Util.CopyFile(gameLibBuildPath, Util.CombinePaths(publishDir, "Library", gameLibDllName));

			// Copy Universe to Publish Folder
			if (!Util.FolderExists(project.UniversePath)) return ERROR_UNIVERSE_FOLDER_NOT_FOUND;
			Util.CreateFolder(publishDir);
			string universePublishFolderPath = Util.CombinePaths(publishDir, "Universe");
			Util.CopyFolder(project.UniversePath, universePublishFolderPath, true, true);

			// Delete Temp Folder
			Util.DeleteFolder(project.TempRoot);

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

		return 0;
	}


}