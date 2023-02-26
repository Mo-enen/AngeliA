using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;



namespace Yaya {


	public enum NavigationMotion {
		None = 0,
		Move = 1,
		Jump = 2,
		Fly = 3,
	}


	public static class CellNavigation {




		#region --- SUB ---


		public interface IExpandRangeValidator {
			public bool Verify (int cellX, int cellY);
		}


		private class NavigationStartInAirValidator : IExpandRangeValidator {
			public static readonly NavigationStartInAirValidator Instance = new();
			public int CellFromX;
			public int CellFromY;
			public int CellRangeX;
			public int CellRangeY;
			public bool Verify (int cellX, int cellY) =>
				cellY <= CellFromY &&
				cellY >= CellFromY - CellRangeY &&
				Mathf.Abs(cellX - CellFromX) * CellRangeY <= Mathf.Abs(cellY - CellFromY) * CellRangeX;
		}


		private enum BlockType { Air, Liquid, Solid, }


		public struct Operation {
			public NavigationMotion Motion;
			public int TargetGlobalX;
			public int TargetGlobalY;
		}


		private struct Cell {

			public static readonly Cell EMPTY = new() {
				BlockStamp = -1,
				OperationStamp = 0,
				Operation = {
					Motion = NavigationMotion.None,
				},
			};

			// Operation
			public bool OperationDataValid {
				get => OperationStamp == CellNavigation.OperationStamp;
				set => OperationStamp = value ? CellNavigation.OperationStamp : 0;
			}
			public Operation Operation;
			private uint OperationStamp;
			public int Distance;
			public int FromCellX;
			public int FromCellY;
			public int ToCellX;
			public int ToCellY;

			// Block
			public bool BlockDataValid {
				get => BlockStamp == Game.GlobalFrame;
				set => BlockStamp = value ? Game.GlobalFrame : -1;
			}
			public bool IsSolid => BlockType == BlockType.Solid;
			public bool IsAir => BlockType == BlockType.Air;
			public bool IsLiquid => BlockType == BlockType.Liquid;

			public BlockType BlockType;
			public bool IsBlockedLeft;
			public bool IsBlockedRight;
			public bool IsBlockedDown;
			public bool IsBlockedUp;
			public int PlatformY;
			private int BlockStamp;

		}


		#endregion




		#region --- VAR ---


		// Api
		internal static int CellWidth { get; private set; } = 1;
		internal static int CellHeight { get; private set; } = 1;

		// Data
		private static Cell[,] Cells = null;
		private static Queue<Vector3Int> ExpandQueue = null;
		private static Queue<Vector3Int> ExpandQueueJump = null;
		private static uint OperationStamp = 0;
		private static int CachedFrame = int.MinValue;
		private static int CellUnitOffsetX = 0;
		private static int CellUnitOffsetY = 0;


		#endregion




		#region --- API ---


		[AfterGameInitialize]
		public static void Initialize () {
			var viewConfig = Game.Current.ViewConfig;
			CellWidth = (viewConfig.ViewRatio * viewConfig.MaxHeight / 1000 + Const.SPAWN_PADDING * 2) / Const.CEL + Const.LEVEL_SPAWN_PADDING_UNIT * 2;
			CellHeight = (viewConfig.MaxHeight + Const.SPAWN_PADDING * 2) / Const.CEL + Const.LEVEL_SPAWN_PADDING_UNIT * 2;
			Cells = new Cell[CellWidth, CellHeight];
			for (int i = 0; i < CellWidth; i++) {
				for (int j = 0; j < CellHeight; j++) {
					Cells[i, j] = Cell.EMPTY;
				}
			}
			ExpandQueue = new(CellWidth * CellHeight + 1);
			ExpandQueueJump = new(CellWidth * CellHeight + 1);
		}


