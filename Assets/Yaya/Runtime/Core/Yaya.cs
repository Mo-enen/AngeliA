using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using UnityEngine.InputSystem;


namespace Yaya {
	public class Yaya : Game {




		#region --- VAR ---


		// Api
		public static new Yaya Current => Game.Current as Yaya;
		public new YayaWorldSquad WorldSquad_Behind => base.WorldSquad_Behind as YayaWorldSquad;
		public new YayaWorldSquad WorldSquad => base.WorldSquad as YayaWorldSquad;
		public override int PhysicsLayerCount => YayaConst.PHYSICS_LAYER_COUNT;
		public int AimViewX { get; private set; } = 0;
		public int AimViewY { get; private set; } = 0;

		// Data
		private static readonly HitInfo[] c_DamageCheck = new HitInfo[16];
		private eGamePadUI GamePadUI = null;
		private eControlHintUI ControlHintUI = null;
		private ePauseMenu PauseMenu = null;
		private bool CutsceneLock = true;
		private int PlayerLastGroundedY = 0;

		// Saving
		private readonly SavingBool ShowGamePadUI = new("Yaya.ShowGamePadUI", false);
		private readonly SavingBool ShowControlHint = new("Yaya.ShowControlHint", true);


		#endregion




		#region --- MSG ---


		// Init
		protected override void Initialize () {

			base.Initialize();

			// UI Entity
			GamePadUI = PeekOrGetEntity<eGamePadUI>();
			PauseMenu = PeekOrGetEntity<ePauseMenu>();
			ControlHintUI = PeekOrGetEntity<eControlHintUI>();

			// Quit
			Application.wantsToQuit += () => {
#if UNITY_EDITOR
				if (UnityEditor.EditorApplication.isPlaying) return true;
#endif
				if (State == GameState.Pause && PauseMenu.QuitMode) {
					return true;
				} else {
					State = GameState.Pause;
					TryAddEntity(PauseMenu.TypeID, 0, 0, out _);
					PauseMenu.SetAsQuitMode();
					return false;
				}
			};

			// Start the Game !!
			if (FrameTask.TryAddToLast(tOpening.TYPE_ID, Const.TASK_ROUTE, out var task) && task is tOpening oTask) {
				oTask.ViewX = YayaConst.OPENING_X;
				oTask.ViewYStart = YayaConst.OPENING_Y;
				oTask.ViewYEnd = YayaConst.OPENING_END_Y;
			}

			// Custom Keys
			FrameInput.AddCustomKey(Key.Digit1);
			FrameInput.AddCustomKey(Key.Digit2);
			FrameInput.AddCustomKey(Key.Digit3);
			FrameInput.AddCustomKey(Key.Digit4);
			FrameInput.AddCustomKey(Key.Digit5);
			FrameInput.AddCustomKey(Key.Digit6);
			FrameInput.AddCustomKey(Key.Digit7);
			FrameInput.AddCustomKey(Key.Digit8);
			FrameInput.AddCustomKey(Key.Digit9);
			FrameInput.AddCustomKey(Key.Digit0);

		}


