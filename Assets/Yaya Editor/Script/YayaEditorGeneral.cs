using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AngeliaFramework;


namespace Yaya.Editor {
	public class YayaEditorGeneral {


		[InitializeOnLoadMethod]
		private static void Init () {
			var game = Object.FindObjectOfType<Yaya>();
			if (game != null) {
				//Util.SetFieldValue(game, "__UniverseRoot", null);
				//Debug.Log(Util.GetFieldValue(game, "__UniverseRoot"));
			}
			EditorApplication.playModeStateChanged += (state) => {
				if (state == PlayModeStateChange.ExitingPlayMode) {
					PlayerData.SaveToDisk();
				}
			};
		}


	}
}
