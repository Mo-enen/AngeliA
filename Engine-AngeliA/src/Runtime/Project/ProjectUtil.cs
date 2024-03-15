using AngeliA;

namespace AngeliaEngine;

public static class ProjectUtil {

	public const string CS_PROJECT_NAME = "Project.csproj";

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

	public static bool IsValidProjectPath (string projectPath) => !string.IsNullOrEmpty(projectPath) && Util.FileExists(JsonUtil.GetJsonPath<ProjectInfo>(projectPath));

}
