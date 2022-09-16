using UnityEngine;
using UnityEditor;




namespace PixelJelly.Editor {



	public static class Layout {

		public static GUIStyle BoxMarginless => _BoxMarginless ??= new GUIStyle(GUI.skin.box) {
			margin = new RectOffset(0, 0, 0, 0),
			padding = new RectOffset(4, 12, 0, 0),
		};
		private static GUIStyle _BoxMarginless = null;
		public static GUIStyle ArrowStyle => _ArrowStyle ??= new GUIStyle(GUI.skin.button) {
			alignment = TextAnchor.MiddleCenter,
			fontSize = 11,
		};
		private static GUIStyle _ArrowStyle = null;
		public static GUIStyle RichLabelStyle => _RichLabelStyle ??= new GUIStyle(GUI.skin.label) {
			richText = true,
		};
		private static GUIStyle _RichLabelStyle = null;
		public static GUIContent LeftContent => _LeftContent ??= EditorGUIUtility.IconContent("scrollleft");
		private static GUIContent _LeftContent = null;
		public static GUIContent RightContent => _RightContent ??= EditorGUIUtility.IconContent("scrollright");
		private static GUIContent _RightContent = null;


		public static Rect Rect (int w, int h) => GUILayoutUtility.GetRect(w, h, GUILayout.ExpandWidth(w == 0), GUILayout.ExpandHeight(h == 0));
		public static Rect LastRect () => GUILayoutUtility.GetLastRect();
		public static Rect AspectRect (float aspect) => GUILayoutUtility.GetAspectRect(aspect);
		public static void Space (int space = 4) => GUILayout.Space(space);
		public static bool Fold (string label, bool open, GUIStyle scopeStyle = null, float offset = 0f) {
			using (new GUILayout.VerticalScope(scopeStyle ?? GUIStyle.none)) {
				var rect = Rect(0, 22);
				rect.x -= offset;
				rect.width += offset;
				open = EditorGUI.Toggle(rect, open, EditorStyles.foldout);
				rect.x += 14;
				rect.width -= 14;
				GUI.Label(rect, label);
			}
			return open;
		}

		public static bool IconButton (Rect rect, string label, Texture2D icon, GUIStyle style = null) => IconButton(rect, label, icon, null, null, style);
		public static bool IconButton (Rect rect, string label, Texture2D iconA, Texture2D iconB, GUIStyle style = null) => IconButton(rect, label, iconA, iconB, null, style);
		public static bool IconButton (Rect rect, string label, Texture2D iconA, Texture2D iconB, Texture2D iconC, GUIStyle style = null) {
			if (style == null) {
				style = GUI.skin.button;
			}
			bool result = GUI.Button(rect, label, style);
			IconLeftGUI(rect.Expand(-6, 0f, -3f, -3f), iconA);
			if (iconB != null) {
				IconLeftGUI(rect.Expand(-rect.height, 0f, -3f, -3f), iconB);
			}
			if (iconC != null) {
				IconLeftGUI(rect.Expand(6 - rect.height - rect.height, 0f, -3f, -3f), iconC);
			}
			return result;
		}
		public static void IconLeftGUI (Rect rect, Texture2D icon) {
			rect.width = rect.height;
			GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit);
		}

		public static bool QuickButton (Rect rect, string label, GUIStyle style = null) => QuickButton(rect, new GUIContent(label), style);
		public static bool QuickButton (Rect rect, GUIContent label, GUIStyle style = null) {
			if (style == null) { style = GUI.skin.button; }
			bool down = Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition);
			GUI.Button(rect, label, style);
			if (down) {
				GUI.changed = true;
			}
			return down;
		}

		public static void FrameGUI (Rect rect, float thickness, Color color) {
			EditorGUI.DrawRect(rect.Expand(0, 0, thickness - rect.height, 0), color);
			EditorGUI.DrawRect(rect.Expand(0, 0, 0, thickness - rect.height), color);
			EditorGUI.DrawRect(rect.Expand(thickness - rect.width, 0, -thickness, -thickness), color);
			EditorGUI.DrawRect(rect.Expand(0, thickness - rect.width, -thickness, -thickness), color);
		}

		public static bool DelayedIntField (int labelWidth, int fieldWidth, int height, int value, string label, out int newValue, int min = int.MinValue, int max = int.MaxValue) {
			using var _ = new GUILayout.HorizontalScope();
			GUI.Label(Rect(labelWidth, height), label);
			newValue = EditorGUI.DelayedIntField(Rect(fieldWidth, height), value);
			newValue = Mathf.Clamp(newValue, min, max);
			return newValue != value;
		}

		public static bool LeftButton (Rect rect) => GUI.Button(rect, LeftContent, ArrowStyle);
		public static bool RightButton (Rect rect) => GUI.Button(rect, RightContent, ArrowStyle);
		public static bool LeftQuickButton (Rect rect) => QuickButton(rect, LeftContent, ArrowStyle);
		public static bool RightQuickButton (Rect rect) => QuickButton(rect, RightContent, ArrowStyle);

		public static int PageField (Rect rect, int pageIndex, int pageCount, int buttonWidth = 24, int fieldWidth = 24) {

			const int LABEL_WIDTH = 24;
			var _rect = new Rect(rect.x, rect.y, buttonWidth, rect.height);
			_rect.x += rect.width - buttonWidth * 2 - fieldWidth - LABEL_WIDTH - 12;

			// Left
			using (new EditorGUI.DisabledGroupScope(pageIndex <= 0)) {
				if (LeftQuickButton(_rect)) {
					pageIndex = Mathf.Clamp(pageIndex - 1, 0, pageCount - 1);
				}
			}
			_rect.x += _rect.width + 4f;

			// Index
			int oldI = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			_rect.width = fieldWidth;
			pageIndex = Mathf.Clamp(EditorGUI.DelayedIntField(_rect, GUIContent.none, pageIndex + 1) - 1, 0, pageCount - 1);
			EditorGUI.indentLevel = oldI;
			_rect.x += _rect.width + 4f;

			// Count
			_rect.width = LABEL_WIDTH;
			GUI.Label(_rect, $"/ {pageCount}");
			_rect.x += _rect.width + 4f;

			// Right
			_rect.width = buttonWidth;
			using (new EditorGUI.DisabledGroupScope(pageIndex >= pageCount - 1)) {
				if (RightQuickButton(_rect)) {
					pageIndex = Mathf.Clamp(pageIndex + 1, 0, pageCount - 1);
				}
			}
			return pageIndex;
		}

		public static void SimpleLine () {
			EditorGUI.DrawRect(
				Rect(0, 1),
				EditorGUIUtility.isProSkin ? new Color(0f, 0f, 0f, 0.4f) : new Color(1f, 1f, 1f, 0.4f)
			);
		}

	}


}