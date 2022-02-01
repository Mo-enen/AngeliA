using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


namespace AngeliaFramework.World {
	public class WorldData {




		#region --- SUB ---


		public struct Block {
			public int TypeID;
			public void SetValues (int typeID) {
				TypeID = typeID;
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
		public Block[,,] Blocks { get; set; } = null;
		public Entity[,,] Entities { get; set; } = null;
		public Vector2Int FilledPosition { get; private set; } = default;

		// Short
		private bool AsyncReady => FillingTask.IsCompleted && (LoadingRequest == null || LoadingRequest.isDone);

		// Data
		private Task FillingTask = Task.CompletedTask;
		private ResourceRequest LoadingRequest = null;


		#endregion




		#region --- API ---


		public WorldData (int size) {
			Blocks = new Block[size, size, Const.BLOCK_LAYER_COUNT];
			Entities = new Entity[size, size, Const.ENTITY_LAYER_COUNT];
			FilledPosition = new(int.MinValue, int.MinValue);
		}


		public void FillAsync (Vector2Int pos) {
			if (FilledPosition == pos || !AsyncReady) return;
			LoadingRequest = Resources.LoadAsync<Map>($"Map/{pos.x}_{pos.y}");
			LoadingRequest.completed += (_) => {
				if (LoadingRequest.asset != null) {
					FillAsync(LoadingRequest.asset as Map, pos);
				}
			};
		}


		public async void FillAsync (Map source, Vector2Int pos) {
			if (FilledPosition == pos || !AsyncReady) return;
			FillingTask = Task.Run(() => Fill(source, pos));
			await FillingTask;
		}


		public void Fill (Vector2Int pos) {
			if (FilledPosition == pos) return;
			Fill(Resources.Load<Map>($"Map/{pos.x}_{pos.y}"), pos);
		}


		public void Fill (Map source, Vector2Int pos) {
			if (FilledPosition == pos) return;
			System.Array.Clear(Blocks, 0, Blocks.Length);
			System.Array.Clear(Entities, 0, Entities.Length);
			if (source == null) return;
			// Blocks
			int bWidth = Blocks.GetLength(0);
			int bHeight = Blocks.GetLength(1);
			int bDepth = Blocks.GetLength(2);
			foreach (var block in source.Blocks) {
				if (
					block.X < 0 || block.X >= bWidth ||
					block.Y < 0 || block.Y >= bHeight ||
					block.Layer < 0 || block.Layer >= bDepth
				) continue;
				Blocks[block.X, block.Y, block.Layer].SetValues(
					block.TypeID
				);
			}
			// Entities
			int eWidth = Entities.GetLength(0);
			int eHeight = Entities.GetLength(1);
			int eDepth = Entities.GetLength(2);
			foreach (var entity in source.Entities) {
				if (
					entity.X < 0 || entity.X >= eWidth ||
					entity.Y < 0 || entity.Y >= eHeight ||
					entity.Layer < 0 || entity.Layer >= eDepth
				) continue;
				Entities[entity.X, entity.Y, entity.Layer].SetValues(
					entity.InstanceID, entity.TypeID
				);
			}
			FilledPosition = pos;
			OnMapFilled(source);
		}


		#endregion




	}
}
