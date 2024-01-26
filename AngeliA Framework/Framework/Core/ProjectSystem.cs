using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {



	[System.AttributeUsage(System.AttributeTargets.Method)]
	public class OnProjectOpenAttribute : OrderedAttribute { public OnProjectOpenAttribute (int order = 0) : base(order) { } }



	public static class ProjectSystem {

		// Event
		private static event System.Action OnProjectOpen;

		// Api
		public static Project CurrentProject { get; private set; } = null;
		public static Project BuiltInProject { get; private set; } = null;
		public static List<Project> UserProjects { get; } = new();
		public static List<Project> DownloadedProjects { get; } = new();

		// MSG
		[OnGameInitialize(int.MinValue)]
		public static void OnGameInitializeMin () {

			Util.LinkEventWithAttribute<OnProjectOpenAttribute>(typeof(ProjectSystem), nameof(OnProjectOpen));

			// Load BuiltIn Project
			CurrentProject = BuiltInProject = new(
				Util.CombinePaths(AngePath.BuiltInUniverseRoot),
				Util.CombinePaths(AngePath.BuiltInSavingRoot),
				@readonly: true
			);

			// Fill User Projects
			UserProjects.Clear();
			Util.CreateFolder(AngePath.WorkspaceRoot);
			foreach (var folder in Util.EnumerateFolders(AngePath.WorkspaceRoot, true)) {
				UserProjects.Add(new Project(folder, @readonly: false));
			}
			SortUserProjectList();

			// Fill Downloaded Projects
			DownloadedProjects.Clear();
			Util.CreateFolder(AngePath.DownloadRoot);
			foreach (var folder in Util.EnumerateFolders(AngePath.DownloadRoot, true)) {
				DownloadedProjects.Add(new Project(folder, @readonly: true));
			}

		}

		[OnGameInitialize(int.MaxValue)]
		public static void OnGameInitializeMax () => OpenProject(BuiltInProject);

		// API
		public static Project CreateProject (string projectName) {

			string projectFolderPath = Util.CombinePaths(
				AngePath.WorkspaceRoot, System.Guid.NewGuid().ToString()
			);

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
			var project = new Project(projectFolderPath, @readonly: false);
			var info = project.Info;
			info.ProjectName = projectName;
			info.Creator = "";
			info.ModifyDate = info.CreatedDate = System.DateTime.Now.ToFileTime();
			info.EditorMajorVersion = Game.GameMajorVersion;
			info.EditorMinorVersion = Game.GameMinorVersion;
			info.EditorPatchVersion = Game.GamePatchVersion;

			// Save to Disk
			project.SaveProjectInfoToDisk();

			// Add into User List
			UserProjects.Add(project);
			SortUserProjectList();

			return project;
		}

		public static void OpenProject (Project project, bool ignoreCallback = false) {
			if (project == null) return;
			CurrentProject = project;
			project.CreateFolders();
			project.Info.ModifyDate = System.DateTime.Now.ToFileTime();
			if (!ignoreCallback) OnProjectOpen?.Invoke();
			SortUserProjectList();
		}

		private static void SortUserProjectList () => UserProjects.Sort((a, b) => b.Info.ModifyDate.CompareTo(a.Info.ModifyDate));

	}



	public class Project {

		// SUB
		public class ProjectInfo {
			public string ProjectName = "(no name)";
			public string Creator = "";
			public long CreatedDate = 0;
			public long ModifyDate = 0;
			public int EditorMajorVersion = -1;
			public int EditorMinorVersion = -1;
			public int EditorPatchVersion = -1;
		}

		// Path
		public string UniverseRoot { get; init; }
		public string SheetPath { get; init; }
		public string DialogueRoot { get; init; }
		public string UniverseMetaRoot { get; init; }
		public string MapRoot { get; init; }
		public string LanguageRoot { get; init; }
		public string SavingRoot { get; init; }
		public string ItemCustomizationRoot { get; init; }
		public string SavingMetaRoot { get; init; }
		public string ProcedureMapRoot { get; init; }

		// Api
		public ProjectInfo Info { get; init; }
		public bool Readonly { get; init; }
		public object Cover { get; init; }

		// MSG
		public Project (string projectFolder, bool @readonly) : this(Util.CombinePaths(projectFolder, "Universe"), Util.CombinePaths(projectFolder, "Saving"), @readonly) { }

		public Project (string universeFolder, string savingFolder, bool @readonly) {

			Readonly = !Game.IsEdittime && @readonly;

			// Universe
			UniverseRoot = universeFolder;
			SheetPath = AngePath.GetSheetPath(universeFolder);
			DialogueRoot = AngePath.GetDialogueRoot(universeFolder);
			UniverseMetaRoot = AngePath.GetUniverseMetaRoot(universeFolder);
			MapRoot = AngePath.GetMapRoot(universeFolder);
			LanguageRoot = AngePath.GetLanguageRoot(universeFolder);

			// Saving
			SavingRoot = savingFolder;
			ItemCustomizationRoot = AngePath.GetItemCustomizationRoot(savingFolder);
			SavingMetaRoot = AngePath.GetSavingMetaRoot(savingFolder);
			ProcedureMapRoot = AngePath.GetProcedureMapRoot(savingFolder);

			// Create Folders 
			CreateFolders();

			// Load Info
			Info = JsonUtil.LoadOrCreateJson<ProjectInfo>(universeFolder);

			// Cover
			var coverBytes = Util.FileToByte(AngePath.GetProjectCoverPath(universeFolder));
			Cover = Game.PngBytesToTexture(coverBytes);

		}

		// API
		public void SaveProjectInfoToDisk () => JsonUtil.SaveJson(Info, UniverseRoot);

		public void CreateFolders () {
			Util.CreateFolder(DialogueRoot);
			Util.CreateFolder(UniverseMetaRoot);
			Util.CreateFolder(MapRoot);
			Util.CreateFolder(LanguageRoot);
			Util.CreateFolder(SavingRoot);
			Util.CreateFolder(ItemCustomizationRoot);
			Util.CreateFolder(SavingMetaRoot);
			Util.CreateFolder(ProcedureMapRoot);
		}

	}

}