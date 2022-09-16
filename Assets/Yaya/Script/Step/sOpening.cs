using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class sOpening : StepItem {



		// Const
		private const int DURATION = 180;
		private const int BLACK_DURATION = 120;
		private const int SKIP_DURATION = 12;

		// Api
		public int ViewX = 0;
		public int ViewYStart = 0;
		public int ViewYEnd = 0;
		public bool SpawnPlayerAtStart = false;
		public bool RemovePlayerAtStart = false;

		// Data
		private int SkipFrame = int.MaxValue;
		private int SkipY = 0;


		// MSG
		public override void OnStart (Game game) {
			base.OnStart(game);
			SetViewPosition(game, ViewX, ViewYStart);
			SkipFrame = int.MaxValue;
			// Draw Black Fade Out
			CellRenderer.Draw(
				Const.PIXEL,
				CellRenderer.CameraRect.Expand(Const.CELL_SIZE),
				new Color32(0, 0, 0, 255)
			);
			// Remove Player
			if (RemovePlayerAtStart && game.TryGetEntityInStage<ePlayer>(out var player)) {
				player.Active = false;
				player.InvokeWakeup();
			}
		}


		public override bool FrameUpdate (Game game) {
			int localFrame = LocalFrame;
			// Spawn Player
			if (localFrame == 1 && SpawnPlayerAtStart) {
				var player = (game as Yaya).SpawnPlayer(ViewX, ViewYEnd);
				if (player != null) {
					if (game.TryGetEntityNearby<eBed>(new(ViewX, ViewYEnd), out var bed)) {
						bed.Invoke(player);
					}
				}
			}
			if (localFrame < SkipFrame) {
				if (FrameInput.AnyKeyPressed) {
					SkipFrame = localFrame;
					SkipY = (int)Util.Remap(0, DURATION, ViewYStart, ViewYEnd, localFrame);
				}
				return Update_Opening(game, localFrame);
			} else {
				return Update_QuickSkip(game, localFrame);
			}
		}


		private bool Update_Opening (Game game, int localFrame) {
			// Black FadeIn
			if (localFrame <= BLACK_DURATION) {
				CellRenderer.SetLayer(YayaConst.SHADER_UI);
				CellRenderer.Draw(
					Const.PIXEL,
					CellRenderer.CameraRect.Expand(Const.CELL_SIZE),
					new Color32(0, 0, 0, (byte)Util.Remap(0, BLACK_DURATION, byte.MaxValue, byte.MinValue, localFrame))
				);
				CellRenderer.SetLayerToDefault();
			}
			if (localFrame < DURATION) {
				// Camera Down
				SetViewPosition(
					game,
					ViewX,
					(int)Util.Remap(0, DURATION, ViewYStart, ViewYEnd, localFrame)
				);
				return true;
			} else {
				// End
				return false;
			}
		}


		private bool Update_QuickSkip (Game game, int localFrame) {
			if (localFrame < SKIP_DURATION + SkipFrame) {
				SetViewPosition(
					game,
					ViewX,
					(int)Util.Remap(SkipFrame, SKIP_DURATION + SkipFrame, SkipY, ViewYEnd, localFrame)
				);
				return true;
			} else {
				// End
				return false;
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