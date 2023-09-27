using UnityEditor;
using UnityEngine;

namespace Moenen.Editor {
	public static class ProjectWindowHighlight {

		private static double LastCacheUpdateTime = -3d;
		private static EditorWindow ProjectWindow = null;

		[InitializeOnLoadMethod]
		public static void EditorInit () {

			// Update
			EditorApplication.update += () => {
				if (ProjectWindow != null || EditorApplication.timeSinceStartup <= LastCacheUpdateTime + 2d) return;
				LastCacheUpdateTime = EditorApplication.timeSinceStartup;
				foreach (var window in Resources.FindObjectsOfTypeAll<EditorWindow>()) {
					if (window.GetType().Name.EndsWith("ProjectBrowser")) {
						window.wantsMouseMove = true;
						ProjectWindow = window;
						break;
					}
				}
			};

			// Project Item GUI
			EditorApplication.projectWindowItemOnGUI -= ProjectItemGUI;
			EditorApplication.projectWindowItemOnGUI += ProjectItemGUI;
			static void ProjectItemGUI (string guid, Rect selectionRect) {
				selectionRect.width += selectionRect.x;
				selectionRect.x = 0;
				if (selectionRect.Contains(Event.current.mousePosition)) {
					EditorGUI.DrawRect(selectionRect, new Color(1, 1, 1, 0.06f));
				}
				if (EditorWindow.mouseOverWindow != null && Event.current.type == EventType.MouseMove) {
					EditorWindow.mouseOverWindow.Repaint();
					Event.current.Use();
				}
			}

		}
	}
}