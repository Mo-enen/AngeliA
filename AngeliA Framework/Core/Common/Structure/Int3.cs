using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;


namespace AngeliA; 
[Serializable]
public struct Int3 : IEquatable<Int3>, IFormattable {

	private int m_X;
	private int m_Y;
	private int m_Z;

	private static readonly Int3 s_Zero = new(0, 0, 0);
	private static readonly Int3 s_One = new(1, 1, 1);
	private static readonly Int3 s_Up = new(0, 1, 0);
	private static readonly Int3 s_Down = new(0, -1, 0);
	private static readonly Int3 s_Left = new(-1, 0, 0);
	private static readonly Int3 s_Right = new(1, 0, 0);
	private static readonly Int3 s_Forward = new(0, 0, 1);
	private static readonly Int3 s_Back = new(0, 0, -1);

	public int x {
		readonly get => m_X;
		set => m_X = value;
	}
	public int y {
		readonly get => m_Y;

		set => m_Y = value;
	}
	public int z {
		readonly get => m_Z;

		set => m_Z = value;
	}

	public int this[int index] {
		readonly get => index switch {
			0 => x,
			1 => y,
			2 => z,
			_ => throw new IndexOutOfRangeException(string.Format("Invalid Vector3Int index addressed: {0}!", index)),
		};
		set {
			switch (index) {
				case 0:
					x = value;
					break;
				case 1:
					y = value;
					break;
				case 2:
					z = value;
					break;
				default:
					throw new IndexOutOfRangeException(string.Format("Invalid Vector3Int index addressed: {0}!", index));
			}
		}
	}

	public readonly float magnitude => (float)Math.Sqrt(x * x + y * y + z * z);

	public readonly int sqrMagnitude {

		get {
			return x * x + y * y + z * z;
		}
	}

	public static Int3 zero {

		get {
			return s_Zero;
		}
	}

	public static Int3 one {

		get {
			return s_One;
		}
	}

	public static Int3 up {

		get {
			return s_Up;
		}
	}

	public static Int3 down {

		get {
			return s_Down;
		}
	}

	public static Int3 left {

		get {
			return s_Left;
		}
	}

	public static Int3 right {

		get {
			return s_Right;
		}
	}

	public static Int3 forward {

		get {
			return s_Forward;
		}
	}

	public static Int3 back {

		get {
			return s_Back;
		}
	}

	public Int3 (int x, int y) {
		m_X = x;
		m_Y = y;
		m_Z = 0;
	}

	public Int3 (int x, int y, int z) {
		m_X = x;
		m_Y = y;
		m_Z = z;
	}

	public void Set (int x, int y, int z) {
		m_X = x;
		m_Y = y;
		m_Z = z;
	}

	public static float Distance (Int3 a, Int3 b) {
		return (a - b).magnitude;
	}

	public static Int3 Min (Int3 lhs, Int3 rhs) {
		return new Int3(Math.Min(lhs.x, rhs.x), Math.Min(lhs.y, rhs.y), Math.Min(lhs.z, rhs.z));
	}

	public static Int3 Max (Int3 lhs, Int3 rhs) {
		return new Int3(Math.Max(lhs.x, rhs.x), Math.Max(lhs.y, rhs.y), Math.Max(lhs.z, rhs.z));
	}

	public static Int3 Scale (Int3 a, Int3 b) {
		return new Int3(a.x * b.x, a.y * b.y, a.z * b.z);
	}

	public void Scale (Int3 scale) {
		x *= scale.x;
		y *= scale.y;
		z *= scale.z;
	}

	public void Clamp (Int3 min, Int3 max) {
		x = Math.Max(min.x, x);
		x = Math.Min(max.x, x);
		y = Math.Max(min.y, y);
		y = Math.Min(max.y, y);
		z = Math.Max(min.z, z);
		z = Math.Min(max.z, z);
	}

	public static implicit operator Float3 (Int3 v) {
		return new Float3(v.x, v.y, v.z);
	}

	public static explicit operator Int2 (Int3 v) {
		return new Int2(v.x, v.y);
	}

	public static Int3 FloorToInt (Float3 v) {
		return new Int3(v.x.FloorToInt(), v.y.FloorToInt(), v.z.FloorToInt());
	}

	public static Int3 CeilToInt (Float3 v) {
		return new Int3(v.x.CeilToInt(), v.y.CeilToInt(), v.z.CeilToInt());
	}

	public static Int3 RoundToInt (Float3 v) {
		return new Int3(v.x.RoundToInt(), v.y.RoundToInt(), v.z.RoundToInt());
	}

	public static Int3 operator + (Int3 a, Int3 b) {
		return new Int3(a.x + b.x, a.y + b.y, a.z + b.z);
	}

	public static Int3 operator - (Int3 a, Int3 b) {
		return new Int3(a.x - b.x, a.y - b.y, a.z - b.z);
	}

	public static Int3 operator * (Int3 a, Int3 b) {
		return new Int3(a.x * b.x, a.y * b.y, a.z * b.z);
	}

	public static Int3 operator - (Int3 a) {
		return new Int3(-a.x, -a.y, -a.z);
	}

	public static Int3 operator * (Int3 a, int b) {
		return new Int3(a.x * b, a.y * b, a.z * b);
	}

	public static Int3 operator * (int a, Int3 b) {
		return new Int3(a * b.x, a * b.y, a * b.z);
	}

	public static Int3 operator / (Int3 a, int b) {
		return new Int3(a.x / b, a.y / b, a.z / b);
	}

	public static bool operator == (Int3 lhs, Int3 rhs) {
		return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
	}

	public static bool operator != (Int3 lhs, Int3 rhs) {
		return !(lhs == rhs);
	}

	public override readonly bool Equals (object other) {
		if (other is not Int3) {
			return false;
		}

		return Equals((Int3)other);
	}

	public readonly bool Equals (Int3 other) {
		return this == other;
	}

	public override readonly int GetHashCode () {
		int hashCode = y.GetHashCode();
		int hashCode2 = z.GetHashCode();
		return x.GetHashCode() ^ (hashCode << 4) ^ (hashCode >> 28) ^ (hashCode2 >> 4) ^ (hashCode2 << 28);
	}

	public override readonly string ToString () => ToString(null, null);

	public readonly string ToString (string format) {
		return ToString(format, null);
	}

	public readonly string ToString (string format, IFormatProvider formatProvider) {
		formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;
		return string.Format("({0}, {1}, {2})", x.ToString(format, formatProvider), y.ToString(format, formatProvider), z.ToString(format, formatProvider));
	}


}