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
		public static HitInfo Overlap (PhysicsLayer layer, RectInt globalRect, Entity ignore = null, OperationMode mode = OperationMode.ColliderOnly, int tag = 0) {
			HitInfo result = null;
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
						if (cell.Frame != CurrentFrame) { break; }
						if (ignore != null && cell.Entity == ignore) { continue; }
						if (tag != 0 && cell.Tag != tag) { continue; }
						if ((cell.IsTrigger && useTrigger) || (!cell.IsTrigger && useCollider)) {
							if (cell.Rect.Overlaps(globalRect)) {
								return cell.GetInfo();
							}
						}
					}
				}
			}
			return result;
		}


		public static void ForAllOverlaps (PhysicsLayer layer, RectInt globalRect, System.Func<HitInfo, bool> func) {
			var layerItem = Layers[(int)layer];
			int l = Mathf.Max(globalRect.xMin.GetCellIndexX() - 1, 0);
			int d = Mathf.Max(globalRect.yMin.GetCellIndexY() - 1, 0);
			int r = Mathf.Min((globalRect.xMax - 1).GetCellIndexX() + 1, Width - 1);
			int u = Mathf.Min((globalRect.yMax - 1).GetCellIndexY() + 1, Height - 1);
			for (int j = d; j <= u; j++) {
				for (int i = l; i <= r; i++) {
					for (int dep = 0; dep < CELL_DEPTH; dep++) {
						var cell = layerItem[i, j, dep];
						if (cell.Frame != CurrentFrame) { break; }
						if (cell.Rect.Overlaps(globalRect)) {
							if (!func(cell.GetInfo())) { return; }
						}
					}
				}
			}
		}


		// Move
		public static bool Move (PhysicsLayer layer, Vector2Int from, Vector2Int to, Vector2Int size, Entity entity, out Vector2Int result) =>
			Move(layer, from, to, size, entity, out result, out _);


		public static bool Move (PhysicsLayer layer, Vector2Int from, Vector2Int to, Vector2Int size, Entity entity, out Vector2Int result, out Direction2 hitDirection) {
			var _result = result = to;
			int distance = int.MaxValue;
			bool success = false;
			Direction2 _direction = default;
			Direction2 _velDir = Mathf.Abs((from - to).x) > Mathf.Abs((from - to).y) ? Direction2.Horizontal : Direction2.Vertical;
			int push = entity != null ? 0 : int.MaxValue;
			if (entity is eRigidbody rig) {
				push = rig.PushLevel;
			}
			ForAllOverlaps(layer, new RectInt(to, size), (info) => {
				if (entity != null && info.Entity == entity) { return true; }
				if (info.IsTrigger) { return true; }
				int hitPush = info.Entity != null ? 0 : int.MaxValue;
				if (info.Entity is eRigidbody hitRig) {
					hitPush = hitRig.PushLevel;
				}



				var _hitRect = info.Rect;
				var _hitCenter = _hitRect.center.RoundToInt();
				var _posH = new Vector2Int(
					to.x + size.x < _hitCenter.x ? _hitRect.x - size.x : _hitRect.x + _hitRect.width,
					to.y
				);
				var _posV = new Vector2Int(
					to.x,
					to.y + size.y < _hitCenter.y ? _hitRect.y - size.y : _hitRect.y + _hitRect.height
				);
				// Overlap Check
				bool hHit = Overlap(layer, new RectInt(_posH, size), entity) != null;
				bool vHit = Overlap(layer, new RectInt(_posV, size), entity) != null;
				Vector2Int _pos;
				if (hHit != vHit) {
					_pos = hHit ? _posV : _posH;
					_direction = hHit ? Direction2.Vertical : Direction2.Horizontal;
				} else {
					// Select by Distance with "from"
					if (Util.SqrtDistance(from, _posH) < Util.SqrtDistance(from, _posV)) {
						_pos = _posH;
						_direction = Direction2.Horizontal;
					} else {
						_pos = _posV;
						_direction = Direction2.Vertical;
					}
				}
				int _dis = Util.SqrtDistance(from, _pos);
				if (_dis < distance) {
					distance = _dis;
					_result = _pos;
				}
				success = true;
				return true;
			});
			result = _result;
			hitDirection = _direction;
			return success;
		}


		public static Vector2Int Push (
			PhysicsLayer layer, RectInt heavy,
			Vector2Int lightFrom, Vector2Int lightTo, Vector2Int lightSize, Entity lightEntity,
			out Direction2 direction
		) {

			var _hitCenter = heavy.center.RoundToInt();
			var _posH = new Vector2Int(
				lightTo.x + lightSize.x < _hitCenter.x ? heavy.x - lightSize.x : heavy.x + heavy.width,
				lightTo.y
			);
			var _posV = new Vector2Int(
				lightTo.x,
				lightTo.y + lightSize.y < _hitCenter.y ? heavy.y - lightSize.y : heavy.y + heavy.height
			);

			// Overlap Check
			bool hHit = Overlap(layer, new RectInt(_posH, lightSize), lightEntity) != null;
			bool vHit = Overlap(layer, new RectInt(_posV, lightSize), lightEntity) != null;

			Vector2Int _pos;
			if (hHit != vHit) {
				_pos = hHit ? _posV : _posH;
				direction = hHit ? Direction2.Vertical : Direction2.Horizontal;
			} else {
				// Select by Distance with "from"
				if (Util.SqrtDistance(lightFrom, _posH) < Util.SqrtDistance(lightFrom, _posV)) {
					_pos = _posH;
					direction = Direction2.Horizontal;
				} else {
					_pos = _posV;
					direction = Direction2.Vertical;
				}
			}

			return _pos;
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


		#endregion




	}
}
