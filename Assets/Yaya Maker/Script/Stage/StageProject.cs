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


		public bool LoadProject (string projectPath, System.Action<string> callback = null) {
			if (string.IsNullOrEmpty(projectPath)) {
				callback?.Invoke(LConst.ProjectPathEmpty);
				return false;
			}
			if (!Util.FolderExists(projectPath)) {
				callback?.Invoke(LConst.ProjectPathNotExists);
				return false;
			}
			try {
				ProjectStream.LoadProject(projectPath);
				OnProjectLoaded?.Invoke(projectPath);
			} catch (System.Exception ex) {
				Debug.LogException(ex);
				callback?.Invoke(ex.Message);
				return false;
			}
			return true;
		}


		public bool SaveProject (string projectPath, System.Action<string> callback = null) {
			if (string.IsNullOrEmpty(projectPath)) {
				callback?.Invoke(LConst.ProjectPathEmpty);
				return false;
			}
			if (!Util.FolderExists(projectPath)) {
				callback?.Invoke(LConst.ProjectPathNotExists);
				return false;
			}
			try {
				ProjectStream.SaveProject(projectPath);
				OnProjectSaved?.Invoke(projectPath);
			} catch (System.Exception ex) {
				Debug.LogException(ex);
				callback?.Invoke(ex.Message);
				return false;
			}
			return true;
		}


		#endregion




		#region --- LGC ---





		#endregion




	}
}