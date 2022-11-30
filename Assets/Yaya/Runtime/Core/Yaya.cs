using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using UnityEngine.InputSystem;


namespace Yaya {
	public class Yaya : IInitialize {




		#region --- VAR ---


		// Api
		public static Yaya Current { get; private set; } = null;
		public YayaWorldSquad WorldSquad { get; private set; } = null;
		public YayaWorldSquad WorldSquad_Behind { get; private set; } = null;
		public int AimViewX { get; private set; } = 0;
		public int AimViewY { get; private set; } = 0;

		// Data
		private static readonly HitInfo[] c_DamageCheck = new HitInfo[16];
		private readonly eGamePadUI GamePadUI = null;
		private readonly eControlHintUI ControlHintUI = null;
		private readonly ePauseMenu PauseMenu = null;
		private bool CutsceneLock = true;
		private int PlayerLastGroundedY = 0;

		// Saving
		private readonly SavingBool ShowGamePadUI = new("Yaya.ShowGamePadUI", false);
		private readonly SavingBool ShowControlHint = new("Yaya.ShowControlHint", true);


		#endregion




		#region --- MSG ---


		// Init
		private Yaya () {

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
			GamePadUI = game.PeekOrGetEntity<eGamePadUI>();
			PauseMenu = game.PeekOrGetEntity<ePauseMenu>();
			ControlHintUI = game.PeekOrGetEntity<eControlHintUI>();

			// Quit
			Application.wantsToQuit -= OnQuit;
			Application.wantsToQuit += OnQuit;

			// Custom Keys
			FrameInput.AddCustomKey(Key.Digit0);
			FrameInput.AddCustomKey(Key.Digit1);
			FrameInput.AddCustomKey(Key.Digit2);
			FrameInput.AddCustomKey(Key.Digit3);
			FrameInput.AddCustomKey(Key.Digit4);
			FrameInput.AddCustomKey(Key.Digit5);
			FrameInput.AddCustomKey(Key.Digit6);
			FrameInput.AddCustomKey(Key.Digit7);
			FrameInput.AddCustomKey(Key.Digit8);
			FrameInput.AddCustomKey(Key.Digit9);

			// Start the Game !!
			if (FrameTask.TryAddToLast(tOpening.TYPE_ID, Const.TASK_ROUTE, out var task) && task is tOpening oTask) {
				oTask.ViewX = YayaConst.OPENING_X;
				oTask.ViewYStart = YayaConst.OPENING_Y;
				oTask.ViewYEnd = YayaConst.OPENING_END_Y;
			}

		}


		public static void Initialize () => Current = new Yaya();


		// Update
		private void FrameUpdate () {

			var game = Game.Current;
			if (game == null) return;

			Update_View(game);
			Update_Damage(game);
			Update_HintUI(game);

			if (FrameInput.CustomKeyDown(Key.Digit1)) {
				game.SetViewZ(game.ViewZ + 1);
			}
			if (FrameInput.CustomKeyDown(Key.Digit2)) {
				game.SetViewZ(game.ViewZ - 1);
			}
			if (FrameInput.CustomKeyDown(Key.Digit3)) {
				game.PeekOrGetEntity<eGuaGua>().FollowOwner = true;
			}
			if (FrameInput.CustomKeyDown(Key.Digit4)) {
				AudioPlayer.PlayMusic("A Creature in the Wild!".AngeHash());
			}
			if (FrameInput.CustomKeyUp(Key.Digit5)) {

			}
			if (FrameInput.CustomKeyUp(Key.Digit6)) {
				Cutscene.Play("Test Video 1".AngeHash());
			}
			if (FrameInput.CustomKeyDown(Key.Digit7)) {
				Cutscene.PlayTask(typeof(TestDialogue).AngeHash());
			}

		}


		private void Update_View (Game game) {

			var player = ePlayer.Current;
			if (FrameTask.HasTask(Const.TASK_ROUTE)) return;
			if (player == null) return;

			const int LINGER_RATE = 32;
			bool flying = player.Movement.IsFlying;
			int playerX = player.X;
			int playerY = player.Y;
			bool inAir = player.InAir;

			if (!inAir || flying) PlayerLastGroundedY = playerY;
			int linger = game.ViewRect.width * LINGER_RATE / 1000;
			int centerX = game.ViewRect.x + game.ViewRect.width / 2;
			if (playerX < centerX - linger) {
				AimViewX = playerX + linger - game.ViewRect.width / 2;
			} else if (playerX > centerX + linger) {
				AimViewX = playerX - linger - game.ViewRect.width / 2;
			}
			AimViewY = !inAir || flying || playerY < PlayerLastGroundedY ? playerY - game.ViewRect.height * 382 / 1000 : AimViewY;
			game.SetViewPositionDely(AimViewX, AimViewY, YayaConst.PLAYER_VIEW_LERP_RATE, YayaConst.VIEW_PRIORITY_PLAYER);

			// Clamp
			if (!game.ViewRect.Contains(playerX, playerY)) {
				if (playerX >= game.ViewRect.xMax) AimViewX = playerX - game.ViewRect.width + 1;
				if (playerX <= game.ViewRect.xMin) AimViewX = playerX - 1;
				if (playerY >= game.ViewRect.yMax) AimViewY = playerY - game.ViewRect.height + 1;
				if (playerY <= game.ViewRect.yMin) AimViewY = playerY - 1;
				game.SetViewPositionDely(AimViewX, AimViewY, 1000, YayaConst.VIEW_PRIORITY_PLAYER + 1);
			}

		}


