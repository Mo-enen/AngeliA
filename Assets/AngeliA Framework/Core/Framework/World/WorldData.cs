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
			public int InstanceID;
			public int TypeID;
			public void SetValues (int instanceID, int typeID) {
				TypeID = typeID;
				InstanceID = instanceID;
			}
		}


		public delegate void VoidObjectHandler (Object obj);


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
		public Block[,,] Blocks { get; set; } = null;
		public Entity[,,] Entities { get; set; } = null;
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
			Entities = new Entity[Const.WORLD_MAP_SIZE, Const.WORLD_MAP_SIZE, Const.ENTITY_LAYER_COUNT];
			FilledPosition = new(int.MinValue, int.MinValue);
		}


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
				if (source == null) return;
				if (source.IsProcedure) {
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
					int eDepth = Entities.GetLength(2);
					foreach (var entity in source.Map.Entities) {
						if (
							entity.X < 0 || entity.X >= eWidth ||
							entity.Y < 0 || entity.Y >= eHeight ||
							entity.Layer < 0 || entity.Layer >= eDepth
						) continue;
						Entities[entity.X, entity.Y, entity.Layer].SetValues(
							entity.InstanceID, entity.TypeID
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


		#endregion




	}
}
