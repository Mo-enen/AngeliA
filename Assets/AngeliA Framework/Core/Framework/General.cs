using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }



[System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class AngeliaInspectorAttribute : System.Attribute { }


[System.AttributeUsage(System.AttributeTargets.Field)]
public class ACodeIntAttribute : PropertyAttribute { }


namespace AngeliaFramework {


#if UNITY_EDITOR
	using UnityEditor;
	[CustomPropertyDrawer(typeof(ACodeIntAttribute))]
	public class ACodeIntAttribute_AttributeDrawer : PropertyDrawer {
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			if (property.propertyType == SerializedPropertyType.Integer) {
				const int INT_WIDTH = 86;
				string newText = EditorGUI.DelayedTextField(position.Shrink(0, INT_WIDTH, 0, 0), label, "");
				if (!string.IsNullOrEmpty(newText)) {
					property.intValue = newText.ACode();
				}
				GUI.Label(
					position.Shrink(position.width - INT_WIDTH, 0, 0, 0),
					" " + property.intValue.ToString(),
					EditorStyles.centeredGreyMiniLabel
				);
			} else {
				EditorGUI.PropertyField(position, property, label, true);
			}
		}
	}
#endif



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
			BottomLeft = "Pixel".ACode(),
			BottomMid = "Pixel".ACode(),
			BottomRight = "Pixel".ACode(),
			MidLeft = "Pixel".ACode(),
			MidRight = "Pixel".ACode(),
			TopLeft = "Pixel".ACode(),
			TopMid = "Pixel".ACode(),
			TopRight = "Pixel".ACode(),
			border = new(3, 3, 3, 3),
		};
		public static readonly NineSliceSprites PIXEL_FRAME_6 = new() {
			BottomLeft = "Pixel".ACode(),
			BottomMid = "Pixel".ACode(),
			BottomRight = "Pixel".ACode(),
			MidLeft = "Pixel".ACode(),
			MidRight = "Pixel".ACode(),
			TopLeft = "Pixel".ACode(),
			TopMid = "Pixel".ACode(),
			TopRight = "Pixel".ACode(),
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


	public static class Const {

		public const int CELL_SIZE = 256;       // N¡ÁN Pixels per Tile
		public const int WORLD_MAP_SIZE = 128;  // N¡ÁN Tiles per Map
		public const int GLOBAL_SIZE = 32768;   // (-N,N)¡Á(-N,N) Maps in Total (23726566 max)

		public const int BLOCK_LAYER_COUNT = 2;
		public const int ENTITY_LAYER_COUNT = 6;

		public const int PHYSICS_LAYER_COUNT = 4;
		public const int DEFAULT_VIEW_WIDTH = 28 * CELL_SIZE;
		public const int DEFAULT_VIEW_HEIGHT = 16 * CELL_SIZE;
		public const int BLOCK_SPAWN_PADDING = 6;
		public const int MIN_VIEW_WIDTH = 4 * CELL_SIZE;
		public const int MIN_VIEW_HEIGHT = 2 * CELL_SIZE;
		public const int MAX_VIEW_WIDTH = 72 * CELL_SIZE;
		public const int MAX_VIEW_HEIGHT = 56 * CELL_SIZE;
		public const int SPAWN_GAP = 6 * CELL_SIZE;
		public const int DESPAWN_GAP = 6 * CELL_SIZE;

		public const int WATER_SPEED_LOSE = 400;
		public static int WATER_TAG = "Water".ACode();

	}


	public static class AUtil {


		// Physics Math
		/*
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
		*/


		// Extension
		public static bool HasLayer (this PhysicsMask mask, PhysicsLayer layer) => layer switch {
			PhysicsLayer.Level => mask.HasFlag(PhysicsMask.Level),
			PhysicsLayer.Environment => mask.HasFlag(PhysicsMask.Environment),
			PhysicsLayer.Item => mask.HasFlag(PhysicsMask.Item),
			PhysicsLayer.Character => mask.HasFlag(PhysicsMask.Character),
			_ => throw new System.NotImplementedException(),
		};


		public static long GetEntityInstanceID (int mapX, int mapY, int entityIndex) =>
			(mapY + Const.GLOBAL_SIZE) * Const.GLOBAL_SIZE * 2 + mapX + Const.GLOBAL_SIZE +
			(entityIndex * Const.WORLD_MAP_SIZE * Const.WORLD_MAP_SIZE);


		public static int AltDivide (this int value, int gap) =>
			value > 0 || value % gap == 0 ?
			value / gap :
			value / gap - 1;


		public static int AltMode (this int value, int gap) =>
			value > 0 || value % gap == 0 ?
			value % gap :
			value % gap + gap;


		public static Vector2Int Divide (this Vector2Int v, int gap) {
			v.x = v.x.AltDivide(gap);
			v.y = v.y.AltDivide(gap);
			return v;
		}


		public static RectInt Divide (this RectInt rect, int gap) {
			rect.SetMinMax(rect.min.Divide(gap), rect.max.Divide(gap));
			return rect;
		}


		// AngeliA Hash Code
		public static int ACode (this System.Type type) => type.Name.ACode();
		public static int ACode (this string str) {
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


	}


}
