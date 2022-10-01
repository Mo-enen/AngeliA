using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class sFadeOut : StepItem {


		// Data
		private const int FADE_OUT = 32;


		// MSG
		public override bool FrameUpdate () {
			int localFrame = LocalFrame;
			CellRenderer.SetLayer(YayaConst.SHADER_UI);
			CellRenderer.Draw(
				Const.PIXEL,
				CellRenderer.CameraRect.Expand(Const.CELL_SIZE),
				new Color32(0, 0, 0, (byte)Util.Remap(0f, FADE_OUT, byte.MinValue, byte.MaxValue, localFrame))
			);
			CellRenderer.SetLayerToDefault();
			return localFrame < FADE_OUT;
		}


	}
}
