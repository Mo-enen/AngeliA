using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using System.IO;
using UnityEngine;


namespace AngeliaFramework {
	public static class Extension {



		// Number
		public static bool GetBit (this ulong value, int index) => (value & (1UL << index)) != 0;
		public static bool GetBit (this int value, int index) => (value & (1 << index)) != 0;
		public static bool GetBit (this ushort value, int index) => (value & (1 << index)) != 0;
		
		public static void SetBit (this ref ulong value, int index, bool bitValue) {
			if (index < 0 || index > 63) return;
			var val = 1UL << index;
			value = bitValue ? (value | val) : (value & ~val);
		}
		public static void SetBit (this ref int value, int index, bool bitValue) {
			if (index < 0 || index > 31) return;
			var val = 1 << index;
			value = bitValue ? (value | val) : (value & ~val);
		}

		public static bool Almost (this float a, float b) => Mathf.Approximately(a, b);
		public static bool AlmostZero (this float a) => Mathf.Approximately(a, 0f);
		public static bool NotAlmost (this float a, float b) => !Mathf.Approximately(a, b);
		public static bool NotAlmostZero (this float a) => !Mathf.Approximately(a, 0f);
		public static bool GreaterOrAlmost (this float a, float b) => a > b || a.Almost(b);
		public static bool LessOrAlmost (this float a, float b) => a < b || a.Almost(b);

		public static int UDivide (this int value, int step) =>
				value > 0 || value % step == 0 ?
				value / step :
				value / step - 1;
		public static int UMod (this int value, int step) =>
			value > 0 || value % step == 0 ?
			value % step :
			value % step + step;
		public static int UCeil (this int value, int step) =>
			value % step == 0 ? value :
			value > 0 ? value - (value % step) + step :
			value - (value % step);
		public static int UFloor (this int value, int step) =>
			value % step == 0 ? value :
			value > 0 ? value - (value % step) :
			value - (value % step) - step;
		public static int Distance (this int value, int target) => Mathf.Abs(value - target);

		public static int CeilDivide (this int value, int target) => value / target + (value % target == 0 ? 0 : 1);

		public static int Abs (this int value) => value > 0 ? value : -value;
		public static float Abs (this float value) => value > 0 ? value : -value;

		public static int RoundToInt (this float a) => Mathf.RoundToInt(a);
		public static int CeilToInt (this float a) => Mathf.CeilToInt(a);
		public static int FloorToInt (this float a) => Mathf.FloorToInt(a);
		public static float UMod (this float value, float gap) =>
			value > 0 || value % gap == 0 ?
			value % gap :
			value % gap + gap;
		public static float UCeil (this float value, float gap) => value % gap == 0 ? value :
			value > 0 ? value - (value % gap) + gap :
			value - (value % gap);
		public static float UFloor (this float value, float gap) => value % gap == 0 ? value :
			value > 0 ? value - (value % gap) :
			value - (value % gap) - gap;

		public static int Clamp (this int a, int min, int max) {
			//if (min > max) (min, max) = (max, min);
			return a < min ? min : a > max ? max : a;
		}
		public static float Clamp (this float a, float min, float max) {
			//if (min > max) (min, max) = (max, min);
			return a < min ? min : a > max ? max : a;
		}

		public static float Clamp01 (this float value) => value < 0f ? 0f : value > 1f ? 1f : value;

		public static int GreaterOrEquel (this int value, int target) => value > target ? value : target;
		public static int LessOrEquel (this int value, int target) => value < target ? value : target;

		public static int GreaterOrEquelThanZero (this int value) => value > 0 ? value : 0;
		public static int LessOrEquelThanZero (this int value) => value < 0 ? value : 0;

		public static int Sign (this int i) => (i >= 0) ? 1 : -1;
		public static int Sign3 (this int i) => i == 0 ? 0 : i > 0 ? 1 : -1;

		public static int MoveTowards (this int current, int target, int maxDelta) {
			if (Mathf.Abs(target - current) <= maxDelta) {
				return target;
			}
			return current + (target - current).Sign() * maxDelta;
		}
		public static int MoveTowards (this int current, int target, int positiveDelta, int negativeDelta) => current.MoveTowards(
			target, Mathf.Abs(target) > Mathf.Abs(current) ? positiveDelta : negativeDelta
		);


		public static bool InRange (this int value, int min, int max) => value >= min && value <= max;
		public static bool InRangeExclude (this int value, int min, int max) => value > min && value < max;
		public static bool InLength (this int value, int length) => value >= 0 && value < length;


		public static int LerpTo (this int from, int to, float lerp01) =>
			from + ((to - from) * lerp01).RoundToInt();
		public static int LerpTo (this int from, int to, int lerpRate) {
			int result = from + ((to - from) * lerpRate / 1000);
			if (result == from && from != to && lerpRate != 0) {
				return to > from ? from + 1 : from - 1;
			}
			return result;
		}


