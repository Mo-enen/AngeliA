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
		private readonly SavingBool ShowControlHint = new("Yaya.ShowControlHint", true);


		#endregion




		#region --- MSG ---


		// Init
		protected override void Initialize () {

			base.Initialize();
			Initialize_Quit();
			Initialize_YayaMeta();
			Initialize_Player();



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
			Update_HintUI();




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


		private void Update_HintUI () {

			if (FrameInput.CustomKeyDown(KeyCode.F2)) {
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
				if (GamePadUI == null) {
					if (TryGetEntityInStage<eGamePadUI>(out var gPad)) {
						GamePadUI = gPad;
					} else {
						GamePadUI = AddEntity(typeof(eGamePadUI).AngeHash(), 0, 0) as eGamePadUI;
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
				}
			} else if (GamePadUI != null) {
				// Inactive
				GamePadUI.Active = false;
				GamePadUI = null;
			}

			// Ctrl Hint
			if (ShowControlHint.Value && CurrentPlayer != null && CurrentPlayer.Active) {

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
				ControlHintUI.Player = CurrentPlayer;

				// Y
				int y = 32;
				if (GamePadUI != null && GamePadUI.Active) {
					y = Mathf.Max(GamePadUI.Y + GamePadUI.Height + 32, y);
				}
				ControlHintUI.Y = y;

			} else if (ControlHintUI != null) {
				// Inactive
				ControlHintUI.Active = false;
				ControlHintUI = null;
			}

		}


		// Override
		protected override void PauselessUpdate () {
			base.PauselessUpdate();
			// Pause
			if (FrameInput.KeyDown(GameKey.Start)) {
				IsPausing = !IsPausing;
				if (IsPausing) {
					AudioPlayer.Pause();
				} else {
					AudioPlayer.UnPause();
				}
			}
			if (IsPausing) {
				if (ControlHintUI != null && ControlHintUI.Active) {
					ControlHintUI.FrameUpdate();
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
