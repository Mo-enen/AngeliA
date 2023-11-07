using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



namespace AngeliaFramework.Editor {
	[CustomEditor(typeof(Game))]
	public class GameInspector : UnityEditor.Editor {



		// Data
		private static readonly EditorSavingBool ShowEffectPanel = new("GameInspector.ShowEffectPanel", false);
		private static GUIContent EffectIconContent => _EffectIconContent ??= EditorGUIUtility.IconContent("VideoEffect Icon");
		private static GUIContent _EffectIconContent = null;


		// MSG
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
			if (EditorApplication.isPlaying) {
				ScreenEffectGUI();
			}
		}


		private void ScreenEffectGUI () {
			const int HEIGHT = 18;
			bool open = ShowEffectPanel.Value;
			if (MGUI.Fold(new GUIContent("Screen Effects", EffectIconContent.image), ref open)) {

				// Title
				using (new GUILayout.HorizontalScope()) {
					GUI.Label(MGUI.Rect(0, HEIGHT), "Name", MGUI.MiniGreyLabel);
					GUI.Label(MGUI.Rect(48, HEIGHT), "Order", MGUI.MiniGreyLabel);
					MGUI.Space(8);
					GUI.Label(MGUI.Rect(22 + 26, HEIGHT), "Enable", MGUI.MiniGreyLabel);
				}

				// Content
				bool oldE = GUI.enabled;
				var enu = ScreenEffect.GetEnumerator();
				while (enu.MoveNext()) {
					var effect = enu.Current;
					if (effect == null) continue;
					using (new GUILayout.HorizontalScope()) {

						// Name
						GUI.enabled = oldE;
						if (GUI.Button(MGUI.Rect(0, HEIGHT), effect.DisplayName, EditorStyles.linkLabel)) {
							Selection.activeObject = effect.Material;
							effect.enabled = true;
						}
						MGUI.AddCursorToLastRect();

						// Order
						GUI.enabled = false;
						EditorGUI.IntField(MGUI.Rect(48, HEIGHT), effect.Order);
						MGUI.Space(8);

						// Enable
						GUI.enabled = oldE;
						MGUI.Space(13);
						effect.enabled = GUI.Toggle(MGUI.Rect(22, HEIGHT), effect.enabled, GUIContent.none);
						MGUI.Space(13);

					}
					MGUI.Space(2);
				}
				GUI.enabled = oldE;

			}
			ShowEffectPanel.Value = open;

		}


	}
}
