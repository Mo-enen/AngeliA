using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


public struct PhysicsCell {

	public static readonly PhysicsCell EMPTY = new();

	public IRect Rect;
	public Entity Entity;
	public uint Frame;
	public bool IsTrigger;
	public Tag Tag;
	public int SourceID;

}



public enum OperationMode {
	ColliderOnly = 0,
	TriggerOnly = 1,
	ColliderAndTrigger = 2,
}


public static class Physics {




	#region --- SUB ---


	private class Layer {
		public const string CellsName = nameof(Cells);
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
	public const string LayersName = nameof(Layers);
	public const string CellsName = Layer.CellsName;

	// Api
	public static int PositionX { get; private set; } = 0;
	public static int PositionY { get; private set; } = 0;
	public static int CellWidth { get; private set; } = 1;
	public static int CellHeight { get; private set; } = 1;
	public static uint CurrentFrame { get; private set; } = uint.MinValue;
	public static bool IsReady { get; private set; } = false;

	// Data
	private static readonly PhysicsCell[] c_RoomOneway = new PhysicsCell[32];
	private static readonly PhysicsCell[] c_Movement = new PhysicsCell[32];
	private static readonly PhysicsCell[] c_Oneway = new PhysicsCell[32];
	private static readonly PhysicsCell[] c_GetEntity = new PhysicsCell[32];
	private static readonly PhysicsCell[] c_OverlapAll = new PhysicsCell[1024];
	private static readonly Pipe<(Rigidbody rig, IRect from, IRect to)> c_ForcePushCache = new(256);
	private static Layer[] Layers = null;
	private static Layer CurrentLayer = null;
	private static int CurrentLayerEnum = -1;
	private static int LayerCount = 0;
	private static int GlobalOperationStamp = int.MinValue;


	#endregion




	#region --- API ---


	[OnGameInitializeLater(64)]
	public static void Initialize () {
		int maxHeight = Universe.BuiltInInfo.MaxViewHeight;
		CellWidth = Universe.BuiltInInfo.ViewRatio * maxHeight / 1000 / Const.CEL + Const.SPAWN_PADDING_UNIT * 2 + Const.LEVEL_SPAWN_PADDING_UNIT * 2;
		CellHeight = maxHeight / Const.CEL + Const.SPAWN_PADDING_UNIT * 2 + Const.LEVEL_SPAWN_PADDING_UNIT * 2;
		LayerCount = PhysicsLayer.COUNT;
		Layers = new Layer[PhysicsLayer.COUNT];
		for (int i = 0; i < PhysicsLayer.COUNT; i++) {
			Layers[i] = CurrentLayer = new Layer(CellWidth, CellHeight);
		}
		IsReady = true;
	}


	// Fill
	[OnGameUpdate(-1024)]
	internal static void BeginFill () {
		PositionX = Stage.ViewRect.x - Const.SPAWN_PADDING - Const.LEVEL_SPAWN_PADDING;
		PositionY = Stage.ViewRect.y - Const.SPAWN_PADDING - Const.LEVEL_SPAWN_PADDING;
		CurrentFrame++;
	}


	public static void FillBlock (int layer, int blockID, IRect globalRect, bool isTrigger = false, Tag tag = 0) => FillLogic(layer, blockID, globalRect, null, 0, 0, isTrigger, tag);


	public static void FillEntity (int layer, Entity entity, bool isTrigger = false, Tag tag = 0) => FillLogic(layer, entity != null ? entity.TypeID : 0, entity.Rect, entity, 0, 0, isTrigger, tag);


	// Overlap
	public static bool Overlap (int mask, IRect globalRect, Entity ignore = null, OperationMode mode = OperationMode.ColliderOnly, Tag tag = 0) {
		for (int layerIndex = 0; layerIndex < LayerCount; layerIndex++) {
			if ((mask & (1 << layerIndex)) == 0) continue;
			if (OverlapLogic(layerIndex, globalRect, ignore, mode, tag, out _)) return true;
		}
		return false;
	}


	public static bool Overlap (int mask, IRect globalRect, out PhysicsCell info, Entity ignore = null, OperationMode mode = OperationMode.ColliderOnly, Tag tag = 0) {
		for (int layerIndex = 0; layerIndex < LayerCount; layerIndex++) {
			if ((mask & (1 << layerIndex)) == 0) continue;
			if (OverlapLogic(layerIndex, globalRect, ignore, mode, tag, out info)) return true;
		}
		info = PhysicsCell.EMPTY;
		return false;
	}


