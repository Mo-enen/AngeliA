using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }



[System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class AngeliaInspectorAttribute : System.Attribute { }



namespace AngeliaFramework {


	public enum EntityLayer {
		UI = 0,
		Environment = 1,
		Item = 2,
		Character = 3,
		Projectile = 4,
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
		Left = -1,
		Down = -1,
		Horizontal = -1,
		Positive = 1,
		Up = 1,
		Right = 1,
		Vertical = 1,
	}


	public enum Direction3 {
		None = 0,
		Negative = -1,
		Left = -1,
		Down = -1,
		Horizontal = -1,
		Positive = 1,
		Up = 1,
		Right = 1,
		Vertical = 1,
	}


	public enum Direction4 {
		Up = 0,
		Top = 0,
		Down = 1,
		Bottom = 1,
		Left = 2,
		Right = 3,
	}


	public enum Direction5 {
		Up = 0,
		Top = 0,
		Down = 1,
		Bottom = 1,
		Left = 2,
		Right = 3,
		Center = 4,
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

		public float Width => TopRight.x - BottomLeft.x;
		public float Height => TopRight.y - BottomLeft.y;

		public Vector2 BottomLeft;
		public Vector2 BottomRight;
		public Vector2 TopLeft;
		public Vector2 TopRight;

	}


	[System.Serializable]
	public struct NineSliceSprites {


		// Const
		public static readonly NineSliceSprites PIXEL_FRAME_3 = new() {
			BottomLeft = "Pixel".AngeHash(),
			BottomMid = "Pixel".AngeHash(),
			BottomRight = "Pixel".AngeHash(),
			MidLeft = "Pixel".AngeHash(),
			MidRight = "Pixel".AngeHash(),
			TopLeft = "Pixel".AngeHash(),
			TopMid = "Pixel".AngeHash(),
			TopRight = "Pixel".AngeHash(),
			border = new(3, 3, 3, 3),
		};
		public static readonly NineSliceSprites PIXEL_FRAME_6 = new() {
			BottomLeft = "Pixel".AngeHash(),
			BottomMid = "Pixel".AngeHash(),
			BottomRight = "Pixel".AngeHash(),
			MidLeft = "Pixel".AngeHash(),
			MidRight = "Pixel".AngeHash(),
			TopLeft = "Pixel".AngeHash(),
			TopMid = "Pixel".AngeHash(),
			TopRight = "Pixel".AngeHash(),
			border = new(6, 6, 6, 6),
		};

		// Api-Ser
		public int TopLeft;
		public int TopMid;
		public int TopRight;
		public int MidLeft;
		public int MidMid;
		public int MidRight;
		public int BottomLeft;
		public int BottomMid;
		public int BottomRight;
		public RectOffset border;

		public NineSliceSprites (NineSliceSprites source) {
			TopLeft = source.TopLeft;
			TopMid = source.TopMid;
			TopRight = source.TopRight;
			MidLeft = source.MidLeft;
			MidMid = source.MidMid;
			MidRight = source.MidRight;
			BottomLeft = source.BottomLeft;
			BottomMid = source.BottomMid;
			BottomRight = source.BottomRight;
			border = new RectOffset(
				source.border.left, source.border.right, source.border.top, source.border.bottom
			);
		}

	}



	[System.Serializable]
	public struct Int4 {

		public int Left { get => A; set => A = value; }
		public int Right { get => B; set => B = value; }
		public int Down { get => C; set => C = value; }
		public int Up { get => D; set => D = value; }

		public int A;
		public int B;
		public int C;
		public int D;

	}



	public static class Const {

		public const int CELL_SIZE = 256;       // N¡ÁN Pixels per Tile
		public const int WORLD_MAP_SIZE = 128;  // N¡ÁN Tiles per Map
		public const int GLOBAL_SIZE = 32768;   // (-N,N)¡Á(-N,N) Maps in Total (23726566 max)

		public const int ENTITY_LAYER_COUNT = 5;
		public const int PHYSICS_LAYER_COUNT = 4;

		public const int BLOCK_SPAWN_PADDING_UNIT = 6;
		public const int BLOCK_SPAWN_PADDING = 6 * CELL_SIZE;
		public const int DEFAULT_VIEW_WIDTH = 28 * CELL_SIZE;
		public const int DEFAULT_VIEW_HEIGHT = 16 * CELL_SIZE;
		public const int MIN_VIEW_WIDTH = 4 * CELL_SIZE;
		public const int MIN_VIEW_HEIGHT = 2 * CELL_SIZE;
		public const int MAX_VIEW_WIDTH = 72 * CELL_SIZE;
		public const int MAX_VIEW_HEIGHT = 56 * CELL_SIZE;

		public const int ENTITY_UPDATE_GAP = 1 * CELL_SIZE;
		public const int SPAWN_GAP = 8 * CELL_SIZE;
		public const int DESPAWN_GAP = 6 * CELL_SIZE;

		public const int RIGIDBODY_FAST_SPEED = CELL_SIZE / 8;

		public const int WATER_SPEED_LOSE = 400;
		public static int WATER_TAG = "Water".AngeHash();
		public static int ONEWAY_UP_TAG = "OnewayUp".AngeHash();
		public static int ONEWAY_DOWN_TAG = "OnewayDown".AngeHash();
		public static int ONEWAY_LEFT_TAG = "OnewayLeft".AngeHash();
		public static int ONEWAY_RIGHT_TAG = "OnewayRight".AngeHash();

		public const string MAP_FILE_EXT = "aamap";

		public static int GetOnewayTag (Direction4 gateDirection) =>
			gateDirection switch {
				Direction4.Down => ONEWAY_DOWN_TAG,
				Direction4.Up => ONEWAY_UP_TAG,
				Direction4.Left => ONEWAY_LEFT_TAG,
				Direction4.Right => ONEWAY_RIGHT_TAG,
				_ => throw new System.NotImplementedException(),
			};



	}


	public static class AUtil {


		// Extension
		public static bool HasLayer (this PhysicsMask mask, PhysicsLayer layer) => layer switch {
			PhysicsLayer.Level => mask.HasFlag(PhysicsMask.Level),
			PhysicsLayer.Environment => mask.HasFlag(PhysicsMask.Environment),
			PhysicsLayer.Item => mask.HasFlag(PhysicsMask.Item),
			PhysicsLayer.Character => mask.HasFlag(PhysicsMask.Character),
			_ => throw new System.NotImplementedException(),
		};


		public static int AltDivide (this int value, int gap) =>
			value > 0 || value % gap == 0 ?
			value / gap :
			value / gap - 1;


		public static int AltMod (this int value, int gap) =>
			value > 0 || value % gap == 0 ?
			value % gap :
			value % gap + gap;


		public static string GetMapRoot () => Util.CombinePaths(Util.GetRuntimeBuiltRootPath(), "Maps");


		public static Vector2Int AltDivide (this Vector2Int v, int gap) {
			v.x = v.x.AltDivide(gap);
			v.y = v.y.AltDivide(gap);
			return v;
		}


		public static RectInt AltDivide (this RectInt rect, int gap) {
			rect.SetMinMax(rect.min.AltDivide(gap), rect.max.AltDivide(gap));
			return rect;
		}


		public static Direction4 Opposite (this Direction4 dir) => dir switch {
			Direction4.Down => Direction4.Up,
			Direction4.Up => Direction4.Down,
			Direction4.Left => Direction4.Right,
			Direction4.Right => Direction4.Left,
			_ => throw new System.NotImplementedException(),
		};


		public static Vector2Int Normal (this Direction4 dir) => dir switch {
			Direction4.Down => new(0, -1),
			Direction4.Up => new(0, 1),
			Direction4.Left => new(-1, 0),
			Direction4.Right => new(1, 0),
			_ => throw new System.NotImplementedException(),
		};


		// AngeliA Hash Code
		public static int AngeHash (this System.Type type) => type.Name.AngeHash();


		public static int AngeHash (this string str) {
			const int p = 31;
			const int m = 1837465129;
			int hash_value = 0;
			int p_pow = 1;
			foreach (var c in str) {
				hash_value = (hash_value + (c - 'a' + 1) * p_pow) % m;
				p_pow = (p_pow * p) % m;
			}
			return hash_value == 0 ? 1 : hash_value;
		}


		public static void Dispose (this HitInfo[] hits) {
			int len = hits.Length;
			for (int i = 0; i < len; i++) {
				if (hits[i] == null) break;
				hits[i] = null;
			}
		}


	}


}
