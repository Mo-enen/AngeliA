using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


namespace Yaya {
	public class OpeningTask : TaskItem {



		// Const
		public static readonly int TYPE_ID = typeof(OpeningTask).AngeHash();
		private const int FADE_OUT_DURATION = 60;
		private const int DURATION = 180;
		private const int BLACK_DURATION = 120;
		private const int SKIP_DURATION = 12;
		private const int DOLLY_HEIGHT = Const.CEL * 10;

		// Api
		public int TargetViewX { get; set; } = 0;
		public int TargetViewY { get; set; } = 0;
		public int TargetViewZ { get; set; } = 0;
		public bool FadeOut { get; set; } = true;
		public bool GotoBed { get; set; } = true;

		// Data
		private int SkipFrame = int.MaxValue;
		private int SkipY = 0;
		private int PlayerSpawnY = 0;


		// MSG
		public override void OnStart () {
			base.OnStart();
			SkipFrame = int.MaxValue;
			ScreenEffect.SetEffectEnable(RetroDarkenEffect.TYPE_ID, true);
			PlayerSpawnY = TargetViewY;
			TargetViewY += Game.Current.ViewRect.height / 2 - Player.GetCameraShiftOffset(Game.Current.ViewRect.height);
		}


		public override void OnEnd () {
			base.OnEnd();
			ScreenEffect.SetEffectEnable(RetroDarkenEffect.TYPE_ID, false);
		}


		public override TaskResult FrameUpdate () {

			int localFrame = LocalFrame;

			// Fade Out
			if (FadeOut) {
				if (localFrame < FADE_OUT_DURATION) {
					var view = Game.Current.ViewRect;
					Game.Current.SetViewPositionDelay(view.x, view.y, 1000, 0);
					RetroDarkenEffect.SetAmount(Util.Remap(
						0f, FADE_OUT_DURATION,
						0f, 1f,
						localFrame
					));
					return TaskResult.Continue;
				} else {
					localFrame -= FADE_OUT_DURATION;
				}
			}

			// Remove Player
			if (localFrame == 1) {
				var player = Player.Selecting;
				if (player != null) {
					player.Active = false;
					player.SetCharacterState(CharacterState.GamePlay);
				}
				SetViewPosition(TargetViewX, TargetViewY + DOLLY_HEIGHT);
				Game.Current.SetViewZ(TargetViewZ);
			}

			// Spawn Player
			if (localFrame == 2) {
				var player = Player.TrySpawnSelectingPlayer(TargetViewX, PlayerSpawnY);
				if (GotoBed) {
					if (Game.Current.TryGetEntityNearby<eBed>(new(player.X, player.Y), out var bed)) {
						bed.Invoke(player);
						player.FullSleep();
					}
				}
			}

			if (localFrame < SkipFrame) {
				// Slow 
				if (localFrame > 2 && FrameInput.AnyKeyDown) {
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
			0
		);


	}
}