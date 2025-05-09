using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;


/// <summary>
/// Type of navigation motion for a single step
/// </summary>
public enum NavigationOperateMotion {
	None = 0,
	Move = 1,
	Jump = 2,
}


/// <summary>
/// Core system for navigate a platformer entity to move naturally
/// </summary>
public static class Navigation {




	#region --- SUB ---


	private enum BlockType { Air, Liquid, Solid, }


	/// <summary>
	/// Data for a single step of navigation operation
	/// </summary>
	public struct Operation {
		/// <summary>
		/// Type of the motion
		/// </summary>
		public NavigationOperateMotion Motion;
		public int TargetGlobalX;
		public int TargetGlobalY;
	}



	private class BlockCell {

		public bool BlockDataValid {
			get => BlockStamp == GlobalFrame;
			set => BlockStamp = value ? GlobalFrame : -1;
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

		public bool OperationValid {
			get => OperationStamp == Navigation.OperationStamp;
			set => OperationStamp = value ? Navigation.OperationStamp : 0;
		}
		public bool OperationValidAlt {
			get => OperationStampAlt == Navigation.OperationStamp;
			set => OperationStampAlt = value ? Navigation.OperationStamp : 0;
		}

		public NavigationOperateMotion FromMotion = NavigationOperateMotion.None;
		public int FromCellX = 0;
		public int FromCellY = 0;
		public int ToCellX = 0;
		public int ToCellY = 0;

		private uint OperationStamp = 0;
		private uint OperationStampAlt = 0;

	}


	#endregion




	#region --- VAR ---


	// Api
	internal static int CellWidth { get; private set; } = 1;
	internal static int CellHeight { get; private set; } = 1;
	/// <summary>
	/// True if the navigation system is ready to use
	/// </summary>
	public static bool IsReady { get; private set; } = false;

	// Data
	private static OperationCell[,] OperationCells = null;
	private static BlockCell[,] BlockCells = null;
	private static Queue<Int3> ExpandQueue = null;
	private static Queue<Int3> ExpandQueueJump = null;
	private static uint OperationStamp = 0;
	private static int CachedFrame = int.MinValue;
	private static int CellUnitOffsetX = 0;
	private static int CellUnitOffsetY = 0;
	private static int FinalCellX;
	private static int FinalCellY;
	private static int FinalDistance;
	private static int GlobalFrame = 0;


	#endregion




	#region --- API ---


	[OnGameInitialize]
	internal static void Initialize () {

		int maxHeigh = Universe.BuiltInInfo.MaxViewHeight;
		CellWidth = Universe.BuiltInInfo.ViewRatio * maxHeigh / 1000 / Const.CEL + Const.SPAWN_PADDING_UNIT * 2 + Const.LEVEL_SPAWN_PADDING_UNIT * 2;
		CellHeight = maxHeigh / Const.CEL + Const.SPAWN_PADDING_UNIT * 2 + Const.LEVEL_SPAWN_PADDING_UNIT * 2;
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
		IsReady = true;
	}


