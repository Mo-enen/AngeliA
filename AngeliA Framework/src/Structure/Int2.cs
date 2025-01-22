using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;


namespace AngeliA;

[Serializable]
public struct Int2 : IEquatable<Int2>, IFormattable {
	private int m_X;

	private int m_Y;

	private static readonly Int2 s_Zero = new(0, 0);

	private static readonly Int2 s_One = new(1, 1);

	private static readonly Int2 s_Up = new(0, 1);

	private static readonly Int2 s_Down = new(0, -1);

	private static readonly Int2 s_Left = new(-1, 0);

	private static readonly Int2 s_Right = new(1, 0);

	public readonly int Area => x * y;

	public int x {
		readonly get {
			return m_X;
		}

		set {
			m_X = value;
		}
	}

	public int y {
		readonly get {
			return m_Y;
		}

		set {
			m_Y = value;
		}
	}

	public int this[int index] {
		readonly get {
			return index switch {
				0 => x,
				1 => y,
				_ => throw new IndexOutOfRangeException($"Invalid Vector2Int index addressed: {index}!"),
			};
		}

		set {
			switch (index) {
				case 0:
					x = value;
					break;
				case 1:
					y = value;
					break;
				default:
					throw new IndexOutOfRangeException($"Invalid Vector2Int index addressed: {index}!");
			}
		}
	}

	public readonly float magnitude => (float)Math.Sqrt(x * x + y * y);

	public readonly int sqrMagnitude => x * x + y * y;

	public static Int2 zero {

		get {
			return s_Zero;
		}
	}

	public static Int2 one {

		get {
			return s_One;
		}
	}

	public static Int2 up {

		get {
			return s_Up;
		}
	}

	public static Int2 down {

		get {
			return s_Down;
		}
	}

	public static Int2 left {

		get {
			return s_Left;
		}
	}

	public static Int2 right {

		get {
			return s_Right;
		}
	}

	public Int2 (int x, int y) {
		m_X = x;
		m_Y = y;
	}

	public readonly void Deconstruct (out int x, out int y) {
		x = this.x;
		y = this.y;
	}

	public void Set (int x, int y) {
		m_X = x;
		m_Y = y;
	}

	public static float Distance (Int2 a, Int2 b) {
		float num = a.x - b.x;
		float num2 = a.y - b.y;
		return (float)Math.Sqrt(num * num + num2 * num2);
	}

	public static Int2 Min (Int2 lhs, Int2 rhs) {
		return new Int2(Math.Min(lhs.x, rhs.x), Math.Min(lhs.y, rhs.y));
	}

	public static Int2 Max (Int2 lhs, Int2 rhs) {
		return new Int2(Math.Max(lhs.x, rhs.x), Math.Max(lhs.y, rhs.y));
	}

	public static Int2 Scale (Int2 a, Int2 b) {
		return new Int2(a.x * b.x, a.y * b.y);
	}

	public void Scale (Int2 scale) {
		x *= scale.x;
		y *= scale.y;
	}

	public void Clamp (Int2 min, Int2 max) {
		x = Math.Max(min.x, x);
		x = Math.Min(max.x, x);
		y = Math.Max(min.y, y);
		y = Math.Min(max.y, y);
	}

	public void Clamp (IRect range) {
		x = Math.Max(range.xMin, x);
		x = Math.Min(range.xMax, x);
		y = Math.Max(range.yMin, y);
		y = Math.Min(range.yMax, y);
	}

	public readonly Int2 Shift (int x, int y) => new(m_X + x, m_Y + y);

	public static implicit operator Float2 (Int2 v) {
		return new Float2(v.x, v.y);
	}

	public static explicit operator Int3 (Int2 v) {
		return new Int3(v.x, v.y, 0);
	}

	public static Int2 FloorToInt (Float2 v) => new(v.x.FloorToInt(), v.y.FloorToInt());

	public static Int2 CeilToInt (Float2 v) => new(v.x.CeilToInt(), v.y.CeilToInt());

	public static Int2 RoundToInt (Float2 v) => new(v.x.RoundToInt(), v.y.RoundToInt());

	public static Int2 operator - (Int2 v) {
		return new Int2(-v.x, -v.y);
	}

	public static Int2 operator + (Int2 a, Int2 b) {
		return new Int2(a.x + b.x, a.y + b.y);
	}

	public static Int2 operator - (Int2 a, Int2 b) {
		return new Int2(a.x - b.x, a.y - b.y);
	}

	public static Int2 operator * (Int2 a, Int2 b) {
		return new Int2(a.x * b.x, a.y * b.y);
	}

	public static Int2 operator * (int a, Int2 b) {
		return new Int2(a * b.x, a * b.y);
	}

	public static Int2 operator * (Int2 a, int b) {
		return new Int2(a.x * b, a.y * b);
	}

	public static Int2 operator / (Int2 a, int b) {
		return new Int2(a.x / b, a.y / b);
	}

	public static bool operator == (Int2 lhs, Int2 rhs) {
		return lhs.x == rhs.x && lhs.y == rhs.y;
	}

	public static bool operator != (Int2 lhs, Int2 rhs) {
		return !(lhs == rhs);
	}

	public override readonly bool Equals (object other) {
		if (other is not Int2) {
			return false;
		}

		return Equals((Int2)other);
	}

	public readonly bool Equals (Int2 other) {
		return x == other.x && y == other.y;
	}

	public override readonly int GetHashCode () {
		return x.GetHashCode() ^ (y.GetHashCode() << 2);
	}

	public override readonly string ToString () {
		return ToString(null, null);
	}

	public readonly string ToString (string format) {
		return ToString(format, null);
	}

	public readonly string ToString (string format, IFormatProvider formatProvider) {
		formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;
		return string.Format("({0}, {1})", x.ToString(format, formatProvider), y.ToString(format, formatProvider));
	}


}