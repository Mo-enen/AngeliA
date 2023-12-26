using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaForUnity {
	public static class ExtensionUnity {

		public static Float2 ToAngelia (this Vector2 v) => new(v.x, v.y);
		public static Vector2 ToUnity (this Float2 v) => new(v.x, v.y);

		public static Float3 ToAngelia (this Vector3 v) => new(v.x, v.y, v.z);
		public static Vector3 ToUnity (this Float3 v) => new(v.x, v.y, v.z);

		public static Float4 ToAngelia (this Vector4 v) => new(v.x, v.y, v.z, v.w);
		public static Vector4 ToUnity (this Float4 v) => new(v.x, v.y, v.z, v.w);

		public static Int2 ToAngelia (this Vector2Int v) => new(v.x, v.y);
		public static Vector2Int ToUnity (this Int2 v) => new(v.x, v.y);

		public static Int3 ToAngelia (this Vector3Int v) => new(v.x, v.y, v.z);
		public static Vector3Int ToUnity (this Int3 v) => new(v.x, v.y, v.z);

		public static Byte4 ToAngelia (this Color c) => new((byte)Util.RoundToInt(c.r.Clamp01() * 255f), (byte)Util.RoundToInt(c.g.Clamp01() * 255f), (byte)Util.RoundToInt(c.b.Clamp01() * 255f), (byte)Util.RoundToInt(c.a.Clamp01() * 255f));
		public static Color ToUnityColor (this Byte4 c) => new(c.r / 255f, c.g / 255f, c.b / 255f, c.a / 255f);

		public static Byte4 ToAngelia (this Color32 c) => new(c.r, c.g, c.b, c.a);
		public static Color32 ToUnityColor32 (this Byte4 c) => new(c.r, c.g, c.b, c.a);

		public static RectInt ToUnity (this IRect a) => new(a.x, a.y, a.width, a.height);
		public static IRect ToAngelia (this RectInt a) => new(a.x, a.y, a.width, a.height);

		public static Rect ToUnity (this FRect v) => new(v.x, v.y, v.width, v.height);
		public static FRect ToAngelia (this Rect v) => new(v.x, v.y, v.width, v.height);

		public static Byte4[] ToAngelia (this Color32[] colors) {
			var result = new Byte4[colors.Length];
			for (int i = 0; i < result.Length; i++) {
				result[i] = colors[i].ToAngelia();
			}
			return result;
		}
		public static Color32[] ToUnity (this Byte4[] colors) {
			var result = new Color32[colors.Length];
			for (int i = 0; i < result.Length; i++) {
				result[i] = colors[i].ToUnityColor32();
			}
			return result;
		}

		// Rect
		public static Rect Shift (this Rect rect, float x, float y) {
			rect.x += x;
			rect.y += y;
			return rect;
		}
		public static Rect Expand (this Rect rect, float offset) => rect.Expand(offset, offset, offset, offset);
		public static Rect Expand (this Rect rect, float l, float r, float d, float u) {
			rect.x -= l;
			rect.y -= d;
			rect.width += l + r;
			rect.height += d + u;
			return rect;
		}
		public static Rect Shrink (this Rect rect, float offset) => rect.Expand(-offset);
		public static Rect Shrink (this Rect rect, float l, float r, float d, float u) => rect.Expand(-l, -r, -d, -u);
		public static Rect Fit (this Rect rect, float targetAspect, float pivotX = 0.5f, float pivotY = 0.5f) {
			float sizeX = rect.width;
			float sizeY = rect.height;
			if (targetAspect > rect.width / rect.height) {
				sizeY = sizeX / targetAspect;
			} else {
				sizeX = sizeY * targetAspect;
			}
			return new Rect(
				rect.x + Util.Abs(rect.width - sizeX) * pivotX,
				rect.y + Util.Abs(rect.height - sizeY) * pivotY,
				sizeX, sizeY
			);
		}
		public static Rect Envelope (this Rect rect, float targetAspect) {
			float sizeX = rect.width;
			float sizeY = rect.height;
			if (targetAspect < rect.width / rect.height) {
				sizeY = sizeX / targetAspect;
			} else {
				sizeX = sizeY * targetAspect;
			}
			return new Rect(
				rect.x - Util.Abs(rect.width - sizeX) / 2f,
				rect.y - Util.Abs(rect.height - sizeY) / 2f,
				sizeX, sizeY
			);
		}
		public static RectInt ToRectInt (this Rect rect) => new((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
		public static void Clamp (this ref Rect rect, Rect target) => rect = Rect.MinMaxRect(
			Util.Min(target.xMax, Util.Max(rect.xMin, target.xMin)),
			Util.Min(target.yMax, Util.Max(rect.yMin, target.yMin)),
			Util.Max(target.xMin, Util.Min(rect.xMax, target.xMax)),
			Util.Max(target.yMin, Util.Min(rect.yMax, target.yMax))
		);
		public static Rect GetClamp (this Rect rect, Rect target) => Rect.MinMaxRect(
			Util.Min(target.xMax, Util.Max(rect.xMin, target.xMin)),
			Util.Min(target.yMax, Util.Max(rect.yMin, target.yMin)),
			Util.Max(target.xMin, Util.Min(rect.xMax, target.xMax)),
			Util.Max(target.yMin, Util.Min(rect.yMax, target.yMax))
		);

		public static Float2 BottomLeft (this Rect rect) => new(rect.xMin, rect.yMin);
		public static Float2 BottomRight (this Rect rect) => new(rect.xMax, rect.yMin);
		public static Float2 TopLeft (this Rect rect) => new(rect.xMin, rect.yMax);
		public static Float2 TopRight (this Rect rect) => new(rect.xMax, rect.yMax);

		// RectInt
		public static Rect ToRect (this RectInt rect) => new(rect.x, rect.y, rect.width, rect.height);
		public static RectInt Fit (this RectInt rect, AngeSprite sprite, int pivotX = 500, int pivotY = 500) => rect.Fit(sprite.GlobalWidth, sprite.GlobalHeight, pivotX, pivotY);
		public static RectInt Fit (this RectInt rect, int aspWidth, int aspHeight, int pivotX = 500, int pivotY = 500) {
			if (aspWidth * aspHeight == 0) return rect;
			int sizeX = rect.width;
			int sizeY = rect.height;
			if (aspWidth * rect.height > rect.width * aspHeight) {
				sizeY = sizeX * aspHeight / aspWidth;
			} else {
				sizeX = sizeY * aspWidth / aspHeight;
			}
			return new RectInt(
				rect.x + Util.Abs(rect.width - sizeX) * pivotX / 1000,
				rect.y + Util.Abs(rect.height - sizeY) * pivotY / 1000,
				sizeX, sizeY
			);
		}
		public static RectInt Expand (this RectInt rect, int offset) => rect.Expand(offset, offset, offset, offset);
		public static RectInt Expand (this RectInt rect, int l, int r, int d, int u) {
			rect.x -= l;
			rect.y -= d;
			rect.width += l + r;
			rect.height += d + u;
			return rect;
		}
		public static RectInt Expand (this RectInt rect, RectOffset offset) {
			rect.x -= offset.left;
			rect.y -= offset.bottom;
			rect.width += offset.horizontal;
			rect.height += offset.vertical;
			return rect;
		}
		public static RectInt Expand (this RectInt rect, Int4 offset) {
			rect.x -= offset.left;
			rect.y -= offset.down;
			rect.width += offset.left + offset.right;
			rect.height += offset.down + offset.up;
			return rect;
		}
		public static RectInt Shrink (this RectInt rect, int offset) => rect.Expand(-offset);
		public static RectInt Shrink (this RectInt rect, int l, int r, int d, int u) => rect.Expand(-l, -r, -d, -u);
		public static RectInt Shrink (this RectInt rect, RectOffset offset) {
			rect.x += offset.left;
			rect.y += offset.bottom;
			rect.width -= offset.horizontal;
			rect.height -= offset.vertical;
			return rect;
		}
		public static RectInt Shrink (this RectInt rect, Int4 offset) {
			rect.x += offset.left;
			rect.y += offset.down;
			rect.width -= offset.left + offset.right;
			rect.height -= offset.down + offset.up;
			return rect;
		}
		public static void FlipHorizontal (this ref RectInt rect) {
			rect.x += rect.width;
			rect.width = -rect.width;
		}
		public static void FlipVertical (this ref RectInt rect) {
			rect.y += rect.height;
			rect.height = -rect.height;
		}
		public static void FlipNegative (this ref RectInt rect) {
			if (rect.width < 0) {
				rect.x += rect.width;
				rect.width = -rect.width;
			}
			if (rect.height < 0) {
				rect.y += rect.height;
				rect.height = -rect.height;
			}
		}
		public static RectInt GetFlipNegative (this RectInt rect) {
			if (rect.width < 0) {
				rect.x += rect.width;
				rect.width = -rect.width;
			}
			if (rect.height < 0) {
				rect.y += rect.height;
				rect.height = -rect.height;
			}
			return rect;
		}
		public static RectInt Shift (this RectInt rect, int x, int y) {
			rect.x += x;
			rect.y += y;
			return rect;
		}
		public static void SetMinMax (this ref RectInt rect, int xMin, int xMax, int yMin, int yMax) {
			rect.x = xMin;
			rect.y = yMin;
			rect.width = xMax - xMin;
			rect.height = yMax - yMin;
		}

		public static bool IsSame (this RectInt a, RectInt b) => a.x == b.x && a.y == b.y && a.width == b.width && a.height == b.height;
		public static bool IsNotSame (this RectInt a, RectInt b) => a.x != b.x || a.y != b.y || a.width != b.width || a.height != b.height;
		public static bool Contains (this RectInt rect, int x, int y) => rect.Contains(new Vector2Int(x, y));
		public static int CenterX (this RectInt rect) => rect.x + rect.width / 2;
		public static int CenterY (this RectInt rect) => rect.y + rect.height / 2;
		public static Int2 CenterInt (this RectInt rect) => new(rect.x + rect.width / 2, rect.y + rect.height / 2);
		public static Int2 BottomLeft (this RectInt rect) => new(rect.xMin, rect.yMin);
		public static Int2 BottomRight (this RectInt rect) => new(rect.xMax, rect.yMin);
		public static Int2 TopLeft (this RectInt rect) => new(rect.xMin, rect.yMax);
		public static Int2 TopRight (this RectInt rect) => new(rect.xMax, rect.yMax);

		public static void ClampPositionInside (ref this RectInt rect, RectInt bounds) {
			rect.x = rect.x.Clamp(bounds.x, bounds.xMax - rect.width);
			rect.y = rect.y.Clamp(bounds.y, bounds.yMax - rect.height);
		}

		public static RectInt LerpTo (this RectInt from, RectInt to, int lerpRate) => new(
			from.x.LerpTo(to.x, lerpRate),
			from.y.LerpTo(to.y, lerpRate),
			from.width.LerpTo(to.width, lerpRate),
			from.height.LerpTo(to.height, lerpRate)
		);
		public static RectInt LerpTo (this RectInt from, RectInt to, float lerp) => new(
			from.x.LerpTo(to.x, lerp),
			from.y.LerpTo(to.y, lerp),
			from.width.LerpTo(to.width, lerp),
			from.height.LerpTo(to.height, lerp)
		);


		// Vector
		public static void Clamp (this ref Vector2Int v, int minX, int minY, int maxX, int maxY) {
			v.x = Util.Clamp(v.x, minX, maxX);
			v.y = Util.Clamp(v.y, minY, maxY);
		}
		public static void Clamp (this ref Vector2 v, float minX, float minY, float maxX, float maxY) {
			v.x = Util.Clamp(v.x, minX, maxX);
			v.y = Util.Clamp(v.y, minY, maxY);
		}
		public static Vector2Int UDivide (this Vector2Int v, int gap) {
			v.x = v.x.UDivide(gap);
			v.y = v.y.UDivide(gap);
			return v;
		}
		public static Vector2Int Clamped (this Vector2Int v, int minX, int minY, int maxX = int.MaxValue, int maxY = int.MaxValue) {
			v.Clamp(minX, minY, maxX, maxY);
			return v;
		}
		public static bool Almost (this Vector3 a, Vector3 b) => Util.Approximately(a.x, b.x) && Util.Approximately(a.y, b.y) && Util.Approximately(a.z, b.z);
		public static bool Almost (this Vector2 a, Vector2 b) => Util.Approximately(a.x, b.x) && Util.Approximately(a.y, b.y);
		public static bool Almost (this Rect a, Rect b) => Util.Approximately(a.x, b.x) && Util.Approximately(a.y, b.y) && Util.Approximately(a.width, b.width) && Util.Approximately(a.height, b.height);
		public static bool NotAlmost (this Vector3 a, Vector3 b) => !a.Almost(b);
		public static bool NotAlmost (this Vector2 a, Vector2 b) => !a.Almost(b);
		public static bool NotAlmost (this Rect a, Rect b) => !a.Almost(b);


		// Transform
		public static void DestroyAllChildrenImmediate (this Transform target) {
			int childCount = target.childCount;
			for (int i = 0; i < childCount; i++) {
				Object.DestroyImmediate(target.GetChild(0).gameObject, false);
			}
		}

	}
}