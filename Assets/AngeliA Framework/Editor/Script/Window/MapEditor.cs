using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Moenen.Standard;


namespace AngeliaFramework.Editor {
	public class MapEditor : EditorWindow {




		#region --- VAR ---


		// SUB
		private enum Tool {
			Selection = 0,
			Paint = 1,
		}

		// Const
		private const string WINDOW_TITLE = "Map Editor";

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
		private Vector2 MasterScrollPosition = default;
		private readonly List<MapEditor_PaletteGroup> PaletteGroups = new();
		private ReorderableList PaletteGroupList = null;
		private int SelectingPaletteGroupIndex = 0;
		private int SelectingPaletteItemIndex = 0;

		// Saving
		private EditorSavingString GroupAssetsGuids = new("MapEditor.GroupAssetsGuids", "");
		private EditorSavingBool UseBigPaletteItem = new("MapEditor.UseBigPaletteItem", true);
		private EditorSavingInt SelectingToolIndex = new("MapEditor.SelectingToolIndex", 0);


		#endregion




		#region --- MSG ---


		[MenuItem("AngeliA/Map Editor")]
		private static void OpenWindow () => GetOrCreateWindow();


		private void OnEnable () {
			LoadGroupAssets();
			Enable_PaletteGroupList();
		}


		private void Enable_PaletteGroupList () {
			PaletteGroupList = new ReorderableList(
				PaletteGroups, typeof(MapEditor_PaletteGroup),
				true, true, true, true
			) {
				elementHeight = 20,
				drawElementCallback = (rect, index, active, focus) => {
					rect.height -= 2;
					rect.y++;
					PaletteGroups[index] = EditorGUI.ObjectField(rect, PaletteGroups[index], typeof(MapEditor_PaletteGroup), false) as MapEditor_PaletteGroup;
				},
				onAddCallback = (list) => list.list.Add(null),
				drawHeaderCallback = (rect) => GUI.Label(rect, "Palette Group Assets"),
			};
		}


		private void OnGUI () {
			if (EditorApplication.isPlaying) {
				// Runtime
				GUI_Toolbar();
				Layout.Space(2);
				GUI_Palette();
			} else {
				// Edittime
				GUI_Resource();
			}
			Layout.CancelFocusOnClick(this);
		}


		private void GUI_Toolbar () {
			// Bar
			using (new GUILayout.HorizontalScope(EditorStyles.toolbar)) {
				const int HEIGHT = 22;
				UseBigPaletteItem.Value = GUI.Toggle(
					Layout.Rect(22, HEIGHT),
					UseBigPaletteItem.Value,
					PaletteIconSizeContent,
					EditorStyles.toolbarButton
				);
				Layout.Rect(0, HEIGHT);
			}
			Layout.Space(2);
			// Tools
			using (new GUILayout.HorizontalScope()) {
				const int WIDTH = 28;
				const int HEIGHT = 28;
				var oldB = GUI.backgroundColor;
				Layout.Rect(0, HEIGHT);

				GUI.backgroundColor = CurrentTool == Tool.Selection ? new Color32(128, 128, 128, 255) : oldB;
				if (GUI.Button(Layout.Rect(WIDTH, HEIGHT), SelectionToolContent)) {
					CurrentTool = Tool.Selection;
				}
				Layout.Space(2);

				GUI.backgroundColor = CurrentTool == Tool.Paint ? new Color32(128, 128, 128, 255) : oldB;
				if (GUI.Button(Layout.Rect(WIDTH, HEIGHT), PaintToolContent)) {
					CurrentTool = Tool.Paint;
				}

				Layout.Rect(0, HEIGHT);
				GUI.backgroundColor = oldB;
			}
			Layout.Space(2);
		}


		private void GUI_Palette () {
			using var scroll = new GUILayout.ScrollViewScope(MasterScrollPosition, Layout.MasterScrollStyle);
			MasterScrollPosition = scroll.scrollPosition;
			bool mouseDown = Event.current.type == EventType.MouseDown;
			int clickGroup = -1;
			int clickItem = -1;
			for (int groupIndex = 0; groupIndex < PaletteGroups.Count; groupIndex++) {
				var pGroup = PaletteGroups[groupIndex];
				const int ITEM_GAP = 4;
				int ITEM_SIZE = UseBigPaletteItem.Value ? 48 : 32;
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
										rect.Shrink(2).Fit(icon.rect.width / icon.rect.height),
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
									Layout.FrameGUI(rect.Shrink(2), 4f, new Color32(36, 181, 161, 255));
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


		private void GUI_Resource () {
			using var scroll = new GUILayout.ScrollViewScope(MasterScrollPosition, Layout.MasterScrollStyle);
			MasterScrollPosition = scroll.scrollPosition;
			using var change = new EditorGUI.ChangeCheckScope();
			PaletteGroupList.DoLayoutList();
			if (change.changed) {
				SaveGroupAssets();
			}
		}


		#endregion




		#region --- LGC ---


		private static MapEditor GetOrCreateWindow () {
			try {
				var window = GetWindow<MapEditor>(WINDOW_TITLE, false);
				window.minSize = new Vector2(275, 400);
				window.maxSize = new Vector2(600, 1000);
				window.titleContent = EditorGUIUtility.IconContent("TerrainInspector.TerrainToolSplat");
				window.titleContent.text = WINDOW_TITLE;
				return window;
			} catch (System.Exception ex) {
				Debug.LogWarning("Failed to open window.\n" + ex.Message);
			}
			return null;
		}


		private void LoadGroupAssets () {
			PaletteGroups.Clear();
			var guids = GroupAssetsGuids.Value.Split('\n');
			foreach (var guid in guids) {
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var group = AssetDatabase.LoadAssetAtPath<MapEditor_PaletteGroup>(path);
				if (group != null) {
					PaletteGroups.Add(group);
				}
			}
		}


		private void SaveGroupAssets () {
			var builder = new StringBuilder();
			foreach (var group in PaletteGroups) {
				var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(group));
				if (!string.IsNullOrEmpty(guid)) {
					builder.Append(guid);
					builder.Append('\n');
				}
			}
			GroupAssetsGuids.Value = builder.ToString();
		}


		#endregion




	}
}
