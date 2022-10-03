using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class ePauseMenu : eYayaScreenUI {


		protected override void UpdateForUI () {

			var windowSize = new Vector2Int(500 * UNIT, 300 * UNIT);
			int charSize = 36 * UNIT;

			var cameraRect = CellRenderer.CameraRect;
			Width = windowSize.x;
			Height = windowSize.y;
			X = cameraRect.x + cameraRect.width / 2 - Width / 2;
			Y = cameraRect.y + cameraRect.height / 2 - Height / 2;

			// BG
			CellRenderer.Draw(Const.PIXEL, cameraRect, new Color32(0, 0, 0, 128)).Z = int.MinValue;
			CellRenderer.Draw(Const.PIXEL, Rect, new Color32(0, 0, 0, 255)).Z = int.MinValue + 1;

			// Menu




		}


	}
}