	// Navigate
	/// <summary>
	/// Perform a navigation analysis to find the natural way to walk to target position
	/// </summary>
	/// <param name="Operations">Array to store the result data</param>
	/// <param name="operationCount">How many operations generated</param>
	/// <param name="fromX">Navigate from this position in global space</param>
	/// <param name="fromY">Navigate from this position in global space</param>
	/// <param name="toX">Navigate to this position in global space</param>
	/// <param name="toY">Navigate to this position in global space</param>
	/// <param name="jumpIteration">Iteration limit for jumpping logic</param>
	public static void NavigateTo (
		Operation[] Operations, out int operationCount,
		int fromX, int fromY, int toX, int toY,
		int jumpIteration = 16
	) {

		operationCount = 0;
		if (Operations == null || Operations.Length == 0) return;
		GlobalFrame = Game.GlobalFrame;

		RefreshFrameCache(Stage.ViewRect);

		int fromCellX = (fromX.UDivide(Const.CEL) - CellUnitOffsetX).Clamp(0, CellWidth - 1);
		int fromCellY = (fromY.UDivide(Const.CEL) - CellUnitOffsetY).Clamp(0, CellHeight - 1);
		FinalDistance = int.MaxValue;
		FinalCellX = -1;
		FinalCellY = -1;

		// Start in Air
		if (!IsGroundCell(fromCellX, fromCellY, out _)) {
			if (ExpandTo(
				fromX, fromY, fromX, fromY,
				Operations.Length, out int _groundX, out int _groundY,
				endInAir: false
			)) {
				// Add Drop Operation
				Operations[0] = new Operation() {
					Motion = NavigationOperateMotion.Jump,
					TargetGlobalX = _groundX,
					TargetGlobalY = _groundY,
				};
				operationCount = 1;
				// Redirect From Position
				fromX = _groundX;
				fromY = _groundY;
				fromCellX = (fromX.UDivide(Const.CEL) - CellUnitOffsetX).Clamp(0, CellWidth - 1);
				fromCellY = (fromY.UDivide(Const.CEL) - CellUnitOffsetY).Clamp(0, CellHeight - 1);
			} else return;
		}

		// Nav Expand
		int toCellX = (toX.UDivide(Const.CEL) - CellUnitOffsetX).Clamp(0, CellWidth - 1);
		int toCellY = (toY.UDivide(Const.CEL) - CellUnitOffsetY).Clamp(0, CellHeight - 1);

		OperationStamp++;
		ExpandQueue.Clear();
		ExpandQueueJump.Clear();

		// First Enqueue
		ExpandQueue.Enqueue(new Int3(fromCellX, fromCellY, 0));
		ExpandQueueJump.Enqueue(new Int3(fromCellX, fromCellY, 0));
		var firstCell = OperationCells[fromCellX, fromCellY];
		firstCell.OperationValid = true;
		firstCell.FromCellX = fromCellX;
		firstCell.FromCellY = fromCellY;
		firstCell.FromMotion = NavigationOperateMotion.Move;

		// Expand
		while (ExpandQueue.Count > 0) {
			Navigate_ExpandGroundLogic(Operations.Length, toCellX, toCellY);
			Navigate_ExpandJumpLogic(jumpIteration);
		}
		ExpandQueue.Clear();
		ExpandQueueJump.Clear();

		if (fromCellX == FinalCellX && fromCellY == FinalCellY) {
			operationCount = 0;
			return;
		}

		// Backward Trace
		bool traceSuccess = false;
		if (
			FinalCellX.InRangeInclude(0, CellWidth - 1) &&
			FinalCellY.InRangeInclude(0, CellHeight - 1)
		) {
			int safeCount = CellWidth * CellHeight;
			int currentCellX = FinalCellX;
			int currentCellY = FinalCellY;
			var cell = OperationCells[FinalCellX, FinalCellY];
			OperationCell prevCell;
			cell.ToCellX = FinalCellX;
			cell.ToCellY = FinalCellY;
			if (cell.OperationValid) {
				for (int safe = 0; safe < safeCount; safe++) {
					prevCell = OperationCells[cell.FromCellX, cell.FromCellY];
					if (!prevCell.OperationValid) break;
					prevCell.ToCellX = currentCellX;
					prevCell.ToCellY = currentCellY;
					cell.OperationValid = false;
					if (prevCell == firstCell) {
						traceSuccess = true;
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
			var cell = firstCell;
			int operatingGroundY = fromY;
			for (int index = operationCount; index < Operations.Length; index++) {

				var nextCell = OperationCells[cell.ToCellX, cell.ToCellY];

				// Combine Jump
				const int MAX_COMBINE_COMBO = 6;
				int combineCombo = 0;
				while (nextCell.FromMotion == NavigationOperateMotion.Jump) {
					if (nextCell.ToCellX == cell.ToCellX && nextCell.ToCellY == cell.ToCellY) break;
					var nextNextCell = OperationCells[nextCell.ToCellX, nextCell.ToCellY];
					if (nextNextCell.FromMotion != NavigationOperateMotion.Jump) break;
					if (combineCombo >= MAX_COMBINE_COMBO) {
						var nextBlock = GetBlockCell(cell.ToCellX, cell.ToCellY);
						if (nextBlock.IsBlockedUp) break;
					}
					combineCombo++;
					cell.ToCellX = nextCell.ToCellX;
					cell.ToCellY = nextCell.ToCellY;
					nextCell = nextNextCell;
				}

				// Fix Global Ground Y
				int targetGlobalY = (cell.ToCellY + CellUnitOffsetY) * Const.CEL;
				var blockCell = GetBlockCell(cell.ToCellX, cell.ToCellY);
				if (
					blockCell.IsBlockedUp &&
					blockCell.PlatformY < targetGlobalY + Const.CEL &&
					blockCell.PlatformY <= operatingGroundY
				) {
					targetGlobalY = blockCell.PlatformY;
				} else if (cell.ToCellY - 1 >= 0) {
					var blockCellDown = GetBlockCell(cell.ToCellX, cell.ToCellY - 1);
					if (!blockCellDown.IsBlockedUp) {
						targetGlobalY = blockCell.PlatformY;
					} else if (!blockCell.IsBlockedUp) {
						targetGlobalY = blockCellDown.PlatformY;
					}
				}
				operatingGroundY = targetGlobalY;

				// Set Target Global Pos
				Operations[index] = new Operation() {
					Motion = nextCell.FromMotion,
					TargetGlobalX = (cell.ToCellX + CellUnitOffsetX) * Const.CEL + Const.HALF,
					TargetGlobalY = targetGlobalY,
				};
				operationCount = index + 1;

				// to Next
				if (cell.ToCellX == FinalCellX && cell.ToCellY == FinalCellY) break;
				cell = nextCell;
			}

		}

	}



	// Expand
	/// <summary>
	/// Search for a valid place to stay with
	/// </summary>
	/// <param name="fromX">Search from this position in global space</param>
	/// <param name="fromY">Search from this position in global space</param>
	/// <param name="toX">Search to this position in global space</param>
	/// <param name="toY">Search to this position in global space</param>
	/// <param name="maxIteration">Limitation for searching iteration (Set to 16 if you have no idea how many it should be)</param>
	/// <param name="resultX">Result position to stay with in global space</param>
	/// <param name="resultY">Result position to stay with in global space</param>
	/// <param name="endInAir">True if the target is able to stay in air</param>
	/// <returns>True if the searching performs successfuly</returns>
	public static bool ExpandTo (int fromX, int fromY, int toX, int toY, int maxIteration, out int resultX, out int resultY, bool endInAir = false) => ExpandToInternal(fromX, fromY, toX, toY, maxIteration, out resultX, out resultY, endInAir, false);
	private static bool ExpandToInternal (
		int fromX, int fromY, int toX, int toY, int maxIteration,
		out int resultX, out int resultY, bool endInAir, bool useInAirValidator
	) {

		GlobalFrame = Game.GlobalFrame;
		RefreshFrameCache(Stage.ViewRect);
		OperationStamp++;
		resultX = fromX;
		resultY = fromY;

		int fromCellX = (fromX.UDivide(Const.CEL) - CellUnitOffsetX).Clamp(0, CellWidth - 1);
		int fromCellY = (fromY.UDivide(Const.CEL) - CellUnitOffsetY).Clamp(0, CellHeight - 1);
		int toCellX = (toX.UDivide(Const.CEL) - CellUnitOffsetX).Clamp(0, CellWidth - 1);
		int toCellY = (toY.UDivide(Const.CEL) - CellUnitOffsetY).Clamp(0, CellHeight - 1);
		int minSquareDis = int.MaxValue;

		ExpandQueue.Clear();
		ExpandQueue.Enqueue(new Int3(fromCellX, fromCellY, 0));
		OperationCells[fromCellX, fromCellY].OperationValid = true;
		bool rightFirst = fromX.UMod(Const.CEL) > Const.HALF;
		bool upFirst = fromY.UMod(Const.CEL) > Const.HALF;

		while (ExpandQueue.Count > 0) {

			var pos = ExpandQueue.Dequeue();

			// Check Success
			int _globalY = (pos.y + CellUnitOffsetY) * Const.CEL + Const.HALF;
			int _resultY = _globalY;
			if (endInAir || IsGroundCell(pos.x, pos.y, out _resultY, Const.HALF)) {
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
					!_cell.OperationValid &&
					!GetBlockCell(pos.x - 1, pos.y).IsBlockedRight &&
					(!useInAirValidator || Verify(pos.x - 1, pos.y))
				) {
					ExpandQueue.Enqueue(new Int3(pos.x - 1, pos.y, pos.z + 1));
					_cell.OperationValid = true;
				}
			}
			void ExpandR () {
				if (pos.x + 1 >= CellWidth) return;
				var _cell = OperationCells[pos.x + 1, pos.y];
				if (
					!_cell.OperationValid &&
					!GetBlockCell(pos.x + 1, pos.y).IsBlockedLeft &&
					(!useInAirValidator || Verify(pos.x + 1, pos.y))
				) {
					ExpandQueue.Enqueue(new Int3(pos.x + 1, pos.y, pos.z + 1));
					_cell.OperationValid = true;
				}
			}
			void ExpandD () {
				if (pos.y - 1 < 0) return;
				var _cell = OperationCells[pos.x, pos.y - 1];
				if (
					!_cell.OperationValid &&
					!GetBlockCell(pos.x, pos.y - 1).IsBlockedUp &&
					(!useInAirValidator || Verify(pos.x, pos.y - 1))
				) {
					ExpandQueue.Enqueue(new Int3(pos.x, pos.y - 1, pos.z + 1));
					_cell.OperationValid = true;
				}
			}
			void ExpandU () {
				if (pos.y + 1 >= CellHeight) return;
				var _cell = OperationCells[pos.x, pos.y + 1];
				if (
					!_cell.OperationValid &&
					!GetBlockCell(pos.x, pos.y + 1).IsBlockedDown &&
					(!useInAirValidator || Verify(pos.x, pos.y + 1))
				) {
					ExpandQueue.Enqueue(new Int3(pos.x, pos.y + 1, pos.z + 1));
					_cell.OperationValid = true;
				}
			}
			bool Verify (int cellX, int cellY) =>
				cellX.InRangeInclude(fromCellX - 3, fromCellX + 3) &&
				cellY.InRangeInclude(fromCellY - 6, fromCellY + 6);
		}
		return minSquareDis != int.MaxValue;
	}



	// Ground
	/// <summary>
	/// True if the given location is ground tile for navigation
	/// </summary>
	/// <param name="globalX">Target position in global space</param>
	/// <param name="globalY">Target position in global space</param>
	/// <param name="groundY">Position Y for top edge of the founded ground in global space</param>
	public static bool IsGround (int globalX, int globalY, out int groundY) {
		GlobalFrame = Game.GlobalFrame;
		RefreshFrameCache(Stage.ViewRect);
		groundY = globalY;
		int cellX = globalX.UDivide(Const.CEL) - CellUnitOffsetX;
		if (cellX.InRangeInclude(0, CellWidth - 1)) {
			int cellY = globalY.UDivide(Const.CEL) - CellUnitOffsetY;
			if (cellY.InRangeInclude(0, CellHeight - 1)) {
				return IsGroundCell(cellX, cellY, out groundY, Const.HALF);
			}
		}
		return false;
	}


	#endregion




	#region --- LGC ---


	private static void RefreshFrameCache (IRect viewRect) {
		if (CachedFrame == GlobalFrame) return;
		CachedFrame = GlobalFrame;
		CellUnitOffsetX = (viewRect.CenterX() - CellWidth * Const.HALF).UDivide(Const.CEL);
		CellUnitOffsetY = (viewRect.CenterY() - CellHeight * Const.HALF).UDivide(Const.CEL);
	}


	private static BlockCell GetBlockCell (int cellX, int cellY) {

		var cell = BlockCells[cellX, cellY];
		if (cell.BlockDataValid) return cell;

		int x = (cellX + CellUnitOffsetX) * Const.CEL;
		int y = (cellY + CellUnitOffsetY) * Const.CEL;
		var blockRect = new IRect(x, y, Const.CEL, Const.CEL);
		var centerRect = blockRect.Shrink(16);
		bool solid = Physics.Overlap(
			PhysicsMask.MAP, centerRect, out var info
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
			if (Physics.Overlap(PhysicsMask.MAP, centerRect, null, OperationMode.TriggerOnly, Tag.Water)) {
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
				bool hitted = Physics.Overlap(
					PhysicsMask.MAP,
					blockRect, out var hit,
					null, OperationMode.TriggerOnly,
FrameworkUtil.GetOnewayTag(gateDirection)
				);
				if (hitted && gateDirection == Direction4.Up) {
					platformY = hit.Rect.yMax;
				}
				return hitted;
			}
		}
		return cell;
	}


	private static bool IsGroundCell (int cellX, int cellY, out int groundY, int dropDownY = 0) {

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
				!isGround || dropDownY <= 0 ||
				cell.PlatformY < (cellY + CellUnitOffsetY) * Const.CEL + dropDownY
			) {
				groundY = cell.PlatformY;
				isGround = true;
			}
		}

		return isGround;
	}


	// Navigate Logic
	private static void Navigate_ExpandGroundLogic (int maxIteration, int toCellX, int toCellY) {
		while (ExpandQueue.Count > 0) {

			var pos = ExpandQueue.Dequeue();
			int dis = Util.SquareDistance(pos.x + Const.HALF, pos.y, toCellX + Const.HALF, toCellY);
			if (dis < FinalDistance) {
				FinalDistance = dis;
				FinalCellX = pos.x;
				FinalCellY = pos.y;
				if (dis == 0) {
					ExpandQueue.Clear();
					ExpandQueueJump.Clear();
					return;
				}
			}

			// Expand
			if (pos.z < maxIteration) {
				TryExpand(pos.x - 1, pos.y);
				TryExpand(pos.x + 1, pos.y);
				TryExpand(pos.x, pos.y - 1);
				TryExpand(pos.x, pos.y + 1);
			}
			// Func
			void TryExpand (int _x, int _y) {

				// Range Check
				if (!_x.InRangeInclude(0, CellWidth - 1) || !_y.InRangeInclude(0, CellHeight - 1)) return;
				var _cell = OperationCells[_x, _y];

				// Valid Check
				if (_cell.OperationValid) return;

				// Blocked Check
				var blockData = GetBlockCell(_x, _y);
				if (_x > pos.x ? blockData.IsBlockedLeft : blockData.IsBlockedRight) return;

				// is Ground Check
				if (!IsGroundCell(_x, _y, out _)) return;

				// Water Check
				if (_y != pos.y && blockData.BlockType != BlockType.Liquid) return;

				// Enqueue for Move
				ExpandQueue.Enqueue(new Int3(_x, _y, pos.z + 1));
				ExpandQueueJump.Enqueue(new Int3(_x, _y, 0));
				_cell.OperationValid = true;
				_cell.OperationValidAlt = true;
				_cell.FromCellX = pos.x;
				_cell.FromCellY = pos.y;
				_cell.FromMotion = NavigationOperateMotion.Move;

			}
		}
	}


