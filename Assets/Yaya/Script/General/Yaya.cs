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
		protected override int PhysicsLayerCount => YayaConst.PHYSICS_LAYER_COUNT;
		public override string SheetRoot => Util.CombinePaths(Util.GetRuntimeBuiltRootPath(), "Universes", UniverseName.Value, "Sheets");
		public override string MapRoot => Util.CombinePaths(Util.GetRuntimeBuiltRootPath(), "Universes", UniverseName.Value, "Maps");
		public override string MapThumbnailRoot => Util.CombinePaths(Util.GetRuntimeBuiltRootPath(), "Universes", UniverseName.Value, "Map Thumbnails");

		// Saving
		private readonly SavingString UniverseName = new("Yaya.UniverseName", "Moenen.Yaya");


		#endregion




		#region --- MSG ---


		protected override void Initialize () {
			Initialize_Universe();
			base.Initialize();
			Initialize_Quit();
		}


		private void Initialize_Universe () {
			string uniRoot = Util.CombinePaths(Util.GetRuntimeBuiltRootPath(), "Universes", UniverseName.Value);
			if (!Util.FolderExists(uniRoot)) UniverseName.Value = UniverseName.DefaultValue;
			uniRoot = Util.CombinePaths(Util.GetRuntimeBuiltRootPath(), "Universes", UniverseName.Value);
			if (!Util.FolderExists(uniRoot)) {
				foreach (var folder in Util.GetFoldersIn(Util.CombinePaths(Util.GetRuntimeBuiltRootPath(), "Universes"), true)) {
					UniverseName.Value = folder.Name;
					break;
				}
			}
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


		#endregion




		#region --- PRO ---


		protected override WorldSquad CreateWorldSquad () => new(MapRoot, YayaConst.LEVEL);


		protected override bool LanguageSupported (SystemLanguage language) => SupportedLanguages.Contains(language);


		#endregion




		#region --- LGC ---




		#endregion




	}
}
