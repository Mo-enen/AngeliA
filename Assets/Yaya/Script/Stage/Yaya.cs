using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using Moenen.Stage;


namespace Yaya {
	public class Yaya : GamePerformer {




		#region --- VAR ---


		// Short
		private string ProjectRoot => Util.CombinePaths(Application.persistentDataPath, "Project");

		// Ser
		[SerializeField] StageLanguage m_Language = null;


		#endregion




		#region --- MSG ---


		private void Awake () {
			Awake_Language();
			Awake_Music();
			Awake_Project();
			Awake_Shortcut();
			Awake_Quit();
		}


		private void Start () {
			try {
				LoadProject();
			} catch (System.Exception ex) { LogException(ex); }
		}


		private void Awake_Language () {

			StageLanguage.OnLanguageLoaded = () => {

			};

			LConst.GetLanguage = m_Language.Get;

		}


		private void Awake_Music () {
			StageAudio.OnMusicClipLoaded = () => {

			};
			StageAudio.OnMusicPlayPause = (playing) => {

			};
			StageAudio.OnMusicTimeChanged = (time) => {

			};
			StageAudio.OnPitchChanged = () => {

			};
		}


		private void Awake_Project () {

			// Init Project Folders


		}


		private void Awake_Shortcut () {
			StageShortcut.IsTyping = () => {


				return false;
			};
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
			if (string.IsNullOrEmpty(path)) throw new LanguageException(LConst.ProjectPathEmpty);
			Util.CreateFolder(path);
			var project = ProjectStream.LoadProject(path) ?? throw new LanguageException(LConst.FailToLoadProject);



			//Debug.Log("Project loaded. " + project);

		}


		#endregion




	}
}