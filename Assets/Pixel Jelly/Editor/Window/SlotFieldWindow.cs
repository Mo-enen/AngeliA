using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace PixelJelly.Editor {
	public class SlotFieldWindow : EditorWindow {



		// Const
		private const string TEXT_CONTROL_NAME = "PixelJelly.SlotField.TextField";

		// Data
		private System.Func<string, bool> Done = null;
		private string Slot = "";
		private string AssetRoot = "";
		private string ButtonLabel = "OK";
		private string Message = "";
		private bool AllowDone = true;
		private bool FirstGUI = true;


		// API
		public static void Open (string title, string buttonLabel, string slot, string assetRoot, System.Func<string, bool> done) {
			var window = GetWindow<SlotFieldWindow>(true, title);
			window.minSize = window.maxSize = new Vector2(320, 24 * 4 + 6);
			window.Slot = slot;
			window.Done = done;
			window.AssetRoot = assetRoot;
			window.ButtonLabel = buttonLabel;
			window.AllowDone = !window.CheckFileExist();
		}



		private void OnGUI () {

			using var _ = new GUILayout.VerticalScope(new GUIStyle() {
				padding = new RectOffset(12, 12, 12, 12),
			});

			// Field
			EditorGUI.BeginChangeCheck();
			GUI.SetNextControlName(TEXT_CONTROL_NAME);
			Slot = GUI.TextField(Layout.Rect(0, 24), Slot);
			EditorGUIUtility.AddCursorRect(Layout.LastRect(), MouseCursor.Text);

			if (FirstGUI) {
				GUI.FocusControl(TEXT_CONTROL_NAME);
			}

			// Message
			if (EditorGUI.EndChangeCheck()) {
				AllowDone = !CheckFileExist();
				Message = AllowDone ? "" : $"{Slot} already exists";
			}
			var oldC = GUI.color;
			GUI.color = new Color(1f, 0.8f, 0f, 1f);
			GUI.Label(Layout.Rect(0, 24), Message);
			GUI.color = oldC;
			Layout.Rect(0, 0);
			using (new GUILayout.HorizontalScope()) {
				Layout.Rect(0, 24);
				var oldE = GUI.enabled;
				GUI.enabled = AllowDone;
				if (GUI.Button(Layout.Rect(64, 24), ButtonLabel)) {
					if (!CheckFileExist()) {
						Done?.Invoke(Slot);
						Close();
					}
				}
				GUI.enabled = oldE;
				Layout.Space(2);
				if (GUI.Button(Layout.Rect(64, 24), "Cancel")) {
					Close();
				}
			}

			// Final
			if (Event.current.type == EventType.MouseDown && GUI.GetNameOfFocusedControl() == TEXT_CONTROL_NAME) {
				GUI.FocusControl("");
				Repaint();
			}

			FirstGUI = false;
		}


		private void OnLostFocus () => Close();



		private bool CheckFileExist () => Util.FileExists(Util.CombinePaths(AssetRoot, Slot + ".asset"));


	}
}
