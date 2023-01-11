using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class OpeningTask : TaskItem {



		// Const
		public static readonly int TYPE_ID = typeof(OpeningTask).AngeHash();
		private const int DURATION = 180;
		private const int BLACK_DURATION = 120;
		private const int SKIP_DURATION = 12;
		private const int DOLLY_HEIGHT = Const.CEL * 11;
		
		// Api
		public int TargetViewX { get; set; } = 0;
		public int TargetViewY { get; set; } = 0;
		public int TargetViewZ { get; set; } = 0;

		// Data
		private int SkipFrame = int.MaxValue;
		private int SkipY = 0;
		private bool GotoBed = true;


		// MSG
		public override void OnStart () {
			base.OnStart();
			GotoBed = true;
			// Set Pos to Check Point
			if (eCheckPoint.SavedPosition.HasValue) {
				var pos = eCheckPoint.SavedPosition.Value;
				TargetViewX = pos.x;
				TargetViewY = pos.y;
				TargetViewZ = pos.z;
				GotoBed = false;
			}
			SetViewPosition(TargetViewX, TargetViewY + DOLLY_HEIGHT);
			Game.Current.SetViewZ(TargetViewZ);
			SkipFrame = int.MaxValue;
			// Remove Player
			var player = ePlayer.Current;
			if (player != null) {
				player.Active = false;
				player.SetCharacterState(CharacterState.GamePlay);
			}
		}


		public override TaskResult FrameUpdate () {
			int localFrame = LocalFrame;
			if (localFrame == 0) {
				ScreenEffect.SetEffectEnable(RetroDarkenEffect.TYPE_ID, true);
			}
			// Spawn Player
			if (localFrame == 2) {
				if (GotoBed) {
					ePlayer.TrySpawnPlayerToBed(TargetViewX, TargetViewY);
				} else {
					ePlayer.TrySpawnPlayer(TargetViewX, TargetViewY);
				}
			}
			if (localFrame < SkipFrame) {
				// Slow 
				if (FrameInput.AnyKeyPressed) {
					SkipFrame = localFrame;
					SkipY = (int)Util.Remap(0f, DURATION, TargetViewY + DOLLY_HEIGHT, TargetViewY, localFrame);
				}
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
						TargetViewX,
						(int)Util.Remap(0f, DURATION, TargetViewY + DOLLY_HEIGHT, TargetViewY, localFrame)
					);
					return TaskResult.Continue;
				} else {
					// End
					ScreenEffect.SetEffectEnable(RetroDarkenEffect.TYPE_ID, false);
					return TaskResult.End;
				}
			} else {
				// Quick
				ScreenEffect.SetEffectEnable(RetroDarkenEffect.TYPE_ID, false);
				if (localFrame < SKIP_DURATION + SkipFrame) {
					SetViewPosition(
						TargetViewX,
						(int)Util.Remap((float)SkipFrame, SKIP_DURATION + SkipFrame, SkipY, TargetViewY, localFrame)
					);
					return TaskResult.Continue;
				} else {
					// End
					return TaskResult.End;
				}
			}
		}


		// LGC
		private void SetViewPosition (int x, int y) => Game.Current.SetViewPositionDelay(
			x - Game.Current.ViewRect.width / 2,
			y - Game.Current.ViewRect.height / 2,
			1000,
			YayaConst.VIEW_PRIORITY_SYSTEM
		);


	}
}