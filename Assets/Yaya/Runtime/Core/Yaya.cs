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
		private static readonly PhysicsCell[] c_DamageCheck = new PhysicsCell[16];
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

			FrameUpdate_Player();
			Update_View(game);
			Update_Damage(game);
			Update_HintUI(game);

			if (FrameInput.KeyDown(Key.Digit1)) {
				game.SetViewZImmediately(game.ViewZ + 1);
			}
			if (FrameInput.KeyDown(Key.Digit2)) {
				game.SetViewZImmediately(game.ViewZ - 1);
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
			}
			if (FrameInput.KeyPress(Key.Digit6)) {
				game.SetViewSizeDelay(game.ViewRect.height + Const.CEL);
			}
			if (FrameInput.KeyDown(Key.Digit7)) {
				FrameTask.AddToLast(typeof(TestDialogue).AngeHash(), Const.TASK_ROUTE);
			}
			if (FrameInput.KeyDown(Key.Digit8)) {
				Cutscene.PlayVideo("Test Video 0".AngeHash());
			}
			if (FrameInput.KeyDown(Key.Digit9)) {
				Cutscene.PlayVideo("Test Video 1".AngeHash());
			}



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
					// Game Play
					FrameTask.AddToLast(FadeOutTask.TYPE_ID, Const.TASK_ROUTE);
					if (FrameTask.TryAddToLast(OpeningTask.TYPE_ID, Const.TASK_ROUTE, out var task) && task is OpeningTask oTask) {
						oTask.ViewX = YayaConst.OPENING_X;
						oTask.ViewYStart = YayaConst.OPENING_Y;
						oTask.ViewYEnd = YayaConst.OPENING_END_Y;
					}
				}
			}
		}


		private void Update_View (Game game) {

			if (FrameTask.IsTasking<OpeningTask>(Const.TASK_ROUTE)) return;
			var player = ePlayer.Current;
			if (player == null || !player.Active) return;

			const int LINGER_RATE = 32;
			bool flying = player.IsFlying;
			int playerX = player.X;
			int playerY = player.Y;
			bool notInAir = player.IsGrounded || player.InWater || player.InSand || player.IsClimbing;

			if (notInAir || flying) PlayerLastGroundedY = playerY;
			int linger = game.ViewRect.width * LINGER_RATE / 1000;
			int centerX = game.ViewRect.x + game.ViewRect.width / 2;
			if (playerX < centerX - linger) {
				AimViewX = playerX + linger - game.ViewRect.width / 2;
			} else if (playerX > centerX + linger) {
				AimViewX = playerX - linger - game.ViewRect.width / 2;
			}
			AimViewY = notInAir || flying || player.IsSliding || playerY < PlayerLastGroundedY ?
				playerY - game.ViewRect.height * 382 / 1000 : AimViewY;
			game.SetViewPositionDelay(AimViewX, AimViewY, YayaConst.PLAYER_VIEW_LERP_RATE, YayaConst.VIEW_PRIORITY_PLAYER);

			// Clamp
			if (!game.ViewRect.Contains(playerX, playerY)) {
				if (playerX >= game.ViewRect.xMax) AimViewX = playerX - game.ViewRect.width + 1;
				if (playerX <= game.ViewRect.xMin) AimViewX = playerX - 1;
				if (playerY >= game.ViewRect.yMax) AimViewY = playerY - game.ViewRect.height + 1;
				if (playerY <= game.ViewRect.yMin) AimViewY = playerY - 1;
				game.SetViewPositionDelay(AimViewX, AimViewY, 1000, YayaConst.VIEW_PRIORITY_PLAYER + 1);
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

			if (FrameInput.KeyDown(Key.F2)) {
				ShowGamePadUI.Value = !ShowGamePadUI.Value;
				ShowControlHint.Value = ShowGamePadUI.Value ? ShowControlHint.Value : !ShowControlHint.Value;
			}

			// Gamepad
			if (ShowGamePadUI.Value) {
				// Active
				if (!GamePadUI.Active) {
					game.TryAddEntity(GamePadUI.TypeID, 0, 0, out _);
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
				}

				// Y
				int y = 0;
				if (GamePadUI.Active) {
					y = Mathf.Max(GamePadUI.Y + GamePadUI.Height, y);
				}
				ControlHintUI.Y = y;

			} else if (ControlHintUI.Active) {
				// Inactive
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
					game.TryAddEntity(PauseMenu.TypeID, 0, 0, out _);
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
						game.TryAddEntity(ControlHintUI.TypeID, 0, 0, out _);
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
				game.TryAddEntity(PauseMenu.TypeID, 0, 0, out _);
				PauseMenu.SetAsQuitMode();
				return false;
			}
		}


		#endregion




		#region --- API ---


		public void ResetAimViewPosition () {
			AimViewX = Game.Current.ViewRect.x;
			AimViewY = Game.Current.ViewRect.y;
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