		public static int NavigateTo (
			in Operation[] Operations,
			int fromX, int fromY, int toX, int toY,
			int jumpRangeX, int jumpRangeY,
			int maxIteration = int.MaxValue
		) {

			if (Operations == null || Operations.Length == 0) return 0;

			RefreshFrameCache();

			int fromCellX = (fromX.UDivide(Const.CEL) - CellUnitOffsetX).Clamp(0, CellWidth - 1);
			int fromCellY = (fromY.UDivide(Const.CEL) - CellUnitOffsetY).Clamp(0, CellHeight - 1);
			int operationCount = 0;
			int finalDistanceSq = int.MaxValue;
			int finalCellX = -1;
			int finalCellY = -1;

			// Start in Air
			if (!IsGroundCell(fromCellX, fromCellY, fromY, out _)) {
				var val = NavigationStartInAirValidator.Instance;
				val.CellFromX = fromCellX;
				val.CellFromY = fromCellY;
				val.CellRangeX = 3;
				val.CellRangeY = 6;
				if (ExpandTo(
					fromX, fromY, fromX, fromY,
					maxIteration, out int _groundX, out int _groundY,
					endInAir: false,
					rangeValidator: val
				)) {
					// Add Drop Operation
					Operations[0] = new Operation() {
						Motion = NavigationMotion.Jump,
						TargetGlobalX = _groundX,
						TargetGlobalY = _groundY,
					};
					operationCount = 1;
					// Redirect From Position
					fromX = _groundX;
					fromY = _groundY;
					fromCellX = (fromX.UDivide(Const.CEL) - CellUnitOffsetX).Clamp(0, CellWidth - 1);
					fromCellY = (fromY.UDivide(Const.CEL) - CellUnitOffsetY).Clamp(0, CellHeight - 1);
				}
			}

			Operations[operationCount] = new Operation() {
				Motion = NavigationMotion.Jump,
				TargetGlobalX = toX,
				TargetGlobalY = toY,
			};
			operationCount++;
			return operationCount;

			// Nav Expand
			int toCellX = (toX.UDivide(Const.CEL) - CellUnitOffsetX).Clamp(0, CellWidth - 1);
			int toCellY = (toY.UDivide(Const.CEL) - CellUnitOffsetY).Clamp(0, CellHeight - 1);
			bool rightFirst = fromX.UMod(Const.CEL) > Const.HALF;
			bool upFirst = fromY.UMod(Const.CEL) > Const.HALF;

			OperationStamp++;
			ExpandQueue.Clear();
			ExpandQueueJump.Clear();
			Enqueue(ExpandQueue, fromCellX, fromCellY, 0, fromCellX, fromCellY, 0);

			// Expand
			while (ExpandQueue.Count > 0) {
				ExpandMovement();
				ExpandJump();

				// 到达终点也不要马上跳出
				// 尘埃落定才能跳出

			}

			// Func
			void ExpandMovement () {
				while (ExpandQueue.Count > 0) {
					var pos = ExpandQueue.Dequeue();







				}
			}


			void ExpandJump () {
				while (ExpandQueueJump.Count > 0) {
					var pos = ExpandQueueJump.Dequeue();





				}
			}

			// Backward Trace
			bool traceSuccess = false;
			if (
				finalCellX.InRange(0, CellWidth - 1) &&
				finalCellY.InRange(0, CellHeight - 1)
			) {
				int safeCount = CellWidth * CellHeight;
				int currentCellX = finalCellX;
				int currentCellY = finalCellY;
				ref var cell = ref Cells[finalCellX, finalCellY];
				ref Cell prevCell = ref cell;
				cell.ToCellX = finalCellX;
				cell.ToCellY = finalCellY;
				if (cell.OperationDataValid) {
					for (int safe = 0; safe < safeCount; safe++) {
						int prevX = cell.FromCellX;
						int prevY = cell.FromCellY;
						prevCell = ref Cells[prevX, prevY];
						if (!prevCell.OperationDataValid) break;
						ref var prevOp = ref prevCell.Operation;
						prevOp.TargetGlobalX = (currentCellX + CellUnitOffsetX) * Const.CEL + Const.HALF;
						prevOp.TargetGlobalY = (currentCellY + CellUnitOffsetY) * Const.CEL;
						prevCell.ToCellX = currentCellX;
						prevCell.ToCellY = currentCellY;
						currentCellX = prevY;
						currentCellY = prevY;
						cell.OperationDataValid = false;
						if (prevCell.FromCellX == prevX && prevCell.FromCellY == prevY) {
							traceSuccess = prevX == fromCellX && prevY == fromCellY;
							break;
						}
						cell = prevCell;
					}
				}
			}

			// Fill Operation Result
			if (traceSuccess) {
				ref var cell = ref Cells[fromCellX, fromCellY];
				for (int index = operationCount; index < Operations.Length; index++) {
					Operations[index] = cell.Operation;
					operationCount = index + 1;
					if (cell.ToCellX == finalCellX && cell.ToCellY == finalCellY) break;
					cell = Cells[cell.ToCellX, cell.ToCellY];
				}
			}

			// Fly At End Check
			if (finalDistanceSq != 0 && finalDistanceSq != int.MaxValue && operationCount < Operations.Length) {
				Operations[operationCount] = new Operation() {
					Motion = NavigationMotion.Fly,
					TargetGlobalX = toX,
					TargetGlobalY = toY,
				};
				operationCount++;
			}

			return operationCount;

			// Func
			static void Enqueue (
				Queue<Vector3Int> _queue,
				int _cellX, int _cellY, int _iteration,
				int _fromX, int _fromY, int _distance
			) {
				_queue.Enqueue(new Vector3Int(_cellX, _cellY, _iteration));
				ref var cell = ref Cells[_cellX, _cellY];
				cell.OperationDataValid = true;
				cell.FromCellX = _fromX;
				cell.FromCellY = _fromY;
				cell.Distance = _distance;
			}
		}


