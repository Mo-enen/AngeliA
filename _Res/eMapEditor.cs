using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Moenen.Standard;


namespace AngeliaFramework.Editor {
	public class eMapEditor : Entity {




		#region --- SUB ---

		private enum Tool {
			Selection = 0,
			Paint = 1,
		}

		#endregion




		#region --- VAR ---


		// Const
		private static readonly int LINE_V_CODE = "LineV_4".ACode();
		private static readonly int LINE_H_CODE = "LineH_4".ACode();
		private const int DRAG_TOLERANT = 24;

		// Api
		public override bool Despawnable => false;
		public override EntityLayer Layer => EntityLayer.Debug;
		public static MapPalette.Unit SelectingUnit { get; set; } = null;
		public static Dictionary<int, Sprite> SpritePool { get; } = new();

		// Short
		private static Game Game => _Game != null ? _Game : (_Game = Object.FindObjectOfType<Game>());
		private static Game _Game = null;
		private static bool Painting => CurrentTool == Tool.Paint;
		private static Tool CurrentTool {
			get => (Tool)SelectingToolIndex.Value;
			set => SelectingToolIndex.Value = (int)value;
		}

		// Data
		private Vector2Int PrevMousePos = default;
		private Vector2Int? MouseLeftDownUnitPos = null;
		private Vector2Int? MouseRightDownPos = null;
		private RectInt? MosueDragUnitRect = null;
		private Vector2Int? ViewPivotPosition = null;
		private NineSliceSprites DraggingRectFrame = new(NineSliceSprites.PIXEL_FRAME_3);
		private bool FocusingGameView = false;
		private static bool RequireChangeDebugMode = false;
		private static readonly Dictionary<int, System.Type> EntityPool = new();

		// Saving
		private static readonly EditorSavingInt SelectingToolIndex = new("MapEditor.SelectingToolIndex", 0);


		#endregion




		#region --- MSG ---


		[RuntimeInitializeOnLoadMethod]
		private static void RuntimeInit () {
			WorldSquad.BeforeWorldShift += () => {
				if (Game.DebugMode) {
					AllWorldsToMap(false);
				}
			};
		}


		[InitializeOnLoadMethod]
		private static void EdittimeInit () {
			EditorApplication.playModeStateChanged += (mode) => {
				if (mode == PlayModeStateChange.ExitingPlayMode) {
					AllWorldsToMap(false);
				}
				if (mode == PlayModeStateChange.EnteredEditMode) {
					AssetDatabase.SaveAssets();
				}
			};
			ReloadSpritePool();
			MoenenTools.OnDoTheThing += () => RequireChangeDebugMode = true;
		}


		[MenuItem("Tools/Hotkeys/Select Tool _1")]
		private static void HotKey_SelectTool () {
			CurrentTool = Tool.Selection;
			Event.current?.Use();
		}


		[MenuItem("Tools/Hotkeys/Paint Tool _2")]
		private static void HotKey_PaintTool () {
			CurrentTool = Tool.Paint;
			Event.current?.Use();
		}


		public override void OnCreate (int frame) {
			RequireChangeDebugMode = false;
			base.OnCreate(frame);
		}


		public override void FrameUpdate (int frame) {
			if (SpritePool.Count == 0) {
				ReloadSpritePool();
			}
			FocusingGameView = EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.GetType().Name == "GameView";
			Update_Workflow();
			Update_MouseLeft();
			Update_MouseRight();
			Update_MouseMid();
			Update_Key();
			Update_Gizmos(frame);
			PrevMousePos = FrameInput.MouseScreenPosition;
		}


		private void Update_Workflow () {
			if (Game.DebugMode) {
				// Editing 
				if (RequireChangeDebugMode) {
					// Goto Play
					int x = MousePosition.x.Divide(Const.CELL_SIZE);
					int y = MousePosition.y.Divide(Const.CELL_SIZE);
					var player = new eDebugPlayer {
						X = x * Const.CELL_SIZE + Const.CELL_SIZE / 2,
						Y = y * Const.CELL_SIZE,
					};
					SetDebugMode(false);
					Game.AddEntity(player);
				}
			} else {
				// Playing
				if (RequireChangeDebugMode) {
					// Goto Edit
					SetDebugMode(true);
				}
			}
			RequireChangeDebugMode = false;
		}


