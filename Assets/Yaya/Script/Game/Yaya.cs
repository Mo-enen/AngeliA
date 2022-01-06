using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class Yaya : GamePerformer {




		#region --- VAR ---


		// Short
		private string ProjectRoot => Util.CombinePaths(Application.persistentDataPath, "Project");


		#endregion




		#region --- MSG ---


		private void Awake () {
			Awake_Project();
			Awake_Misc();
			Awake_Quit();
		}


		private void Start () {
			try {
				LoadProject();
			} catch (System.Exception ex) {
				LogException(ex);
			}
		}


		private void Awake_Project () {

			// Init Project Folders


		}


		private void Awake_Misc () {
			LConst.GetLanguage = (key) => Game.CurrentLanguage ? Game.CurrentLanguage[key] : "";

		}


		private void Awake_Quit () {
			bool willQuit = false;
			Application.wantsToQuit += () => {
#if UNITY_EDITOR
				if (UnityEditor.EditorApplication.isPlaying) { return true; }
#endif
				if (willQuit) {
					return true;
				} else {
					// Show Quit Dialog
					var dialog = new eDialog(
						2048, LConst.QuitConfirmContent, LConst.LabelQuit, LConst.LabelCancel, "",
						() => {
							willQuit = true;
							Application.Quit();
						},
						() => { },
						null
					);
					Game.AddEntity(dialog, EntityLayer.UI);
					return false;
				}
			};
		}


		#endregion




		#region --- LGC ---


		private void LogException (System.Exception ex) {




		}


		private void LoadProject () {
			string path = ProjectRoot;
			//if (string.IsNullOrEmpty(path)) throw new LanguageException(LConst.ProjectPathEmpty);
			Util.CreateFolder(path);
			//var project = ProjectStream.LoadProject(path) ?? throw new LanguageException(LConst.FailToLoadProject);



			//Debug.Log("Project loaded. " + project);

		}


		#endregion




	}
}