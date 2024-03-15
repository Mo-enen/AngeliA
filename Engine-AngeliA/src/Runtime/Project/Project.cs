using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

public class Project {

	// VAR
	public const string CS_PROJECT_NAME = "Project.csproj";
	public bool IsValid => !string.IsNullOrEmpty(ProjectPath);
	public string ProjectPath { get; init; }
	public string CsProjectPath { get; init; }
	public string UniversePath { get; init; }
	public Universe Universe { get; init; }

	public Project (string projectPath) {
		ProjectPath = projectPath;
		CsProjectPath = Util.CombinePaths(projectPath, CS_PROJECT_NAME);
		UniversePath = AngePath.GetUniverseRoot(projectPath);
		Universe = new Universe(projectPath, @readonly: false);
	}

	// API
	public static bool CreateProjectToDisk (string projectPath) {

		if (IsValidProjectPath(projectPath)) return false;

		Util.CreateFolder(projectPath);

		Util.CopyFolder(EngineUtil.UniverseTemplatePath, AngePath.GetUniverseRoot(projectPath), true, true);

		Util.TextToFile(EngineUtil.GenerateCsProjectFile(), Util.CombinePaths(projectPath, CS_PROJECT_NAME));

		return true;
	}

	public static bool IsValidProjectPath (string projectPath) => !string.IsNullOrEmpty(projectPath) && Util.FileExists(Util.CombinePaths(projectPath, CS_PROJECT_NAME));

}
