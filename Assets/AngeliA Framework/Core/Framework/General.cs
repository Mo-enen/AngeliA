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


	public enum Alignment {
		TopLeft = 0,
		TopMid = 1,
		TopRight = 2,
		MidLeft = 3,
		MidMid = 4,
		MidRight = 5,
		BottomLeft = 6,
		BottomMid = 7,
		BottomRight = 8,
	}


	[System.Serializable]
	public struct UvRect {
		public Vector2 BottomLeft;
		public Vector2 BottomRight;
		public Vector2 TopLeft;
		public Vector2 TopRight;
	}


	[System.Serializable]
	public struct IntAlignment {
		public int TopLeft;
		public int TopMid;
		public int TopRight;
		public int MidLeft;
		public int MidMid;
		public int MidRight;
		public int BottomLeft;
		public int BottomMid;
		public int BottomRight;
		public IntAlignment (int topLeft, int topMid, int topRight, int midLeft, int midMid, int midRight, int bottomLeft, int bottomMid, int bottomRight) {
			TopLeft = topLeft;
			TopMid = topMid;
			TopRight = topRight;
			MidLeft = midLeft;
			MidMid = midMid;
			MidRight = midRight;
			BottomLeft = bottomLeft;
			BottomMid = bottomMid;
			BottomRight = bottomRight;
		}
	}


	public static class Const {
		public const int CELL_SIZE = 256;
		public const int ENTITY_LAYER_COUNT = 4;
		public const int PHYSICS_LAYER_COUNT = 3;
	}


}