		public static bool ExpandTo (
			int fromX, int fromY, int toX, int toY, int maxIteration,
			out int resultX, out int resultY,
			bool endInAir = false, IExpandRangeValidator rangeValidator = null
		) {

			RefreshFrameCache();
			OperationStamp++;
			resultX = fromX;
			resultY = fromY;

			int fromCellX = (fromX.UDivide(Const.CEL) - CellUnitOffsetX).Clamp(0, CellWidth - 1);
			int fromCellY = (fromY.UDivide(Const.CEL) - CellUnitOffsetY).Clamp(0, CellHeight - 1);
			int toCellX = (toX.UDivide(Const.CEL) - CellUnitOffsetX).Clamp(0, CellWidth - 1);
			int toCellY = (toY.UDivide(Const.CEL) - CellUnitOffsetY).Clamp(0, CellHeight - 1);
			int minSquareDis = int.MaxValue;

			ExpandQueue.Clear();
			ExpandQueue.Enqueue(new Vector3Int(fromCellX, fromCellY, 0));
			Cells[fromCellX, fromCellY].OperationDataValid = true;
			bool rightFirst = fromX.UMod(Const.CEL) > Const.HALF;
			bool upFirst = fromY.UMod(Const.CEL) > Const.HALF;

			while (ExpandQueue.Count > 0) {

				var pos = ExpandQueue.Dequeue();

				// Check Success
				int _globalY = (pos.y + CellUnitOffsetY) * Const.CEL + Const.HALF;
				int _resultY = _globalY;
				if (endInAir || IsGroundCell(pos.x, pos.y, _globalY, out _resultY)) {
					int sqDis = Util.SquareDistance(toCellX, toCellY, pos.x, pos.y);
					if (sqDis < minSquareDis) {
						minSquareDis = sqDis;
						resultX = (pos.x + CellUnitOffsetX) * Const.CEL + Const.HALF;
						resultY = _resultY;
						if (sqDis == 0) return true;
					}
				}

				// Expand
				if (pos.z < maxIteration) {
					if (rightFirst) {
						ExpandR();
						ExpandL();
					} else {
						ExpandL();
						ExpandR();
					}

					if (upFirst) {
						ExpandU();
						ExpandD();
					} else {
						ExpandD();
						ExpandU();
					}
				}

				// Func
				void ExpandL () {
					if (
						pos.x - 1 >= 0 &&
						!Cells[pos.x - 1, pos.y].OperationDataValid &&
						!GetBlockedData(pos.x - 1, pos.y).IsBlockedRight &&
						(rangeValidator == null || rangeValidator.Verify(pos.x - 1, pos.y))
					) {
						ExpandQueue.Enqueue(new Vector3Int(pos.x - 1, pos.y, pos.z + 1));
						Cells[pos.x - 1, pos.y].OperationDataValid = true;
					}
				}
				void ExpandR () {
					if (
						pos.x + 1 < CellWidth &&
						!Cells[pos.x + 1, pos.y].OperationDataValid &&
						!GetBlockedData(pos.x + 1, pos.y).IsBlockedLeft &&
						(rangeValidator == null || rangeValidator.Verify(pos.x + 1, pos.y))
					) {
						ExpandQueue.Enqueue(new Vector3Int(pos.x + 1, pos.y, pos.z + 1));
						Cells[pos.x + 1, pos.y].OperationDataValid = true;
					}
				}
				void ExpandD () {
					if (
						pos.y - 1 >= 0 &&
						!Cells[pos.x, pos.y - 1].OperationDataValid &&
						!GetBlockedData(pos.x, pos.y - 1).IsBlockedUp &&
						(rangeValidator == null || rangeValidator.Verify(pos.x, pos.y - 1))
					) {
						ExpandQueue.Enqueue(new Vector3Int(pos.x, pos.y - 1, pos.z + 1));
						Cells[pos.x, pos.y - 1].OperationDataValid = true;
					}
				}
				void ExpandU () {
					if (
						pos.y + 1 < CellHeight &&
						!Cells[pos.x, pos.y + 1].OperationDataValid &&
						!GetBlockedData(pos.x, pos.y + 1).IsBlockedDown &&
						(rangeValidator == null || rangeValidator.Verify(pos.x, pos.y + 1))
					) {
						ExpandQueue.Enqueue(new Vector3Int(pos.x, pos.y + 1, pos.z + 1));
						Cells[pos.x, pos.y + 1].OperationDataValid = true;
					}
				}
			}
			return minSquareDis != int.MaxValue;
		}


