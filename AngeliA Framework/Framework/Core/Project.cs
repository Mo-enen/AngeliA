using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {


	[System.AttributeUsage(System.AttributeTargets.Method)]
	public class OnProjectOpenAttribute : OrderedAttribute { public OnProjectOpenAttribute (int order = 0) : base(order) { } }


	public class ProjectInfo {
		public string ProjectName = "(no name)";
		public string Creator = "";
		public int EditorMajorVersion = -1;
		public int EditorMinorVersion = -1;
		public int EditorPatchVersion = -1;
	}


	[RequireLanguageFromField]
	public class Project {




		#region --- VAR ---


		// Const
		private static readonly LanguageCode NEW_PROJECT_NAME = "Project.NewProjectName";

		// Event
		private static event System.Action OnProjectOpen;

		// Path
		public string UniverseRoot { get; init; }
		public string SheetPath { get; init; }
		public string AtlasRoot { get; init; }
		public string DialogueRoot { get; init; }
		public string UniverseMetaRoot { get; init; }
		public string MapRoot { get; init; }
		public string LanguageRoot { get; init; }
		public string SavingRoot { get; init; }
		public string ItemCustomizationRoot { get; init; }
		public string SavingMetaRoot { get; init; }
		public string ProcedureMapRoot { get; init; }

		// Api
		public static Project CurrentProject { get; private set; } = null;
		public static Project BuiltInProject { get; private set; } = null;
		public static List<Project> UserProjects { get; } = new();
		public static List<Project> DownloadedProjects { get; } = new();
		public ProjectInfo Info { get; init; }


		#endregion




		#region --- MSG ---


		[OnGameInitialize(int.MinValue)]
		public static void OnGameInitializeMin () {

			Util.LinkEventWithAttribute<OnProjectOpenAttribute>(typeof(Project), nameof(OnProjectOpen));

			// Load BuiltIn Project
			CurrentProject = BuiltInProject = new(
				Util.CombinePaths(AngePath.ApplicationDataPath, "Universe"),
				Util.CombinePaths(AngePath.PersistentDataPath, "Built In Saving")
			);

			// Fill User Projects
			UserProjects.Clear();
			Util.CreateFolder(AngePath.WorkspaceRoot);
			foreach (var folder in Util.EnumerateFolders(AngePath.WorkspaceRoot, true)) {
				UserProjects.Add(new Project(folder));
			}

			// Fill Downloaded Projects
			DownloadedProjects.Clear();
			Util.CreateFolder(AngePath.DownloadRoot);
			foreach (var folder in Util.EnumerateFolders(AngePath.DownloadRoot, true)) {
				DownloadedProjects.Add(new Project(folder));
			}

		}


		[OnGameInitialize(int.MaxValue)]
		public static void OnGameInitializeMax () => OpenProject(BuiltInProject);


		public Project (string projectFolder) : this(Util.CombinePaths(projectFolder, "Universe"), Util.CombinePaths(projectFolder, "Saving")) { }


		public Project (string universeFolder, string savingFolder) {
			// Universe
			UniverseRoot = universeFolder;
			SheetPath = GetSheetPath(universeFolder);
			AtlasRoot = GetAtlasRoot(universeFolder);
			DialogueRoot = GetDialogueRoot(universeFolder);
			UniverseMetaRoot = GetUniverseMetaRoot(universeFolder);
			MapRoot = GetMapRoot(universeFolder);
			LanguageRoot = GetLanguageRoot(universeFolder);
			// Saving
			SavingRoot = savingFolder;
			ItemCustomizationRoot = GetItemCustomizationRoot(savingFolder);
			SavingMetaRoot = GetSavingMetaRoot(savingFolder);
			ProcedureMapRoot = GetProcedureMapRoot(savingFolder);
			// Create Folders 
			CreateFolders();
			// Load Info
			Info = JsonUtil.LoadOrCreateJson<ProjectInfo>(universeFolder);
		}


		#endregion




		#region --- API ---


		public static void OpenProject (Project project, bool ignoreCallback = false) {
			if (project == null) return;
			CurrentProject = project;
			project.CreateFolders();
			if (!ignoreCallback) OnProjectOpen?.Invoke();
		}


		public static Project CreateProject (string projectFolderPath) {
			// Copy Template Files
			if (Util.FolderExists(AngePath.ProjectTemplateRoot)) {
				Util.CopyFolder(
					AngePath.ProjectTemplateRoot,
					projectFolderPath,
					copySubDirs: true,
					ignoreHidden: true
				);
			}
			// Create Project Object
			var project = new Project(projectFolderPath);
			project.Info.ProjectName = NEW_PROJECT_NAME.Get("New Project");
			project.Info.Creator = "";
			project.Info.EditorMajorVersion = Game.GameMajorVersion;
			project.Info.EditorMinorVersion = Game.GameMinorVersion;
			project.Info.EditorPatchVersion = Game.GamePatchVersion;
			project.SaveProjectInfoToDisk();
			return project;
		}


		public void SaveProjectInfoToDisk () => JsonUtil.SaveJson(Info, UniverseRoot);


		public void CreateFolders () {
			Util.CreateFolder(AtlasRoot);
			Util.CreateFolder(DialogueRoot);
			Util.CreateFolder(UniverseMetaRoot);
			Util.CreateFolder(MapRoot);
			Util.CreateFolder(LanguageRoot);
			Util.CreateFolder(SavingRoot);
			Util.CreateFolder(ItemCustomizationRoot);
			Util.CreateFolder(SavingMetaRoot);
			Util.CreateFolder(ProcedureMapRoot);
		}


		// Util
		public static string GetSheetPath (string universeFolder) => Util.CombinePaths(universeFolder, "Sheet.sheet");
		public static string GetAtlasRoot (string universeFolder) => Util.CombinePaths(universeFolder, "Atlas");
		public static string GetDialogueRoot (string universeFolder) => Util.CombinePaths(universeFolder, "Dialogue");
		public static string GetUniverseMetaRoot (string universeFolder) => Util.CombinePaths(universeFolder, "Meta");
		public static string GetMapRoot (string universeFolder) => Util.CombinePaths(universeFolder, "Map");
		public static string GetLanguageRoot (string universeFolder) => Util.CombinePaths(universeFolder, "Language");
		public static string GetItemCustomizationRoot (string savingFolder) => Util.CombinePaths(savingFolder, "Item Customization");
		public static string GetSavingMetaRoot (string savingFolder) => Util.CombinePaths(savingFolder, "Meta");
		public static string GetProcedureMapRoot (string savingFolder) => Util.CombinePaths(savingFolder, "Procedure Map");


		#endregion




	}
}