using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }


namespace Yaya {


	public class Yaya : Game {


		// Api
		public override string MapRoot => !string.IsNullOrEmpty(_MapRoot) ? _MapRoot : (_MapRoot = Util.CombinePaths(Util.GetRuntimeBuiltRootPath(), "Maps"));
		public override string DialogueRoot => !string.IsNullOrEmpty(_DialogueRoot) ? _DialogueRoot : (_DialogueRoot = Util.CombinePaths(Util.GetRuntimeBuiltRootPath(), "Dialogues"));

		// Data
		private string _MapRoot = null;
		private string _DialogueRoot = null;


		// MSG
		private void Awake () {
			Awake_Misc();
			Awake_Quit();
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
