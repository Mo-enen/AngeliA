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

		}


		#endregion




		#region --- VAR ---


		// Api
		internal static int PositionX { get; private set; } = 0;
		internal static int PositionY { get; private set; } = 0;
		internal static int CellWidth { get; private set; } = 1;
		internal static int CellHeight { get; private set; } = 1;

		// Data
		private static Cell[,] Cells = null;
		private static uint OperationStamp = 0;


		#endregion




		#region --- API ---


		internal static void Initialize (int cellWidth, int cellHeight) {
			CellWidth = cellWidth;
			CellHeight = cellHeight;
			Cells = new Cell[CellWidth, CellHeight];
		}


		public static NavigationResult Navigate (
			eCharacter character, int toX, int toY, int jumpRange,
			out int resultX, out int resultY
		) {
			var result = NavigationResult.None;
			resultX = character.X;
			resultY = character.Y;
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


		private static bool PassThrough (Rigidbody rig, int fromX, int fromY, int toX, int toY) {

			int width = rig.Width;
			int height = rig.Height;
			fromX += rig.OffsetX;
			fromY += rig.OffsetY;
			toX += rig.OffsetX;
			toY += rig.OffsetY;

			//CellPhysics.MoveImmediately();



			return true;
		}


		private static void BeginOperation () {
			PositionX = Game.Current.SpawnRect.x;
			PositionY = Game.Current.SpawnRect.y;
			OperationStamp++;
		}


		// Util
		private static int GlobalX_to_CellX (int globalX) => (globalX - PositionX) / Const.CEL + Const.LEVEL_SPAWN_PADDING_UNIT;
		private static int GlobalY_to_CellY (int globalY) => (globalY - PositionY) / Const.CEL + Const.LEVEL_SPAWN_PADDING_UNIT;


		#endregion




	}
}