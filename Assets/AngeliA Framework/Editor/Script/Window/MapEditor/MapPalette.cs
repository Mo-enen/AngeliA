using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace AngeliaFramework.Editor {
	[CreateAssetMenu(fileName = "New Palette", menuName = "бя AngeliA/Map Palette", order = 99)]
	public class MapPalette : ScriptableObject {

		[System.Serializable]
		public class Block {
			public Sprite Sprite;
		}

		[System.Serializable]
		public class Entity : Block {
			[TypeEnum(typeof(Entities.Entity))]
			public string TypeFullName;
		}

		public int AllCount => Blocks.Length + Entities.Length;
		public Block this[int index] {
			get {
				if (index < Blocks.Length) {
					return Blocks[index];
				} else {
					return Entities[index - Blocks.Length];
				}
			}
		}

		public bool Opening = false;
		public Block[] Blocks = null;
		public Entity[] Entities = null;

	}


	public class MapEditorPalettePost : AssetPostprocessor {
		private static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			if (MapEditor.Main == null) return;
			foreach (var path in importedAssets) {
				if (AssetDatabase.LoadAssetAtPath<MapPalette>(path) != null) {
					MapEditor.Main.SetNeedReloadAsset();
					return;
				}
			}
		}
	}


}


#if UNITY_EDITOR
namespace AngeliaFramework.Editor {
	using UnityEngine;
	using UnityEditor;
	[CustomEditor(typeof(MapPalette))]
	public class MapEditor_PaletteGroup_Inspector : Editor {
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif
