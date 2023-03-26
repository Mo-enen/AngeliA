using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using Rigidbody = AngeliaFramework.Rigidbody;


namespace Yaya {
	public class YayaGame {




		#region --- VAR ---


		// Api
		public static YayaGame Current { get; private set; } = null;

		// Data
		private readonly ePauseMenu PauseMenu = null;
		private readonly eMapEditor MapEditor = null;


		#endregion




		#region --- MSG ---


		// Init
		[AfterGameInitialize]
		public static void AfterInitialize () => new YayaGame();


		private YayaGame () {

			var game = Game.Current;
			Current = this;

			game.OnFrameUpdate -= FrameUpdate;
			game.OnFrameUpdate += FrameUpdate;

			game.OnPauselessUpdate -= PauselessUpdate;
			game.OnPauselessUpdate += PauselessUpdate;

			game.BeforeViewZChange -= YayaBeforeViewZChange;
			game.BeforeViewZChange += YayaBeforeViewZChange;

			Application.wantsToQuit -= OnQuit;
			Application.wantsToQuit += OnQuit;

			PauseMenu = game.PeekOrGetEntity<ePauseMenu>();
			MapEditor = game.PeekOrGetEntity<eMapEditor>();

			Rigidbody.WaterSplashParticleID = typeof(eWaterSplashParticle).AngeHash();
			Character.FootstepParticleCode = eCharacterFootstep.TYPE_ID;
			Character.PassoutParticleCode = ePassoutStarParticle.TYPE_ID;
			Character.SleepParticleCode = eSleepParticle.TYPE_ID;
			Character.SlideParticleCode = eSlideDust.TYPE_ID;
			Character.SleepDoneParticleCode = eDefaultParticle.TYPE_ID;

			// Start Game
			if (
				FrameTask.TryAddToLast(OpeningTask.TYPE_ID, Const.TASK_ROUTE, out var task) &&
				task is OpeningTask oTask
			) {
				Game.Current.SetViewSizeDelay(Game.Current.ViewConfig.DefaultHeight, 1000, int.MaxValue);
				oTask.TargetViewX = 0;
				oTask.TargetViewY = 0;
				oTask.TargetViewZ = 0;
				oTask.GotoBed = true;
				oTask.FadeOut = false;
			}

		}


		// Update
		private void FrameUpdate () {

			if (Game.Current == null) return;

			FrameUpdate_Player();

		}


		private void FrameUpdate_Player () {

			// Spawn Player when No Player Entity
			if (
				Player.Selecting != null &&
				!Player.Selecting.Active &&
				!FrameTask.HasTask(Const.TASK_ROUTE) &&
				!MapEditor.Active
			) {
				var center = CellRenderer.CameraRect.CenterInt();
				Player.TrySpawnSelectingPlayer(center.x, center.y);
			}

			// Reload Game After Player Passout
			if (
				Player.Selecting != null &&
				Player.Selecting.Active &&
				Player.Selecting.CharacterState == CharacterState.Passout &&
				!MapEditor.IsEditing
			) {
				if (
					Player.Selecting.IsFullPassout &&
					FrameInput.GameKeyDown(Gamekey.Action) &&
					!FrameTask.HasTask(Const.TASK_ROUTE)
				) {
					var targetPos = Vector3Int.zero;
					bool gotoBed;
					if (eCheckPoint.SavedUnitPosition.HasValue) {
						// Set Pos to Check Point Saved Pos
						targetPos = eCheckPoint.SavedUnitPosition.Value;
						targetPos.x = targetPos.x.ToGlobal();
						targetPos.y = targetPos.y.ToGlobal();
						gotoBed = false;
					} else {
						// No Check Point
						gotoBed = true;
					}
					// Reload Game
					FrameInput.UseAllHoldingKeys();
					if (FrameTask.TryAddToLast(OpeningTask.TYPE_ID, Const.TASK_ROUTE, out var task) && task is OpeningTask oTask) {
						oTask.TargetViewX = targetPos.x;
						oTask.TargetViewY = targetPos.y;
						oTask.TargetViewZ = targetPos.z;
						oTask.GotoBed = gotoBed;
						oTask.FadeOut = true;
					}
				}
			}
		}


		// Misc
		private void PauselessUpdate () {

			var game = Game.Current;

			// Pause Menu
			if (game.State == GameState.Pause) {
				if (PauseMenu.Active) PauseMenu.FrameUpdate();
				if (!PauseMenu.Active) {
					game.TrySpawnEntity(PauseMenu.TypeID, 0, 0, out _);
					PauseMenu.SetAsPauseMode();
				}
			} else {
				if (PauseMenu.Active) PauseMenu.Active = false;
			}

			// Hint
			if (game.State == GameState.Cutscene) {
				ControlHintUI.AddHint(Gamekey.Start, Language.Get(WORD.HINT_SKIP));
			}

			// Start Key to Switch State
			if (FrameInput.GameKeyDown(Gamekey.Start)) {
				switch (game.State) {
					case GameState.Play:
						game.State = GameState.Pause;
						break;
					case GameState.Pause:
						game.State = GameState.Play;
						break;
					case GameState.Cutscene:
						game.State = GameState.Play;
						break;
				}
			}

		}


		private bool OnQuit () {
#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isPlaying) {
				if (MapEditor.Active) MapEditor.OnInactived();
				return true;
			}
#endif
			var game = Game.Current;
			if (game == null) return true;
			if (game.State == GameState.Pause && PauseMenu.QuitMode) {
				if (MapEditor.Active) MapEditor.OnInactived();
				return true;
			} else {
				game.State = GameState.Pause;
				game.TrySpawnEntity(PauseMenu.TypeID, 0, 0, out _);
				PauseMenu.SetAsQuitMode();
				return false;
			}
		}


		#endregion




		#region --- LGC ---


		private void YayaBeforeViewZChange (int newZ) {
			// Player
			var current = Player.Selecting;
			if (current != null && current.Active) {
				current.RenderBounce();
			}
		}


		#endregion




	}
}