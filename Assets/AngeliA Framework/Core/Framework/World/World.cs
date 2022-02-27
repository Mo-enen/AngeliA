using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace AngeliaFramework {
	public class World {




		#region --- SUB ---


		public struct BackgroundBlock {
			public int TypeID;
			public void SetValues (int typeID) {
				TypeID = typeID;
			}
		}


		public struct Block {
			public int TypeID;
			public int Tag;
			public bool IsTrigger;
			public Int4 ColliderBorder;
			public void SetValues (int typeID, int tag, bool isTrigger, Int4 border) {
				TypeID = typeID;
				Tag = tag;
				IsTrigger = isTrigger;
				ColliderBorder = border;
			}
		}


		public struct Entity {
			public int InstanceID;
			public int TypeID;
			public void SetValues (int instanceID, int typeID) {
				InstanceID = instanceID;
				TypeID = typeID;
			}
		}


		public delegate WorldGenerator WorldGeneratorIntHandler (int id);


		#endregion




		#region --- VAR ---


		// Handler
		public static WorldGeneratorIntHandler CreateGenerator { get; set; } = null;

		// Api
		public RectInt WorldUnitRect => new(
			WorldPosition.x * Const.WORLD_MAP_SIZE,
			WorldPosition.y * Const.WORLD_MAP_SIZE,
			Const.WORLD_MAP_SIZE,
			Const.WORLD_MAP_SIZE
		);
		public Vector2Int WorldPosition { get; private set; } = default;
		public BackgroundBlock[] Background { get => m_Background; set => m_Background = value; }
		public Block[] Level { get => m_Level; set => m_Level = value; }
		public Entity[] Entities { get => m_Entities; set => m_Entities = value; }

		// Short
		private string MapRoot => !string.IsNullOrEmpty(_MapRoot) ? _MapRoot : (_MapRoot = AUtil.GetMapRoot());

		// Data
		private BackgroundBlock[] m_Background = null;
		private Block[] m_Level = null;
		private Entity[] m_Entities = null;
		private string _MapRoot = null;


		#endregion




		#region --- API ---


		public World () {
			m_Level = new Block[Const.WORLD_MAP_SIZE * Const.WORLD_MAP_SIZE];
			m_Background = new BackgroundBlock[Const.WORLD_MAP_SIZE * Const.WORLD_MAP_SIZE];
			m_Entities = new Entity[Const.WORLD_MAP_SIZE * Const.WORLD_MAP_SIZE];
			WorldPosition = new(int.MinValue, int.MinValue);
		}


		// Get
		public Block GetLevelBlock (int localX, int localY) => m_Level[localY * Const.WORLD_MAP_SIZE + localX
		];


		public BackgroundBlock GetBackgroundBlock (int localX, int localY) => m_Background[localY * Const.WORLD_MAP_SIZE + localX];


		public Entity GetEntity (int localX, int localY) => m_Entities[
			localY * Const.WORLD_MAP_SIZE + localX
		];


		// Load
		public bool LoadFromDisk (int worldX, int worldY) {
			bool success = false;
			try {
				System.Array.Clear(m_Level, 0, m_Level.Length);
				System.Array.Clear(m_Background, 0, m_Background.Length);
				System.Array.Clear(m_Entities, 0, m_Entities.Length);
				WorldPosition = new(worldX, worldY);

				string path = Util.CombinePaths(MapRoot, $"{worldX}_{worldY}.{Const.MAP_FILE_EXT}");
				if (!Util.FileExists(path)) return false;

				using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
				using var reader = new BinaryReader(stream, System.Text.Encoding.ASCII);

				int generatorID = reader.ReadInt32();
				if (generatorID != 0) {
					// Procedure
					CreateGenerator(generatorID)?.FillWorld(this);
				} else {
					// Static
					int SIZE = Const.WORLD_MAP_SIZE;
					while (reader.NotEnd()) {
						int id = reader.ReadInt32();
						int x = reader.ReadInt32();
						int y = reader.ReadInt32();
						if (x >= 0) {
							// Entity +x +y
							int instanceID = reader.ReadInt32();
							if (x < 0 || x >= SIZE || y < 0 || y >= SIZE) continue;
							m_Entities[y * SIZE + x].SetValues(instanceID, id);
						} else {
							if (y >= 0) {
								// Level -x +y
								x = -x - 1;
								int tag = reader.ReadInt32();
								bool isTrigger = reader.ReadBoolean();
								int borderL = reader.ReadInt32();
								int borderR = reader.ReadInt32();
								int borderD = reader.ReadInt32();
								int borderU = reader.ReadInt32();
								if (x < 0 || x >= SIZE || y < 0 || y >= SIZE) continue;
								m_Level[y * SIZE + x].SetValues(
									id, tag, isTrigger, new Int4() { Left = borderL, Right = borderR, Down = borderD, Up = borderU, }
								);
							} else {
								// Background -x -y
								x = -x - 1;
								y = -y - 1;
								if (x < 0 || x >= SIZE || y < 0 || y >= SIZE) continue;
								m_Background[y * SIZE + x].SetValues(id);
							}
						}
					}
				}
				success = true;
			} catch (System.Exception ex) {
#if UNITY_EDITOR
				Debug.LogException(ex);
#endif
			}
			return success;
		}



#if UNITY_EDITOR
		public int EditorOnly_SaveToDisk (int worldX, int worldY, int instanceID) {

			const int SIZE = Const.WORLD_MAP_SIZE;
			string path = Util.CombinePaths(MapRoot, $"{worldX}_{worldY}.{Const.MAP_FILE_EXT}");
			using var stream = new FileStream(path, FileMode.Append, FileAccess.Write);
			using var writer = new BinaryWriter(stream, System.Text.Encoding.ASCII);
			if (stream.Position == 0) {
				writer.Write((int)0);
			}

			// Level
			for (int y = 0; y < SIZE; y++) {
				for (int x = 0; x < SIZE; x++) {
					var block = m_Level[y * SIZE + x];
					if (block.TypeID == 0) continue;
					writer.Write((int)block.TypeID);
					writer.Write((int)-x - 1);
					writer.Write((int)y);
					writer.Write((int)block.Tag);
					writer.Write((bool)block.IsTrigger);
					writer.Write((int)block.ColliderBorder.Left);
					writer.Write((int)block.ColliderBorder.Right);
					writer.Write((int)block.ColliderBorder.Down);
					writer.Write((int)block.ColliderBorder.Up);
				}
			}

			// Background
			for (int y = 0; y < SIZE; y++) {
				for (int x = 0; x < SIZE; x++) {
					var block = m_Background[y * SIZE + x];
					if (block.TypeID == 0) continue;
					writer.Write((int)block.TypeID);
					writer.Write((int)-x - 1);
					writer.Write((int)-y - 1);
				}
			}

			// Entity
			for (int y = 0; y < SIZE; y++) {
				for (int x = 0; x < SIZE; x++) {
					var entity = m_Entities[y * SIZE + x];
					if (entity.TypeID == 0) continue;
					int insID = instanceID;
					instanceID++;
					writer.Write((int)entity.TypeID);
					writer.Write((int)x);
					writer.Write((int)y);
					writer.Write((int)insID);
				}
			}

			return instanceID;
		}
#endif


		#endregion



	}
}
