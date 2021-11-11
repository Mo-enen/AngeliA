using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using Moenen.Stage;


namespace YayaMaker.Stage {
	public class StageProject : MonoBehaviour {




		#region --- SUB ---


		// Handler
		public delegate void VoidHandler ();


		#endregion




		#region --- VAR ---


		// Api
		public static VoidHandler OnProjectLoaded { get; set; } = null;
		public static VoidHandler OnProjectSaved { get; set; } = null;

		// Data
		private Project CurrentProject = new();


		#endregion




		#region --- API ---


		public void LoadProject (string projectPath) {
			if (string.IsNullOrEmpty(projectPath)) {
				throw new LanguageException(LConst.ProjectPathEmpty);
			}
			if (!Util.FolderExists(projectPath)) {
				throw new LanguageException(LConst.ProjectPathNotExists);
			}
			CurrentProject =
				ProjectStream.LoadProject(projectPath) ??
				throw new LanguageException(LConst.FailToLoadProject);
			OnProjectLoaded?.Invoke();
		}


		public void SaveProject (string projectPath) {
			if (string.IsNullOrEmpty(projectPath)) {
				throw new LanguageException(LConst.ProjectPathEmpty);
			}
			if (!Util.FolderExists(projectPath)) {
				throw new LanguageException(LConst.ProjectPathNotExists);
			}
			ProjectStream.SaveProject(projectPath, CurrentProject);
			OnProjectSaved?.Invoke();
		}


		#endregion




		#region --- LGC ---





		#endregion




	}
}