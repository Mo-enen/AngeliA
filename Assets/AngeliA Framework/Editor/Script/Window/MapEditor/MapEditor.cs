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
		private static GUIContent PaletteIconSizeContent => _PaletteIconSizeContent ??= EditorGUIUtility.IconContent("d_CameraPreview@2x");
		private static GUIContent _PaletteIconSizeContent = null;
		private static GUIContent SelectionToolContent => _SelectionToolContent ??= EditorGUIUtility.IconContent("d_Outline Icon");
		private static GUIContent _SelectionToolContent = null;
		private static GUIContent PaintToolContent => _PaintToolContent ??= EditorGUIUtility.IconContent("d_Grid.PaintTool@2x");
		private static GUIContent _PaintToolContent = null;
		private Tool CurrentTool {
			get => (Tool)SelectingToolIndex.Value;
			set => SelectingToolIndex.Value = (int)value;
		}

		// Data
		private readonly List<MapEditor_PaletteGroup> PaletteGroups = new();
		private Game Game = null;
		private Vector2 PaletteScrollPosition = default;
		private int SelectingPaletteGroupIndex = 0;
		private int SelectingPaletteItemIndex = 0;
		private static bool NeedReloadAsset = false;

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
			Event.current.Use();
		}


		[MenuItem("Tools/Map Editor Hotkeys/Paint Tool _2")]
		private static void HotKey_PaintTool () {
			if (Main == null) return;
			Main.CurrentTool = Tool.Paint;
			Main.Repaint();
			Event.current.Use();
		}


		private void OnEnable () {
			ReloadGameAsset();
			ReloadPaletteGroupAssets();
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
				ReloadPaletteGroupAssets();
				NeedReloadAsset = false;
			}
		}


		private void OnGUI () {
			if (Main != this) Main = this;
			GUI_Toolbar();
			GUI_Palette();
			Layout.CancelFocusOnClick(this);
		}


		private void OnDestroy () {
			if (Main == this) Main = null;
		}


		private void GUI_Toolbar () {
			// Tools
			Layout.Space(6);
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
			Layout.Space(2);
		}


		private void GUI_Palette () {
			using var scroll = new GUILayout.ScrollViewScope(
				PaletteScrollPosition, Layout.MasterScrollStyle
			);
			PaletteScrollPosition = scroll.scrollPosition;
			bool mouseDown = Event.current.type == EventType.MouseDown;
			int clickGroup = -1;
			int clickItem = -1;
			// Remove Null
			for (int i = 0; i < PaletteGroups.Count; i++) {
				if (PaletteGroups[i] == null) {
					PaletteGroups.RemoveAt(i);
					i--;
				}
			}
			// All Groups
			for (int groupIndex = 0; groupIndex < PaletteGroups.Count; groupIndex++) {
				var pGroup = PaletteGroups[groupIndex];
				if (pGroup == null) { continue; }
				const int ITEM_GAP = 4;
				const int ITEM_SIZE = 48;
				int COLUMN = ((EditorGUIUtility.currentViewWidth - 24f) / ITEM_SIZE).FloorToInt();
				bool opening = pGroup.Opening;
				if (Layout.Fold(pGroup.name, ref opening)) {
					int bCount = pGroup.Blocks.Length;
					int eCount = pGroup.Entities.Length;
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
									pGroup.Blocks[i].Sprite :
									pGroup.Entities[i - bCount].Icon;
								if (icon != null && icon.texture != null) {
									float tWidth = icon.texture.width;
									float tHeight = icon.texture.height;
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
								}
								// Highlight
								if (SelectingPaletteGroupIndex == groupIndex && SelectingPaletteItemIndex == i) {
									Layout.FrameGUI(rect.Shrink(2), 2f, HIGHLIGHT);
								}
								// Click
								if (mouseDown && rect.Contains(Event.current.mousePosition)) {
									clickGroup = groupIndex;
									clickItem = i;
								}
							}
						}
						Layout.Space(ITEM_GAP);
					}
				}
				Layout.Space(2);
				pGroup.Opening = opening;
			}
			// Click
			if (clickGroup >= 0 && clickItem >= 0) {
				SelectingPaletteGroupIndex = clickGroup;
				SelectingPaletteItemIndex = clickItem;
			}
		}


		#endregion




		#region --- API ---


		public static void SetNeedReloadAsset () => NeedReloadAsset = true;


		#endregion




		#region --- LGC ---


		private void ReloadPaletteGroupAssets () {
			PaletteGroups.Clear();
			var guids = AssetDatabase.FindAssets("t:MapEditor_PaletteGroup");
			foreach (var guid in guids) {
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var group = AssetDatabase.LoadAssetAtPath<MapEditor_PaletteGroup>(path);
				PaletteGroups.Add(group);
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