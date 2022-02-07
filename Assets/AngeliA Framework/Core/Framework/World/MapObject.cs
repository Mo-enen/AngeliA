using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace AngeliaFramework {



	[CreateAssetMenu(fileName = "New Map", menuName = "бя AngeliA/Map", order = 99)]
	[PreferBinarySerialization]
	public class MapObject : ScriptableObject {

		public bool IsProcedure => !string.IsNullOrEmpty(GeneratorFullname);

		public Map Map = null;
		[TypeEnum(typeof(WorldGenerator))] public string GeneratorFullname = "";

		public WorldGenerator CreateProcedureGenerator () {
			try {
				var generator = System.Activator.CreateInstance(
					System.Type.GetType(GeneratorFullname, false)
				) as WorldGenerator;
				generator.SourceMap = Map;
				return generator;
			} catch { return null; }
		}

	}



	[System.Serializable]
	public class Map {

		[System.Serializable]
		public struct Block {
			[ACodeInt] public int TypeID;
			public int X;
			public int Y;
			public int Layer;
		}

		[System.Serializable]
		public struct Entity {
			public int InstanceID;
			[ACodeInt] public int TypeID;
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
	[CustomEditor(typeof(MapObject))]
	[CanEditMultipleObjects]
	public class Map_Inspector : Editor {
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif
