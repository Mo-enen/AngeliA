using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {



	public enum Layer {
		Background = 0,
		Level = 1,
		Item = 2,
		Character = 3,
		Projectile = 4,
		UI = 5,
	}



	public enum Direction2 {
		Horizontal = 0,
		Vertical = 1,
	}



	public enum Direction4 {
		Up = 0,
		Down = 1,
		Left = 2,
		Right = 3,
	}



	public enum Direction8 {
		Up = 0,
		Down = 1,
		Left = 2,
		Right = 3,
		UpLeft = 4,
		UpRight = 5,
		DownLeft = 6,
		DownRight = 7,
	}



	public static class Const {
		public const int CELL_SIZE = 256;
		public const int LAYER_COUNT = 6;
	}


}
