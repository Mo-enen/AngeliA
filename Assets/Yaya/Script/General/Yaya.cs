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
		public override string UniversePath {
			get {
				if (string.IsNullOrEmpty(_YayaUniversePath)) {
					if (!string.IsNullOrEmpty(UniverseName.Value)) {
						_YayaUniversePath = Util.CombinePaths(CustomUniverseRoot, UniverseName.Value);
					} else {
						_YayaUniversePath = base.UniversePath;
					}
				}
				return _YayaUniversePath;
			}
		}
		protected override int PhysicsLayerCount => YayaConst.PHYSICS_LAYER_COUNT;

		// Short
		private string CustomUniverseRoot => !string.IsNullOrEmpty(_CustomUniverseRoot) ?
			_CustomUniverseRoot :
			(_CustomUniverseRoot = Util.CombinePaths(Application.persistentDataPath, "Universes"));

		// Data 
		[System.NonSerialized] string _YayaUniversePath = null;
		[System.NonSerialized] string _CustomUniverseRoot = null;

		// Saving
		private readonly SavingString UniverseName = new("Yaya.UniverseName", "");


		#endregion




		#region --- MSG ---


		protected override void Initialize () {
			Initialize_Universe();
			base.Initialize();
			Initialize_Quit();
			Initialize_Player();
		}


		private void Initialize_Universe () {

			if (Util.FolderExists(UniversePath)) return;

			// Load Default Universe
			UniverseName.Value = "";
			_YayaUniversePath = "";
			if (Util.FolderExists(UniversePath)) return;

			// Load First Custom Universe
			_YayaUniversePath = "";
			foreach (var folder in Util.GetFoldersIn(Util.CombinePaths(Application.persistentDataPath, "Universes"), true)) {
				UniverseName.Value = folder.Name;
				break;
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
