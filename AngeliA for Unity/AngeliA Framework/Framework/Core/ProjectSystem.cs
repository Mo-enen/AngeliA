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
			SortProjectList(UserProjects);

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

			Util.CreateFolder(projectFolderPath);
			string newUniversePath = AngePath.GetUniverseRoot(projectFolderPath);

			// Copy Maps
			Util.CopyFolder(
				BuiltInProject.MapRoot,
				AngePath.GetMapRoot(newUniversePath),
				copySubDirs: true,
				ignoreHidden: true
			);

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
			UserProjects.Insert(0, project);

			return project;
		}

		public static void OpenProject (Project project, bool ignoreCallback = false) {
			if (project == null) return;
			// Open
			CurrentProject = project;
			project.CreateFolders();
			project.Info.ModifyDate = System.DateTime.Now.ToFileTime();
			// Callback
			if (!ignoreCallback) OnProjectOpen?.Invoke();
			// Sort
			if (project != BuiltInProject && UserProjects.Contains(project)) {
				SortProjectList(UserProjects);
			}
		}

		private static void SortProjectList (List<Project> projects) => projects.Sort((a, b) => b.Info.ModifyDate.CompareTo(a.Info.ModifyDate));

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
		public string ConversationRoot { get; init; }
		public string EditableConversationRoot { get; init; }
		public string UniverseMetaRoot { get; init; }
		public string MapRoot { get; init; }
		public string ArtworkRoot { get; init; }
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
			ConversationRoot = AngePath.GetConversationRoot(universeFolder);
			EditableConversationRoot = AngePath.GetEditableConversationRoot(universeFolder);
			UniverseMetaRoot = AngePath.GetUniverseMetaRoot(universeFolder);
			MapRoot = AngePath.GetMapRoot(universeFolder);
			ArtworkRoot = AngePath.GetArtworkRoot(universeFolder);

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
			Util.CreateFolder(ConversationRoot);
			Util.CreateFolder(UniverseMetaRoot);
			Util.CreateFolder(MapRoot);
			Util.CreateFolder(SavingRoot);
			Util.CreateFolder(ItemCustomizationRoot);
			Util.CreateFolder(SavingMetaRoot);
			Util.CreateFolder(ProcedureMapRoot);
		}

	}

}