using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;


namespace AngeliaFramework {
	public static class Extension {


		// Number
		public static bool GetBit (this ulong value, int index) => (value & (1UL << index)) != 0;
		public static bool GetBit (this int value, int index) => (value & (1 << index)) != 0;
		public static bool GetBit (this ushort value, int index) => (value & (1 << index)) != 0;
		public static bool GetBit (this byte value, int index) => (value & (1 << index)) != 0;

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
		public static void SetBit (this ref byte value, int index, bool bitValue) {
			if (index < 0 || index > 7) return;
			var val = 1 << index;
			value = (byte)(bitValue ? (value | val) : (value & ~val));
		}

		public static bool Almost (this float a, float b) => Util.Approximately(a, b);
		public static bool AlmostZero (this float a) => Util.Approximately(a, 0f);
		public static bool NotAlmost (this float a, float b) => !Util.Approximately(a, b);
		public static bool NotAlmostZero (this float a) => !Util.Approximately(a, 0f);
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
		public static int Distance (this int value, int target) => Util.Abs(value - target);

		public static int CeilDivide (this int value, int target) => value / target + (value % target == 0 ? 0 : 1);

		public static int Abs (this int value) => value > 0 ? value : -value;
		public static float Abs (this float value) => value > 0 ? value : -value;

		public static int RoundToInt (this float a) => (int)System.Math.Round(a);
		public static int CeilToInt (this float a) => (int)System.Math.Ceiling(a);
		public static int FloorToInt (this float a) => (int)System.Math.Floor(a);
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

		public static int Clamp (this int a, int min, int max) => a < min ? min : a > max ? max : a;
		public static float Clamp (this float a, float min, float max) => a < min ? min : a > max ? max : a;
		public static float Clamp01 (this float value) => value < 0f ? 0f : value > 1f ? 1f : value;

		public static int GreaterOrEquel (this int value, int target) => value > target ? value : target;
		public static int LessOrEquel (this int value, int target) => value < target ? value : target;

		public static int GreaterOrEquelThanZero (this int value) => value > 0 ? value : 0;
		public static int LessOrEquelThanZero (this int value) => value < 0 ? value : 0;

		public static int Sign (this int i) => (i >= 0) ? 1 : -1;
		public static int Sign3 (this int i) => i == 0 ? 0 : i > 0 ? 1 : -1;

