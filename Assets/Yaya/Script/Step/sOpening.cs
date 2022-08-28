using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class sOpening : Step {



		// Const
		private const int VIEW_X = 10 * Const.CELL_SIZE;
		private const int VIEW_Y_START = 19 * Const.CELL_SIZE;
		private const int VIEW_Y_END = 8 * Const.CELL_SIZE;
		private const int DURATION = 180;
		private const int BLACK_DURATION = 120;
		private const int SKIP_DURATION = 12;

		// Data
		private int SkipFrame = int.MaxValue;
		private int SkipY = 0;
		private YayaGame Yaya = null;


		// MSG
		public override void OnStart (Game game) {
			base.OnStart(game);
			Yaya = game as YayaGame;
			SetViewPosition(game, VIEW_X, VIEW_Y_START);
			SkipFrame = int.MaxValue;
			// Remove Player
			if (game.TryGetEntityInStage<ePlayer>(out var player)) {
				player.Active = false;
				player.Wakeup();
			}
			// Draw Black Fade Out
			CellRenderer.Draw(
				Const.PIXEL,
				CellRenderer.CameraRect.Expand(Const.CELL_SIZE),
				new Color32(0, 0, 0, 255)
			);
		}


		public override StepResult FrameUpdate (Game game) {
			int localFrame = LocalFrame;
			if (localFrame < SkipFrame) {
				if (FrameInput.AnyKeyPressed) {
					SkipFrame = localFrame;
					SkipY = (int)Util.Remap(0, DURATION, VIEW_Y_START, VIEW_Y_END, localFrame);
				}
				return Update_Opening(game, localFrame);
			} else {
				return Update_QuickSkip(game, localFrame);
			}
		}


		private StepResult Update_Opening (Game game, int localFrame) {
			// Black FadeIn
			if (localFrame <= BLACK_DURATION) {
				CellRenderer.Draw(
					Const.PIXEL,
					CellRenderer.CameraRect.Expand(Const.CELL_SIZE),
					new Color32(0, 0, 0, (byte)Util.Remap(0, BLACK_DURATION, byte.MaxValue, byte.MinValue, localFrame))
				);
			}
			if (localFrame < DURATION) {
				// Camera Down
				SetViewPosition(
					game,
					VIEW_X,
					(int)Util.Remap(0, DURATION, VIEW_Y_START, VIEW_Y_END, localFrame)
				);
				return StepResult.Continue;
			} else {
				// End
				Yaya.SpawnPlayer(VIEW_X, VIEW_Y_END, true);
				return StepResult.Over;
			}
		}


		private StepResult Update_QuickSkip (Game game, int localFrame) {
			if (localFrame < SKIP_DURATION + SkipFrame) {
				SetViewPosition(
					game,
					VIEW_X,
					(int)Util.Remap(SkipFrame, SKIP_DURATION + SkipFrame, SkipY, VIEW_Y_END, localFrame)
				);
				return StepResult.Continue;
			} else {
				// End
				Yaya.SpawnPlayer(VIEW_X, VIEW_Y_END, true);
				return StepResult.Over;
			}
		}


		// LGC
		private void SetViewPosition (Game game, int x, int y) => game.SetViewPositionDely(
			x - game.ViewRect.width / 2,
			y - game.ViewRect.height / 2,
			1000,
			Const.VIEW_PRIORITY_ANIMATION
		);


	}
}