		// Update
		protected override void FrameUpdate () {

			base.FrameUpdate();
			Update_View();
			Update_Damage();
			Update_HintUI();


			if (FrameInput.CustomKeyDown(Key.Digit1)) {
				SetViewZ(ViewZ + 1);
			}
			if (FrameInput.CustomKeyDown(Key.Digit2)) {
				SetViewZ(ViewZ - 1);
			}
			if (FrameInput.CustomKeyDown(Key.Digit3)) {
				PeekOrGetEntity<eGuaGua>().FollowOwner = true;
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


		private void Update_View () {

			var player = ePlayer.Current;
			if (FrameTask.HasTask(Const.TASK_ROUTE)) return;
			if (player == null) return;

			const int LINGER_RATE = 32;
			bool flying = player.Movement.IsFlying;
			int playerX = player.X;
			int playerY = player.Y;
			bool inAir = player.InAir;

			if (!inAir || flying) PlayerLastGroundedY = playerY;
			int linger = ViewRect.width * LINGER_RATE / 1000;
			int centerX = ViewRect.x + ViewRect.width / 2;
			if (playerX < centerX - linger) {
				AimViewX = playerX + linger - ViewRect.width / 2;
			} else if (playerX > centerX + linger) {
				AimViewX = playerX - linger - ViewRect.width / 2;
			}
			AimViewY = !inAir || flying || playerY < PlayerLastGroundedY ? playerY - ViewRect.height * 382 / 1000 : AimViewY;
			SetViewPositionDely(AimViewX, AimViewY, YayaConst.PLAYER_VIEW_LERP_RATE, YayaConst.VIEW_PRIORITY_PLAYER);

			// Clamp
			if (!ViewRect.Contains(playerX, playerY)) {
				if (playerX >= ViewRect.xMax) AimViewX = playerX - ViewRect.width + 1;
				if (playerX <= ViewRect.xMin) AimViewX = playerX - 1;
				if (playerY >= ViewRect.yMax) AimViewY = playerY - ViewRect.height + 1;
				if (playerY <= ViewRect.yMin) AimViewY = playerY - 1;
				SetViewPositionDely(AimViewX, AimViewY, 1000, YayaConst.VIEW_PRIORITY_PLAYER + 1);
			}

		}


		private void Update_Damage () {
			if (State != GameState.Play) return;
			int len = EntityLen;
			for (int i = 0; i < len; i++) {
				var entity = StagedEntities[i];
				if (entity is not IDamageReceiver receiver) continue;
				int count = YayaCellPhysics.OverlapAll_Damage(
					c_DamageCheck, entity.Rect, entity, entity is ePlayer
				);
				for (int j = 0; j < count; j++) {
					receiver.TakeDamage(c_DamageCheck[j].Tag);
				}
			}
		}


		private void Update_HintUI () {

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
					TryAddEntity(GamePadUI.TypeID, 0, 0, out _);
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
					TryAddEntity(ControlHintUI.TypeID, 0, 0, out _);
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
		protected override void PauselessUpdate () {
			base.PauselessUpdate();

			// Pausing
			if (State == GameState.Pause) {
				// Update Entity
				if (ControlHintUI.Active) ControlHintUI.FrameUpdate();
				if (GamePadUI.Active) GamePadUI.FrameUpdate();
				if (PauseMenu.Active) PauseMenu.FrameUpdate();
				if (!PauseMenu.Active) {
					TryAddEntity(PauseMenu.TypeID, 0, 0, out _);
					PauseMenu.SetAsPauseMode();
				}
			} else {
				if (PauseMenu.Active) PauseMenu.Active = false;
			}

			// Cutscene Hint
			if (
				State == GameState.Cutscene &&
				Cutscene.IsPlayingVideo &&
				GlobalFrame > Cutscene.StartFrame + GameMeta.CutsceneVideoFadeoutDuration
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
				switch (State) {
					case GameState.Play:
						State = GameState.Pause;
						break;
					case GameState.Pause:
						State = GameState.Play;
						break;
					case GameState.Cutscene:
						if (!CutsceneLock || Cutscene.IsPlayingTask) {
							State = GameState.Play;
						}
						break;
				}
			}

		}


		#endregion




		#region --- PRO ---


		protected override WorldSquad CreateWorldSquad () => new YayaWorldSquad();


		public void SetViewZDelay (int newZ) {
			if (FrameTask.HasTask(Const.TASK_ROUTE)) return;
			// Add Task
			if (FrameTask.TryAddToLast(tSetViewZTask.TYPE_ID, Const.TASK_ROUTE, out var task) && task is tSetViewZTask svTask) {
				svTask.Duration = YayaConst.SQUAD_TRANSITION_DURATION;
				svTask.NewZ = newZ;
			}
		}


		protected override void BeforeViewZChange (int newZ) {
			base.BeforeViewZChange(newZ);
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
