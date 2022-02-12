using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {



	[CreateAssetMenu(fileName = "New Map", menuName = "бя AngeliA/Map", order = 99)]
	[PreferBinarySerialization]
	public class MapObject : ScriptableObject {

		public bool IsProcedure => !string.IsNullOrEmpty(GeneratorFullname);

		[TypeEnum(typeof(WorldGenerator))]
		public string GeneratorFullname = "";
		public Map Map = null;

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
			public int Tag;
			public bool IsTrigger;
			public Block (int typeID, int x, int y, int layer, int tag, bool isTrigger) {
				TypeID = typeID;
				X = x;
				Y = y;
				Layer = layer;
				Tag = tag;
				IsTrigger = isTrigger;
			}
		}

		[System.Serializable]
		public struct Entity {
			public int InstanceID;
			[ACodeInt] public int TypeID;
			public int X;
			public int Y;
			public int Layer;
			public Entity (int instanceID, int typeID, int x, int y, int layer) {
				InstanceID = instanceID;
				TypeID = typeID;
				X = x;
				Y = y;
				Layer = layer;
			}
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

		//private void OnEnable () {
		//	foreach (MapObject map in targets) {
		//		var blocks = new List<Map.Block>();
		//		for (int i = 0; i < Const.WORLD_MAP_SIZE; i++) {
		//			blocks.Add(new() {
		//				TypeID = "Test Block".ACode(),
		//				X = 0,
		//				Y = i,
		//				Layer = 1,
		//				IsTrigger = false,
		//				Tag = 0,
		//			});
		//			if (i != 0) {
		//				blocks.Add(new() {
		//					TypeID = "Test Block".ACode(),
		//					X = i,
		//					Y = 0,
		//					Layer = 1,
		//					IsTrigger = false,
		//					Tag = 0,
		//				});
		//			}
		//		}
		//		map.Map.Blocks = blocks.ToArray();
		//		EditorUtility.SetDirty(map);
		//	}
		//	AssetDatabase.SaveAssets();
		//	AssetDatabase.Refresh();
		//}
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif
