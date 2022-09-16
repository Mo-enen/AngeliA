using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



namespace PixelJelly.Editor {
	public class PixelJellyHelpWindow : EditorWindow {



		private Vector2 MasterScrollPos = Vector2.zero;


		public static void Open () {
			var window = GetWindow<PixelJellyHelpWindow>(true, "Help", true);
			window.minSize = window.maxSize = new Vector2(480, 340);
		}



		private void OnGUI () {
			using var scroll = new GUILayout.ScrollViewScope(MasterScrollPos);
			MasterScrollPos = scroll.scrollPosition;
			var richLabelStyle = new GUIStyle(GUI.skin.label) {
				richText = true,
			};
			int oldI = EditorGUI.indentLevel;
			using (new GUILayout.VerticalScope(new GUIStyle(GUI.skin.box) {
				margin = new RectOffset(12, 12, 12, 12),
				padding = new RectOffset(12, 12, 12, 12),
			})) {
				Label("Pixel Jelly by 楠瓜Moenen");
				EditorGUI.indentLevel++;
				OLabel("Twitter", "@_Moenen");
				OLabel("Email", "moenen6@gmail.com");
			}
			EditorGUI.indentLevel = oldI;
			using (new GUILayout.VerticalScope(new GUIStyle(GUI.skin.box) {
				margin = new RectOffset(12, 12, 0, 12),
				padding = new RectOffset(12, 12, 12, 12),
			})) {
				Label("Hotkeys:");
				EditorGUI.indentLevel++;
				OLabel("[G]", "Show/Hide Checker Floor");
				OLabel("[C]", "Show/Hide Comments");
				OLabel("[R]", "Generate Random Seed");
				OLabel("[-]", "Prev Data Slot");
				OLabel("[+]", "Next Data Slot");
				OLabel("[<]", "Prev Frame");
				OLabel("[>]", "Next Frame");
				OLabel("[P]", "Play/Pause Animation");
				Layout.Space(18);
				Label("Hotkey Only work when it's available");
			}
			EditorGUI.indentLevel = oldI;
			// Func
			void Label (string text) => EditorGUI.LabelField(Layout.Rect(0, 18), text, richLabelStyle);
			void OLabel (string oriangeText, string text) => EditorGUI.LabelField(Layout.Rect(0, 18), $"<color=#FFCC33>{oriangeText}</color> {text}", richLabelStyle);
		}


		private void OnLostFocus () {
			Close();
		}


	}
}
