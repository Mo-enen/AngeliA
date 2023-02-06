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


		private struct Cell {

			public static readonly Cell EMPTY = new();

			public uint Stamp;
			public NavigationResult Result;
			public int TargetCellX;
			public int TargetCellY;

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


		#endregion




		#region --- API ---


		[AfterGameInitialize]
		public static void Initialize () {
			var viewConfig = Game.Current.ViewConfig;
			CellWidth = (viewConfig.ViewRatio * viewConfig.MaxHeight / 1000 + Const.SPAWN_PADDING * 2) / Const.CEL + Const.LEVEL_SPAWN_PADDING_UNIT * 2;
			CellHeight = (viewConfig.MaxHeight + Const.SPAWN_PADDING * 2) / Const.CEL + Const.LEVEL_SPAWN_PADDING_UNIT * 2;
			Cells = new Cell[CellWidth, CellHeight];
		}


		public static NavigationResult Navigate (
			eCharacter character, int toX, int toY, int jumpUnitRange,
			out int resultX, out int resultY
		) {
			var result = NavigationResult.None;
			resultX = character.X;
			resultY = character.Y;
			int toUnitX = toX.UDivide(Const.CEL);
			int toUnitY = toY.UDivide(Const.CEL);
			BeginOperation();






			return result;
		}


		public static (int x, int y) FlyAround (
			int fromX, int fromY, int toX, int toY,
			int dislocationIndex
		) {



			return (fromX, fromY);
		}


		public static (int x, int y) GetReasonablePositionNearby (int x, int y, int range) {




			return (x, y);
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


		private static void SetCell (int cellX, int cellY, NavigationResult result, int targetX, int targetY) {
			ref var cell = ref Cells[cellX, cellY];
			cell.Stamp = OperationStamp;
			cell.Result = result;
			cell.TargetCellX = targetX;
			cell.TargetCellY = targetY;
		}


		private static bool IsBlocked (int mask, Entity entity, int cellX, int cellY) => CellPhysics.Overlap(
			mask, new RectInt(
				(cellX + CellUnitOffsetX) * Const.CEL + Const.CEL / 2,
				(cellY + CellUnitOffsetY) * Const.CEL + Const.CEL / 2,
				1, 1
			), entity
		);


		#endregion




	}
}