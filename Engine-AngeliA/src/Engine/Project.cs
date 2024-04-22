using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

[System.Serializable]
public class ProjectInfo {

	public int MajorVersion = 0;
	public int MinorVersion = 0;
	public int PatchVersion = 0;

}

public class Project {

	public string ProjectPath { get; init; }
	public string SourceCodePath { get; init; }
	public string ObjectPath { get; init; }
	public string UniversePath { get; init; }
	public string InfoPath { get; init; }
	public string IconPath { get; init; }
	public Universe Universe { get; init; }
	public ProjectInfo Info { get; init; }

	public Project (string projectPath) {
		ProjectPath = projectPath;
		SourceCodePath = Util.CombinePaths(projectPath, "src");
		ObjectPath = Util.CombinePaths(projectPath, "obj");
		IconPath = Util.CombinePaths(projectPath, "Icon.ico");
		InfoPath = Util.CombinePaths(projectPath, "Info.json");
		Info = JsonUtil.LoadJsonFromPath<ProjectInfo>(InfoPath);
		UniversePath = AngePath.GetUniverseRoot(projectPath);
		Universe = new Universe(AngePath.GetUniverseRoot(projectPath), "", @readonly: false);
	}

	public static void CreateProjectToDisk (string projectPath) => Util.CopyFolder(EngineUtil.ProjectTemplatePath, projectPath, true, true);

}
