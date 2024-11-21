using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;


namespace AngeliA;
[Serializable]
public struct Int4 : IEquatable<Int4>, IFormattable {

	public static readonly Int4 zero = new(0, 0, 0, 0);
	public static readonly Int4 one = new(1, 1, 1, 1);
	public static readonly Int4 two = new(2, 2, 2, 2);
	public static readonly Int4 three = new(3, 3, 3, 3);
	public int this[int index] {
		readonly get => index switch {
			0 => x,
			1 => y,
			2 => z,
			3 => w,
			_ => throw new ArgumentOutOfRangeException(),
		};
		set {
			switch (index) {
				case 0: x = value; break;
				case 1: y = value; break;
				case 2: z = value; break;
				case 3: w = value; break;
				default: throw new ArgumentOutOfRangeException();
			}
		}
	}
	public readonly bool IsZero => x == 0 && y == 0 && z == 0 && w == 0;
	public int left { readonly get => x; set => x = value; }
	public int right { readonly get => y; set => y = value; }
	public int down { readonly get => z; set => z = value; }
	public int up { readonly get => w; set => w = value; }
	public readonly int horizontal => left + right;
	public readonly int vertical => down + up;

	public int x;
	public int y;
	public int z;
	public int w;

	public Int4 (int x, int y, int z, int w) {
		this.x = x;
		this.y = y;
		this.z = z;
		this.w = w;
	}
	public static Int4 Direction (int left, int right, int down, int up) => new(left, right, down, up);

	public static Int4 operator * (int b, Int4 a) => a * b;
	public static bool operator == (Int4 a, Int4 b) => a.Equals(b);
	public static bool operator != (Int4 a, Int4 b) => !a.Equals(b);
	public static Int4 operator * (Int4 a, int b) {
		a.x *= b;
		a.y *= b;
		a.z *= b;
		a.w *= b;
		return a;
	}
	public static Int4 operator + (Int4 a, Int4 b) {
		a.x += b.x;
		a.y += b.y;
		a.z += b.z;
		a.w += b.w;
		return a;
	}

	public override readonly bool Equals (object other) {
		if (other is not Int4) return false;
		return Equals((Int4)other);
	}
	public readonly bool Equals (Int4 other) => x == other.x && y == other.y && z == other.z && w == other.w;
	public override readonly int GetHashCode () => x ^ (y << 2) ^ (z >> 2) ^ (w >> 1);

	public readonly bool Contains (int value) => x == value || y == value || z == value || w == value;
	public readonly int Count (int value) {
		int count = 0;
		if (x == value) count++;
		if (y == value) count++;
		if (z == value) count++;
		if (w == value) count++;
		return count;
	}

	public override readonly string ToString () {
		return ToString(null, null);
	}
	public readonly string ToString (string format) {
		return ToString(format, null);
	}
	public readonly string ToString (string format, IFormatProvider formatProvider) {
		formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;
		return string.Format(
			"({0}, {1}, {2}, {3})",
			x.ToString(format, formatProvider),
			y.ToString(format, formatProvider),
			z.ToString(format, formatProvider),
			w.ToString(format, formatProvider)
		);
	}

}