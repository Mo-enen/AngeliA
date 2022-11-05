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
			byte t = (byte)Util.Remap(0f, FADE_OUT, byte.MaxValue, byte.MinValue, localFrame);
			CellRenderer.SetLayer(YayaConst.SHADER_MULT);
			CellRenderer.Draw(
				Const.PIXEL,
				CellRenderer.CameraRect.Expand(Const.CEL),
				new Color32(t, t, t, 255)
			).Z = int.MaxValue;
			CellRenderer.SetLayerToDefault();
			return localFrame < FADE_OUT;
		}


	}
}
