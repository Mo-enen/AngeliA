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
		public Map Map = new();


		public WorldGenerator CreateProcedureGenerator () {
			try {
				var generator = System.Activator.CreateInstance(
					System.Type.GetType(GeneratorFullname, false)
				) as WorldGenerator;
				generator.SourceMap = Map;
				return generator;
			} catch { return null; }
		}


		public static Vector2Int? GetPositionFromName (string name) {
			try {
				int dIndex = name.IndexOf('_');
				return new Vector2Int(
					int.Parse(name[..dIndex]),
					int.Parse(name[(dIndex + 1)..])
				);
			} catch (System.Exception ex) { Debug.LogException(ex); }
			return null;
		}


	}



	[System.Serializable]
	public class Map {

		[System.Serializable]
		public struct Block {

			public int TypeID;
			public int X;
			public int Y;
			public int Tag;
			public bool IsTrigger;
			public Int4 ColliderBorder;

			public Block (int typeID, int x, int y, int tag, bool isTrigger, Int4 border) {
				TypeID = typeID;
				X = x;
				Y = y;
				Tag = tag;
				IsTrigger = isTrigger;
				ColliderBorder = border;
			}

		}

		[System.Serializable]
		public struct Entity {

			public int TypeID;
			public long InstanceID;
			public int X;
			public int Y;

			public Entity (int typeID, long instanceID, int x, int y) {
				TypeID = typeID;
				InstanceID = instanceID;
				X = x;
				Y = y;
			}

		}

		public Block[] Level = new Block[0];
		public Block[] Background = new Block[0];
		public Entity[] Entities = new Entity[0];

	}



}


#if UNITY_EDITOR
namespace AngeliaFramework.Editor {
	using UnityEditor;
	[CustomEditor(typeof(MapObject))]
	[CanEditMultipleObjects]
	public class Map_Inspector : Editor {
		/*
		private void OnEnable () {
			foreach (MapObject map in targets) {
				var blocks = new List<Map.Block>();
				for (int i = 0; i < Const.WORLD_MAP_SIZE; i++) {
					// Frame
					blocks.Add(new() {
						TypeID = "Test Block".ACode(),
						X = 0,
						Y = i,
						Layer = 1,
						IsTrigger = false,
						Tag = 0,
					});
					blocks.Add(new() {
						TypeID = "Test Block".ACode(),
						X = Const.WORLD_MAP_SIZE - 1,
						Y = i,
						Layer = 1,
						IsTrigger = false,
						Tag = 0,
					});
					if (i != 0) {
						blocks.Add(new() {
							TypeID = "Test Block".ACode(),
							X = i,
							Y = 0,
							Layer = 1,
							IsTrigger = false,
							Tag = 0,
						});
					}
					if (i != Const.WORLD_MAP_SIZE - 1) {
						blocks.Add(new() {
							TypeID = "Test Block".ACode(),
							X = i,
							Y = Const.WORLD_MAP_SIZE - 1,
							Layer = 1,
							IsTrigger = false,
							Tag = 0,
						});
					}
					// BG
					blocks.Add(new() {
						TypeID = "Test BG".ACode(),
						X = i,
						Y = Random.Range(0, Const.WORLD_MAP_SIZE),
						Layer = 0,
						IsTrigger = false,
						Tag = 0,
					});
				}
				map.Map.Blocks = blocks.ToArray();
				// Entities
				var entities = new List<Map.Entity>();
				for (int i = 0; i < Const.WORLD_MAP_SIZE; i++) {
					entities.Add(new() {
						TypeID = typeof(eBarrel).ACode(),
						X = i,
						Y = 3,
					});
				}
				map.Map.Entities = entities.ToArray();
				EditorUtility.SetDirty(map);
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