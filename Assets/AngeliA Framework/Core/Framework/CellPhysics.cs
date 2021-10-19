using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace AngeliaFramework {
	public static class CellPhysics {




		#region --- SUB ---


		public class Cell {

		}


		#endregion




		#region --- VAR ---


		// Api
		public static int Width { get; private set; } = 0;
		public static int Height { get; private set; } = 0;

		// Data
		private static Cell[,] Cells = null;


		#endregion




		#region --- API ---


		public static void Init (int width, int height) {
			Width = width;
			Height = height;
			Cells = new Cell[width, height];
		}


		public static void Clear () => System.Array.Clear(Cells, 0, Cells.Length);


		#endregion




		#region --- LGC ---




		#endregion




	}
}