	public static PhysicsCell[] OverlapAll (
		int mask, IRect globalRect, out int count, Entity ignore = null,
		OperationMode mode = OperationMode.ColliderOnly, Tag tag = 0
	) {
		count = OverlapAll(c_OverlapAll, mask, globalRect, ignore, mode, tag);
		return c_OverlapAll;
	}


	public static int OverlapAll (
		PhysicsCell[] hits, int mask, IRect globalRect, Entity ignore = null,
		OperationMode mode = OperationMode.ColliderOnly, Tag tag = 0
	) {
		int count = 0;
		for (int layerIndex = 0; layerIndex < LayerCount; layerIndex++) {
			if ((mask & (1 << layerIndex)) == 0) continue;
			count = OverlapAllLogic(hits, count, layerIndex, globalRect, ignore, mode, tag, false);
		}
		return count;
	}


	// Entity
	public static T GetEntity<T> (IRect globalRect, int mask, Entity ignore = null, OperationMode mode = OperationMode.ColliderOnly, Tag tag = 0) {
		int count = OverlapAll(c_GetEntity, mask, globalRect, ignore, mode, tag);
		for (int i = 0; i < count; i++) {
			var e = c_GetEntity[i].Entity;
			if (e is T et && e.Active) return et;
		}
		return default;
	}
	public static Entity GetEntity (int typeID, IRect globalRect, int mask, Entity ignore = null, OperationMode mode = OperationMode.ColliderOnly, Tag tag = 0) {
		int count = OverlapAll(c_GetEntity, mask, globalRect, ignore, mode, tag);
		for (int i = 0; i < count; i++) {
			var e = c_GetEntity[i].Entity;
			if (e != null && e.Active && e.TypeID == typeID) return e;
		}
		return null;
	}


	public static bool HasEntity<T> (IRect globalRect, int mask, Entity ignore = null, OperationMode mode = OperationMode.ColliderOnly, Tag tag = 0) where T : Entity => GetEntity<T>(globalRect, mask, ignore, mode, tag) != null;


	// Room Check
	public static bool RoomCheck (int mask, IRect rect, Entity entity, Direction4 direction, OperationMode mode = OperationMode.ColliderOnly, Tag tag = 0) => RoomCheck(mask, rect, entity, direction, out _, mode, tag);
	public static bool RoomCheck (int mask, IRect rect, Entity entity, Direction4 direction, out PhysicsCell hit, OperationMode mode = OperationMode.ColliderOnly, Tag tag = 0) => !Overlap(mask, rect.EdgeOutside(direction), out hit, entity, mode, tag);


	public static bool RoomCheckOneway (int mask, IRect rect, Entity entity, Direction4 direction, bool overlapCheck = false, bool blockOnly = false) =>
		RoomCheckOneway(mask, rect, entity, direction, out _, overlapCheck, blockOnly);
	public static bool RoomCheckOneway (int mask, IRect rect, Entity entity, Direction4 direction, out PhysicsCell hit, bool overlapCheck = false, bool blockOnly = false) {
		hit = PhysicsCell.EMPTY;
		bool result = true;
		var gateDir = direction.Opposite();
		int count = OverlapAll(
			c_RoomOneway,
			mask, rect.EdgeOutside(direction), entity, OperationMode.TriggerOnly, FrameworkUtil.GetOnewayTag(gateDir)
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
	public static Int2 MoveIgnoreOneway (int mask, Int2 from, int speedX, int speedY, Int2 size, Entity entity) => MoveSafeLogic(mask, from, speedX, speedY, size, entity, true);


	public static Int2 Move (int mask, Int2 from, int speedX, int speedY, Int2 size, Entity entity) => MoveSafeLogic(mask, from, speedX, speedY, size, entity, false);


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
				mask, from, result, size, entity
			);
		}
		return result;
	}


