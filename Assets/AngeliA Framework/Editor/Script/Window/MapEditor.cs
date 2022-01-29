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


		// Const
		private const string WINDOW_TITLE = "Map Editor";

		// Data
		private Vector2 MasterScrollPosition = default;
		private readonly List<MapEditor_PaletteGroup> PaletteGroups = new();
		private ReorderableList PaletteGroupList = null;

		// Saving
		private EditorSavingString GroupAssetsGuids = new("MapEditor.GroupAssetsGuids", "");


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
			using var scroll = new GUILayout.ScrollViewScope(MasterScrollPosition, Layout.MasterScrollStyle);
			MasterScrollPosition = scroll.scrollPosition;
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



		}


		private void GUI_Palette () {
			foreach (var pGroup in PaletteGroups) {
				bool opening = pGroup.Opening;
				if (Layout.Fold(pGroup.name, ref opening)) {



				}
				Layout.Space(2);
				pGroup.Opening = opening;
			}
		}


		private void GUI_Resource () {
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
