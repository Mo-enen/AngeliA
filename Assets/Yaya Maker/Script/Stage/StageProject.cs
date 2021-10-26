using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;



namespace YayaMaker.Stage {
	public class StageProject : MonoBehaviour {




		#region --- SUB ---


		// Handler
		public delegate void VoidHandler ();
		public delegate void StringHandler (string str);


		#endregion




		#region --- VAR ---


		// Api
		public static StringHandler OnProjectLoaded { get; set; } = null;
		public static StringHandler OnProjectSaved { get; set; } = null;
		

		#endregion




		#region --- API ---


		public string LoadProject (string projectPath) {
			if (string.IsNullOrEmpty(projectPath)) { return LConst.ProjectPathEmpty; }
			if (!Util.FolderExists(projectPath)) { return LConst.ProjectPathNotExists; }



			OnProjectLoaded?.Invoke(projectPath);
			return "";
		}


		public string SaveProject (string projectPath) {
			if (string.IsNullOrEmpty(projectPath)) { return LConst.ProjectPathEmpty; }
			if (!Util.FolderExists(projectPath)) { return LConst.ProjectPathNotExists; }




			OnProjectSaved?.Invoke(projectPath);
			return "";
		}


		#endregion




		#region --- LGC ---




		#endregion




	}
}