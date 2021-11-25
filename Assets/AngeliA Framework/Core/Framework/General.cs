using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {



	public enum EntityLayer {
		Item = 0,
		Character = 1,
		Projectile = 2,
		UI = 3,
	}


	public enum PhysicsLayer {
		Level = 0,
		Item = 1,
		Character = 2,
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
		public const int ENTITY_LAYER_COUNT = 4;
		public const int PHYSICS_LAYER_COUNT = 3;
	}


}
