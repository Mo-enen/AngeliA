using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	// === Main ===
	public partial class Yaya : Game {




		#region --- VAR ---


		// Api
		public override int PhysicsLayerCount => YayaConst.PHYSICS_LAYER_COUNT;
		public static Dictionary<Vector2Int, CheckPointMeta.Data> CpPool { get; } = new();
		public static Dictionary<int, Vector2Int> CpAltarPool { get; } = new();
		public override RectInt CameraRect => YayaCameraRect;
		public YayaMeta YayaMeta => m_YayaMeta;
		public YayaAsset YayaAsset => m_YayaAsset;

		// Ser
		[SerializeField] YayaMeta m_YayaMeta = null;
		[SerializeField] YayaAsset m_YayaAsset = null;

		// Data
		private static readonly HitInfo[] c_DamageCheck = new HitInfo[16];
		private RectInt YayaCameraRect = default;
		private eGamePadUI GamePadUI = null;
		private eControlHint ControlHintUI = null;

		// Saving
		private readonly SavingBool ShowGamePadUI = new("Yaya.ShowGamePadUI", false);


		#endregion




		#region --- MSG ---


		// Init
		protected override void Initialize () {

			base.Initialize();
			Initialize_Quit();
			Initialize_YayaMeta();
			Initialize_Player();
			FrameInput.AddCustomKey(KeyCode.Escape);


			FrameInput.AddCustomKey(KeyCode.Alpha1);
			FrameInput.AddCustomKey(KeyCode.Alpha2);
			FrameInput.AddCustomKey(KeyCode.Alpha3);
			FrameInput.AddCustomKey(KeyCode.Alpha4);
			FrameInput.AddCustomKey(KeyCode.Alpha5);
			FrameInput.AddCustomKey(KeyCode.Alpha6);

		}


		private void Initialize_Quit () {
			bool willQuit = false;
			Application.wantsToQuit += () => {
#if UNITY_EDITOR
				if (UnityEditor.EditorApplication.isPlaying) return true;
#endif
				if (willQuit) {
					return true;
				} else {
					// Show Quit Dialog



					willQuit = true;
					Application.Quit();
					return false;



				}
			};
		}


		private void Initialize_YayaMeta () {
			// Check Points
			try {
				CpPool.Clear();
				CpAltarPool.Clear();
				var cpMeta = LoadMeta<CheckPointMeta>();
				if (cpMeta != null) {
					foreach (var cpData in cpMeta.CPs) {
						var pos = new Vector2Int(cpData.X, cpData.Y);
						CpPool.TryAdd(pos, cpData);
						if (cpData.IsAltar) CpAltarPool.TryAdd(cpData.Index, pos);
					}
				}
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}


		// Update
		protected override void FrameUpdate () {

			Update_CameraRect();
			base.FrameUpdate();
			Update_Damage();
			Update_Player();

			UpdateUI_Gamepad();
			UpdateUI_ControlHint();




			if (FrameInput.CustomKeyDown(KeyCode.Alpha1)) {
				SetViewZ(ViewZ + 1);
				//AudioPlayer.PlaySound("BloopDownPitch".AngeHash());
			}
			if (FrameInput.CustomKeyDown(KeyCode.Alpha2)) {
				SetViewZ(ViewZ - 1);
				//AudioPlayer.PlaySound("Brassic".AngeHash());

			}

		}


		private void Update_CameraRect () {
			if (CellRenderer.HasEffect<fSquadTransition>()) {
				var exp = (
					(Vector2)CellRenderer.CameraRect.size * (Universe.Meta.SquadBehindParallax / 1000f - 1f)
				).CeilToInt();
				YayaCameraRect = base.CameraRect.Expand(exp.x, exp.x, exp.y, exp.y);
			} else {
				YayaCameraRect = base.CameraRect;
			}
		}


		private void Update_Damage () {
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


		private void UpdateUI_Gamepad () {
			if (FrameInput.CustomKeyDown(KeyCode.F2)) {
				ShowGamePadUI.Value = !ShowGamePadUI.Value;
				if (GamePadUI != null) GamePadUI.ResetLocalFrame();
			}
			if (ShowGamePadUI.Value) {
				// Active
				if (GamePadUI == null) {
					if (TryGetEntityInStage<eGamePadUI>(out var gPad)) {
						GamePadUI = gPad;
					} else {
						GamePadUI = AddEntity(typeof(eGamePadUI).AngeHash(), 0, 0) as eGamePadUI;
						GamePadUI.X = 12;
						GamePadUI.Y = -300;
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
				}
				// Lerp
				const int DURATION = 20;
				if (GamePadUI.LocalFrame <= DURATION) {
					float t01 = m_YayaAsset.UiPopCurve.Evaluate(Mathf.InverseLerp(0, DURATION, GamePadUI.LocalFrame));
					GamePadUI.Y = Mathf.LerpUnclamped(-300, 12, t01).RoundToInt();
				} else if (GamePadUI.Y != 12) {
					GamePadUI.Y = 12;
				}
			} else if (GamePadUI != null) {
				// Inactive
				if (GamePadUI.Y > -320) {
					GamePadUI.Y = Mathf.LerpUnclamped(GamePadUI.Y, -340, 0.1f).RoundToInt();
				} else {
					GamePadUI.Active = false;
					GamePadUI = null;
				}
			}
		}


		private void UpdateUI_ControlHint () {

			if (CurrentPlayer == null || !CurrentPlayer.Active) return;

			// Spawn
			if (ControlHintUI == null) {
				if (TryGetEntityInStage<eControlHint>(out var cHint)) {
					ControlHintUI = cHint;
				} else {
					ControlHintUI = AddEntity(typeof(eControlHint).AngeHash(), 0, 0) as eControlHint;
					ControlHintUI.X = 32;
					ControlHintUI.Y = 32;
				}
			}
			if (ControlHintUI == null) return;

			// Y
			ControlHintUI.Player = CurrentPlayer;
			int y = 32;
			if (GamePadUI != null && GamePadUI.Active) {
				y = Mathf.Max(GamePadUI.Y + GamePadUI.Height + 32, y);
			}
			ControlHintUI.Y = y;

		}


		// Override
		protected override void PauselessUpdate () {
			base.PauselessUpdate();
			// Pause
			if (FrameInput.KeyDown(GameKey.Start) || FrameInput.CustomKeyDown(KeyCode.Escape)) {
				IsPausing = !IsPausing;
				if (IsPausing) {
					AudioPlayer.Pause();
				} else {
					AudioPlayer.UnPause();
				}
			}
		}


		#endregion




		#region --- PRO ---


		protected override WorldSquad CreateWorldSquad () => new YayaWorldSquad(Universe.MapRoot);


		protected override void BeforeViewZChange (int newZ) {
			base.BeforeViewZChange(newZ);
			// Add Effect
			CellRenderer.RemoveEffect<fSquadTransition>();
			CellRenderer.AddEffect(new fSquadTransition(
				Universe.Meta.SquadTransitionDuration,
				newZ > ViewZ ?
					1000f / Universe.Meta.SquadBehindParallax :
					Universe.Meta.SquadBehindParallax / 1000f,
				Universe.Meta.SquadBehindAlpha / 255f,
				m_YayaAsset.SquadTransitionCurve
			));
		}


		#endregion




	}
}
