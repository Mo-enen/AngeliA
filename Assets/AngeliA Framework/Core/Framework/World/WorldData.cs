using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


namespace AngeliaFramework {
	public class WorldData {




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
		public static event BoolHandler AllowWorldGenerator = null;

		// Api
		public RectInt FilledUnitRect => new(
			FilledPosition.x * Const.WORLD_MAP_SIZE,
			FilledPosition.y * Const.WORLD_MAP_SIZE,
			Const.WORLD_MAP_SIZE,
			Const.WORLD_MAP_SIZE
		);
		public Block[,,] Blocks { get; set; } = null;
		public Entity[,] Entities { get; set; } = null;
		public Vector2Int FilledPosition { get; private set; } = default;
		public bool IsFilling { get; private set; } = false;

		// Short
		private bool AsyncReady => FillingTask.IsCompleted && (LoadingRequest == null || LoadingRequest.isDone);

		// Data
		private Task FillingTask = Task.CompletedTask;
		private ResourceRequest LoadingRequest = null;


		#endregion




		#region --- API ---


		public WorldData () {
			Blocks = new Block[Const.WORLD_MAP_SIZE, Const.WORLD_MAP_SIZE, Const.BLOCK_LAYER_COUNT];
			Entities = new Entity[Const.WORLD_MAP_SIZE, Const.WORLD_MAP_SIZE];
			FilledPosition = new(int.MinValue, int.MinValue);
		}


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


		public void Fill (Vector2Int pos) => Fill(Resources.Load<MapObject>($"Map/{pos.x}_{pos.y}"), pos);


		public void Fill (MapObject source, Vector2Int pos) {
			IsFilling = true;
			try {
				System.Array.Clear(Blocks, 0, Blocks.Length);
				System.Array.Clear(Entities, 0, Entities.Length);
				FilledPosition = pos;
				if (source == null) {
					IsFilling = false;
					return;
				}
				if (source.IsProcedure && AllowWorldGenerator()) {
					// Procedure
					source.CreateProcedureGenerator().FillWorld(this, pos);
				} else {
					// Static
					// Blocks
					int bWidth = Blocks.GetLength(0);
					int bHeight = Blocks.GetLength(1);
					int bDepth = Blocks.GetLength(2);
					foreach (var block in source.Map.Blocks) {
						if (
							block.X < 0 || block.X >= bWidth ||
							block.Y < 0 || block.Y >= bHeight ||
							block.Layer < 0 || block.Layer >= bDepth
						) continue;
						Blocks[block.X, block.Y, block.Layer].SetValues(
							block.TypeID, block.Tag, block.IsTrigger
						);
					}
					// Entities
					int eWidth = Entities.GetLength(0);
					int eHeight = Entities.GetLength(1);
					for (int i = 0; i < source.Map.Entities.Length; i++) {
						var entity = source.Map.Entities[i];
						if (
							entity.X < 0 || entity.X >= eWidth ||
							entity.Y < 0 || entity.Y >= eHeight
						) continue;
						Entities[entity.X, entity.Y].SetValues(
							AUtil.GetEntityInstanceID(pos.x, pos.y, i),
							entity.TypeID
						);
					}
				}
			} catch (System.Exception ex) {
#if UNITY_EDITOR
				Debug.LogException(ex);
#endif
			}
			IsFilling = false;
			OnMapFilled(source);
		}


#if UNITY_EDITOR
		public void EditorOnly_SaveToDisk (MapObject mapObject) {
			if (IsFilling || mapObject == null) return;
			// Blocks
			var blocks = new List<Map.Block>();
			for (int layer = 0; layer < Const.BLOCK_LAYER_COUNT; layer++) {
				for (int y = 0; y < Const.WORLD_MAP_SIZE; y++) {
					for (int x = 0; x < Const.WORLD_MAP_SIZE; x++) {
						var block = Blocks[x, y, layer];
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
			for (int y = 0; y < Const.WORLD_MAP_SIZE; y++) {
				for (int x = 0; x < Const.WORLD_MAP_SIZE; x++) {
					var entity = Entities[x, y];
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
