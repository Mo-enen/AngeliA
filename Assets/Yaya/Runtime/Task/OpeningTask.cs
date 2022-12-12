using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class OpeningTask : TaskItem {



		// Const
		private const int DURATION = 180;
		private const int BLACK_DURATION = 120;
		private const int SKIP_DURATION = 12;
		public static readonly int TYPE_ID = typeof(OpeningTask).AngeHash();

		// Api
		public int ViewX = 0;
		public int ViewYStart = 0;
		public int ViewYEnd = 0;

		// Data
		private int SkipFrame = int.MaxValue;
		private int SkipY = 0;


		// MSG
		public override void OnStart () {
			base.OnStart();
			var game = Game.Current;
			SetViewPosition(game, ViewX, ViewYStart);
			game.SetViewZImmediately(0);
			SkipFrame = int.MaxValue;
			// Draw Black Fade Out
			CellRenderer.Draw(
				Const.PIXEL,
				CellRenderer.CameraRect.Expand(Const.CEL),
				new Color32(0, 0, 0, 255)
			).Z = int.MaxValue;
			// Remove Player
			var player = ePlayer.Current;
			if (player != null) {
				player.Active = false;
				player.SetCharacterState(CharacterState.GamePlay);
			}
		}


		public override TaskResult FrameUpdate () {
			int localFrame = LocalFrame;
			var game = Game.Current;
			if (localFrame == 0) {
				ScreenEffect.SetEffectEnable(RetroDarkenEffect.TYPE_ID, true);
			}
			// Spawn Player
			if (localFrame == 2) {
				var player = ePlayer.TrySpawnPlayer(ViewX, ViewYEnd);
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


		private TaskResult Update_Opening (Game game, int localFrame) {
			// Black FadeIn
			if (localFrame <= BLACK_DURATION) {
				RetroDarkenEffect.SetAmount(Util.Remap(
					0f, BLACK_DURATION,
					1f, 0f,
					localFrame
				));
			}
			if (localFrame < DURATION) {
				// Camera Down
				SetViewPosition(
					game,
					ViewX,
					(int)Util.Remap(0f, DURATION, ViewYStart, ViewYEnd, localFrame)
				);
				return TaskResult.Continue;
			} else {
				// End
				ScreenEffect.SetEffectEnable(RetroDarkenEffect.TYPE_ID, false);
				return TaskResult.End;
			}
		}


		private TaskResult Update_QuickSkip (Game game, int localFrame) {
			ScreenEffect.SetEffectEnable(RetroDarkenEffect.TYPE_ID, false);
			if (localFrame < SKIP_DURATION + SkipFrame) {
				SetViewPosition(
					game,
					ViewX,
					(int)Util.Remap((float)SkipFrame, SKIP_DURATION + SkipFrame, SkipY, ViewYEnd, localFrame)
				);
				return TaskResult.Continue;
			} else {
				// End
				return TaskResult.End;
			}
		}


		// LGC
		private void SetViewPosition (Game game, int x, int y) => game.SetViewPositionDelay(
			x - game.ViewRect.width / 2,
			y - game.ViewRect.height / 2,
			1000,
			YayaConst.VIEW_PRIORITY_CUTSCENE
		);


	}
}