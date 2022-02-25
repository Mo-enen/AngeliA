using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class Yaya : Game {


		private void Awake () {
			Awake_Misc();
			Awake_Quit();
		}


		protected override void OnGameStart () {
			var yaya = new eYaya() {
				X = 3357,
				Y = 3200,
			};
			AddEntity(yaya);
		}


		private void Awake_Misc () {
			LConst.GetLanguage = (key) => CurrentLanguage ? CurrentLanguage[key] : "";

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
					AddEntity(new eDialog(
						2048, LConst.QuitConfirmContent, LConst.LabelQuit, LConst.LabelCancel, "",
						() => {
							willQuit = true;
							Application.Quit();
						},
						() => { },
						null
					));
					return false;
				}
			};
		}


	}
}