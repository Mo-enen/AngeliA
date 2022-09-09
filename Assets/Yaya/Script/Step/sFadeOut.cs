using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class sFadeOut : StepItem {


		// Data
		private const int FADE_OUT = 32;


		// MSG
		public override StepResult FrameUpdate (Game game) {
			int localFrame = LocalFrame;
            AngeliaFramework.Renderer.SetLayer(YayaConst.SHADER_UI);
            AngeliaFramework.Renderer.Draw(
                Const.PIXEL,
                AngeliaFramework.Renderer.CameraRect.Expand(Const.CELL_SIZE),
				new Color32(0, 0, 0, (byte)Util.Remap(0, FADE_OUT, byte.MinValue, byte.MaxValue, localFrame))
			);
            AngeliaFramework.Renderer.SetLayerToDefault();
			return localFrame < FADE_OUT ? StepResult.Continue : StepResult.Over;
		}


	}
}