		public static float LerpWithGap (this float from, float to, float lerp, float gap) => Mathf.Abs(from - to) > gap ? Mathf.LerpUnclamped(from, to, lerp) : to;


		public static int PingPong (this int value, int length) {
			value = value.UMod(length * 2);
			return length - Mathf.Abs(value - length);
		}
		public static int PingPong (this int value, int min, int max) {
			int length = max - min;
			value = value.UMod(length * 2);
			return length - Mathf.Abs(value - length) + min;
		}


		// Vector
		public static void Clamp (this ref Vector2Int v, int minX, int minY, int maxX, int maxY) {
			v.x = Mathf.Clamp(v.x, minX, maxX);
			v.y = Mathf.Clamp(v.y, minY, maxY);
		}
		public static void Clamp (this ref Vector2 v, float minX, float minY, float maxX, float maxY) {
			v.x = Mathf.Clamp(v.x, minX, maxX);
			v.y = Mathf.Clamp(v.y, minY, maxY);
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
		public static Vector3Int ToVector3Int (this Vector2Int v, int z) => new(v.x, v.y, z);


		public static bool Almost (this Vector3 a, Vector3 b) => Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y) && Mathf.Approximately(a.z, b.z);
		public static bool Almost (this Vector2 a, Vector2 b) => Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y);
		public static bool Almost (this Rect a, Rect b) => Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y) && Mathf.Approximately(a.width, b.width) && Mathf.Approximately(a.height, b.height);
		public static bool NotAlmost (this Vector3 a, Vector3 b) => !a.Almost(b);
		public static bool NotAlmost (this Vector2 a, Vector2 b) => !a.Almost(b);
		public static bool NotAlmost (this Rect a, Rect b) => !a.Almost(b);
		public static bool Inside01 (this Vector2 v) => Inside(v, 0f, 1f, 0f, 1f);
		public static bool Inside (this Vector2 v, Vector2 min, Vector2 max) => Inside(v, min.x, max.x, min.y, max.y);
		public static bool Inside (this Vector2 v, float left, float right, float down, float up) => v.x >= left && v.x <= right && v.y >= down && v.y <= up;
		public static Vector2Int RoundToInt (this Vector2 v) => new(
			Mathf.RoundToInt(v.x),
			Mathf.RoundToInt(v.y)
		);
		public static Vector2Int CeilToInt (this Vector2 v) => new(
			Mathf.CeilToInt(v.x),
			Mathf.CeilToInt(v.y)
		);
		public static Vector2Int FloorToInt (this Vector2 v) => new(
			Mathf.FloorToInt(v.x),
			Mathf.FloorToInt(v.y)
		);
		public static bool AlmostZero (this Vector4 value) => value.x.AlmostZero() && value.y.AlmostZero() && value.z.AlmostZero() && value.w.AlmostZero();


		public static float Duration (this AnimationCurve curve) {
			if (curve.length <= 1) { return 0f; }
			return curve.keys[curve.length - 1].time - curve.keys[0].time;
		}


		// Transform
		public static void DestroyAllChildrenImmediate (this Transform target) {
			int childCount = target.childCount;
			for (int i = 0; i < childCount; i++) {
				Object.DestroyImmediate(target.GetChild(0).gameObject, false);
			}
		}


		public static bool IsChildOf (this Transform tf, Transform root) {
			while (tf != null && tf != root) {
				tf = tf.parent;
			}
			return tf == root;
		}


		public static void SetHideFlagForAllChildren (this Transform target, HideFlags flag) {
			target.gameObject.hideFlags = flag;
			foreach (Transform t in target) {
				SetHideFlagForAllChildren(t, flag);
			}
		}


		public static void ClampInsideParent (this RectTransform target) {
			target.anchoredPosition = VectorClamp2(
				target.anchoredPosition,
				target.pivot * target.rect.size - target.anchorMin * ((RectTransform)target.parent).rect.size,
				(Vector2.one - target.anchorMin) * ((RectTransform)target.parent).rect.size - (Vector2.one - target.pivot) * target.rect.size
			);
			static Vector2 VectorClamp2 (Vector2 v, Vector2 min, Vector2 max) => new(
				Mathf.Clamp(v.x, min.x, max.x),
				Mathf.Clamp(v.y, min.y, max.y)
			);
		}


		public static Vector2 GetPosition01 (this RectTransform rt, Vector2 screenPos, Camera camera) {
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenPos, camera, out Vector2 localPos);
			return new Vector2(
				(localPos.x - rt.rect.xMin) / Mathf.Max(rt.rect.width, float.Epsilon),
				(localPos.y - rt.rect.yMin) / Mathf.Max(rt.rect.height, float.Epsilon)
			);
		}


		public static int LengthInt (this Vector2Int v) => Util.DistanceInt(v.x, v.y, 0, 0);


		// Coroutine
		public static Coroutine StartBetterCoroutine (this MonoBehaviour beh, IEnumerator routine, System.Action onFinished = null) => beh.StartBetterCoroutine(routine, (ex) => onFinished?.Invoke());
		public static Coroutine StartBetterCoroutine (this MonoBehaviour beh, IEnumerator routine, System.Action<System.Exception> onFinished = null) {
			return beh.StartCoroutine(Coroutine(routine, onFinished));
			static IEnumerator Coroutine (IEnumerator routine, System.Action<System.Exception> onFinished) {
				while (true) {
					bool done;
					try {
						done = routine.MoveNext();
					} catch (System.Exception ex) {
						Debug.LogException(ex);
						onFinished?.Invoke(ex);
						yield break;
					}
					if (done) {
						yield return routine.Current;
					} else { break; }
				}
				onFinished?.Invoke(null);
			}
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
				rect.x + Mathf.Abs(rect.width - sizeX) * pivotX,
				rect.y + Mathf.Abs(rect.height - sizeY) * pivotY,
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
				rect.x - Mathf.Abs(rect.width - sizeX) / 2f,
				rect.y - Mathf.Abs(rect.height - sizeY) / 2f,
				sizeX, sizeY
			);
		}
		public static RectInt ToRectInt (this Rect rect) => new((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
		public static void Clamp (this ref Rect rect, Rect target) => rect = Rect.MinMaxRect(
			Mathf.Min(target.xMax, Mathf.Max(rect.xMin, target.xMin)),
			Mathf.Min(target.yMax, Mathf.Max(rect.yMin, target.yMin)),
			Mathf.Max(target.xMin, Mathf.Min(rect.xMax, target.xMax)),
			Mathf.Max(target.yMin, Mathf.Min(rect.yMax, target.yMax))
		);
		public static Rect GetClamp (this Rect rect, Rect target) => Rect.MinMaxRect(
			Mathf.Min(target.xMax, Mathf.Max(rect.xMin, target.xMin)),
			Mathf.Min(target.yMax, Mathf.Max(rect.yMin, target.yMin)),
			Mathf.Max(target.xMin, Mathf.Min(rect.xMax, target.xMax)),
			Mathf.Max(target.yMin, Mathf.Min(rect.yMax, target.yMax))
		);
		public static Vector2 BottomLeft (this Rect rect) => new(rect.xMin, rect.yMin);
		public static Vector2 BottomRight (this Rect rect) => new(rect.xMax, rect.yMin);
		public static Vector2 TopLeft (this Rect rect) => new(rect.xMin, rect.yMax);
		public static Vector2 TopRight (this Rect rect) => new(rect.xMax, rect.yMax);

		// RectInt
		public static Rect ToRect (this RectInt rect) => new(rect.x, rect.y, rect.width, rect.height);
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
				rect.x + Mathf.Abs(rect.width - sizeX) * pivotX / 1000,
				rect.y + Mathf.Abs(rect.height - sizeY) * pivotY / 1000,
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
		public static RectInt Expand (this RectInt rect, Vector4Int offset) {
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
		public static RectInt Shrink (this RectInt rect, Vector4Int offset) {
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
		public static Vector2Int CenterInt (this RectInt rect) => new(rect.x + rect.width / 2, rect.y + rect.height / 2);
		public static RectInt UDivide (this RectInt rect, int divide) {
			rect.SetMinMax(rect.min.UDivide(divide), rect.max.UDivide(divide));
			return rect;
		}
		public static Vector2Int BottomLeft (this RectInt rect) => new(rect.xMin, rect.yMin);
		public static Vector2Int BottomRight (this RectInt rect) => new(rect.xMax, rect.yMin);
		public static Vector2Int TopLeft (this RectInt rect) => new(rect.xMin, rect.yMax);
		public static Vector2Int TopRight (this RectInt rect) => new(rect.xMax, rect.yMax);

		public static void ClampPositionInside (ref this RectInt rect, RectInt bounds) {
			rect.x = rect.x.Clamp(bounds.x, bounds.xMax - rect.width);
			rect.y = rect.y.Clamp(bounds.y, bounds.yMax - rect.height);
		}


		// Misc
		public static bool IsSame (this Color32 a, Color32 b, bool ignoreAlpha = false) => a.r == b.r && a.g == b.g && a.b == b.b && (ignoreAlpha || a.a == b.a);
		public static Color32 Mult (this Color32 a, Color32 b) {
			a.r = (byte)(a.r * b.r / 255);
			a.g = (byte)(a.g * b.g / 255);
			a.b = (byte)(a.b * b.b / 255);
			a.a = (byte)(a.a * b.a / 255);
			return a;
		}


		public static void TryAdd<T> (this HashSet<T> hash, T value) {
			if (!hash.Contains(value)) hash.Add(value);
		}


		public static bool NotEnd (this BinaryReader reader) => reader.BaseStream.Position < reader.BaseStream.Length;


		public static bool IsZero (this RectOffset value) => value.left == 0 && value.right == 0 && value.top == 0 && value.bottom == 0;
		public static bool IsNotZero (this RectOffset value) => value.left != 0 || value.right != 0 || value.top != 0 || value.bottom != 0;
		public static void SetToZero (this RectOffset value) => value.left = value.right = value.top = value.bottom = 0;


		public static Direction4? GetDirection (this Gamekey key) => key switch {
			Gamekey.Down => Direction4.Down,
			Gamekey.Up => Direction4.Up,
			Gamekey.Left => Direction4.Left,
			Gamekey.Right => Direction4.Right,
			_ => null,
		};


		public static bool IsLeft (this Alignment alignment) => alignment == Alignment.BottomLeft || alignment == Alignment.MidLeft || alignment == Alignment.TopLeft;
		public static bool IsMidX (this Alignment alignment) => alignment == Alignment.BottomMid || alignment == Alignment.MidMid || alignment == Alignment.TopMid;
		public static bool IsRight (this Alignment alignment) => alignment == Alignment.BottomRight || alignment == Alignment.MidRight || alignment == Alignment.TopRight;
		public static bool IsBottom (this Alignment alignment) => alignment == Alignment.BottomLeft || alignment == Alignment.BottomMid || alignment == Alignment.BottomRight;
		public static bool IsMidY (this Alignment alignment) => alignment == Alignment.MidLeft || alignment == Alignment.MidMid || alignment == Alignment.MidRight;
		public static bool IsTop (this Alignment alignment) => alignment == Alignment.TopLeft || alignment == Alignment.TopMid || alignment == Alignment.TopRight;


		// Enum
		public static int EnumLength (this System.Type @enum) => System.Enum.GetValues(@enum).Length;
		public static E Next<E> (this E @enum) where E : System.Enum {
			var type = typeof(E);
			int nextInt = ((int)(object)@enum) + 1;
			if (System.Enum.IsDefined(type, nextInt)) {
				return (E)System.Enum.ToObject(type, nextInt);
			}
			return @enum;
		}
		public static E Prev<E> (this E @enum) where E : System.Enum {
			var type = typeof(E);
			int nextInt = ((int)(object)@enum) - 1;
			if (System.Enum.IsDefined(type, nextInt)) {
				return (E)System.Enum.ToObject(type, nextInt);
			}
			return @enum;
		}


		// String
		public static string TrimWhiteForStartAndEnd (this string str) => str.TrimStart(' ', '\t').TrimEnd(' ', '\t');
		public static string TrimEnd_Numbers (this string str) => str.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
		public static string TrimEnd_NumbersEmpty (this string str) => str.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ' ');
		public static string TrimEnd_NumbersEmpty_ (this string str) => str.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ' ', '_');
		public static string TrimEnd_IfEndWith (this string str, string end, params char[] trimChars) {
			if (str.EndsWith(end)) {
				return str[..^end.Length].TrimEnd(trimChars);
			}
			return str;
		}

		public static bool StartsWith (this string str, char targetChar, char ignoreChar) {
			foreach (var c in str) {
				if (c == targetChar) {
					return true;
				} else if (c != ignoreChar) {
					return false;
				}
			}
			return false;
		}
		public static bool StartsWith_IgnoreWhiteSpace (this string str, char targetChar) => StartsWith_IgnoreWhiteSpace(str, targetChar, out _);
		public static bool StartsWith_IgnoreWhiteSpace (this string str, char targetChar, out int index) {
			index = -1;
			for (int i = 0; i < str.Length; i++) {
				char c = str[i];
				if (c == targetChar) {
					index = i;
					return true;
				} else if (!char.IsWhiteSpace(c)) {
					return false;
				}
			}
			return false;
		}


		// Ref
		public static IEnumerable<(string name, T value)> AllFields<T> (this object target) {
			var type = target.GetType();
			var tType = typeof(T);
			foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
				if (field.FieldType == tType || field.FieldType.IsSubclassOf(tType)) {
					yield return (field.Name, (T)field.GetValue(target));
				}
			}
		}


		public static IEnumerable<(string name, T value)> AllProperties<T> (this object target) {
			var type = target.GetType();
			var tType = typeof(T);
			foreach (var pro in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
				if (pro.PropertyType == tType || pro.PropertyType.IsSubclassOf(tType)) {
					yield return (pro.Name, (T)pro.GetValue(target));
				}
			}
		}


	}
}