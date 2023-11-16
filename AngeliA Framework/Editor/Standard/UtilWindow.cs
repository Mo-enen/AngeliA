using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace AngeliaFramework.Editor {
	public abstract class UtilWindow : EditorWindow {


		// VAR
		private static GUIStyle MasterStyle => _MasterStyle ??= new GUIStyle() {
			padding = new RectOffset(24, 24, 6, 24),
		};
		private static GUIStyle _MasterStyle = null;

		private Vector2 MasterScrollPos = default;
		protected virtual bool ScrollHorizontal => false;
		protected virtual bool ScrollVertical => true;


		// API
		public static T OpenEditor<T> (string title = "") where T : UtilWindow {
			var window = GetWindow<T>(true, "", true);
			window.minSize = new Vector2(256, 256);
			window.maxSize = new Vector2(1024, 1024);
			window.titleContent = new GUIContent(title);
			return window;
		}
		public static T OpenEditor<T> (string title, Vector2 minSize, Vector2 maxSize) where T : UtilWindow {
			var window = GetWindow<T>(true, "", true);
			window.minSize = minSize;
			window.maxSize = maxSize;
			window.titleContent = new GUIContent(title);
			return window;
		}


		// MSG
		protected virtual void OnGUI () {
			BeforeWindowGUI();
			using (var scroll = new GUILayout.ScrollViewScope(
				MasterScrollPos,
				ScrollHorizontal ? GUI.skin.horizontalScrollbar : GUIStyle.none,
				ScrollVertical ? GUI.skin.verticalScrollbar : GUIStyle.none
			)) {
				MasterScrollPos = scroll.scrollPosition;
				using (new GUILayout.VerticalScope(MasterStyle)) {
					OnWindowGUI();
				}
			}
			AfterWindowGUI();
			MGUI.CancelFocusOnClick(this);
		}


		protected virtual void BeforeWindowGUI () { }
		protected abstract void OnWindowGUI ();
		protected virtual void AfterWindowGUI () { }


		protected virtual void OnLostFocus () => Close();


		protected void SetScrollY (float scroll) => MasterScrollPos.y = scroll;


	}
}
