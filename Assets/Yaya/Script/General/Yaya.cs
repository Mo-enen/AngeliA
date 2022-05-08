using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class Yaya : Game {


		// Const
		private readonly HashSet<SystemLanguage> SupportedLanguages = new() {
			SystemLanguage.English,
			SystemLanguage.ChineseSimplified,
		};

		// Api
		protected override int PhysicsLayerCount => YayaConst.PHYSICS_LAYER_COUNT;

		// Saving
		private readonly SavingInt LanguageIndex = new("Game.LanguageIndex", -1);



		// MSG
		protected override void Initialize () {
			base.Initialize();
			Initialize_Language();
			Initialize_Quit();
		}


		private void Initialize_Language () {

			var targetLanguage = LanguageIndex.Value < 0 ?
				Application.systemLanguage :
				(SystemLanguage)LanguageIndex.Value;

			if (SupportedLanguages.Contains(targetLanguage)) {
				SetLanguage(targetLanguage);
			} else {
				switch (targetLanguage) {
					case SystemLanguage.Chinese:
					case SystemLanguage.ChineseTraditional:
						if (SupportedLanguages.Contains(SystemLanguage.ChineseSimplified)) {
							SetLanguage(SystemLanguage.ChineseSimplified);
						} else {
							SetLanguage(SystemLanguage.English);
						}
						break;
					case SystemLanguage.ChineseSimplified:
						if (SupportedLanguages.Contains(SystemLanguage.ChineseTraditional)) {
							SetLanguage(SystemLanguage.ChineseTraditional);
						} else {
							SetLanguage(SystemLanguage.English);
						}
						break;
					default:
						SetLanguage(SystemLanguage.English);
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


		// Override
		protected override WorldSquad CreateWorldSquad () => new(MapRoot, YayaConst.LEVEL);


		public override void SetLanguage (SystemLanguage language) {
			base.SetLanguage(language);
			LanguageIndex.Value = (int)language;
		}


	}
}
