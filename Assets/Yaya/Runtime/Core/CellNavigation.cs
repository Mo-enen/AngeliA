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



		private class BlockCell {

			public bool BlockDataValid {
				get => BlockStamp == Game.GlobalFrame;
				set => BlockStamp = value ? Game.GlobalFrame : -1;
			}

			public BlockType BlockType = BlockType.Air;
			public bool IsBlockedLeft = false;
			public bool IsBlockedRight = false;
			public bool IsBlockedDown = false;
			public bool IsBlockedUp = false;
			public int PlatformY = 0;

			private int BlockStamp = -1;

		}


		private class OperationCell {

			public bool OperationDataValid {
				get => OperationStamp == CellNavigation.OperationStamp;
				set => OperationStamp = value ? CellNavigation.OperationStamp : 0;
			}

			public NavigationMotion FromMotion = NavigationMotion.None;
			public int FromCellX = 0;
			public int FromCellY = 0;
			public int ToCellX = 0;
			public int ToCellY = 0;

			private uint OperationStamp = 0;

		}


		#endregion




		#region --- VAR ---


		// Api
		internal static int CellWidth { get; private set; } = 1;
		internal static int CellHeight { get; private set; } = 1;

		// Data
		private static OperationCell[,] OperationCells = null;
		private static BlockCell[,] BlockCells = null;
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
			ExpandQueue = new(CellWidth * CellHeight + 1);
			ExpandQueueJump = new(CellWidth * CellHeight + 1);
			// Operation
			OperationCells = new OperationCell[CellWidth, CellHeight];
			for (int i = 0; i < CellWidth; i++) {
				for (int j = 0; j < CellHeight; j++) {
					OperationCells[i, j] = new();
				}
			}
			// Block
			BlockCells = new BlockCell[CellWidth, CellHeight];
			for (int i = 0; i < CellWidth; i++) {
				for (int j = 0; j < CellHeight; j++) {
					BlockCells[i, j] = new();
				}
			}
		}



		// Navigate
		public static int NavigateTo (
			in Operation[] Operations,
			int fromX, int fromY, int toX, int toY,
			int jumpIteration = 12
		) {

			if (Operations == null || Operations.Length == 0) return 0;

			RefreshFrameCache();

			int fromCellX = (fromX.UDivide(Const.CEL) - CellUnitOffsetX).Clamp(0, CellWidth - 1);
			int fromCellY = (fromY.UDivide(Const.CEL) - CellUnitOffsetY).Clamp(0, CellHeight - 1);
			int operationCount = 0;
			int finalDistance = int.MaxValue;
			int finalCellX = -1;
			int finalCellY = -1;

			// Start in Air
			if (!IsGroundCell(fromCellX, fromCellY, out _)) {
				var val = NavigationStartInAirValidator.Instance;
				val.CellFromX = fromCellX;
				val.CellFromY = fromCellY;
				val.CellRangeX = 3;
				val.CellRangeY = 6;
				if (ExpandTo(
					fromX, fromY, fromX, fromY,
					Operations.Length, out int _groundX, out int _groundY,
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
				} else return 0;
			}

			// Nav Expand
			int toCellX = (toX.UDivide(Const.CEL) - CellUnitOffsetX).Clamp(0, CellWidth - 1);
			int toCellY = (toY.UDivide(Const.CEL) - CellUnitOffsetY).Clamp(0, CellHeight - 1);

			OperationStamp++;
			ExpandQueue.Clear();
			ExpandQueueJump.Clear();

			// First Enqueue
			ExpandQueue.Enqueue(new Vector3Int(fromCellX, fromCellY, 0));
			var firstCell = OperationCells[fromCellX, fromCellY];
			firstCell.OperationDataValid = true;
			firstCell.FromCellX = fromCellX;
			firstCell.FromCellY = fromCellY;
			firstCell.FromMotion = NavigationMotion.None;

			// Expand
			while (ExpandQueue.Count > 0) {
				Navigate_ExpandGroundLogic(
					Operations.Length, toCellX, toCellY,
					ref finalCellX, ref finalCellY, ref finalDistance
				);
				if (finalDistance != 0) {
					Navigate_ExpandJumpLogic(jumpIteration);
				}
			}
			ExpandQueue.Clear();
			ExpandQueueJump.Clear();

			// Backward Trace
			bool traceSuccess = false;
			if (
				finalCellX.InRange(0, CellWidth - 1) &&
				finalCellY.InRange(0, CellHeight - 1)
			) {
				int safeCount = CellWidth * CellHeight;
				int currentCellX = finalCellX;
				int currentCellY = finalCellY;
				var cell = OperationCells[finalCellX, finalCellY];
				OperationCell prevCell;
				cell.ToCellX = finalCellX;
				cell.ToCellY = finalCellY;
				if (cell.OperationDataValid) {
					for (int safe = 0; safe < safeCount; safe++) {
						prevCell = OperationCells[cell.FromCellX, cell.FromCellY];
						if (!prevCell.OperationDataValid) break;
						prevCell.ToCellX = currentCellX;
						prevCell.ToCellY = currentCellY;
						cell.OperationDataValid = false;
						if (prevCell.FromCellX == cell.FromCellX && prevCell.FromCellY == cell.FromCellY) {
							traceSuccess = cell.FromCellX == fromCellX && cell.FromCellY == fromCellY;
							break;
						}
						// Current >> Prev
						currentCellX = cell.FromCellX;
						currentCellY = cell.FromCellY;
						cell = prevCell;
					}
				}
			}

			// Fill Operation Result
			if (traceSuccess) {
				var cell = OperationCells[fromCellX, fromCellY];
				int operatingGroundY = fromY;
				for (int index = operationCount; index < Operations.Length; index++) {
					// Fix Global Ground Y
					int targetGlobalY = (cell.ToCellY + CellUnitOffsetY) * Const.CEL;
					var blockCell = GetBlockCell(cell.ToCellX, cell.ToCellY);
					if (blockCell.IsBlockedUp && blockCell.PlatformY <= operatingGroundY) {
						targetGlobalY = blockCell.PlatformY;
					}
					operatingGroundY = targetGlobalY;
					// Set Target Global Pos
					var toCell = OperationCells[cell.ToCellX, cell.ToCellY];
					Operations[index] = new Operation() {
						Motion = toCell.FromMotion,
						TargetGlobalX = (cell.ToCellX + CellUnitOffsetX) * Const.CEL + Const.HALF,
						TargetGlobalY = targetGlobalY,
					};
					operationCount = index + 1;
					if (cell.ToCellX == finalCellX && cell.ToCellY == finalCellY) break;
					// to Next
					cell = toCell;
				}
			}

			// Fly At End Check
			if (finalDistance != 0 && finalDistance != int.MaxValue && operationCount < Operations.Length) {
				Operations[operationCount] = new Operation() {
					Motion = NavigationMotion.Fly,
					TargetGlobalX = toX,
					TargetGlobalY = toY,
				};
				operationCount++;
			}

			return operationCount;
		}



		// Expand
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
			OperationCells[fromCellX, fromCellY].OperationDataValid = true;
			bool rightFirst = fromX.UMod(Const.CEL) > Const.HALF;
			bool upFirst = fromY.UMod(Const.CEL) > Const.HALF;

			while (ExpandQueue.Count > 0) {

				var pos = ExpandQueue.Dequeue();

				// Check Success
				int _globalY = (pos.y + CellUnitOffsetY) * Const.CEL + Const.HALF;
				int _resultY = _globalY;
				if (endInAir || IsGroundCell(pos.x, pos.y, out _resultY, true)) {
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
					if (pos.x - 1 < 0) return;
					var _cell = OperationCells[pos.x - 1, pos.y];
					if (
						!_cell.OperationDataValid &&
						!GetBlockCell(pos.x - 1, pos.y).IsBlockedRight &&
						(rangeValidator == null || rangeValidator.Verify(pos.x - 1, pos.y))
					) {
						ExpandQueue.Enqueue(new Vector3Int(pos.x - 1, pos.y, pos.z + 1));
						_cell.OperationDataValid = true;
					}
				}
				void ExpandR () {
					if (pos.x + 1 >= CellWidth) return;
					var _cell = OperationCells[pos.x + 1, pos.y];
					if (
						!_cell.OperationDataValid &&
						!GetBlockCell(pos.x + 1, pos.y).IsBlockedLeft &&
						(rangeValidator == null || rangeValidator.Verify(pos.x + 1, pos.y))
					) {
						ExpandQueue.Enqueue(new Vector3Int(pos.x + 1, pos.y, pos.z + 1));
						_cell.OperationDataValid = true;
					}
				}
				void ExpandD () {
					if (pos.y - 1 < 0) return;
					var _cell = OperationCells[pos.x, pos.y - 1];
					if (
						!_cell.OperationDataValid &&
						!GetBlockCell(pos.x, pos.y - 1).IsBlockedUp &&
						(rangeValidator == null || rangeValidator.Verify(pos.x, pos.y - 1))
					) {
						ExpandQueue.Enqueue(new Vector3Int(pos.x, pos.y - 1, pos.z + 1));
						_cell.OperationDataValid = true;
					}
				}
				void ExpandU () {
					if (pos.y + 1 >= CellHeight) return;
					var _cell = OperationCells[pos.x, pos.y + 1];
					if (
						!_cell.OperationDataValid &&
						!GetBlockCell(pos.x, pos.y + 1).IsBlockedDown &&
						(rangeValidator == null || rangeValidator.Verify(pos.x, pos.y + 1))
					) {
						ExpandQueue.Enqueue(new Vector3Int(pos.x, pos.y + 1, pos.z + 1));
						_cell.OperationDataValid = true;
					}
				}
			}
			return minSquareDis != int.MaxValue;
		}



		// Ground
		public static bool IsGround (int globalX, int globalY, out int groundY) {
			RefreshFrameCache();
			groundY = globalY;
			int cellX = globalX.UDivide(Const.CEL) - CellUnitOffsetX;
			if (cellX.InRange(0, CellWidth - 1)) {
				int cellY = globalY.UDivide(Const.CEL) - CellUnitOffsetY;
				if (cellY.InRange(0, CellHeight - 1)) {
					return IsGroundCell(cellX, cellY, out groundY, true);
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


		private static BlockCell GetBlockCell (int cellX, int cellY) {

			var cell = BlockCells[cellX, cellY];
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


		private static bool IsGroundCell (int cellX, int cellY, out int groundY, bool dropDownForHalf = false) {

			var cell = GetBlockCell(cellX, cellY);
			groundY = cell.PlatformY;

			// Check Liquid
			if (cell.BlockType == BlockType.Liquid) return true;

			bool isGround = false;

			// Check Block Down
			if (cell.BlockType != BlockType.Solid && cellY - 1 >= 0) {
				var cellD = GetBlockCell(cellX, cellY - 1);
				if (cellD.IsBlockedUp) {
					groundY = cellD.PlatformY;
					isGround = true;
				}
			}

			// Check This Block
			if (
				cell.IsBlockedUp &&
				(cellY + 1 >= CellHeight || GetBlockCell(cellX, cellY + 1).BlockType != BlockType.Solid)
			) {
				if (
					!isGround || !dropDownForHalf ||
					cell.PlatformY < (cellY + CellUnitOffsetY) * Const.CEL + Const.HALF
				) {
					groundY = cell.PlatformY;
					isGround = true;
				}
			}

			return isGround;
		}


		// Navigate Logic
		private static void Navigate_ExpandGroundLogic (
			int maxIteration, int toCellX, int toCellY,
			ref int finalCellX, ref int finalCellY, ref int finalDistance
		) {
			while (ExpandQueue.Count > 0) {

				var pos = ExpandQueue.Dequeue();
				int dis = Util.SquareDistance(pos.x, pos.y, toCellX, toCellY);
				if (dis < finalDistance) {
					finalDistance = dis;
					finalCellX = pos.x;
					finalCellY = pos.y;
					if (dis == 0) {
						ExpandQueue.Clear();
						break;
					}
				}

				// Expand
				if (pos.z < maxIteration) {
					TryExpand(pos.x - 1, pos.y);
					TryExpand(pos.x + 1, pos.y);
				}
				// Func
				void TryExpand (int _x, int _y) {

					// Range Check
					if (!_x.InRange(0, CellWidth - 1) || !_y.InRange(0, CellHeight - 1)) return;
					var _cell = OperationCells[_x, _y];

					// Valid Check
					if (_cell.OperationDataValid) return;

					// Blocked Check
					var blockData = GetBlockCell(_x, _y);
					if (_x > pos.x ? blockData.IsBlockedLeft : blockData.IsBlockedRight) return;

					// is Ground Check
					if (!IsGroundCell(_x, _y, out _)) return;

					// Enqueue for Move
					ExpandQueue.Enqueue(new Vector3Int(_x, _y, pos.z + 1));
					_cell.OperationDataValid = true;
					_cell.FromCellX = pos.x;
					_cell.FromCellY = pos.y;
					_cell.FromMotion = NavigationMotion.Move;

					// Push for Jump
					ExpandQueueJump.Enqueue(new Vector3Int(_x, _y, 0));

				}
			}
		}



		private static void Navigate_ExpandJumpLogic (int jumpIteration) {
			while (ExpandQueueJump.Count > 0) {

				var pos = ExpandQueueJump.Dequeue();

				// Expand
				if (pos.z < jumpIteration) {
					TryExpand(pos.x, pos.y + 1);
					TryExpand(pos.x - 1, pos.y);
					TryExpand(pos.x + 1, pos.y);
					TryExpand(pos.x, pos.y - 1);
				}
				// Func
				void TryExpand (int _x, int _y) {

					// Range Check
					if (!_x.InRange(0, CellWidth - 1) || !_y.InRange(0, CellHeight - 1)) return;
					var _cell = OperationCells[_x, _y];

					// Valid Check
					if (_cell.OperationDataValid) return;

					// Blocked Check
					var blockData = GetBlockCell(_x, _y);
					if (_x > pos.x ? blockData.IsBlockedLeft : blockData.IsBlockedRight) return;
					if (_y > pos.y ? blockData.IsBlockedDown : blockData.IsBlockedUp) return;

					// Ground Check
					if (IsGroundCell(_x, _y, out _)) {
						// Enqueue for Move
						ExpandQueue.Enqueue(new Vector3Int(_x, _y, 0));
						_cell.OperationDataValid = true;
						_cell.FromCellX = pos.x;
						_cell.FromCellY = pos.y;
						_cell.FromMotion = NavigationMotion.Jump;
						return;
					}

					// Solid Check
					if (blockData.BlockType != BlockType.Air) return;

					// Grow Iteration
					int newIteration = pos.z;
					if (_y > pos.y) newIteration += 2;
					if (_x != pos.x) newIteration++;

					// Push
					ExpandQueueJump.Enqueue(new Vector3Int(_x, _y, newIteration));
					_cell.OperationDataValid = true;
					_cell.FromCellX = pos.x;
					_cell.FromCellY = pos.y;
					_cell.FromMotion = NavigationMotion.Jump;

				}
			}
		}



		#endregion




	}
}