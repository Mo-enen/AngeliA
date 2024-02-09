using System.Text;
using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {


	// Element
	[EntityAttribute.MapEditorGroup("MapGenerator")]
	[RequireSprite("{0}")]
	public abstract class MapGeneratorElement : IMapItem { }
	public class RoomWall : MapGeneratorElement { }
	public class RoomTunnel : MapGeneratorElement { }
	public class RoomConnector : MapGeneratorElement { }


	// Room
	public class RoomNode {
		private class RoomNodeComparer : IComparer<RoomNode> {
			public static readonly RoomNodeComparer Instance = new();
			public int Compare (RoomNode a, RoomNode b) {
				int result = a.Room.TypeID.CompareTo(b.Room.TypeID);
				return result != 0 ? result : a.Room.ID.CompareTo(b.Room.ID);
			}
		}
		public Room Room { get; init; }
		public RoomNode Base { get; init; }
		public List<RoomNode> Children { get; init; }
		public Dictionary<int, Cubicle> Meta { get; init; }
		public RoomNode (Room room, RoomNode baseNode) {
			Room = room ?? Room.EMPTY;
			Base = baseNode;
			Children = new();
			Meta = new();
		}
		public string PrintTree () {

			var builder = new StringBuilder();
			AppendTree(this, builder, 0);
			return builder.ToString();

			static void AppendTree (RoomNode node, StringBuilder builder, int indent) {

				// Room ID
				if (node.Room != null) {
					builder.Append($" {node.Room}");
				} else {
					builder.Append('?');
				}

				// Meta
				builder.Append(" <color=#888888FF>");
				builder.Append(new string('m', node.Meta.Count));
				builder.Append("</color>");

				// Line
				builder.AppendLine();

				// Children
				for (int i = 0; i < node.Children.Count; i++) {
					builder.Append(new string('\t', indent));
					AppendTree(node.Children[i], builder, indent + 1);
				}
			}
		}
		public void SortAllChildren () {
			Children.Sort(RoomNodeComparer.Instance);
			foreach (var child in Children) {
				child.SortAllChildren();
			}
		}
	}


	public class Cubicle {
		public int ID;
		public int TypeID;
		public int ContentMinX;
		public int ContentMinY;
		public int ContentWidth;
		public int ContentHeight;
		public int[] Entities;
		public int[] Levels;
		public int[] Backgrounds;
		public int[] Elements;
		public int ContentMaxX => ContentMinX + ContentWidth - 1;
		public int ContentMaxY => ContentMinY + ContentHeight - 1;
	}


	public class Room : Cubicle {

		public static readonly Room EMPTY = new();

		public struct Tunnel {
			public int Index;
			public int Size;
			public Direction4 Direction;
		}

		public struct Teleporter {
			public int X;
			public int Y;
			public bool Front;
			public bool IsPortal;
		}

		public int EdgeMinX => ContentMinX - 1;
		public int EdgeMinY => ContentMinY - 1;
		public int EdgeMaxX => ContentMaxX + 1;
		public int EdgeMaxY => ContentMaxY + 1;
		public int EdgeWidth => ContentWidth + 2;
		public int EdgeHeight => ContentHeight + 2;

		public int[] EdgeLeft;
		public int[] EdgeRight;
		public int[] EdgeDown;
		public int[] EdgeUp;
		public Tunnel[] Tunnels;
		public Teleporter[] Teleporters;

		public int[] GetEdge (Direction4 direction) => direction switch {
			Direction4.Up => EdgeUp,
			Direction4.Down => EdgeDown,
			Direction4.Left => EdgeLeft,
			Direction4.Right => EdgeRight,
			_ => EdgeDown,
		};

		public override string ToString () => $"<color=#FFCC33>{ID}</color> {(TypeID != 0 ? "<size=75%>Ⓣ</size> " : "")}{ContentWidth}×{ContentHeight}<color=#888888FF>{(Tunnels?.Length > 0 ? " " + new string('t', Tunnels.Length) : "")}{(Teleporters?.Length > 0 ? " " + new string('d', Teleporters.Length) : "")}</color>";

	}


	// Generator
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


		[OnGameInitializeLater]
		public static void OnGameInitialize_Room () {
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


		protected override void AfterMapGenerate () {
			base.AfterMapGenerate();
			SampleReader.Clear();
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
				Elements = new int[width * height],
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
				room.EdgeDown[x] = squad.GetBlockAt(roomPointX + x, roomPointY, z, BlockType.Element);
				room.EdgeUp[x] = squad.GetBlockAt(roomPointX + x, roomPointY + edgeHeight - 1, z, BlockType.Element);
			}
			for (int y = 0; y < edgeHeight; y++) {
				room.EdgeLeft[y] = squad.GetBlockAt(roomPointX, roomPointY + y, z, BlockType.Element);
				room.EdgeRight[y] = squad.GetBlockAt(roomPointX + edgeWidth - 1, roomPointY + y, z, BlockType.Element);
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
					room.Elements[index] = squad.GetBlockAt(_x, _y, z, BlockType.Element);
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
				int id = squad.GetBlockAt(roomPointX + x, roomPointY, z, BlockType.Element);
				if (!IsRoomEdgeBlock(id)) break;
				xMax = roomPointX + x;
			}
			if (xMax < roomPointX + 2) return false;

			// Check Edge Left
			int yMax = roomPointY;
			for (int y = 0; y < MAX_HEIGHT; y++) {
				int id = squad.GetBlockAt(roomPointX, roomPointY + y, z, BlockType.Element);
				if (!IsRoomEdgeBlock(id)) break;
				yMax = roomPointY + y;
			}
			if (yMax < roomPointY + 2) return false;

			// Check Edge Up
			for (int x = roomPointX; x <= xMax; x++) {
				int id = squad.GetBlockAt(x, yMax, z, BlockType.Element);
				if (!IsRoomEdgeBlock(id)) return false;
			}

			// Check Edge Right
			for (int y = roomPointY; y <= yMax; y++) {
				int id = squad.GetBlockAt(xMax, y, z, BlockType.Element);
				if (!IsRoomEdgeBlock(id)) return false;
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
				int id = squad.GetBlockAt(unitX - x, unitY, z, BlockType.Element);
				if (IsRoomEdgeBlock(id)) {
					roomPoint.x = unitX - x;
					roomCaught = true;
					break;
				}
			}
			if (!roomCaught) return false;

			// Down
			roomCaught = false;
			for (int y = 0; y < MAX_HEIGHT; y++) {
				int id = squad.GetBlockAt(roomPoint.x, unitY - y, z, BlockType.Element);
				if (!IsRoomEdgeBlock(id)) {
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
					if (squad.GetBlockAt(x, resultRoom.EdgeMinY - 1, z, BlockType.Element) == CONNECTOR_ID) {
						EntryPointCache.Add(new EntryUnit() {
							Point = new(x, resultRoom.EdgeMinY - 1),
							IterateDirection = Direction4.Down,
							RoomDirection = Direction4.Down,
						});
					}
					if (squad.GetBlockAt(x, resultRoom.EdgeMaxY + 1, z, BlockType.Element) == CONNECTOR_ID) {
						EntryPointCache.Add(new EntryUnit() {
							Point = new(x, resultRoom.EdgeMaxY + 1),
							IterateDirection = Direction4.Up,
							RoomDirection = Direction4.Up,
						});
					}
				}
				for (int y = resultRoom.EdgeMinY; y <= resultRoom.EdgeMaxY; y++) {
					if (squad.GetBlockAt(resultRoom.EdgeMinX - 1, y, z, BlockType.Element) == CONNECTOR_ID) {
						EntryPointCache.Add(new EntryUnit() {
							Point = new(resultRoom.EdgeMinX - 1, y),
							IterateDirection = Direction4.Left,
							RoomDirection = Direction4.Left,
						});
					}
					if (squad.GetBlockAt(resultRoom.EdgeMaxX + 1, y, z, BlockType.Element) == CONNECTOR_ID) {
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
						int id;
						var pos = unit.Point;
						var forward = unit.IterateDirection.Normal();
						var left = unit.IterateDirection.AntiClockwise().Normal();
						var right = unit.IterateDirection.Clockwise().Normal();
						for (int step = 0; step < MAX_STEP; step++) {
							id = squad.GetBlockAt(pos.x, pos.y, z, BlockType.Element);
							if (id == CONNECTOR_ID) {
								// Turn Left
								var leftPoint = pos + left;
								int leftID = squad.GetBlockAt(leftPoint.x, leftPoint.y, z, BlockType.Element);
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
								int rightID = squad.GetBlockAt(rightPoint.x, rightPoint.y, z, BlockType.Element);
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
								if (IsRoomEdgeBlock(id)) {
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


		#endregion




		#region --- LGC ---


		private static bool IsRoomEdgeBlock (int id) => id == SOLID_ID || id == TUNNEL_ID;


		#endregion




	}
}