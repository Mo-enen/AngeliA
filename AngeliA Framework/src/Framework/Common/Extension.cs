using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace AngeliA;

public static class Extension {


	private const MethodImplOptions INLINE = MethodImplOptions.AggressiveInlining;
	private static readonly StringBuilder TypePathBuilder = new();


	// AngeliA Hash Code
	public static string AngeName (this System.Type type) {
		string name = type.Name;
		if (char.IsLower(name[0])) name = name[1..];
		return name;
	}

	public static int AngeHash (this System.Type type) => type.AngeName().AngeHash();

	public static int AngeHash (this string str, int avoid = 0) {
		const int p = 31;
		const int m = 1837465129;
		int hash_value = 0;
		int p_pow = 1;
		for (int i = 0; i < str.Length; i++) {
			char c = str[i];
			hash_value = (hash_value + (c - 'a' + 1) * p_pow) % m;
			p_pow = (p_pow * p) % m;
		}
		return hash_value == avoid ? avoid + 1 : hash_value;
	}

	// Misc
	public static System.Span<T> GetSpan<T> (this List<T> list) => CollectionsMarshal.AsSpan(list);
	public static System.Span<T> GetSpan<T> (this T[] arr) => new(arr);
	public static System.ReadOnlySpan<T> GetReadOnlySpan<T> (this T[] arr) => new(arr);

	public static string ToStringWithDoubleQuotes (this string str) => $"\"{str}\"";
	public static string ToStringWithDoubleQuotes (this StringBuilder builder) => $"\"{builder}\"";

	public static void AddDistinct<T> (this IList<T> list, T item) {
		if (!list.Contains(item)) list.Add(item);
	}

	public static void ForEach<T> (this object[] arr, System.Action<T> action) {
		foreach (var a in arr) {
			if (a is T t) action(t);
		}
	}

	public static void ForEach<T> (this object[] arr, System.Action<T, int> action) {
		int index = 0;
		foreach (var a in arr) {
			if (a is T t) {
				action(t, index);
				index++;
			}
		}
	}

	public static void AppendWithDoubleQuotes (this StringBuilder builder, string content) {
		builder.Append('"');
		builder.Append(content);
		builder.Append('"');
	}

	public static byte[] Pixels_to_Bytes (this Color32[] pixels) {
		var bytes = new byte[pixels.Length * 4];
		var bSpan = new System.Span<byte>(bytes);
		var pixSpan = new System.ReadOnlySpan<Color32>(pixels);
		int len = pixels.Length;
		for (int i = 0; i < len; i++) {
			var p = pixSpan[i];
			bSpan[i * 4 + 0] = p.r;
			bSpan[i * 4 + 1] = p.g;
			bSpan[i * 4 + 2] = p.b;
			bSpan[i * 4 + 3] = p.a;
		}
		return bytes;
	}

	public static Color32[] Bytes_to_Pixels (this byte[] bytes, int width, int height) {
		var pixels = new Color32[width * height];
		var bSpan = new System.ReadOnlySpan<byte>(bytes);
		var pSpan = new System.Span<Color32>(pixels);
		int index = 0;
		int len = pSpan.Length;
		for (int i = 0; i < len; i++) {
			pSpan[i] = new Color32(
				bSpan[index + 0],
				bSpan[index + 1],
				bSpan[index + 2],
				bSpan[index + 3]
			);
			index += 4;
		}
		return pixels;
	}

	[MethodImpl(INLINE)] public static bool GetBit (this ulong value, int index) => (value & (1UL << index)) != 0;
	[MethodImpl(INLINE)] public static bool GetBit (this int value, int index) => (value & (1 << index)) != 0;
	[MethodImpl(INLINE)] public static bool GetBit (this uint value, int index) => (value & (1U << index)) != 0;
	[MethodImpl(INLINE)] public static bool GetBit (this ushort value, int index) => (value & (1 << index)) != 0;
	[MethodImpl(INLINE)] public static bool GetBit (this byte value, int index) => (value & (1 << index)) != 0;

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
	public static void SetBit (this ref uint value, int index, bool bitValue) {
		if (index < 0 || index > 31) return;
		uint val = (uint)(1 << index);
		value = bitValue ? (value | val) : (value & ~val);
	}
	public static void SetBit (this ref byte value, int index, bool bitValue) {
		if (index < 0 || index > 7) return;
		var val = 1 << index;
		value = (byte)(bitValue ? (value | val) : (value & ~val));
	}

	[MethodImpl(INLINE)] public static bool Almost (this float a, float b) => Util.Approximately(a, b);
	[MethodImpl(INLINE)] public static bool AlmostZero (this float a) => Util.Approximately(a, 0f);
	[MethodImpl(INLINE)] public static bool NotAlmost (this float a, float b) => !Util.Approximately(a, b);
	[MethodImpl(INLINE)] public static bool NotAlmostZero (this float a) => !Util.Approximately(a, 0f);
	[MethodImpl(INLINE)] public static bool GreaterOrAlmost (this float a, float b) => a > b || a.Almost(b);
	[MethodImpl(INLINE)] public static bool LessOrAlmost (this float a, float b) => a < b || a.Almost(b);

	[MethodImpl(INLINE)]
	public static int UDivide (this int value, int step) =>
			value > 0 || value % step == 0 ?
			value / step :
			value / step - 1;
	[MethodImpl(INLINE)]
	public static int UMod (this int value, int step) =>
		value > 0 || value % step == 0 ?
		value % step :
		value % step + step;
	[MethodImpl(INLINE)]
	public static long UMod (this long value, long step) =>
		value > 0 || value % step == 0 ?
		value % step :
		value % step + step;
	[MethodImpl(INLINE)]
	public static int UCeil (this int value, int step) =>
		value % step == 0 ? value :
		value > 0 ? value - (value % step) + step :
		value - (value % step);
	[MethodImpl(INLINE)]
	public static int UFloor (this int value, int step) =>
		value % step == 0 ? value :
		value > 0 ? value - (value % step) :
		value - (value % step) - step;
	[MethodImpl(INLINE)] public static int Distance (this int value, int target) => Util.Abs(value - target);

