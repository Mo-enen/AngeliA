using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class Yaya : Game {


		// Const
		private static readonly int[] ENTITY_CAPACITY = new int[] { 512, 512, 256, 512, 1024 };

		// Api
		protected override int PhysicsLayerCount => YayaConst.PHYSICS_LAYER_COUNT;


		// MSG
		protected override void Initialize () {
			base.Initialize();
			Initialize_Misc();
			Initialize_Quit();
		}


		private void Initialize_Misc () {
			YayaConst.GetLanguage = (key) => CurrentLanguage ? CurrentLanguage[key] : "";
		}


		private void Initialize_Quit () {
			bool willQuit = false;
			Application.wantsToQuit += () => {
#if UNITY_EDITOR
				if (UnityEditor.EditorApplication.isPlaying) { return true; }
#endif
				if (willQuit) {
					return true;
				} else {
					// Show Quit Dialog
					var dialog = AddEntity(typeof(eDialog).AngeHash(), 0, 0) as eDialog;
					dialog.Setup(
						2048,
						YayaConst.QuitConfirmContent, YayaConst.LabelQuit, YayaConst.LabelCancel, "",
						() => {
							willQuit = true;
							PlayerData.SaveToDisk();
							Application.Quit();
						},
						() => { },
						null
					);
					return false;
				}
			};
		}


		// Override
		protected override WorldSquad CreateWorldSquad () => new(MapRoot, (int)PhysicsLayer.Level);


	}
}
