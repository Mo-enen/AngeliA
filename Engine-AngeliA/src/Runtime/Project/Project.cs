using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

public class Project {

	// VAR
	public const string CS_PROJECT_NAME = "Project.csproj";
	public const string SOURCE_NAME = "src";
	public const string OBJECT_NAME = "obj";
	public const string BUILD_NAME = "Build";
	public bool IsValid => !string.IsNullOrEmpty(ProjectPath);
	public string ProjectPath { get; init; }
	public string SourceCodePath { get; init; }
	public string BuildPath { get; init; }
	public string ObjectPath { get; init; }
	public string CsProjectPath { get; init; }
	public string UniversePath { get; init; }
	public Universe Universe { get; init; }

	public Project (string projectPath) {
		ProjectPath = projectPath;
		SourceCodePath = Util.CombinePaths(projectPath, SOURCE_NAME);
		BuildPath = Util.CombinePaths(projectPath, BUILD_NAME);
		ObjectPath = Util.CombinePaths(projectPath, OBJECT_NAME);
		CsProjectPath = Util.CombinePaths(projectPath, CS_PROJECT_NAME);
		UniversePath = AngePath.GetUniverseRoot(projectPath);
		Universe = new Universe(projectPath, @readonly: false);
	}

	// API
	public static bool CreateProjectToDisk (string projectPath) {

		if (IsValidProjectPath(projectPath)) return false;

		Util.CreateFolder(projectPath);
		Util.CreateFolder(Util.CombinePaths(projectPath, SOURCE_NAME));
		Util.CreateFolder(Util.CombinePaths(projectPath, BUILD_NAME));
		Util.CreateFolder(Util.CombinePaths(projectPath, OBJECT_NAME));

		Util.CopyFolder(EngineUtil.UniverseTemplatePath, AngePath.GetUniverseRoot(projectPath), true, true);
		Util.TextToFile(EngineUtil.GenerateCsProjectFile(), Util.CombinePaths(projectPath, CS_PROJECT_NAME));

		return true;
	}

	public static bool IsValidProjectPath (string projectPath) => !string.IsNullOrEmpty(projectPath) && Util.FileExists(Util.CombinePaths(projectPath, CS_PROJECT_NAME));

}
