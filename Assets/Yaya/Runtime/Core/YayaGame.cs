using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using UnityEngine.InputSystem;


namespace Yaya {

	public interface IDamageReceiver {
		void TakeDamage (int damage);
	}

	public enum FittingPose {
		Unknown = 0,
		Left = 1,
		Down = 1,
		Mid = 2,
		Right = 3,
		Up = 3,
		Single = 4,
	}

	public class YayaGame {




		#region --- VAR ---


		// Api
		public static YayaGame Current { get; private set; } = null;
		public YayaWorldSquad WorldSquad { get; private set; } = null;
		public YayaWorldSquad WorldSquad_Behind { get; private set; } = null;
		public bool UseGamePadHint {
			get => ShowGamePadUI.Value;
			set => ShowGamePadUI.Value = value;
		}
		public bool UseControlHint {
			get => ShowControlHint.Value;
			set => ShowControlHint.Value = value;
		}

		// Data
		private static readonly PhysicsCell[] c_DamageCheck = new PhysicsCell[16];
		private readonly eControlHintUI ControlHintUI = null;
		private readonly ePauseMenu PauseMenu = null;
		private bool CutsceneLock = true;

		// Saving
		private readonly SavingBool ShowGamePadUI = new("Yaya.ShowGamePadUI", false);
		private readonly SavingBool ShowControlHint = new("Yaya.ShowControlHint", true);


		#endregion




		#region --- MSG ---


		// Init
		[BeforeGameInitialize]
		public static void BeforeInitialize () {
			Game.Current.PhysicsLayerCount = YayaConst.LAYER_COUNT;
			Game.Current.TaskLayerCount = YayaConst.TASK_LAYER_COUNT;
		}


		[AfterGameInitialize]
		public static void AfterInitialize () => Current = new YayaGame();


		private YayaGame () {

			var game = Game.Current;
			if (game == null) return;

			game.OnFrameUpdate -= FrameUpdate;
			game.OnFrameUpdate += FrameUpdate;

			game.OnPauselessUpdate -= PauselessUpdate;
			game.OnPauselessUpdate += PauselessUpdate;

			// World
			game.WorldSquad = WorldSquad = new YayaWorldSquad();
			game.WorldSquad_Behind = WorldSquad_Behind = new YayaWorldSquad(true);
			game.BeforeViewZChange -= YayaBeforeViewZChange;
			game.BeforeViewZChange += YayaBeforeViewZChange;

			// UI Entity
			PauseMenu = game.PeekOrGetEntity<ePauseMenu>();
			ControlHintUI = game.PeekOrGetEntity<eControlHintUI>();

			// Quit
			Application.wantsToQuit -= OnQuit;
			Application.wantsToQuit += OnQuit;

			// Start the Game !!
			if (
				FrameTask.TryAddToLast(OpeningTask.TYPE_ID, YayaConst.TASK_ROUTE, out var task) &&
				task is OpeningTask oTask
			) {
				var homePos = ePlayer.Selecting != null ? ePlayer.Selecting.GetHomePosition() : default;
				oTask.TargetViewX = homePos.x;
				oTask.TargetViewY = homePos.y;
				oTask.TargetViewZ = homePos.z;
				oTask.GotoBed = true;
				oTask.FadeOut = false;
			}

		}


		// Update
		private void FrameUpdate () {

			if (Game.Current == null) return;

			FrameUpdate_Player();
			Update_Damage();
			Update_HintUI();


			// ============ Test ============

			var game = Game.Current;
			if (FrameInput.KeyDown(Key.Digit1)) {
				game.SetViewZ(game.ViewZ + 1);
			}
			if (FrameInput.KeyDown(Key.Digit2)) {
				game.SetViewZ(game.ViewZ - 1);
			}
			if (FrameInput.KeyDown(Key.Digit3)) {
				if (ePlayer.Selecting != null) {
					ePlayer.Selecting.Mascot.FollowOwner = true;
				}
			}
			if (FrameInput.KeyDown(Key.Digit4)) {
				AudioPlayer.PlayMusic("A Creature in the Wild!".AngeHash());
			}
			if (FrameInput.KeyPress(Key.Digit5)) {
				game.SetViewSizeDelay(game.ViewRect.height - Const.CEL);
				//AudioPlayer.SetLowpass(1000);
			}
			if (FrameInput.KeyPress(Key.Digit6)) {
				game.SetViewSizeDelay(game.ViewRect.height + Const.CEL);
				//AudioPlayer.SetLowpass(100);
			}
			if (FrameInput.KeyDown(Key.Digit7)) {
				DialoguePerformer.PerformDialogue("TestConversation", YayaConst.TASK_ROUTE);
			}
			if (FrameInput.KeyDown(Key.Digit8)) {
				var miniGame = game.PeekOrGetEntity<eGomokuUI>();
				if (miniGame == null || !miniGame.Active) {
					game.SpawnEntity<eGomokuUI>(0, 0);
				} else {
					miniGame.Active = false;
				}
			}
			if (FrameInput.KeyDown(Key.Digit9)) {

				Cutscene.Play("Test Video 1".AngeHash());
			}
			if (FrameInput.KeyDown(Key.Digit0)) {
				ePlayer.Selecting.SetHealth(0);
			}

			// ============ Test ============



		}


