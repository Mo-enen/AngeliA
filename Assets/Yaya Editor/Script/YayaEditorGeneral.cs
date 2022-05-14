using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AngeliaFramework;


namespace Yaya.Editor {
	public class YayaEditorGeneral {


		[InitializeOnLoadMethod]
		private static void Init () {
			EditorApplication.playModeStateChanged += (state) => {
				if (state == PlayModeStateChange.ExitingPlayMode) {
					PlayerData.SaveToDisk();
				}
			};
		}


	}
}