	public static void ForcePush (Rigidbody host, Direction4 direction, int distance) {
		c_ForcePushCache.Reset();
		int deltaX = direction.IsHorizontal() ? distance : 0;
		int deltaY = direction.IsVertical() ? distance : 0;
		c_ForcePushCache.LinkToTail((host, host.Rect, host.Rect.Shift(deltaX, deltaY)));
		host.X += deltaX;
		host.Y += deltaY;
		if (deltaX != 0) host.VelocityX = 0;
		if (deltaY != 0) host.VelocityY = 0;
		for (int safe = 0; c_ForcePushCache.TryPopHead(out var pair) && safe < 4096; safe++) {
			var source = pair.rig;
			var sourceRect = pair.from;
			var targetSourceRect = pair.to;
			var hits = OverlapAll(host.CollisionMask, targetSourceRect, out int count, source);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Entity is not Rigidbody hitRig) continue;
				var hitRect = hitRig.Rect;
				switch (direction) {
					case Direction4.Left:
						if (hitRect.x >= sourceRect.x) continue;
						hitRig.X = targetSourceRect.x - hitRect.width;
						break;
					case Direction4.Right:
						if (hitRect.x <= sourceRect.x) continue;
						hitRig.X = targetSourceRect.xMax;
						break;
					case Direction4.Down:
						if (hitRect.y >= sourceRect.y) continue;
						hitRig.Y = targetSourceRect.y - hitRect.height;
						break;
					case Direction4.Up:
						if (hitRect.y <= sourceRect.y) continue;
						hitRig.Y = targetSourceRect.yMax;
						break;
				}
				hitRig.MakeGrounded(1, source.TypeID);
				hitRig.IgnoreGravity.True(1);
				c_ForcePushCache.LinkToTail((hitRig, hitRect, hitRig.Rect));
				if (deltaX != 0) hitRig.VelocityX = 0;
				if (deltaY != 0) hitRig.VelocityY = 0;
			}
		}
		c_ForcePushCache.Reset();
	}


	#endregion




	#region --- LGC ---


	private static bool OverlapLogic (int layer, IRect globalRect, Entity ignore, OperationMode mode, Tag tag, out PhysicsCell info) {
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
					ref var hit = ref layerItem.Cells[i, j, dep];
					if (hit.Frame != CurrentFrame) break;
					if (ignore != null && hit.Entity == ignore) continue;
					if (tag != Tag.None && !hit.Tag.HasAll(tag)) continue;
					if ((!hit.IsTrigger || !useTrigger) && (hit.IsTrigger || !useCollider)) continue;
					if (globalRect.Overlaps(hit.Entity != null ? hit.Entity.Rect : hit.Rect)) {
						info = hit;
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
		IRect globalRect, Entity ignore, OperationMode mode, Tag tag, bool ignoreStamp
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
					ref var hit = ref layerItem.Cells[i, j, dep];
					if (hit.Frame != CurrentFrame) { break; }
					if (ignore != null && hit.Entity == ignore) continue;
					if (tag != Tag.None && !hit.Tag.HasAll(tag)) continue;
					if ((!hit.IsTrigger || !useTrigger) && (hit.IsTrigger || !useCollider)) continue;
					if (!ignoreStamp && hit.Entity != null && hit.Entity.Stamp == entityStamp) continue;
					if (globalRect.Overlaps(hit.Entity != null ? hit.Entity.Rect : hit.Rect)) {
						if (hit.Entity != null) hit.Entity.Stamp = entityStamp;
						hits[count] = hit;
						count++;
						if (count >= maxLength) return count;
					}
				}
			}
		}
		return count;
	}


	private static void FillLogic (int layer, int sourceId, IRect globalRect, Entity entity, int speedX, int speedY, bool isTrigger, Tag tag) {

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
	private static Int2 MoveSafeLogic (int mask, Int2 from, int speedX, int speedY, Int2 size, Entity entity, bool ignoreOneway) {
		const int RIGIDBODY_FAST_SPEED = 32;
		var _from = from;
		var result = from;
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
				mask, from, result, size, entity
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
		var fromRect = new IRect(from, size);
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
			// Ignore Overlap Check
			if (
				entity is Rigidbody rig &&
				hit.Entity is Rigidbody hitRig &&
				(rig.RequireDodgeOverlap || hitRig.RequireDodgeOverlap) &&
				fromRect.Overlaps(hitRect)
			) {
				rig.RequireDodgeOverlap = true;
				hitRig.RequireDodgeOverlap = true;
				continue;
			}
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


	private static Int2 OnewayCheck (int mask, Int2 from, Int2 to, Int2 size, Entity entity) {
		int velX = to.x - from.x;
		int velY = to.y - from.y;
		if (velX != 0 && OnewayCheckLogic(mask, velX > 0 ? Direction4.Right : Direction4.Left, from, to, size, entity, out var newPos0)) {
			to.x = newPos0.x;
		}
		if (velY != 0 && OnewayCheckLogic(mask, velY > 0 ? Direction4.Up : Direction4.Down, from, to, size, entity, out var newPos1)) {
			to.y = newPos1.y;
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
FrameworkUtil.GetOnewayTag(gateDir)
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
	private static int GlobalX_to_CellX (int globalX) => (globalX - PositionX) / Const.CEL;
	private static int GlobalY_to_CellY (int globalY) => (globalY - PositionY) / Const.CEL;


	#endregion




}
