using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework {



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
				@readonly: !Game.IsEdittime
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

}