	private static void Navigate_ExpandJumpLogic (int jumpIteration) {

		while (ExpandQueueJump.Count > 0) {

			var pos = ExpandQueueJump.Dequeue();

			// Expand
			if (pos.z < jumpIteration) {
				bool requireBreak = false;
				requireBreak = !TryExpand(pos.x, pos.y + 1) || requireBreak;
				requireBreak = !TryExpand(pos.x - 1, pos.y) || requireBreak;
				requireBreak = !TryExpand(pos.x + 1, pos.y) || requireBreak;
				requireBreak = !TryExpand(pos.x, pos.y - 1) || requireBreak;
				if (requireBreak) return;
			}
			// Func
			bool TryExpand (int _x, int _y) {

				// Range Check
				if (!_x.InRangeInclude(0, CellWidth - 1) || !_y.InRangeInclude(0, CellHeight - 1)) return true;
				var _cell = OperationCells[_x, _y];

				// Valid Check
				if (_cell.OperationValid || _cell.OperationValidAlt) return true;

				// Blocked Check
				var blockData = GetBlockCell(_x, _y);
				if (_x > pos.x && blockData.IsBlockedLeft) return true;
				if (_x < pos.x && blockData.IsBlockedRight) return true;
				if (_y > pos.y && blockData.IsBlockedDown) return true;

				// Ground Check
				if (IsGroundCell(_x, _y, out _)) {
					// Enqueue for Move
					ExpandQueue.Enqueue(new Int3(_x, _y, 0));
					ExpandQueueJump.Enqueue(new Int3(_x, _y, 0));
					_cell.OperationValid = true;
					_cell.FromCellX = pos.x;
					_cell.FromCellY = pos.y;
					_cell.FromMotion = NavigationOperateMotion.Jump;
					return false;
				}

				// Blocked Up Check
				if (_y < pos.y && blockData.IsBlockedUp) return true;

				// Solid Check
				if (blockData.BlockType != BlockType.Air) return true;

				// Grow Iteration
				int newIteration = pos.z;
				if (_y > pos.y) newIteration += 2;
				if (_x != pos.x) newIteration++;

				// Push
				ExpandQueueJump.Enqueue(new Int3(_x, _y, newIteration));
				_cell.OperationValid = true;
				_cell.FromCellX = pos.x;
				_cell.FromCellY = pos.y;
				_cell.FromMotion = NavigationOperateMotion.Jump;
				return true;
			}
		}
	}


	#endregion




}