	[MethodImpl(INLINE)] public static int CeilDivide (this int value, int target) => value / target + (value % target == 0 ? 0 : 1);

	[MethodImpl(INLINE)] public static int Abs (this int value) => value > 0 ? value : -value;
	[MethodImpl(INLINE)] public static float Abs (this float value) => value > 0 ? value : -value;

	[MethodImpl(INLINE)] public static int RoundToInt (this float a) => (int)System.Math.Round(a);
	[MethodImpl(INLINE)] public static int CeilToInt (this float a) => (int)System.Math.Ceiling(a);
	[MethodImpl(INLINE)] public static int FloorToInt (this float a) => (int)System.Math.Floor(a);
	[MethodImpl(INLINE)]
	public static float UMod (this float value, float gap) =>
		value > 0 || value % gap == 0 ?
		value % gap :
		value % gap + gap;
	[MethodImpl(INLINE)]
	public static float UCeil (this float value, float gap) => value % gap == 0 ? value :
		value > 0 ? value - (value % gap) + gap :
		value - (value % gap);
	[MethodImpl(INLINE)]
	public static float UFloor (this float value, float gap) => value % gap == 0 ? value :
		value > 0 ? value - (value % gap) :
		value - (value % gap) - gap;

	[MethodImpl(INLINE)]
	public static int ClampDisorder (this int a, int rangeA, int rangeB) {
		if (rangeA > rangeB) {
			(rangeA, rangeB) = (rangeB, rangeA);
		}
		return a.Clamp(rangeA, rangeB);
	}
	[MethodImpl(INLINE)] public static int Clamp (this int a, int min, int max) => a < min ? min : a > max ? max : a;
	[MethodImpl(INLINE)] public static float Clamp (this float a, float min, float max) => a < min ? min : a > max ? max : a;
	[MethodImpl(INLINE)] public static float Clamp01 (this float value) => value < 0f ? 0f : value > 1f ? 1f : value;

	[MethodImpl(INLINE)] public static int GreaterOrEquel (this int value, int target) => value > target ? value : target;
	[MethodImpl(INLINE)] public static long GreaterOrEquel (this long value, long target) => value > target ? value : target;
	[MethodImpl(INLINE)] public static int LessOrEquel (this int value, int target) => value < target ? value : target;

	[MethodImpl(INLINE)] public static int GreaterOrEquelThanZero (this int value) => value > 0 ? value : 0;
	[MethodImpl(INLINE)] public static int LessOrEquelThanZero (this int value) => value < 0 ? value : 0;

	[MethodImpl(INLINE)] public static int Sign (this int i) => i >= 0 ? 1 : -1;
	[MethodImpl(INLINE)] public static int Sign3 (this int i) => i == 0 ? 0 : i > 0 ? 1 : -1;

	[MethodImpl(INLINE)]
	public static int MoveTowards (this int current, int target, int maxDelta) {
		if (Util.Abs(target - current) <= maxDelta) {
			return target;
		}
		return current + (target - current).Sign() * maxDelta;
	}
	[MethodImpl(INLINE)]
	public static int MoveTowards (this int current, int target, int positiveDelta, int negativeDelta) => current.MoveTowards(
		target, Util.Abs(target) > Util.Abs(current) ? positiveDelta : negativeDelta
	);

	[MethodImpl(INLINE)] public static bool InRange (this int value, int min, int max) => value >= min && value <= max;
	[MethodImpl(INLINE)] public static bool InRangeExclude (this int value, int min, int max) => value > min && value < max;
	[MethodImpl(INLINE)] public static bool InLength (this int value, int length) => value >= 0 && value < length;

	[MethodImpl(INLINE)]
	public static int LerpTo (this int from, int to, float lerp01) =>
		from + ((to - from) * lerp01).RoundToInt();
	[MethodImpl(INLINE)]
	public static int LerpTo (this int from, int to, int lerpRate) {
		int result = from + ((to - from) * lerpRate / 1000);
		if (result == from && from != to && lerpRate != 0) {
			return to > from ? from + 1 : from - 1;
		}
		return result;
	}

	[MethodImpl(INLINE)] public static float LerpWithGap (this float from, float to, float lerp, float gap) => Util.Abs(from - to) > gap ? Util.LerpUnclamped(from, to, lerp) : to;

	[MethodImpl(INLINE)]
	public static int PingPong (this int value, int length) {
		value = value.UMod(length * 2);
		return length - Util.Abs(value - length);
	}
	[MethodImpl(INLINE)]
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

	// Coord
	[MethodImpl(INLINE)] public static int ToUnit (this int globalPos) => globalPos.UDivide(Const.CEL);
	[MethodImpl(INLINE)] public static int ToGlobal (this int unitPos) => unitPos * Const.CEL;
	[MethodImpl(INLINE)] public static int ToUnifyGlobal (this int globalPos) => globalPos.UDivide(Const.CEL) * Const.CEL;

	[MethodImpl(INLINE)] public static Int2 ToUnit (this Int2 globalPos) => globalPos.UDivide(Const.CEL);
	[MethodImpl(INLINE)] public static Int2 ToGlobal (this Int2 unitPos) => unitPos * Const.CEL;
	[MethodImpl(INLINE)] public static Int2 ToUnifyGlobal (this Int2 globalPos) => globalPos.ToUnit().ToGlobal();

	[MethodImpl(INLINE)]
	public static Int3 ToUnit (this Int3 globalPos) => new(
		globalPos.x.UDivide(Const.CEL),
		globalPos.y.UDivide(Const.CEL),
		globalPos.z
	);
	[MethodImpl(INLINE)]
	public static Int3 ToGlobal (this Int3 unitPos) => new(
		unitPos.x * Const.CEL,
		unitPos.y * Const.CEL,
		unitPos.z
	);
	[MethodImpl(INLINE)]
	public static Int3 ToUnifyGlobal (this Int3 globalPos) => new(
		globalPos.x.ToUnit().ToGlobal(),
		globalPos.y.ToUnit().ToGlobal(),
		globalPos.z
	);

