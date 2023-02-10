using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;



namespace Yaya {


	public enum NavigationResult {
		None = 0,
		Run = 1,
		Jump = 2,
		Fly = 3,
	}


	public static class CellNavigation {




		#region --- SUB ---


		public struct Operation {
			public NavigationResult Result;
			public int TargetCellX;
			public int TargetCellY;
		}


		private struct Cell {

			public static readonly Cell EMPTY = new() {
				BlockStamp = -1,
				OperationStamp = 0,
				Operation = {
					Result = NavigationResult.None,
					TargetCellX = -1,
					TargetCellY = -1,
				},
				Distance = -1,
			};


			// Operation
			public bool OperationDataValid {
				get => OperationStamp == CellNavigation.OperationStamp;
				set => OperationStamp = value ? CellNavigation.OperationStamp : 0;
			}
			public Operation Operation;
			public int Distance;
			private uint OperationStamp;

			// Block
			public bool BlockDataValid {
				get => BlockStamp == Game.GlobalFrame;
				set => BlockStamp = value ? Game.GlobalFrame : -1;
			}
			public bool IsAllBlocked;
			public bool IsSolidBlocked;
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
		internal static int CellUnitOffsetX { get; private set; } = 0;
		internal static int CellUnitOffsetY { get; private set; } = 0;
		internal static int CellWidth { get; private set; } = 1;
		internal static int CellHeight { get; private set; } = 1;

		// Data
		private static Cell[,] Cells = null;
		private static uint OperationStamp = 0;
		private static readonly PhysicsCell[] OnewayCheckHits = new PhysicsCell[8];


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
			int jumpUnitRange, int minDistance
		) {
			BeginOperation();
			var unitRangeRect = new RectInt(CellUnitOffsetX, CellUnitOffsetY, CellWidth, CellHeight);
			int fromUnitX = character.X.UDivide(Const.CEL).Clamp(unitRangeRect.xMin, unitRangeRect.xMax - 1);
			int fromUnitY = character.Y.UDivide(Const.CEL).Clamp(unitRangeRect.yMin, unitRangeRect.yMax - 1);
			int toUnitX = toX.UDivide(Const.CEL).Clamp(unitRangeRect.xMin, unitRangeRect.xMax - 1);
			int toUnitY = toY.UDivide(Const.CEL).Clamp(unitRangeRect.yMin, unitRangeRect.yMax - 1);





			return 0;
		}


		public static bool TryGetGroundNearby (
			int x, int y, RectInt range,
			out int resultX, out int resultY
		) {
			resultX = x;
			resultY = y;




			return true;
		}


		public static bool TryGetGroundPosition (int x, int y, out int groundY) {
			int cellX = UnitX_to_CellX(x.UDivide(Const.CEL));
			int cellY = UnitY_to_CellY(y.UDivide(Const.CEL));
			groundY = y;
			if (!cellX.InRange(0, CellWidth - 1) || !(cellY - 1).InRange(0, CellHeight - 1)) return false;
			return IsGround(cellX, cellY - 1, out groundY);
		}


		#endregion




		#region --- LGC ---


		private static void BeginOperation () {
			CellUnitOffsetX = (Game.Current.ViewRect.x - Const.LEVEL_SPAWN_PADDING_UNIT).UDivide(Const.CEL);
			CellUnitOffsetY = (Game.Current.ViewRect.y - Const.LEVEL_SPAWN_PADDING_UNIT).UDivide(Const.CEL);
			OperationStamp++;
		}


		// Util
		private static int UnitX_to_CellX (int unitX) => unitX - CellUnitOffsetX;
		private static int UnitY_to_CellY (int unitY) => unitY - CellUnitOffsetY;


		private static Cell GetBlockedData (int cellX, int cellY) {

			ref var cell = ref Cells[cellX, cellY];
			if (cell.BlockDataValid) return cell;
			int x = (cellX + CellUnitOffsetX) * Const.CEL;
			int y = (cellY + CellUnitOffsetY) * Const.CEL;
			var blockRect = new RectInt(x, y, Const.CEL, Const.CEL);
			bool solid = CellPhysics.Overlap(
				YayaConst.MASK_MAP, new RectInt(x + Const.HALF, y + Const.HALF, 1, 1), out var info
			);
			cell.BlockDataValid = true;
			cell.IsSolidBlocked = solid;
			cell.PlatformY = y + Const.CEL;

			if (solid) {
				// Solid Block
				cell.IsAllBlocked = true;
				cell.IsBlockedLeft = true;
				cell.IsBlockedRight = true;
				cell.IsBlockedDown = true;
				cell.IsBlockedUp = true;
				cell.PlatformY = info.Rect.yMax;
			} else {
				// Oneway
				int platformY = cell.PlatformY;
				var onewayRect = blockRect.Shrink(Const.CEL / 64);
				cell.IsBlockedLeft = OnewaySolid(Direction4.Left);
				cell.IsBlockedRight = OnewaySolid(Direction4.Right);
				cell.IsBlockedDown = OnewaySolid(Direction4.Down);
				cell.IsBlockedUp = OnewaySolid(Direction4.Up);
				cell.IsAllBlocked = cell.IsBlockedLeft && cell.IsBlockedRight && cell.IsBlockedDown && cell.IsBlockedUp;
				cell.PlatformY = platformY;
				bool OnewaySolid (Direction4 gateDirection) {
					int count = CellPhysics.OverlapAll(
						OnewayCheckHits, YayaConst.MASK_MAP, onewayRect,
						null, OperationMode.TriggerOnly,
						AngeUtil.GetOnewayTag(gateDirection)
					);
					for (int i = 0; i < count; i++) {
						var hit = OnewayCheckHits[i];
						switch (gateDirection) {
							case Direction4.Down:
								if (hit.Rect.yMin >= blockRect.yMin) return true;
								break;
							case Direction4.Up:
								if (hit.Rect.yMax <= blockRect.yMax) {
									platformY = hit.Rect.yMax;
									return true;
								}
								break;
							case Direction4.Left:
								if (hit.Rect.xMin >= blockRect.xMin) return true;
								break;
							case Direction4.Right:
								if (hit.Rect.xMax >= blockRect.xMax) return true;
								break;
						}
					}
					return false;
				}
			}
			return cell;
		}


		private static Cell GetOperationData (int cellX, int cellY) {
			var cell = Cells[cellX, cellY];
			return cell.OperationDataValid ? cell : Cell.EMPTY;
		}


		private static bool IsGround (int cellX, int cellY, out int groundY) {
			// Check Standable
			var cell = GetBlockedData(cellX, cellY);
			groundY = cell.PlatformY;
			if (!cell.IsBlockedUp) return false;
			// Check Space on Top
			if (cellY + 1 < CellHeight) {
				var cellUp = GetBlockedData(cellX, cellY + 1);
				if (cellUp.IsSolidBlocked) return false;
			}
			return true;
		}


		#endregion




	}
}