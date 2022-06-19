using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class aReopening : aOpening {



		// Data
		private const int FADE_OUT = 32;


		// MSG
		public override void OnStart (int globalFrame) { }


		public override void FrameUpdate (Game game, int localFrame) {
			if (localFrame < FADE_OUT) {
				// Fade Out
				CellRenderer.Draw(
					Const.PIXEL,
					CellRenderer.CameraRect.Expand(Const.CELL_SIZE),
					new Color32(0, 0, 0, (byte)Util.Remap(0, FADE_OUT, byte.MinValue, byte.MaxValue, localFrame))
				);
			} else {
				if (localFrame == FADE_OUT) {
					// First Frame of Fade In 
					if (game.TryGetEntityInStage<ePlayer>(out var player)) {
						player.Active = false;
					}
					base.OnStart(Game.GlobalFrame);
				}
				base.FrameUpdate(game, localFrame - FADE_OUT);
			}
		}



	}
}
