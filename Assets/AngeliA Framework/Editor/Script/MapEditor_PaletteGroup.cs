using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework.Editor {
	[CreateAssetMenu(fileName = "New Group", menuName = "бя AngeliA/Map Editor Palette Group", order = 99)]
	public class MapEditor_PaletteGroup : ScriptableObject {


		// SUB
		[System.Serializable]
		public class Block {
			public Sprite Sprite;

		}


		[System.Serializable]
		public class Entity {
			public Sprite Icon;
			[TypeEnum(typeof(Entities.Entity))]
			public string TypeFullName;
		}


		// Api
		public bool Opening = false;
		public Block[] Blocks = null;
		public Entity[] Entities = null;


	}
}


#if UNITY_EDITOR
namespace AngeliaFramework.Editor {
	using UnityEngine;
	using UnityEditor;
	[CustomEditor(typeof(MapEditor_PaletteGroup))]
	public class MapEditor_PaletteGroup_Inspector : Editor {
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif
