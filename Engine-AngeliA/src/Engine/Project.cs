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

	public const string SOURCE_NAME = "src";
	public const string OBJECT_NAME = "obj";
	public const string INFO_NAME = "Info.json";

	public string ProjectPath { get; init; }
	public string SourceCodePath { get; init; }
	public string ObjectPath { get; init; }
	public string UniversePath { get; init; }
	public string InfoPath { get; init; }
	public Universe Universe { get; init; }
	public ProjectInfo Info { get; init; }

	public Project (string projectPath) {
		ProjectPath = projectPath;
		SourceCodePath = Util.CombinePaths(projectPath, SOURCE_NAME);
		ObjectPath = Util.CombinePaths(projectPath, OBJECT_NAME);
		UniversePath = AngePath.GetUniverseRoot(projectPath);
		InfoPath = Util.CombinePaths(projectPath, INFO_NAME);
		Universe = new Universe(AngePath.GetUniverseRoot(projectPath), "", @readonly: false);
		Info = JsonUtil.LoadJsonFromPath<ProjectInfo>(InfoPath);
	}

	public static void CreateProjectToDisk (string projectPath) => Util.CopyFolder(EngineUtil.ProjectTemplatePath, projectPath, true, true);

}
