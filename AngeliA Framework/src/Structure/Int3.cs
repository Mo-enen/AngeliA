using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;

namespace AngeliA;

/// <summary>
/// 3D vector with intager data
/// </summary>
[Serializable]
public struct Int3 : IEquatable<Int3> {

	public static readonly Int3 Zero = new(0, 0, 0);
	public static readonly Int3 One = new(1, 1, 1);
	public static readonly Int3 Up = new(0, 1, 0);
	public static readonly Int3 Down = new(0, -1, 0);
	public static readonly Int3 Left = new(-1, 0, 0);
	public static readonly Int3 Right = new(1, 0, 0);
	public static readonly Int3 Forward = new(0, 0, 1);
	public static readonly Int3 Back = new(0, 0, -1);

	public int x;
	public int y;
	public int z;

	/// <summary>
	/// Get int data inside with given index. (0 means x, 1 means y, 2 means z)
	/// </summary>
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

	/// <summary>
	/// Length of this vector
	/// </summary>
	public readonly float Magnitude => (float)Math.Sqrt(x * x + y * y + z * z);

	/// <summary>
	/// Square length of this vector
	/// </summary>
	public readonly int SqrMagnitude => x * x + y * y + z * z;

	public Int3 (int x, int y) {
		this.x = x;
		this.y = y;
		this.z = 0;
	}

	public Int3 (int x, int y, int z) {
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public void Set (int x, int y, int z) {
		this.x = x;
		this.y = y;
		this.z = z;
	}

	/// <summary>
	/// Distance between two given position
	/// </summary>
	public static float Distance (Int3 a, Int3 b) {
		return (a - b).Magnitude;
	}

	/// <summary>
	/// Move position by given amount
	/// </summary>
	public readonly Int3 Shift (int x, int y, int z = 0) => new(this.x + x, this.y + y, this.z + z);

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

	public override readonly string ToString () => $"({x}, {y}, {z})";


	// OPR
	public static implicit operator Float3 (Int3 v) => new(v.x, v.y, v.z);

	public static explicit operator Int2 (Int3 v) => new(v.x, v.y);

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

}