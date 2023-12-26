using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AngeliaFramework;


namespace AngeliaForUnity.Editor {
	public class StringWindow : UtilWindow {

		private System.Action<string> OnConfirm;
		private string Label;
		private string Data;

		public static void OpenWindow (System.Action<string> onConfirm, string defaultData = "", string title = "", string label = "") {
			var window = OpenEditor<StringWindow>(title, new Float2(256, 96), new Float2(1024, 96));
			window.OnConfirm = onConfirm;
			window.Data = defaultData;
			window.Label = label;
		}

		protected override void OnWindowGUI () {
			MGUI.Space(2);
			using (new GUILayout.HorizontalScope()) {
				EditorGUI.LabelField(MGUI.Rect(64, 20), Label);
				Data = EditorGUI.TextField(MGUI.Rect(0, 20), Data);
			}
			MGUI.Space(12);
			using (new GUILayout.HorizontalScope()) {
				MGUI.Rect(0, 20);
				if (GUI.Button(MGUI.Rect(96, 26), "OK")) {
					OnConfirm?.Invoke(Data);
					Close();
				}
				MGUI.Space(2);
				if (GUI.Button(MGUI.Rect(96, 26), "Cancel")) {
					Close();
				}
			}
		}

		protected override void OnLostFocus () { }

	}
}