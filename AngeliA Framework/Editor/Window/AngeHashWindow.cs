using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AngeliaFramework.Editor {
	public class AngeHashWindow : UtilWindow {


		private string Data = "";
		private string Hash = "";


		[MenuItem("AngeliA/Other/Ange Hash")]
		public static void OpenWindow () {
			var window = GetWindow<AngeHashWindow>(true, "Ange Hash", true);
			window.minSize = new Vector2(256, 256);
			window.maxSize = new Vector2(512, 2048);
			window.Data = "";
			window.Hash = "";
		}


		protected override void OnWindowGUI () {

			// Text
			GUILayout.Label("Text");
			string newData = EditorGUILayout.TextArea(Data);
			if (newData != Data) {
				Data = newData;
				Hash = "";
				foreach (string line in Util.ForAllLinesInString(Data)) {
					Hash += line.AngeHash() + "\n";
				}
				if (Hash.Length > 0) Hash.Remove(Hash.Length - 1, 1);
			}
			MGUI.Space(8);

			// Hash Int
			GUILayout.Label("Hash");
			EditorGUILayout.TextArea(Hash);

		}


		protected override void OnLostFocus () { }


	}
}