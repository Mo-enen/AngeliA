using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BlankInspector;


namespace Yaya.Editor {
	public class EditorGeneral {
		[InitializeOnLoadMethod]
		private static void Init () {
			foreach (var type in typeof(IBlankInspector).AllClassImplemented()) {
				if (type.Name == "AngeliABlankInspector_Usage") {
					// Entity Names
					string names = "";
					for (int i = 0; i < YayaConst.ENTITY_LAYER_COUNT; i++) {
						names += ((EntityLayer)i).ToString() + "\n";
					}
					Util.InvokeStaticMethod(type, "SetEntityNames", new object[] { names });
					break;
				}
			}
		}
	}
}
