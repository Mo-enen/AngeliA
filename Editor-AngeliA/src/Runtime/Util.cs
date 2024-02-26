using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using AngeliA;

namespace AngeliaEditor;

public static class EditorUtil {


	private const string RUNTIME_LOCATION = "[angelia] dotnet runtime location";
	private static readonly StringBuilder CacheBuilder = new();


	public static void BuildProject (
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
		CacheBuilder.Append(AngeliaVersionAttribute.GetVersionString(prefixV: false, lifeCycle: false));
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
		Util.ExecuteCommand(Util.GetParentPath(projectPath), CacheBuilder.ToString(), logMessage);
		CacheBuilder.Clear();

	}


	public static string GenerateProjectFile (string sdkPath) {

		// Find Runtime
		string runtimeRoot = string.Empty;
		foreach (string path in Util.EnumerateFiles(Util.GetParentPath(sdkPath), false, RUNTIME_LOCATION)) {
			runtimeRoot = Util.GetParentPath(path);
			break;
		}
		if (string.IsNullOrEmpty(runtimeRoot)) {
			Util.LogError($"Can not find dotnet runtime. it should be inside dotnet sdk folder: {sdkPath}");
			return string.Empty;
		}

		// Create Project File
		CacheBuilder.Clear();
		CacheBuilder.AppendLine("<Project>");

		CacheBuilder.AppendLine("\t<PropertyGroup>");
		CacheBuilder.AppendLine("\t\t<TargetFramework>net7.0-windows</TargetFramework>");
		CacheBuilder.AppendLine("\t</PropertyGroup>");

		// Add Dotnet Runtime Dll
		foreach (string path in Util.EnumerateFiles(runtimeRoot, true, "*.dll")) {
            CacheBuilder.AppendLine("\t<ItemGroup>");
            // Include
            CacheBuilder.Append("\t\t<Reference Include=\"");
            CacheBuilder.Append(Util.GetNameWithoutExtension(path));
            CacheBuilder.AppendLine("\">");
            // Hint Path
            CacheBuilder.Append("\t\t\t<HintPath>");
            CacheBuilder.Append(path);
            CacheBuilder.AppendLine("</HintPath>");
            // End
            CacheBuilder.AppendLine("\t\t</Reference>");
            CacheBuilder.AppendLine("\t</ItemGroup>");
		}

		CacheBuilder.AppendLine("</Project>");
		string result = CacheBuilder.ToString();
		CacheBuilder.Clear();
		return result;
	}


}