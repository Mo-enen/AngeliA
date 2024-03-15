using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

public class Project {

	public bool IsValid => !string.IsNullOrEmpty(ProjectPath);
	public string ProjectPath { get; init; }
	public string CsProjectPath { get; init; }
	public string UniversePath { get; init; }
	public string InfoPath { get; init; }
	public ProjectInfo Info { get; init; }
	public Universe Universe { get; init; }

	public Project (string projectPath) {
		ProjectPath = projectPath;
		CsProjectPath = Util.CombinePaths(projectPath, ProjectUtil.CS_PROJECT_NAME);
		UniversePath = AngePath.GetUniverseRoot(projectPath);
		InfoPath = JsonUtil.GetJsonPath<ProjectInfo>(projectPath);
		Universe = new Universe(projectPath, @readonly: false);
		Info = JsonUtil.LoadOrCreateJson<ProjectInfo>(projectPath);
	}

}
