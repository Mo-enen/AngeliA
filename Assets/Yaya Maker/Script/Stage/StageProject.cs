using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Moenen.Standard;



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

		// Short
		private string RootPath => Util.CombinePaths(Application.persistentDataPath, "World");


		#endregion




		#region --- MSG ---


		private void Awake () {
			for (int i = 0; i < SLOT_COUNT; i++) {
				Util.CreateFolder(GetProjectPath(i));
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


		private string GetProjectPath (int slot) => Util.CombinePaths(RootPath, $"Slot_{slot}");


		#endregion




	}
}