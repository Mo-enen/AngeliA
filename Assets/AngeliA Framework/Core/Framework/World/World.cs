using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


namespace AngeliaFramework.World {
	public class World {




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


		#endregion




		#region --- VAR ---


		// Api
		public Block[,,] Blocks { get; set; } = null;
		public Entity[,,] Entities { get; set; } = null;
		public bool ReadyToFill => FillingTask == null || FillingTask.IsCompleted;

		// Data
		private Task FillingTask = null;


		#endregion




		#region --- API ---


		public World (int size) {
			Blocks = new Block[size, size, Const.BLOCK_LAYER_COUNT];
			Entities = new Entity[size, size, Const.ENTITY_LAYER_COUNT];
		}


		public async void Fill (Map source) {

			if (!ReadyToFill) { return; }

			FillingTask = Task.Run(() => {
				System.Array.Clear(Blocks, 0, Blocks.Length);
				System.Array.Clear(Entities, 0, Entities.Length);
				foreach (var block in source.Blocks) {
					Blocks[block.X, block.Y, block.Layer].SetValues(
						block.TypeID
					);
				}
				foreach (var entity in source.Entities) {
					Entities[entity.X, entity.Y, entity.Layer].SetValues(
						entity.InstanceID, entity.TypeID
					);
				}
				Resources.UnloadAsset(source);
			}).ContinueWith((_) => FillingTask = null);

			await FillingTask;

		}


		#endregion




		#region --- LGC ---




		#endregion












	}
}
