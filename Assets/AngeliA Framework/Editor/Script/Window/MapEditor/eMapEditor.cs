using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework.Entities;
using AngeliaFramework.Rendering;
using Moenen.Standard;


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
		private static readonly int PIXEL_CODE = "Pixel".ACode();
		private static readonly int LINE_V_CODE = "LineV_4".ACode();
		private static readonly int LINE_H_CODE = "LineH_4".ACode();

		// Api
		public override bool Despawnable => false;

		// Data
		private MapPalette.Unit SelectingBlock = null;


		#endregion




		#region --- MSG ---


		public override void FrameUpdate (int frame) {
			if (MapEditor.Main == null) return;
			SelectingBlock = MapEditor.Main.GetSelection();
			Update_Grid();
			Update_Cursor();
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
			if (SelectingBlock != null && MapEditor.Main.Painting) {
				CellRenderer.Draw(
					SelectingBlock.Sprite.name.ACode(),
					cursorRect.Fit((int)SelectingBlock.Sprite.rect.width, (int)SelectingBlock.Sprite.rect.height)
				);
			}

		}


		#endregion




		#region --- LGC ---




		#endregion




	}
}