		public static bool IsGround (int globalX, int globalY, out int groundY) {
			RefreshFrameCache();
			groundY = globalY;
			int cellX = globalX.UDivide(Const.CEL) - CellUnitOffsetX;
			if (cellX.InRange(0, CellWidth - 1)) {
				int cellY = globalY.UDivide(Const.CEL) - CellUnitOffsetY;
				if (cellY.InRange(0, CellHeight - 1)) {
					return IsGroundCell(cellX, cellY, globalY, out groundY);
				}
			}
			return false;
		}


		#endregion




		#region --- LGC ---


		private static void RefreshFrameCache () {
			if (CachedFrame == Game.GlobalFrame) return;
			CachedFrame = Game.GlobalFrame;
			CellUnitOffsetX = (Game.Current.ViewRect.x - Const.LEVEL_SPAWN_PADDING_UNIT * Const.CEL).UDivide(Const.CEL);
			CellUnitOffsetY = (Game.Current.ViewRect.y - Const.LEVEL_SPAWN_PADDING_UNIT * Const.CEL).UDivide(Const.CEL);
		}


		private static Cell GetBlockedData (int cellX, int cellY) {

			ref var cell = ref Cells[cellX, cellY];
			if (cell.BlockDataValid) return cell;
			int x = (cellX + CellUnitOffsetX) * Const.CEL;
			int y = (cellY + CellUnitOffsetY) * Const.CEL;
			var blockRect = new RectInt(x, y, Const.CEL, Const.CEL);
			var centerRect = new RectInt(x + Const.HALF, y + Const.HALF, 1, 1);
			bool solid = CellPhysics.Overlap(
				YayaConst.MASK_MAP, centerRect, out var info
			);
			cell.BlockDataValid = true;
			cell.BlockType = solid ? BlockType.Solid : BlockType.Air;

			if (solid) {
				// Solid Block
				cell.IsBlockedLeft = true;
				cell.IsBlockedRight = true;
				cell.IsBlockedDown = true;
				cell.IsBlockedUp = true;
				cell.PlatformY = info.Rect.yMax;
			} else {
				int platformY = y + Const.CEL;
				// Liquid Check
				if (CellPhysics.Overlap(YayaConst.MASK_MAP, centerRect, null, OperationMode.TriggerOnly, YayaConst.WATER_TAG)) {
					cell.BlockType = BlockType.Liquid;
					platformY = y + Const.HALF;
				}
				// Oneway
				cell.IsBlockedLeft = OnewaySolid(Direction4.Left);
				cell.IsBlockedRight = OnewaySolid(Direction4.Right);
				cell.IsBlockedDown = OnewaySolid(Direction4.Down);
				cell.IsBlockedUp = OnewaySolid(Direction4.Up);
				cell.PlatformY = platformY;
				// Func
				bool OnewaySolid (Direction4 gateDirection) {
					bool hitted = CellPhysics.Overlap(
						YayaConst.MASK_MAP,
						blockRect, out var hit,
						null, OperationMode.TriggerOnly,
						AngeUtil.GetOnewayTag(gateDirection)
					);
					if (hitted && gateDirection == Direction4.Up) {
						platformY = hit.Rect.yMax;
					}
					return hitted;
				}
			}
			return cell;
		}


		private static bool IsGroundCell (int cellX, int cellY, int globalY, out int groundY) {

			var cell = GetBlockedData(cellX, cellY);
			groundY = cell.PlatformY;

			// Check Liquid
			if (cell.IsLiquid) return true;

			bool isGround = false;

			// Check Block Down
			if (!cell.IsSolid && cellY - 1 >= 0) {
				var cellD = GetBlockedData(cellX, cellY - 1);
				if (cellD.IsBlockedUp) {
					groundY = cellD.PlatformY;
					isGround = true;
				}
			}

			// Check This Block
			if (
				cell.IsBlockedUp &&
				(cellY + 1 >= CellHeight || !GetBlockedData(cellX, cellY + 1).IsSolid)
			) {
				if (!isGround || cell.PlatformY < globalY) {
					groundY = cell.PlatformY;
				}
				isGround = true;
			}

			return isGround;
		}


		#endregion




	}
}