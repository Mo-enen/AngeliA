using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }


namespace Yaya {

	/*
	public class Test : CellEffect {
		public Test (int duration) : base(duration) { }
		public override void Perform (CellLayer layer, int layerIndex) {
			int count = layer.Count;
			for (int i = 0; i < count; i++) {
				ref var cell = ref layer.Cells[i];
				if (cell.ID < 0) { continue; }
				var clear = cell.Color;
				clear.a = 0;
				if (LocalFrame < Duration / 2) {
					//cell.Width = (int)Util.Remap(0, Duration / 2, cell.Width, 0, LocalFrame);
					//cell.Height = (int)Util.Remap(0, Duration / 2, cell.Height, 0, LocalFrame);
					cell.Color = Color.Lerp(cell.Color, clear, (float)LocalFrame / (Duration / 2));
				} else {
					//cell.Width = (int)Util.Remap(Duration / 2, Duration, 0, cell.Width, LocalFrame);
					//cell.Height = (int)Util.Remap(Duration / 2, Duration, 0, cell.Height, LocalFrame);
					cell.Color = Color.Lerp(clear, cell.Color, (float)(LocalFrame - (Duration / 2)) / (Duration / 2));
				}
			}
		}
	}
	//*/

	public class Yaya : Game {


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
