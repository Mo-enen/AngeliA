using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public static class CellPhysics {




		#region --- SUB ---


		private struct Cell {

			public RectInt GlobalRect => Entity != null ? Entity.Rect : Rect;

			public RectInt Rect;
			public Entity Entity;
			public uint Frame;
			public bool IsTrigger;
			public int Tag;

			private HitInfo Info;

			public HitInfo GetInfo () {
				if (Info == null) {
					Info = new();
				}
				Info.Rect = GlobalRect;
				Info.Entity = Entity;
				Info.IsTrigger = IsTrigger;
				Info.Tag = Tag;
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
							Cells[i, j, z] = new() { Frame = uint.MinValue };
						}
					}
				}
			}
		}


		public class HitInfo {
			public RectInt Rect;
			public Entity Entity;
			public bool IsTrigger;
			public int Tag;
		}


		public enum OperationMode {
			ColliderOnly = 0,
			TriggerOnly = 1,
			ColliderAndTrigger = 2,
		}


		#endregion




		#region --- VAR ---


		// Const
		private const int CELL_DEPTH = 8;
		private const int OVERLAP_RESULT_COUNT = CELL_DEPTH * Const.PHYSICS_LAYER_COUNT * 4;

		// Api
		public static int Width { get; } = (Const.MAX_VIEW_WIDTH + Const.SPAWN_GAP * 2) / Const.CELL_SIZE;
		public static int Height { get; } = (Const.MAX_VIEW_HEIGHT + Const.SPAWN_GAP * 2) / Const.CELL_SIZE;
		public static int PositionX { get; private set; } = 0;
		public static int PositionY { get; private set; } = 0;
		public static HitInfo[] OverlapResults { get; } = new HitInfo[OVERLAP_RESULT_COUNT];

		// Data
		private readonly static Layer[] Layers = new Layer[Const.PHYSICS_LAYER_COUNT];
		private static Layer CurrentLayer = null;
		private static PhysicsLayer CurrentLayerEnum = PhysicsLayer.Item;
		private static uint CurrentFrame = uint.MinValue;


		#endregion




		#region --- API ---


		// Setup
		public static void SetupLayer (int index) {
			Layers[index] = CurrentLayer = new Layer(Width, Height);
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
		public static HitInfo Overlap (PhysicsMask mask, RectInt globalRect, Entity ignore = null, OperationMode mode = OperationMode.ColliderOnly, int tag = 0) {
			HitInfo result = null;
			for (int layerIndex = 0; layerIndex < Const.PHYSICS_LAYER_COUNT; layerIndex++) {
				var layer = (PhysicsLayer)layerIndex;
				if (!mask.HasLayer(layer)) continue;
				result = Overlap(layer, globalRect, ignore, mode, tag);
				if (result != null) break;
			}
			return result;
		}


		public static HitInfo Overlap (PhysicsLayer layer, RectInt globalRect, Entity ignore = null, OperationMode mode = OperationMode.ColliderOnly, int tag = 0) {
			var layerItem = Layers[(int)layer];
			int l = Mathf.Max(globalRect.xMin.GetCellIndexX() - 1, 0);
			int d = Mathf.Max(globalRect.yMin.GetCellIndexY() - 1, 0);
			int r = Mathf.Min((globalRect.xMax - 1).GetCellIndexX() + 1, Width - 1);
			int u = Mathf.Min((globalRect.yMax - 1).GetCellIndexY() + 1, Height - 1);
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
								return cell.GetInfo();
							}
						}
					}
				}
			}
			return null;
		}


		public static int ForAllOverlaps (PhysicsMask mask, RectInt globalRect, Entity ignore = null, OperationMode mode = OperationMode.ColliderOnly, int tag = 0) {
			//return 0;
			int count = 0;
			for (int layerIndex = 0; layerIndex < Const.PHYSICS_LAYER_COUNT; layerIndex++) {
				var layer = (PhysicsLayer)layerIndex;
				if (!mask.HasLayer(layer)) continue;
				count = ForAllOverlapsLogic(layer, globalRect, count, ignore, mode, tag);
			}
			ClearOverlapResult(count);
			return count;
		}


		public static int ForAllOverlaps (PhysicsLayer layer, RectInt globalRect, Entity ignore = null, OperationMode mode = OperationMode.ColliderOnly, int tag = 0) {
			int count = ForAllOverlapsLogic(layer, globalRect, 0, ignore, mode, tag);
			ClearOverlapResult(count);
			return count;
		}


		// Check
		public static bool RoomCheck (PhysicsMask mask, Entity entity, Direction4 direction, OperationMode mode = OperationMode.ColliderOnly, int tag = 0) {
			const int GAP = 1;
			var eRect = entity.Rect;
			RectInt rect = direction switch {
				Direction4.Down => new(eRect.x, eRect.y - GAP, eRect.width, GAP),
				Direction4.Up => new(eRect.x, eRect.yMax, eRect.width, GAP),
				Direction4.Left => new(eRect.x - GAP, eRect.y, GAP, eRect.height),
				Direction4.Right => new(eRect.xMax, eRect.y, GAP, eRect.height),
				_ => throw new System.NotImplementedException(),
			};
			return Overlap(mask, rect, entity, mode, tag) == null;
		}


		public static bool PushCheck (PhysicsMask mask, int pushLevel, Entity target, Direction4 direction) =>
			target != null &&
			pushLevel > eRigidbody.GetPushLevel(target) &&
			RoomCheck(mask, target, direction);


		public static bool StopCheck (PhysicsMask mask, eRigidbody rig, Direction4 dir) {
			if (RoomCheck(mask, rig, dir)) return false;
			var rect = rig.Rect;
			int count = ForAllOverlaps(
				mask, new(
					rect.x,
					rect.y + (dir == Direction4.Up ? rect.height : -1),
					rect.width,
					1
				), rig
			);
			for (int i = 0; i < count; i++) {
				if (!PushCheck(mask, rig.PushLevel, OverlapResults[i].Entity, dir)) return true;
			}
			return false;
		}


		// Move
		public static Vector2Int Move (PhysicsMask mask, Vector2Int from, Vector2Int to, Vector2Int size, Entity entity) {
			bool moveH = from.x != to.x;
			bool moveV = from.y != to.y;
			if (moveH != moveV) {
				return Push(mask, from, to, size, entity);
			} else {
				var pos = Push(mask, from, new Vector2Int(from.x, to.y), size, entity);
				return Push(mask, new(from.x, pos.y), new(to.x, pos.y), size, entity);
			}
			// Func
			static Vector2Int Push (PhysicsMask mask, Vector2Int from, Vector2Int to, Vector2Int size, Entity entity) {
				int pushLevel = eRigidbody.GetPushLevel(entity);
				int distance = int.MaxValue;
				Vector2Int result = to;
				Vector2Int center = default;
				Vector2Int ghostH = default;
				Vector2Int ghostV = default;
				int count = ForAllOverlaps(mask, new RectInt(to, size), entity);
				for (int index = 0; index < count; index++) {
					var hit = OverlapResults[index];
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
					if (PushCheck(mask, pushLevel, hit.Entity, roomDir)) continue;
					// Solve
					int _dis = Util.SqrtDistance(useH ? ghostH : ghostV, from);
					if (_dis < distance) {
						distance = _dis;
						result = useH ? ghostH : ghostV;
					}
				}
				return result;
			}
		}


		// Editor
