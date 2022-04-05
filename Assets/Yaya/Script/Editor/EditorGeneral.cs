using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
			AngeliABlankInspector_Usage.SetEntityNames(names);

		}


		public class YayaScriptHub : IScriptHubRootPath {
			public string Path => !string.IsNullOrEmpty(_Path) ? _Path : (_Path = GetPath());
			private string _Path = "";
			public string IgnoreFolders => "";
			public string IgnoreFiles => "EditorGeneral.cs";
			public string Title => "Yaya";
			public IScriptHubRootPath.FileExtension[] FileExtensions => new IScriptHubRootPath.FileExtension[]{
				new ("cs", "", true),
			};
			public int Order => 0;

			private string GetPath () {
				const string TARGET = "Packages/com.moenengames.yaya";
				if (EditorUtil.ForAllPackages().Any(package => package == TARGET)) return TARGET;
				return "Assets";
			}
		}

		public class YayaArtworkHub : IScriptHubRootPath {
			public string Path => !string.IsNullOrEmpty(_Path) ? _Path : (_Path = GetPath());
			private string _Path = "";
			public string IgnoreFolders => "";
			public string IgnoreFiles => "";
			public string Title => "Yaya Artwork";
			public IScriptHubRootPath.FileExtension[] FileExtensions => new IScriptHubRootPath.FileExtension[]{

				new ("ase", "Ase", true),
				new ("aseprite", "Aseprite", true),
			};
			public int Order => 0 + 1;

			private string GetPath () {
				const string TARGET = "Packages/com.moenengames.yaya";
				if (EditorUtil.ForAllPackages().Any(package => package == TARGET)) return TARGET;
				return "Assets";
			}
		}

	}
}
