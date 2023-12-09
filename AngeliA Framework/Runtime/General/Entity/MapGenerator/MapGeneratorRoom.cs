using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	[EntityAttribute.MapEditorGroup("MapGenerator")]
	public abstract class MapGeneratorElement : IMapEditorItem { }
	public class MapGenerator_Solid : MapGeneratorElement { }
	public class MapGenerator_Tunnel : MapGeneratorElement { }
	public class MapGenerator_Connector : MapGeneratorElement { }


	public class MapGeneratorRoom {




		#region --- SUB ---


		public class Tunnel {
			public int LocalIndex;
			public int Size;
			public Direction4 Direction;
		}


		#endregion




		#region --- VAR ---


		// Const
		public const int MAX_WIDTH = 256;
		public const int MAX_HEIGHT = 128;
		public static readonly int SOLID_ID = typeof(MapGenerator_Solid).AngeHash();
		public static readonly int TUNNEL_ID = typeof(MapGenerator_Tunnel).AngeHash();
		public static readonly int CONNECTOR_ID = typeof(MapGenerator_Connector).AngeHash();

		// Api
		public int EdgeMinX => ContentMinX - 1;
		public int EdgeMinY => ContentMinY - 1;
		public int EdgeMaxX => ContentMaxX + 1;
		public int EdgeMaxY => ContentMaxY + 1;
		public int EdgeWidth => ContentWidth + 2;
		public int EdgeHeight => ContentHeight + 2;
		public bool Valid { get; init; }
		public int ID { get; init; }
		public int ContentMinX { get; init; }
		public int ContentMinY { get; init; }
		public int ContentMaxX { get; init; }
		public int ContentMaxY { get; init; }
		public int ContentWidth { get; init; }
		public int ContentHeight { get; init; }
		public int[] EdgeLeft { get; init; }
		public int[] EdgeRight { get; init; }
		public int[] EdgeDown { get; init; }
		public int[] EdgeUp { get; init; }
		public int[] Entities { get; init; }
		public int[] Levels { get; init; }
		public int[] Backgrounds { get; init; }
		public Tunnel[] Tunnels { get; init; }

		// Cache
		private static readonly List<(Vector2Int point, Direction4 dir)> EntryPointCache = new();
		private static readonly List<Vector2Int> EndPointCache = new();
		private static readonly List<Tunnel> TunnelsCache = new();
		private static readonly HashSet<Vector2Int> RoomPointCache = new();
		private static readonly Queue<Vector2Int> RoomPointQueue = new();
		private static int DynamicID = int.MinValue + 1;


		#endregion




		#region --- API ---


		public MapGeneratorRoom (IBlockSquad squad, int roomPointX, int roomPointY, int z) {

			Valid = false;
			if (!HasValidRoomAt(squad, roomPointX, roomPointY, z, out int id, out int edgeWidth, out int edgeHeight)) return;
			int width = edgeWidth - 2;
			int height = edgeHeight - 2;

			Valid = true;
			ID = id != int.MinValue ? id : DynamicID++;
			ContentMinX = roomPointX + 1;
			ContentMinY = roomPointY + 1;
			ContentMaxX = roomPointX + 1 + width - 1;
			ContentMaxY = roomPointY + 1 + height - 1;
			ContentWidth = width;
			ContentHeight = height;
			Entities = new int[width * height];
			Levels = new int[width * height];
			Backgrounds = new int[width * height];
			EdgeLeft = new int[edgeHeight];
			EdgeRight = new int[edgeHeight];
			EdgeDown = new int[edgeWidth];
			EdgeUp = new int[edgeWidth];

			// Fill Edges
			for (int x = 0; x < edgeWidth; x++) {
				EdgeDown[x] = squad.GetBlockAt(roomPointX + x, roomPointY, z, BlockType.Entity);
				EdgeUp[x] = squad.GetBlockAt(roomPointX + x, roomPointY + edgeHeight - 1, z, BlockType.Entity);
			}
			for (int y = 0; y < edgeHeight; y++) {
				EdgeLeft[y] = squad.GetBlockAt(roomPointX, roomPointY + y, z, BlockType.Entity);
				EdgeRight[y] = squad.GetBlockAt(roomPointX + edgeWidth - 1, roomPointY + y, z, BlockType.Entity);
			}

			// Fill Content
			int index = 0;
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					int _x = roomPointX + x + 1;
					int _y = roomPointY + y + 1;
					Entities[index] = squad.GetBlockAt(_x, _y, z, BlockType.Entity);
					Levels[index] = squad.GetBlockAt(_x, _y, z, BlockType.Level);
					Backgrounds[index] = squad.GetBlockAt(_x, _y, z, BlockType.Background);
					index++;
				}
			}

			// Tunnel
			System.Threading.Monitor.Enter(TunnelsCache);
			try {
				TunnelsCache.Clear();
				FillCache(EdgeLeft, Direction4.Left);
				FillCache(EdgeRight, Direction4.Right);
				FillCache(EdgeDown, Direction4.Down);
				FillCache(EdgeUp, Direction4.Up);
				Tunnels = TunnelsCache.ToArray();
				static void FillCache (int[] _edge, Direction4 _dir) {
					int currentSize = 0;
					for (int i = 0; i <= _edge.Length; i++) {
						if (i == _edge.Length || _edge[i] == SOLID_ID) {
							if (currentSize > 0) {
								TunnelsCache.Add(new Tunnel() {
									LocalIndex = i - currentSize,
									Size = currentSize,
									Direction = _dir,
								});
							}
							currentSize = 0;
						} else {
							currentSize++;
						}
					}
				}
			} finally {
				System.Threading.Monitor.Exit(TunnelsCache);
			}
		}


		public static bool HasValidRoomAt (IBlockSquad squad, int roomPointX, int roomPointY, int z, out int id, out int edgeWidth, out int edgeHeight) {

			edgeWidth = default;
			edgeHeight = default;
			id = int.MinValue;

			// ID
			if (squad.ReadSystemNumber(roomPointX, roomPointY - 1, z, Direction4.Right, out int _id)) {
				id = _id;
			}

			// Check Edge Down
			int xMax = roomPointX;
			for (int x = 0; x < MAX_WIDTH; x++) {
				int entityID = squad.GetBlockAt(roomPointX + x, roomPointY, z, BlockType.Entity);
				if (!IsRoomEdgeBlock(entityID)) break;
				xMax = roomPointX + x;
			}
			if (xMax < roomPointX + 2) return false;

			// Check Edge Left
			int yMax = roomPointY;
			for (int y = 0; y < MAX_HEIGHT; y++) {
				int entityID = squad.GetBlockAt(roomPointX, roomPointY + y, z, BlockType.Entity);
				if (!IsRoomEdgeBlock(entityID)) break;
				yMax = roomPointY + y;
			}
			if (yMax < roomPointY + 2) return false;

			// Check Edge Up
			for (int x = roomPointX; x <= xMax; x++) {
				int entityID = squad.GetBlockAt(x, yMax, z, BlockType.Entity);
				if (!IsRoomEdgeBlock(entityID)) return false;
			}

			// Check Edge Right
			for (int y = roomPointY; y <= yMax; y++) {
				int entityID = squad.GetBlockAt(xMax, y, z, BlockType.Entity);
				if (!IsRoomEdgeBlock(entityID)) return false;
			}

			edgeWidth = xMax - roomPointX + 1;
			edgeHeight = yMax - roomPointY + 1;
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
					foreach (var room in FindRooms(squad, point.x, point.y, z)) {
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
			// Func
			static IEnumerable<MapGeneratorRoom> FindRooms (IBlockSquad squad, int roomPointX, int roomPointY, int z) {

				// Current Room
				var currentRoom = new MapGeneratorRoom(squad, roomPointX, roomPointY, z);
				if (!currentRoom.Valid) yield break;

				yield return currentRoom;

				EntryPointCache.Clear();
				EndPointCache.Clear();

				// Squad >> Entry Points
				for (int x = currentRoom.EdgeMinX; x <= currentRoom.EdgeMaxX; x++) {
					if (squad.GetBlockAt(x, currentRoom.EdgeMinY - 1, z, BlockType.Entity) == CONNECTOR_ID) {
						EntryPointCache.Add((new(x, currentRoom.EdgeMinY - 1), Direction4.Down));
					}
					if (squad.GetBlockAt(x, currentRoom.EdgeMaxY + 1, z, BlockType.Entity) == CONNECTOR_ID) {
						EntryPointCache.Add((new(x, currentRoom.EdgeMaxY + 1), Direction4.Up));
					}
				}
				for (int y = currentRoom.EdgeMinY; y <= currentRoom.EdgeMaxY; y++) {
					if (squad.GetBlockAt(currentRoom.EdgeMinX - 1, y, z, BlockType.Entity) == CONNECTOR_ID) {
						EntryPointCache.Add((new(currentRoom.EdgeMinX - 1, y), Direction4.Left));
					}
					if (squad.GetBlockAt(currentRoom.EdgeMaxX + 1, y, z, BlockType.Entity) == CONNECTOR_ID) {
						EntryPointCache.Add((new(currentRoom.EdgeMaxX + 1, y), Direction4.Right));
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
		}


		#endregion




		#region --- LGC ---


		private static bool IsRoomEdgeBlock (int id) => id == SOLID_ID || id == TUNNEL_ID;


		#endregion




	}
}