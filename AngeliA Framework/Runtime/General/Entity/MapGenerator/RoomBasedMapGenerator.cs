using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class RoomBasedMapGenerator : ActionMapGenerator {




		#region --- VAR ---


		// Data
		protected readonly Dictionary<int, Room> RoomPool = new();
		


		#endregion




		#region --- MSG ---


		protected override void BeforeMapGenerate () {
			base.BeforeMapGenerate();
			// Get Rooms
			RoomPool.Clear();
			foreach (var room in Room.ForAllConnectedRooms(SampleReader, X.ToUnit(), Y.ToUnit(), Stage.ViewZ - 1)) {
				RoomPool.TryAdd(room.ID, room);
			}
			// Clear Reader
			SampleReader.Clear();
			System.GC.Collect();
		}


		#endregion




		#region --- API ---



		#endregion




		#region --- LGC ---



		#endregion




	}
}