	[MethodImpl(INLINE)] public static IRect ToUnit (this IRect global) => global.UDivide(Const.CEL);
	[MethodImpl(INLINE)]
	public static IRect ToGlobal (this IRect unit) => new(
		unit.x * Const.CEL,
		unit.y * Const.CEL,
		unit.width * Const.CEL,
		unit.height * Const.CEL
	);
	[MethodImpl(INLINE)] public static IRect ToUnifyGlobal (this IRect global) => global.ToUnit().ToGlobal();

	// Vector
	[MethodImpl(INLINE)]
	public static void Clamp (this ref Int2 v, int minX, int minY, int maxX, int maxY) {
		v.x = Util.Clamp(v.x, minX, maxX);
		v.y = Util.Clamp(v.y, minY, maxY);
	}
	[MethodImpl(INLINE)]
	public static void Clamp (this ref Float2 v, float minX, float minY, float maxX, float maxY) {
		v.x = Util.Clamp(v.x, minX, maxX);
		v.y = Util.Clamp(v.y, minY, maxY);
	}
	[MethodImpl(INLINE)]
	public static Int2 UDivide (this Int2 v, int gap) {
		v.x = v.x.UDivide(gap);
		v.y = v.y.UDivide(gap);
		return v;
	}
	[MethodImpl(INLINE)]
	public static Int2 Clamped (this Int2 v, int minX, int minY, int maxX = int.MaxValue, int maxY = int.MaxValue) {
		v.Clamp(minX, minY, maxX, maxY);
		return v;
	}
	[MethodImpl(INLINE)] public static Int3 ToVector3Int (this Int2 v, int z) => new(v.x, v.y, z);
	[MethodImpl(INLINE)]
	public static Int2 MoveTowards (this Int2 v, Int2 target, int delta) => new(
		v.x.MoveTowards(target.x, delta), v.y.MoveTowards(target.y, delta)
	);
	[MethodImpl(INLINE)]
	public static Int2 MoveTowards (this Int2 v, Int2 target, Int2 delta) => new(
		v.x.MoveTowards(target.x, delta.x), v.y.MoveTowards(target.y, delta.y)
	);
	[MethodImpl(INLINE)] public static Int2 Shift (this Int2 v, int x, int y) => new(v.x + x, v.y + y);
	[MethodImpl(INLINE)] public static Int2 ShiftX (this Int2 v, int x) => new(v.x + x, v.y);
	[MethodImpl(INLINE)] public static Int2 ShiftY (this Int2 v, int y) => new(v.x, v.y + y);

	[MethodImpl(INLINE)] public static bool Almost (this Float3 a, Float3 b) => Util.Approximately(a.x, b.x) && Util.Approximately(a.y, b.y) && Util.Approximately(a.z, b.z);
	[MethodImpl(INLINE)] public static bool Almost (this Float2 a, Float2 b) => Util.Approximately(a.x, b.x) && Util.Approximately(a.y, b.y);
	[MethodImpl(INLINE)] public static bool Almost (this FRect a, FRect b) => Util.Approximately(a.x, b.x) && Util.Approximately(a.y, b.y) && Util.Approximately(a.width, b.width) && Util.Approximately(a.height, b.height);
	[MethodImpl(INLINE)] public static bool NotAlmost (this Float3 a, Float3 b) => !a.Almost(b);
	[MethodImpl(INLINE)] public static bool NotAlmost (this Float2 a, Float2 b) => !a.Almost(b);
	[MethodImpl(INLINE)] public static bool NotAlmost (this FRect a, FRect b) => !a.Almost(b);
	[MethodImpl(INLINE)] public static bool Inside01 (this Float2 v) => Inside(v, 0f, 1f, 0f, 1f);
	[MethodImpl(INLINE)] public static bool Inside (this Float2 v, Float2 min, Float2 max) => Inside(v, min.x, max.x, min.y, max.y);
	[MethodImpl(INLINE)] public static bool Inside (this Float2 v, float left, float right, float down, float up) => v.x >= left && v.x <= right && v.y >= down && v.y <= up;
	[MethodImpl(INLINE)]
	public static Int2 RoundToInt (this Float2 v) => new(
		Util.RoundToInt(v.x),
		Util.RoundToInt(v.y)
	);
	[MethodImpl(INLINE)]
	public static Int2 CeilToInt (this Float2 v) => new(
		Util.CeilToInt(v.x),
		Util.CeilToInt(v.y)
	);
	[MethodImpl(INLINE)]
	public static Int2 FloorToInt (this Float2 v) => new(
		Util.FloorToInt(v.x),
		Util.FloorToInt(v.y)
	);
	[MethodImpl(INLINE)] public static bool AlmostZero (this Float4 value) => value.x.AlmostZero() && value.y.AlmostZero() && value.z.AlmostZero() && value.w.AlmostZero();

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
	[MethodImpl(INLINE)] public static int GetRotation (this Float2 vector) => ((float)(System.Math.Atan2(vector.x, vector.y) * Util.Rad2Deg)).RoundToInt();
	[MethodImpl(INLINE)] public static int GetRotation (this Int2 vector) => ((float)(System.Math.Atan2(vector.x, vector.y) * Util.Rad2Deg)).RoundToInt();

	[MethodImpl(INLINE)]
	public static bool TryGetDirection8 (this Int2 dir, out Direction8 result) {
		result =
			dir.x == 0 ? (dir.y < 0 ? Direction8.Bottom : Direction8.Top) :
			dir.x < 0 ? (dir.y == 0 ? Direction8.Left : dir.y < 0 ? Direction8.BottomLeft : Direction8.TopLeft) :
			(dir.y == 0 ? Direction8.Right : dir.y < 0 ? Direction8.BottomRight : Direction8.TopRight);
		return dir != Int2.zero;
	}

	// Direction
	[MethodImpl(INLINE)] public static bool IsHorizontal (this Direction4 dir) => dir == Direction4.Left || dir == Direction4.Right;
	[MethodImpl(INLINE)] public static bool IsVertical (this Direction4 dir) => dir == Direction4.Down || dir == Direction4.Up;

	[MethodImpl(INLINE)]
	public static Direction4 Opposite (this Direction4 dir) => dir switch {
		Direction4.Down => Direction4.Up,
		Direction4.Up => Direction4.Down,
		Direction4.Left => Direction4.Right,
		Direction4.Right => Direction4.Left,
		_ => throw new System.NotImplementedException(),
	};

