using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework.Editor {
	public class eMapEditor : Entity {




		#region --- VAR ---


		// Const
		private static readonly int LINE_V_CODE = "LineV_4".ACode();
		private static readonly int LINE_H_CODE = "LineH_4".ACode();
		private const int RIGHT_DRAG_TOLERANT = 24;

		// Api
		public override bool Despawnable => false;

		// Short
		private static Game Game => _Game != null ? _Game : (_Game = Object.FindObjectOfType<Game>());
		private static Game _Game = null;

		// Data
		private MapPalette.Unit SelectingUnit = null;
		private Vector2Int? MouseLeftDownUnitPos = null;
		private Vector2Int? MouseRightDownPos = null;
		private RectInt? MosueDragUnitRect = null;
		private Vector2Int? ViewPivotPosition = null;


		#endregion




		#region --- MSG ---


		public override void FrameUpdate (int frame) {
			if (MapEditorWindow.Main == null) return;
			SelectingUnit = MapEditorWindow.Main.GetSelection();
			Update_Workflow();
			Update_MouseLeft();
			Update_MouseRight();
			Update_MouseMid();
			Update_Gizmos(frame);
		}


		private void Update_Workflow () {
			bool pressingSS =
				(FrameInput.KeyDown(GameKey.Select) && FrameInput.KeyPressing(GameKey.Start)) ||
				(FrameInput.KeyDown(GameKey.Start) && FrameInput.KeyPressing(GameKey.Select));
			if (Game.DebugMode) {
				// Editing 
				if (pressingSS) {
					// Goto Play
					var player = new eDebugPlayer {
						X = MousePosition.x / Const.CELL_SIZE * Const.CELL_SIZE + Const.CELL_SIZE / 2,
						Y = MousePosition.y / Const.CELL_SIZE * Const.CELL_SIZE,
					};
					Game.AddEntity(player, EntityLayer.Character);
					SetDebugMode(false);
				}
			} else {
				// Playing
				if (pressingSS) {
					// Goto Edit
					SetDebugMode(true);
				}
			}
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
				if (SelectingUnit != null && MapEditorWindow.Main.Painting) {
					CellRenderer.Draw(
						SelectingUnit.Sprite.name.ACode(),
						cursorRect.Fit((int)SelectingUnit.Sprite.rect.width, (int)SelectingUnit.Sprite.rect.height)
					);
				}
			} else {
				// Draging Rect
				var rect = new RectInt(
					MosueDragUnitRect.Value.x * Const.CELL_SIZE,
					MosueDragUnitRect.Value.y * Const.CELL_SIZE,
					(MosueDragUnitRect.Value.width + 1) * Const.CELL_SIZE,
					(MosueDragUnitRect.Value.height + 1) * Const.CELL_SIZE
				);
				CellGUI.Draw_9Slice(
					rect,
					frame % 30 > 15 ? new Color32(255, 255, 255, 255) : new Color32(230, 230, 230, 255),
					NineSliceSprites.PIXEL_FRAME_3
				);
				CellGUI.Draw_9Slice(
					rect.Shrink(3),
					frame % 30 > 15 ? new Color32(0, 0, 0, 255) : new Color32(20, 20, 20, 255),
					NineSliceSprites.PIXEL_FRAME_3
				);
			}

		}


		private void Update_MouseLeft () {

			if (!Game.DebugMode) return;

			if (FrameInput.MouseLeft) {
				if (SelectingUnit != null && MapEditorWindow.Main.Painting) {
					var mouseUnitPos = new Vector2Int(
						(int)Mathf.Lerp(CameraRect.xMin, CameraRect.xMax, FrameInput.MousePosition01.x) / Const.CELL_SIZE,
						(int)Mathf.Lerp(CameraRect.yMin, CameraRect.yMax, FrameInput.MousePosition01.y) / Const.CELL_SIZE
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
				}
			} else if (MouseLeftDownUnitPos.HasValue) {
				// Up



				RegisterUndo();
				MosueDragUnitRect = null;
				MouseLeftDownUnitPos = null;
			}



		}


		private void Update_MouseRight () {
			if (!Game.DebugMode) return;
			if (FrameInput.MouseRight) {
				var mouseScreenPos = new Vector2Int(
					(int)(FrameInput.MousePosition01.x * Screen.width),
					(int)(FrameInput.MousePosition01.y * Screen.height)
				);
				if (!MouseRightDownPos.HasValue) {
					// Down
					MouseRightDownPos = mouseScreenPos;
				} else {
					// Pressing
					if (ViewPivotPosition.HasValue) {
						var size = Game.ViewRect.size;
						Game.ViewRect = new(
							(int)(ViewPivotPosition.Value.x + (MouseRightDownPos.Value.x - mouseScreenPos.x) * ((float)CameraRect.width / Screen.width)),
							(int)(ViewPivotPosition.Value.y + (MouseRightDownPos.Value.y - mouseScreenPos.y) * ((float)CameraRect.height / Screen.height)),
							size.x, size.y
						);
					} else {
						float dis = Vector2Int.Distance(MouseRightDownPos.Value, mouseScreenPos);
						if (dis > RIGHT_DRAG_TOLERANT) {
							ViewPivotPosition = Game.ViewRect.position;
							MouseRightDownPos = mouseScreenPos;
						}
					}
				}
			} else if (MouseRightDownPos.HasValue) {
				// Up
				if (!ViewPivotPosition.HasValue) {
					// Pick


				}
				ViewPivotPosition = null;
				MouseRightDownPos = null;
			}
		}


		private void Update_MouseMid () {
			if (!Game.DebugMode) return;
			float deltaY = Input.mouseScrollDelta.y;
			if (deltaY.NotAlmostZero()) {



			}
		}


		#endregion




		#region --- LGC ---


		private void SetDebugMode (bool on) {
			Game.DebugMode = on;
			Util.SetFieldValue(Game, "LoadedUnitRect", new RectInt());
			UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
		}


		private void RegisterUndo () {



		}


		#endregion




	}
}
