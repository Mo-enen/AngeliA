using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Moenen.Standard;
using AngeliaFramework.Entities;


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
		private readonly static Color HIGHLIGHT = new Color32(36, 181, 161, 255);

		// Api
		public static MapEditor Main { get; private set; } = null;

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
		private Game Game = null;
		private Vector2 PaletteScrollPosition = default;
		private int SelectingPaletteIndex = 0;
		private int SelectingPaletteItemIndex = 0;
		private bool NeedReloadAsset = false;

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
				if (Game != null && Game.FindEntityOfType<eMapEditor>(EntityLayer.UI) == null) {
					Game.AddEntity(new eMapEditor(), EntityLayer.UI);
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
			GUI_Toolbar();
			GUI_Palette();
			Layout.CancelFocusOnClick(this);
			if (Event.current.type == EventType.MouseLeaveWindow && EditorApplication.isPlaying) {
				EditorApplication.ExecuteMenuItem("Window/General/Game");
			}
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
					EditorGUI.DrawRect(_rect.Shrink(2, 2, _rect.height - 3, 1), HIGHLIGHT);
				}
				Layout.Space(2);

				// Paint
				if (GUI.Button(Layout.Rect(WIDTH, HEIGHT), PaintToolContent)) {
					CurrentTool = Tool.Paint;
				}
				if (CurrentTool == Tool.Paint) {
					var _rect = Layout.LastRect();
					EditorGUI.DrawRect(_rect.Shrink(2, 2, _rect.height - 3, 1), HIGHLIGHT);
				}

				Layout.Rect(0, HEIGHT);
			}
		}


		private void GUI_Palette () {
			using var scroll = new GUILayout.ScrollViewScope(
				PaletteScrollPosition, Layout.MasterScrollStyle
			);
			PaletteScrollPosition = scroll.scrollPosition;
			bool mouseDown = Event.current.type == EventType.MouseDown;
			int clickPal = -1;
			int clickItem = -1;
			bool enable = CurrentTool == Tool.Paint;
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
				const int ITEM_SIZE = 48;
				int COLUMN = ((EditorGUIUtility.currentViewWidth - 24f) / ITEM_SIZE).FloorToInt();
				bool opening = pal.Opening;
				if (Layout.Fold(pal.name, ref opening)) {
					GUI.enabled = enable;
					int bCount = pal.Blocks.Length;
					int eCount = pal.Entities.Length;
					int count = bCount + eCount;
					int rowCount = Mathf.CeilToInt(count / (float)COLUMN);
					for (int y = 0, i = 0; y < rowCount; y++) {
						using (new GUILayout.HorizontalScope()) {
							for (int x = 0; x < COLUMN && i < count; x++, i++) {
								var rect = Layout.Rect(ITEM_SIZE, ITEM_SIZE);
								Layout.Space(ITEM_GAP);
								// Background
								GUI.Label(rect, GUIContent.none, GUI.skin.textField);
								// Icon
								var icon = i < bCount ?
									pal.Blocks[i].Sprite :
									pal.Entities[i - bCount].Sprite;
								if (icon != null && icon.texture != null) {
									float tWidth = icon.texture.width;
									float tHeight = icon.texture.height;
									GUI.color = enable ? oldC : new Color(1, 1, 1, 0.3f);
									GUI.DrawTextureWithTexCoords(
										rect.Shrink(12).Shift(0, 4).Fit(icon.rect.width / icon.rect.height),
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
									Layout.FrameGUI(rect.Shrink(2), 2f, HIGHLIGHT);
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
				}
				Layout.Space(2);
				pal.Opening = opening;
			}
			// Click
			if (enable && clickPal >= 0 && clickItem >= 0) {
				SelectingPaletteIndex = clickPal;
				SelectingPaletteItemIndex = clickItem;
			}
		}


		#endregion




		#region --- API ---


		public void SetNeedReloadAsset () => NeedReloadAsset = true;


		public MapPalette.Block GetSelection () {
			if (
				SelectingPaletteIndex >= 0 &&
				SelectingPaletteIndex < Palettes.Count &&
				SelectingPaletteItemIndex >= 0 &&
				SelectingPaletteItemIndex < Palettes[SelectingPaletteIndex].AllCount
			) {
				return Palettes[SelectingPaletteIndex][SelectingPaletteItemIndex];
			}
			return null;
		}


		#endregion




		#region --- LGC ---


		private void ReloadPaletteAssets () {
			Palettes.Clear();
			var guids = AssetDatabase.FindAssets($"t:{nameof(MapPalette)}");
			foreach (var guid in guids) {
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var pal = AssetDatabase.LoadAssetAtPath<MapPalette>(path);
				Palettes.Add(pal);
			}
		}


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


		#endregion




	}
}