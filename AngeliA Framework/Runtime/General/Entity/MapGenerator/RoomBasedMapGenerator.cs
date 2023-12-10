using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	public class TestGenerator : RoomBasedMapGenerator {
		protected override void GenerateMap () {

		}
	}


	public abstract class RoomBasedMapGenerator : ActionMapGenerator {




		#region --- SUB ---


		private struct DoorInfo {
			public bool FrontDoor;
		}


		private struct EntryUnit {
			public Vector2Int Point;
			public Direction4 Direction;
			public EntryUnit (Vector2Int point, Direction4 direction) {
				Point = point;
				Direction = direction;
			}
		}


		private struct RoomTarget {
			public Vector2Int Point;
			public RoomNode BaseNode;
			public RoomTarget (Vector2Int point, RoomNode baseNode) {
				Point = point;
				BaseNode = baseNode;
			}
		}


		#endregion




		#region --- VAR ---


		// Const
		public const int MAX_WIDTH = 256;
		public const int MAX_HEIGHT = 128;
		public static readonly int SOLID_ID = typeof(RoomWall).AngeHash();
		public static readonly int TUNNEL_ID = typeof(RoomTunnel).AngeHash();
		public static readonly int CONNECTOR_ID = typeof(RoomConnector).AngeHash();

		// Data
		protected readonly Dictionary<int, Room> RoomPool = new();
		protected RoomNode RootNode = null;

		// Cache
		private static readonly HashSet<Vector2Int> RoomPointCache = new();
		private static readonly List<EntryUnit> EntryPointCache = new();
		private static readonly List<Vector2Int> EndPointCache = new();
		private static readonly Queue<RoomTarget> TargetQueue = new();
		private static readonly List<Room.Tunnel> TunnelsCache = new();
		private static readonly List<Room.Door> DoorsCache = new();
		private static readonly Dictionary<int, DoorInfo> DoorPool = new();
		private static int DynamicID = -1;


		#endregion




		#region --- MSG ---


		[OnGameInitialize]
		public static void OnGameInitialize () {
			foreach (var type in typeof(Door).AllChildClass()) {
				if (System.Activator.CreateInstance(type) is not Door door) continue;
				DoorPool.TryAdd(type.AngeHash(), new DoorInfo() {
					FrontDoor = door.IsFrontDoor,
				});
			}
		}


		protected override void BeforeMapGenerate () {
			base.BeforeMapGenerate();
			RoomPool.Clear();
			try {
				RootNode = ForAllConnectedRooms(
					SampleReader, X.ToUnit(), Y.ToUnit(), Stage.ViewZ - 1,
					(_room) => RoomPool.TryAdd(_room.ID, _room)
				);
				Debug.Log(RootNode.PrintTree());
			} catch (System.Exception ex) { Debug.LogException(ex); }
			SampleReader.Clear();
			System.GC.Collect();
		}


		#endregion




		#region --- API ---


		public static Room LoadRoomAt (IBlockSquad squad, int roomPointX, int roomPointY, int z) {

			if (!HasValidRoomAt(squad, roomPointX, roomPointY, z, out int id, out int edgeWidth, out int edgeHeight)) return null;
			int width = edgeWidth - 2;
			int height = edgeHeight - 2;

			var room = new Room {
				ID = id != int.MinValue ? id : DynamicID--,
				ContentMinX = roomPointX + 1,
				ContentMinY = roomPointY + 1,
				ContentMaxX = roomPointX + 1 + width - 1,
				ContentMaxY = roomPointY + 1 + height - 1,
				ContentWidth = width,
				ContentHeight = height,
				Entities = new int[width * height],
				Levels = new int[width * height],
				Backgrounds = new int[width * height],
				EdgeLeft = new int[edgeHeight],
				EdgeRight = new int[edgeHeight],
				EdgeDown = new int[edgeWidth],
				EdgeUp = new int[edgeWidth],
			};

			// Fill Edges
			for (int x = 0; x < edgeWidth; x++) {
				room.EdgeDown[x] = squad.GetBlockAt(roomPointX + x, roomPointY, z, BlockType.Entity);
				room.EdgeUp[x] = squad.GetBlockAt(roomPointX + x, roomPointY + edgeHeight - 1, z, BlockType.Entity);
			}
			for (int y = 0; y < edgeHeight; y++) {
				room.EdgeLeft[y] = squad.GetBlockAt(roomPointX, roomPointY + y, z, BlockType.Entity);
				room.EdgeRight[y] = squad.GetBlockAt(roomPointX + edgeWidth - 1, roomPointY + y, z, BlockType.Entity);
			}

			// Fill Content
			int index = 0;
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					int _x = roomPointX + x + 1;
					int _y = roomPointY + y + 1;
					room.Entities[index] = squad.GetBlockAt(_x, _y, z, BlockType.Entity);
					room.Levels[index] = squad.GetBlockAt(_x, _y, z, BlockType.Level);
					room.Backgrounds[index] = squad.GetBlockAt(_x, _y, z, BlockType.Background);
					index++;
				}
			}

			// Tunnel
			System.Threading.Monitor.Enter(TunnelsCache);
			try {
				TunnelsCache.Clear();
				FillCache(room.EdgeLeft, Direction4.Left);
				FillCache(room.EdgeRight, Direction4.Right);
				FillCache(room.EdgeDown, Direction4.Down);
				FillCache(room.EdgeUp, Direction4.Up);
				room.Tunnels = TunnelsCache.ToArray();
				static void FillCache (int[] _edge, Direction4 _dir) {
					int currentSize = 0;
					for (int i = 0; i <= _edge.Length; i++) {
						if (i == _edge.Length || _edge[i] == SOLID_ID) {
							if (currentSize > 0) {
								TunnelsCache.Add(new Room.Tunnel() {
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
				TunnelsCache.Clear();
				System.Threading.Monitor.Exit(TunnelsCache);
			}

			// Doors
			System.Threading.Monitor.Enter(DoorsCache);
			try {
				DoorsCache.Clear();
				for (int i = 0; i < room.Entities.Length; i++) {
					int entityID = room.Entities[i];
					if (entityID != 0 && DoorPool.TryGetValue(entityID, out var doorInfo)) {
						DoorsCache.Add(new Room.Door() {
							X = i % width,
							Y = i / width,
							FrontDoor = doorInfo.FrontDoor,
						});
					}
				}
				room.Doors = DoorsCache.ToArray();
			} finally {
				DoorsCache.Clear();
				System.Threading.Monitor.Exit(DoorsCache);
			}

			return room;
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


		public static bool SearchForRoom (IBlockSquad squad, int unitX, int unitY, int z, out Vector2Int roomPoint) {

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


		public static RoomNode ForAllConnectedRooms (IBlockSquad squad, int unitX, int unitY, int z, System.Action<Room> onRoomLoaded = null) {
			System.Threading.Monitor.Enter(RoomPointCache);
			System.Threading.Monitor.Enter(EntryPointCache);
			System.Threading.Monitor.Enter(EndPointCache);
			System.Threading.Monitor.Enter(TargetQueue);
			RoomPointCache.Clear();
			EntryPointCache.Clear();
			EndPointCache.Clear();
			TargetQueue.Clear();
			var rootNode = new RoomNode(null, null);
			try {
				if (SearchForRoom(squad, unitX, unitY, z, out var rootPoint)) {
					const int MAX_ROOM_COUNT = 4096;
					RoomPointCache.Add(rootPoint);
					TargetQueue.Enqueue(new RoomTarget(rootPoint, rootNode));
					for (int roomCount = 0; TargetQueue.Count > 0 && roomCount < MAX_ROOM_COUNT; roomCount++) {
						var target = TargetQueue.Dequeue();
						Iterate(squad, target.Point, z, target.BaseNode, out Room resultRoom);
						if (onRoomLoaded != null && resultRoom != null) {
							onRoomLoaded.Invoke(resultRoom);
						}
					}
					if (rootNode.Children.Count > 0) rootNode = rootNode.Children[0];
				}
			} finally {
				RoomPointCache.Clear();
				EntryPointCache.Clear();
				EndPointCache.Clear();
				TargetQueue.Clear();
				System.Threading.Monitor.Exit(RoomPointCache);
				System.Threading.Monitor.Exit(EntryPointCache);
				System.Threading.Monitor.Exit(EndPointCache);
				System.Threading.Monitor.Exit(TargetQueue);
			}
			return rootNode;
			// Func
			static void Iterate (IBlockSquad squad, Vector2Int roomPoint, int z, RoomNode baseNode, out Room resultRoom) {

				// Current
				resultRoom = LoadRoomAt(squad, roomPoint.x, roomPoint.y, z);
				if (resultRoom == null) return;

				// Meta Room
				if (resultRoom.ContentWidth == 1 && resultRoom.ContentHeight == 1) {
					if (baseNode != null && !baseNode.Meta.ContainsKey(resultRoom.ID)) {
						baseNode.Meta.Add(resultRoom.ID, new RoomMeta(
							resultRoom.ID,
							resultRoom.Entities[0],
							resultRoom.Levels[0],
							resultRoom.Backgrounds[0]
						));
					}
					resultRoom = null;
					return;
				}

				// Content Room
				var currentNode = new RoomNode(resultRoom, baseNode);
				baseNode?.Children.Add(currentNode);

				EntryPointCache.Clear();
				EndPointCache.Clear();

				// Squad >> Entry Points
				for (int x = resultRoom.EdgeMinX; x <= resultRoom.EdgeMaxX; x++) {
					if (squad.GetBlockAt(x, resultRoom.EdgeMinY - 1, z, BlockType.Entity) == CONNECTOR_ID) {
						EntryPointCache.Add(new EntryUnit(
							new(x, resultRoom.EdgeMinY - 1), Direction4.Down
						));
					}
					if (squad.GetBlockAt(x, resultRoom.EdgeMaxY + 1, z, BlockType.Entity) == CONNECTOR_ID) {
						EntryPointCache.Add(new EntryUnit(
							new(x, resultRoom.EdgeMaxY + 1), Direction4.Up
						));
					}
				}
				for (int y = resultRoom.EdgeMinY; y <= resultRoom.EdgeMaxY; y++) {
					if (squad.GetBlockAt(resultRoom.EdgeMinX - 1, y, z, BlockType.Entity) == CONNECTOR_ID) {
						EntryPointCache.Add(new EntryUnit(
							new(resultRoom.EdgeMinX - 1, y), Direction4.Left
						));
					}
					if (squad.GetBlockAt(resultRoom.EdgeMaxX + 1, y, z, BlockType.Entity) == CONNECTOR_ID) {
						EntryPointCache.Add(new EntryUnit(
							new(resultRoom.EdgeMaxX + 1, y), Direction4.Right
						));
					}
				}

				// Entry Points >> End Points
				foreach (var unit in EntryPointCache) {
					var currentPos = unit.Point;
					var currentDir = unit.Direction;
					var currentNormal = unit.Direction.Normal();
					const int MAX_STEP = 1024;
					int entityID, forwardEntityID;
					for (int step = 0; step < MAX_STEP; step++) {
						// Try Forward
						var pos = currentPos + currentNormal;
						var forwardNormal = currentNormal;
						entityID = forwardEntityID = squad.GetBlockAt(pos.x, pos.y, z, BlockType.Entity);
						if (entityID == CONNECTOR_ID) {
							currentPos = pos;
							continue;
						}
						// Try Turn Left
						currentDir = currentDir.AntiClockwise();
						currentNormal = currentDir.Normal();
						pos = currentPos + currentNormal;
						entityID = squad.GetBlockAt(pos.x, pos.y, z, BlockType.Entity);
						if (entityID == CONNECTOR_ID) {
							currentPos = pos;
							continue;
						}
						// Try Turn Right
						currentDir = currentDir.Opposite();
						currentNormal = currentDir.Normal();
						pos = currentPos + currentNormal;
						entityID = squad.GetBlockAt(pos.x, pos.y, z, BlockType.Entity);
						if (entityID == CONNECTOR_ID) {
							currentPos = pos;
							continue;
						}
						// Jump Out
						if (IsRoomEdgeBlock(forwardEntityID)) {
							EndPointCache.Add(currentPos + forwardNormal + forwardNormal);
						}
					}
				}

				// End Points >> Room Points
				foreach (var endPoint in EndPointCache) {
					if (!SearchForRoom(squad, endPoint.x, endPoint.y, z, out var _rootPoint)) continue;
					if (RoomPointCache.Contains(_rootPoint)) continue;
					RoomPointCache.Add(_rootPoint);
					TargetQueue.Enqueue(new RoomTarget(_rootPoint, currentNode));
				}

				// Finish
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