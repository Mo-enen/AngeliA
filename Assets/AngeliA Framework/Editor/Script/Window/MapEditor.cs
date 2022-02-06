using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AngeliaFramework.Entities;
using AngeliaFramework.Rendering;
using Moenen.Standard;


namespace AngeliaFramework.Editor {
	public class MapEditor : EditorWindow {




		#region --- VAR ---


		// Tool
		private enum Tool {
			Selection = 0,
			Paint = 1,
		}

		// Const
		private const string WINDOW_TITLE = "Map Editor";

		// Api
		public static MapEditor Main { get; private set; } = null;
		public Game Game { get; private set; } = null;
		public bool Painting => CurrentTool == Tool.Paint;

		// Short
		private static GUIContent SelectionToolContent => _SelectionToolContent ??= EditorGUIUtility.IconContent("d_Outline Icon");
		private static GUIContent _SelectionToolContent = null;
		private static GUIContent PaintToolContent => _PaintToolContent ??= EditorGUIUtility.IconContent("d_Grid.PaintTool@2x");
		private static GUIContent _PaintToolContent = null;
		private Tool CurrentTool {
			get => (Tool)SelectingToolIndex.Value;
			set => SelectingToolIndex.Value = (int)value;
		}

		// Data
		private readonly List<MapPalette> Palettes = new();
		private Vector2 PaletteScrollPosition = default;
		private int SelectingPaletteIndex = 0;
		private int SelectingPaletteItemIndex = 0;
		private bool NeedReloadAsset = false;
		private int CurrentPickerID = 0;
		private int PickingPalette = -1;

		// Saving
		private EditorSavingInt SelectingToolIndex = new("MapEditor.SelectingToolIndex", 0);


		#endregion




		#region --- MSG ---


		[MenuItem("AngeliA/Map Editor")]
		private static void OpenWindow () {
			try {
				var window = GetWindow<MapEditor>(WINDOW_TITLE, false);
				window.minSize = new Vector2(275, 400);
				window.maxSize = new Vector2(600, 1000);
				window.titleContent = EditorGUIUtility.IconContent("TerrainInspector.TerrainToolSplat");
				window.titleContent.text = WINDOW_TITLE;
			} catch (System.Exception ex) {
				Debug.LogWarning("Failed to open window.\n" + ex.Message);
			}
		}


		[MenuItem("Tools/Map Editor Hotkeys/Select Tool _1")]
		private static void HotKey_SelectTool () {
			if (Main == null) return;
			Main.CurrentTool = Tool.Selection;
			Main.Repaint();
			Event.current?.Use();
		}


		[MenuItem("Tools/Map Editor Hotkeys/Paint Tool _2")]
		private static void HotKey_PaintTool () {
			if (Main == null) return;
			Main.CurrentTool = Tool.Paint;
			Main.Repaint();
			Event.current?.Use();
		}


		private void OnEnable () {
			wantsMouseEnterLeaveWindow = true;
			ReloadGameAsset();
			ReloadPaletteAssets();
		}


		private void Update () {
			if (EditorApplication.isPlaying) {
				if (Game != null && Game.FindEntityOfType<eMapEditor>(EntityLayer.Debug) == null) {
					Game.AddEntity(new eMapEditor(), EntityLayer.Debug);
				}
			}
			if (Game == null) {
				ReloadGameAsset();
			}
			if (NeedReloadAsset) {
				ReloadGameAsset();
				ReloadPaletteAssets();
				NeedReloadAsset = false;
			}
		}


		private void OnGUI () {
			if (Main != this) Main = this;
			bool oldE = GUI.enabled;
			GUI.enabled = !EditorApplication.isPlaying || Game.DebugMode;
			if (EditorApplication.isPlaying) {
				GUI_Toolbar();
			}
			GUI_Inspector();
			GUI_Palette();
			GUI_Misc();
			GUI.enabled = oldE;
		}


