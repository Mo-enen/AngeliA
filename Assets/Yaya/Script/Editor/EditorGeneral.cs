using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using AngeliaFramework.Editor;
using Moenen.Standard;


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


		public class YayaScriptHub : IScriptHubRootPath {
			public string Path => !string.IsNullOrEmpty(_Path) ? _Path : (_Path = GetPath());
			private string _Path = "";
			public string IgnoreFolders => "";
			public string IgnoreFiles => "";
			public string Title => "Yaya";

			private string GetPath () {
				var req = Client.List(true, false);
				while (!req.IsCompleted) { }
				if (req.Status == StatusCode.Success) {
					foreach (var package in req.Result) {
						if (package.name == "com.moenengames.yaya") {
							return "Packages/com.moenengames.yaya";
						}
					}
				}
				return "Assets";
			}
		}



	}
}
