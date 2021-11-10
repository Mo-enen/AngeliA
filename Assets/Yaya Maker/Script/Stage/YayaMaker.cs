using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UIGadget;
using AngeliaFramework;
using Moenen.Stage;
using YayaMaker.Stage;
using YayaMaker.UI;


namespace YayaMaker {
	public class YayaMaker : MonoBehaviour {




		#region --- VAR ---


		// Const
		public const int SLOT_COUNT = 24;

		// Ser
		[Header("Stage")]
		[SerializeField] StageLanguage m_Language = null;
		[SerializeField] StageProject m_Project = null;
		[Header("AngeliA")]
		[SerializeField] Game m_Game = null;
		[Header("UI")]
		[SerializeField] TileUI m_TileLayout = null;
		[SerializeField] RectTransform m_WindowRoot = null;
		[SerializeField] DialogWindow m_DialogTemplate = null;

		// Save
		private SavingInt ProjectSlot = new("YayaMaker.ProjectSlot", 0);


		#endregion




		#region --- MSG ---


		private void Awake () {
			Awake_Game();
			Awake_Language();
			Awake_Music();
			Awake_Project();
			Awake_Shortcut();
			Awake_UI();
			Awake_Quit();
		}


		private void Start () {
			m_Project.LoadProject(GetProjectPathAtSlot(ProjectSlot.Value));


		}


		private void FixedUpdate () => m_Game.FrameUpdate();


		private void Awake_Game () => m_Game.Init();


		private void Awake_Language () {

			StageLanguage.OnLanguageLoaded = () => {

			};

			LConst.GetLanguage = m_Language.Get;

		}


		private void Awake_Music () {
			StageMusic.OnMusicClipLoaded = () => {

			};
			StageMusic.OnMusicPlayPause = (playing) => {

			};
			StageMusic.OnMusicTimeChanged = (time) => {

			};
			StageMusic.OnPitchChanged = () => {

			};
		}


		private void Awake_Project () {

			// Init Project Folders
			for (int i = 0; i < SLOT_COUNT; i++) {
				Util.CreateFolder(GetProjectPathAtSlot(i));
			}

			// Message
			StageProject.OnProjectLoaded = (projectPath) => {
				//MapStream.LoadInfo(GetWorldInfoPath(projectPath));


			};
			StageProject.OnProjectSaved = (projectPath) => {
				//MapStream.SaveInfo(GetWorldInfoPath(projectPath));


			};
		}


		private void Awake_Shortcut () {
			StageShortcut.IsTyping = () => {
				var g = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
				if (g) {
					var input = g.GetComponent<UnityEngine.UI.InputField>();
					return input && input.isFocused;
				} else {
					return false;
				}
			};
		}


		private void Awake_UI () {

			m_TileLayout.ReloadTiles();




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
					var dialog = Window.Spawn(m_DialogTemplate, m_WindowRoot);
					dialog.SetContent("", LConst.QuitConfirmContent);
					dialog.AddOption(LConst.LabelQuit, LConst.Red, () => {
						willQuit = true;
						Application.Quit();
					});
					dialog.AddOption(LConst.LabelCancel, () => { });
					return false;
				}
			};
		}


		#endregion




		#region --- API ---




		#endregion




		#region --- LGC ---


		private static string GetProjectPathAtSlot (int slot) => Util.CombinePaths(
			Application.persistentDataPath, "Local", $"Slot_{slot:00}"
		);


		#endregion




	}
}



#if UNITY_EDITOR
namespace YayaMaker.Editor {
	using UnityEditor;
	[CustomEditor(typeof(YayaMaker))]
	public class YayaMaker_Inspector : Editor {
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif