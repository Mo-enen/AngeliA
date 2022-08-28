using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	// === Main ===
	public partial class YayaGame : Game {




		#region --- VAR ---


		// Const
		private readonly HashSet<SystemLanguage> SupportedLanguages = new() { SystemLanguage.English, SystemLanguage.ChineseSimplified, };

		// Api
		public override int PhysicsLayerCount => YayaConst.PHYSICS_LAYER_COUNT;
		public static Dictionary<Vector2Int, CheckPointMeta.Data> CpPool { get; } = new();
		public static Dictionary<int, Vector2Int> CpAltarPool { get; } = new();


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
		}


		// Update
		protected override void FrameUpdate () {
			base.FrameUpdate();
			Update_Player();


			if (FrameInput.CustomKeyDown(KeyCode.Alpha1)) {
				SetViewZ(ViewZ + 1);
			}
			if (FrameInput.CustomKeyDown(KeyCode.Alpha2)) {
				SetViewZ(ViewZ - 1);
			}


		}


		#endregion




		#region --- PRO ---


		protected override WorldSquad CreateWorldSquad () => new YayaWorldSquad(Universe.MapRoot);


		protected override bool LanguageSupported (SystemLanguage language) => SupportedLanguages.Contains(language);


		#endregion




	}
}
