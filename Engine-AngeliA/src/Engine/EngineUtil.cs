using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using AngeliA;

namespace AngeliaEngine;

public static class EngineUtil {


	public static string DotnetSdkPath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "dotnet", "dotnet.exe");
	public static string ProjectTemplatePath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "ProjectTemplate");
	public static string DebugRuntimePath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Runtime", "AngeliA Runtime Debug.exe");
	public static string ReleaseRuntimePath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Runtime", "AngeliA Runtime Release.exe");

	private static readonly StringBuilder CacheBuilder = new();


	public static int BuildDotnetProject (
		string projectFolder, string sdkPath, bool publish, bool debug,
		string assemblyName = "", string version = "", string outputPath = "", string publishDir = ""
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

		// Execute
		return Util.ExecuteCommand(projectFolder, CacheBuilder.ToString());
	}


	public static int BuildAngeliaProject (string projectFolder, string outputPath, string publishDir) {





		return 0;
	}


}