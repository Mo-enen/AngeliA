using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Moenen.Standard;


namespace AngeliaFramework.Editor {
	public class MapNavigator : EditorWindow {




		#region --- VAR ---


		// Const
		private const string WINDOW_TITLE = "Navigator";

		// Short
		private static Game Game => _Game != null ? _Game : (_Game = FindObjectOfType<Game>());
		private static Game _Game = null;

		// Data
		private static MapNavigator Main = null;
		private readonly List<Vector2Int> MapPositions = new();
		private Vector2Int MapPositionMin = default;
		private Vector2Int MapPositionMax = default;
		private Rect ContentRect = default;
		private RectInt PrevCameraRect = default;


		#endregion




		#region --- MSG ---


		[MenuItem("AngeliA/Map Navigator")]
		public static void OpenWindow () {
			try {
				var window = GetWindow<MapNavigator>(WINDOW_TITLE, false);
				window.minSize = new Vector2(275, 400);
				window.maxSize = new Vector2(600, 1000);
				window.titleContent = EditorGUIUtility.IconContent("TerrainInspector.TerrainToolRaise On");
				window.titleContent.text = WINDOW_TITLE;
			} catch (System.Exception ex) {
				Debug.LogWarning("Failed to open window.\n" + ex.Message);
			}
		}


		private void OnEnable () => Main = this;


		private void OnGUI () {
			Main = this;
			GUI_Toolbar();
			GUI_Content();
			GUI_View();
			GUI_Key();
		}


		private void Update () {
			if (!EditorApplication.isPlaying || Main == null) return;
			if (CellRenderer.CameraRect.IsNotSame(Main.PrevCameraRect)) {
				Main.PrevCameraRect = CellRenderer.CameraRect;
				Main.Repaint();
			}
		}


		private void GUI_Toolbar () {
			using (new GUILayout.HorizontalScope(EditorStyles.toolbar)) {
				const int HEIGHT = 20;




				Layout.Rect(0, HEIGHT);
			}
		}


		private void GUI_Content () {
			// Get Maps
			if (MapPositions.Count == 0) {
				MapPositionMin = new Vector2Int(int.MaxValue, int.MaxValue);
				MapPositionMax = new Vector2Int(int.MinValue, int.MinValue);
				foreach (var file in Util.GetFilesIn(Util.CombinePaths(Application.dataPath, "Resources", "Map"), true, "*.asset")) {
					try {
						string name = Util.GetNameWithoutExtension(file.Name);
						int dIndex = name.IndexOf('_');
						var pos = new Vector2Int(
							int.Parse(name[..dIndex]),
							int.Parse(name[(dIndex + 1)..])
						);
						MapPositions.Add(pos);
						MapPositionMin = Vector2Int.Min(MapPositionMin, pos);
						MapPositionMax = Vector2Int.Max(MapPositionMax, pos);
					} catch (System.Exception ex) { Debug.LogException(ex); }
				}
			}
			// Draw Maps
			Layout.Space(6);
			using (new GUILayout.HorizontalScope()) {
				Layout.Space(6);
				ContentRect = Layout.Rect(0, 0).Fit((float)(MapPositionMax.x - MapPositionMin.x) / (MapPositionMax.y - MapPositionMin.y));
				var normalColor = new Color32(96, 96, 96, 255);
				var dotSize = new Vector2(
					ContentRect.width / (MapPositionMax.x - MapPositionMin.x + 1),
					ContentRect.height / (MapPositionMax.y - MapPositionMin.y + 1)
				);
				foreach (var pos in MapPositions) {
					EditorGUI.DrawRect(new Rect(
						ContentRect.x + (pos.x - MapPositionMin.x) * dotSize.x,
						ContentRect.y + (MapPositionMax.y - (pos.y - MapPositionMin.y) + 1) * dotSize.y,
						dotSize.x,
						dotSize.y
					).Shrink(1), normalColor);
				}
				Layout.Space(6);
			}
			Layout.Space(6);
		}


		private void GUI_View () {
			if (!EditorApplication.isPlaying || Game == null) return;

			// Draw View Rect 
			var cameraRect = CellRenderer.CameraRect.ToRect();
			var worldRect = new Rect(
				cameraRect.x / Const.CELL_SIZE / Const.WORLD_MAP_SIZE,
				cameraRect.y / Const.CELL_SIZE / Const.WORLD_MAP_SIZE,
				cameraRect.width / Const.CELL_SIZE / Const.WORLD_MAP_SIZE,
				cameraRect.height / Const.CELL_SIZE / Const.WORLD_MAP_SIZE
			);
			var dotSize = new Vector2(
				ContentRect.width / (MapPositionMax.x - MapPositionMin.x + 1),
				ContentRect.height / (MapPositionMax.y - MapPositionMin.y + 1)
			);
			float viewContentSizeX = worldRect.width * dotSize.x;
			float viewContentSizeY = worldRect.height * dotSize.y;
			Layout.FrameGUI(new(
				Util.RemapUnclamped(
					MapPositionMin.x, MapPositionMax.x + 1,
					ContentRect.xMin, ContentRect.xMax,
					worldRect.x
				),
				Util.RemapUnclamped(
					MapPositionMin.y, MapPositionMax.y + 1,
					ContentRect.yMax - viewContentSizeY, ContentRect.yMin - viewContentSizeY,
					worldRect.y
				),
				viewContentSizeX, viewContentSizeY
			), 1f, Color.white);


			// Mosue Logic




		}


		private void GUI_Key () {


		}


		#endregion




		#region --- API ---


		public void ClearMapPositions () => MapPositions.Clear();


		#endregion




		#region --- LGC ---




		#endregion




	}



	public class MapNavigatorPost : AssetPostprocessor {
		private static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			if (!EditorWindow.HasOpenInstances<MapNavigator>()) return;
			foreach (var path in importedAssets) {
				if (AssetDatabase.LoadAssetAtPath<MapObject>(path) != null) {
					var oldF = EditorWindow.focusedWindow;
					var navi = EditorWindow.GetWindow<MapNavigator>();
					if (oldF != null) {
						EditorWindow.FocusWindowIfItsOpen(oldF.GetType());
					}
					navi.ClearMapPositions();
					return;
				}
			}
		}
	}
}