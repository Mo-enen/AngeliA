using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework.Entities;


namespace AngeliaFramework.Physics {
	public static class CellPhysics {




		#region --- SUB ---


		private struct Cell {
			public RectInt Rect;
			public Entity Entity;
			public uint Frame;
			public bool IsTrigger;
			public int Tag;
			public HitInfo GetInfo () => new(Rect, Entity, IsTrigger, Tag);
		}


		private class Layer {
			public Cell this[int x, int y, int z] {
				get => Cells[x, y, z];
				set => Cells[x, y, z] = value;
			}
			private readonly Cell[,,] Cells = null;
			public Layer (int width, int height) {
				Cells = new Cell[width, height, CELL_DEPTH];
				for (int i = 0; i < width; i++) {
					for (int j = 0; j < height; j++) {
						for (int z = 0; z < CELL_DEPTH; z++) {
							Cells[i, j, z].Frame = uint.MinValue;
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
			public HitInfo (RectInt rect, Entity entity, bool isTrigger, int tag) {
				Rect = rect;
				Entity = entity;
				IsTrigger = isTrigger;
				Tag = tag;
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
		public const int CELL_DEPTH = 8;
		public const int OVERLAP_RESULT_COUNT = CELL_DEPTH * Const.PHYSICS_LAYER_COUNT * 4;

		// Api
		public static int Width { get; private set; } = 0;
		public static int Height { get; private set; } = 0;
		public static int PositionX { get; set; } = 0;
		public static int PositionY { get; set; } = 0;

		// Data
		private static Layer[] Layers = null;
		private static Layer CurrentLayer = null;
		private static PhysicsLayer CurrentLayerEnum = PhysicsLayer.Item;
		private static uint CurrentFrame = uint.MinValue;
		private static readonly HitInfo[] OverlapResults = new HitInfo[OVERLAP_RESULT_COUNT];


		#endregion




		#region --- API ---


		// Setup
		public static void Init (int width, int height, int layerCount) {
			Width = width;
			Height = height;
			Layers = new Layer[layerCount];
		}


		public static void SetupLayer (int index) {
			Layers[index] = CurrentLayer = new Layer(Width, Height);
			CurrentLayerEnum = (PhysicsLayer)index;
		}


		// Fill
		public static void BeginFill () => CurrentFrame++;


		public static void Fill (PhysicsLayer layer, RectInt globalRect, Entity entity, bool isTrigger = false, int tag = 0) {
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
				var cell = CurrentLayer[i, j, dep];
				if (cell.Frame != CurrentFrame) {
					cell.Rect = globalRect;
					cell.Entity = entity;
					cell.Frame = CurrentFrame;
					cell.IsTrigger = isTrigger;
					cell.Tag = tag;
					CurrentLayer[i, j, dep] = cell;
					return;
				}
			}
		}


		// Overlap
		public static HitInfo Overlap (PhysicsMask mask, RectInt globalRect, Entity ignore = null, OperationMode mode = OperationMode.ColliderOnly, int tag = 0) {
			HitInfo result = null;
			for (int layerIndex = 0; layerIndex < Const.PHYSICS_LAYER_COUNT; layerIndex++) {
				var layer = (PhysicsLayer)layerIndex;
				if (!mask.HasFlag(layer.ToMask())) continue;
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
						var cell = layerItem[i, j, dep];
						if (cell.Frame != CurrentFrame) break;
						if (ignore != null && cell.Entity == ignore) continue;
						if (tag != 0 && cell.Tag != tag) continue;
						if ((cell.IsTrigger && useTrigger) || (!cell.IsTrigger && useCollider)) {
							if (cell.Rect.Overlaps(globalRect)) {
								return cell.GetInfo();
							}
						}
					}
				}
			}
			return null;
		}


		public static int ForAllOverlaps (PhysicsMask mask, RectInt globalRect, out HitInfo[] results, Entity ignore = null, OperationMode mode = OperationMode.ColliderOnly, int tag = 0) {
			int count = 0;
			for (int layerIndex = 0; layerIndex < Const.PHYSICS_LAYER_COUNT; layerIndex++) {
				var layer = (PhysicsLayer)layerIndex;
				if (!mask.HasFlag(layer.ToMask())) continue;
				count = ForAllOverlapsLogic(layer, globalRect, count, ignore, mode, tag);
			}
			ClearOverlapResult(count);
			results = OverlapResults;
			return count;
		}


		public static int ForAllOverlaps (PhysicsLayer layer, RectInt globalRect, out HitInfo[] results, Entity ignore = null, OperationMode mode = OperationMode.ColliderOnly, int tag = 0) {
			results = OverlapResults;
			int count = ForAllOverlapsLogic(layer, globalRect, 0, ignore, mode, tag);
			ClearOverlapResult(count);
			return count;
		}


		// Move
		public static Vector2Int Move (PhysicsMask mask, Vector2Int from, Vector2Int to, Vector2Int size, Entity entity) =>
			Move(mask, from, to, size, entity, out _);


		public static Vector2Int Move (PhysicsMask mask, Vector2Int from, Vector2Int to, Vector2Int size, Entity entity, out Direction4? hit) {
			var result0 = Push(mask, from, new Vector2Int(from.x, to.y), size, entity, out var hit0, out int dis0);
			var result1 = Push(mask, new(from.x, result0.y), new(to.x, result0.y), size, entity, out var hit1, out var dis1);
			hit = dis0 < dis1 ? hit0 : hit1;
			return result1;
			static Vector2Int Push (PhysicsMask mask, Vector2Int from, Vector2Int to, Vector2Int size, Entity entity, out Direction4? hitDirection, out int distance) {
				Vector2Int result = to;
				hitDirection = null;
				distance = int.MaxValue;
				int count = ForAllOverlaps(mask, new RectInt(to, size), out var results, entity);
				for (int index = 0; index < count; index++) {
					var hit = results[index];
					var center = hit.Rect.center.RoundToInt();
					bool leftSide = to.x < center.x;
					bool downSide = to.y < center.y;
					var ghostH = new Vector2Int(leftSide ? hit.Rect.x - size.x : hit.Rect.xMax, to.y);
					var ghostV = new Vector2Int(to.x, downSide ? hit.Rect.y - size.y : hit.Rect.yMax);
					bool useH = Util.SqrtDistance(ghostH, from) < Util.SqrtDistance(ghostV, from);
					var ghostPos = useH ? ghostH : ghostV;
					int _dis = Util.SqrtDistance(ghostPos, from);
					if (_dis < distance) {
						distance = _dis;
						result = ghostPos;
						hitDirection = useH ?
							leftSide ? Direction4.Left : Direction4.Right :
							downSide ? Direction4.Down : Direction4.Up;
					}
				}
				return result;
			}
		}


		// Editor
#if UNITY_EDITOR
		public static void Editor_ForAllCells (System.Action<int, HitInfo> action) {
			for (int i = 0; i < Layers.Length; i++) {
				if (Layers[i] == null) { continue; }
				for (int y = 0; y < Height; y++) {
					for (int x = 0; x < Width; x++) {
						for (int d = 0; d < CELL_DEPTH; d++) {
							var cell = Layers[i][x, y, d];
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
						var cell = layerItem[i, j, dep];
						if (cell.Frame != CurrentFrame) { break; }
						if (ignore != null && cell.Entity == ignore) continue;
						if (tag != 0 && cell.Tag != tag) continue;
						if ((cell.IsTrigger && useTrigger) || (!cell.IsTrigger && useCollider)) {
							if (cell.Rect.Overlaps(globalRect)) {
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