		private void Update_Gizmos (int frame) {

			if (!Game.DebugMode) return;

			// Grid
			{
				int l = Mathf.FloorToInt(CameraRect.xMin / Const.CELL_SIZE) * Const.CELL_SIZE;
				int r = Mathf.CeilToInt(CameraRect.xMax / Const.CELL_SIZE) * Const.CELL_SIZE;
				int d = Mathf.FloorToInt(CameraRect.yMin / Const.CELL_SIZE) * Const.CELL_SIZE;
				int u = Mathf.CeilToInt(CameraRect.yMax / Const.CELL_SIZE) * Const.CELL_SIZE;
				const int THICKNESS = 6;
				var tint = new Color32(255, 255, 255, 12);
				for (int y = d; y <= u; y += Const.CELL_SIZE) {
					CellRenderer.Draw(LINE_V_CODE, new RectInt(
						CameraRect.x, y - THICKNESS, CameraRect.width, THICKNESS * 2
					), tint);
				}
				for (int x = l; x <= r; x += Const.CELL_SIZE) {
					CellRenderer.Draw(LINE_H_CODE, new RectInt(
						x - THICKNESS, CameraRect.y, THICKNESS * 2, CameraRect.height
					), tint);
				}
			}

			if (!MosueDragUnitRect.HasValue) {
				// Cursor
				int cursorX = Mathf.FloorToInt((float)MousePosition.x / Const.CELL_SIZE) * Const.CELL_SIZE;
				int cursorY = Mathf.FloorToInt((float)MousePosition.y / Const.CELL_SIZE) * Const.CELL_SIZE;

				var cursorRect = new RectInt(cursorX, cursorY, Const.CELL_SIZE, Const.CELL_SIZE);

				// Frame
				CellGUI.Draw_9Slice(cursorRect, Color.grey, NineSliceSprites.PIXEL_FRAME_3);
				CellGUI.Draw_9Slice(cursorRect.Shrink(3), Color.black, NineSliceSprites.PIXEL_FRAME_3);

				// Icon
				if (SelectingUnit != null && Painting) {
					int id = SelectingUnit.IsEntity ? SelectingUnit.TypeFullName.ACode() : SelectingUnit.BlockID;
					if (SpritePool.ContainsKey(id)) {
						var sp = SpritePool[id];
						if (SelectingUnit.IsEntity) {
							id = sp.name.ACode();
						}
						CellRenderer.Draw(id, cursorRect.Fit((int)sp.rect.width, (int)sp.rect.height));
					}
				}
			} else {
				// Draging Rect
				var rect = new RectInt(
					MosueDragUnitRect.Value.x * Const.CELL_SIZE,
					MosueDragUnitRect.Value.y * Const.CELL_SIZE,
					(MosueDragUnitRect.Value.width + 1) * Const.CELL_SIZE,
					(MosueDragUnitRect.Value.height + 1) * Const.CELL_SIZE
				);
				int cameraSacle = Mathf.Clamp(Mathf.Max(CameraRect.width, CameraRect.height) / 1000, 3, 64);
				DraggingRectFrame.border = new RectOffset(cameraSacle, cameraSacle, cameraSacle, cameraSacle);
				CellGUI.Draw_9Slice(
					rect,
					frame % 30 > 15 ? new Color32(255, 255, 255, 255) : new Color32(230, 230, 230, 255),
					DraggingRectFrame
				);
				CellGUI.Draw_9Slice(
					rect.Shrink(cameraSacle),
					frame % 30 > 15 ? new Color32(0, 0, 0, 255) : new Color32(20, 20, 20, 255),
					DraggingRectFrame
				);
			}

		}


		private void Update_MouseLeft () {

			if (!Game.DebugMode || !FocusingGameView) return;

			if (FrameInput.MouseLeft) {
				var mouseUnitPos = new Vector2Int(
					((int)Mathf.Lerp(CameraRect.xMin, CameraRect.xMax, FrameInput.MousePosition01.x)).Divide(Const.CELL_SIZE),
					((int)Mathf.Lerp(CameraRect.yMin, CameraRect.yMax, FrameInput.MousePosition01.y)).Divide(Const.CELL_SIZE)
				);
				if (!MouseLeftDownUnitPos.HasValue) {
					// Down
					MouseLeftDownUnitPos = mouseUnitPos;
					MosueDragUnitRect = new RectInt(mouseUnitPos.x, mouseUnitPos.y, 0, 0);
				} else {
					// Pressing
					var rect = MosueDragUnitRect.Value;
					rect.SetMinMax(
						Vector2Int.Min(mouseUnitPos, MouseLeftDownUnitPos.Value),
						Vector2Int.Max(mouseUnitPos, MouseLeftDownUnitPos.Value)
					);
					MosueDragUnitRect = rect;
				}
			} else if (MouseLeftDownUnitPos.HasValue) {
				// Up
				if (Painting) {
					if (SelectingUnit != null) {
						// Paint




					}
				} else {
					// Select



				}
				MosueDragUnitRect = null;
				MouseLeftDownUnitPos = null;
			}



		}


