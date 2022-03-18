using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AngeliaFramework.Editor;


namespace Yaya.Editor {
	public class EditorGeneral {


		[InitializeOnLoadMethod]
		private static void Init () {

			// Entity Names
			string names = "";
			for (int i = 0; i < YayaConst.ENTITY_LAYER_COUNT; i++) {
				names += ((EntityLayer)i).ToString() + "\n";
			}
			AngeliA_BlankInspector.SetEntityNames(names);

		}



	}
}
