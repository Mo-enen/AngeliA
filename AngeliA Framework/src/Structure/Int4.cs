using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;

namespace AngeliA;

/// <summary>
/// 4D vector with intager data
/// </summary>
[Serializable]
public struct Int4 (int x, int y, int z, int w) : IEquatable<Int4> {

	public static readonly Int4 Zero = new(0, 0, 0, 0);
	public static readonly Int4 One = new(1, 1, 1, 1);
	public static readonly Int4 Two = new(2, 2, 2, 2);
	public static readonly Int4 Three = new(3, 3, 3, 3);

	/// <summary>
	/// Get int data inside with given index. (0 means x, 1 means y, 2 means z, 3 means w)
	/// </summary>
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
				default: break;
			}
		}
	}

	/// <summary>
	/// True if all values of this vector are 0
	/// </summary>
	public readonly bool IsZero => x == 0 && y == 0 && z == 0 && w == 0;

	/// <summary>
	/// Treat this vector as padding/border value with 4 directions
	/// </summary>
	public int left { readonly get => x; set => x = value; }

	/// <summary>
	/// Treat this vector as padding/border value with 4 directions
	/// </summary>
	public int right { readonly get => y; set => y = value; }

	/// <summary>
	/// Treat this vector as padding/border value with 4 directions
	/// </summary>
	public int down { readonly get => z; set => z = value; }

	/// <summary>
	/// Treat this vector as padding/border value with 4 directions
	/// </summary>
	public int up { readonly get => w; set => w = value; }

	/// <summary>
	/// left + right
	/// </summary>
	public readonly int horizontal => left + right;

	/// <summary>
	/// down + up
	/// </summary>
	public readonly int vertical => down + up;

	public int x = x;
	public int y = y;
	public int z = z;
	public int w = w;

	/// <summary>
	/// Get a Int4 as a padding/border with 4 directions
	/// </summary>
	/// <param name="left"></param>
	/// <param name="right"></param>
	/// <param name="down"></param>
	/// <param name="up"></param>
	/// <returns></returns>
	public static Int4 Direction (int left, int right, int down, int up) => new(left, right, down, up);

	/// <summary>
	/// True if any value inside this vector is given value
	/// </summary>
	public readonly bool Contains (int value) => x == value || y == value || z == value || w == value;

	/// <summary>
	/// How many value inside this vector is equal to given value
	/// </summary>
	public readonly int Count (int value) {
		int count = 0;
		if (x == value) count++;
		if (y == value) count++;
		if (z == value) count++;
		if (w == value) count++;
		return count;
	}

	/// <summary>
	/// Index of the given value from this vector
	/// </summary>
	public readonly int FindIndex (int value) {
		if (x == value) return 0;
		if (y == value) return 1;
		if (z == value) return 2;
		if (w == value) return 3;
		return -1;
	}

	public override readonly bool Equals (object other) {
		if (other is not Int4) return false;
		return Equals((Int4)other);
	}
	public readonly bool Equals (Int4 other) => x == other.x && y == other.y && z == other.z && w == other.w;
	public override readonly int GetHashCode () => x ^ (y << 2) ^ (z >> 2) ^ (w >> 1);

	public override readonly string ToString () => $"({x}, {y}, {z}, {w})";

	// OPR
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
	public static Int4 operator / (Int4 a, int b) {
		a.x /= b;
		a.y /= b;
		a.z /= b;
		a.w /= b;
		return a;
	}
	public static Int4 operator + (Int4 a, Int4 b) {
		a.x += b.x;
		a.y += b.y;
		a.z += b.z;
		a.w += b.w;
		return a;
	}

	public static explicit operator Int2 (Int4 i) => new(i.x, i.y);

	public static explicit operator Int3 (Int4 i) => new(i.x, i.y, i.z);

}