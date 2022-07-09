using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class Yaya : Game {




		#region --- VAR ---


		// Const
		private readonly HashSet<SystemLanguage> SupportedLanguages = new() { SystemLanguage.English, SystemLanguage.ChineseSimplified, };
		//private static readonly int OPENING_STEP_ID = typeof(sOpening).AngeHash();
		//private static readonly int FADEOUT_STEP_ID = typeof(sFadeOut).AngeHash();

		// Api
		public override int PhysicsLayerCount => YayaConst.PHYSICS_LAYER_COUNT;
		public PhysicsMeta PhysicsMeta { get; private set; } = new();
		public YayaMeta YayaMeta { get; private set; } = new();
		public static Dictionary<Vector2Int, CheckPointMeta.Data> CpPool { get; } = new();
		public static Dictionary<int, Vector2Int> CpAltarPool { get; } = new();

		// Ser
		[SerializeField] PhysicsMeta m_DefaultPhysicsMeta = null;
		[SerializeField] YayaMeta m_DefaultYayaMeta = null;

		// Data
		private ePlayer Player => _Player ??= TryGetEntityInStage<ePlayer>(out var p) ? p : null;
		private ePlayer _Player = null;


		#endregion




		#region --- MSG ---


		// Init
		protected override void Initialize () {
			base.Initialize();
			Initialize_Quit();
			Initialize_YayaMeta();
			CellStep.AddToLast(new sOpening());
			Initialize_Player();
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
					PlayerData.SaveToDisk();
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
			// Metas
			try {
				PhysicsMeta = LoadMeta<PhysicsMeta>() ?? m_DefaultPhysicsMeta;
				YayaMeta = LoadMeta<YayaMeta>() ?? m_DefaultYayaMeta;
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}


		private void Initialize_Player () {
			try {
				if (!CellStep.IsStepping<sOpening>()) {
					// Spawn Player
					var pos = ViewRect.CenterInt();
					AddEntity(typeof(ePlayer).AngeHash(), pos.x, pos.y);
					// Fix View Position
					var view = ViewRect;
					view.x = pos.x - ViewRect.width / 2;
					view.y = pos.y - ViewRect.height / 2;
					SetViewPositionDely(view.x, view.y, 1000, int.MaxValue);
					SetViewRectImmediately(view);
				}
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}


		// Update
		protected override void FrameUpdate () {
			base.FrameUpdate();
			Update_Player();
		}


		private void Update_Player () {
			if (Player == null) return;
			// Reload Game After Passout
			if (
				Player.Active &&
				Player.CharacterState == eCharacter.State.Passout &&
				GlobalFrame > Player.PassoutFrame + 48 &&
				FrameInput.KeyDown(GameKey.Action)
			) {
				CellStep.AddToLast(new sFadeOut());
				CellStep.AddToLast(new sOpening());
			}
		}


		#endregion




		#region --- PRO ---


		protected override WorldSquad CreateWorldSquad () => new YayaWorldSquad(
			Universe.MapRoot, Universe.RenderMeta.MaxParallax, YayaMeta
		);


		protected override bool LanguageSupported (SystemLanguage language) => SupportedLanguages.Contains(language);


		#endregion




		#region --- LGC ---




		#endregion




	}
}