	[MethodImpl(INLINE)]
	public static Direction3 Opposite (this Direction3 dir) => dir switch {
		Direction3.Down => Direction3.Up,
		Direction3.Up => Direction3.Down,
		Direction3.None => Direction3.None,
		_ => throw new System.NotImplementedException(),
	};

	[MethodImpl(INLINE)]
	public static Direction4 Clockwise (this Direction4 dir) => dir switch {
		Direction4.Down => Direction4.Left,
		Direction4.Left => Direction4.Up,
		Direction4.Up => Direction4.Right,
		Direction4.Right => Direction4.Down,
		_ => throw new System.NotImplementedException(),
	};
	[MethodImpl(INLINE)]
	public static Direction4 AntiClockwise (this Direction4 dir) => dir switch {
		Direction4.Down => Direction4.Right,
		Direction4.Right => Direction4.Up,
		Direction4.Up => Direction4.Left,
		Direction4.Left => Direction4.Down,
		_ => throw new System.NotImplementedException(),
	};
	[MethodImpl(INLINE)]
	public static Int2 Normal (this Direction4 dir) => dir switch {
		Direction4.Down => new(0, -1),
		Direction4.Up => new(0, 1),
		Direction4.Left => new(-1, 0),
		Direction4.Right => new(1, 0),
		_ => throw new System.NotImplementedException(),
	};
	[MethodImpl(INLINE)]
	public static Int2 Normal (this Direction8 dir) => dir switch {
		Direction8.Bottom => new(0, -1),
		Direction8.BottomLeft => new(-1, -1),
		Direction8.BottomRight => new(1, -1),
		Direction8.Top => new(0, 1),
		Direction8.TopLeft => new(-1, 1),
		Direction8.TopRight => new(1, 1),
		Direction8.Left => new(-1, 0),
		Direction8.Right => new(1, 0),
		_ => throw new System.NotImplementedException(),
	};
	[MethodImpl(INLINE)]
	public static int GetRotation (this Direction4 dir) => dir switch {
		Direction4.Up => 0,
		Direction4.Down => 180,
		Direction4.Left => -90,
		Direction4.Right => 90,
		_ => 0,
	};
	[MethodImpl(INLINE)]
	public static Direction8 Clockwise (this Direction8 dir) => (Direction8)(((int)dir + 1) % 8);
	[MethodImpl(INLINE)]
	public static Direction8 AntiClockwise (this Direction8 dir) => (Direction8)(((int)dir + 7) % 8);
	[MethodImpl(INLINE)]
	public static Direction8 Opposite (this Direction8 dir) => (Direction8)(((int)dir + 4) % 8);
	[MethodImpl(INLINE)]
	public static int Rotation (this Direction8 dir) => (int)dir * 45;

	[MethodImpl(INLINE)] public static bool IsLeft (this Direction8 dir) => dir == Direction8.Left || dir == Direction8.BottomLeft || dir == Direction8.TopLeft;
	[MethodImpl(INLINE)] public static bool IsRight (this Direction8 dir) => dir == Direction8.Right || dir == Direction8.BottomRight || dir == Direction8.TopRight;
	[MethodImpl(INLINE)] public static bool IsVertical (this Direction8 dir) => dir == Direction8.Top || dir == Direction8.Bottom;
	[MethodImpl(INLINE)] public static bool IsHorizontal (this Direction8 dir) => dir == Direction8.Left || dir == Direction8.Right;
	[MethodImpl(INLINE)] public static bool IsBottom (this Direction8 dir) => dir == Direction8.Bottom || dir == Direction8.BottomLeft || dir == Direction8.BottomRight;
	[MethodImpl(INLINE)] public static bool IsTop (this Direction8 dir) => dir == Direction8.Top || dir == Direction8.TopLeft || dir == Direction8.TopRight;


	// Rect
	[MethodImpl(INLINE)]
	public static FRect Shift (this FRect rect, float x, float y) {
		rect.x += x;
		rect.y += y;
		return rect;
	}
	[MethodImpl(INLINE)] public static FRect Expand (this FRect rect, float offset) => rect.Expand(offset, offset, offset, offset);
	[MethodImpl(INLINE)]
	public static FRect Expand (this FRect rect, float l, float r, float d, float u) {
		rect.x -= l;
		rect.y -= d;
		rect.width += l + r;
		rect.height += d + u;
		return rect;
	}
	[MethodImpl(INLINE)] public static FRect Shrink (this FRect rect, float offset) => rect.Expand(-offset);
	[MethodImpl(INLINE)] public static FRect Shrink (this FRect rect, float l, float r, float d, float u) => rect.Expand(-l, -r, -d, -u);

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
	[MethodImpl(INLINE)]
	public static void Clamp (this ref FRect rect, FRect target) => rect = FRect.MinMaxRect(
		Util.Min(target.xMax, Util.Max(rect.xMin, target.xMin)),
		Util.Min(target.yMax, Util.Max(rect.yMin, target.yMin)),
		Util.Max(target.xMin, Util.Min(rect.xMax, target.xMax)),
		Util.Max(target.yMin, Util.Min(rect.yMax, target.yMax))
	);
	[MethodImpl(INLINE)]
	public static FRect GetClamp (this FRect rect, FRect target) => FRect.MinMaxRect(
		Util.Min(target.xMax, Util.Max(rect.xMin, target.xMin)),
		Util.Min(target.yMax, Util.Max(rect.yMin, target.yMin)),
		Util.Max(target.xMin, Util.Min(rect.xMax, target.xMax)),
		Util.Max(target.yMin, Util.Min(rect.yMax, target.yMax))
	);

	[MethodImpl(INLINE)] public static Float2 BottomLeft (this FRect rect) => new(rect.xMin, rect.yMin);
	[MethodImpl(INLINE)] public static Float2 BottomRight (this FRect rect) => new(rect.xMax, rect.yMin);
	[MethodImpl(INLINE)] public static Float2 TopLeft (this FRect rect) => new(rect.xMin, rect.yMax);
	[MethodImpl(INLINE)] public static Float2 TopRight (this FRect rect) => new(rect.xMax, rect.yMax);

