using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Moenen.Standard;


namespace AngeliaFramework.Editor {
	public class DialogueEditor : EditorWindow {


		// Data
		private static GUIStyle MasterStyle => _MasterStyle ??= new GUIStyle() {
			padding = new RectOffset(24, 24, 24, 24),
		};
		private static GUIStyle _MasterStyle = null;
		private Vector2 MasterScrollPos = default;


		// API
		public static void OpenEditor () {
			var window = GetWindow<DialogueEditor>(true, "Dialogue Editor", true);
			window.minSize = new Vector2(256, 256);
			window.maxSize = new Vector2(1024, 1024);
			if (!window.Load()) {
				window.Close();
			}
		}


		private void OnGUI () {
			using (new GUILayout.VerticalScope(MasterStyle)) {
				using var scroll = new GUILayout.ScrollViewScope(MasterScrollPos);
				MasterScrollPos = scroll.scrollPosition;




			}
			Layout.CancelFocusOnClick(this);
		}


		private void OnLostFocus () => Close();


		private void OnDestroy () => Save();


		private bool Load () {



			return true;
		}


		private void Save () {



		}


	}
}
