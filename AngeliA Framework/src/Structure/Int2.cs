using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;


namespace AngeliA;

/// <summary>
/// 2D vector with intager data
/// </summary>
[Serializable]
public struct Int2 : IEquatable<Int2> {

	// VAR
	public int x;
	public int y;

	public static readonly Int2 Zero = new(0, 0);
	public static readonly Int2 One = new(1, 1);
	public static readonly Int2 Up = new(0, 1);
	public static readonly Int2 Down = new(0, -1);
	public static readonly Int2 Left = new(-1, 0);
	public static readonly Int2 Right = new(1, 0);

	/// <summary>
	/// x * y
	/// </summary>
	public readonly int Area => x * y;

	/// <summary>
	/// Get int data inside with given index. (0 means x, 1 means y)
	/// </summary>
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

	/// <summary>
	/// Length of the vector
	/// </summary>
	public readonly float Magnitude => (float)Math.Sqrt(x * x + y * y);

	/// <summary>
	/// Square length of the vector
	/// </summary>
	public readonly int SqrMagnitude => x * x + y * y;

	// API
	public Int2 (int x, int y) {
		this.x = x;
		this.y = y;
	}

	public readonly void Deconstruct (out int x, out int y) {
		x = this.x;
		y = this.y;
	}

	public void Set (int x, int y) {
		this.x = x;
		this.y = y;
	}

	/// <summary>
	/// Distance between two given position
	/// </summary>
	public static float Distance (Int2 a, Int2 b) {
		float num = a.x - b.x;
		float num2 = a.y - b.y;
		return (float)Math.Sqrt(num * num + num2 * num2);
	}

	/// <summary>
	/// Move the position with given amount
	/// </summary>
	public readonly Int2 Shift (int x, int y) => new(this.x + x, this.y + y);

	public override readonly string ToString () => $"({x}, {y})";

	public override readonly bool Equals (object other) {
		if (other is not Int2) {
			return false;
		}
		return Equals((Int2)other);
	}

	public readonly bool Equals (Int2 other) => x == other.x && y == other.y;

	public override readonly int GetHashCode () => x.GetHashCode() ^ (y.GetHashCode() << 2);

	// OPR
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

	public static implicit operator Float2 (Int2 v) => new(v.x, v.y);

	public static explicit operator Int3 (Int2 v) => new(v.x, v.y, 0);

}