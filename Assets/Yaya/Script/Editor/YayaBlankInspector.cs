using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using AngeliaFramework;
using Moenen.Standard;


namespace Yaya.Editor {
	public class YayaBlankInspector : BlankInspector.IBlankInspector, IInitialize {
		public string Label => "";
		public int Order => 0;
		public bool AvailableOnEdittime => false;
		public bool AvailableOnRuntime => false;
		private static Game Game => _Game != null ? _Game : (_Game = Object.FindObjectOfType<Game>());
		private static Game _Game = null;
		private static readonly EditorSavingBool SpawnPlayerAtStart = new("EntityDebuger.SpawnPlayerAtStart", false);


		public void OnInspectorGUI () { }


		public void AddItemsToMenu (GenericMenu menu) {
			menu.AddItem(new GUIContent("Spawn Player At Start"), SpawnPlayerAtStart.Value, () => {
				SpawnPlayerAtStart.Value = !SpawnPlayerAtStart.Value;
			});
		}


		public static void Initialize () {
			if (SpawnPlayerAtStart.Value && Game != null && Game.FirstEntityOfType(typeof(eYaya).AngeHash()) == null) {
				Game.AddEntity(
					typeof(eYaya).AngeHash(),
					Game.ViewRect.CenterInt().x,
					Game.ViewRect.CenterInt().y
				);
			}
		}


	}
}