		private void Update_MouseRight () {
			if (!Game.DebugMode || !FocusingGameView) return;
			// Right Drag/Click
			if (FrameInput.MouseRight) {
				var mouseScreenPos = FrameInput.MouseScreenPosition;
				if (!MouseRightDownPos.HasValue) {
					// Down
					MouseRightDownPos = mouseScreenPos;
					Game.StopViewDely();
				} else {
					// Pressing
					if (ViewPivotPosition.HasValue) {
						Game.SetViewPositionDely(
							(int)(ViewPivotPosition.Value.x + (MouseRightDownPos.Value.x - mouseScreenPos.x) * ((float)CameraRect.width / Screen.width)),
							(int)(ViewPivotPosition.Value.y + (MouseRightDownPos.Value.y - mouseScreenPos.y) * ((float)CameraRect.height / Screen.height))
						);
					} else {
						float dis = Vector2Int.Distance(MouseRightDownPos.Value, mouseScreenPos);
						if (dis > DRAG_TOLERANT) {
							ViewPivotPosition = Game.ViewRect.position;
							MouseRightDownPos = mouseScreenPos;
						}
					}
				}
			} else if (MouseRightDownPos.HasValue) {
				// Up
				if (!ViewPivotPosition.HasValue) {
					// Pick
					Pick(
						(int)Util.RemapUnclamped(0, Screen.width, CameraRect.xMin, CameraRect.xMax, FrameInput.MouseScreenPosition.x),
						(int)Util.RemapUnclamped(0, Screen.height, CameraRect.yMin, CameraRect.yMax, FrameInput.MouseScreenPosition.y)
					);
				} else {
					// Moving
					if (FrameInput.MouseScreenPosition != PrevMousePos) {
						var delta = (PrevMousePos - FrameInput.MouseScreenPosition) * 7;
						Game.SetViewPositionDely(
							(int)(Game.ViewRect.x + delta.x * (float)CameraRect.width / Screen.width),
							(int)(Game.ViewRect.y + delta.y * (float)CameraRect.height / Screen.height),
							100
						);
					}
				}
				ViewPivotPosition = null;
				MouseRightDownPos = null;
			}

		}


		private void Update_MouseMid () {
			if (!Game.DebugMode || !FocusingGameView) return;

			// Zoom
			int deltaY = -Input.mouseScrollDelta.y.RoundToInt();
			if (
				deltaY != 0 &&
				!FrameInput.KeyPressing(GameKey.Down) &&
				!FrameInput.KeyPressing(GameKey.Up) &&
				!FrameInput.KeyPressing(GameKey.Left) &&
				!FrameInput.KeyPressing(GameKey.Right) &&
				!FrameInput.KeyPressing(GameKey.Start) &&
				!FrameInput.KeyPressing(GameKey.Select)
			) {
				Zoom(deltaY);
			}
		}


		private void Update_Key () {
			if (!Game.DebugMode || !FocusingGameView) return;
			bool control = Input.GetKey(KeyCode.LeftControl);
			// Shift
			if (!control) {
				var dir = Vector2Int.zero;
				int SPEED = Game.ViewRect.height / 40;
				if (FrameInput.KeyPressing(GameKey.Left)) dir.x = -SPEED;
				if (FrameInput.KeyPressing(GameKey.Right)) dir.x = SPEED;
				if (FrameInput.KeyPressing(GameKey.Down)) dir.y = -SPEED;
				if (FrameInput.KeyPressing(GameKey.Up)) dir.y = SPEED;
				if (dir != Vector2Int.zero) {
					var newPos = Game.ViewRect.position + dir;
					Game.SetViewPositionDely(newPos.x, newPos.y);
				}
			}
		}


		#endregion




		#region --- LGC ---


