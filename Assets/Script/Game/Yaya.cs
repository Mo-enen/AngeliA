using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class Yaya : Game {


		// Const
		private static readonly int[] ENTITY_CAPACITY = new int[] { 512, 512, 256, 512, 1024 };

		// Api
		public override string MapRoot => !string.IsNullOrEmpty(_MapRoot) ? _MapRoot : (_MapRoot = Util.CombinePaths(Util.GetRuntimeBuiltRootPath(), "Maps"));
		protected override int EntityLayerCount => YayaConst.ENTITY_LAYER_COUNT;
		protected override int PhysicsLayerCount => YayaConst.PHYSICS_LAYER_COUNT;


		// Data
		private string _MapRoot = null;


		// MSG
		private void Awake () {
			Awake_Misc();
			Awake_Quit();
		}


		private void Awake_Misc () {
			LConst.GetLanguage = (key) => CurrentLanguage ? CurrentLanguage[key] : "";
		}


		private void Awake_Quit () {
			bool willQuit = false;
			Application.wantsToQuit += () => {
#if UNITY_EDITOR
				if (UnityEditor.EditorApplication.isPlaying) { return true; }
#endif
				if (willQuit) {
					return true;
				} else {
					// Show Quit Dialog
					AddEntity(new eDialog(
						2048, LConst.QuitConfirmContent, LConst.LabelQuit, LConst.LabelCancel, "",
						() => {
							willQuit = true;
							PlayerData.SaveToDisk(DataSlot);
							Application.Quit();
						},
						() => { },
						null
					));
					return false;
				}
			};
		}


		// Override
		protected override int GetEntityCapacity (int layer) => ENTITY_CAPACITY[layer.Clamp(0, EntityLayerCount - 1)];
		protected override WorldSquad CreateWorldSquad () => new YayaWorldSquad(MapRoot);


	}
}
