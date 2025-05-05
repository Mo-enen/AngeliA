using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AngeliA;

public class Project {

	public string ProjectPath { get; init; }
	public string SourceCodePath { get; init; }
	public string BuildPath { get; init; }
	public string DllLibPath_Debug { get; init; }
	public string DllLibPath_Release { get; init; }
	public string TempBuildPath { get; init; }
	public string TempPublishPath { get; init; }
	public string TempRoot { get; init; }
	public string UniversePath { get; init; }
	public string IconPath { get; init; }
	public string CsprojPath { get; init; }
	public string LocalEntryRoot { get; init; }
	public string BackupSavingDataRoot { get; init; }
	public bool IsEngineInternalProject { get; init; }
	public Universe Universe { get; init; }

	public static Project LoadProject (string projectPath) {
		return new Project() {
			ProjectPath = projectPath,
			SourceCodePath = Util.CombinePaths(projectPath, "src"),
			BuildPath = Util.CombinePaths(projectPath, "Build"),
			DllLibPath_Debug = GetLibraryFolderPath(projectPath, true),
			DllLibPath_Release = GetLibraryFolderPath(projectPath, false),
			TempRoot = Util.CombinePaths(projectPath, "Temp"),
			TempBuildPath = Util.CombinePaths(projectPath, "Temp", "Build"),
			TempPublishPath = Util.CombinePaths(projectPath, "Temp", "Publish"),
			IconPath = Util.CombinePaths(projectPath, "Icon.ico"),
			UniversePath = AngePath.GetUniverseRoot(projectPath),
			CsprojPath = Util.EnumerateFiles(projectPath, true, "*.csproj").FirstOrDefault(path => !path.Contains("#ignore", System.StringComparison.OrdinalIgnoreCase), defaultValue: ""),
			LocalEntryRoot = Util.CombinePaths(projectPath, "Entry"),
			BackupSavingDataRoot = Util.CombinePaths(projectPath, "Backup Saving Data"),
			IsEngineInternalProject = Util.IsSamePath(Util.GetParentPath(projectPath), GetBuiltInProjectRoot()),
			Universe = Universe.LoadFromFile(AngePath.GetUniverseRoot(projectPath)),
		};
	}

	public static string GetLibraryFolderPath (string projectPath, bool debug) => Util.CombinePaths(projectPath, "lib", debug ? "Debug" : "Release");

	public static string GetBuiltInProjectRoot () => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Built-In Projects");

}
