using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


/// <summary>
/// Basic unit of a physics data structure
/// </summary>
public struct PhysicsCell {

	public static readonly PhysicsCell EMPTY = new();

	/// <summary>
	/// Rect position in global space
	/// </summary>
	public IRect Rect;
	/// <summary>
	/// Target entity (null if from block)
	/// </summary>
	public Entity Entity;
	internal uint Frame;
	/// <summary>
	/// True if this cell is marked as trigger
	/// </summary>
	public bool IsTrigger;
	public Tag Tag;
	/// <summary>
	/// ID for identify which object filled this cell
	/// </summary>
	public int SourceID;

}


/// <summary>
/// What type of cells are included for the operation
/// </summary>
public enum OperationMode {
	ColliderOnly = 0,
	TriggerOnly = 1,
	ColliderAndTrigger = 2,
}


/// <summary>
/// Core system that handles physics of AngeliA games.
/// Logic of the system is frame-isolated which means data from prev frame will never effect current frame.
/// </summary>
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
	internal const string LayersName = nameof(Layers);
	internal const string CellsName = Layer.CellsName;

	// Api
	/// <summary>
	/// True if the system is ready to use.
	/// </summary>
	public static bool IsReady { get; private set; } = false;
	internal static uint CurrentFrame { get; private set; } = uint.MinValue;

	// Data
	private static readonly PhysicsCell[] c_RoomOneway = new PhysicsCell[32];
	private static readonly PhysicsCell[] c_Movement = new PhysicsCell[32];
	private static readonly PhysicsCell[] c_Oneway = new PhysicsCell[32];
	private static readonly PhysicsCell[] c_GetEntity = new PhysicsCell[32];
	private static readonly PhysicsCell[] c_OverlapAll = new PhysicsCell[1024];
	private static readonly Pipe<(Rigidbody rig, IRect from, IRect to)> c_ForcePushCache = new(256);
	private static Layer[] Layers = null;
	private static int PositionX = 0;
	private static int PositionY = 0;
	private static int CellWidth = 1;
	private static int CellHeight = 1;
	private static int LayerCount = 0;
	private static int GlobalOperationStamp = int.MinValue;


	#endregion




	#region --- API ---


	[OnGameInitializeLater(64)]
	internal static void Initialize () {
		int maxHeight = Universe.BuiltInInfo.MaxViewHeight;
		CellWidth = Universe.BuiltInInfo.ViewRatio * maxHeight / 1000 / Const.CEL + Const.SPAWN_PADDING_UNIT * 2 + Const.LEVEL_SPAWN_PADDING_UNIT * 2;
		CellHeight = maxHeight / Const.CEL + Const.SPAWN_PADDING_UNIT * 2 + Const.LEVEL_SPAWN_PADDING_UNIT * 2;
		LayerCount = PhysicsLayer.COUNT;
		Layers = new Layer[PhysicsLayer.COUNT];
		for (int i = 0; i < PhysicsLayer.COUNT; i++) {
			Layers[i] = new Layer(CellWidth, CellHeight);
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


	/// <summary>
	/// Add a physics cell for a map block for current frame. Call this function inside Entity.FirstUpdate
	/// </summary>
	/// <param name="layer">Which layer to add this cell into. (Use PhysicsLayer.XXX to get this value)</param>
	/// <param name="blockID">ID of the source block</param>
	/// <param name="globalRect">Rect position in global space</param>
	/// <param name="isTrigger">True if the cell should mark as trigger</param>
	/// <param name="tag">What extra info this cell have</param>
	public static void FillBlock (int layer, int blockID, IRect globalRect, bool isTrigger = false, Tag tag = 0) => FillLogic(layer, blockID, globalRect, null, 0, 0, isTrigger, tag);


	/// <summary>
	/// Add a physics cell for an entity for current frame. Call this function inside Entity.FirstUpdate
	/// </summary>
	/// <param name="layer">Which layer to add this cell into. (Use PhysicsLayer.XXX to get this value)</param>
	/// <param name="entity">Source entity for this cell</param>
	/// <param name="isTrigger">True if the cell should mark as trigger</param>
	/// <param name="tag">What extra info this cell have</param>
	public static void FillEntity (int layer, Entity entity, bool isTrigger = false, Tag tag = 0) => FillLogic(layer, entity != null ? entity.TypeID : 0, entity.Rect, entity, 0, 0, isTrigger, tag);


	/// <summary>
	/// Remove all cells that overlap target range
	/// </summary>
	/// <param name="mask">What physics layers is included (use PhysicsMask.XXX to get this value)</param>
	/// <param name="globalRect">Rect position in global space</param>
	/// <param name="mode">What type of cells are included for the operation</param>
	public static void IgnoreOverlap (int mask, IRect globalRect, OperationMode mode = OperationMode.ColliderAndTrigger) {
		for (int layerIndex = 0; layerIndex < LayerCount; layerIndex++) {
			if ((mask & (1 << layerIndex)) == 0) continue;
			var layerItem = Layers[layerIndex];
			int l = Util.Max(GlobalX_to_CellX(globalRect.xMin) - 1, 0);
			int d = Util.Max(GlobalY_to_CellY(globalRect.yMin) - 1, 0);
			int r = Util.Min(GlobalX_to_CellX(globalRect.xMax - 1) + 1, CellWidth - 1);
			int u = Util.Min(GlobalY_to_CellY(globalRect.yMax - 1) + 1, CellHeight - 1);
			bool useCollider = mode != OperationMode.TriggerOnly;
			bool useTrigger = mode != OperationMode.ColliderOnly;
			for (int j = d; j <= u; j++) {
				for (int i = l; i <= r; i++) {
					for (int dep = 0; dep < DEPTH; dep++) {
						ref var hit = ref layerItem.Cells[i, j, dep];
						if (hit.Frame != CurrentFrame) { break; }
						if ((!hit.IsTrigger || !useTrigger) && (hit.IsTrigger || !useCollider)) continue;
						if (!globalRect.Overlaps(hit.Entity != null ? hit.Entity.Rect : hit.Rect)) continue;
						hit.Rect = new IRect(int.MinValue, int.MinValue, 0, 0);
						hit.Entity = null;
						hit.SourceID = 0;
						hit.IsTrigger = true;
						hit.Tag = Tag.None;
					}
				}
			}
		}
	}


	// Overlap
	/// <inheritdoc cref="Overlap(int, IRect, out PhysicsCell, Entity, OperationMode, Tag)"/>
	public static bool Overlap (int mask, IRect globalRect, Entity ignore = null, OperationMode mode = OperationMode.ColliderOnly, Tag tag = 0) {
		for (int layerIndex = 0; layerIndex < LayerCount; layerIndex++) {
			if ((mask & (1 << layerIndex)) == 0) continue;
			if (OverlapLogic(layerIndex, globalRect, ignore, mode, tag, out _)) return true;
		}
		return false;
	}


	/// <summary>
	/// True if any cell overlap the given rect
	/// </summary>
	/// <param name="mask">What physics layers is included (use PhysicsMask.XXX to get this value)</param>
	/// <param name="globalRect">Rect position in global space</param>
	/// <param name="ignore">Entity that should be excluded</param>
	/// <param name="mode">What type of cells are included for the operation</param>
	/// <param name="tag">Only cells with all tags should be included</param>
	/// <param name="info">Cell of the overlaping object</param>
	public static bool Overlap (int mask, IRect globalRect, out PhysicsCell info, Entity ignore = null, OperationMode mode = OperationMode.ColliderOnly, Tag tag = 0) {
		for (int layerIndex = 0; layerIndex < LayerCount; layerIndex++) {
			if ((mask & (1 << layerIndex)) == 0) continue;
			if (OverlapLogic(layerIndex, globalRect, ignore, mode, tag, out info)) return true;
		}
		info = PhysicsCell.EMPTY;
		return false;
	}


	/// <summary>
	/// Find all cells that overlap with given rect and fill into an array (The array is cached internaly. Max size 1024)
	/// </summary>
	/// <param name="mask">What physics layers is included (use PhysicsMask.XXX to get this value)</param>
	/// <param name="globalRect">Rect position in global space</param>
	/// <param name="count">How many cells are founded</param>
	/// <param name="ignore">Entity that should be excluded</param>
	/// <param name="mode">What type of cells are included for the operation</param>
	/// <param name="tag">Only cells with all tags should be included</param>
	/// <returns>Cell array with the results</returns>
	public static PhysicsCell[] OverlapAll (
		int mask, IRect globalRect, out int count, Entity ignore = null,
		OperationMode mode = OperationMode.ColliderOnly, Tag tag = 0
	) {
		count = OverlapAll(c_OverlapAll, mask, globalRect, ignore, mode, tag);
		return c_OverlapAll;
	}


	/// <summary>
	/// Find all cells that overlap with given rect and fill into given array
	/// </summary>
	/// <param name="hits">The array that will hold the result</param>
	/// <param name="mask">What physics layers is included (use PhysicsMask.XXX to get this value)</param>
	/// <param name="globalRect">Rect position in global space</param>
	/// <param name="ignore">Entity that should be excluded</param>
	/// <param name="mode">What type of cells are included for the operation</param>
	/// <param name="tag">Only cells with all tags should be included</param>
	/// <returns>How many cells are founded</returns>
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
	/// <summary>
	/// Get entity instance from stage that overlap given rect
	/// </summary>
	/// <typeparam name="T">Type of the target entity</typeparam>
	/// <param name="globalRect">Rect position in global space</param>
	/// <param name="mask">What physics layers is included (use PhysicsMask.XXX to get this value)</param>
	/// <param name="ignore">Entity that should be excluded</param>
	/// <param name="mode">What type of cells are included for the operation</param>
	/// <param name="tag">Only cells with all tags should be included</param>
	public static T GetEntity<T> (IRect globalRect, int mask, Entity ignore = null, OperationMode mode = OperationMode.ColliderOnly, Tag tag = 0) {
		int count = OverlapAll(c_GetEntity, mask, globalRect, ignore, mode, tag);
		for (int i = 0; i < count; i++) {
			var e = c_GetEntity[i].Entity;
			if (e is T et && e.Active) return et;
		}
		return default;
	}


	/// <summary>
	/// Get entity instance from stage that overlap given rect
	/// </summary>
	/// <param name="typeID">Type of the target entity</param>
	/// <param name="globalRect">Rect position in global space</param>
	/// <param name="mask">What physics layers is included (use PhysicsMask.XXX to get this value)</param>
	/// <param name="ignore">Entity that should be excluded</param>
	/// <param name="mode">What type of cells are included for the operation</param>
	/// <param name="tag">Only cells with all tags should be included</param>
	public static Entity GetEntity (int typeID, IRect globalRect, int mask, Entity ignore = null, OperationMode mode = OperationMode.ColliderOnly, Tag tag = 0) {
		int count = OverlapAll(c_GetEntity, mask, globalRect, ignore, mode, tag);
		for (int i = 0; i < count; i++) {
			var e = c_GetEntity[i].Entity;
			if (e != null && e.Active && e.TypeID == typeID) return e;
		}
		return null;
	}


	/// <summary>
	/// True if any entity instance from stage that overlap given rect
	/// </summary>
	/// <typeparam name="T">Type of the target entity</typeparam>
	/// <param name="globalRect">Rect position in global space</param>
	/// <param name="mask">What physics layers is included (use PhysicsMask.XXX to get this value)</param>
	/// <param name="ignore">Entity that should be excluded</param>
	/// <param name="mode">What type of cells are included for the operation</param>
	/// <param name="tag">Only cells with all tags should be included</param>
	public static bool HasEntity<T> (IRect globalRect, int mask, Entity ignore = null, OperationMode mode = OperationMode.ColliderOnly, Tag tag = 0) where T : Entity => GetEntity<T>(globalRect, mask, ignore, mode, tag) != null;


	// Room Check
	/// <inheritdoc cref="RoomCheck(int, IRect, Entity, Direction4, out PhysicsCell, OperationMode, Tag)"/>
	public static bool RoomCheck (int mask, IRect rect, Entity entity, Direction4 direction, OperationMode mode = OperationMode.ColliderOnly, Tag tag = 0) => RoomCheck(mask, rect, entity, direction, out _, mode, tag);


	/// <summary>
	/// True if there is free room founded at given direction (only Involving solid colliders)
	/// </summary>
	/// <param name="mask">What physics layers is included (use PhysicsMask.XXX to get this value)</param>
	/// <param name="rect">Start location in global space</param>
	/// <param name="entity">Entity that should be exclude</param>
	/// <param name="direction"></param>
	/// <param name="mode">What type of cells are included for the operation</param>
	/// <param name="tag">Only cells with all tags should be included</param>
	/// <param name="hit">Cell of the object that blocks the free room</param>
	public static bool RoomCheck (int mask, IRect rect, Entity entity, Direction4 direction, out PhysicsCell hit, OperationMode mode = OperationMode.ColliderOnly, Tag tag = 0) => !Overlap(mask, rect.EdgeOutside(direction), out hit, entity, mode, tag);


	/// <inheritdoc cref="RoomCheckOneway(int, IRect, Entity, Direction4, out PhysicsCell, bool, bool)"/>
	public static bool RoomCheckOneway (int mask, IRect rect, Entity entity, Direction4 direction, bool overlapCheck = false, bool blockOnly = false) =>
		RoomCheckOneway(mask, rect, entity, direction, out _, overlapCheck, blockOnly);


	/// <summary>
	/// True if there is free room founded at given direction (only Involving oneway gate)
	/// </summary>
	/// <param name="mask">What physics layers is included (use PhysicsMask.XXX to get this value)</param>
	/// <param name="rect">Start location in global space</param>
	/// <param name="entity">Entity that should be exclude</param>
	/// <param name="direction"></param>
	/// <param name="hit">Cell of the object that blocks the free room</param>
	/// <param name="overlapCheck">True if oneway gates that not blocking the way (only overlap with rect) count as blocked</param>
	/// <param name="blockOnly">True if ignore oneway gates from entities</param>
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
	/// <summary>
	/// Perform move without oneway involved
	/// </summary>
	/// <param name="mask">What physics layers is included (use PhysicsMask.XXX to get this value)</param>
	/// <param name="from">Starting position in global space</param>
	/// <param name="speedX">(in global space)</param>
	/// <param name="speedY">(in global space)</param>
	/// <param name="size">(in global space)</param>
	/// <param name="entity">Target that is performing this movement</param>
	/// <returns>New position in global space after the movement</returns>
	public static Int2 MoveIgnoreOneway (int mask, Int2 from, int speedX, int speedY, Int2 size, Entity entity) => MoveSafeLogic(mask, from, speedX, speedY, size, entity, true);


	/// <summary>
	/// Perform move
	/// </summary>
	/// <param name="mask">What physics layers is included (use PhysicsMask.XXX to get this value)</param>
	/// <param name="from">Starting position in global space</param>
	/// <param name="speedX">(in global space)</param>
	/// <param name="speedY">(in global space)</param>
	/// <param name="size">(in global space)</param>
	/// <param name="entity">Target that is performing this movement</param>
	/// <returns>New position in global space after the movement</returns>
	public static Int2 Move (int mask, Int2 from, int speedX, int speedY, Int2 size, Entity entity) => MoveSafeLogic(mask, from, speedX, speedY, size, entity, false);


	/// <summary>
	/// Perform move without safe checks. (eg. Collide with objects in middle when moving too fast) This version saves CPU usage.
	/// </summary>
	/// <param name="mask">What physics layers is included (use PhysicsMask.XXX to get this value)</param>
	/// <param name="from">Starting position in global space</param>
	/// <param name="direction">Which direction to move</param>
	/// <param name="speed">(in global space)</param>
	/// <param name="size">(in global space)</param>
	/// <param name="entity">Target that is performing this movement</param>
	/// <param name="ignoreOneway">True if oneway gates are excluded</param>
	/// <returns>New position in global space after the movement</returns>
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


	/// <summary>
	/// Make a recursive push
	/// </summary>
	/// <param name="host">Entity that pushs other</param>
	/// <param name="direction"></param>
	/// <param name="distance">(in global space)</param>
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
		bool useCollider = mode != OperationMode.TriggerOnly;
		bool useTrigger = mode != OperationMode.ColliderOnly;
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
					if (!globalRect.Overlaps(hit.Entity != null ? hit.Entity.Rect : hit.Rect)) continue;
					if (hit.Entity != null) hit.Entity.Stamp = entityStamp;
					hits[count] = hit;
					count++;
					if (count >= maxLength) return count;
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
		// Fill
		var currentLayer = Layers[layer];
		for (int dep = 0; dep < DEPTH; dep++) {
			ref var cell = ref currentLayer.Cells[i, j, dep];
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
