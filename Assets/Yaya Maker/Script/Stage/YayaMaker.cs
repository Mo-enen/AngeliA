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


		// Ser
		[Header("Stage")]
		[SerializeField] StageLanguage m_Language = null;
		[SerializeField] StageProject m_Project = null;
		[Header("UI")]
		[SerializeField] TileLayout m_TileLayout = null;
		[SerializeField] RectTransform m_WindowRoot = null;
		[SerializeField] DialogWindow m_DialogTemplate = null;
		[Header("Data")]
		[SerializeField] Text[] m_LanguageTexts = null;

		// Data
		private SavingInt ProjectSlot = new SavingInt("YayaMaker.ProjectSlot", 0);


		#endregion




		#region --- MSG ---


		private void Awake () {
			Awake_System();
			Awake_Language();
			Awake_Music();
			Awake_Project();
			Awake_Shortcut();
			Awake_Misc();
			Awake_Quit();
		}


		private void Start () {
			m_Project.LoadProject(ProjectSlot.Value);


		}


		private void Awake_System () {
			Application.targetFrameRate = 10000;



		}


		private void Awake_Language () {

			StageLanguage.OnLanguageLoaded = () => {

			};

			LConst.GetLanguage = m_Language.Get;

			// Reload Language Texts
			foreach (var text in m_LanguageTexts) {
				if (text != null) {
					text.text = m_Language.Get(text.name);
				}
			}
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
			StageProject.OnProjectLoaded = (projectPath) => {
				WorldStream.LoadInfo(Util.CombinePaths(projectPath, "Info.json"));
				ProjectSlot.Value = m_Project.CurrentSlot;


			};
			StageProject.OnProjectSaved = (projectPath) => {
				WorldStream.SaveInfo(Util.CombinePaths(projectPath, "Info.json"));


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


		private void Awake_Misc () {

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