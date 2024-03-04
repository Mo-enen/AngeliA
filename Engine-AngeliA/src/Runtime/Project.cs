using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;


[System.Serializable]
public class ProjectInfo {
	public string ProjectName = "";
	public string Developer = "";
}


public class Project {


	private const string CS_PROJECT_NAME = "Project.csproj";

	public bool IsValid => !string.IsNullOrEmpty(ProjectPath);
	public string ProjectPath { get; init; }
	public string CsProjectPath { get; init; }
	public string UniversePath { get; init; }
	public string InfoPath { get; init; }
	public ProjectInfo Info { get; init; }
	public Universe Universe { get; init; }


	public Project (string projectPath) {
		ProjectPath = projectPath;
		CsProjectPath = Util.CombinePaths(projectPath, CS_PROJECT_NAME);
		UniversePath = AngePath.GetUniverseRoot(projectPath);
		InfoPath = JsonUtil.GetJsonPath<ProjectInfo>(projectPath);
		Universe = new Universe(projectPath, @readonly: false);
		Info = JsonUtil.LoadOrCreateJson<ProjectInfo>(projectPath);
	}


	public static bool CreateProjectToDisk (string projectPath) {
		if (IsValidProjectPath(projectPath)) return false;

		Util.CreateFolder(projectPath);

		JsonUtil.SaveJson(new ProjectInfo() {
			ProjectName = Util.GetDisplayName(Util.GetNameWithoutExtension(projectPath)),
			Developer = "Default Company",
		}, projectPath, prettyPrint: true);

		Util.CopyFolder(EngineUtil.UniverseTemplatePath, AngePath.GetUniverseRoot(projectPath), true, true);

		Util.TextToFile(EngineUtil.GenerateCsProjectFile(), Util.CombinePaths(projectPath, CS_PROJECT_NAME));

		return true;
	}


	public static bool IsValidProjectPath (string projectPath) => Util.FileExists(JsonUtil.GetJsonPath<ProjectInfo>(projectPath));


}
