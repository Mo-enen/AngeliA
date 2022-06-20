using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class aOpening : Animation {



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


		// MSG
		public override void OnStart (int globalFrame) {
			base.OnStart(globalFrame);
			CellAnimation.SetViewPosition(VIEW_X, VIEW_Y_START);
			SkipFrame = int.MaxValue;
		}


		public override void FrameUpdate (Game game, int localFrame) {
			base.FrameUpdate(game, localFrame);
			if (localFrame < SkipFrame) {
				if (FrameInput.AnyKeyPressed) {
					SkipFrame = localFrame;
					SkipY = (int)Util.Remap(0, DURATION, VIEW_Y_START, VIEW_Y_END, localFrame);
				}
				Update_Opening(game, localFrame);
			} else {
				Update_QuickSkip(game, localFrame);
			}
		}


		private void Update_Opening (Game game, int localFrame) {
			// Black FadeIn
			if (localFrame < BLACK_DURATION) {
				CellRenderer.Draw(
					Const.PIXEL,
					CellRenderer.CameraRect.Expand(Const.CELL_SIZE),
					new Color32(0, 0, 0, (byte)Util.Remap(0, BLACK_DURATION, byte.MaxValue, byte.MinValue, localFrame))
				);
			}
			if (localFrame < DURATION) {
				// Camera Down
				CellAnimation.SetViewPosition(
					VIEW_X,
					(int)Util.Remap(0, DURATION, VIEW_Y_START, VIEW_Y_END, localFrame)
				);
			} else {
				// End
				SpawnPlayer(game);
				Stop();
			}
		}


		private void Update_QuickSkip (Game game, int localFrame) {
			if (localFrame < SKIP_DURATION + SkipFrame) {
				CellAnimation.SetViewPosition(
					VIEW_X,
					(int)Util.Remap(
						SkipFrame, SKIP_DURATION + SkipFrame, SkipY, VIEW_Y_END, localFrame
					)
				);
			} else {
				// End
				SpawnPlayer(game);
				Stop();
			}
		}


		private void SpawnPlayer (Game game) {

			var pos = new Vector2Int(VIEW_X, VIEW_Y_END);

			// Find Check Point



			// Spawn on Check Point



			// Find Best Bed
			eBed finalBed = null;
			int finalDistance = int.MaxValue;
			int count = game.StagedEntities.Length;
			for (int i = 0; i < count; i++) {
				var e = game.StagedEntities[i];
				if (e == null) break;
				if (e is not eBed bed) continue;
				if (finalBed == null) {
					finalBed = bed;
					finalDistance = Util.SqrtDistance(bed.Rect.position, pos);
				} else {
					int dis = Util.SqrtDistance(bed.Rect.position, pos);
					if (dis < finalDistance) {
						finalDistance = dis;
						finalBed = bed;
					}
				}
			}

			// Spawn on Bed
			if (finalBed != null) {
				var yaya = game.AddEntity(typeof(ePlayer).AngeHash(), finalBed.X, finalBed.Y) as ePlayer;
				finalBed.Invoke(yaya);
				return;
			}

			// Failback
			game.AddEntity(typeof(ePlayer).AngeHash(), pos.x, pos.y);
		}



	}
}