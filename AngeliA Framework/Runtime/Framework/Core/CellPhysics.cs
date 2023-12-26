using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {


	public struct PhysicsCell {

		public static readonly PhysicsCell EMPTY = new();

		public IRect Rect;
		public Entity Entity;
		public uint Frame;
		public bool IsTrigger;
		public int Tag;
		public int SourceID;

	}



	public enum OperationMode {
		ColliderOnly = 0,
		TriggerOnly = 1,
		ColliderAndTrigger = 2,
	}


	public static class CellPhysics {




		#region --- SUB ---


		private class Layer {
			public readonly PhysicsCell[,,] Cells = null;
			public Layer (int width, int height) {
				Cells = new PhysicsCell[width, height, DEPTH];
				for (int i = 0; i < width; i++) {
					for (int j = 0; j < height; j++) {
						for (int z = 0; z < DEPTH; z++) {
							Cells[i, j, z] = new PhysicsCell() {
								Frame = uint.MinValue,
							};
						}
					}
				}
			}
		}


		#endregion




		#region --- VAR ---


		// Const
		private const int DEPTH = 8;

		// Api
		internal static int PositionX { get; private set; } = 0;
		internal static int PositionY { get; private set; } = 0;
		internal static int CellWidth { get; private set; } = 1;
		internal static int CellHeight { get; private set; } = 1;

		// Data
		private static readonly PhysicsCell[] c_RoomOneway = new PhysicsCell[32];
		private static readonly PhysicsCell[] c_Movement = new PhysicsCell[32];
		private static readonly PhysicsCell[] c_Oneway = new PhysicsCell[32];
		private static readonly PhysicsCell[] c_GetEntity = new PhysicsCell[32];
		private static readonly PhysicsCell[] c_GeneralHits = new PhysicsCell[1024];
		private static Layer[] Layers = null;
		private static Layer CurrentLayer = null;
		private static uint CurrentFrame = uint.MinValue;
		private static int CurrentLayerEnum = -1;
		private static int LayerCount = 0;
		private static int GlobalOperationStamp = int.MinValue;


		#endregion




		#region --- API ---


		[OnGameInitializeLater(64)]
		public static void Initialize () {
			int layerCount = PhysicsLayer.COUNT;
			CellWidth = (Const.VIEW_RATIO * Const.MAX_HEIGHT / 1000) / Const.CEL + Const.SPAWN_PADDING_UNIT * 2 + Const.LEVEL_SPAWN_PADDING_UNIT * 2;
			CellHeight = (Const.MAX_HEIGHT) / Const.CEL + Const.SPAWN_PADDING_UNIT * 2 + Const.LEVEL_SPAWN_PADDING_UNIT * 2;
			LayerCount = layerCount;
			Layers = new Layer[layerCount];
			for (int i = 0; i < layerCount; i++) {
				Layers[i] = CurrentLayer = new Layer(CellWidth, CellHeight);
			}
		}


		// Fill
		internal static void BeginFill (int positionX, int positionY) {
			PositionX = positionX;
			PositionY = positionY;
			CurrentFrame++;
		}


		public static void FillBlock (int layer, int blockID, IRect globalRect, bool isTrigger = false, int tag = 0) => FillLogic(layer, blockID, globalRect, null, 0, 0, isTrigger, tag);


		public static void FillEntity (int layer, Entity entity, bool isTrigger = false, int tag = 0) => FillLogic(layer, entity != null ? entity.TypeID : 0, entity.Rect, entity, 0, 0, isTrigger, tag);


		// Overlap
		public static bool Overlap (int mask, IRect globalRect, Entity ignore = null, OperationMode mode = OperationMode.ColliderOnly, int tag = 0) {
			for (int layerIndex = 0; layerIndex < LayerCount; layerIndex++) {
				if ((mask & (1 << layerIndex)) == 0) continue;
				if (OverlapLogic(layerIndex, globalRect, ignore, mode, tag, out _)) return true;
			}
			return false;
		}


		public static bool Overlap (int mask, IRect globalRect, out PhysicsCell info, Entity ignore = null, OperationMode mode = OperationMode.ColliderOnly, int tag = 0) {
			for (int layerIndex = 0; layerIndex < LayerCount; layerIndex++) {
				if ((mask & (1 << layerIndex)) == 0) continue;
				if (OverlapLogic(layerIndex, globalRect, ignore, mode, tag, out info)) return true;
			}
			info = PhysicsCell.EMPTY;
			return false;
		}


		public static PhysicsCell[] OverlapAll (
			int mask, IRect globalRect, out int count, Entity ignore = null,
			OperationMode mode = OperationMode.ColliderOnly, int tag = 0
		) {
			count = OverlapAll(c_GeneralHits, mask, globalRect, ignore, mode, tag);
			return c_GeneralHits;
		}


		private static int OverlapAll (
			PhysicsCell[] hits, int mask, IRect globalRect, Entity ignore = null,
			OperationMode mode = OperationMode.ColliderOnly, int tag = 0
		) {
			int count = 0;
			for (int layerIndex = 0; layerIndex < LayerCount; layerIndex++) {
				if ((mask & (1 << layerIndex)) == 0) continue;
				count = OverlapAllLogic(hits, count, layerIndex, globalRect, ignore, mode, tag, false);
			}
			return count;
		}


		// Entity
		public static T GetEntity<T> (IRect globalRect, int mask, Entity ignore = null, OperationMode mode = OperationMode.ColliderOnly, int tag = 0) where T : Entity {
			int count = OverlapAll(c_GetEntity, mask, globalRect, ignore, mode, tag);
			for (int i = 0; i < count; i++) {
				var e = c_GetEntity[i].Entity;
				if (e is T et && e.Active) return et;
			}
			return null;
		}
		public static Entity GetEntity (System.Type type, IRect globalRect, int mask, Entity ignore = null, OperationMode mode = OperationMode.ColliderOnly, int tag = 0) {
			int count = OverlapAll(c_GetEntity, mask, globalRect, ignore, mode, tag);
			for (int i = 0; i < count; i++) {
				var e = c_GetEntity[i].Entity;
				if (e == null || !e.Active) continue;
				var eType = e.GetType();
				if (eType == type || eType.IsSubclassOf(type)) return e;
			}
			return null;
		}
		public static Entity GetEntity (int typeID, IRect globalRect, int mask, Entity ignore = null, OperationMode mode = OperationMode.ColliderOnly, int tag = 0) {
			int count = OverlapAll(c_GetEntity, mask, globalRect, ignore, mode, tag);
			for (int i = 0; i < count; i++) {
				var e = c_GetEntity[i].Entity;
				if (e != null && e.Active && e.TypeID == typeID) return e;
			}
			return null;
		}


		public static bool HasEntity<T> (IRect globalRect, int mask, Entity ignore = null, OperationMode mode = OperationMode.ColliderOnly, int tag = 0) where T : Entity => GetEntity<T>(globalRect, mask, ignore, mode, tag) != null;
		public static bool HasEntity (System.Type type, IRect globalRect, int mask, Entity ignore = null, OperationMode mode = OperationMode.ColliderOnly, int tag = 0) => GetEntity(type, globalRect, mask, ignore, mode, tag) != null;


		// Room Check
		public static bool RoomCheck (int mask, IRect rect, Entity entity, Direction4 direction, OperationMode mode = OperationMode.ColliderOnly, int tag = 0) => RoomCheck(mask, rect, entity, direction, out _, mode, tag);
		public static bool RoomCheck (int mask, IRect rect, Entity entity, Direction4 direction, out PhysicsCell hit, OperationMode mode = OperationMode.ColliderOnly, int tag = 0) => !Overlap(mask, rect.Edge(direction), out hit, entity, mode, tag);


		public static bool RoomCheckOneway (int mask, IRect rect, Entity entity, Direction4 direction, bool overlapCheck = false, bool blockOnly = false) =>
			RoomCheckOneway(mask, rect, entity, direction, out _, overlapCheck, blockOnly);
		public static bool RoomCheckOneway (int mask, IRect rect, Entity entity, Direction4 direction, out PhysicsCell hit, bool overlapCheck = false, bool blockOnly = false) {
			hit = PhysicsCell.EMPTY;
			bool result = true;
			var gateDir = direction.Opposite();
			int count = OverlapAll(
				c_RoomOneway,
				mask, rect.Edge(direction), entity, OperationMode.TriggerOnly,
				AngeUtil.GetOnewayTag(gateDir)
			);
			for (int i = 0; i < count; i++) {
				hit = c_RoomOneway[i];
				if (blockOnly && hit.Entity != null) continue;
				if (!overlapCheck || !hit.Rect.Shrink(1).Overlaps(rect)) {
					result = false;
					break;
				}
			}
			return result;
		}


		// Move
		public static Int2 MoveIgnoreOneway (int mask, Int2 from, int speedX, int speedY, Int2 size, Entity entity) => MoveSafeLogic(mask, from, speedX, speedY, size, entity, true, out _, out _);


		public static Int2 Move (int mask, Int2 from, int speedX, int speedY, Int2 size, Entity entity, out bool stopX, out bool stopY) => MoveSafeLogic(mask, from, speedX, speedY, size, entity, false, out stopX, out stopY);


		public static Int2 MoveImmediately (
			int mask, Int2 from, Direction4 direction, int speed,
			Int2 size, Entity entity, bool ignoreOneway = false
		) {
			var dir = direction.Normal();
			var result = MoveLogic(
				mask, from,
				new Int2(from.x + dir.x * speed, from.y + dir.y * speed),
				size, entity
			);
			if (!ignoreOneway) {
				result = OnewayCheck(
					mask, from, result, size, entity, out _, out _
				);
			}
			return result;
		}


		#endregion




		#region --- LGC ---


		private static bool OverlapLogic (int layer, IRect globalRect, Entity ignore, OperationMode mode, int tag, out PhysicsCell info) {
			var layerItem = Layers[layer];
			int l = Util.Max(GlobalX_to_CellX(globalRect.xMin) - 1, 0);
			int d = Util.Max(GlobalY_to_CellY(globalRect.yMin) - 1, 0);
			int r = Util.Min(GlobalX_to_CellX(globalRect.xMax - 1) + 1, CellWidth - 1);
			int u = Util.Min(GlobalY_to_CellY(globalRect.yMax - 1) + 1, CellHeight - 1);
			bool useCollider = mode == OperationMode.ColliderOnly || mode == OperationMode.ColliderAndTrigger;
			bool useTrigger = mode == OperationMode.TriggerOnly || mode == OperationMode.ColliderAndTrigger;
			for (int j = d; j <= u; j++) {
				for (int i = l; i <= r; i++) {
					for (int dep = 0; dep < DEPTH; dep++) {
						ref var cell = ref layerItem.Cells[i, j, dep];
						if (cell.Frame != CurrentFrame) break;
						if (ignore != null && cell.Entity == ignore) continue;
						if (tag != 0 && cell.Tag != tag) continue;
						if ((!cell.IsTrigger || !useTrigger) && (cell.IsTrigger || !useCollider)) continue;
						if (globalRect.Overlaps(cell.Entity != null ? cell.Entity.Rect : cell.Rect)) {
							info = cell;
							return true;
						}
					}
				}
			}
			info = PhysicsCell.EMPTY;
			return false;
		}


		private static int OverlapAllLogic (
			PhysicsCell[] hits, int startIndex, int layer,
			IRect globalRect, Entity ignore, OperationMode mode, int tag, bool ignoreStamp
		) {
			int count = startIndex;
			int maxLength = hits.Length;
			if (count >= maxLength) { return maxLength; }
			var layerItem = Layers[layer];
			int l = Util.Max(GlobalX_to_CellX(globalRect.xMin) - 1, 0);
			int d = Util.Max(GlobalY_to_CellY(globalRect.yMin) - 1, 0);
			int r = Util.Min(GlobalX_to_CellX(globalRect.xMax - 1) + 1, CellWidth - 1);
			int u = Util.Min(GlobalY_to_CellY(globalRect.yMax - 1) + 1, CellHeight - 1);
			bool useCollider = mode == OperationMode.ColliderOnly || mode == OperationMode.ColliderAndTrigger;
			bool useTrigger = mode == OperationMode.TriggerOnly || mode == OperationMode.ColliderAndTrigger;
			int entityStamp = GlobalOperationStamp++;
			for (int j = d; j <= u; j++) {
				for (int i = l; i <= r; i++) {
					for (int dep = 0; dep < DEPTH; dep++) {
						ref var cell = ref layerItem.Cells[i, j, dep];
						if (cell.Frame != CurrentFrame) { break; }
						if (ignore != null && cell.Entity == ignore) continue;
						if (tag != 0 && cell.Tag != tag) continue;
						if ((!cell.IsTrigger || !useTrigger) && (cell.IsTrigger || !useCollider)) continue;
						if (!ignoreStamp && cell.Entity != null && cell.Entity.PhysicsOperationStamp == entityStamp) continue;
						if (globalRect.Overlaps(cell.Entity != null ? cell.Entity.Rect : cell.Rect)) {
							if (cell.Entity != null) cell.Entity.PhysicsOperationStamp = entityStamp;
							hits[count] = cell;
							count++;
							if (count >= maxLength) return count;
						}
					}
				}
			}
			return count;
		}


		private static void FillLogic (int layer, int sourceId, IRect globalRect, Entity entity, int speedX, int speedY, bool isTrigger, int tag) {

			if (globalRect.width > Const.CEL || globalRect.height > Const.CEL) {
				// Too Large
				for (int y = globalRect.yMin; y < globalRect.yMax; y += Const.CEL) {
					int _height = Util.Min(Const.CEL, globalRect.yMax - y);
					for (int x = globalRect.xMin; x < globalRect.xMax; x += Const.CEL) {
						FillLogic(layer, sourceId, new(x, y, Util.Min(Const.CEL, globalRect.xMax - x), _height), entity, speedX, speedY, isTrigger, tag);
					}
				}
				return;
			}

			int i = GlobalX_to_CellX(globalRect.x);
			int j = GlobalY_to_CellY(globalRect.y);
			if (i < 0 || j < 0 || i >= CellWidth || j >= CellHeight) { return; }
			if (layer != CurrentLayerEnum) {
				CurrentLayerEnum = layer;
				CurrentLayer = Layers[layer];
			}

			// Fill
			for (int dep = 0; dep < DEPTH; dep++) {
				ref var cell = ref CurrentLayer.Cells[i, j, dep];
				if (cell.Frame != CurrentFrame) {
					cell.Rect = globalRect;
					cell.Entity = entity;
					cell.Frame = CurrentFrame;
					cell.IsTrigger = isTrigger;
					cell.Tag = tag;
					cell.SourceID = sourceId;
					break;
				}
			}
		}


		// Move
		private static Int2 MoveSafeLogic (int mask, Int2 from, int speedX, int speedY, Int2 size, Entity entity, bool ignoreOneway, out bool stopForOnewayX, out bool stopForOnewayY) {
			const int RIGIDBODY_FAST_SPEED = 32;
			var _from = from;
			var result = from;
			stopForOnewayX = false;
			stopForOnewayY = false;
			
			if (Util.Abs(speedX) > RIGIDBODY_FAST_SPEED || Util.Abs(speedY) > RIGIDBODY_FAST_SPEED) {
				// Too Fast
				int _speedX = speedX;
				int _speedY = speedY;
				while (_speedX != 0 || _speedY != 0) {
					int _sX = Util.Clamp(_speedX, -RIGIDBODY_FAST_SPEED, RIGIDBODY_FAST_SPEED);
					int _sY = Util.Clamp(_speedY, -RIGIDBODY_FAST_SPEED, RIGIDBODY_FAST_SPEED);
					_speedX -= _sX;
					_speedY -= _sY;
					result = MoveLogic3(
						mask, _from,
						new Int2(_from.x + _sX, _from.y + _sY),
						new(size.x, size.y), entity
					);
					if (result == _from) break;
					_from = result;
				}
			} else {
				// Normal
				result = MoveLogic3(
					mask, _from,
					new Int2(_from.x + speedX, _from.y + speedY),
					new(size.x, size.y),
					entity
				);
			}
			if (!ignoreOneway) {
				result = OnewayCheck(
					mask, from, result, size, entity,
					out stopForOnewayX, out stopForOnewayY
				);
			}
			return result;
		}


		private static Int2 MoveLogic3 (int mask, Int2 from, Int2 to, Int2 size, Entity entity) {
			bool moveH = from.x != to.x;
			bool moveV = from.y != to.y;
			if (moveH != moveV) {
				return MoveLogic(mask, from, to, size, entity);
			} else {
				var pos = MoveLogic(mask, from, new Int2(from.x, to.y), size, entity);
				return MoveLogic(mask, new(from.x, pos.y), new(to.x, pos.y), size, entity);
			}
		}


		private static Int2 MoveLogic (int mask, Int2 from, Int2 to, Int2 size, Entity entity) {
			int distance = int.MaxValue;
			Int2 result = to;
			Int2 center = default;
			Int2 ghostH = default;
			Int2 ghostV = default;

			int count = 0;
			var globalRect = new IRect(to, size);
			for (int layerIndex = 0; layerIndex < LayerCount; layerIndex++) {
				if ((mask & (1 << layerIndex)) == 0) continue;
				count = OverlapAllLogic(c_Movement, count, layerIndex, globalRect, entity, OperationMode.ColliderOnly, 0, true);
			}

			bool heavyPushed = false;
			for (int i = 0; i < count; i++) {
				var hit = c_Movement[i];
				var hitRect = hit.Rect;
				bool _heavyPush = hit.Entity == null;
				if (heavyPushed && !_heavyPush) continue;
				// H or V
				center.x = hitRect.x + hitRect.width / 2;
				center.y = hitRect.y + hitRect.height / 2;
				bool leftSide = from.x + size.x / 2 < center.x;
				bool downSide = from.y + size.y / 2 < center.y;
				ghostH.x = leftSide ? hitRect.x - size.x : hitRect.xMax;
				ghostH.y = to.y;
				ghostV.x = to.x;
				ghostV.y = downSide ? hitRect.y - size.y : hitRect.yMax;
				// Solve
				bool useH = Util.SquareDistance(ghostH, from) < Util.SquareDistance(ghostV, from);
				int _dis = Util.SquareDistance(useH ? ghostH : ghostV, from);
				if (_dis < distance || (!heavyPushed && _heavyPush)) {
					distance = _dis;
					heavyPushed = _heavyPush;
					result = useH ? ghostH : ghostV;
				}
			}
			return result;
		}


		private static Int2 OnewayCheck (int mask, Int2 from, Int2 to, Int2 size, Entity entity, out bool stopForOnewayX, out bool stopForOnewayY) {
			stopForOnewayX = false;
			stopForOnewayY = false;
			int velX = to.x - from.x;
			int velY = to.y - from.y;
			if (velX != 0 && OnewayCheckLogic(mask, velX > 0 ? Direction4.Right : Direction4.Left, from, to, size, entity, out var newPos0)) {
				to.x = newPos0.x;
				stopForOnewayX = true;
			}
			if (velY != 0 && OnewayCheckLogic(mask, velY > 0 ? Direction4.Up : Direction4.Down, from, to, size, entity, out var newPos1)) {
				to.y = newPos1.y;
				stopForOnewayY = true;
			}
			return to;
		}


		private static bool OnewayCheckLogic (int mask, Direction4 moveDirection, Int2 from, Int2 to, Int2 size, Entity entity, out Int2 newPos) {
			newPos = to;
			bool result = false;
			var gateDir = moveDirection.Opposite();
			int oCount = OverlapAll(
				c_Oneway, mask, new(to.x, to.y, size.x, size.y), entity,
				OperationMode.TriggerOnly,
				AngeUtil.GetOnewayTag(gateDir)
			);
			for (int i = 0; i < oCount; i++) {
				var hit = c_Oneway[i];
				if (!OnewayPassCheck(
					hit.Rect,
					gateDir,
					new(from.x, from.y),
					new(to.x, to.y),
					size,
					out newPos
				)) {
					result = true;
					break;
				}
			}
			return result;
			// Func
			static bool OnewayPassCheck (IRect onewayRect, Direction4 gateDirection, Int2 from, Int2 to, Int2 size, out Int2 newPos) {
				newPos = to;
				var rect = onewayRect;
				switch (gateDirection) {
					case Direction4.Down:
						if (from.y + size.y <= rect.yMin && to.y + size.y > rect.yMin) {
							newPos.y = rect.yMin - size.y;
							return false;
						}
						break;
					case Direction4.Up:
						if (from.y >= rect.yMax && to.y < rect.yMax) {
							newPos.y = rect.yMax;
							return false;
						}
						break;
					case Direction4.Left:
						if (from.x + size.x <= rect.xMin && to.x + size.x > rect.xMin) {
							newPos.x = rect.xMin - size.x;
							return false;
						}
						break;
					case Direction4.Right:
						if (from.x >= rect.xMax && to.x < rect.xMax) {
							newPos.x = rect.xMax;
							return false;
						}
						break;
				}
				return true;
			}
		}


		// Util
		private static int GlobalX_to_CellX (int globalX) => (globalX - PositionX) / Const.CEL + Const.LEVEL_SPAWN_PADDING_UNIT + Const.SPAWN_PADDING_UNIT;
		private static int GlobalY_to_CellY (int globalY) => (globalY - PositionY) / Const.CEL + Const.LEVEL_SPAWN_PADDING_UNIT + Const.SPAWN_PADDING_UNIT;


		#endregion




	}
}
