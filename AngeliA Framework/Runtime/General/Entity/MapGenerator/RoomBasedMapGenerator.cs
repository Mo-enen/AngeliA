using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	public class TestGenerator : RoomBasedMapGenerator {
		protected override void OnMapGenerate () {

			//for (int i = 0; i < 16; i++) {
			//	ResultWriter.SetBlockAt(-i, 0, 0, BlockType.Entity, "CheckLalynnA".AngeHash());
			//}

		}
	}


	public abstract class RoomBasedMapGenerator : ActionMapGenerator {




		#region --- SUB ---


		private struct TeleInfo {
			public bool Front;
			public bool Portal;
			public TeleInfo (bool front, bool portal) {
				Front = front;
				Portal = portal;
			}
		}


		private struct EntryUnit {
			public Int2 Point;
			public Direction4 IterateDirection;
			public Direction4 RoomDirection;
		}


		private struct RoomSearchRequire {
			public Int2 Point;
			public RoomNode BaseNode;
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
		private static readonly List<EntryUnit> EntryPointCache = new();
		private static readonly List<EntryUnit> EndPointCache = new();
		private static readonly Queue<EntryUnit> EntryRequireQueue = new();
		private static readonly HashSet<Int2> EntryRequireHash = new();
		private static readonly Queue<RoomSearchRequire> SearchRequireQueue = new();
		private static readonly HashSet<Int2> SearchRequireHash = new();
		private static readonly List<Room.Tunnel> TunnelsCache = new();
		private static readonly List<Room.Teleporter> TelesCache = new();
		private static readonly Dictionary<int, TeleInfo> TelePool = new();
		private static int DynamicID = -1;


		#endregion




		#region --- MSG ---


		[OnGameInitialize]
		public static void OnGameInitialize () {
			foreach (var type in typeof(Door).AllChildClass()) {
				if (System.Activator.CreateInstance(type) is not Door door) continue;
				TelePool.TryAdd(type.AngeHash(), new TeleInfo(door.IsFrontDoor, false));
			}
			foreach (var type in typeof(PortalFront).AllChildClass()) {
				if (System.Activator.CreateInstance(type) is not PortalFront portal) continue;
				TelePool.TryAdd(type.AngeHash(), new TeleInfo(true, true));
			}
			foreach (var type in typeof(PortalBack).AllChildClass()) {
				if (System.Activator.CreateInstance(type) is not PortalBack portal) continue;
				TelePool.TryAdd(type.AngeHash(), new TeleInfo(false, true));
			}
		}


		protected override void BeforeMapGenerate () {
			base.BeforeMapGenerate();
			RoomPool.Clear();
			RootNode = ForAllConnectedRooms(
				SampleReader, X.ToUnit(), Y.ToUnit(), Stage.ViewZ - 1,
				(_room) => RoomPool.TryAdd(_room.ID, _room)
			);
			//Debug.Log(RootNode.PrintTree());
		}


		protected override void AfterMapGenerate () {
			base.AfterMapGenerate();
			SampleReader.Clear();
			RoomPool.Clear();
			RootNode = null;
			System.GC.Collect();
		}


		#endregion




		#region --- API ---


		public static Room LoadRoomAt (IBlockSquad squad, int roomPointX, int roomPointY, int z) {

			if (!HasValidRoomAt(squad, roomPointX, roomPointY, z, out int edgeWidth, out int edgeHeight)) return null;
			int width = edgeWidth - 2;
			int height = edgeHeight - 2;

			var room = new Room {
				ContentMinX = roomPointX + 1,
				ContentMinY = roomPointY + 1,
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

			// Room Quick Info
			if (squad.ReadSystemNumber(roomPointX, roomPointY - 1, z, Direction4.Right, out int _id)) {
				// ID
				room.ID = _id;
				room.TypeID = 0;
			} else {
				// ID
				room.ID = squad.ReadSystemNumber(roomPointX + 1, roomPointY - 1, z, Direction4.Right, out _id) ? _id : DynamicID--;
				// Type
				room.TypeID = squad.GetBlockAt(roomPointX, roomPointY - 1, z);
			}

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
									Index = i - currentSize,
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
			System.Threading.Monitor.Enter(TelesCache);
			try {
				TelesCache.Clear();
				for (int i = 0; i < room.Entities.Length; i++) {
					int entityID = room.Entities[i];
					if (entityID != 0 && TelePool.TryGetValue(entityID, out var info)) {
						TelesCache.Add(new Room.Teleporter() {
							X = i % width,
							Y = i / width,
							Front = info.Front,
							IsPortal = info.Portal,
						});
					}
				}
				room.Teleporters = TelesCache.ToArray();
			} finally {
				TelesCache.Clear();
				System.Threading.Monitor.Exit(TelesCache);
			}

			return room;
		}


		public static bool HasValidRoomAt (IBlockSquad squad, int roomPointX, int roomPointY, int z, out int edgeWidth, out int edgeHeight) {

			edgeWidth = default;
			edgeHeight = default;

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


		public static bool SearchForRoom (IBlockSquad squad, int unitX, int unitY, int z, out Int2 roomPoint) {

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

			System.Threading.Monitor.Enter(SearchRequireHash);
			System.Threading.Monitor.Enter(EntryRequireHash);
			System.Threading.Monitor.Enter(EntryPointCache);
			System.Threading.Monitor.Enter(EndPointCache);
			System.Threading.Monitor.Enter(SearchRequireQueue);
			System.Threading.Monitor.Enter(EntryRequireQueue);

			SearchRequireHash.Clear();
			EntryRequireHash.Clear();
			EntryPointCache.Clear();
			EndPointCache.Clear();
			SearchRequireQueue.Clear();
			EntryRequireQueue.Clear();

			try {
				var rootNode = new RoomNode(null, null);
				if (!SearchForRoom(squad, unitX, unitY, z, out var rootPoint)) return rootNode;
				const int MAX_ROOM_COUNT = 4096;
				SearchRequireHash.Add(rootPoint);
				SearchRequireQueue.Enqueue(new RoomSearchRequire() {
					Point = rootPoint,
					BaseNode = rootNode,
				});
				for (int roomCount = 0; SearchRequireQueue.Count > 0 && roomCount < MAX_ROOM_COUNT; roomCount++) {
					var target = SearchRequireQueue.Dequeue();
					Iterate(squad, target.Point, z, target.BaseNode, out Room resultRoom);
					if (onRoomLoaded != null && resultRoom != null) {
						onRoomLoaded.Invoke(resultRoom);
					}
				}
				if (rootNode.Children.Count > 0) rootNode = rootNode.Children[0];
				rootNode.SortAllChildren();
				return rootNode;
			} finally {
				EntryRequireHash.Clear();
				SearchRequireHash.Clear();
				EntryPointCache.Clear();
				EndPointCache.Clear();
				SearchRequireQueue.Clear();
				EntryRequireQueue.Clear();

				System.Threading.Monitor.Exit(EntryRequireHash);
				System.Threading.Monitor.Exit(SearchRequireHash);
				System.Threading.Monitor.Exit(EntryPointCache);
				System.Threading.Monitor.Exit(EndPointCache);
				System.Threading.Monitor.Exit(SearchRequireQueue);
				System.Threading.Monitor.Exit(EntryRequireQueue);
			}

			// Func
			static void Iterate (IBlockSquad squad, Int2 roomPoint, int z, RoomNode baseNode, out Room resultRoom) {

				// Current
				resultRoom = LoadRoomAt(squad, roomPoint.x, roomPoint.y, z);
				if (resultRoom == null) return;

				// Meta Room
				if (resultRoom.ContentWidth == 1 && resultRoom.ContentHeight == 1) {
					if (baseNode != null && !baseNode.Meta.ContainsKey(resultRoom.ID)) {
						baseNode.Meta.Add(resultRoom.ID, new Cubicle() {
							ID = resultRoom.ID,
							ContentMinX = resultRoom.ContentMinX,
							ContentMinY = resultRoom.ContentMinY,
							ContentWidth = resultRoom.ContentWidth,
							ContentHeight = resultRoom.ContentHeight,
							Entities = resultRoom.Entities,
							Levels = resultRoom.Levels,
							Backgrounds = resultRoom.Backgrounds,
						});
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
						EntryPointCache.Add(new EntryUnit() {
							Point = new(x, resultRoom.EdgeMinY - 1),
							IterateDirection = Direction4.Down,
							RoomDirection = Direction4.Down,
						});
					}
					if (squad.GetBlockAt(x, resultRoom.EdgeMaxY + 1, z, BlockType.Entity) == CONNECTOR_ID) {
						EntryPointCache.Add(new EntryUnit() {
							Point = new(x, resultRoom.EdgeMaxY + 1),
							IterateDirection = Direction4.Up,
							RoomDirection = Direction4.Up,
						});
					}
				}
				for (int y = resultRoom.EdgeMinY; y <= resultRoom.EdgeMaxY; y++) {
					if (squad.GetBlockAt(resultRoom.EdgeMinX - 1, y, z, BlockType.Entity) == CONNECTOR_ID) {
						EntryPointCache.Add(new EntryUnit() {
							Point = new(resultRoom.EdgeMinX - 1, y),
							IterateDirection = Direction4.Left,
							RoomDirection = Direction4.Left,
						});
					}
					if (squad.GetBlockAt(resultRoom.EdgeMaxX + 1, y, z, BlockType.Entity) == CONNECTOR_ID) {
						EntryPointCache.Add(new EntryUnit() {
							Point = new(resultRoom.EdgeMaxX + 1, y),
							IterateDirection = Direction4.Right,
							RoomDirection = Direction4.Right,
						});
					}
				}

				// Entry Points >> End Points
				EntryRequireQueue.Clear();
				EntryRequireHash.Clear();
				foreach (var unit in EntryPointCache) {
					if (EntryRequireHash.Contains(unit.Point)) continue;
					EntryRequireQueue.Enqueue(unit);
					EntryRequireHash.Add(unit.Point);
				}
				const int MAX_CONNECTOR_STEP = 1024;
				for (int safe = 0; EntryRequireQueue.Count > 0 && safe < MAX_CONNECTOR_STEP; safe++) {
					var unit = EntryRequireQueue.Dequeue();
					IterateConnector(squad, unit, z);
					static void IterateConnector (IBlockSquad squad, EntryUnit unit, int z) {
						const int MAX_STEP = 1024;
						int entityID;
						var pos = unit.Point;
						var forward = unit.IterateDirection.Normal();
						var left = unit.IterateDirection.AntiClockwise().Normal();
						var right = unit.IterateDirection.Clockwise().Normal();
						for (int step = 0; step < MAX_STEP; step++) {
							entityID = squad.GetBlockAt(pos.x, pos.y, z, BlockType.Entity);
							if (entityID == CONNECTOR_ID) {
								// Turn Left
								var leftPoint = pos + left;
								int leftID = squad.GetBlockAt(leftPoint.x, leftPoint.y, z, BlockType.Entity);
								if (leftID == CONNECTOR_ID && !EntryRequireHash.Contains(leftPoint)) {
									EntryRequireHash.Add(leftPoint);
									EntryRequireQueue.Enqueue(new EntryUnit() {
										Point = leftPoint,
										IterateDirection = unit.IterateDirection.AntiClockwise(),
										RoomDirection = unit.RoomDirection,
									});
								}
								// Turn Right
								var rightPoint = pos + right;
								int rightID = squad.GetBlockAt(rightPoint.x, rightPoint.y, z, BlockType.Entity);
								if (rightID == CONNECTOR_ID && !EntryRequireHash.Contains(rightPoint)) {
									EntryRequireHash.Add(rightPoint);
									EntryRequireQueue.Enqueue(new EntryUnit() {
										Point = rightPoint,
										IterateDirection = unit.IterateDirection.Clockwise(),
										RoomDirection = unit.RoomDirection,
									});
								}
								// Iter
								pos += forward;
							} else {
								// Jump Out
								if (IsRoomEdgeBlock(entityID)) {
									EndPointCache.Add(new EntryUnit() {
										Point = pos + forward,
										IterateDirection = unit.IterateDirection,
										RoomDirection = unit.RoomDirection,
									});
								}
								break;
							}
						}
					}
				}

				// End Points >> Room Points
				foreach (var endPoint in EndPointCache) {
					if (!SearchForRoom(squad, endPoint.Point.x, endPoint.Point.y, z, out var _rootPoint)) continue;
					if (SearchRequireHash.Contains(_rootPoint)) continue;
					SearchRequireHash.Add(_rootPoint);
					SearchRequireQueue.Enqueue(new RoomSearchRequire() {
						Point = _rootPoint,
						BaseNode = currentNode,
					});
				}

				// Finish
				EntryPointCache.Clear();
				EndPointCache.Clear();
				EntryRequireQueue.Clear();
				EntryRequireHash.Clear();
			}
		}


		public static void GenerateRoomCollapseMap (IBlockSquad squad, RoomNode rootNode, int unitX, int unitY, int z) {





		}


		#endregion




		#region --- LGC ---


		private static bool IsRoomEdgeBlock (int id) => id == SOLID_ID || id == TUNNEL_ID;


		#endregion




	}
}