using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

public class Project {

	public const string SOURCE_NAME = "src";
	public const string OBJECT_NAME = "obj";
	public bool IsValid => !string.IsNullOrEmpty(ProjectPath);
	public string ProjectPath { get; init; }
	public string SourceCodePath { get; init; }
	public string ObjectPath { get; init; }
	public string UniversePath { get; init; }
	public Universe Universe { get; init; }

	public Project (string projectPath) {
		ProjectPath = projectPath;
		SourceCodePath = Util.CombinePaths(projectPath, SOURCE_NAME);
		ObjectPath = Util.CombinePaths(projectPath, OBJECT_NAME);
		UniversePath = AngePath.GetUniverseRoot(projectPath);
		Universe = new Universe(AngePath.GetUniverseRoot(projectPath), "", @readonly: false);
	}

	public static void CreateProjectToDisk (string projectPath) => Util.CopyFolder(EngineUtil.ProjectTemplatePath, projectPath, true, true);

}
