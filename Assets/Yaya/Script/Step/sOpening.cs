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
		public override void OnStart () {
			base.OnStart();
			var game = Game.Current;
			SetViewPosition(game, ViewX, ViewYStart);
			game.SetViewZ(0);
			SkipFrame = int.MaxValue;
			// Draw Black Fade Out
			CellRenderer.Draw(
				Const.PIXEL,
				CellRenderer.CameraRect.Expand(Const.CEL),
				new Color32(0, 0, 0, 255)
			);
			// Remove Player
			if (RemovePlayerAtStart && game.TryGetEntityInStage<ePlayer>(out var player)) {
				player.Active = false;
				player.SetCharacterState(CharacterState.GamePlay);
			}
		}


		public override bool FrameUpdate () {
			int localFrame = LocalFrame;
			var game = Game.Current;
			// Spawn Player
			if (localFrame == 2 && SpawnPlayerAtStart) {
				var player = (game as Yaya).SpawnPlayer(ViewX, ViewYEnd);
				if (player != null) {
					if (game.TryGetEntityNearby<eBed>(new(ViewX, ViewYEnd), out var bed)) {
						bed.Invoke(player);
					}
					player.SleepAmount = 1000;
				}
			}
			if (localFrame < SkipFrame) {
				if (FrameInput.AnyKeyPressed) {
					SkipFrame = localFrame;
					SkipY = (int)Util.Remap(0f, DURATION, ViewYStart, ViewYEnd, localFrame);
				}
				return Update_Opening(game, localFrame);
			} else {
				return Update_QuickSkip(game, localFrame);
			}
		}


		private bool Update_Opening (Game game, int localFrame) {
			// Black FadeIn
			if (localFrame <= BLACK_DURATION) {
				byte t = (byte)Util.Remap(0f, BLACK_DURATION, byte.MinValue, byte.MaxValue, localFrame);
				CellRenderer.SetLayer(YayaConst.SHADER_MULT);
				CellRenderer.Draw(
					Const.PIXEL,
					CellRenderer.CameraRect.Expand(Const.CEL),
					new Color32(t, t, t, 255)
				);
				CellRenderer.SetLayerToDefault();
			}
			if (localFrame < DURATION) {
				// Camera Down
				SetViewPosition(
					game,
					ViewX,
					(int)Util.Remap(0f, DURATION, ViewYStart, ViewYEnd, localFrame)
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
					(int)Util.Remap((float)SkipFrame, SKIP_DURATION + SkipFrame, SkipY, ViewYEnd, localFrame)
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
			YayaConst.VIEW_PRIORITY_CUTSCENE
		);


	}
}