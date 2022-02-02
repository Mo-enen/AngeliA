using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework.Rendering;


namespace AngeliaFramework.Entities.Editor {
	public class eDebugChar : Entity {

		public int Width = Const.CELL_SIZE * 16;
		public int Height = Const.CELL_SIZE * 4;
		public int CharSize = 256;
		public int CharSpace = 8;
		public int LineSpace = 8;
		public Color32 BGColor = new(64, 64, 64, 255);
		public Color32 AreaColor = new(255, 255, 255, 64);
		public Color32 Color = new(255, 255, 255, 255);
		public string Content = "±¿±¿Ñ¾ÄÏ¹Ï¹ÏYaYaGuaGua";


		public override void FrameUpdate (int frame) {
			CellRenderer.Draw(
				"Pixel".ACode(),
				X, Y, 0, 0,
				0, Width, Height, BGColor
			);
			CellGUI.DrawLabel(
				Content,
				new RectInt(X, Y, Width, Height),
				Color, CharSize, CharSpace, LineSpace, true
			);
		}


	}


}
