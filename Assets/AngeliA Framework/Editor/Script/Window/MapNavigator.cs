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
		private static GUIContent GlobalContent => _GlobalContent ??= EditorGUIUtility.IconContent("ToolHandleGlobal");
		private static GUIContent _GlobalContent = null;
		private static GUIContent LocalContent => _LocalContent ??= EditorGUIUtility.IconContent("d_ToolHandleLocal");
		private static GUIContent _LocalContent = null;
		private bool GlobalMode {
			get => _GlobalMode.Value || !EditorApplication.isPlaying;
			set => _GlobalMode.Value = value;
		}

		// Data
		private static MapNavigator Main = null;
		private readonly List<Vector2Int> MapPositions = new();
		private Vector2Int MapPositionMin = default;
		private Vector2Int MapPositionMax = default;
		private Rect ContentRect = default;
		private RectInt PrevCameraRect = default;

		// Saving
		private EditorSavingBool _GlobalMode = new("MapNavigator.GlobalMode", false);


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


		private void OnEnable () {
			Main = this;
			wantsMouseMove = false;
			wantsMouseEnterLeaveWindow = true;
		}


		private void OnGUI () {
			Main = this;
			GUI_Maps();
			GUI_View();
			GUI_Misc();
		}


		private void Update () {
			if (!EditorApplication.isPlaying || Main == null) return;
			if (CellRenderer.CameraRect.IsNotSame(Main.PrevCameraRect)) {
				Main.PrevCameraRect = CellRenderer.CameraRect;
				Main.Repaint();
			}
		}


		private void GUI_Maps () {
			// Get Maps
			if (MapPositions.Count == 0) {
				MapPositionMin = new Vector2Int(int.MaxValue, int.MaxValue);
				MapPositionMax = new Vector2Int(int.MinValue, int.MinValue);
				foreach (var file in Util.GetFilesIn(Util.CombinePaths(Application.dataPath, "Resources", "Map"), true, "*.asset")) {
					try {
						string name = Util.GetNameWithoutExtension(file.Name);
						var pos = MapObject.GetPositionFromName(name);
						if (pos.HasValue) {
							MapPositions.Add(pos.Value);
							MapPositionMin = Vector2Int.Min(MapPositionMin, pos.Value);
							MapPositionMax = Vector2Int.Max(MapPositionMax, pos.Value);
						}
					} catch (System.Exception ex) { Debug.LogException(ex); }
				}
			}

			Layout.Space(6);
			using (new GUILayout.HorizontalScope()) {
				Layout.Space(6);
				ContentRect = Layout.Rect(0, 0).Fit((float)(MapPositionMax.x - MapPositionMin.x + 1) / (MapPositionMax.y - MapPositionMin.y + 1));
				GUI.Box(ContentRect, GUIContent.none);
				if (GlobalMode) {
					// Global
					var filledColor = new Color32(64, 128, 96, 255);
					var unFilledColor = new Color32(96, 96, 96, 255);
					float dotSize = ContentRect.width / (MapPositionMax.x - MapPositionMin.x + 1);
					var filledRect = EditorApplication.isPlaying ? new RectInt(
						(Game.WorldSquad.Worlds[1, 1].FilledPosition - Vector2Int.one) * Const.WORLD_MAP_SIZE * Const.CELL_SIZE,
						Vector2Int.one * 3 * Const.WORLD_MAP_SIZE * Const.CELL_SIZE
					) : default;
					foreach (var pos in MapPositions) {
						bool filled = filledRect.width > 0 && filledRect.Contains(pos * Const.WORLD_MAP_SIZE * Const.CELL_SIZE);
						EditorGUI.DrawRect(new Rect(
							ContentRect.x + (pos.x - MapPositionMin.x) * dotSize,
							ContentRect.y + (MapPositionMax.y - (pos.y - MapPositionMin.y) + 1) * dotSize,
							dotSize, dotSize
						).Shrink(dotSize > 3 ? 1 : 0), filled ? filledColor : unFilledColor);
					}
				} else {
					// Local
					var zoneCenter = (Game.ViewRect.center / Const.CELL_SIZE).RoundToInt();
					var zoneUnitRect = new RectInt(
						zoneCenter.x - Const.WORLD_MAP_SIZE / 2,
						zoneCenter.y - Const.WORLD_MAP_SIZE / 2,
						Const.WORLD_MAP_SIZE,
						Const.WORLD_MAP_SIZE
					);
					var blockTint = new Color32(96, 96, 96, 255);
					var entityTint = new Color32(0, 196, 96, 255);

					var rect = new Rect(
						0, 0,
						ContentRect.width / zoneUnitRect.width,
						ContentRect.height / zoneUnitRect.height
					);

					// Blocks
					foreach (var (block, x, y, _) in Game.WorldSquad.ForAllBlocksInsideAllLayers(zoneUnitRect)) {
						rect.x = Util.Remap(
							zoneUnitRect.x, zoneUnitRect.xMax,
							ContentRect.xMin, ContentRect.xMax,
							x / Const.CELL_SIZE
						);
						rect.y = Util.Remap(
							zoneUnitRect.y, zoneUnitRect.yMax,
							ContentRect.yMax, ContentRect.yMin,
							y / Const.CELL_SIZE
						);
						EditorGUI.DrawRect(rect, Game.GetMinimapColor(block.TypeID, blockTint));
					}

					// Entities
					foreach (var (entity, globalX, globalY) in Game.WorldSquad.ForAllEntitiesInside(zoneUnitRect)) {
						rect.x = Util.Remap(
							zoneUnitRect.x, zoneUnitRect.xMax,
							ContentRect.xMin, ContentRect.xMax,
							(float)globalX / Const.CELL_SIZE
						);
						rect.y = Util.Remap(
							zoneUnitRect.y, zoneUnitRect.yMax,
							ContentRect.yMax, ContentRect.yMin,
							(float)globalY / Const.CELL_SIZE
						);
						EditorGUI.DrawRect(rect, Game.GetMinimapColor(entity.TypeID, entityTint));
					}

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
			if (GlobalMode) {
				// Global Mode
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
			} else {
				// Local Mode
				float width = ContentRect.width * CellRenderer.CameraRect.width / Const.WORLD_MAP_SIZE / Const.CELL_SIZE;
				float height = ContentRect.height * CellRenderer.CameraRect.height / Const.WORLD_MAP_SIZE / Const.CELL_SIZE;
				Layout.FrameGUI(new(
					ContentRect.x + ContentRect.width / 2f - width / 2f,
					ContentRect.y + ContentRect.height / 2f - height / 2f,
					width, height
				), 1f, Color.white);
			}

			// Mosue Logic
			if (GlobalMode) {
				if (
					(Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown) &&
					(Event.current.button == 0 || Event.current.button == 1)
				) {
					var viewSize = Game.ViewRect.size;
					var mousePos = Event.current.mousePosition;
					var mousePos01 = new Vector2(
						Mathf.InverseLerp(ContentRect.xMin, ContentRect.xMax, mousePos.x).Clamp01(),
						Mathf.InverseLerp(ContentRect.yMax, ContentRect.yMin, mousePos.y).Clamp01()
					);
					// Global Mode
					int viewX = (int)(Mathf.LerpUnclamped(
						MapPositionMin.x * Const.WORLD_MAP_SIZE * Const.CELL_SIZE,
						(MapPositionMax.x + 1) * Const.WORLD_MAP_SIZE * Const.CELL_SIZE,
						mousePos01.x
					) - viewSize.x / 2f);
					int viewY = (int)(Mathf.LerpUnclamped(
						MapPositionMin.y * Const.WORLD_MAP_SIZE * Const.CELL_SIZE,
						(MapPositionMax.y + 1) * Const.WORLD_MAP_SIZE * Const.CELL_SIZE,
						mousePos01.y
					) - viewSize.y / 2f);
					Game.SetViewPositionDely(viewX, viewY, 300);
				}
			} else {
				// Local
				bool perform = false;
				if (Event.current.button == 1) {
					if (ContentRect.Contains(Event.current.mousePosition)) {
						perform = GUI.RepeatButton(ContentRect, GUIContent.none, GUIStyle.none);
						Handles.BeginGUI();
						Handles.DrawLine(ContentRect.center, Event.current.mousePosition, 2f);
						Handles.EndGUI();
					}
				}

				if (Event.current.button == 0) {
					perform = Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown;
				}

				if (perform) {
					int viewX = Game.ViewRect.x;
					int viewY = Game.ViewRect.y;
					var viewSize = Game.ViewRect.size;

					Vector2 delta = default;
					float scale = 0f;
					if (Event.current.button == 1) {
						delta = Event.current.mousePosition - ContentRect.center;
						scale = 40;
					}
					if (Event.current.button == 0) {
						delta = Event.current.delta;
						scale = -600;
					}

					int centerPosX = viewX + viewSize.x / 2 + (delta.x * scale).RoundToInt();
					int centerPosY = viewY + viewSize.y / 2 + (delta.y * -scale).RoundToInt();

					viewX = centerPosX - viewSize.x / 2;
					viewY = centerPosY - viewSize.y / 2;
					Game.SetViewPositionDely(viewX, viewY, 300);
				}
			}

		}


		private void GUI_Misc () {
			if (EditorApplication.isPlaying && Event.current.type == EventType.MouseLeaveWindow) {
				EditorApplication.ExecuteMenuItem("Window/General/Game");
			}

			if (Event.current.type == EventType.MouseDown && Event.current.button == 2) {
				_GlobalMode.Value = !_GlobalMode.Value;
				Repaint();
			}

		}


		#endregion




		#region --- API ---


		public static void ClearMapPositions () {
			if (Main != null) {
				Main.MapPositions.Clear();
			}
		}


		#endregion




		#region --- LGC ---




		#endregion




	}



	public class MapNavigatorPost : AssetPostprocessor {
		private static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			foreach (var path in importedAssets) {
				if (AssetDatabase.LoadAssetAtPath<MapObject>(path) != null) {
					MapNavigator.ClearMapPositions();
					return;
				}
			}
		}
	}
}