using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
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
		public bool GotoBed { get; set; } = false;

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
			TargetViewY += Stage.ViewRect.height / 2 - Player.GetCameraShiftOffset(Stage.ViewRect.height);
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
					var view = Stage.ViewRect;
					Stage.SetViewPositionDelay(view.x, view.y, 1000, 0);
					RetroDarkenEffect.SetAmount(Util.Remap(
						0f, FADE_OUT_DURATION, 0f, 1f, localFrame
					));
					return TaskResult.Continue;
				} else {
					localFrame -= FADE_OUT_DURATION;
				}
			}

			// Frame 1
			if (localFrame == 1) {
				var player = Player.Selecting;
				if (player != null) {
					player.Active = false;
					player.SetCharacterState(CharacterState.GamePlay);
				}
				SetViewPosition(TargetViewX, TargetViewY + DOLLY_HEIGHT);
				Stage.SetViewZ(TargetViewZ);
			}

			// Frame 2
			if (localFrame == 2) {
				var player = Player.Selecting;
				if (player != null) {
					if (!player.Active) {
						player = Stage.SpawnEntity(player.TypeID, TargetViewX, PlayerSpawnY) as Player;
					} else {
						player.X = TargetViewX;
						player.Y = PlayerSpawnY;
					}
					player?.OnActivated();
					player?.Heal(player.MaxHP);
					if (player != null && GotoBed && Stage.TryGetEntity<Bed>(out var bed)) {
						bed.GetTargetOnBed(Player.Selecting);
						player.SetAsFullAsleep();
						TargetViewX = player.Rect.CenterX();
						TargetViewY = player.Y + Stage.ViewRect.height / 2 - Player.GetCameraShiftOffset(Stage.ViewRect.height);
						SetViewPosition(TargetViewX, TargetViewY + DOLLY_HEIGHT);
					}
				}
				Stage.ClearGlobalAntiSpawn();
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


		[OnGameRestart]
		public static void OnGameRestart () {

			if (Player.Selecting == null) return;

			bool gotoBed = true;

			// Get Start Position
			Vector3Int startUnitPosition;
			if (Player.RespawnUnitPosition.HasValue) {
				// CP Respawn Pos
				startUnitPosition = Player.RespawnUnitPosition.Value;
				gotoBed = false;
			} else if (Player.HomeUnitPosition.HasValue) {
				// Sleeped Pos
				startUnitPosition = new Vector3Int(
					Player.HomeUnitPosition.Value.x,
					Player.HomeUnitPosition.Value.y,
					Player.HomeUnitPosition.Value.z
				);
			} else if (IGlobalPosition.TryGetPosition(Player.Selecting.TypeID, out var mapUnitPosition)) {
				// Global Map Pos
				startUnitPosition = mapUnitPosition;
			} else {
				// Fail
				startUnitPosition = default;
			}

			// Start Opening
			if (
				FrameTask.TryAddToLast(TYPE_ID, out var task) &&
				task is OpeningTask oTask
			) {
				Stage.SetViewSizeDelay(Const.DEFAULT_HEIGHT, 1000, int.MaxValue);
				oTask.TargetViewX = startUnitPosition.x.ToGlobal();
				oTask.TargetViewY = startUnitPosition.y.ToGlobal();
				oTask.TargetViewZ = startUnitPosition.z;
				oTask.FadeOut = false;
				oTask.GotoBed = gotoBed;
			}
		}


		// LGC
		private void SetViewPosition (int x, int y) => Stage.SetViewPositionDelay(
			x - Stage.ViewRect.width / 2,
			y - Stage.ViewRect.height / 2,
			1000, 0
		);


	}
}