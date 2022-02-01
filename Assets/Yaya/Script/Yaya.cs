using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using AngeliaFramework.Entities;

using System.Threading.Tasks;
using System.Threading;

namespace Yaya {
	public class Yaya : GamePerformer {


		private void Awake () {
			Awake_Misc();
			Awake_Quit();
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


	}
}