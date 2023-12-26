using UnityEngine;
using UnityEditor;
using AngeliaFramework;


namespace AngeliaForUnity.Editor {
	public static class MGUI {

		public static GUIStyle BoxMarginless => _BoxMarginless ??= new GUIStyle(GUI.skin.box) {
			margin = new RectOffset(0, 0, 0, 0),
			padding = new RectOffset(0, 0, 0, 0),
		};
		private static GUIStyle _BoxMarginless = null;

		public static GUIStyle BoxMarginless_Padding_6 => _BoxMarginless_Padding_6 ??= new GUIStyle(GUI.skin.box) {
			margin = new RectOffset(0, 0, 0, 0),
			padding = new RectOffset(6, 6, 6, 6),
		};
		private static GUIStyle _BoxMarginless_Padding_6 = null;

		public static GUIStyle CenteredLabel => _CenteredLabel ??= new GUIStyle(GUI.skin.label) {
			alignment = TextAnchor.MiddleCenter,
		};
		private static GUIStyle _CenteredLabel = null;

		public static GUIStyle CenteredBoldLabel => _CenteredBoldLabel ??= new GUIStyle(GUI.skin.label) {
			alignment = TextAnchor.MiddleCenter,
			fontStyle = FontStyle.Bold,
		};
		private static GUIStyle _CenteredBoldLabel = null;

		public static GUIStyle CenteredMiniLabel => _CenteredMiniLabel ??= new GUIStyle(EditorStyles.miniLabel) {
			alignment = TextAnchor.MiddleCenter,
		};
		private static GUIStyle _CenteredMiniLabel = null;

		public static GUIStyle RightMiniLabel => _RightMiniLabel ??= new GUIStyle(EditorStyles.miniLabel) {
			alignment = TextAnchor.MiddleRight,
		};
		private static GUIStyle _RightMiniLabel = null;

		public static GUIStyle RightGreyMiniLabel => _RightGreyMiniLabel ??= new GUIStyle(EditorStyles.centeredGreyMiniLabel) {
			alignment = TextAnchor.MiddleRight,
		};
		private static GUIStyle _RightGreyMiniLabel = null;

		public static GUIStyle CenteredGreyMiniLabel => EditorStyles.centeredGreyMiniLabel;

		public static GUIStyle CenteredMiniMiniBoldLabel => _CenteredMiniMiniBoldLabel ??= new GUIStyle(EditorStyles.miniLabel) {
			alignment = TextAnchor.MiddleCenter,
			fontSize = 7,
			fontStyle = FontStyle.Bold,
		};
		private static GUIStyle _CenteredMiniMiniBoldLabel = null;

		public static GUIStyle MiniGreyLabel => _MiniGreyLabel ??= new GUIStyle(EditorStyles.centeredGreyMiniLabel) {
			alignment = TextAnchor.MiddleLeft,
		};
		private static GUIStyle _MiniGreyLabel = null;

		public static GUIStyle RichMiniGreyLabel => _RichMiniGreyLabel ??= new GUIStyle(EditorStyles.centeredGreyMiniLabel) {
			alignment = TextAnchor.MiddleLeft,
			richText = true,
		};
		private static GUIStyle _RichMiniGreyLabel = null;

		public static GUIStyle RichLabel => _RichLabel ??= new GUIStyle(GUI.skin.label) {
			richText = true,
		};
		private static GUIStyle _RichLabel = null;

		public static GUIStyle MasterScrollStyle => _MasterScrollStyle ??= new GUIStyle(GUI.skin.scrollView) {
			padding = new RectOffset(6, 6, 6, 6),
		};
		private static GUIStyle _MasterScrollStyle = null;

		public static GUIStyle PaddingPanelStyle_6 => _PaddingPanelStyle_6 ??= new GUIStyle() {
			padding = new RectOffset(6, 6, 6, 6),
		};
		private static GUIStyle _PaddingPanelStyle_6 = null;

		public static GUIStyle PaddingPanelStyle_32 => _PaddingPanelStyle_32 ??= new GUIStyle() {
			padding = new RectOffset(32, 32, 32, 32),
		};
		private static GUIStyle _PaddingPanelStyle_32 = null;

		public static GUIStyle BoxPaddingPanelStyle => _BoxPaddingPanelStyle ??= new GUIStyle(GUI.skin.box) {
			padding = new RectOffset(6, 6, 6, 6),
			margin = new RectOffset(0, 0, 0, 0),
		};
		private static GUIStyle _BoxPaddingPanelStyle = null;

		public static GUIStyle WrapTextArea => _WrapTextArea ??= new GUIStyle(GUI.skin.textArea) {
			wordWrap = true,
		};
		private static GUIStyle _WrapTextArea = null;

		public static GUIStyle CenteredLinkLabel => _CenteredLinkLabel ??= new GUIStyle(EditorStyles.linkLabel) {
			alignment = TextAnchor.MiddleCenter,
		};
		private static GUIStyle _CenteredLinkLabel = null;

		public static readonly Color HighlightBlue = new Color32(44, 93, 135, 255);
		public static readonly Color HighlightBlue_Alpha50 = new Color32(44, 93, 135, 128);
		public static readonly Color HighlightBlue_Alpha25 = new Color32(44, 93, 135, 64);
		public static readonly Color HighlightCyan = new Color32(36, 181, 161, 255);
		public static readonly Color HighlightOrange = new Color32(252, 195, 81, 255);


		public static Rect Rect (int w, int h) => GUILayoutUtility.GetRect(w, h, GUILayout.ExpandWidth(w == 0), GUILayout.ExpandHeight(h == 0));
		public static Rect LastRect () => GUILayoutUtility.GetLastRect();
		public static Rect AspectRect (float aspect) => GUILayoutUtility.GetAspectRect(aspect);
		public static void Space (int space = 4) => GUILayout.Space(space);
		public static bool Fold (string label, ref bool open) {
			open = EditorGUILayout.Foldout(open, label, true);
			Space(2);
			return open;
		}
		public static bool Fold (GUIContent label, ref bool open) {
			open = EditorGUILayout.Foldout(open, label, true);
			Space(2);
			return open;
		}
		public static void AddCursorToLastRect (MouseCursor cursor = MouseCursor.Link) {
			if (!GUI.enabled) return;
			EditorGUIUtility.AddCursorRect(LastRect(), cursor);
		}

		public static bool DownButton (Rect rect, string label, GUIStyle style = null) => DownButton(rect, new GUIContent(label), style);
		public static bool DownButton (Rect rect, GUIContent label, GUIStyle style = null) {
			style ??= GUI.skin.button;
			bool down = Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition);
			GUI.Button(rect, label, style);
			if (down) {
				GUI.changed = true;
			}
			return down;
		}
		public static bool IconButton (Rect rect, string label, Texture icon, float iconSize = -1, float iconShiftX = 0f, GUIStyle style = null, GUIStyle labelStyle = null) {
			if (iconSize < 0) iconSize = rect.height;
			bool click = GUI.Button(rect, GUIContent.none, style ?? GUI.skin.button);
			GUI.Label(rect.Shrink(iconSize, 0, 0, 0), label, labelStyle ?? CenteredLabel);
			rect.width = iconSize;
			rect.x += iconShiftX;
			if (icon != null) GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit);
			return click;
		}
		public static void FrameGUI (Rect rect, float thickness, Color color) {
			EditorGUI.DrawRect(rect.Expand(0, 0, thickness - rect.height, 0), color);
			EditorGUI.DrawRect(rect.Expand(0, 0, 0, thickness - rect.height), color);
			EditorGUI.DrawRect(rect.Expand(thickness - rect.width, 0, -thickness, -thickness), color);
			EditorGUI.DrawRect(rect.Expand(0, thickness - rect.width, -thickness, -thickness), color);
		}
		public static void CornerFrameGUI (Rect rect, float thickness, Float2 size, Color color) {

			// TL
			EditorGUI.DrawRect(new(rect.xMin, rect.yMax, size.x, thickness), color);
			EditorGUI.DrawRect(new(rect.xMin, rect.yMax - size.y, thickness, size.y), color);

			// TR
			EditorGUI.DrawRect(new(rect.xMax - size.x, rect.yMax, size.x, thickness), color);
			EditorGUI.DrawRect(new(rect.xMax, rect.yMax - size.y, thickness, size.y), color);

			// BL
			EditorGUI.DrawRect(new(rect.xMin, rect.yMin, size.x, thickness), color);
			EditorGUI.DrawRect(new(rect.xMin, rect.yMin, thickness, size.y), color);

			// BR
			EditorGUI.DrawRect(new(rect.xMax - size.x, rect.yMin, size.x, thickness), color);
			EditorGUI.DrawRect(new(rect.xMax, rect.yMin, thickness, size.y), color);


		}
		public static void DottedFrameGUI (Rect rect, float thickness, float dotSize, Color colorA, Color colorB) {
			DottedLine(rect.Expand(0, 0, thickness - rect.height, 0), dotSize, colorA, colorB, true);
			DottedLine(rect.Expand(0, 0, 0, thickness - rect.height), dotSize, colorA, colorB, true);
			DottedLine(rect.Expand(thickness - rect.width, 0, -thickness, -thickness), dotSize, colorA, colorB, false);
			DottedLine(rect.Expand(0, thickness - rect.width, -thickness, -thickness), dotSize, colorA, colorB, false);
		}
		public static int PageList (int pageIndex, int pageSize, int itemCount, System.Action<int, Rect> itemGUI, bool showIndex = true, System.Action<int, int> moreButtons = null) {

			const int HEIGHT = 18;
			const int BUTTON_HEIGHT = 22;
			int pageCount = Mathf.CeilToInt((float)itemCount / pageSize);
			pageIndex = Mathf.Clamp(pageIndex, 0, itemCount - 1);
			int from = Mathf.Clamp(pageIndex * pageSize, 0, itemCount - 1);
			int to = Mathf.Clamp((pageIndex + 1) * pageSize, 0, itemCount);
			var oldE = GUI.enabled;
			var oldC = GUI.color;
			float bgWidth = Rect(0, 1).width;

			// Content
			for (int i = 0; i < pageSize; i++) {
				int index = i + from;
				if (index >= from && index < to) {
					using (new GUILayout.HorizontalScope()) {

						var _rect = Rect(24, HEIGHT);

						// BG
						var bgRect = new Rect(_rect.x, _rect.y, bgWidth, HEIGHT);
						EditorGUI.DrawRect(
							bgRect,
							index % 2 == 0 ? Color.clear : new Color(0f, 0f, 0f, 0.1f)
						);

						// Index
						if (showIndex) {
							GUI.Label(_rect, index.ToString("00"));
							Space(2);
						}

						itemGUI(index, bgRect);
					}
				} else {
					// Out of Range
					Space(HEIGHT);
				}
			}
			GUI.enabled = oldE;
			GUI.color = oldC;
			Space(4);

			// Page Switch
			using (new GUILayout.HorizontalScope()) {
				Rect(0, BUTTON_HEIGHT);
				moreButtons?.Invoke(42, BUTTON_HEIGHT);
				if (GUI.Button(Rect(42, BUTTON_HEIGHT), "◀")) {
					pageIndex = Mathf.Clamp(pageIndex - 1, 0, pageCount - 1);
				}
				GUI.Label(
					Rect(36, BUTTON_HEIGHT),
					$"{pageIndex + 1}/{pageCount}",
					CenteredLabel
				);
				if (GUI.Button(Rect(42, BUTTON_HEIGHT), "▶")) {
					pageIndex = Mathf.Clamp(pageIndex + 1, 0, pageCount - 1);
				}
				Space(8);
			}

			return pageIndex;
		}
		public static bool Link (Rect rect, string link, GUIStyle style = null) {
			bool click = GUI.Button(rect, link, style ?? EditorStyles.linkLabel);
			EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
			return click;
		}
		public static bool Link (Rect rect, GUIContent link, GUIStyle style = null, string tooltip = "") {
			link.tooltip = tooltip;
			bool click = GUI.Button(rect, link, style ?? EditorStyles.linkLabel);
			EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
			return click;
		}
		public static void CrossLineGUI (Rect rect, float thickness, Color color) {
			float length = Mathf.Sqrt(rect.width * rect.width + rect.height * rect.height);
			float angle = Mathf.Atan2(rect.width, rect.height) * Mathf.Rad2Deg;
			var oldM = GUI.matrix;
			GUIUtility.RotateAroundPivot(-angle, rect.position);
			EditorGUI.DrawRect(new(rect.x, rect.y, thickness, length), color);
			GUI.matrix = oldM;
			GUIUtility.RotateAroundPivot(-90 + angle, new(rect.x, rect.yMax));
			EditorGUI.DrawRect(new(rect.x, rect.yMax, length, thickness), color);
			GUI.matrix = oldM;
		}
		public static void DottedLine (Rect rect, float dotSize, Color colorA, Color colorB, bool horizontal) {
			if (dotSize < 1f) dotSize = 1f;
			var _rect = rect;
			_rect.width = horizontal ? dotSize : _rect.width;
			_rect.height = !horizontal ? dotSize : _rect.height;
			int len = (int)((horizontal ? rect.width : rect.height) / dotSize);
			for (int i = 0; i < len; i++) {
				EditorGUI.DrawRect(_rect, i % 2 == 0 ? colorA : colorB);
				if (horizontal) _rect.x += dotSize; else _rect.y += dotSize;
			}
			_rect.xMax = _rect.xMax.Clamp(rect.xMin, rect.xMax);
			_rect.yMax = _rect.yMax.Clamp(rect.yMin, rect.yMax);
			EditorGUI.DrawRect(_rect, len % 2 == 0 ? colorA : colorB);
		}
		public static bool ToolToggleButton (int size, bool value, Texture icon, string tooltip = "") {
			value = GUI.Toggle(Rect(size, size), value, new GUIContent("", tooltip), GUI.skin.button);
			GUI.DrawTexture(LastRect().Shrink(6), icon);
			EditorGUIUtility.AddCursorRect(LastRect(), MouseCursor.Link);
			return value;
		}
		public static bool ToolButton (int size, Texture icon, string tooltip = "") {
			bool clicked = GUI.Button(Rect(size, size), new GUIContent("", tooltip));
			GUI.DrawTexture(LastRect().Shrink(6), icon);
			EditorGUIUtility.AddCursorRect(LastRect(), MouseCursor.Link);
			return clicked;
		}


		public static void HorizontalLine (int thickness = 1) => HorizontalLine(thickness, new Color(0.1367f, 0.1367f, 0.1367f, 1f));
		public static void HorizontalLine (Color color) => HorizontalLine(1, color);
		public static void HorizontalLine (int thickness, Color color) {
			var rect = Rect(0, thickness);
			EditorGUI.DrawRect(rect, color);
		}


		public static void VerticalLine (int thickness = 1) => VerticalLine(thickness, new Color(0.1367f, 0.1367f, 0.1367f, 1f));
		public static void VerticalLine (Color color) => VerticalLine(1, color);
		public static void VerticalLine (int thickness, Color color) {
			var rect = Rect(thickness, 0);
			EditorGUI.DrawRect(rect, color);
		}


		public static string BetterTextArea (Rect rect, string text, GUIStyle style = null) {
			bool preventSelection = Event.current.type != EventType.Repaint;
			Color oldCursorColor = GUI.skin.settings.cursorColor;
			if (preventSelection) GUI.skin.settings.cursorColor = new Color(0, 0, 0, 0);
			text = EditorGUI.TextArea(rect, text, style ?? GUI.skin.textArea);
			if (preventSelection) GUI.skin.settings.cursorColor = oldCursorColor;
			return text;
		}


		public static void CancelFocusOnClick (EditorWindow window, bool clearSelection = false) {
			if (Event.current.type == EventType.MouseDown) {
				if (clearSelection) {
					Selection.activeObject = null;
				}
				GUI.FocusControl("");
				window.Repaint();
			}
		}


		public static bool CancelFocusOnClick (bool clearSelection = false) {
			if (Event.current.type == EventType.MouseDown) {
				if (clearSelection) {
					Selection.activeObject = null;
				}
				GUI.FocusControl("");
				return true;
			}
			return false;
		}


	}
}