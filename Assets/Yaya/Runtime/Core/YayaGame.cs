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
			set {
				if (ShowGamePadUI.Value != value) {
					ShowGamePadUI.Value = value;
					if (value) {
						Game.Current.SpawnEntity(GamePadUI.TypeID, 0, 0);
						GamePadUI.Active = true;
					} else {
						GamePadUI.Active = false;
					}
				}
			}
		}
		public bool UseControlHint {
			get => ShowControlHint.Value;
			set {
				if (ShowControlHint.Value != value) {
					ShowControlHint.Value = value;
					if (value) {
						Game.Current.SpawnEntity(ControlHintUI.TypeID, 0, 0);
						ControlHintUI.Active = true;
					} else {
						ControlHintUI.Active = false;
					}
				}
			}
		}

		// Data
		private static readonly PhysicsCell[] c_DamageCheck = new PhysicsCell[16];
		private readonly eGamePadUI GamePadUI = null;
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
		public static void BeforeInitialize () => Game.Current.PhysicsLayerCount = YayaConst.LAYER_COUNT;


		[AfterGameInitialize]
		public static void AfterInitialize () => Current = new YayaGame();


		private YayaGame () {

			var game = Game.Current;
			if (game == null) return;

			game.OnFrameUpdate -= FrameUpdate;
			game.OnFrameUpdate += FrameUpdate;

			game.OnPauselessUpdate -= PauselessUpdate;
			game.OnPauselessUpdate += PauselessUpdate;

			game.PhysicsLayerCount = YayaConst.LAYER_COUNT;

			// World
			game.WorldSquad = WorldSquad = new YayaWorldSquad();
			game.WorldSquad_Behind = WorldSquad_Behind = new YayaWorldSquad(true);
			game.BeforeViewZChange -= YayaBeforeViewZChange;
			game.BeforeViewZChange += YayaBeforeViewZChange;

			// UI Entity
			GamePadUI = game.PeekOrGetEntity<eGamePadUI>();
			PauseMenu = game.PeekOrGetEntity<ePauseMenu>();
			ControlHintUI = game.PeekOrGetEntity<eControlHintUI>();

			// Quit
			Application.wantsToQuit -= OnQuit;
			Application.wantsToQuit += OnQuit;

			// Start the Game !!
			if (FrameTask.TryAddToLast(OpeningTask.TYPE_ID, Const.TASK_ROUTE, out var task) && task is OpeningTask oTask) {
				oTask.TargetViewX = YayaConst.OPENING_X;
				oTask.TargetViewY = YayaConst.OPENING_Y;
				oTask.TargetViewZ = 0;
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
				if (ePlayer.Current != null) {
					ePlayer.Current.Mascot.FollowOwner = true;
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
				DialoguePerformer.PerformTask<YayaDialoguePerformer>("TestConversation");
			}
			if (FrameInput.KeyDown(Key.Digit8)) {
				game.WorldSquad.SetDataChannel(World.DataChannel.BuiltIn);
				game.WorldSquad_Behind.SetDataChannel(World.DataChannel.BuiltIn);
				game.SetViewZ(game.ViewZ);
			}
			if (FrameInput.KeyDown(Key.Digit9)) {
				game.WorldSquad.SetDataChannel(World.DataChannel.User);
				game.WorldSquad_Behind.SetDataChannel(World.DataChannel.User);
				game.SetViewZ(game.ViewZ);
				//Cutscene.PlayVideo("Test Video 1".AngeHash());
			}
			if (FrameInput.KeyDown(Key.Digit0)) {
				ePlayer.Current.SetHealth(0);
			}

			// ============ Test ============



		}


		private void FrameUpdate_Player () {

			// Spawn Player when No Player Entity
			if (
				ePlayer.Current == null &&
				!FrameTask.HasTask(Const.TASK_ROUTE)
			) {
				var center = CellRenderer.CameraRect.CenterInt();
				ePlayer.TrySpawnPlayer(center.x, center.y);
			}

			// Reload Game and Player After Passout
			if (
				ePlayer.Current != null && ePlayer.Current.Active &&
				ePlayer.Current.CharacterState == CharacterState.Passout
			) {
				if (
					Game.GlobalFrame > ePlayer.Current.PassoutFrame + YayaConst.PASSOUT_WAIT &&
					FrameInput.GameKeyDown(GameKey.Action) &&
					!FrameTask.HasTask(Const.TASK_ROUTE)
				) {
					// Reopen Game
					FrameTask.AddToLast(FadeOutTask.TYPE_ID, Const.TASK_ROUTE);
					if (FrameTask.TryAddToLast(OpeningTask.TYPE_ID, Const.TASK_ROUTE, out var task) && task is OpeningTask oTask) {
						oTask.TargetViewX = YayaConst.OPENING_X;
						oTask.TargetViewY = YayaConst.OPENING_Y;
						oTask.TargetViewZ = 0;
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

			// Gamepad
			if (ShowGamePadUI.Value) {
				if (!GamePadUI.Active) {
					Game.Current.TrySpawnEntity(GamePadUI.TypeID, 0, 0, out _);
				}
			} else if (GamePadUI.Active) {
				GamePadUI.Active = false;
			}

			// Ctrl Hint
			if (ShowControlHint.Value) {
				if (!ControlHintUI.Active) {
					Game.Current.TrySpawnEntity(ControlHintUI.TypeID, 0, 0, out _);
				}
			} else if (ControlHintUI.Active) {
				ControlHintUI.Active = false;
			}

		}


		// Misc
		private void PauselessUpdate () {

			var game = Game.Current;
			if (game == null) return;

			// Pausing
			if (game.State == GameState.Pause) {
				// Update Entity
				if (ControlHintUI.Active) ControlHintUI.FrameUpdate();
				if (GamePadUI.Active) GamePadUI.FrameUpdate();
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
				Cutscene.IsPlayingVideo &&
				Game.GlobalFrame > Cutscene.StartFrame + game.CutsceneVideoFadeoutDuration
			) {
				if (!CutsceneLock) {
					if (ControlHintUI.Active) {
						ControlHintUI.FrameUpdate();
					} else {
						game.TrySpawnEntity(ControlHintUI.TypeID, 0, 0, out _);
					}
				} else if (
					FrameInput.AnyKeyboardKeyPress(out _) ||
					FrameInput.AnyGamepadButtonPress(out _) ||
					FrameInput.MouseLeftButton || FrameInput.MouseRightButton
				) {
					CutsceneLock = false;
					FrameInput.UseGameKey(GameKey.Start);
				}

				if (GamePadUI.Active) GamePadUI.Active = false;

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
						if (!CutsceneLock || Cutscene.IsPlayingTask) {
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
			var current = ePlayer.Current;
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