using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


namespace AngeliaFramework {
	public class World {




		#region --- SUB ---



		public struct Block {
			public int TypeID;
			public int Tag;
			public bool IsTrigger;
			public void SetValues (int typeID, int tag, bool isTrigger) {
				TypeID = typeID;
				Tag = tag;
				IsTrigger = isTrigger;
			}
		}



		public struct Entity {
			public long InstanceID;
			public int TypeID;
			public void SetValues (long instanceID, int typeID) {
				InstanceID = instanceID;
				TypeID = typeID;
			}
		}


		public delegate void VoidObjectHandler (Object obj);
		public delegate bool BoolHandler ();


		#endregion




		#region --- VAR ---


		// Callback
		public static event VoidObjectHandler OnMapFilled = null;

		// Api
		public RectInt FilledUnitRect => new(
			FilledPosition.x * Const.WORLD_MAP_SIZE,
			FilledPosition.y * Const.WORLD_MAP_SIZE,
			Const.WORLD_MAP_SIZE,
			Const.WORLD_MAP_SIZE
		);
		public Vector2Int FilledPosition { get; private set; } = default;
		public bool IsFilling { get; private set; } = false;
		public Block[] Blocks { get => m_Blocks; set => m_Blocks = value; }
		public Entity[] Entities { get => m_Entities; set => m_Entities = value; }

		// Short
		private bool AsyncReady => FillingTask.IsCompleted && (LoadingRequest == null || LoadingRequest.isDone);


		// Data
		private Block[] m_Blocks = null;
		private Entity[] m_Entities = null;
		private Task FillingTask = Task.CompletedTask;
		private ResourceRequest LoadingRequest = null;


		#endregion




		#region --- API ---


		public World () {
			m_Blocks = new Block[Const.WORLD_MAP_SIZE * Const.WORLD_MAP_SIZE * Const.BLOCK_LAYER_COUNT];
			m_Entities = new Entity[Const.WORLD_MAP_SIZE * Const.WORLD_MAP_SIZE];
			FilledPosition = new(int.MinValue, int.MinValue);
		}


		// Get
		public Block GetBlock (int localX, int localY, int layer) => m_Blocks[
			layer * Const.WORLD_MAP_SIZE * Const.WORLD_MAP_SIZE +
			localY * Const.WORLD_MAP_SIZE + localX
		];


		public Entity GetEntity (int localX, int localY) => m_Entities[
			localY * Const.WORLD_MAP_SIZE + localX
		];


		// Fill
		public void FillAsync (Vector2Int pos) {
			if (!AsyncReady) return;
			FilledPosition = pos;
			LoadingRequest = Resources.LoadAsync<MapObject>($"Map/{pos.x}_{pos.y}");
			LoadingRequest.completed += (_) => FillAsync(LoadingRequest.asset as MapObject, pos);
		}


		public async void FillAsync (MapObject source, Vector2Int pos) {
			if (!AsyncReady) return;
			FilledPosition = pos;
			FillingTask = Task.Run(() => Fill(source, pos));
			await FillingTask;
		}


		public bool Fill (Vector2Int pos) => Fill(Resources.Load<MapObject>($"Map/{pos.x}_{pos.y}"), pos);


		public bool Fill (Vector2Int pos, out MapObject map) => Fill(map = Resources.Load<MapObject>($"Map/{pos.x}_{pos.y}"), pos);


		public bool Fill (MapObject source, Vector2Int pos) {
			IsFilling = true;
			bool success = false;
			try {
				System.Array.Clear(m_Blocks, 0, m_Blocks.Length);
				System.Array.Clear(m_Entities, 0, m_Entities.Length);
				FilledPosition = pos;
				if (source == null) {
					IsFilling = false;
					return false;
				}
				if (source.IsProcedure) {
					// Procedure
					source.CreateProcedureGenerator().FillWorld(this, pos);
				} else {
					// Static
					// Blocks
					int bWidth = Const.WORLD_MAP_SIZE;
					int bHeight = Const.WORLD_MAP_SIZE;
					int bDepth = Const.BLOCK_LAYER_COUNT;
					foreach (var block in source.Map.Blocks) {
						if (
							block.X < 0 || block.X >= bWidth ||
							block.Y < 0 || block.Y >= bHeight ||
							block.Layer < 0 || block.Layer >= bDepth
						) continue;
						m_Blocks[block.Layer * bWidth * bHeight + block.Y * bWidth + block.X].SetValues(
							block.TypeID, block.Tag, block.IsTrigger
						);
					}
					// Entities
					int eWidth = Const.WORLD_MAP_SIZE;
					int eHeight = Const.WORLD_MAP_SIZE;
					for (int i = 0; i < source.Map.Entities.Length; i++) {
						var entity = source.Map.Entities[i];
						if (
							entity.X < 0 || entity.X >= eWidth ||
							entity.Y < 0 || entity.Y >= eHeight
						) continue;
						m_Entities[entity.Y * eWidth + entity.X].SetValues(
							AUtil.GetEntityInstanceID(pos.x, pos.y, i),
							entity.TypeID
						);
					}
				}
				success = true;
			} catch (System.Exception ex) {
#if UNITY_EDITOR
				Debug.LogException(ex);
#endif
			}
			IsFilling = false;
			OnMapFilled?.Invoke(source);
			return success;
		}


#if UNITY_EDITOR
		public void EditorOnly_SaveToDisk (MapObject mapObject, bool overrideData = true) {
			if (IsFilling || mapObject == null) return;
			const int SIZE = Const.WORLD_MAP_SIZE;
			// Blocks
			var blocks = new List<Map.Block>();
			if (!overrideData) {
				blocks.AddRange(mapObject.Map.Blocks);
			}
			for (int layer = 0; layer < Const.BLOCK_LAYER_COUNT; layer++) {
				for (int y = 0; y < SIZE; y++) {
					for (int x = 0; x < SIZE; x++) {
						var block = m_Blocks[layer * SIZE * SIZE + y * SIZE + x];
						if (block.TypeID == 0) continue;
						blocks.Add(new(
							block.TypeID, x, y, layer, block.Tag, block.IsTrigger
						));
					}
				}
			}
			mapObject.Map.Blocks = blocks.ToArray();
			// Entities
			var entities = new List<Map.Entity>();
			if (!overrideData) {
				entities.AddRange(mapObject.Map.Entities);
			}
			for (int y = 0; y < SIZE; y++) {
				for (int x = 0; x < SIZE; x++) {
					var entity = m_Entities[y * SIZE + x];
					if (entity.TypeID == 0) continue;
					entities.Add(new(entity.TypeID, x, y));
				}
			}
			mapObject.Map.Entities = entities.ToArray();
		}
#endif


		#endregion




	}
}
