using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class Yaya : Game {




		#region --- VAR ---


		// Const
		private readonly HashSet<SystemLanguage> SupportedLanguages = new() { SystemLanguage.English, SystemLanguage.ChineseSimplified, };

		// Api
		public override int PhysicsLayerCount => YayaConst.PHYSICS_LAYER_COUNT;
		public static readonly Dictionary<Vector2Int, YayaMeta.Data> CpPool = new();
		public static readonly Dictionary<int, Vector2Int> CpAltarPool = new();


		#endregion




		#region --- MSG ---


		protected override void Initialize () {
			base.Initialize();
			Initialize_Quit();
			Initialize_YayaMeta();
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
			try {
				// Check Points
				CpPool.Clear();
				CpAltarPool.Clear();
				var cpMeta = LoadMeta<YayaMeta>();
				if (cpMeta != null) {
					foreach (var cpData in cpMeta.CPs) {
						var pos = new Vector2Int(cpData.X, cpData.Y);
						CpPool.TryAdd(pos, cpData);
						if (cpData.IsAltar) CpAltarPool.TryAdd(cpData.Index, pos);
					}
				}
				{
					// Spawn Player
					var pos = ViewRect.CenterInt();
					if (!WorldSquad.FindBlock(typeof(ePlayerBed).AngeHash(), out _, BlockType.Entity)) {
						AddEntity(typeof(ePlayer).AngeHash(), pos.x, pos.y);
					}
					// Fix View Position
					var view = ViewRect;
					view.x = pos.x - ViewRect.width / 2;
					view.y = pos.y - ViewRect.height / 2;
					SetViewPositionDely(view.x, view.y);
					SetViewRectImmediately(view);


				}
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}


		#endregion




		#region --- PRO ---


		protected override WorldSquad CreateWorldSquad () => new(Universe.MapRoot, YayaConst.LEVEL, Universe.RenderMeta.MaxParallax);


		protected override bool LanguageSupported (SystemLanguage language) => SupportedLanguages.Contains(language);


		#endregion




		#region --- LGC ---




		#endregion




	}
}
