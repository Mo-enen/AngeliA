using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using UnityEngine.InputSystem;
using UnityEngine.Audio;


namespace Yaya {
	public partial class Yaya : Game {




		#region --- VAR ---


		// Api
		public static new Yaya Current => Game.Current as Yaya;
		public new YayaWorldSquad WorldSquad_Behind => base.WorldSquad_Behind as YayaWorldSquad;
		public new YayaWorldSquad WorldSquad => base.WorldSquad as YayaWorldSquad;
		public override int PhysicsLayerCount => YayaConst.PHYSICS_LAYER_COUNT;
		public override int StepLayerCount => 3;
		public override int CutsceneStepLayer => YayaConst.STEP_CUTSCENE;
		public YayaMeta YayaMeta => m_YayaMeta;
		public YayaAsset YayaAsset => m_YayaAsset;

		// Ser
		[SerializeField] YayaMeta m_YayaMeta = null;
		[SerializeField] YayaAsset m_YayaAsset = null;

		// Data
		private static readonly HitInfo[] c_DamageCheck = new HitInfo[16];
		private eGamePadUI GamePadUI = null;
		private eControlHintUI ControlHintUI = null;
		private ePauseMenu PauseMenu = null;
		private bool CutsceneLock = true;

		// Saving
		private readonly SavingBool ShowGamePadUI = new("Yaya.ShowGamePadUI", false);
		private readonly SavingBool ShowControlHint = new("Yaya.ShowControlHint", true);


		#endregion




		#region --- MSG ---


#if UNITY_EDITOR
		protected override void Reset () {
			base.Reset();
			m_YayaAsset = new YayaAsset() {
				SquadTransitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f),
				ScreenEffectShaders = new Shader[] {
					Shader.Find("Yaya/GreyScale"),
				},
			};
		}
#endif


		// Init
		protected override void Initialize () {

			base.Initialize();

			// UI Entity
			GamePadUI = PeekOrGetEntity<eGamePadUI>();
			PauseMenu = PeekOrGetEntity<ePauseMenu>();
			ControlHintUI = PeekOrGetEntity<eControlHintUI>();

			// Screen Effects
			CellRenderer.ClearScreenEffects();
			foreach (var shader in m_YayaAsset.ScreenEffectShaders) {
				CellRenderer.AddScreenEffect(shader);
			}

			// Pipeline
			Initialize_Quit();
			Initialize_Player();

			// Start the Game !!
			if (FrameStep.TryAddToLast<sOpening>(YayaConst.OPENING_ID, YayaConst.STEP_ROUTE, out var step)) {
				step.ViewX = VIEW_X;
				step.ViewYStart = VIEW_Y_START;
				step.ViewYEnd = VIEW_Y_END;
				step.SpawnPlayerAtStart = true;
				step.RemovePlayerAtStart = true;
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


		private void Initialize_Quit () {
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
		}


		// Update
		protected override void FrameUpdate () {

			base.FrameUpdate();
			Update_Damage();
			Update_Player();
			Update_HintUI();


			if (FrameInput.CustomKeyDown(Key.Digit1)) {
				SetViewZ(ViewZ + 1);
			}
			if (FrameInput.CustomKeyDown(Key.Digit2)) {
				SetViewZ(ViewZ - 1);
			}
			if (FrameInput.CustomKeyDown(Key.Digit3)) {
				PeekOrGetEntity<eGuaGua>().Feed();
			}
			if (FrameInput.CustomKeyDown(Key.Digit4)) {
				AudioPlayer.PlayMusic("A Creature in the Wild!".AngeHash());
			}
			if (FrameInput.CustomKeyUp(Key.Digit5)) {
				Cutscene.Play(typeof(TestCStep).AngeHash());
			}
			if (FrameInput.CustomKeyUp(Key.Digit6)) {
				Cutscene.Play("Test Video 1".AngeHash());
			}
			if (FrameInput.CustomKeyDown(Key.Digit7)) {
				CellRenderer.StartCameraShake(30);
			}
			if (FrameInput.CustomKeyDown(Key.Digit8)) {
				if (TryAddEntity(typeof(ScreenDialogUI).AngeHash(), 0, 0, out var e)) {
					var dialog = e as ScreenDialogUI;
					dialog.LoadConversation("Test Conversation 0");

				}
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
				ControlHintUI.Player = CurrentPlayer;

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


		// Pauseless
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
				GlobalFrame > Cutscene.StartFrame + Const.CUTSCENE_FADEOUT_DURATION
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
			if (FrameInput.GetKeyDown(GameKey.Start)) {
				switch (State) {
					case GameState.Play:
						State = GameState.Pause;
						break;
					case GameState.Pause:
						State = GameState.Play;
						break;
					case GameState.Cutscene:
						if (!CutsceneLock || Cutscene.IsPlayingStep) {
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
			if (FrameStep.HasStep(YayaConst.STEP_ROUTE)) return;
			// Add Step
			if (FrameStep.TryAddToLast<sSetViewZStep>(YayaConst.SQUAD_STEP_ID, YayaConst.STEP_ROUTE, out var step)) {
				step.Duration = Meta.SquadTransitionDuration;
				step.NewZ = newZ;
			}
		}


		protected override void BeforeViewZChange (int newZ) {
			base.BeforeViewZChange(newZ);
			// Player
			if (CurrentPlayer != null && CurrentPlayer.Active) {
				CurrentPlayer.Renderer.Bounce();
				if (CurrentPlayer is eYaya yaya) yaya.SummonGuaGua(true);
			}
		}


		#endregion




	}
}
