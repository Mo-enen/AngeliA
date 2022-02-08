using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }



[System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class AngeliaInspectorAttribute : System.Attribute { }



namespace AngeliaFramework {



	public enum BlockLayer {
		Background = 0,
		Level = 1,
	}


	public enum EntityLayer {
		Environment = 0,
		Item = 1,
		Character = 2,
		Projectile = 3,
		UI = 4,
		Debug = 5,
	}




	public enum PhysicsLayer {
		Level = 0,      // Ground, Water, OnewayGate...
		Environment = 1,// Barrel, Chest, EventTrigger...
		Item = 2,       // HealthPotion, Coin, BouncyBall...
		Character = 3,  // Player, Enemy, NPC...
	}


	[System.Flags]
	public enum PhysicsMask {
		None = 0,
		Level = 1 << 0,      // Ground, Water, OnewayGate...
		Environment = 1 << 1,// Barrel, Chest, EventTrigger...
		Item = 1 << 2,       // HealthPotion, Coin, BouncyBall...
		Character = 1 << 3,  // Player, Enemy, NPC...
	}


	public enum Direction2 {
		Negative = -1,
		Positive = 1,
		Horizontal = -1,
		Vertical = 1,
	}


	public enum Direction3 {
		Negative = -1,
		None = 0,
		Positive = 1,
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
	public struct AlignmentInt {
		public int TopLeft;
		public int TopMid;
		public int TopRight;
		public int MidLeft;
		public int MidMid;
		public int MidRight;
		public int BottomLeft;
		public int BottomMid;
		public int BottomRight;
		public AlignmentInt (int topLeft, int topMid, int topRight, int midLeft, int midMid, int midRight, int bottomLeft, int bottomMid, int bottomRight) {
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
		public const int DEFAULT_VIEW_WIDTH = 28 * CELL_SIZE;
		public const int DEFAULT_VIEW_HEIGHT = 16 * CELL_SIZE;
		public const int BLOCK_LAYER_COUNT = 2;
		public const int ENTITY_LAYER_COUNT = 6;
		public const int PHYSICS_LAYER_COUNT = 4;
		public const int WORLD_MAP_SIZE = 128;
		public static int WATER_TAG = "Water".ACode();
	}


	public static class AUtil {


		// Physics Math
		public static bool Intersect_SegementSegement (
			Vector2Int a0, Vector2Int a1, Vector2Int b0, Vector2Int b1, out Vector2Int intersection
		) {
			intersection = default;

			var qp = b0 - a0;
			var r = a1 - a0;
			var s = b1 - b0;
			var rs = Cross(r, s);
			var qpr = Cross(qp, r);
			var qps = Cross(qp, s);

			if (rs == 0 && qpr == 0) return false;
			if (rs == 0 && qpr != 0) return false;
			if (
				rs != 0 &&
				0f <= qps * rs && (rs > 0 ? qps <= rs : qps >= rs) &&
				0f <= qpr * rs && (rs > 0 ? qpr <= rs : qpr >= rs)
			) {
				intersection = a0 + qps * r / rs;
				return true;
			}

			return false;

			static int Cross (Vector2Int a, Vector2Int b) => a.x * b.y - a.y * b.x;
		}


		public static int Intersect_SegementRect (
			Vector2Int a0, Vector2Int a1, RectInt rect,
			out Vector2Int intersection0, out Vector2Int intersection1
		) => Intersect_SegementRect(a0, a1, rect, out intersection0, out intersection1, out _, out _);


		public static int Intersect_SegementRect (
			Vector2Int a0, Vector2Int a1, RectInt rect,
			out Vector2Int intersection0, out Vector2Int intersection1,
			out Direction4 normalDirection0, out Direction4 normalDirection1
		) {
			intersection0 = default;
			intersection1 = default;
			int interCount = 0;
			normalDirection0 = default;
			normalDirection1 = default;

			// U
			if (Intersect_SegementSegement(
				a0, a1, new Vector2Int(rect.xMin, rect.yMax), new Vector2Int(rect.xMax, rect.yMax),
				out var inter
			)) {
				intersection0 = inter;
				normalDirection0 = Direction4.Up;
				interCount++;
			}

			// D
			if (Intersect_SegementSegement(
				a0, a1, new Vector2Int(rect.xMin, rect.yMin), new Vector2Int(rect.xMax, rect.yMin),
				out inter
			)) {
				if (interCount == 0) {
					intersection0 = inter;
					normalDirection0 = Direction4.Down;
					interCount++;
				} else {
					intersection1 = inter;
					normalDirection1 = Direction4.Down;
					return 2;
				}
			}

			// L 
			if (Intersect_SegementSegement(
				a0, a1, new Vector2Int(rect.xMin, rect.yMin), new Vector2Int(rect.xMin, rect.yMax),
				out inter
			)) {
				if (interCount == 0) {
					intersection0 = inter;
					normalDirection0 = Direction4.Left;
					interCount++;
				} else {
					intersection1 = inter;
					normalDirection1 = Direction4.Left;
					return 2;
				}
			}

			// R
			if (Intersect_SegementSegement(
				a0, a1, new Vector2Int(rect.xMax, rect.yMin), new Vector2Int(rect.xMax, rect.yMax),
				out inter
			)) {
				if (interCount == 0) {
					intersection0 = inter;
					normalDirection0 = Direction4.Right;
					interCount++;
				} else {
					intersection1 = inter;
					normalDirection1 = Direction4.Right;
					return 2;
				}
			}

			return interCount;
		}



		// Extension
		public static bool HasLayer (this PhysicsMask mask, PhysicsLayer layer) => layer switch {
			PhysicsLayer.Level => mask.HasFlag(PhysicsMask.Level),
			PhysicsLayer.Environment => mask.HasFlag(PhysicsMask.Environment),
			PhysicsLayer.Item => mask.HasFlag(PhysicsMask.Item),
			PhysicsLayer.Character => mask.HasFlag(PhysicsMask.Character),
			_ => throw new System.NotImplementedException(),
		};


	}


}
