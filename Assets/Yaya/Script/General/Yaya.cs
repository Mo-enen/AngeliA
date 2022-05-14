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


		#endregion




		#region --- MSG ---


		protected override void Initialize () {
			base.Initialize();
			Initialize_Quit();
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


		private void Initialize_Player () {
			var pos = ViewRect.CenterInt();
			AddEntity(typeof(ePlayer).AngeHash(), pos.x, pos.y);
		}


		#endregion




		#region --- PRO ---


		protected override WorldSquad CreateWorldSquad () => new(MapRoot, YayaConst.LEVEL);


		protected override bool LanguageSupported (SystemLanguage language) => SupportedLanguages.Contains(language);


		#endregion




		#region --- LGC ---




		#endregion




	}
}
