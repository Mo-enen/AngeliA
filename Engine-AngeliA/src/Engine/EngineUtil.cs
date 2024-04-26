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
	private const int ERROR_EXE_NOT_FOUND = -109;

	// Api
	public static string DotnetSdkPath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "dotnet", "dotnet.exe");
	public static string ProjectTemplatePath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "ProjectTemplate");
	public static string DebugRuntimePath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Runtime", "Debug", "AngeliA Runtime.exe");
	public static string ReleaseRuntimePath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Runtime", "Release", "AngeliA Runtime.exe");

	// Data
	private static readonly StringBuilder CacheBuilder = new();


	// API
	public static int BuildDotnetProject (
		string projectFolder, string sdkPath, bool publish, bool debug,
		string assemblyName = "", string version = "", string outputPath = "", string publishDir = ""
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

		// Execute
		return Util.ExecuteCommand(projectFolder, CacheBuilder.ToString());
	}


	public static int BuildAngeliaProject (Project project, bool runAfterBuild) => BuildAngeliaProjectLogic(project, "", false, runAfterBuild);
	public static int PublishAngeliaProject (Project project, string publishDir) => BuildAngeliaProjectLogic(project, publishDir, true, false);
	private static int BuildAngeliaProjectLogic (Project project, string publishDir, bool publish, bool runAfterBuild) {

		if (project == null) return ERROR_PROJECT_OBJECT_IS_NULL;
		if (!Util.IsPathValid(project.ProjectPath)) return ERROR_PROJECT_FOLDER_INVALID;
		if (publish && !Util.IsPathValid(publishDir)) return ERROR_PUBLISH_DIR_INVALID;
		if (!Util.FolderExists(project.ProjectPath)) return ERROR_PROJECT_FOLDER_NOT_EXISTS;
		if (!Util.IsValidForFileName(project.Universe.Info.ProductName)) return ERROR_PRODUCT_NAME_INVALID;
		if (!Util.IsValidForFileName(project.Universe.Info.DeveloperName)) return ERROR_DEV_NAME_INVALID;

		var info = project.Universe.Info;
		string assemblyName = $"lib.{info.ProductName}";

		// Delete Build Folder
		Util.DeleteFolder(project.BuildPath);

		// Build Dotnet Project
		int returnCode = BuildDotnetProject(
			project.ProjectPath, DotnetSdkPath,
			publish: publish,
			debug: !publish,
			assemblyName: assemblyName,
			version: $"{info.MajorVersion}.{info.MinorVersion}.{info.PatchVersion}",
			outputPath: project.BuildPath,
			publishDir: publishDir
		);

		if (returnCode != 0) return returnCode;

		string resultDllPath = Util.CombinePaths(project.BuildPath, $"{assemblyName}.dll");
		if (!Util.FileExists(resultDllPath)) return ERROR_RESULT_DLL_NOT_FOUND;

		// Result Dll to Lib Folder
		string resultDllName = Util.GetNameWithExtension(resultDllPath);
		string libFolderBuildPath = Util.CombinePaths(project.BuildPath, "Library");
		Util.CreateFolder(libFolderBuildPath);
		Util.CopyFile(resultDllPath, Util.CombinePaths(libFolderBuildPath, resultDllName));
		if (publish) {
			Util.CreateFolder(publishDir);
			string libFolderPubPath = Util.CombinePaths(publishDir, "Library");
			Util.CopyFile(resultDllPath, Util.CombinePaths(libFolderPubPath, resultDllName));
		}

		// Clean Result Folders
		foreach (var filePath in Util.EnumerateFiles(project.BuildPath, true, "*")) {
			Util.DeleteFile(filePath);
		}
		if (publish) {
			foreach (var filePath in Util.EnumerateFiles(publishDir, true, "*")) {
				Util.DeleteFile(filePath);
			}
		}

		// Copy Runtime into Result Folder
		string runtimeSourcePath = publish ? ReleaseRuntimePath : DebugRuntimePath;
		string runtimeBuildResultPath = Util.CombinePaths(project.BuildPath, $"{info.ProductName}.exe");
		string runtimePubResultPath = Util.CombinePaths(publishDir, $"{info.ProductName}.exe");
		if (!Util.FileExists(runtimeSourcePath)) return ERROR_RUNTIME_FILE_NOT_FOUND;
		Util.CopyFile(runtimeSourcePath, runtimeBuildResultPath);
		if (publish) {
			Util.CopyFile(runtimeSourcePath, runtimePubResultPath);
			//IconChanger.ChangeIcon(runtimePubResultPath, project.IconPath);
		}

		// Copy Universe into Publish Folder
		if (publish) {
			if (!Util.FolderExists(project.UniversePath)) return ERROR_UNIVERSE_FOLDER_NOT_FOUND;
			string universePublishFolderPath = Util.CombinePaths(publishDir, "Universe");
			Util.CopyFolder(project.UniversePath, universePublishFolderPath, true, true);
		}

		// Run
		if (runAfterBuild) {
			string exePath = publish ? runtimePubResultPath : runtimeBuildResultPath;
			if (Util.FileExists(exePath)) {
				Util.ExecuteCommand(project.BuildPath, exePath, logMessage: false, wait: false);
			} else {
				return ERROR_EXE_NOT_FOUND;
			}
		}

		return 0;
	}


}