		private void OnDestroy () {
			if (Main == this) Main = null;
		}


		private void GUI_Toolbar () {
			// Tools
			Layout.Space(10);
			using (new GUILayout.HorizontalScope()) {
				const int WIDTH = 28;
				const int HEIGHT = 28;
				Layout.Space(6);

				// Select
				if (GUI.Button(Layout.Rect(WIDTH, HEIGHT), SelectionToolContent)) {
					CurrentTool = Tool.Selection;
				}
				if (CurrentTool == Tool.Selection) {
					var _rect = Layout.LastRect();
					EditorGUI.DrawRect(_rect.Shrink(2, 2, _rect.height - 2.5f, 1), Layout.HighlightColor_Alt);
				}
				Layout.Space(2);

				// Paint
				if (GUI.Button(Layout.Rect(WIDTH, HEIGHT), PaintToolContent)) {
					CurrentTool = Tool.Paint;
				}
				if (CurrentTool == Tool.Paint) {
					var _rect = Layout.LastRect();
					EditorGUI.DrawRect(_rect.Shrink(2, 2, _rect.height - 2.5f, 1), Layout.HighlightColor_Alt);
				}

				Layout.Rect(0, HEIGHT);
			}
		}


		private void GUI_Inspector () {

			using var _ = new GUILayout.VerticalScope(Layout.PaddingPanelStyle);
			if (SelectingPaletteIndex < 0 || SelectingPaletteIndex >= Palettes.Count) return;
			var pal = Palettes[SelectingPaletteIndex];
			if (SelectingPaletteItemIndex < 0 || SelectingPaletteItemIndex >= pal.Count) return;
			var unit = pal[SelectingPaletteItemIndex];

			const int HEIGHT = 18;
			const int ICON_SIZE = 56;

			GUI.changed = false;

			Layout.Space(4);
			using (new GUILayout.HorizontalScope()) {
				// Icon
				unit.Sprite = EditorGUI.ObjectField(Layout.Rect(ICON_SIZE, ICON_SIZE), unit.Sprite, typeof(Sprite), false) as Sprite;
				Layout.Space(8);
				using (new GUILayout.VerticalScope()) {
					// Name
					GUI.Label(Layout.Rect(0, HEIGHT), unit.DisplayName, EditorStyles.boldLabel);
					// Dot
					var oldC = GUI.color;
					GUI.color = unit.IsEntity ? new Color32(0, 255, 200, 255) : new Color32(255, 200, 0, 255);
					GUI.Label(Layout.Rect(0, HEIGHT), unit.IsEntity ? "¡ñ Entity" : "¡ñ Block");
					GUI.color = oldC;
				}
			}
			if (GUI.changed) {
				EditorUtility.SetDirty(pal);
				AssetDatabase.SaveAssetIfDirty(pal);
			}
		}


