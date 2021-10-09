using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;



namespace YayaMaker.Stage {
	public class StageProject : MonoBehaviour {




		#region --- SUB ---


		// Const
		public const int SLOT_COUNT = 24;

		// Handler
		public delegate void VoidHandler ();
		public delegate void StringHandler (string str);


		#endregion




		#region --- VAR ---


		// Api
		public static StringHandler OnProjectLoaded { get; set; } = null;
		public static StringHandler OnProjectSaved { get; set; } = null;
		public int CurrentSlot { get; private set; } = 0;
		public string WorldPath => GetWorldRoot(CurrentSlot);
		public string WorldInfoPath => Util.CombinePaths(GetWorldRoot(CurrentSlot), "Info.json");
		public string PrefabPath => GetPrefabRoot(CurrentSlot);


		#endregion




		#region --- MSG ---


		private void Awake () {
			// Init Project Folders
			for (int i = 0; i < SLOT_COUNT; i++) {
				Util.CreateFolder(GetProjectPath(i));
				Util.CreateFolder(GetWorldRoot(i));
				Util.CreateFolder(GetPrefabRoot(i));
			}
		}


		#endregion




		#region --- API ---


		public string LoadProject (int slot) {
			string projectPath = GetProjectPath(slot);
			if (string.IsNullOrEmpty(projectPath)) { return LConst.ProjectPathEmpty; }
			if (!Util.FolderExists(projectPath)) { return LConst.ProjectPathNotExists; }



			CurrentSlot = slot;
			OnProjectLoaded?.Invoke(projectPath);
			return "";
		}


		public string SaveProject () {
			string projectPath = GetProjectPath(CurrentSlot);
			if (string.IsNullOrEmpty(projectPath)) { return LConst.ProjectPathEmpty; }
			if (!Util.FolderExists(projectPath)) { return LConst.ProjectPathNotExists; }




			OnProjectSaved?.Invoke(projectPath);
			return "";
		}


		#endregion




		#region --- LGC ---


		private string GetProjectPath (int slot) => Util.CombinePaths(Application.persistentDataPath, $"Slot_{slot:00}");


		private string GetWorldRoot (int slot) => Util.CombinePaths(GetProjectPath(slot), "World");


		private string GetPrefabRoot (int slot) => Util.CombinePaths(GetProjectPath(slot), "Prefab");


		#endregion




	}
}