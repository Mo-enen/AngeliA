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
			public const int MAX_WIDTH = 256;
			public const int MAX_HEIGHT = 128;
			public static readonly int ROOM_ID = MapGenerator_Room.TYPE_ID;
			public static readonly int DOOR_ID = MapGenerator_Door.TYPE_ID;
			public static readonly int CONNECTOR_ID = MapGenerator_Connector.TYPE_ID;
			public int ID;
			public int Width;
			public int Height;
			public int[] Entities;
			public int[] Levels;
			public int[] Backgrounds;
			public Room (WorldStream stream, int unitX, int unitY, int z) {
				// Get Width
				for (int x = 0; x < MAX_WIDTH; x++) {
					int entityID = stream.GetBlockAt(unitX + x, unitY, z, BlockType.Entity);
					if (entityID != ROOM_ID && entityID != DOOR_ID) {
						Width = x;
						break;
					}
				}
				// Get Height
				for (int y = 0; y < MAX_HEIGHT; y++) {
					int entityID = stream.GetBlockAt(unitX, unitY + y, z, BlockType.Entity);
					if (entityID != ROOM_ID && entityID != DOOR_ID) {
						Height = y;
						break;
					}
				}
				// Get ID



			}
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


		private bool DetectRoomAt (int unitX, int unitY) {


			return false;
		}


		#endregion




	}
}