		private void Update_Damage (Game game) {
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


		private void Update_HintUI (Game game) {

			if (FrameInput.CustomKeyDown(Key.F2)) {
				if (ShowGamePadUI.Value != ShowControlHint.Value) {
					// 1 >> 2
					ShowGamePadUI.Value = true;
					ShowControlHint.Value = true;
				} else if (ShowGamePadUI.Value) {
					// 2 >> 0
					ShowGamePadUI.Value = false;
					ShowControlHint.Value = false;
				} else {
					// 0 >> 1
					ShowGamePadUI.Value = false;
					ShowControlHint.Value = true;
				}
			}

			// Gamepad
			if (ShowGamePadUI.Value) {
				// Active
				if (!GamePadUI.Active) {
					game.TryAddEntity(GamePadUI.TypeID, 0, 0, out _);
					GamePadUI.X = 12;
					GamePadUI.Y = 12;
					GamePadUI.Width = 660;
					GamePadUI.Height = 300;
					GamePadUI.DPadLeftPosition = new(50, 110, 60, 40);
					GamePadUI.DPadRightPosition = new(110, 110, 60, 40);
					GamePadUI.DPadDownPosition = new(90, 70, 40, 60);
					GamePadUI.DPadUpPosition = new(90, 130, 40, 60);
					GamePadUI.SelectPosition = new(220, 100, 60, 20);
					GamePadUI.StartPosition = new(300, 100, 60, 20);
					GamePadUI.ButtonAPosition = new(530, 90, 60, 60);
					GamePadUI.ButtonBPosition = new(430, 90, 60, 60);
					GamePadUI.ColorfulButtonTint = new(240, 86, 86, 255);
					GamePadUI.DarkButtonTint = new(0, 0, 0, 255);
					GamePadUI.PressingTint = new(0, 255, 0, 255);

				}
			} else if (GamePadUI.Active) {
				// Inactive
				GamePadUI.Active = false;
			}

			// Ctrl Hint
			if (ShowControlHint.Value) {

				// Spawn
				if (!ControlHintUI.Active) {
					game.TryAddEntity(ControlHintUI.TypeID, 0, 0, out _);
					ControlHintUI.X = 32;
					ControlHintUI.Y = 32;
				}
				ControlHintUI.Player = ePlayer.Current;

				// Y
				int y = 32;
				if (GamePadUI.Active) {
					y = Mathf.Max(GamePadUI.Y + GamePadUI.Height + 32, y);
				}
				ControlHintUI.Y = y;

			} else if (ControlHintUI.Active) {
				// Inactive
				ControlHintUI.Active = false;
			}

		}


		// Override
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
					game.TryAddEntity(PauseMenu.TypeID, 0, 0, out _);
					PauseMenu.SetAsPauseMode();
				}
			} else {
				if (PauseMenu.Active) PauseMenu.Active = false;
			}

			// Cutscene Hint
			if (
				game.State == GameState.Cutscene &&
				Cutscene.IsPlayingVideo &&
				Game.GlobalFrame > Cutscene.StartFrame + game.CutsceneVideoFadeoutDuration
			) {
				if (!CutsceneLock) {
					if (ControlHintUI.Active) {
						ControlHintUI.FrameUpdate();
					}
				} else if (FrameInput.AnyKeyboardKeyPressed(out _) || FrameInput.AnyGamepadButtonPressed(out _)) {
					CutsceneLock = false;
					FrameInput.UseGameKey(GameKey.Start);
				}

				if (GamePadUI.Active) GamePadUI.FrameUpdate();

			} else if (!CutsceneLock) {
				CutsceneLock = true;
			}

			// Start Key to Switch State
			if (FrameInput.GetGameKeyDown(GameKey.Start)) {
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
				game.TryAddEntity(PauseMenu.TypeID, 0, 0, out _);
				PauseMenu.SetAsQuitMode();
				return false;
			}
		}


		#endregion




		#region --- PRO ---


		public void SetViewZDelay (int newZ) {
			if (FrameTask.HasTask(Const.TASK_ROUTE)) return;
			// Add Task
			if (FrameTask.TryAddToLast(tSetViewZTask.TYPE_ID, Const.TASK_ROUTE, out var task) && task is tSetViewZTask svTask) {
				svTask.Duration = YayaConst.SQUAD_TRANSITION_DURATION;
				svTask.NewZ = newZ;
			}
		}


		private void YayaBeforeViewZChange (int newZ) {
			// Player
			var current = ePlayer.Current;
			if (current != null && current.Active) {
				current.Renderer.Bounce();
				if (current.Mascot != null && current.Mascot.FollowOwner) {
					current.Mascot.Summon();
				}
			}
		}


		#endregion




	}
}