#if UNITY_EDITOR
		public static void Editor_ForAllCells (System.Action<int, HitInfo> action) {
			for (int i = 0; i < Layers.Length; i++) {
				var layer = Layers[i];
				if (layer == null) { continue; }
				for (int y = 0; y < Height; y++) {
					for (int x = 0; x < Width; x++) {
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


		private static int GetCellIndexX (this int x) => (x - PositionX) / Const.CELL_SIZE;


		private static int GetCellIndexY (this int y) => (y - PositionY) / Const.CELL_SIZE;


		private static void FillLogic (PhysicsLayer layer, RectInt globalRect, Entity entity, bool isTrigger, int tag) {
#if UNITY_EDITOR
			if (globalRect.width > Const.CELL_SIZE || globalRect.height > Const.CELL_SIZE) {
				Debug.LogWarning("[CellPhysics] Rect size too large.");
			}
#endif
			int i = globalRect.x.GetCellIndexX();
			int j = globalRect.y.GetCellIndexY();
			if (i < 0 || j < 0 || i >= Width || j >= Height) { return; }
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


		private static int ForAllOverlapsLogic (PhysicsLayer layer, RectInt globalRect, int startIndex, Entity ignore, OperationMode mode, int tag) {
			var layerItem = Layers[(int)layer];
			int l = Mathf.Max(globalRect.xMin.GetCellIndexX() - 1, 0);
			int d = Mathf.Max(globalRect.yMin.GetCellIndexY() - 1, 0);
			int r = Mathf.Min((globalRect.xMax - 1).GetCellIndexX() + 1, Width - 1);
			int u = Mathf.Min((globalRect.yMax - 1).GetCellIndexY() + 1, Height - 1);
			int count = startIndex;
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
								OverlapResults[count] = cell.GetInfo();
								count++;
								if (count >= OVERLAP_RESULT_COUNT) { return count; }
							}
						}
					}
				}
			}
			return count;
		}


		private static void ClearOverlapResult (int count) {
			for (int i = count; i < OVERLAP_RESULT_COUNT; i++) {
				if (OverlapResults[i] == null) break;
				OverlapResults[i] = null;
			}
		}


		#endregion




	}
}