		private void GUI_Palette () {
			using var scroll = new GUILayout.ScrollViewScope(
				PaletteScrollPosition, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, Layout.MasterScrollStyle
			);
			PaletteScrollPosition = scroll.scrollPosition;
			bool mouseDown = Event.current.type == EventType.MouseDown;
			int clickPal = -1;
			int clickItem = -1;
			bool enable = GUI.enabled && CurrentTool == Tool.Paint;
			// Remove Null
			for (int i = 0; i < Palettes.Count; i++) {
				if (Palettes[i] == null) {
					Palettes.RemoveAt(i);
					i--;
				}
			}
			// All Palettes
			bool oldE = GUI.enabled;
			var oldC = GUI.color;
			for (int palIndex = 0; palIndex < Palettes.Count; palIndex++) {
				var pal = Palettes[palIndex];
				if (pal == null) { continue; }
				const int ITEM_GAP = 4;
				const int ITEM_SIZE = 40;
				int COLUMN = ((EditorGUIUtility.currentViewWidth - 8f) / (ITEM_SIZE + ITEM_GAP)).FloorToInt();
				//int COLUMN = 4;
				bool opening = pal.Opening;
				if (Layout.Fold(pal.name, ref opening)) {
					Layout.Space(2);
					GUI.enabled = enable;
					int count = pal.Count;
					int rowCount = Mathf.CeilToInt(count / (float)COLUMN);
					for (int y = 0, i = 0; y < rowCount; y++) {
						using (new GUILayout.HorizontalScope()) {
							for (int x = 0; x < COLUMN && i < count; x++, i++) {
								var unit = pal[i];
								var rect = Layout.Rect(ITEM_SIZE, ITEM_SIZE);
								Layout.Space(ITEM_GAP);
								// Background
								GUI.Label(rect, GUIContent.none, GUI.skin.textField);
								// Icon
								var icon = unit.Sprite;
								if (icon != null && icon.texture != null) {
									float tWidth = icon.texture.width;
									float tHeight = icon.texture.height;
									GUI.color = enable ? oldC : new Color(1, 1, 1, 0.3f);
									GUI.DrawTextureWithTexCoords(
										rect.Shrink(8).Shift(0, 2).Fit(icon.rect.width / icon.rect.height),
										icon.texture,
										new Rect(
											icon.rect.x / tWidth,
											icon.rect.y / tHeight,
											icon.rect.width / tWidth,
											icon.rect.height / tHeight
										)
									);
									GUI.color = oldC;
								}
								// Highlight
								if (enable && SelectingPaletteIndex == palIndex && SelectingPaletteItemIndex == i) {
									Layout.FrameGUI(rect.Shrink(1.5f), 1.5f, Layout.HighlightColor_Alt);
								}
								// Click
								if (mouseDown && rect.Contains(Event.current.mousePosition)) {
									clickPal = palIndex;
									clickItem = i;
								}
							}
						}
						Layout.Space(ITEM_GAP);
					}
					GUI.enabled = oldE;
					// Add Button
					if (count == 0) {
						if (GUI.Button(Layout.Rect(96, 18).Shrink(24, 0, 0, 0), "New Block", EditorStyles.linkLabel)) {
							PickingPalette = palIndex;
							CurrentPickerID = GUIUtility.GetControlID(FocusType.Passive) + 100;
							EditorGUIUtility.ShowObjectPicker<Sprite>(null, false, "", CurrentPickerID);
						}
						EditorGUIUtility.AddCursorRect(Layout.LastRect().Shrink(24, 0, 0, 0), MouseCursor.Link);
						Layout.Space(2);

						if (GUI.Button(Layout.Rect(96, 18).Shrink(24, 0, 0, 0), "New Entity", EditorStyles.linkLabel)) {
							var menu = new GenericMenu();
							Menu_NewEntity(menu, pal, false);
							menu.ShowAsContext();
						}
						EditorGUIUtility.AddCursorRect(Layout.LastRect().Shrink(24, 0, 0, 0), MouseCursor.Link);
						Layout.Space(2);
					}
				}
				Layout.Space(2);
				if (pal.Opening != opening) {
					pal.Opening = opening;
					EditorUtility.SetDirty(pal);
					AssetDatabase.SaveAssets();
				}
			}
			// Click
			if (clickPal >= 0 && clickItem >= 0) {
				if (enable && Event.current.button == 0) {
					// Left Button
					SelectingPaletteIndex = clickPal;
					SelectingPaletteItemIndex = clickItem;
				} else if (Event.current.button == 1) {
					// Right Button
					ShowPaletteMenu(clickPal, clickItem);
				}
			}
		}


