using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class sFadeOut : Step {


		// Data
		private const int FADE_OUT = 32;


		// MSG
		public override StepResult FrameUpdate (Game game) {
			int localFrame = LocalFrame;
			var cell = CellRenderer.Draw(
				Const.PIXEL,
				CellRenderer.CameraRect.Expand(Const.CELL_SIZE),
				new Color32(0, 0, 0, (byte)Util.Remap(0, FADE_OUT, byte.MinValue, byte.MaxValue, localFrame))
			);
			
			return localFrame < FADE_OUT ? StepResult.Continue : StepResult.Over;
		}


	}
}