		private void SetDebugMode (bool on) {
			Game.DebugMode = on;
			Util.SetFieldValue(Game, "LoadedUnitRect", new RectInt());
			UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
		}


		private void Zoom (int delta) {
			int width = Game.ViewRect.width;
			int height = Game.ViewRect.height;

			var pivot = new Vector2Int(
				(int)Mathf.Lerp(CameraRect.x, CameraRect.xMax, FrameInput.MousePosition01.x),
				(int)Mathf.Lerp(CameraRect.y, CameraRect.yMax, FrameInput.MousePosition01.y)
			);
			var pivotOffset = pivot - Game.ViewRect.position;

			int scale = Const.CELL_SIZE * width / 15 / Const.CELL_SIZE;
			int newHeight = height + delta * scale;
			int newWidth = newHeight * width / height;

			newWidth = Mathf.Clamp(newWidth, Const.MIN_VIEW_WIDTH, Const.MAX_VIEW_WIDTH);
			newHeight = Mathf.Clamp(newHeight, Const.MIN_VIEW_HEIGHT, Const.MAX_VIEW_HEIGHT);

			pivotOffset.x = pivotOffset.x * newWidth / width;
			pivotOffset.y = pivotOffset.y * newHeight / height;

			Game.SetViewPositionDely(pivot.x - pivotOffset.x, pivot.y - pivotOffset.y, 400);
			Game.SetViewSizeDely(newWidth, newHeight, 400);
		}


		// Edit
		private void SetBlock (RectInt unitRect, WorldData.Block block) {





		}


		private static void AllWorldsToMap (bool undo) {
			WorldToMap(0, 0, undo);
			WorldToMap(1, 0, undo);
			WorldToMap(2, 0, undo);
			WorldToMap(0, 1, undo);
			WorldToMap(1, 1, undo);
			WorldToMap(2, 1, undo);
			WorldToMap(0, 2, undo);
			WorldToMap(1, 2, undo);
			WorldToMap(2, 2, undo);
		}


		private static void WorldToMap (int i, int j, bool undo) {
			try {
				var world = Game.WorldSquad.Worlds[i, j];
				var map = Resources.Load<MapObject>($"Map/{world.FilledPosition.x}_{world.FilledPosition.y}"); ;
				if (map != null) {
					if (undo) {
						Undo.RegisterCompleteObjectUndo(map, $"[{Game.GlobalFrame}]World to Map");
					}
					world.EditorOnly_SaveToDisk(map);
					EditorUtility.SetDirty(map);
				}
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}


		// Pick
		private void Pick (int globalX, int globalY) {
			SelectingUnit = null;
			if (Game.WorldSquad.GetEntityAt(globalX, globalY, out var entity)) {
				SelectingUnit = new() {
					TypeFullName = GetEntityTypeWithID(entity.TypeID).FullName,
				};
			} else if (Game.WorldSquad.GetBlockAt(globalX, globalY, out var block, out var layer)) {
				SelectingUnit = new() {
					BlockID = block.TypeID,
					BlockLayer = layer,
					IsTrigger = block.IsTrigger,
					Tag = block.Tag,
					TypeFullName = "",
				};
			}
			MapPaletteWindow.RequireClearSelection();
		}


		// Asset
		private static void ReloadSpritePool () {
			SpritePool.Clear();
			// Sheets
			foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(SpriteSheet)}")) {
				var sheet = AssetDatabase.LoadAssetAtPath<SpriteSheet>(AssetDatabase.GUIDToAssetPath(guid));
				if (sheet == null || sheet.Texture == null) continue;
				foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(sheet.Texture))) {
					if (obj is Sprite sp) {
						SpritePool.TryAdd(sp.name.ACode(), sp);
					}
				}
			}
			// Entity
			foreach (var type in typeof(Entity).GetAllChildClass()) {
				if (System.Activator.CreateInstance(type) is not Entity e) continue;
				if (SpritePool.ContainsKey(e.Thumbnail)) {
					SpritePool.TryAdd(type.ACode(), SpritePool[e.Thumbnail]);
				}
			}
		}


		private static System.Type GetEntityTypeWithID (int id) {
			if (EntityPool.Count == 0) {
				foreach (var type in typeof(Entity).GetAllChildClass()) {
					EntityPool.TryAdd(type.ACode(), type);
				}
			}
			return EntityPool.ContainsKey(id) ? EntityPool[id] : null;
		}


		#endregion




	}
}
