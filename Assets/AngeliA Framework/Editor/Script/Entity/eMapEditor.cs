using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework.Editor {
	public class eMapEditor : Entity {




		#region --- VAR ---


		// Const
		private static readonly AlignmentInt PixelFrame = new(
			"Pixel".ACode(),
			"Pixel".ACode(),
			"Pixel".ACode(),
			"Pixel".ACode(),
			0,
			"Pixel".ACode(),
			"Pixel".ACode(),
			"Pixel".ACode(),
			"Pixel".ACode()
		);
		//private static readonly int PIXEL_CODE = "Pixel".ACode();
		private static readonly int LINE_V_CODE = "LineV_4".ACode();
		private static readonly int LINE_H_CODE = "LineH_4".ACode();

		// Api
		public override bool Despawnable => false;

		// Data
		private MapPalette.Unit SelectingBlock = null;
		private Vector2Int? ResetingPlayerPosition = null;


		#endregion




		#region --- MSG ---


		public override void FrameUpdate (int frame) {
			if (MapEditorWindow.Main == null) return;
			SelectingBlock = MapEditorWindow.Main.GetSelection();
			Update_Workflow();
			if (MapEditorWindow.Main.Game.DebugMode) {
				Update_Grid();
				Update_Cursor();
			}
		}


		private void Update_Workflow () {
			bool pressingSS =
				(FrameInput.KeyDown(GameKey.Select) && FrameInput.KeyPressing(GameKey.Start)) ||
				(FrameInput.KeyDown(GameKey.Start) && FrameInput.KeyPressing(GameKey.Select));
			if (MapEditorWindow.Main.Game.DebugMode) {
				// Editing 
				if (pressingSS) {
					ResetingPlayerPosition = MousePosition;
					SetDebugMode(false);
				}
			} else {
				// Playing
				if (pressingSS) {
					SetDebugMode(true);
				}
				if (ResetingPlayerPosition.HasValue) {
					var game = MapEditorWindow.Main.Game;
					var player = game.FindEntityOfType<ePlayer>(EntityLayer.Character);
					if (player != null) {
						player.X = ResetingPlayerPosition.Value.x;
						player.Y = ResetingPlayerPosition.Value.y;
						ResetingPlayerPosition = null;
					}
				}
			}
		}


		private void Update_Grid () {
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


		private void Update_Cursor () {

			int cursorX = Mathf.FloorToInt((float)MousePosition.x / Const.CELL_SIZE) * Const.CELL_SIZE;
			int cursorY = Mathf.FloorToInt((float)MousePosition.y / Const.CELL_SIZE) * Const.CELL_SIZE;
			const int THICKNESS = 3;
			var cursorRect = new RectInt(cursorX, cursorY, Const.CELL_SIZE, Const.CELL_SIZE);

			// Frame
			CellGUI.Draw_9Slice(
				cursorRect,
				new RectOffset(THICKNESS, THICKNESS, THICKNESS, THICKNESS),
				Color.grey, PixelFrame
			);
			CellGUI.Draw_9Slice(
				cursorRect.Shrink(THICKNESS),
				new RectOffset(THICKNESS, THICKNESS, THICKNESS, THICKNESS),
				Color.black, PixelFrame
			);

			// Icon
			if (SelectingBlock != null && MapEditorWindow.Main.Painting) {
				CellRenderer.Draw(
					SelectingBlock.Sprite.name.ACode(),
					cursorRect.Fit((int)SelectingBlock.Sprite.rect.width, (int)SelectingBlock.Sprite.rect.height)
				);
			}

		}


		#endregion




		#region --- LGC ---


		private void SetDebugMode (bool on) {
			MapEditorWindow.Main.Game.DebugMode = on;
			if (!on) {
				MapEditorWindow.Main.Game.AddEntity(new eDebugPlayer(), EntityLayer.Character);
			}
#if UNITY_EDITOR
			UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
#endif
		}


		#endregion




	}
}
