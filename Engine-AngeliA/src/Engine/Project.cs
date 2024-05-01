using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

public class Project {

	public string ProjectPath { get; init; }
	public string SourceCodePath { get; init; }
	public string BuildPath { get; init; }
	public string BuildLibraryPath { get; init; }
	public string TempBuildPath { get; init; }
	public string TempPublishPath { get; init; }
	public string TempRoot { get; init; }
	public string UniversePath { get; init; }
	public string IconPath { get; init; }
	public Universe Universe { get; init; }

	public static Project LoadProject (string projectPath) => new() {
		ProjectPath = projectPath,
		SourceCodePath = Util.CombinePaths(projectPath, "src"),
		BuildPath = Util.CombinePaths(projectPath, "Build"),
		BuildLibraryPath = Util.CombinePaths(projectPath, "Build", "Library"),
		TempRoot = Util.CombinePaths(projectPath, "Temp"),
		TempBuildPath = Util.CombinePaths(projectPath, "Temp", "Build"),
		TempPublishPath = Util.CombinePaths(projectPath, "Temp", "Publish"),
		IconPath = Util.CombinePaths(projectPath, "Icon.ico"),
		UniversePath = AngePath.GetUniverseRoot(projectPath),
		Universe = Universe.LoadUniverse(AngePath.GetUniverseRoot(projectPath), @readonly: false),
	};

}