	[MethodImpl(INLINE)]
	public static FRect Edge (this FRect rect, Direction4 edge, float thickness = 1f) => edge switch {
		Direction4.Up => rect.Shrink(0, 0, rect.height, -thickness),
		Direction4.Down => rect.Shrink(0, 0, -thickness, rect.height),
		Direction4.Left => rect.Shrink(-thickness, rect.width, 0, 0),
		Direction4.Right => rect.Shrink(rect.width, -thickness, 0, 0),
		_ => throw new System.NotImplementedException(),
	};

	[MethodImpl(INLINE)] public static FRect ScaleFrom (this FRect rect, float scale, float pointX, float pointY) => ResizeFrom(rect, rect.width * scale, rect.height * scale, pointX, pointY);
	[MethodImpl(INLINE)]
	public static FRect ResizeFrom (this FRect rect, float newWidth, float newHeight, float pointX, float pointY) {
		rect.x = pointX - (pointX - rect.x) * newWidth / rect.width;
		rect.y = pointY - (pointY - rect.y) * newHeight / rect.height;
		rect.width = newWidth;
		rect.height = newHeight;
		return rect;
	}
	[MethodImpl(INLINE)] public static IRect ToIRect (this FRect rect) => new((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
	[MethodImpl(INLINE)]
	public static IRect ExpandToIRect (this FRect rect) => IRect.MinMaxRect(
		rect.xMin.FloorToInt(),
		rect.yMin.FloorToInt(),
		rect.xMax.CeilToInt(),
		rect.yMax.CeilToInt()
	);


	// IRect
	[MethodImpl(INLINE)] public static FRect ToFRect (this IRect rect) => new(rect.x, rect.y, rect.width, rect.height);
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
	[MethodImpl(INLINE)] public static IRect Expand (this IRect rect, int offset) => rect.Expand(offset, offset, offset, offset);
	[MethodImpl(INLINE)]
	public static IRect Expand (this IRect rect, int l, int r, int d, int u) {
		rect.x -= l;
		rect.y -= d;
		rect.width += l + r;
		rect.height += d + u;
		return rect;
	}
	[MethodImpl(INLINE)]
	public static IRect Expand (this IRect rect, Int4 offset) {
		rect.x -= offset.left;
		rect.y -= offset.down;
		rect.width += offset.left + offset.right;
		rect.height += offset.down + offset.up;
		return rect;
	}
	[MethodImpl(INLINE)] public static IRect Shrink (this IRect rect, int offset) => rect.Expand(-offset);
	[MethodImpl(INLINE)] public static IRect Shrink (this IRect rect, int l, int r, int d, int u) => rect.Expand(-l, -r, -d, -u);
	[MethodImpl(INLINE)]
	public static IRect Shrink (this IRect rect, Int4 offset) {
		rect.x += offset.left;
		rect.y += offset.down;
		rect.width -= offset.left + offset.right;
		rect.height -= offset.down + offset.up;
		return rect;
	}
	[MethodImpl(INLINE)]
	public static IRect ShrinkLeft (this IRect rect, int left) {
		rect.x += left;
		rect.width -= left;
		return rect;
	}
	[MethodImpl(INLINE)]
	public static IRect ShrinkRight (this IRect rect, int right) {
		rect.width -= right;
		return rect;
	}
	[MethodImpl(INLINE)]
	public static IRect ShrinkDown (this IRect rect, int down) {
		rect.y += down;
		rect.height -= down;
		return rect;
	}
	[MethodImpl(INLINE)]
	public static IRect ShrinkUp (this IRect rect, int up) {
		rect.height -= up;
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
	[MethodImpl(INLINE)]
	public static IRect Shift (this IRect rect, int x, int y) {
		rect.x += x;
		rect.y += y;
		return rect;
	}
	[MethodImpl(INLINE)]
	public static void SetMinMax (this ref IRect rect, int xMin, int xMax, int yMin, int yMax) {
		rect.x = xMin;
		rect.y = yMin;
		rect.width = xMax - xMin;
		rect.height = yMax - yMin;
	}
	[MethodImpl(INLINE)] public static bool IsSame (this IRect a, IRect b) => a.x == b.x && a.y == b.y && a.width == b.width && a.height == b.height;
	[MethodImpl(INLINE)] public static bool IsNotSame (this IRect a, IRect b) => a.x != b.x || a.y != b.y || a.width != b.width || a.height != b.height;
	[MethodImpl(INLINE)] public static bool Contains (this IRect rect, int x, int y) => rect.Contains(new Int2(x, y));
	[MethodImpl(INLINE)] public static int CenterX (this IRect rect) => rect.x + rect.width / 2;
	[MethodImpl(INLINE)] public static int CenterY (this IRect rect) => rect.y + rect.height / 2;
	[MethodImpl(INLINE)] public static Int2 CenterInt (this IRect rect) => new(rect.x + rect.width / 2, rect.y + rect.height / 2);
	[MethodImpl(INLINE)]
	public static IRect UDivide (this IRect rect, int divide) {
		rect.SetMinMax(rect.min.UDivide(divide), rect.max.UDivide(divide));
		return rect;
	}
	[MethodImpl(INLINE)] public static Int2 BottomLeft (this IRect rect) => new(rect.xMin, rect.yMin);
	[MethodImpl(INLINE)] public static Int2 BottomRight (this IRect rect) => new(rect.xMax, rect.yMin);
	[MethodImpl(INLINE)] public static Int2 TopLeft (this IRect rect) => new(rect.xMin, rect.yMax);
	[MethodImpl(INLINE)] public static Int2 TopRight (this IRect rect) => new(rect.xMax, rect.yMax);
	[MethodImpl(INLINE)]
	public static void ClampPositionInside (ref this IRect rect, IRect bounds) {
		rect.x = rect.x.Clamp(bounds.x, bounds.xMax - rect.width);
		rect.y = rect.y.Clamp(bounds.y, bounds.yMax - rect.height);
	}
	[MethodImpl(INLINE)]
	public static IRect LerpTo (this IRect from, IRect to, int lerpRate) => new(
		from.x.LerpTo(to.x, lerpRate),
		from.y.LerpTo(to.y, lerpRate),
		from.width.LerpTo(to.width, lerpRate),
		from.height.LerpTo(to.height, lerpRate)
	);
	[MethodImpl(INLINE)]
	public static IRect LerpTo (this IRect from, IRect to, float lerp) => new(
		from.x.LerpTo(to.x, lerp),
		from.y.LerpTo(to.y, lerp),
		from.width.LerpTo(to.width, lerp),
		from.height.LerpTo(to.height, lerp)
	);
	[MethodImpl(INLINE)] public static IRect EdgeLeft (this IRect rect, int size) => rect.Shrink(0, rect.width - size, 0, 0);
	[MethodImpl(INLINE)] public static IRect EdgeRight (this IRect rect, int size) => rect.Shrink(rect.width - size, 0, 0, 0);
	[MethodImpl(INLINE)] public static IRect EdgeDown (this IRect rect, int size) => rect.Shrink(0, 0, 0, rect.height - size);
	[MethodImpl(INLINE)] public static IRect EdgeUp (this IRect rect, int size) => rect.Shrink(0, 0, rect.height - size, 0);
	[MethodImpl(INLINE)] public static IRect EdgeSquareLeft (this IRect rect) => rect.Shrink(0, rect.width - rect.height, 0, 0);
	[MethodImpl(INLINE)] public static IRect EdgeSquareRight (this IRect rect) => rect.Shrink(rect.width - rect.height, 0, 0, 0);
	[MethodImpl(INLINE)] public static IRect EdgeSquareDown (this IRect rect) => rect.Shrink(0, 0, 0, rect.height - rect.width);
	[MethodImpl(INLINE)] public static IRect EdgeSquareUp (this IRect rect) => rect.Shrink(0, 0, rect.height - rect.width, 0);
	[MethodImpl(INLINE)]
	public static IRect Edge (this IRect rect, Direction4 edge, int size = 1) => edge switch {
		Direction4.Up => rect.Shrink(0, 0, rect.height - size, 0),
		Direction4.Down => rect.Shrink(0, 0, 0, rect.height - size),
		Direction4.Left => rect.Shrink(0, rect.width - size, 0, 0),
		Direction4.Right => rect.Shrink(rect.width - size, 0, 0, 0),
		_ => throw new System.NotImplementedException(),
	};
	[MethodImpl(INLINE)]
	public static IRect EdgeExact (this IRect rect, Direction4 edge, int size = 1) => edge switch {
		Direction4.Up => rect.Shrink(0, 0, rect.height - size / 2, -size / 2),
		Direction4.Down => rect.Shrink(0, 0, -size / 2, rect.height - size / 2),
		Direction4.Left => rect.Shrink(-size / 2, rect.width - size / 2, 0, 0),
		Direction4.Right => rect.Shrink(rect.width - size / 2, -size / 2, 0, 0),
		_ => throw new System.NotImplementedException(),
	};
	[MethodImpl(INLINE)]
	public static IRect EdgeOutside (this IRect rect, Direction4 edge, int size = 1) => edge switch {
		Direction4.Up => rect.Shrink(0, 0, rect.height, -size),
		Direction4.Down => rect.Shrink(0, 0, -size, rect.height),
		Direction4.Left => rect.Shrink(-size, rect.width, 0, 0),
		Direction4.Right => rect.Shrink(rect.width, -size, 0, 0),
		_ => throw new System.NotImplementedException(),
	};
	[MethodImpl(INLINE)] public static IRect CornerInside (this IRect rect, Alignment corner, int size) => CornerInside(rect, corner, size, size);
	[MethodImpl(INLINE)]
	public static IRect CornerInside (this IRect rect, Alignment corner, int width, int height) => new(
		corner.IsLeft() ? rect.x : corner.IsRight() ? rect.xMax - width : rect.CenterX() - width / 2,
		corner.IsBottom() ? rect.y : corner.IsTop() ? rect.yMax - height : rect.CenterY() - height / 2,
		width, height
	);
	[MethodImpl(INLINE)] public static IRect CornerOutside (this IRect rect, Alignment corner, int size) => CornerOutside(rect, corner, size, size);
	[MethodImpl(INLINE)]
	public static IRect CornerOutside (this IRect rect, Alignment corner, int width, int height) => new(
		corner.IsLeft() ? rect.x - width : corner.IsRight() ? rect.xMax : rect.CenterX() - width / 2,
		corner.IsBottom() ? rect.y - height : corner.IsTop() ? rect.yMax : rect.CenterY() - height / 2,
		width, height
	);
	[MethodImpl(INLINE)] public static IRect ScaleFrom (this IRect rect, int scale, int pointX, int pointY) => ResizeFrom(rect, rect.width * scale / 1000, rect.height * scale / 1000, pointX, pointY);
	[MethodImpl(INLINE)] public static IRect ScaleFrom (this IRect rect, float scale01, int pointX, int pointY) => ResizeFrom(rect, (rect.width * scale01).RoundToInt(), (rect.height * scale01).RoundToInt(), pointX, pointY);
	[MethodImpl(INLINE)]
	public static IRect ResizeFrom (this IRect rect, int newWidth, int newHeight, int pointX, int pointY) {
		rect.x = pointX - (pointX - rect.x) * newWidth / rect.width;
		rect.y = pointY - (pointY - rect.y) * newHeight / rect.height;
		rect.width = newWidth;
		rect.height = newHeight;
		return rect;
	}
	[MethodImpl(INLINE)] public static bool CompleteInside (this IRect rect, IRect range) => rect.xMin >= range.xMin && rect.xMax <= range.xMax && rect.yMin >= range.yMin && rect.yMax <= range.yMax;
	[MethodImpl(INLINE)]
	public static IRect Clamp (this IRect rect, IRect range) => IRect.MinMaxRect(
		Util.Max(rect.xMin, range.xMin),
		Util.Max(rect.yMin, range.yMin),
		Util.Min(rect.xMax, range.xMax),
		Util.Min(rect.yMax, range.yMax)
	);
	[MethodImpl(INLINE)] public static void SlideLeft (ref this IRect rect, int padding = 0) => rect.x -= rect.width + padding;
	[MethodImpl(INLINE)] public static void SlideRight (ref this IRect rect, int padding = 0) => rect.x += rect.width + padding;
	[MethodImpl(INLINE)] public static void SlideDown (ref this IRect rect, int padding = 0) => rect.y -= rect.height + padding;
	[MethodImpl(INLINE)] public static void SlideUp (ref this IRect rect, int padding = 0) => rect.y += rect.height + padding;
	[MethodImpl(INLINE)]
	public static IRect TopHalf (this IRect rect) {
		int delta = rect.height / 2;
		rect.y = rect.yMax - delta;
		rect.height -= delta;
		return rect;
	}
	[MethodImpl(INLINE)]
	public static IRect BottomHalf (this IRect rect) {
		rect.height /= 2;
		return rect;
	}
	[MethodImpl(INLINE)]
	public static IRect RightHalf (this IRect rect) {
		int delta = rect.width / 2;
		rect.x = rect.xMax - delta;
		rect.width -= delta;
		return rect;
	}
	[MethodImpl(INLINE)]
	public static IRect LeftHalf (this IRect rect) {
		rect.width /= 2;
		return rect;
	}
	[MethodImpl(INLINE)]
	public static IRect MidHalf (this IRect rect) {
		rect.width /= 2;
		rect.x += rect.width / 2;
		return rect;
	}
	[MethodImpl(INLINE)]
	public static IRect Part (this IRect rect, int index, int count) {
		rect.width /= count;
		rect.x += index * rect.width;
		return rect;
	}
	[MethodImpl(INLINE)]
	public static IRect? Intersection (this IRect rect, IRect other) {
		if (!rect.Overlaps(other)) return null;
		return IRect.MinMaxRect(
			Util.Max(rect.xMin, other.xMin),
			Util.Max(rect.yMin, other.yMin),
			Util.Min(rect.xMax, other.xMax),
			Util.Min(rect.yMax, other.yMax)
		);
	}
	public static bool Dodge (this IRect rect, IRect dodge, out IRect? resultTop, out IRect? resultMidL, out IRect? resultMidR, out IRect? resultBottom) {
		resultTop = null;
		resultMidL = null;
		resultMidR = null;
		resultBottom = null;
		if (!rect.Overlaps(dodge) || rect.CompleteInside(dodge)) return false;

		// Top
		int topHeight = rect.yMax - dodge.yMax;
		if (topHeight > 0) {
			resultTop = rect.Edge(Direction4.Up, topHeight);
		}

		// Bottom
		int bottomHeight = dodge.y - rect.y;
		if (bottomHeight > 0) {
			resultBottom = rect.Edge(Direction4.Down, bottomHeight);
		}

		int midTop = Util.Min(rect.yMax, dodge.yMax);
		int midBottom = Util.Max(rect.y, dodge.y);

		// Mid L
		int midLWidth = dodge.x - rect.x;
		if (midLWidth > 0) {
			resultMidL = new IRect(rect.x, midBottom, midLWidth, midTop - midBottom);
		}

		// Mid R
		int midRWidth = rect.xMax - dodge.xMax;
		if (midRWidth > 0) {
			resultMidR = new IRect(dodge.xMax, midBottom, midRWidth, midTop - midBottom);
		}

		return true;
	}
	public static IRect ForceSquare (this IRect rect, bool toLeft, bool toBottom, bool grow = false) {
		if (rect.width == rect.height) return rect;
		if (grow) {
			rect.x -= rect.width < rect.height && toLeft ? Util.Abs(rect.width - rect.height) : 0;
			rect.y -= rect.width > rect.height && toBottom ? Util.Abs(rect.width - rect.height) : 0;
			rect.width = rect.height = Util.Max(rect.width, rect.height);
		} else {
			rect.x += rect.width > rect.height && toLeft ? Util.Abs(rect.width - rect.height) : 0;
			rect.y += rect.width < rect.height && toBottom ? Util.Abs(rect.width - rect.height) : 0;
			rect.width = rect.height = Util.Min(rect.width, rect.height);
		}
		return rect;
	}


	// Misc
	[MethodImpl(INLINE)] public static bool IsSame (this Color32 a, Color32 b, bool ignoreAlpha = false) => a.r == b.r && a.g == b.g && a.b == b.b && (ignoreAlpha || a.a == b.a);
	[MethodImpl(INLINE)]
	public static Color32 Mult (this Color32 a, Color32 b) {
		a.r = (byte)(a.r * b.r / 255);
		a.g = (byte)(a.g * b.g / 255);
		a.b = (byte)(a.b * b.b / 255);
		a.a = (byte)(a.a * b.a / 255);
		return a;
	}

	[MethodImpl(INLINE)] public static bool NotEnd (this BinaryReader reader) => reader.BaseStream.Position < reader.BaseStream.Length;

	[MethodImpl(INLINE)]
	public static Direction4? GetDirection (this Gamekey key) => key switch {
		Gamekey.Down => Direction4.Down,
		Gamekey.Up => Direction4.Up,
		Gamekey.Left => Direction4.Left,
		Gamekey.Right => Direction4.Right,
		_ => null,
	};

	[MethodImpl(INLINE)] public static bool IsLeft (this Alignment alignment) => alignment == Alignment.BottomLeft || alignment == Alignment.MidLeft || alignment == Alignment.TopLeft;
	[MethodImpl(INLINE)] public static bool IsMidX (this Alignment alignment) => alignment == Alignment.BottomMid || alignment == Alignment.MidMid || alignment == Alignment.TopMid;
	[MethodImpl(INLINE)] public static bool IsRight (this Alignment alignment) => alignment == Alignment.BottomRight || alignment == Alignment.MidRight || alignment == Alignment.TopRight;
	[MethodImpl(INLINE)] public static bool IsBottom (this Alignment alignment) => alignment == Alignment.BottomLeft || alignment == Alignment.BottomMid || alignment == Alignment.BottomRight;
	[MethodImpl(INLINE)] public static bool IsMidY (this Alignment alignment) => alignment == Alignment.MidLeft || alignment == Alignment.MidMid || alignment == Alignment.MidRight;
	[MethodImpl(INLINE)] public static bool IsTop (this Alignment alignment) => alignment == Alignment.TopLeft || alignment == Alignment.TopMid || alignment == Alignment.TopRight;
	[MethodImpl(INLINE)]
	public static Int2 Normal (this Alignment alignment) => alignment switch {
		Alignment.TopLeft => new(-1, 1),
		Alignment.TopMid => new(0, 1),
		Alignment.TopRight => new(1, 1),
		Alignment.MidLeft => new(-1, 0),
		Alignment.MidMid => new(0, 0),
		Alignment.MidRight => new(1, 0),
		Alignment.BottomLeft => new(-1, -1),
		Alignment.BottomMid => new(0, -1),
		Alignment.BottomRight => new(1, -1),
		Alignment.Full => new(0, 0),
		_ => new(0, 0),
	};

	[MethodImpl(INLINE)]
	public static A[] FillWithValue<A> (this A[] arr, A value) {
		for (int i = 0; i < arr.Length; i++) {
			arr[i] = value;
		}
		return arr;
	}
	[MethodImpl(INLINE)]
	public static A[] FillWithNewValue<A> (this A[] arr) where A : new() {
		for (int i = 0; i < arr.Length; i++) {
			arr[i] = new A();
		}
		return arr;
	}

	[MethodImpl(INLINE)]
	public static void AddRange<T, V> (this Dictionary<T, V> map, IEnumerable<KeyValuePair<T, V>> values) {
		foreach (var (k, v) in values) map.TryAdd(k, v);
	}


	// Color
	[MethodImpl(INLINE)] public static Color32 WithNewA (this Color32 value, int a) => new(value.r, value.g, value.b, (byte)(a.Clamp(0, 255)));
	[MethodImpl(INLINE)] public static ColorF WithNewA (this ColorF value, float a) => new(value.r, value.g, value.b, a);
	[MethodImpl(INLINE)]
	public static Color32 ToColor32 (this ColorF value) => new(
		(byte)(value.r * 255f),
		(byte)(value.g * 255f),
		(byte)(value.b * 255f),
		(byte)(value.a * 255f)
	);
	[MethodImpl(INLINE)]
	public static ColorF ToColorF (this Color32 value) => new(
		value.r / 255f, value.g / 255f, value.b / 255f, value.a / 255f
	);
	[MethodImpl(INLINE)] public static bool Almost (this ColorF a, ColorF b) => a == b;
	[MethodImpl(INLINE)] public static bool LookDifferent (this Color32 a, Color32 b) => (a.a > 0 || b.a > 0) && a != b;

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


	// Type
	public static string GetTypePath (this System.Type type, System.Type startFrom = null) {
		startFrom ??= typeof(object);
		TypePathBuilder.Clear();
		while (true) {
			TypePathBuilder.Insert(0, $"{type.AngeName()}/");
			type = type.BaseType;
			if (type == startFrom || type == null) break;
		}
		return TypePathBuilder.ToString();
	}


	// String
	[MethodImpl(INLINE)] public static string TrimWhiteForStartAndEnd (this string str) => str.TrimStart(' ', '\t').TrimEnd(' ', '\t');
	[MethodImpl(INLINE)] public static string TrimEnd_Numbers (this string str) => str.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
	[MethodImpl(INLINE)] public static string TrimEnd_NumbersEmpty (this string str) => str.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ' ');
	[MethodImpl(INLINE)] public static string TrimEnd_NumbersEmpty_ (this string str) => str.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ' ', '_');
	[MethodImpl(INLINE)]
	public static string TrimEnd_IfEndWith (this string str, string end, params char[] trimChars) {
		if (str.EndsWith(end)) {
			return str[..^end.Length].TrimEnd(trimChars);
		}
		return str;
	}

	public static string TrimStart_Numbers (this string str) => str.TrimStart('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');

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

	private static readonly StringBuilder JoinArrayBuilder = new();

	public static string JoinArray<T> (this IEnumerable<T> arr, System.Func<T, string> toString, char separator = '\0') {
		JoinArrayBuilder.Clear();
		using var enumerator = arr.GetEnumerator();
		var last = !enumerator.MoveNext();
		T current;
		while (!last) {
			current = enumerator.Current;
			JoinArrayBuilder.Append(toString(current));
			last = !enumerator.MoveNext();
			if (separator != '\0' && !last) {
				JoinArrayBuilder.Append(separator);
			}
		}
		return JoinArrayBuilder.ToString();
	}

	// Ref
	public static IEnumerable<T> ForAllStaticFieldValue<T> (this System.Type type, BindingFlags binding = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, bool inherited = true) {
		var tType = typeof(T);
		foreach (var field in type.GetFields(binding)) {
			if (field.FieldType == tType || (inherited && field.FieldType.IsSubclassOf(tType))) {
				if (type.ContainsGenericParameters) {
					var args = type.GetGenericArguments();
					var gTypes = new System.Type[args.Length];
					for (int i = 0; i < args.Length; i++) gTypes[i] = args[i].BaseType;
					var newType = type.MakeGenericType(gTypes);
					var newField = newType.GetField(field.Name, binding);
					yield return (T)newField.GetValue(null);
				} else {
					yield return (T)field.GetValue(null);
				}
			}
		}
	}

	public static IEnumerable<(FieldInfo field, T value)> ForAllFields<T> (this object target, BindingFlags binding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, bool inherited = true) {
		var type = target.GetType();
		var tType = typeof(T);
		foreach (var field in type.GetFields(binding)) {
			if (field.FieldType == tType || (inherited && field.FieldType.IsSubclassOf(tType))) {
				yield return (field, (T)field.GetValue(target));
			}
		}
	}

	public static IEnumerable<FieldInfo> ForAllFields<T> (this System.Type type, BindingFlags binding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, bool inherited = true) {
		var tType = typeof(T);
		foreach (var field in type.GetFields(binding)) {
			if (field.FieldType == tType || (inherited && field.FieldType.IsSubclassOf(tType))) {
				yield return field;
			}
		}
	}

	public static IEnumerable<(string name, T value)> ForAllProperties<T> (this object target) {
		var type = target.GetType();
		var tType = typeof(T);
		foreach (var pro in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
			if (pro.PropertyType == tType || pro.PropertyType.IsSubclassOf(tType)) {
				yield return (pro.Name, (T)pro.GetValue(target));
			}
		}
	}

}