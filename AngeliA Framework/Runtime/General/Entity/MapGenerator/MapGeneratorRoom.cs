using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	[EntityAttribute.MapEditorGroup("MapGenerator")]
	public abstract class MapGeneratorElement : IMapEditorItem { }
	public class MapGenerator_Wall : MapGeneratorElement { }
	public class MapGenerator_Door : MapGeneratorElement { }
	public class MapGenerator_Connector : MapGeneratorElement { }


	public class MapGeneratorRoom {




		#region --- VAR ---


		// Const
		public const int MAX_WIDTH = 256;
		public const int MAX_HEIGHT = 128;
		public static readonly int WALL_ID = typeof(MapGenerator_Wall).AngeHash();
		public static readonly int DOOR_ID = typeof(MapGenerator_Door).AngeHash();
		public static readonly int CONNECTOR_ID = typeof(MapGenerator_Connector).AngeHash();

		// Api
		public int WallMinX => ContentMinX - 1;
		public int WallMinY => ContentMinY - 1;
		public int WallMaxX => ContentMaxX + 1;
		public int WallMaxY => ContentMaxY + 1;
		public int WallWidth => ContentWidth + 2;
		public int WallHeight => ContentHeight + 2;
		public int ID;
		public int ContentMinX;
		public int ContentMinY;
		public int ContentMaxX;
		public int ContentMaxY;
		public int ContentWidth;
		public int ContentHeight;
		public int[] WallLeft = new int[0];
		public int[] WallRight = new int[0];
		public int[] WallDown = new int[0];
		public int[] WallUp = new int[0];
		public int[] Entities = new int[0];
		public int[] Levels = new int[0];
		public int[] Backgrounds = new int[0];

		// Cache
		private static readonly List<(Vector2Int point, Direction4 dir)> EntryPointCache = new();
		private static readonly List<Vector2Int> EndPointCache = new();
		private static readonly HashSet<Vector2Int> RoomPointCache = new();
		private static readonly Queue<Vector2Int> RoomPointQueue = new();


		#endregion




		#region --- API ---


		public static bool ValidRoom (IBlockSquad squad, int unitX, int unitY, int z, out int id, out int wallWidth, out int wallHeight) {

			wallWidth = default;
			wallHeight = default;

			// Check ID
			if (!squad.ReadSystemNumber(unitX, unitY - 1, z, Direction4.Right, out id)) return false;

			// Check Wall Down
			int xMax = unitX;
			for (int x = 0; x < MAX_WIDTH; x++) {
				int entityID = squad.GetBlockAt(unitX + x, unitY, z, BlockType.Entity);
				if (!IsRoomEdgeBlock(entityID)) break;
				xMax = unitX + x;
			}
			if (xMax < unitX + 2) return false;

			// Check Wall Left
			int yMax = unitY;
			for (int y = 0; y < MAX_HEIGHT; y++) {
				int entityID = squad.GetBlockAt(unitX, unitY + y, z, BlockType.Entity);
				if (!IsRoomEdgeBlock(entityID)) break;
				yMax = unitY + y;
			}
			if (yMax < unitY + 2) return false;

			// Check Wall Up
			for (int x = unitX; x <= xMax; x++) {
				int entityID = squad.GetBlockAt(x, yMax, z, BlockType.Entity);
				if (!IsRoomEdgeBlock(entityID)) return false;
			}

			// Check Wall Right
			for (int y = unitY; y <= yMax; y++) {
				int entityID = squad.GetBlockAt(xMax, y, z, BlockType.Entity);
				if (!IsRoomEdgeBlock(entityID)) return false;
			}

			wallWidth = xMax - unitX + 1;
			wallHeight = yMax - unitY + 1;
			return true;
		}


		public static bool TryLoadRoom (IBlockSquad squad, int unitX, int unitY, int z, out MapGeneratorRoom room) {

			room = null;
			if (!ValidRoom(squad, unitX, unitY, z, out int id, out int wallWidth, out int wallHeight)) return false;
			int width = wallWidth - 2;
			int height = wallHeight - 2;

			room = new MapGeneratorRoom {
				ID = id,
				ContentMinX = unitX + 1,
				ContentMinY = unitY + 1,
				ContentMaxX = unitX + 1 + width - 1,
				ContentMaxY = unitY + 1 + height - 1,
				ContentWidth = width,
				ContentHeight = height,
				Entities = new int[width * height],
				Levels = new int[width * height],
				Backgrounds = new int[width * height],
				WallLeft = new int[wallHeight],
				WallRight = new int[wallHeight],
				WallDown = new int[wallWidth],
				WallUp = new int[wallWidth],
			};

			// Fill Walls
			for (int x = 0; x < wallWidth; x++) {
				room.WallDown[x] = squad.GetBlockAt(unitX + x, unitY, z, BlockType.Entity);
				room.WallUp[x] = squad.GetBlockAt(unitX + x, unitY + wallHeight - 1, z, BlockType.Entity);
			}
			for (int y = 0; y < wallHeight; y++) {
				room.WallLeft[y] = squad.GetBlockAt(unitX, unitY + y, z, BlockType.Entity);
				room.WallRight[y] = squad.GetBlockAt(unitX + wallWidth - 1, unitY + y, z, BlockType.Entity);
			}

			// Fill Content
			int index = 0;
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					int _x = unitX + x + 1;
					int _y = unitY + y + 1;
					room.Entities[index] = squad.GetBlockAt(_x, _y, z, BlockType.Entity);
					room.Levels[index] = squad.GetBlockAt(_x, _y, z, BlockType.Level);
					room.Backgrounds[index] = squad.GetBlockAt(_x, _y, z, BlockType.Background);
					index++;
				}
			}

			return true;
		}


		public static bool SearchRoom (IBlockSquad squad, int unitX, int unitY, int z, out Vector2Int roomPoint) {

			roomPoint = default;
			bool roomCaught = false;

			// Left
			for (int x = 0; x < MAX_WIDTH; x++) {
				int entityID = squad.GetBlockAt(unitX - x, unitY, z, BlockType.Entity);
				if (IsRoomEdgeBlock(entityID)) {
					roomPoint.x = unitX - x;
					roomCaught = true;
					break;
				}
			}
			if (!roomCaught) return false;

			// Down
			roomCaught = false;
			for (int y = 0; y < MAX_HEIGHT; y++) {
				int entityID = squad.GetBlockAt(roomPoint.x, unitY - y, z, BlockType.Entity);
				if (!IsRoomEdgeBlock(entityID)) {
					roomCaught = true;
					break;
				}
				roomPoint.y = unitY - y;
			}

			return roomCaught;
		}


		public static IEnumerable<MapGeneratorRoom> ForAllConnectedRooms (IBlockSquad squad, int unitX, int unitY, int z) {
			System.Threading.Monitor.Enter(RoomPointCache);
			System.Threading.Monitor.Enter(EntryPointCache);
			System.Threading.Monitor.Enter(EndPointCache);
			System.Threading.Monitor.Enter(RoomPointQueue);
			if (!SearchRoom(squad, unitX, unitY, z, out var rootPoint)) yield break;
			try {
				RoomPointCache.Clear();
				RoomPointQueue.Clear();
				RoomPointCache.Add(rootPoint);
				RoomPointQueue.Enqueue(rootPoint);
				for (int safe = 0; RoomPointQueue.Count > 0 && safe < 2048; safe++) {
					var point = RoomPointQueue.Dequeue();
					foreach (var room in ForAllConnectedRoomsLogic(squad, point.x, point.y, z)) {
						yield return room;
					}
				}
			} finally {
				RoomPointCache.Clear();
				EntryPointCache.Clear();
				EndPointCache.Clear();
				System.Threading.Monitor.Exit(RoomPointCache);
				System.Threading.Monitor.Exit(EntryPointCache);
				System.Threading.Monitor.Exit(EndPointCache);
				System.Threading.Monitor.Exit(RoomPointQueue);
			}
		}


		#endregion




		#region --- LGC ---


		private static bool IsRoomEdgeBlock (int id) => id == WALL_ID || id == DOOR_ID;


		private static IEnumerable<MapGeneratorRoom> ForAllConnectedRoomsLogic (IBlockSquad squad, int roomPointX, int roomPointY, int z) {

			// Current Room
			if (!TryLoadRoom(squad, roomPointX, roomPointY, z, out var currentRoom)) yield break;

			yield return currentRoom;

			EntryPointCache.Clear();
			EndPointCache.Clear();

			// Squad >> Entry Points
			for (int x = currentRoom.WallMinX; x <= currentRoom.WallMaxX; x++) {
				if (squad.GetBlockAt(x, currentRoom.WallMinY - 1, z, BlockType.Entity) == CONNECTOR_ID) {
					EntryPointCache.Add((new(x, currentRoom.WallMinY - 1), Direction4.Down));
				}
				if (squad.GetBlockAt(x, currentRoom.WallMaxY + 1, z, BlockType.Entity) == CONNECTOR_ID) {
					EntryPointCache.Add((new(x, currentRoom.WallMaxY + 1), Direction4.Up));
				}
			}
			for (int y = currentRoom.WallMinY; y <= currentRoom.WallMaxY; y++) {
				if (squad.GetBlockAt(currentRoom.WallMinX - 1, y, z, BlockType.Entity) == CONNECTOR_ID) {
					EntryPointCache.Add((new(currentRoom.WallMinX - 1, y), Direction4.Left));
				}
				if (squad.GetBlockAt(currentRoom.WallMaxX + 1, y, z, BlockType.Entity) == CONNECTOR_ID) {
					EntryPointCache.Add((new(currentRoom.WallMaxX + 1, y), Direction4.Right));
				}
			}

			// Entry Points >> End Points
			foreach (var (entryPoint, direction) in EntryPointCache) {
				var currentPos = entryPoint;
				var currentDir = direction;
				var currentNormal = direction.Normal();
				const int MAX_STEP = 1024;
				for (int step = 0; step < MAX_STEP; step++) {
					// Try Forward
					var pos = currentPos + currentNormal;
					int entityID = squad.GetBlockAt(pos.x, pos.y, z, BlockType.Entity);
					if (entityID == CONNECTOR_ID) {
						currentPos = pos;
						continue;
					}
					// Try Turn Left
					currentDir = currentDir.AntiClockwise();
					currentNormal = direction.Normal();
					pos = currentPos + currentNormal;
					entityID = squad.GetBlockAt(pos.x, pos.y, z, BlockType.Entity);
					if (entityID == CONNECTOR_ID) {
						currentPos = pos;
						continue;
					}
					// Try Turn Right
					currentDir = currentDir.Opposite();
					currentNormal = direction.Normal();
					pos = currentPos + currentNormal;
					entityID = squad.GetBlockAt(pos.x, pos.y, z, BlockType.Entity);
					if (entityID == CONNECTOR_ID) {
						currentPos = pos;
						continue;
					}
					// Jump Out
					if (IsRoomEdgeBlock(entityID)) {
						EndPointCache.Add(pos + currentNormal);
					}
				}
			}

			// End Points >> Room Points
			foreach (var endPoint in EndPointCache) {
				if (!SearchRoom(squad, endPoint.x, endPoint.y, z, out var _rootPoint)) continue;
				if (RoomPointCache.Contains(_rootPoint)) continue;
				RoomPointCache.Add(_rootPoint);
				RoomPointQueue.Enqueue(_rootPoint);
			}

			EntryPointCache.Clear();
			EndPointCache.Clear();

		}


		#endregion




	}
}