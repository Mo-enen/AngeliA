using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;



namespace Yaya {


	public enum NavigationMotion {
		None = 0,
		Move = 1,
		Jump = 2,
		Drop = 3,
		Fly = 4,
	}


	public static class CellNavigation {




		#region --- SUB ---


		public struct Operation {
			public NavigationMotion Result;
			public int TargetCellX;
			public int TargetCellY;
			public int Distance;
		}

		private enum BlockType {
			Air, Liquid, Solid,
		}

		private struct Cell {


			public static readonly Cell EMPTY = new() {
				BlockStamp = -1,
				OperationStamp = 0,
				Operation = {
					Result = NavigationMotion.None,
					TargetCellX = -1,
					TargetCellY = -1,
					Distance = -1,
				},
			};


			// Operation
			public bool OperationDataValid {
				get => OperationStamp == CellNavigation.OperationStamp;
				set => OperationStamp = value ? CellNavigation.OperationStamp : 0;
			}
			public Operation Operation;
			private uint OperationStamp;

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
		}


		public static int Navigate (
			Operation[] Operations, eCharacter character, int toX, int toY,
			int jumpUnitRangeX, int jumpUnitRangeY
		) {
			RefreshFrameCache();
			OperationStamp++;
			var unitRangeRect = new RectInt(CellUnitOffsetX, CellUnitOffsetY, CellWidth, CellHeight);
			int fromUnitX = character.X.UDivide(Const.CEL).Clamp(unitRangeRect.xMin, unitRangeRect.xMax - 1);
			int fromUnitY = character.Y.UDivide(Const.CEL).Clamp(unitRangeRect.yMin, unitRangeRect.yMax - 1);
			int toUnitX = toX.UDivide(Const.CEL).Clamp(unitRangeRect.xMin, unitRangeRect.xMax - 1);
			int toUnitY = toY.UDivide(Const.CEL).Clamp(unitRangeRect.yMin, unitRangeRect.yMax - 1);




			return 0;
		}


		public static bool FindGroundInRange (RectInt globalRange, out int resultX, out int resultY) {

			RefreshFrameCache();

			int centerX = resultX = globalRange.x + globalRange.width / 2;
			int centerY = resultY = globalRange.y + globalRange.height / 2;

			for (int j = 0; j <= globalRange.height / 2; j += Const.CEL) {
				if (CheckGroundHorizontal(centerY + j, out resultX, out resultY)) return true;
				if (CheckGroundHorizontal(centerY - j, out resultX, out resultY)) return true;
			}

			return false;

			// Func
			bool CheckGroundHorizontal (int globalY, out int _resultX, out int _resultY) {
				for (int i = 0; i <= globalRange.width / 2; i += Const.CEL) {
					if (TryGetGroundPosition(centerX + i, globalY, out _resultY)) {
						_resultX = (centerX + i).UDivide(Const.CEL) * Const.CEL + Const.HALF;
						return true;
					}
					if (TryGetGroundPosition(centerX - i, globalY, out _resultY)) {
						_resultX = (centerX - i).UDivide(Const.CEL) * Const.CEL + Const.HALF;
						return true;
					}
				}
				_resultX = 0;
				_resultY = 0;
				return false;
			}
		}


		public static bool SnapToGroundNearby (int globalX, int globalY, int distance, out int resultX, out int resultY) {
			resultX = globalX;
			resultY = globalY;
			






			return false;
		}


		public static bool TryGetGroundPosition (int globalX, int globalY, out int groundY) {
			RefreshFrameCache();
			groundY = globalY;
			int cellX = globalX.UDivide(Const.CEL) - CellUnitOffsetX;
			if (!cellX.InRange(0, CellWidth - 1)) return false;
			int cellY = globalY.UDivide(Const.CEL) - CellUnitOffsetY;
			int groundY_This = groundY;
			int groundY_Down = groundY;

			bool thisIsStandable =
				cellY.InRange(0, CellHeight - 1) &&
				IsStandableLogic(cellX, cellY, out groundY_This);
			bool downIsStandable =
				(cellY - 1).InRange(0, CellHeight - 1) &&
				IsStandableLogic(cellX, cellY - 1, out groundY_Down);

			if (thisIsStandable && downIsStandable) {
				groundY = groundY_This <= globalY ? groundY_This : groundY_Down;
				return true;
			} else if (thisIsStandable && !downIsStandable) {
				groundY = groundY_This;
				return groundY_This <= globalY || Cells[cellX, cellY].IsLiquid;
			} else if (!thisIsStandable && downIsStandable) {
				groundY = groundY_Down;
				return true;
			}

			return false;
		}


		public static bool IsStandeable (int globalX, int globalY, out int groundY) {
			groundY = globalY;
			int cellX = globalX.UDivide(Const.CEL) - CellUnitOffsetX;
			if (!cellX.InRange(0, CellWidth - 1)) return false;
			int cellY = globalY.UDivide(Const.CEL) - CellUnitOffsetY;
			if (!cellY.InRange(0, CellHeight - 1)) return false;
			return IsStandableLogic(cellX, cellY, out groundY);
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
						centerRect, out var hit,
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


		private static bool IsStandableLogic (int cellX, int cellY, out int groundY) {
			// Check Standable
			var cell = GetBlockedData(cellX, cellY);
			groundY = cell.PlatformY;
			if (cell.IsLiquid) return true;
			if (!cell.IsBlockedUp) return false;
			// Check Block Up Solid
			if (cellY + 1 < CellHeight) {
				var cellU = GetBlockedData(cellX, cellY + 1);
				if (cellU.IsSolid) return false;
			}
			return true;
		}


		#endregion




	}
}