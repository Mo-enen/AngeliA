using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using AngeliA;

namespace AngeliaEngine;

public static class EditorUtil {


	private static readonly StringBuilder CacheBuilder = new();


	public static int BuildProject (
		string projectPath, string sdkPath, string outputFolderPath,
		bool logMessage = true
	) {

		CacheBuilder.Clear();

		// SDK
		CacheBuilder.AppendWithDoubleQuotes(sdkPath);

		// Build
		CacheBuilder.Append(" build ");
		CacheBuilder.AppendWithDoubleQuotes(projectPath);

		// Property
		CacheBuilder.Append(" -p:Version=");
		//CacheBuilder.Append(AngeliaVersionAttribute.GetVersionString(prefixV: false));
		CacheBuilder.Append(" -p:OutputType=Library");
		CacheBuilder.Append(" -p:ProduceReferenceAssembly=false");
		CacheBuilder.Append(" -p:GenerateDependencyFile=false");
		CacheBuilder.Append(" -p:GenerateDocumentationFile=false");
		CacheBuilder.Append(" -p:DebugType=none");

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


	public static string GenerateProjectFile () {

		// Create Project File
		CacheBuilder.Clear();
		CacheBuilder.AppendLine("<Project>");

		CacheBuilder.AppendLine("\t<PropertyGroup>");
		CacheBuilder.AppendLine("\t\t<TargetFramework>net7.0-windows</TargetFramework>");
		CacheBuilder.AppendLine("\t</PropertyGroup>");

		CacheBuilder.AppendLine("</Project>");
		string result = CacheBuilder.ToString();
		CacheBuilder.Clear();
		return result;
	}


}