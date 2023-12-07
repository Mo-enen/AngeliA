using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class RoomBasedMapGenerator : ActionMapGenerator {




		#region --- SUB ---


		[EntityAttribute.MapEditorGroup("MapGenerator")]
		public class MapGenerator_Room : IMapEditorItem {
			public static readonly int TYPE_ID = typeof(MapGenerator_Room).AngeHash();
		}

		[EntityAttribute.MapEditorGroup("MapGenerator")]
		public class MapGenerator_Door : IMapEditorItem {
			public static readonly int TYPE_ID = typeof(MapGenerator_Door).AngeHash();
		}

		[EntityAttribute.MapEditorGroup("MapGenerator")]
		public class MapGenerator_Connector : IMapEditorItem {
			public static readonly int TYPE_ID = typeof(MapGenerator_Connector).AngeHash();
		}


		public class Room {
			public int ID;
			public int Width;
			public int Height;
			public int[] Entities;
			public int[] Levels;
			public int[] Backgrounds;
		}


		#endregion




		#region --- VAR ---


		// Data
		protected readonly Dictionary<int, Room> RoomPool = new();


		#endregion




		#region --- MSG ---


		protected override void BeforeMapGenerate () {
			base.BeforeMapGenerate();
			// Get Rooms
			RoomPool.Clear();



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