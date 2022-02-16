using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using LdtkToAngeliA;


namespace AngeliaFramework.Editor {
	public class LdtkToolkitWindow : EditorWindow {




		#region --- VAR ---


		private static GUIStyle MasterStyle => _MasterStyle ??= new GUIStyle() {
			padding = new RectOffset(24, 24, 24, 24),
		};
		private static GUIStyle _MasterStyle = null;
		private Vector2 MasterScrollPos = default;


		#endregion




		#region --- MSG ---


		public static void OpenWindow () {
			if (EditorApplication.isPlaying) return;
			var window = GetWindow<LdtkToolkitWindow>(true, "Ldtk Toolkit", true);
			window.minSize = new Vector2(256, 256);
			window.maxSize = new Vector2(1024, 1024);
		}


		private void Update () {
			if (EditorApplication.isPlaying) Close();
		}


		private void OnGUI () {
			using (new GUILayout.VerticalScope(MasterStyle)) {
				using var scroll = new GUILayout.ScrollViewScope(MasterScrollPos);
				MasterScrollPos = scroll.scrollPosition;








			}
		}


		private void OnLostFocus () => Close();


		#endregion




		#region --- LGC ---




		#endregion




	}
}