		public static int MoveTowards (this int current, int target, int maxDelta) {
			if (Util.Abs(target - current) <= maxDelta) {
				return target;
			}
			return current + (target - current).Sign() * maxDelta;
		}
		public static int MoveTowards (this int current, int target, int positiveDelta, int negativeDelta) => current.MoveTowards(
			target, Util.Abs(target) > Util.Abs(current) ? positiveDelta : negativeDelta
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

		public static float LerpWithGap (this float from, float to, float lerp, float gap) => Util.Abs(from - to) > gap ? Util.LerpUnclamped(from, to, lerp) : to;


		public static int PingPong (this int value, int length) {
			value = value.UMod(length * 2);
			return length - Util.Abs(value - length);
		}
		public static int PingPong (this int value, int min, int max) {
			int length = max - min;
			value = value.UMod(length * 2);
			return length - Util.Abs(value - length) + min;
		}


		public static int DigitCount (this int n) {
			if (n >= 0) {
				if (n < 10) return 1;
				if (n < 100) return 2;
				if (n < 1000) return 3;
				if (n < 10000) return 4;
				if (n < 100000) return 5;
				if (n < 1000000) return 6;
				if (n < 10000000) return 7;
				if (n < 100000000) return 8;
				if (n < 1000000000) return 9;
				return 10;
			} else {
				if (n > -10) return 1;
				if (n > -100) return 2;
				if (n > -1000) return 3;
				if (n > -10000) return 4;
				if (n > -100000) return 5;
				if (n > -1000000) return 6;
				if (n > -10000000) return 7;
				if (n > -100000000) return 8;
				if (n > -1000000000) return 9;
				return 10;
			}
		}


		// Vector
		public static void Clamp (this ref Int2 v, int minX, int minY, int maxX, int maxY) {
			v.x = Util.Clamp(v.x, minX, maxX);
			v.y = Util.Clamp(v.y, minY, maxY);
		}
		public static void Clamp (this ref Float2 v, float minX, float minY, float maxX, float maxY) {
			v.x = Util.Clamp(v.x, minX, maxX);
			v.y = Util.Clamp(v.y, minY, maxY);
		}
		public static Int2 UDivide (this Int2 v, int gap) {
			v.x = v.x.UDivide(gap);
			v.y = v.y.UDivide(gap);
			return v;
		}
		public static Int2 Clamped (this Int2 v, int minX, int minY, int maxX = int.MaxValue, int maxY = int.MaxValue) {
			v.Clamp(minX, minY, maxX, maxY);
			return v;
		}
		public static Int3 ToVector3Int (this Int2 v, int z) => new(v.x, v.y, z);
		public static Int2 MoveTowards (this Int2 v, Int2 target, int delta) => new(
			v.x.MoveTowards(target.x, delta), v.y.MoveTowards(target.y, delta)
		);
		public static Int2 MoveTowards (this Int2 v, Int2 target, Int2 delta) => new(
			v.x.MoveTowards(target.x, delta.x), v.y.MoveTowards(target.y, delta.y)
		);


		public static bool Almost (this Float3 a, Float3 b) => Util.Approximately(a.x, b.x) && Util.Approximately(a.y, b.y) && Util.Approximately(a.z, b.z);
		public static bool Almost (this Float2 a, Float2 b) => Util.Approximately(a.x, b.x) && Util.Approximately(a.y, b.y);
		public static bool Almost (this FRect a, FRect b) => Util.Approximately(a.x, b.x) && Util.Approximately(a.y, b.y) && Util.Approximately(a.width, b.width) && Util.Approximately(a.height, b.height);
		public static bool NotAlmost (this Float3 a, Float3 b) => !a.Almost(b);
		public static bool NotAlmost (this Float2 a, Float2 b) => !a.Almost(b);
		public static bool NotAlmost (this FRect a, FRect b) => !a.Almost(b);
		public static bool Inside01 (this Float2 v) => Inside(v, 0f, 1f, 0f, 1f);
		public static bool Inside (this Float2 v, Float2 min, Float2 max) => Inside(v, min.x, max.x, min.y, max.y);
		public static bool Inside (this Float2 v, float left, float right, float down, float up) => v.x >= left && v.x <= right && v.y >= down && v.y <= up;
		public static Int2 RoundToInt (this Float2 v) => new(
			Util.RoundToInt(v.x),
			Util.RoundToInt(v.y)
		);
		public static Int2 CeilToInt (this Float2 v) => new(
			Util.CeilToInt(v.x),
			Util.CeilToInt(v.y)
		);
		public static Int2 FloorToInt (this Float2 v) => new(
			Util.FloorToInt(v.x),
			Util.FloorToInt(v.y)
		);
		public static bool AlmostZero (this Float4 value) => value.x.AlmostZero() && value.y.AlmostZero() && value.z.AlmostZero() && value.w.AlmostZero();


		public static Float3 Rotate (this Float3 vector, float angle) {
			angle = -angle * System.MathF.PI / 180f;
			float sinAngle = (float)System.Math.Sin(angle);
			float cosAngle = (float)System.Math.Cos(angle);
			return new Float3(
				vector.x * cosAngle - vector.y * sinAngle,
				vector.x * sinAngle + vector.y * cosAngle,
				vector.z
			);
		}
		public static Float2 Rotate (this Float2 vector, float angle) {
			angle = -angle * System.MathF.PI / 180f;
			float sinAngle = (float)System.Math.Sin(angle);
			float cosAngle = (float)System.Math.Cos(angle);
			return new Float2(
				vector.x * cosAngle - vector.y * sinAngle,
				vector.x * sinAngle + vector.y * cosAngle
			);
		}
		public static int GetRotation (this Float2 vector) => ((float)(System.Math.Atan2(vector.x, vector.y) * Util.Rad2Deg)).RoundToInt();
		public static int GetRotation (this Int2 vector) => ((float)(System.Math.Atan2(vector.x, vector.y) * Util.Rad2Deg)).RoundToInt();


		// Direction
		public static bool IsHorizontal (this Direction4 dir) => dir == Direction4.Left || dir == Direction4.Right;
		public static bool IsVertical (this Direction4 dir) => dir == Direction4.Down || dir == Direction4.Up;


		public static Direction4 Opposite (this Direction4 dir) => dir switch {
			Direction4.Down => Direction4.Up,
			Direction4.Up => Direction4.Down,
			Direction4.Left => Direction4.Right,
			Direction4.Right => Direction4.Left,
			_ => throw new System.NotImplementedException(),
		};


		public static Direction3 Opposite (this Direction3 dir) => dir switch {
			Direction3.Down => Direction3.Up,
			Direction3.Up => Direction3.Down,
			Direction3.None => Direction3.None,
			_ => throw new System.NotImplementedException(),
		};


		public static Direction4 Clockwise (this Direction4 dir) => dir switch {
			Direction4.Down => Direction4.Left,
			Direction4.Left => Direction4.Up,
			Direction4.Up => Direction4.Right,
			Direction4.Right => Direction4.Down,
			_ => throw new System.NotImplementedException(),
		};


		public static Direction4 AntiClockwise (this Direction4 dir) => dir switch {
			Direction4.Down => Direction4.Right,
			Direction4.Right => Direction4.Up,
			Direction4.Up => Direction4.Left,
			Direction4.Left => Direction4.Down,
			_ => throw new System.NotImplementedException(),
		};


		public static Int2 Normal (this Direction4 dir) => dir switch {
			Direction4.Down => new(0, -1),
			Direction4.Up => new(0, 1),
			Direction4.Left => new(-1, 0),
			Direction4.Right => new(1, 0),
			_ => throw new System.NotImplementedException(),
		};


		public static int GetRotation (this Direction4 dir) => dir switch {
			Direction4.Up => 0,
			Direction4.Down => 180,
			Direction4.Left => -90,
			Direction4.Right => 90,
			_ => 0,
		};


		// Rect
		public static FRect Shift (this FRect rect, float x, float y) {
			rect.x += x;
			rect.y += y;
			return rect;
		}
		public static FRect Expand (this FRect rect, float offset) => rect.Expand(offset, offset, offset, offset);
		public static FRect Expand (this FRect rect, float l, float r, float d, float u) {
			rect.x -= l;
			rect.y -= d;
			rect.width += l + r;
			rect.height += d + u;
			return rect;
		}
		public static FRect Shrink (this FRect rect, float offset) => rect.Expand(-offset);
		public static FRect Shrink (this FRect rect, float l, float r, float d, float u) => rect.Expand(-l, -r, -d, -u);
		public static FRect Fit (this FRect rect, float targetAspect, float pivotX = 0.5f, float pivotY = 0.5f) {
			float sizeX = rect.width;
			float sizeY = rect.height;
			if (targetAspect > rect.width / rect.height) {
				sizeY = sizeX / targetAspect;
			} else {
				sizeX = sizeY * targetAspect;
			}
			return new FRect(
				rect.x + Util.Abs(rect.width - sizeX) * pivotX,
				rect.y + Util.Abs(rect.height - sizeY) * pivotY,
				sizeX, sizeY
			);
		}
		public static FRect Envelope (this FRect rect, float targetAspect) {
			float sizeX = rect.width;
			float sizeY = rect.height;
			if (targetAspect < rect.width / rect.height) {
				sizeY = sizeX / targetAspect;
			} else {
				sizeX = sizeY * targetAspect;
			}
			return new FRect(
				rect.x - Util.Abs(rect.width - sizeX) / 2f,
				rect.y - Util.Abs(rect.height - sizeY) / 2f,
				sizeX, sizeY
			);
		}
		public static IRect ToRectInt (this FRect rect) => new((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
		public static void Clamp (this ref FRect rect, FRect target) => rect = FRect.MinMaxRect(
			Util.Min(target.xMax, Util.Max(rect.xMin, target.xMin)),
			Util.Min(target.yMax, Util.Max(rect.yMin, target.yMin)),
			Util.Max(target.xMin, Util.Min(rect.xMax, target.xMax)),
			Util.Max(target.yMin, Util.Min(rect.yMax, target.yMax))
		);
		public static FRect GetClamp (this FRect rect, FRect target) => FRect.MinMaxRect(
			Util.Min(target.xMax, Util.Max(rect.xMin, target.xMin)),
			Util.Min(target.yMax, Util.Max(rect.yMin, target.yMin)),
			Util.Max(target.xMin, Util.Min(rect.xMax, target.xMax)),
			Util.Max(target.yMin, Util.Min(rect.yMax, target.yMax))
		);

		public static Float2 BottomLeft (this FRect rect) => new(rect.xMin, rect.yMin);
		public static Float2 BottomRight (this FRect rect) => new(rect.xMax, rect.yMin);
		public static Float2 TopLeft (this FRect rect) => new(rect.xMin, rect.yMax);
		public static Float2 TopRight (this FRect rect) => new(rect.xMax, rect.yMax);

		public static FRect Edge (this FRect rect, Direction4 edge, float thickness = 1f) => edge switch {
			Direction4.Up => rect.Shrink(0, 0, rect.height, -thickness),
			Direction4.Down => rect.Shrink(0, 0, -thickness, rect.height),
			Direction4.Left => rect.Shrink(-thickness, rect.width, 0, 0),
			Direction4.Right => rect.Shrink(rect.width, -thickness, 0, 0),
			_ => throw new System.NotImplementedException(),
		};


		// RectInt
		public static FRect ToRect (this IRect rect) => new(rect.x, rect.y, rect.width, rect.height);
		public static IRect Fit (this IRect rect, AngeSprite sprite, int pivotX = 500, int pivotY = 500) => rect.Fit(sprite.GlobalWidth, sprite.GlobalHeight, pivotX, pivotY);
		public static IRect Fit (this IRect rect, int outterWidth, int outterHeight, int pivotX = 500, int pivotY = 500) {
			if (outterWidth * outterHeight == 0) return rect;
			int sizeX = rect.width;
			int sizeY = rect.height;
			if (outterWidth * rect.height > rect.width * outterHeight) {
				sizeY = sizeX * outterHeight / outterWidth;
			} else {
				sizeX = sizeY * outterWidth / outterHeight;
			}
			return new IRect(
				rect.x + Util.Abs(rect.width - sizeX) * pivotX / 1000,
				rect.y + Util.Abs(rect.height - sizeY) * pivotY / 1000,
				sizeX, sizeY
			);
		}
		public static IRect Envelope (this IRect innerRect, int aspWidth, int aspHeight) {
			if (innerRect.width * innerRect.height == 0) return innerRect;
			if (innerRect.width * aspHeight > aspWidth * innerRect.height) {
				int newHeight = aspHeight * innerRect.width / aspWidth;
				return new IRect(
					innerRect.x,
					innerRect.y - (newHeight - innerRect.height) / 2,
					innerRect.width,
					newHeight
				);
			} else {
				int newWidth = aspWidth * innerRect.height / aspHeight;
				return new IRect(
					innerRect.x - (newWidth - innerRect.width) / 2,
					innerRect.y,
					newWidth,
					innerRect.height
				);
			}
		}
		public static IRect Expand (this IRect rect, int offset) => rect.Expand(offset, offset, offset, offset);
		public static IRect Expand (this IRect rect, int l, int r, int d, int u) {
			rect.x -= l;
			rect.y -= d;
			rect.width += l + r;
			rect.height += d + u;
			return rect;
		}
		public static IRect Expand (this IRect rect, IRectOffset offset) {
			rect.x -= offset.left;
			rect.y -= offset.bottom;
			rect.width += offset.horizontal;
			rect.height += offset.vertical;
			return rect;
		}
		public static IRect Expand (this IRect rect, Int4 offset) {
			rect.x -= offset.left;
			rect.y -= offset.down;
			rect.width += offset.left + offset.right;
			rect.height += offset.down + offset.up;
			return rect;
		}
		public static IRect Shrink (this IRect rect, int offset) => rect.Expand(-offset);
		public static IRect Shrink (this IRect rect, int l, int r, int d, int u) => rect.Expand(-l, -r, -d, -u);
		public static IRect Shrink (this IRect rect, IRectOffset offset) {
			rect.x += offset.left;
			rect.y += offset.bottom;
			rect.width -= offset.horizontal;
			rect.height -= offset.vertical;
			return rect;
		}
		public static IRect Shrink (this IRect rect, Int4 offset) {
			rect.x += offset.left;
			rect.y += offset.down;
			rect.width -= offset.left + offset.right;
			rect.height -= offset.down + offset.up;
			return rect;
		}
		public static void FlipHorizontal (this ref IRect rect) {
			rect.x += rect.width;
			rect.width = -rect.width;
		}
		public static void FlipVertical (this ref IRect rect) {
			rect.y += rect.height;
			rect.height = -rect.height;
		}
		public static void FlipNegative (this ref IRect rect) {
			if (rect.width < 0) {
				rect.x += rect.width;
				rect.width = -rect.width;
			}
			if (rect.height < 0) {
				rect.y += rect.height;
				rect.height = -rect.height;
			}
		}
		public static IRect GetFlipNegative (this IRect rect) {
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
		public static IRect Shift (this IRect rect, int x, int y) {
			rect.x += x;
			rect.y += y;
			return rect;
		}
		public static void SetMinMax (this ref IRect rect, int xMin, int xMax, int yMin, int yMax) {
			rect.x = xMin;
			rect.y = yMin;
			rect.width = xMax - xMin;
			rect.height = yMax - yMin;
		}

		public static bool IsSame (this IRect a, IRect b) => a.x == b.x && a.y == b.y && a.width == b.width && a.height == b.height;
		public static bool IsNotSame (this IRect a, IRect b) => a.x != b.x || a.y != b.y || a.width != b.width || a.height != b.height;
		public static bool Contains (this IRect rect, int x, int y) => rect.Contains(new Int2(x, y));
		public static int CenterX (this IRect rect) => rect.x + rect.width / 2;
		public static int CenterY (this IRect rect) => rect.y + rect.height / 2;
		public static Int2 CenterInt (this IRect rect) => new(rect.x + rect.width / 2, rect.y + rect.height / 2);
		public static IRect UDivide (this IRect rect, int divide) {
			rect.SetMinMax(rect.min.UDivide(divide), rect.max.UDivide(divide));
			return rect;
		}
		public static Int2 BottomLeft (this IRect rect) => new(rect.xMin, rect.yMin);
		public static Int2 BottomRight (this IRect rect) => new(rect.xMax, rect.yMin);
		public static Int2 TopLeft (this IRect rect) => new(rect.xMin, rect.yMax);
		public static Int2 TopRight (this IRect rect) => new(rect.xMax, rect.yMax);

		public static void ClampPositionInside (ref this IRect rect, IRect bounds) {
			rect.x = rect.x.Clamp(bounds.x, bounds.xMax - rect.width);
			rect.y = rect.y.Clamp(bounds.y, bounds.yMax - rect.height);
		}

		public static IRect LerpTo (this IRect from, IRect to, int lerpRate) => new(
			from.x.LerpTo(to.x, lerpRate),
			from.y.LerpTo(to.y, lerpRate),
			from.width.LerpTo(to.width, lerpRate),
			from.height.LerpTo(to.height, lerpRate)
		);
		public static IRect LerpTo (this IRect from, IRect to, float lerp) => new(
			from.x.LerpTo(to.x, lerp),
			from.y.LerpTo(to.y, lerp),
			from.width.LerpTo(to.width, lerp),
			from.height.LerpTo(to.height, lerp)
		);

		public static IRect EdgeInside (this IRect rect, Direction4 edge, int thickness = 1) => edge switch {
			Direction4.Up => rect.Shrink(0, 0, rect.height - thickness, 0),
			Direction4.Down => rect.Shrink(0, 0, 0, rect.height - thickness),
			Direction4.Left => rect.Shrink(0, rect.width - thickness, 0, 0),
			Direction4.Right => rect.Shrink(rect.width - thickness, 0, 0, 0),
			_ => throw new System.NotImplementedException(),
		};
		public static IRect EdgeOutside (this IRect rect, Direction4 edge, int thickness = 1) => edge switch {
			Direction4.Up => rect.Shrink(0, 0, rect.height, -thickness),
			Direction4.Down => rect.Shrink(0, 0, -thickness, rect.height),
			Direction4.Left => rect.Shrink(-thickness, rect.width, 0, 0),
			Direction4.Right => rect.Shrink(rect.width, -thickness, 0, 0),
			_ => throw new System.NotImplementedException(),
		};


		// Misc
		public static bool IsSame (this Byte4 a, Byte4 b, bool ignoreAlpha = false) => a.r == b.r && a.g == b.g && a.b == b.b && (ignoreAlpha || a.a == b.a);
		public static Byte4 Mult (this Byte4 a, Byte4 b) {
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


		public static bool IsZero (this IRectOffset value) => value.left == 0 && value.right == 0 && value.top == 0 && value.bottom == 0;
		public static bool IsNotZero (this IRectOffset value) => value.left != 0 || value.right != 0 || value.top != 0 || value.bottom != 0;
		public static void SetToZero (this IRectOffset value) => value.left = value.right = value.top = value.bottom = 0;


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


		public static A[] FillWithValue<A> (this A[] arr, A value) {
			for (int i = 0; i < arr.Length; i++) {
				arr[i] = value;
			}
			return arr;
		}
		public static A[] FillWithNewValue<A> (this A[] arr) where A : new() {
			for (int i = 0; i < arr.Length; i++) {
				arr[i] = new A();
			}
			return arr;
		}


		public static void AddRange<T, V> (this Dictionary<T, V> map, IEnumerable<KeyValuePair<T, V>> values) {
			foreach (var (k, v) in values) map.TryAdd(k, v);
		}


		public static Byte4 WithNewA (this Byte4 value, byte a) => new(value.r, value.g, value.b, a);


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