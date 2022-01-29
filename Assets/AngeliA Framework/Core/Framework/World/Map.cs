using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework.World {
	[CreateAssetMenu(fileName = "New Map", menuName = "бя AngeliA/Map", order = 99)]
	[PreferBinarySerialization]
	public class Map : ScriptableObject {

		[System.Serializable]
		public struct Block {
			public int TypeID;
			public int X;
			public int Y;
			public int Layer;
		}

		[System.Serializable]
		public struct Entity {
			public int InstanceID;
			public int TypeID;
			public int X;
			public int Y;
			public int Layer;
		}

		public Block[] Blocks = new Block[0];
		public Entity[] Entities = new Entity[0];

	}
}


#if UNITY_EDITOR
namespace AngeliaFramework.Editor {
	using UnityEditor;
	using AngeliaFramework.World;
	[CustomEditor(typeof(Map))]
	public class Map_Inspector : Editor {
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif
