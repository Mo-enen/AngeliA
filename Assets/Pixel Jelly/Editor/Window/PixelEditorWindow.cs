using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace PixelJelly.Editor {
	public class PixelEditorWindow : EditorWindow {




		#region --- SUB ---


		private enum Tools {
			Select = 0,
			Rect = 1,
		}


		#endregion




		#region --- VAR ---


		// Const
		private const int TOOLBAR_SIZE = 36;
		private const int PALETTE_WIDTH = 22 * 8;
		private const int MAX_CANVAS_SIZE = 512;
		private static Color32 HIGHLIGHT_TINT = new Color32(80, 207, 252, 255);
		private static Color32 HIGHLIGHT_TINT_ALT = new Color32(80, 207, 252, 128);


		// Short
		private JellyConfig Config => JellyConfig.Main;
		private static GUIStyle PanelStyle => _PanelStyle ??= new GUIStyle() {
			padding = new RectOffset(12, 12, 24, 24),
		};
		private static GUIStyle CenterLabelStyle => _CenterLabelStyle ??= new GUIStyle(GUI.skin.label) {
			alignment = TextAnchor.MiddleCenter,
		};
		private static GUIContent SelectContent => _SelectContent ??= EditorGUIUtility.IconContent("RectTransformBlueprint");
		private static GUIContent RectContent => _RectContent ??= EditorGUIUtility.IconContent("Grid.PaintTool");
		private static GUIContent PivotContent => _PivotContent ??= EditorGUIUtility.IconContent("DotSelection");
		private static GUIContent EraseContent => _EraseContent ??= EditorGUIUtility.IconContent("Grid.EraserTool");

		// Data
		[SerializeField] PixelSprite m_PixelSprite = null;
		private System.Action<PixelSprite> Callback = null;
		private static GUIStyle _PanelStyle = null;
		private static GUIStyle _CenterLabelStyle = null;
		private static GUIContent _SelectContent = null;
		private static GUIContent _RectContent = null;
		private static GUIContent _PivotContent = null;
		private static GUIContent _EraseContent = null;
		private bool ConfigDirty = false;
		private bool PixelsDirty = false;
		private Color32 SelectingColor = default;
		private Tools SelectingTool = Tools.Rect;
		private Texture2D CheckboardTexture = null;
		private Vector2Int? PrevLocalMousePos = null;
		private Vector2Int? MouseDownPos = null;
		private Vector2Int? DraggingMousePos = null;
		private Vector2Int? DraggingSelectionOffset = null;
		private RectInt? Selection = null;
		private RectInt? MovingSelection = null;
		private Rect DraggingGUIRect = default;
		private int ColorPickerID = 0;
		private int SpritePickerID = 0;
		private int PickingIndex = -1;

		// Saving
		private readonly EditorSavingBool UseCheckerFloor = new EditorSavingBool("PEW.UseCheckfloor", true);


		#endregion




		#region --- MSG ---


		[InitializeOnLoadMethod]
		public static void Init () {
			Undo.undoRedoPerformed += () => {
				if (HasOpenInstances<PixelEditorWindow>()) {
					var window = GetWindow<PixelEditorWindow>(true, "Pixel Editor");
					window.RefreshCheckfloor();
				}
			};
		}


		private void OnGUI () {
			if (m_PixelSprite == null) {
				m_PixelSprite = new PixelSprite(1, 1);
			}
			if (m_PixelSprite.Colors == null || m_PixelSprite.Width == 0 || m_PixelSprite.Height == 0) {
				m_PixelSprite.Colors = new Color32[Mathf.Max(m_PixelSprite.Width, 1) * Mathf.Max(m_PixelSprite.Height, 1)];
			}
			m_PixelSprite.BorderL = Mathf.Clamp(m_PixelSprite.BorderL, 0, m_PixelSprite.Width - m_PixelSprite.BorderR);
			m_PixelSprite.BorderR = Mathf.Clamp(m_PixelSprite.BorderR, 0, m_PixelSprite.Width - m_PixelSprite.BorderL);
			m_PixelSprite.BorderD = Mathf.Clamp(m_PixelSprite.BorderD, 0, m_PixelSprite.Height - m_PixelSprite.BorderU);
			m_PixelSprite.BorderU = Mathf.Clamp(m_PixelSprite.BorderU, 0, m_PixelSprite.Height - m_PixelSprite.BorderD);
			using var _ = new GUILayout.HorizontalScope(PanelStyle);
			using (new GUILayout.VerticalScope(GUILayout.Width(PALETTE_WIDTH))) {
				GUI_Palette();
			}
			Layout.Space(24);
			using (new GUILayout.VerticalScope()) {
				GUI_Canvas();
			}
			Layout.Space(24);
			using (new GUILayout.VerticalScope(GUILayout.Width(PALETTE_WIDTH))) {
				GUI_Toolbar();
				Layout.Space(12);
				GUI_Inspector();
			}
			GUI_Hotkey();
			if (ConfigDirty) {
				ConfigDirty = false;
				Config.SaveChanges();
			}
			if (Event.current.type == EventType.MouseDown) {
				GUI.FocusControl("");
				Repaint();
			}
		}


		private void OnDisable () {
			if (PixelsDirty) {
				bool save = EditorUtil.Dialog("Confirm", "Unsaved changes will lost.", "Save", "Don\'t Save");
				if (save) {
					Callback?.Invoke(m_PixelSprite);
				}
			}
		}


		private void GUI_Palette () {
			const int ROW = 16;
			const int COLUMN = 8;
			const int ITEM_SIZE = PALETTE_WIDTH / COLUMN;
			var palColors = Config.PixelEditorPalette;
			int palIndex = 0;
			int downIndex = -1;
			var highlight = HIGHLIGHT_TINT;
			bool coloringToolSelecting = SelectingTool == Tools.Rect;
			// Fix Config Pal Count
			if (palColors.Count > ROW * COLUMN) {
				palColors.RemoveRange(ROW * COLUMN, palColors.Count - ROW * COLUMN);
			} else if (palColors.Count < ROW * COLUMN) {
				palColors.AddRange(new Color32[ROW * COLUMN - palColors.Count]);
			}
			// Draw Pal
			for (int y = 0; y < ROW; y++) {
				var rect = Layout.Rect(0, ITEM_SIZE);
				rect.width = ITEM_SIZE;
				for (int x = 0; x < COLUMN; x++) {
					if (Layout.QuickButton(rect, GUIContent.none, GUI.skin.textField)) {
						downIndex = palIndex;
					}
					if (palIndex < palColors.Count) {
						EditorGUI.DrawRect(rect.Expand(-1.5f), palColors[palIndex]);
					}
					rect.x += ITEM_SIZE;
					palIndex++;
				}
			}
			Layout.Space(8);
			if (coloringToolSelecting) {
				using (new GUILayout.HorizontalScope()) {
					// Draw Selecting Colors
					var sRect = Layout.Rect(0, 24);
					var oldColor = SelectingColor;
					var newColor = EditorGUI.ColorField(sRect, GUIContent.none, oldColor, false, true, false);
					if (newColor != oldColor) {
						SelectingColor = newColor;
						SetConfigDirty();
					}
					Layout.Space(2);
					// Clear Button
					if (GUI.Button(Layout.Rect(36, 24), EraseContent)) {
						SelectingColor = new Color32(0, 0, 0, 0);
						SetConfigDirty();
					}
				}
			}
			// Mouse Down
			if (downIndex >= 0) {
				if (Event.current.button == 0) {
					// Left
					SelectingColor = palColors[downIndex];
				} else if (Event.current.button == 1) {
					// Right
					var clipColor = EditorUtil.GetColorFromClipboard();
					var menu = new GenericMenu();
					menu.AddItem(new GUIContent("Edit"), false, () => {
						PickingIndex = downIndex;
						ColorPickerID = EditorUtil.ShowColorPicker(palColors[downIndex]);
					});
					menu.AddItem(new GUIContent("Copy"), false, () => EditorGUIUtility.systemCopyBuffer = "#" + ColorUtility.ToHtmlStringRGBA(palColors[downIndex]));
					if (clipColor.HasValue) {
						menu.AddItem(new GUIContent("Paste"), false, () => palColors[downIndex] = clipColor.Value);
					} else {
						menu.AddDisabledItem(new GUIContent("Paste"));
					}
					menu.ShowAsContext();
				}
				Event.current.Use();
			}

			// Sprite Picker Event
			if (EditorUtil.ColorPickerChanged(ColorPickerID, out var pickedColor)) {
				if (PickingIndex >= 0 && PickingIndex < palColors.Count) {
					palColors[PickingIndex] = pickedColor;
				}
				Repaint();
			}

			// Preview
			int width = m_PixelSprite.Width;
			int height = m_PixelSprite.Height;
			if (width > 0 && height > 0) {
				Layout.Space(24);
				int pWidth, pHeight;
				if ((float)width / height > 1f) {
					pWidth = 96;
					pHeight = pWidth * height / width;
				} else {
					pHeight = 96;
					pWidth = pHeight * width / height;
				}
				var pRect = Layout.Rect(pWidth, pHeight);
				float pixelWidth = pRect.width / width;
				float pixelHeight = pRect.height / height;
				int offsetX = (PALETTE_WIDTH - pWidth) / 2;
				try {
					for (int j = 0; j < height; j++) {
						for (int i = 0; i < width; i++) {
							EditorGUI.DrawRect(new Rect(
								pRect.x + i * pixelWidth + offsetX,
								pRect.y + (height - j - 1) * pixelHeight,
								pixelWidth + 1f,
								pixelHeight + 1f
							), m_PixelSprite[i, j]);
						}
					}
				} catch { }
			}

		}


		private void GUI_Canvas () {

			var mousePos = Event.current.mousePosition;
			int width = m_PixelSprite.Width;
			int height = m_PixelSprite.Height;

			var canvasRect = Layout.Rect(0, (int)(position.height - 48)).Fit((float)width / height);
			float pixelSizeX = canvasRect.width / width;
			float pixelSizeY = canvasRect.height / height;
			var rect = new Rect(0, 0, pixelSizeX + 1f, pixelSizeY + 1f);
			if ((Selection.HasValue || DraggingSelectionOffset.HasValue) && SelectingTool != Tools.Select) {
				Selection = null;
				DraggingSelectionOffset = null;
			}

			// Check Floor
			if (UseCheckerFloor.Value) {
				GUI.DrawTexture(canvasRect, CheckboardTexture, ScaleMode.StretchToFill);
			}

			// Pixels
			for (int j = 0; j < height; j++) {
				rect.y = LocalToGUI_Y(j);
				for (int i = 0; i < width; i++) {
					rect.x = LocalToGUI_X(i);
					EditorGUI.DrawRect(rect, m_PixelSprite[i, j]);
				}
			}

			// Pivot
			if (
				m_PixelSprite.PivotX >= 0 && m_PixelSprite.PivotX < m_PixelSprite.Width &&
				m_PixelSprite.PivotY >= 0 && m_PixelSprite.PivotY < m_PixelSprite.Height
			) {
				rect.x = LocalToGUI_X(m_PixelSprite.PivotX);
				rect.y = LocalToGUI_Y(m_PixelSprite.PivotY);
				EditorGUI.DropShadowLabel(rect, PivotContent, CenterLabelStyle);
			}

			// Border
			if (m_PixelSprite.BorderL > 0 && m_PixelSprite.BorderL <= m_PixelSprite.Width) {
				EditorGUI.DrawRect(new Rect(canvasRect) {
					x = LocalToGUI_X(m_PixelSprite.BorderL),
					width = 1,
				}, Color.green);
			}
			if (m_PixelSprite.BorderR > 0 && m_PixelSprite.BorderR <= m_PixelSprite.Width) {
				EditorGUI.DrawRect(new Rect(canvasRect) {
					x = LocalToGUI_X(m_PixelSprite.Width - m_PixelSprite.BorderR),
					width = 1,
				}, Color.green);
			}
			if (m_PixelSprite.BorderD > 0 && m_PixelSprite.BorderD <= m_PixelSprite.Height) {
				EditorGUI.DrawRect(new Rect(canvasRect) {
					y = LocalToGUI_Y(m_PixelSprite.BorderD) + pixelSizeY,
					height = 1,
				}, Color.green);
			}
			if (m_PixelSprite.BorderU > 0 && m_PixelSprite.BorderU <= m_PixelSprite.Height) {
				EditorGUI.DrawRect(new Rect(canvasRect) {
					y = LocalToGUI_Y(m_PixelSprite.Height - m_PixelSprite.BorderU) + pixelSizeY,
					height = 1,
				}, Color.green);
			}

			// Dragging GUI Rect
			if (DraggingGUIRect.width.NotAlmostZero() && !DraggingSelectionOffset.HasValue) {
				if (SelectingTool == Tools.Rect) {
					EditorGUI.DrawRect(DraggingGUIRect, SelectingColor);
				}
				Layout.FrameGUI(DraggingGUIRect, 1f, Color.white);
				Layout.FrameGUI(DraggingGUIRect, -1f, Color.black);
			}

			// Selection GUI Rect
			Rect selectionGUIRect = default;
			if (Selection.HasValue) {
				selectionGUIRect = LocalToGUI_Rect(Selection.Value);
				selectionGUIRect.width += pixelSizeX;
				selectionGUIRect.height += pixelSizeY;
				Layout.FrameGUI(selectionGUIRect, 1f, Color.white);
				Layout.FrameGUI(selectionGUIRect, -1f, Color.black);
			}
			if (MovingSelection.HasValue) {
				var guiRect = LocalToGUI_Rect(MovingSelection.Value);
				guiRect.width += pixelSizeX;
				guiRect.height += pixelSizeY;
				if (guiRect.Overlaps(canvasRect, false)) {
					guiRect.Clamp(canvasRect);
					EditorGUI.DrawRect(guiRect, HIGHLIGHT_TINT_ALT);
				}
			}

			// Cursor
			var localMousePos = new Vector2Int(
				(int)((mousePos.x - canvasRect.x) / pixelSizeX),
				height - (int)((mousePos.y - canvasRect.y) / pixelSizeY) - 1
			);
			bool mouseInSelection = Selection.HasValue && Selection.Value.Expand(0, 1, 0, 1).Contains(localMousePos);
			bool mouseInCanvas = canvasRect.Contains(mousePos);
			bool showCursor = SelectingTool == Tools.Rect || !mouseInSelection;
			if (showCursor && mouseInCanvas && !MovingSelection.HasValue) {
				rect.x = LocalToGUI_X(localMousePos.x);
				rect.y = LocalToGUI_Y(localMousePos.y);
				rect.width = pixelSizeX;
				rect.height = pixelSizeY;
				Layout.FrameGUI(rect, 1f, Color.white);
				Layout.FrameGUI(rect, -1f, Color.black);
				if (SelectingTool != Tools.Select) {
					EditorGUI.DrawRect(rect.Expand(-1), SelectingColor);
				}
			}
			if (Selection.HasValue && !MovingSelection.HasValue) {
				EditorGUIUtility.AddCursorRect(selectionGUIRect, MouseCursor.MoveArrow);
			}
			if (Event.current.isMouse) {
				if (PrevLocalMousePos.HasValue != mouseInCanvas || (mouseInCanvas && PrevLocalMousePos.Value != localMousePos)) {
					Repaint();
				}
				if (mouseInCanvas) {
					PrevLocalMousePos = localMousePos;
				} else {
					PrevLocalMousePos = null;
				}
			}


			// Mose Event
			bool dragBegin = false;
			bool dragging = false;
			bool dragEnd = false;
			int button = Event.current.button;
			switch (Event.current.type) {
				case EventType.MouseDown:
					// Mouse Down
					if (button == 0) {
						// Left
						if (mouseInCanvas) {
							MouseDownPos = localMousePos;
							Repaint();
							if (Selection.HasValue) {
								if (mouseInSelection) {
									DraggingSelectionOffset = localMousePos - Selection.Value.position;
									MovingSelection = Selection;
								} else {
									DraggingSelectionOffset = null;
									Selection = null;
								}
							}
						} else {
							Selection = null;
						}
					} else if (button == 1) {
						// Right
						if (mouseInCanvas) {
							SelectingColor = GetPixel(localMousePos.x, localMousePos.y);
						}
						Repaint();
					}
					break;
				case EventType.MouseDrag:
					// Dragging
					if (MouseDownPos.HasValue && (DraggingMousePos.HasValue || localMousePos != MouseDownPos.Value)) {
						if (!DraggingMousePos.HasValue) {
							dragBegin = true;
						}
						dragging = true;
						DraggingMousePos = localMousePos;
						Repaint();
					}
					break;
				case EventType.MouseUp:
				case EventType.MouseMove:
				case EventType.MouseLeaveWindow:
				case EventType.MouseEnterWindow:
					// Drag End
					if (button != 0) { break; }
					if (DraggingMousePos.HasValue || MouseDownPos.HasValue || DraggingSelectionOffset.HasValue || MovingSelection.HasValue) {
						dragEnd = true;
						Repaint();
					}
					break;
			}

			// Mouse Logic
			if (dragBegin) {
				DraggingGUIRect = default;
			}
			if (dragging) {
				if (!DraggingSelectionOffset.HasValue) {
					var dRect = GetDraggingRect(localMousePos, MouseDownPos.Value);
					dRect.width++;
					dRect.height++;
					dRect.y--;
					DraggingGUIRect = LocalToGUI_Rect(dRect);
				} else if (MovingSelection.HasValue) {
					// Moving Selection
					var mRect = MovingSelection.Value;
					mRect.x = localMousePos.x - DraggingSelectionOffset.Value.x;
					mRect.y = localMousePos.y - DraggingSelectionOffset.Value.y;
					MovingSelection = mRect;
				}
				Repaint();
			}
			if (dragEnd) {
				switch (SelectingTool) {
					case Tools.Select:
						if (Selection.HasValue && DraggingSelectionOffset.HasValue) {
							// Move Selection
							BeforePixelChange();
							MovePixels(Selection.Value.Expand(0, 1, 0, 1), localMousePos - DraggingSelectionOffset.Value - Selection.Value.position, Event.current.control);
							var selection = Selection.Value;
							selection.position = localMousePos - DraggingSelectionOffset.Value;
							Selection = selection;
						} else {
							// Select
							Selection = GetDraggingRect(localMousePos, MouseDownPos.Value);
						}
						break;
					case Tools.Rect:
						var dRect = GetDraggingRect(localMousePos, MouseDownPos.Value);
						Undo.RegisterCompleteObjectUndo(this, "Paint");
						PixelsDirty = true;
						for (int j = dRect.yMin; j <= dRect.yMax; j++) {
							for (int i = dRect.xMin; i <= dRect.xMax; i++) {
								SetPixel(i, j, SelectingColor);
							}
						}
						break;
				}
				MouseDownPos = null;
				DraggingMousePos = null;
				DraggingSelectionOffset = null;
				MovingSelection = null;
				DraggingGUIRect = default;
			}

			// Func
			float LocalToGUI_X (int localX) => canvasRect.x + localX * pixelSizeX;
			float LocalToGUI_Y (int localY) => canvasRect.y + (height - localY - 1) * pixelSizeY;
			Rect LocalToGUI_Rect (RectInt _rect) => new Rect(
				LocalToGUI_X(_rect.x),
				LocalToGUI_Y(_rect.y) - _rect.height * pixelSizeY,
				_rect.width * pixelSizeX,
				_rect.height * pixelSizeY
			);
			RectInt GetDraggingRect (Vector2Int a, Vector2Int b) {
				int l = Mathf.Clamp(Mathf.Min(a.x, b.x), 0, width - 1);
				int r = Mathf.Clamp(Mathf.Max(a.x, b.x), 0, width - 1);
				int d = Mathf.Clamp(Mathf.Min(a.y, b.y), 0, height - 1);
				int u = Mathf.Clamp(Mathf.Max(a.y, b.y), 0, height - 1);
				return new RectInt(l, d, r - l, u - d);
			}
		}


		private void GUI_Toolbar () {

			using var _ = new GUILayout.HorizontalScope();
			bool newValue;
			bool selecting;
			Rect rect;

			// Select
			selecting = SelectingTool == Tools.Select;
			rect = Layout.Rect(TOOLBAR_SIZE, TOOLBAR_SIZE);
			newValue = GUI.Toggle(rect, selecting, SelectContent, GUI.skin.button);
			if (newValue != selecting) {
				SelectingTool = Tools.Select;
			}
			Layout.Space(2);
			if (selecting) {
				EditorGUI.DrawRect(rect.Expand(-1, -1, -rect.height + 2, 0), HIGHLIGHT_TINT);
			}

			// Rect
			selecting = SelectingTool == Tools.Rect;
			rect = Layout.Rect(TOOLBAR_SIZE, TOOLBAR_SIZE);
			newValue = GUI.Toggle(rect, selecting, RectContent, GUI.skin.button);
			if (newValue != selecting) {
				SelectingTool = Tools.Rect;
			}
			if (selecting) {
				EditorGUI.DrawRect(rect.Expand(-1, -1, -rect.height + 2, 0), HIGHLIGHT_TINT);
			}
			Layout.Space(2);

		}


		private void GUI_Inspector () {

			int width = m_PixelSprite.Width;
			int height = m_PixelSprite.Height;
			int pivotX = m_PixelSprite.PivotX;
			int pivotY = m_PixelSprite.PivotY;
			int borderD = m_PixelSprite.BorderD;
			int borderU = m_PixelSprite.BorderU;
			int borderL = m_PixelSprite.BorderL;
			int borderR = m_PixelSprite.BorderR;

			// Width
			if (Layout.DelayedIntField(64, 0, 18, width, "Width", out int newWidth, 1, MAX_CANVAS_SIZE)) {
				BeforePixelChange();
				width = newWidth;
				Resize(newWidth, height);
			}
			Layout.Space(4);

			// Height
			if (Layout.DelayedIntField(64, 0, 18, height, "Height", out int newHeight, 1, MAX_CANVAS_SIZE)) {
				BeforePixelChange();
				height = newHeight;
				Resize(width, newHeight);
			}
			Layout.Space(12);

			// Pivot
			if (Layout.DelayedIntField(64, 0, 22, pivotX, "Pivot X", out int newPivotX)) {
				BeforePixelChange();
				m_PixelSprite.PivotX = pivotX = newPivotX;
			}
			Layout.Space(4);

			if (Layout.DelayedIntField(64, 0, 22, pivotY, "Pivot Y", out int newPivotY)) {
				BeforePixelChange();
				m_PixelSprite.PivotY = pivotY = newPivotY;
			}
			Layout.Space(12);

			// Border
			if (Layout.DelayedIntField(64, 0, 22, borderL, "Border L", out int newBorderL)) {
				BeforePixelChange();
				newBorderL = Mathf.Clamp(newBorderL, 0, width - borderR);
				m_PixelSprite.BorderL = borderL = newBorderL;
			}
			Layout.Space(4);

			if (Layout.DelayedIntField(64, 0, 22, borderR, "Border R", out int newBorderR)) {
				BeforePixelChange();
				newBorderR = Mathf.Clamp(newBorderR, 0, width - borderL);
				m_PixelSprite.BorderR = borderR = newBorderR;
			}
			Layout.Space(4);

			if (Layout.DelayedIntField(64, 0, 22, borderU, "Border U", out int newBorderU)) {
				BeforePixelChange();
				newBorderU = Mathf.Clamp(newBorderU, 0, height - borderD);
				m_PixelSprite.BorderU = borderU = newBorderU;
			}
			Layout.Space(4);

			if (Layout.DelayedIntField(64, 0, 22, borderD, "Border D", out int newBorderD)) {
				BeforePixelChange();
				newBorderD = Mathf.Clamp(newBorderD, 0, height - borderU);
				m_PixelSprite.BorderD = borderD = newBorderD;
			}
			Layout.Space(12);


			// Settings
			UseCheckerFloor.Value = EditorGUI.Toggle(Layout.Rect(0, 18), "Show Checker Floor", UseCheckerFloor.Value);
			Layout.Space(12);

			// Rotate
			using (new GUILayout.HorizontalScope()) {
				if (GUI.Button(Layout.Rect(0, 18), "Rotate Left")) {
					BeforePixelChange();
					m_PixelSprite.LoadFromSource(m_PixelSprite.CreateRotated(false));
					RefreshCheckfloor();
					Repaint();
				}
				if (GUI.Button(Layout.Rect(0, 18), "Rotate Right")) {
					BeforePixelChange();
					m_PixelSprite.LoadFromSource(m_PixelSprite.CreateRotated(true));
					RefreshCheckfloor();
					Repaint();
				}
			}
			Layout.Space(4);

			// Flip
			using (new GUILayout.HorizontalScope()) {
				if (GUI.Button(Layout.Rect(0, 18), "Flip X")) {
					BeforePixelChange();
					m_PixelSprite.LoadFromSource(m_PixelSprite.CreateFliped(true, false));
					Repaint();
				}
				if (GUI.Button(Layout.Rect(0, 18), "Flip Y")) {
					BeforePixelChange();
					m_PixelSprite.LoadFromSource(m_PixelSprite.CreateFliped(false, true));
					Repaint();
				}
			}
			Layout.Space(12);

			// Short Code
			if (GUI.Button(Layout.Rect(0, 18), "Copy Short Code")) {
				string code = m_PixelSprite.GetShortCode();
				if (!string.IsNullOrEmpty(code)) {
					EditorGUIUtility.systemCopyBuffer = code;
					Debug.Log(code);
				}
			}
			Layout.Space(4);

			if (GUI.Button(Layout.Rect(0, 18), "Paste Short Code")) {
				string code = EditorGUIUtility.systemCopyBuffer;
				if (!string.IsNullOrEmpty(code)) {
					BeforePixelChange();
					if (!m_PixelSprite.LoadFromShortCode(code)) {
						Debug.Log("Failed to load short code.");
					}
					RefreshCheckfloor();
					Repaint();
				}
			}
			Layout.Space(4);

			// Load Sprite
			if (Layout.QuickButton(Layout.Rect(0, 18), "Load From Sprite")) {
				SpritePickerID = Random.Range(int.MinValue, int.MaxValue);
				EditorGUIUtility.ShowObjectPicker<Sprite>(null, false, "", SpritePickerID);
			}

			// Get Picker
			if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == SpritePickerID) {
				var sprite = EditorGUIUtility.GetObjectPickerObject() as Sprite;
				BeforePixelChange();
				m_PixelSprite.LoadFromSprite(sprite);
				Repaint();
			}


			Layout.Rect(0, 0);

			// Workflow Buttons
			using (new GUILayout.HorizontalScope()) {
				// Cancel
				if (GUI.Button(Layout.Rect(0, 24), "Cancel")) {
					if (PixelsDirty) {
						int id = EditorUtil.DialogComplex("Confirm", "Close pixel editor? Unsaved changes will lost.", "Save", "Don\'t Save", "Cancel");
						if (id == 0) {
							Callback?.Invoke(m_PixelSprite);
							PixelsDirty = false;
						} else if (id == 2) {
							return;
						} else {
							PixelsDirty = false;
						}
					}
					Close();
				}
				Layout.Space(2);
				// Apply
				if (GUI.Button(Layout.Rect(0, 24), "Apply")) {
					Callback?.Invoke(m_PixelSprite);
					PixelsDirty = false;
					Close();
				}
			}
		}


		private void GUI_Hotkey () {
			if (Event.current.type != EventType.KeyDown) { return; }
			bool performed = false;
			if (!Event.current.control && !Event.current.alt && !Event.current.shift) {
				switch (Event.current.keyCode) {
					case KeyCode.M:
					case KeyCode.Alpha1:
						SelectingTool = Tools.Select;
						performed = true;
						break;
					case KeyCode.U:
					case KeyCode.Alpha2:
						SelectingTool = Tools.Rect;
						performed = true;
						break;


				}
			}
			if (performed) {
				Event.current.Use();
				Repaint();
			}
		}


		#endregion




		#region --- API ---


		public static void OpenWindow (PixelSprite pixelSprite, System.Action<PixelSprite> callback) {
			if (pixelSprite == null || pixelSprite.Colors == null || pixelSprite.Colors.Length == 0) {
				pixelSprite = new PixelSprite(1, 1);
			}
			if (pixelSprite.Width > MAX_CANVAS_SIZE || pixelSprite.Height > MAX_CANVAS_SIZE) {
				EditorUtil.Dialog("Warning", $"Pixel too large. Max size: {MAX_CANVAS_SIZE} ({pixelSprite.Width}x{pixelSprite.Height})", "OK");
				return;
			}
			var window = GetWindow<PixelEditorWindow>(true, "Pixel Editor");
			window.minSize = new Vector2(960, 583);
			window.maxSize = new Vector2(2048, 583);
			window.Callback = callback;
			window.m_PixelSprite = pixelSprite;
			window.RefreshCheckfloor();
			window.wantsMouseMove = true;
			window.wantsMouseEnterLeaveWindow = true;
			window.SelectingColor = new Color32(255, 255, 255, 255);
			window.SelectingTool = Tools.Rect;
		}


		#endregion




		#region --- LGC ---


		private void SetConfigDirty () => ConfigDirty = true;


		private void BeforePixelChange () {
			Undo.RecordObject(this, "Pixel Changed");
			PixelsDirty = true;
		}


		private void Resize (int newWidth, int newHeight) {
			var newPixels = new Color32[newWidth * newHeight];
			System.Array.Clear(newPixels, 0, newPixels.Length);
			int minWidth = Mathf.Min(m_PixelSprite.Width, newWidth);
			int minHeight = Mathf.Min(m_PixelSprite.Height, newHeight);
			for (int j = 0; j < minHeight; j++) {
				for (int i = 0; i < minWidth; i++) {
					newPixels[j * newWidth + i] = m_PixelSprite[i, j];
				}
			}
			m_PixelSprite.Colors = newPixels;
			m_PixelSprite.Width = newWidth;
			m_PixelSprite.Height = newHeight;
			m_PixelSprite.BorderL = Mathf.Clamp(m_PixelSprite.BorderL, 0, m_PixelSprite.Width - m_PixelSprite.BorderR);
			m_PixelSprite.BorderR = Mathf.Clamp(m_PixelSprite.BorderR, 0, m_PixelSprite.Width - m_PixelSprite.BorderL);
			m_PixelSprite.BorderD = Mathf.Clamp(m_PixelSprite.BorderD, 0, m_PixelSprite.Height - m_PixelSprite.BorderU);
			m_PixelSprite.BorderU = Mathf.Clamp(m_PixelSprite.BorderU, 0, m_PixelSprite.Height - m_PixelSprite.BorderD);
			RefreshCheckfloor();
		}


		private void SetPixel (int i, int j, Color32 color) {
			var sprite = m_PixelSprite;
			if (sprite == null) { return; }
			if (i >= 0 && i < sprite.Width && j >= 0 && j < sprite.Height) {
				sprite[i, j] = color.a > 0 ? Util.Blend_OneMinusAlpha(sprite[i, j], color) : new Color32(0, 0, 0, 0);
				PixelsDirty = true;
			}
		}


		private Color32 GetPixel (int i, int j) {
			var CLEAR = new Color32(0, 0, 0, 0);
			var sprite = m_PixelSprite;
			if (sprite == null) { return CLEAR; }
			var result = CLEAR;
			if (i >= 0 && i < sprite.Width && j >= 0 && j < sprite.Height) {
				result = sprite[i, j];
			}
			if (result.a == 0) {
				result = CLEAR;
			}
			return result;
		}


		private void RefreshCheckfloor () {
			CheckboardTexture = new Texture2D(m_PixelSprite.Width, m_PixelSprite.Height) { filterMode = FilterMode.Point, };
			var cPixels = new Color32[m_PixelSprite.Width * m_PixelSprite.Height];
			var COLOR0 = EditorGUIUtility.isProSkin ? new Color32(71, 71, 71, 255) : new Color32(203, 203, 203, 255);
			var COLOR1 = EditorGUIUtility.isProSkin ? new Color32(102, 102, 102, 255) : new Color32(255, 255, 255, 255);
			for (int i = 0; i < m_PixelSprite.Width; i++) {
				for (int j = 0; j < m_PixelSprite.Height; j++) {
					cPixels[j * m_PixelSprite.Width + i] = (i + j) % 2 == 0 ? COLOR0 : COLOR1;
				}
			}
			CheckboardTexture.SetPixels32(cPixels);
			CheckboardTexture.Apply();
		}


		private void MovePixels (RectInt rect, Vector2Int offset, bool copy) {
			var sprite = m_PixelSprite;
			if (sprite == null) { return; }
			var CLEAR = new Color32(0, 0, 0, 0);
			int l = Mathf.Clamp(rect.xMin, 0, sprite.Width - 1);
			int r = Mathf.Clamp(rect.xMax - 1, 0, sprite.Width - 1);
			int d = Mathf.Clamp(rect.yMin, 0, sprite.Height - 1);
			int u = Mathf.Clamp(rect.yMax - 1, 0, sprite.Height - 1);
			var pixels = new Color32[r - l + 1, u - d + 1];
			for (int i = l; i <= r; i++) {
				for (int j = d; j <= u; j++) {
					pixels[i - l, j - d] = sprite[i, j];
					if (!copy) {
						sprite[i, j] = CLEAR;
					}
				}
			}
			for (int i = l; i <= r; i++) {
				int x = i + offset.x;
				if (x < 0 || x >= sprite.Width) { continue; }
				for (int j = d; j <= u; j++) {
					int y = j + offset.y;
					if (y < 0 || y >= sprite.Height) { continue; }
					sprite[x, y] = Util.Blend_OneMinusAlpha(sprite[x, y], pixels[i - l, j - d]);
				}
			}
		}


		#endregion




	}
}