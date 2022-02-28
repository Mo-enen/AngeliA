using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	public class HitInfo {
		public RectInt Rect;
		public Entity Entity;
		public bool IsTrigger;
		public int Tag;
		public uint Frame = uint.MinValue;
	}



	public static class CellPhysics {




		#region --- SUB ---


		private struct Cell {

			public RectInt GlobalRect => Entity != null ? Entity.Rect : Rect;

			public RectInt Rect;
			public Entity Entity;
			public uint Frame;
			public bool IsTrigger;
			public int Tag;
			public HitInfo Info;

			public HitInfo GetInfo () {
				if (Info.Frame != Frame) {
					Info.Rect = GlobalRect;
					Info.Entity = Entity;
					Info.IsTrigger = IsTrigger;
					Info.Tag = Tag;
					Info.Frame = Frame;
				}
				return Info;
			}

		}


		private class Layer {
			public readonly Cell[,,] Cells = null;
			public Layer (int width, int height) {
				Cells = new Cell[width, height, CELL_DEPTH];
				for (int i = 0; i < width; i++) {
					for (int j = 0; j < height; j++) {
						for (int z = 0; z < CELL_DEPTH; z++) {
							Cells[i, j, z] = new() {
								Frame = uint.MinValue,
								Info = new(),
							};
						}
					}
				}
			}
		}


		public enum OperationMode {
			ColliderOnly = 0,
			TriggerOnly = 1,
			ColliderAndTrigger = 2,
		}


		#endregion




		#region --- VAR ---


		// Const
		public const int WIDTH =
			(Const.MAX_VIEW_WIDTH + Const.SPAWN_PADDING * 2 + Const.BLOCK_SPAWN_PADDING * 2) / Const.CELL_SIZE;
		public const int HEIGHT =
			(Const.MAX_VIEW_HEIGHT + Const.SPAWN_PADDING * 2 + Const.BLOCK_SPAWN_PADDING * 2) / Const.CELL_SIZE;
		private const int CELL_DEPTH = 8;
		private const PhysicsMask ONEWAY_MASK = PhysicsMask.Level | PhysicsMask.Environment;

		// Api
		public static int PositionX { get; private set; } = 0;
		public static int PositionY { get; private set; } = 0;

		// Data
		private static readonly HitInfo[] c_PushCheck_OnewayCheck = new HitInfo[16];
		private static readonly HitInfo[] c_StopCheck = new HitInfo[16];
		private static readonly HitInfo[] c_MoveLogic = new HitInfo[32];
		private static readonly Layer[] Layers = new Layer[Const.PHYSICS_LAYER_COUNT];
		private static Layer CurrentLayer = null;
		private static PhysicsLayer CurrentLayerEnum = PhysicsLayer.Item;
		private static uint CurrentFrame = uint.MinValue;


		#endregion




		#region --- API ---


		// Setup
		public static void SetupLayer (int index) {
			Layers[index] = CurrentLayer = new Layer(WIDTH, HEIGHT);
			CurrentLayerEnum = (PhysicsLayer)index;
		}


		// Fill
		public static void BeginFill (int positionX, int positionY) {
			PositionX = positionX;
			PositionY = positionY;
			CurrentFrame++;
		}


		public static void FillBlock (PhysicsLayer layer, RectInt globalRect, bool isTrigger = false, int tag = 0) => FillLogic(layer, globalRect, null, isTrigger, tag);


		public static void FillEntity (PhysicsLayer layer, Entity entity, bool isTrigger = false, int tag = 0) => FillLogic(layer, entity.Rect, entity, isTrigger, tag);


		// Overlap
		public static bool Overlap (PhysicsMask mask, RectInt globalRect, Entity ignore = null, OperationMode mode = OperationMode.ColliderOnly, int tag = 0) {
			for (int layerIndex = 0; layerIndex < Const.PHYSICS_LAYER_COUNT; layerIndex++) {
				var layer = (PhysicsLayer)layerIndex;
				if (!mask.HasLayer(layer)) continue;
				if (Overlap(layer, globalRect, ignore, mode, tag)) return true;
			}
			return false;
		}


		public static bool Overlap (PhysicsLayer layer, RectInt globalRect, Entity ignore = null, OperationMode mode = OperationMode.ColliderOnly, int tag = 0) {
			var layerItem = Layers[(int)layer];
			int l = Mathf.Max(globalRect.xMin.GetCellIndexX() - 1, 0);
			int d = Mathf.Max(globalRect.yMin.GetCellIndexY() - 1, 0);
			int r = Mathf.Min((globalRect.xMax - 1).GetCellIndexX() + 1, WIDTH - 1);
			int u = Mathf.Min((globalRect.yMax - 1).GetCellIndexY() + 1, HEIGHT - 1);
			bool useCollider = mode == OperationMode.ColliderOnly || mode == OperationMode.ColliderAndTrigger;
			bool useTrigger = mode == OperationMode.TriggerOnly || mode == OperationMode.ColliderAndTrigger;
			for (int j = d; j <= u; j++) {
				for (int i = l; i <= r; i++) {
					for (int dep = 0; dep < CELL_DEPTH; dep++) {
						ref var cell = ref layerItem.Cells[i, j, dep];
						if (cell.Frame != CurrentFrame) break;
						if (ignore != null && cell.Entity == ignore) continue;
						if (tag != 0 && cell.Tag != tag) continue;
						if ((cell.IsTrigger && useTrigger) || (!cell.IsTrigger && useCollider)) {
							if (cell.GlobalRect.Overlaps(globalRect)) {
								return true;
							}
						}
					}
				}
			}
			return false;
		}


		public static int OverlapAll (
			HitInfo[] hits,
			PhysicsMask mask, RectInt globalRect, Entity ignore = null,
			OperationMode mode = OperationMode.ColliderOnly, int tag = 0
		) {
			int count = 0;
			for (int layerIndex = 0; layerIndex < Const.PHYSICS_LAYER_COUNT; layerIndex++) {
				var layer = (PhysicsLayer)layerIndex;
				if (!mask.HasLayer(layer)) continue;
				count = OverlapAllLogic(hits, count, layer, globalRect, ignore, mode, tag);
			}
			return count;
		}


		public static int OverlapAll (
			HitInfo[] hits,
			PhysicsLayer layer, RectInt globalRect, Entity ignore = null,
			OperationMode mode = OperationMode.ColliderOnly, int tag = 0
		) => OverlapAllLogic(hits, 0, layer, globalRect, ignore, mode, tag);


		// Check
		public static bool RoomCheck (PhysicsMask mask, RectInt rect, Entity entity, Direction4 direction, OperationMode mode = OperationMode.ColliderOnly, int tag = 0) {
			const int GAP = 1;
			RectInt _rect = direction switch {
				Direction4.Down => new(rect.x, rect.y - GAP, rect.width, GAP),
				Direction4.Up => new(rect.x, rect.yMax, rect.width, GAP),
				Direction4.Left => new(rect.x - GAP, rect.y, GAP, rect.height),
				Direction4.Right => new(rect.xMax, rect.y, GAP, rect.height),
				_ => throw new System.NotImplementedException(),
			};
			return !Overlap(mask, _rect, entity, mode, tag);
		}


		public static bool RoomCheck_Oneway (RectInt rect, Entity entity, Direction4 direction, bool overlapCheck = false) {
			const int GAP = 1;
			RectInt _rect = direction switch {
				Direction4.Down => new(rect.x, rect.y - GAP, rect.width, GAP),
				Direction4.Up => new(rect.x, rect.yMax, rect.width, GAP),
				Direction4.Left => new(rect.x - GAP, rect.y, GAP, rect.height),
				Direction4.Right => new(rect.xMax, rect.y, GAP, rect.height),
				_ => throw new System.NotImplementedException(),
			};
			var gateDir = direction.Opposite();
			int count = OverlapAll(
				c_PushCheck_OnewayCheck,
				ONEWAY_MASK, _rect, entity, OperationMode.TriggerOnly,
				Const.GetOnewayTag(gateDir)
			);
			for (int i = 0; i < count; i++) {
				var hit = c_PushCheck_OnewayCheck[i];
				if (!overlapCheck || !hit.Rect.Shrink(1).Overlaps(rect)) {
					return false;
				}
			}
			c_PushCheck_OnewayCheck.Dispose();
			return true;
		}


		public static bool PushCheck (PhysicsMask mask, RectInt rect, Entity target, int pushLevel, Direction4 direction) {
			bool colCheck = target != null &&
				pushLevel > eRigidbody.GetPushLevel(target) &&
				RoomCheck(mask, rect, target, direction);
			if (colCheck && target != null) {
				return RoomCheck_Oneway(rect, target, direction);
			}
			return colCheck;
		}


		public static bool MoveCheck (PhysicsMask mask, RectInt rect, eRigidbody target, Direction4 direction) {
			if (RoomCheck(mask, rect, target, direction)) return true;
			int count = OverlapAll(
				c_StopCheck,
				mask, new(
					rect.x + (direction == Direction4.Right ? rect.width : -1),
					rect.y + (direction == Direction4.Up ? rect.height : -1),
					direction == Direction4.Up || direction == Direction4.Down ? rect.width : 1,
					direction == Direction4.Up || direction == Direction4.Down ? 1 : rect.height
				), target
			);
			for (int i = 0; i < count; i++) {
				var hit = c_StopCheck[i];
				if (hit.Entity == null || !PushCheck(mask, hit.Rect, hit.Entity, target.PushLevel, direction)) return false;
			}
			c_StopCheck.Dispose();
			return true;
		}


		// Move
		public static Vector2Int Move (PhysicsMask mask, Vector2Int from, Vector2Int to, Vector2Int size, Entity entity) {
			bool moveH = from.x != to.x;
			bool moveV = from.y != to.y;
			if (moveH != moveV) {
				return MoveLogic(mask, from, to, size, entity);
			} else {
				var pos = MoveLogic(mask, from, new Vector2Int(from.x, to.y), size, entity);
				return MoveLogic(mask, new(from.x, pos.y), new(to.x, pos.y), size, entity);
			}
		}


		// Editor
#if UNITY_EDITOR
		public static void Editor_ForAllCells (System.Action<int, HitInfo> action) {
			for (int i = 0; i < Layers.Length; i++) {
				var layer = Layers[i];
				if (layer == null) { continue; }
				for (int y = 0; y < HEIGHT; y++) {
					for (int x = 0; x < WIDTH; x++) {
						for (int d = 0; d < CELL_DEPTH; d++) {
							ref var cell = ref layer.Cells[x, y, d];
							if (cell.Frame != CurrentFrame) { break; }
							action(i, cell.GetInfo());
						}
					}
				}
			}
		}
#endif


		#endregion




		#region --- LGC ---


		private static int OverlapAllLogic (
			HitInfo[] hits, int startIndex,
			PhysicsLayer layer, RectInt globalRect, Entity ignore = null,
			OperationMode mode = OperationMode.ColliderOnly, int tag = 0
		) {
			int count = startIndex;
			int maxLength = hits.Length;
			if (count >= maxLength) { return maxLength; }
			var layerItem = Layers[(int)layer];
			int l = Mathf.Max(globalRect.xMin.GetCellIndexX() - 1, 0);
			int d = Mathf.Max(globalRect.yMin.GetCellIndexY() - 1, 0);
			int r = Mathf.Min((globalRect.xMax - 1).GetCellIndexX() + 1, WIDTH - 1);
			int u = Mathf.Min((globalRect.yMax - 1).GetCellIndexY() + 1, HEIGHT - 1);
			bool useCollider = mode == OperationMode.ColliderOnly || mode == OperationMode.ColliderAndTrigger;
			bool useTrigger = mode == OperationMode.TriggerOnly || mode == OperationMode.ColliderAndTrigger;
			for (int j = d; j <= u; j++) {
				for (int i = l; i <= r; i++) {
					for (int dep = 0; dep < CELL_DEPTH; dep++) {
						ref var cell = ref layerItem.Cells[i, j, dep];
						if (cell.Frame != CurrentFrame) { break; }
						if (ignore != null && cell.Entity == ignore) continue;
						if (tag != 0 && cell.Tag != tag) continue;
						if ((cell.IsTrigger && useTrigger) || (!cell.IsTrigger && useCollider)) {
							if (cell.GlobalRect.Overlaps(globalRect)) {
								hits[count] = cell.GetInfo();
								count++;
								if (count >= maxLength) return count;
							}
						}
					}
				}
			}
			return count;
		}


		private static int GetCellIndexX (this int x) =>
			(x - PositionX + Const.BLOCK_SPAWN_PADDING) / Const.CELL_SIZE;


		private static int GetCellIndexY (this int y) =>
			(y - PositionY + Const.BLOCK_SPAWN_PADDING) / Const.CELL_SIZE;


		private static void FillLogic (PhysicsLayer layer, RectInt globalRect, Entity entity, bool isTrigger, int tag) {
#if UNITY_EDITOR
			if (globalRect.width > Const.CELL_SIZE || globalRect.height > Const.CELL_SIZE) {
				Debug.LogWarning("[CellPhysics] Rect size too large.");
			}
#endif
			int i = globalRect.x.GetCellIndexX();
			int j = globalRect.y.GetCellIndexY();
			if (i < 0 || j < 0 || i >= WIDTH || j >= HEIGHT) { return; }
			if (layer != CurrentLayerEnum) {
				CurrentLayerEnum = layer;
				CurrentLayer = Layers[(int)layer];
			}
			for (int dep = 0; dep < CELL_DEPTH; dep++) {
				ref var cell = ref CurrentLayer.Cells[i, j, dep];
				if (cell.Frame != CurrentFrame) {
					cell.Rect = globalRect;
					cell.Entity = entity;
					cell.Frame = CurrentFrame;
					cell.IsTrigger = isTrigger;
					cell.Tag = tag;
					return;
				}
			}
		}


		private static Vector2Int MoveLogic (PhysicsMask mask, Vector2Int from, Vector2Int to, Vector2Int size, Entity entity) {
			int distance = int.MaxValue;
			Vector2Int result = to;
			Vector2Int center = default;
			Vector2Int ghostH = default;
			Vector2Int ghostV = default;
			int pushLevel = eRigidbody.GetPushLevel(entity);
			int count = OverlapAll(c_MoveLogic, mask, new RectInt(to, size), entity);
			for (int i = 0; i < count; i++) {
				var hit = c_MoveLogic[i];
				var hitRect = hit.Rect;
				// H or V
				center.x = hitRect.x + hitRect.width / 2;
				center.y = hitRect.y + hitRect.height / 2;
				bool leftSide = to.x < center.x;
				bool downSide = to.y < center.y;
				ghostH.x = leftSide ? hitRect.x - size.x : hitRect.xMax;
				ghostH.y = to.y;
				ghostV.x = to.x;
				ghostV.y = downSide ? hitRect.y - size.y : hitRect.yMax;
				bool useH = Util.SqrtDistance(ghostH, from) < Util.SqrtDistance(ghostV, from);
				// Push Level
				var roomDir = useH ?
					leftSide ? Direction4.Right : Direction4.Left :
					downSide ? Direction4.Up : Direction4.Down;
				if (PushCheck(mask, hitRect, hit.Entity, pushLevel, roomDir)) continue;
				// Solve
				int _dis = Util.SqrtDistance(useH ? ghostH : ghostV, from);
				if (_dis < distance) {
					distance = _dis;
					result = useH ? ghostH : ghostV;
				}
			}
			c_MoveLogic.Dispose();
			return result;
		}


		#endregion




	}
}