		private void FrameUpdate_Player () {

			// Spawn Player when No Player Entity
			if (
				ePlayer.Selecting == null &&
				!FrameTask.HasTask(YayaConst.TASK_ROUTE)
			) {
				var center = CellRenderer.CameraRect.CenterInt();
				ePlayer.TrySpawnSelectingPlayer(center.x, center.y);
			}

			// Reload Game After Player Passout
			if (
				ePlayer.Selecting != null &&
				ePlayer.Selecting.Active &&
				ePlayer.Selecting.CharacterState == CharacterState.Passout
			) {
				if (
					Game.GlobalFrame > ePlayer.Selecting.PassoutFrame + YayaConst.PASSOUT_WAIT &&
					FrameInput.GameKeyDown(GameKey.Action) &&
					!FrameTask.HasTask(YayaConst.TASK_ROUTE)
				) {
					Vector3Int targetPos;
					bool gotoBed;
					if (eCheckPoint.SavedPosition.HasValue) {
						// Set Pos to Check Point Saved Pos
						targetPos = eCheckPoint.SavedPosition.Value;
						gotoBed = false;
					} else {
						// Set Pos to First Player Map Pos
						var homePos = ePlayer.Selecting != null ? ePlayer.Selecting.GetHomePosition() : default;
						targetPos = homePos;
						gotoBed = true;
					}
					// Reload Game
					FrameInput.UseAllHoldingKeys();
					if (FrameTask.TryAddToLast(OpeningTask.TYPE_ID, YayaConst.TASK_ROUTE, out var task) && task is OpeningTask oTask) {
						oTask.TargetViewX = targetPos.x;
						oTask.TargetViewY = targetPos.y;
						oTask.TargetViewZ = targetPos.z;
						oTask.GotoBed = gotoBed;
						oTask.FadeOut = true;
					}
				}
			}
		}


		private void Update_Damage () {
			var game = Game.Current;
			if (game.State != GameState.Play) return;
			int len = game.EntityLen;
			for (int i = 0; i < len; i++) {
				var entity = game.Entities[i];
				if (entity is not IDamageReceiver receiver) continue;
				int count = YayaCellPhysics.OverlapAll_Damage(
					c_DamageCheck, entity.Rect, entity, entity is ePlayer
				);
				for (int j = 0; j < count; j++) {
					var hit = c_DamageCheck[j];
					receiver.TakeDamage(hit.Tag);
					if (hit.Entity is eBullet bullet) {
						bullet.OnHit(receiver);
					} else if (hit.Entity != null) {
						hit.Entity.Active = false;
					}
				}
			}
		}


		private void Update_HintUI () {

			if (FrameInput.KeyDown(Key.F2)) {
				ShowGamePadUI.Value = !ShowGamePadUI.Value;
				ShowControlHint.Value = ShowGamePadUI.Value ? ShowControlHint.Value : !ShowControlHint.Value;
			}

			// Ctrl Hint
			if (!ControlHintUI.Active) {
				Game.Current.TrySpawnEntity(ControlHintUI.TypeID, 0, 0, out _);
			}

		}


		// Misc
		private void PauselessUpdate () {

			var game = Game.Current;
			if (game == null) return;

			// Pausing
			if (game.State == GameState.Pause) {
				// Update Entity
				Update_HintUI();
				ControlHintUI.FrameUpdate();
				if (PauseMenu.Active) PauseMenu.FrameUpdate();
				if (!PauseMenu.Active) {
					game.TrySpawnEntity(PauseMenu.TypeID, 0, 0, out _);
					PauseMenu.SetAsPauseMode();
				}
			} else {
				if (PauseMenu.Active) PauseMenu.Active = false;
			}

			// Video Cutscene
			if (
				game.State == GameState.Cutscene &&
				Cutscene.IsPlaying &&
				Game.GlobalFrame > Cutscene.StartFrame + game.CutsceneVideoFadeoutDuration
			) {
				if (!CutsceneLock) {
					eControlHintUI.DrawHint(GameKey.Start, WORD.HINT_SKIP);
					ControlHintUI.FrameUpdate();
				} else if (
					FrameInput.AnyKeyboardKeyPress(out _) ||
					FrameInput.AnyGamepadButtonPress(out _) ||
					FrameInput.MouseLeftButton || FrameInput.MouseRightButton
				) {
					CutsceneLock = false;
					FrameInput.UseGameKey(GameKey.Start);
				}
			} else if (!CutsceneLock) {
				CutsceneLock = true;
			}

			// Start Key to Switch State
			if (FrameInput.GameKeyDown(GameKey.Start)) {
				switch (game.State) {
					case GameState.Play:
						game.State = GameState.Pause;
						break;
					case GameState.Pause:
						game.State = GameState.Play;
						break;
					case GameState.Cutscene:
						if (!CutsceneLock) {
							game.State = GameState.Play;
						}
						break;
				}
			}

		}


		private bool OnQuit () {
#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isPlaying) return true;
#endif
			var game = Game.Current;
			if (game == null) return true;
			if (game.State == GameState.Pause && PauseMenu.QuitMode) {
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
			var current = ePlayer.Selecting;
			if (current != null && current.Active) {
				current.RenderBounce();
				if (current.Mascot != null && current.Mascot.FollowOwner) {
					current.Mascot.Summon();
				}
			}
		}


		#endregion




	}
}