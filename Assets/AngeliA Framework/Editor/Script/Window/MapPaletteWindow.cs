using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Moenen.Standard;


namespace AngeliaFramework.Editor {
	public class MapPaletteWindow : EditorWindow {




		#region --- VAR ---


		// Const
		private const string WINDOW_TITLE = "Palette";
		private static readonly string[] PAL_TAGS_DISPLAY = new string[] {
			"(None)", "Water",
		};
		private static readonly int[] PAL_TAGS_VALUE = new int[] {
			0, "Water".ACode(),
		};

		// Short
		private static GUIContent SelectionToolContent => _SelectionToolContent ??= EditorGUIUtility.IconContent("d_Outline Icon");
		private static GUIContent _SelectionToolContent = null;
		private static GUIContent PaintToolContent => _PaintToolContent ??= EditorGUIUtility.IconContent("d_Grid.PaintTool@2x");
		private static GUIContent _PaintToolContent = null;
		private static Game Game => _Game != null ? _Game : (_Game = FindObjectOfType<Game>());
		private static Game _Game = null;

		// Data
		private readonly List<MapPalette> Palettes = new();
		private Vector2 PaletteScrollPosition = default;
		private static bool NeedReloadAsset = false;
		private static bool NeedClearSelection = false;
		private int SelectingPaletteIndex = 0;
		private int SelectingPaletteItemIndex = 0;
		private int CurrentPickerID = 0;
		private int PickingPalette = -1;
		private int LastSpawnEditorEntityFrame = int.MinValue;


		#endregion




		#region --- MSG ---


		[MenuItem("AngeliA/Map Palette")]
		private static void OpenWindow () {
			try {
				var window = GetWindow<MapPaletteWindow>(WINDOW_TITLE, false);
				window.minSize = new Vector2(275, 400);
				window.maxSize = new Vector2(600, 1000);
				window.titleContent = EditorGUIUtility.IconContent("TerrainInspector.TerrainToolSplat");
				window.titleContent.text = WINDOW_TITLE;
			} catch (System.Exception ex) {
				Debug.LogWarning("Failed to open window.\n" + ex.Message);
			}
		}


		private void OnEnable () {
			wantsMouseEnterLeaveWindow = true;
			ReloadPaletteAssets();
		}


		private void Update () {
			if (EditorApplication.isPlaying) {
				if (Game != null && Game.GlobalFrame != LastSpawnEditorEntityFrame && Game.FindEntityOfType<eMapEditor>(EntityLayer.Debug) == null) {
					Game.AddEntity(new eMapEditor());
					LastSpawnEditorEntityFrame = Game.GlobalFrame;
				}
			}
			if (NeedReloadAsset) {
				ReloadPaletteAssets();
				NeedReloadAsset = false;
			}
			if (NeedClearSelection) {
				SelectingPaletteIndex = -1;
				SelectingPaletteItemIndex = -1;
				NeedClearSelection = false;
			}
		}


		private void OnGUI () {

			if (Palettes.Count == 0) return;
			SelectingPaletteIndex = Mathf.Clamp(SelectingPaletteIndex, 0, Palettes.Count - 1);
			var pal = Palettes[SelectingPaletteIndex];
			SelectingPaletteItemIndex = Mathf.Clamp(SelectingPaletteItemIndex, 0, pal.Count - 1);

			bool oldE = GUI.enabled;
			GUI.enabled = !EditorApplication.isPlaying || (Game != null && Game.DebugMode);
			GUI_Inspector();
			GUI_Palette();
			GUI_Misc();
			GUI.enabled = oldE;

		}


		private void GUI_Inspector () {
			const int HEIGHT = 18;
			const int ICON_SIZE = 56;
			using (new GUILayout.VerticalScope(Layout.BoxPaddingPanelStyle)) {

				if (SelectingPaletteIndex < 0 || SelectingPaletteIndex >= Palettes.Count) return;
				var pal = Palettes[SelectingPaletteIndex];
				if (SelectingPaletteItemIndex < 0 || SelectingPaletteItemIndex >= pal.Count) return;
				var unit = pal[SelectingPaletteItemIndex];

				EditorGUI.BeginChangeCheck();

				Layout.Space(4);
				var oldL = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 46;
				using (new GUILayout.HorizontalScope()) {
					// Icon
					unit.Sprite = EditorGUI.ObjectField(Layout.Rect(ICON_SIZE, ICON_SIZE), unit.Sprite, typeof(Sprite), false) as Sprite;
					Layout.Space(8);
					using (new GUILayout.VerticalScope()) {

						// Name
						GUI.Label(Layout.Rect(0, HEIGHT), unit.DisplayName, EditorStyles.boldLabel);
						Layout.Space(2);

						if (!unit.IsEntity) {
							// Block Only

							// Tag
							unit.Tag = EditorGUI.IntPopup(
								Layout.Rect(0, 16), "Tag", unit.Tag, PAL_TAGS_DISPLAY, PAL_TAGS_VALUE
							);
							Layout.Space(5);

							// Layer
							unit.BlockLayer = (BlockLayer)EditorGUI.EnumPopup(
								Layout.Rect(0, 16), "Layer", unit.BlockLayer
							);
							Layout.Space(5);

							// Trigger
							using (new EditorGUI.DisabledGroupScope(unit.BlockLayer == BlockLayer.Background)) {
								unit.IsTrigger = EditorGUI.Toggle(Layout.Rect(0, 16), "Trigger", unit.IsTrigger);
							}

						} else {
							Layout.Space(16 + 5 + 16 + 5 + 16);
						}

					}
				}

				EditorGUIUtility.labelWidth = oldL;

				if (EditorGUI.EndChangeCheck()) {
					EditorUtility.SetDirty(pal);
					if (!EditorApplication.isPlaying) {
						UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
					}
				}
			}
			EditorGUI.DrawRect(Layout.Rect(0, 1), new Color32(26, 26, 26, 255));
		}


		private void GUI_Palette () {
			using var scroll = new GUILayout.ScrollViewScope(
				PaletteScrollPosition, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, Layout.MasterScrollStyle
			);
			PaletteScrollPosition = scroll.scrollPosition;
			bool mouseDown = Event.current.type == EventType.MouseDown;
			int clickPal = -1;
			int clickItem = -1;
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
				bool opening = pal.Opening;
				if (Layout.Fold(pal.name, ref opening)) {
					Layout.Space(2);
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
								}
								// Highlight
								if (SelectingPaletteIndex == palIndex && SelectingPaletteItemIndex == i) {
									Layout.FrameGUI(rect.Shrink(1.5f), 1.5f, Layout.HighlightColor1);
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
					GUI.enabled = oldE;
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
				if (Event.current.button == 0) {
					// Left Button
					SetSelection(clickPal, clickItem);
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


		public static void SetNeedReloadAsset () => NeedReloadAsset = true;


		public static void RequireClearSelection () => NeedClearSelection = true;


		public void SetSelection (int palIndex, int itemIndex) {
			if (
				palIndex >= 0 && palIndex < Palettes.Count &&
				itemIndex >= 0 && itemIndex < Palettes[palIndex].Count
			) {
				SelectingPaletteIndex = palIndex;
				SelectingPaletteItemIndex = itemIndex;
				Repaint();
				eMapEditor.SelectingUnit = Palettes[SelectingPaletteIndex][SelectingPaletteItemIndex];
			}
		}


		#endregion




		#region --- LGC ---


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