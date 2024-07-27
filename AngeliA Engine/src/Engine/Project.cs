using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AngeliA;

namespace AngeliaEngine;

public class Project {

	public string ProjectPath { get; init; }
	public string SourceCodePath { get; init; }
	public string BuildPath { get; init; }
	public string FrameworkDllPath_Debug { get; init; }
	public string FrameworkDllPath_Release { get; init; }
	public string TempBuildPath { get; init; }
	public string TempPublishPath { get; init; }
	public string TempRoot { get; init; }
	public string UniversePath { get; init; }
	public string IconPath { get; init; }
	public string CsprojPath { get; init; }
	public Universe Universe { get; init; }

	public static Project LoadProject (string projectPath) => new() {
		ProjectPath = projectPath,
		SourceCodePath = Util.CombinePaths(projectPath, "src"),
		BuildPath = Util.CombinePaths(projectPath, "Build"),
		FrameworkDllPath_Debug = Util.CombinePaths(projectPath, "lib", "Debug", "AngeliA Framework.dll"),
		FrameworkDllPath_Release = Util.CombinePaths(projectPath, "lib", "Release", "AngeliA Framework.dll"),
		TempRoot = Util.CombinePaths(projectPath, "Temp"),
		TempBuildPath = Util.CombinePaths(projectPath, "Temp", "Build"),
		TempPublishPath = Util.CombinePaths(projectPath, "Temp", "Publish"),
		IconPath = Util.CombinePaths(projectPath, "Icon.ico"),
		UniversePath = AngePath.GetUniverseRoot(projectPath),
		CsprojPath = Util.EnumerateFiles(projectPath, true, "*.csproj").FirstOrDefault(
			path => !path.Contains("#ignore", System.StringComparison.OrdinalIgnoreCase),
			defaultValue: ""
		),
		Universe = Universe.LoadFromFile(AngePath.GetUniverseRoot(projectPath)),
	};


}
