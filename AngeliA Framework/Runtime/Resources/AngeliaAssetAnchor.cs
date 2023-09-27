#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Audio;


namespace AngeliaFramework {
	public class AngeliaAssetAnchor : ScriptableObject {


		public static AngeliaAssetAnchor Instance {
			get {
				if (_Instance == null) {
					var ins = CreateInstance<AngeliaAssetAnchor>();
					var script = MonoScript.FromScriptableObject(ins);
					string assetPath = Util.ChangeExtension(AssetDatabase.GetAssetPath(script), "asset");
					if (Util.FileExists(assetPath)) {
						// Load
						_Instance = AssetDatabase.LoadAssetAtPath<AngeliaAssetAnchor>(assetPath);
					} else {
						// Create
						AssetDatabase.CreateAsset(ins, assetPath);
						AssetDatabase.Refresh();
						_Instance = ins;
					}
				}
				return _Instance;
			}
		}
		private static AngeliaAssetAnchor _Instance = null;


		public Texture2D[] Cursors;
		public Font[] Fonts;
		public Gradient SkyTop;
		public Gradient SkyBottom;


	}
}
#endif