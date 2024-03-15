using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using AngeliA;
using AngeliA.Framework;

namespace AngeliaEngine;

public static class EngineUtil {


	public static string UniverseTemplatePath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, UNIVERSE_TEMPLATE_NAME);
	private const string UNIVERSE_TEMPLATE_NAME = "UniverseTemplate";
	private static readonly StringBuilder CacheBuilder = new();


	// API
	public static int BuildProject (
		string projectPath, string sdkPath, string outputFolderPath,
		string assemblyName,
		bool logMessage = true, string versionString = ""
	) {

		CacheBuilder.Clear();

		// SDK
		CacheBuilder.AppendWithDoubleQuotes(sdkPath);

		// Build
		CacheBuilder.Append(" build ");
		CacheBuilder.AppendWithDoubleQuotes(projectPath);

		// Property
		if (!string.IsNullOrEmpty(versionString)) CacheBuilder.Append($" -p:Version={versionString}");
		CacheBuilder.Append(" -p:OutputType=Library");
		CacheBuilder.Append(" -p:ProduceReferenceAssembly=false");
		CacheBuilder.Append(" -p:GenerateDependencyFile=false");
		CacheBuilder.Append(" -p:GenerateDocumentationFile=false");
		CacheBuilder.Append(" -p:DebugType=none");
		CacheBuilder.Append($" -p:AssemblyName={assemblyName}");

		// Dep
		CacheBuilder.Append(" --no-dependencies");

		// Self
		CacheBuilder.Append(" --self-contained");

		// Output
		CacheBuilder.Append(" -o ");
		CacheBuilder.AppendWithDoubleQuotes(outputFolderPath);

		// Architecture
		CacheBuilder.Append(" -a x64");

		// Release
		CacheBuilder.Append(" -c release");

		// Execute
		int exitCode = Util.ExecuteCommand(Util.GetParentPath(projectPath), CacheBuilder.ToString(), logMessage);
		CacheBuilder.Clear();
		return exitCode;
	}


	public static string GenerateCsProjectFile () {

		// Create Project File
		CacheBuilder.Clear();
		CacheBuilder.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");

		CacheBuilder.AppendLine("\t<PropertyGroup>");
		CacheBuilder.AppendLine("\t\t<TargetFramework>net7.0-windows</TargetFramework>");
		CacheBuilder.AppendLine("\t</PropertyGroup>");

		CacheBuilder.AppendLine("</Project>");
		string result = CacheBuilder.ToString();
		CacheBuilder.Clear();
		return result;
	}




}