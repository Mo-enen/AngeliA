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
		///*
		private void OnEnable () {
			int InstanceID = 0;
			foreach (MapObject m in targets) {
				for (int i = 0; i < m.Map.Blocks.Length; i++) {
					//m.Map.Blocks[i].X %= 128;
					//m.Map.Blocks[i].Y %= 128;
				}
				for (int i = 0; i < m.Map.Entities.Length; i++) {
					m.Map.Entities[i].X = i;
					m.Map.Entities[i].Y = 3;
					m.Map.Entities[i].Layer = (int)EntityLayer.Environment;
					m.Map.Entities[i].TypeID = typeof(eBarrel).FullName.ACode();
					m.Map.Entities[i].InstanceID = InstanceID;
					InstanceID++;
				}
				EditorUtility.SetDirty(m);
			}
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
		//*/
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif
