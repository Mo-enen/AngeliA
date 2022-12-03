using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class tFadeOut : TaskItem {


		// Data
		private const int FADE_OUT = 60;
		public static readonly int TYPE_ID = typeof(tFadeOut).AngeHash();


		// MSG
		public override TaskResult FrameUpdate () {
			int localFrame = LocalFrame;
			if (localFrame < FADE_OUT) {
				if (localFrame == 0) {
					ScreenEffect.SetEffectEnable(RetroDarkenEffect.TYPE_ID, true);
				}
				RetroDarkenEffect.SetAmount(Util.Remap(
					0f, FADE_OUT, 0f, 1f,
					localFrame
				));
				return TaskResult.Continue;
			} else {
				//ScreenEffect.SetEffectEnable(DarkenEffect.TYPE_ID, false);
				return TaskResult.End;
			}
		}


	}
}