		private void GUI_Misc () {

			Layout.CancelFocusOnClick(this);

			// Leave Window
			if (Event.current.type == EventType.MouseLeaveWindow && EditorApplication.isPlaying) {
				EditorApplication.ExecuteMenuItem("Window/General/Game");
			}

			// Sprite Picker
			if (
				Event.current.type == EventType.ExecuteCommand &&
				Event.current.commandName == "ObjectSelectorClosed" &&
				EditorGUIUtility.GetObjectPickerControlID() == CurrentPickerID &&
				PickingPalette >= 0 && PickingPalette < Palettes.Count
			) {
				// Add Block
				var sprite = EditorGUIUtility.GetObjectPickerObject() as Sprite;
				if (sprite != null) {
					var pal = Palettes[PickingPalette];
					pal.Add(new MapPalette.Unit() {
						Sprite = sprite,
						TypeFullName = "",
					});
					pal.Sort();
					EditorUtility.SetDirty(pal);
					AssetDatabase.SaveAssetIfDirty(pal);
					AssetDatabase.Refresh();
					PickingPalette = -1;
				}
			}

		}


		#endregion




		#region --- API ---


		public void SetNeedReloadAsset () => NeedReloadAsset = true;


		public MapPalette.Unit GetSelection () {
			if (
				SelectingPaletteIndex >= 0 &&
				SelectingPaletteIndex < Palettes.Count &&
				SelectingPaletteItemIndex >= 0 &&
				SelectingPaletteItemIndex < Palettes[SelectingPaletteIndex].Count
			) {
				return Palettes[SelectingPaletteIndex][SelectingPaletteItemIndex];
			}
			return null;
		}


		#endregion




		#region --- LGC ---


		private void ReloadGameAsset () {
			Game = null;
			foreach (var guid in AssetDatabase.FindAssets("t:Game")) {
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var game = AssetDatabase.LoadAssetAtPath<Game>(path);
				if (game != null) {
					Game = game;
					break;
				}
			}
		}


		private void ReloadPaletteAssets () {
			Palettes.Clear();
			var guids = AssetDatabase.FindAssets($"t:{nameof(MapPalette)}");
			foreach (var guid in guids) {
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var pal = AssetDatabase.LoadAssetAtPath<MapPalette>(path);
				pal.Sort();
				Palettes.Add(pal);
			}
		}


		private void ShowPaletteMenu (int paletteIndex, int itemIndex) {

			if (paletteIndex < 0 || paletteIndex >= Palettes.Count) return;
			var pal = Palettes[paletteIndex];
			if (itemIndex < 0 || itemIndex >= pal.Count) return;
			var unit = pal[itemIndex];

			var menu = new GenericMenu();

			menu.AddItem(new GUIContent("New Block"), false, () => {
				PickingPalette = paletteIndex;
				CurrentPickerID = GUIUtility.GetControlID(FocusType.Passive) + 100;
				EditorGUIUtility.ShowObjectPicker<Sprite>(null, false, "", CurrentPickerID);
			});

			Menu_NewEntity(menu, pal, true);

			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Select Asset"), false, () => {
				Selection.activeObject = pal;
			});
			menu.AddItem(new GUIContent("Delete"), false, () => {
				var sp = pal[itemIndex].Sprite;
				if (EditorUtility.DisplayDialog("", $"Delete Item {(sp != null ? sp.name : "")}?", "Delete", "Cancel")) {
					pal.RemoveUnit(itemIndex);
					EditorUtility.SetDirty(pal);
					AssetDatabase.SaveAssetIfDirty(pal);
					AssetDatabase.Refresh();
					Repaint();
				}
			});

			menu.ShowAsContext();
		}


		private void Menu_NewEntity (GenericMenu menu, MapPalette pal, bool prefix) {
			foreach (var type in typeof(Entity).GetAllChildClass()) {
				string fullName = type.FullName;
				menu.AddItem(new GUIContent(prefix ? $"New Entity/{type.Name}" : type.Name), false, () => {
					pal.Add(new MapPalette.Unit() {
						Sprite = null,
						TypeFullName = fullName,
					});
					pal.Sort();
					EditorUtility.SetDirty(pal);
					AssetDatabase.SaveAssetIfDirty(pal);
					AssetDatabase.Refresh();
				});
			}
		}


		#endregion




	}
}