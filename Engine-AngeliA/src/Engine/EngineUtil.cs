using System.Collections;
using System.Collections.Generic;
using System.Text;
using AngeliA;

namespace AngeliaEngine;

public static class EngineUtil {


	public const int ERROR_PROJECT_OBJECT_IS_NULL = -2;
	public const int ERROR_PUBLISH_DIR_INVALID = -402;
	public const int ERROR_OUTPUT_PATH_INVALID = -403;
	public const int ERROR_PROJECT_FOLDER_INVALID = -401;
	public const int ERROR_PROJECT_FOLDER_NOT_EXISTS = -404;
	public static string DotnetSdkPath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "dotnet", "dotnet.exe");
	public static string ProjectTemplatePath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "ProjectTemplate");
	public static string DebugRuntimePath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Runtime", "AngeliA Runtime Debug.exe");
	public static string ReleaseRuntimePath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Runtime", "AngeliA Runtime Release.exe");

	private static readonly StringBuilder CacheBuilder = new();


	public static int BuildDotnetProject (
		string projectFolder, string sdkPath, bool publish, bool debug,
		string assemblyName = "", string version = "", string outputPath = "", string publishDir = "",
		string iconPath = ""
	) {

		if (!Util.FolderExists(projectFolder)) return -1;

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
		if (!string.IsNullOrWhiteSpace(iconPath)) {
			CacheBuilder.Append($" -p:ApplicationIcon=\"{iconPath}\"");
		}

		// Execute
		return Util.ExecuteCommand(projectFolder, CacheBuilder.ToString());
	}


	public static int BuildAngeliaProject (
		Project project, string outputPath, string publishDir, bool publish, bool debug
	) {
		if (project == null) return ERROR_PROJECT_OBJECT_IS_NULL;
		if (!Util.IsPathValid(project.ProjectPath)) return ERROR_PROJECT_FOLDER_INVALID;
		if (!Util.IsPathValid(outputPath)) return ERROR_OUTPUT_PATH_INVALID;
		if (!Util.IsPathValid(publishDir)) return ERROR_PUBLISH_DIR_INVALID;
		if (!Util.FolderExists(project.ProjectPath)) return ERROR_PROJECT_FOLDER_NOT_EXISTS;




		return 0;
	}


}