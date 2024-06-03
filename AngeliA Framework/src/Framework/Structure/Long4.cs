using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;

namespace AngeliA;

[Serializable]
public struct Long4 : IEquatable<Long4>, IFormattable {

	public static readonly Long4 zero = new(0, 0, 0, 0);
	public static readonly Long4 one = new(1, 1, 1, 1);
	public static readonly Long4 two = new(2, 2, 2, 2);
	public static readonly Long4 three = new(3, 3, 3, 3);
	public long this[int index] {
		readonly get => index switch {
			0 => x,
			1 => y,
			2 => z,
			3 => w,
			_ => throw new System.ArgumentOutOfRangeException(),
		};
		set {
			switch (index) {
				case 0: x = value; break;
				case 1: y = value; break;
				case 2: z = value; break;
				case 3: w = value; break;
				default: throw new System.ArgumentOutOfRangeException();
			}
		}
	}
	public readonly bool IsZero => x == 0 && y == 0 && z == 0 && w == 0;
	public long left { readonly get => x; set => x = value; }
	public long right { readonly get => y; set => y = value; }
	public long down { readonly get => z; set => z = value; }
	public long up { readonly get => w; set => w = value; }
	public readonly long horizontal => left + right;
	public readonly long vertical => down + up;

	public long x;
	public long y;
	public long z;
	public long w;

	public Long4 (long x, long y, long z, long w) {
		this.x = x;
		this.y = y;
		this.z = z;
		this.w = w;
	}
	public static Long4 Direction (long left, long right, long down, long up) => new(left, right, down, up);

	public static Long4 operator * (long b, Long4 a) => a * b;
	public static bool operator == (Long4 a, Long4 b) => a.Equals(b);
	public static bool operator != (Long4 a, Long4 b) => !a.Equals(b);
	public static Long4 operator * (Long4 a, long b) {
		a.x *= b;
		a.y *= b;
		a.z *= b;
		a.w *= b;
		return a;
	}
	public static Long4 operator + (Long4 a, Long4 b) {
		a.x += b.x;
		a.y += b.y;
		a.z += b.z;
		a.w += b.w;
		return a;
	}

	public override readonly bool Equals (object other) {
		if (other is not Long4) return false;
		return Equals((Long4)other);
	}
	public readonly bool Equals (Long4 other) => x == other.x && y == other.y && z == other.z && w == other.w;
	public override readonly int GetHashCode () => (int)(x ^ (y << 2) ^ (z >> 2) ^ (w >> 1));

	public readonly bool Contains (long value) => x == value || y == value || z == value || w == value;
	public bool Swap (long value, long newValue) {
		if (x == value) {
			x = newValue;
			return true;
		}
		if (y == value) {
			y = newValue;
			return true;
		}
		if (z == value) {
			z = newValue;
			return true;
		}
		if (w == value) {
			w = newValue;
			return true;
		}
		return false;
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