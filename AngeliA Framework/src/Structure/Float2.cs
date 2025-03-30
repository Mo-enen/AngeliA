using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;

namespace AngeliA;

/// <summary>
/// 2D vector with float data values
/// </summary>
[Serializable]
public struct Float2 : IEquatable<Float2> {

	public static readonly Float2 Zero = new(0f, 0f);
	public static readonly Float2 One = new(1f, 1f);
	public static readonly Float2 Up = new(0f, 1f);
	public static readonly Float2 Down = new(0f, -1f);
	public static readonly Float2 Left = new(-1f, 0f);
	public static readonly Float2 Right = new(1f, 0f);
	public float x;
	public float y;

	/// <summary>
	/// Get float data inside with given index. (0 means x, 1 means y)
	/// </summary>
	public float this[int index] {
		readonly get {
			return index switch {
				0 => x,
				1 => y,
				_ => throw new IndexOutOfRangeException("Invalid index"),
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
					throw new IndexOutOfRangeException("Invalid index");
			}
		}
	}

	/// <summary>
	/// Get a vector that has a length of 1, while maintaining the same direction as the original vector.
	/// </summary>
	public readonly Float2 Normalized {
		get {
			var result = new Float2(x, y);
			result.Normalize();
			return result;
		}
	}

	/// <summary>
	/// Length of this vector
	/// </summary>
	public readonly float Magnitude => (float)Math.Sqrt(x * x + y * y);

	/// <summary>
	/// Square of the length of this vector
	/// </summary>
	public readonly float SqrMagnitude => x * x + y * y;

	/// <summary>
	/// 2D vector with float data values
	/// </summary>
	public Float2 (float x, float y) {
		this.x = x;
		this.y = y;
	}

	/// <summary>
	/// Set both values of this vector
	/// </summary>
	/// <param name="newX"></param>
	/// <param name="newY"></param>
	public void Set (float newX, float newY) {
		x = newX;
		y = newY;
	}

	/// <summary>
	/// Find a value transform between two given float smoothly
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <param name="t">Representation of the position. 0 means value A, 1 means value B.</param>
	public static Float2 Lerp (Float2 a, Float2 b, float t) {
		t = t.Clamp01();
		return new Float2(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t);
	}

	/// <summary>
	/// Find a value transform between two given float smoothly without limit the t value
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <param name="t">Representation of the position. 0 means value A, 1 means value B.</param>
	public static Float2 LerpUnclamped (Float2 a, Float2 b, float t) {
		return new Float2(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t);
	}

	/// <summary>
	/// Moves a point towards the target with distance limitation
	/// </summary>
	/// <param name="current">Starting position</param>
	/// <param name="target">Target end position</param>
	/// <param name="maxDistanceDelta">Distance limit</param>
	/// <returns>The new position</returns>
	public static Float2 MoveTowards (Float2 current, Float2 target, float maxDistanceDelta) {
		float num = target.x - current.x;
		float num2 = target.y - current.y;
		float num3 = num * num + num2 * num2;
		if (num3 == 0f || (maxDistanceDelta >= 0f && num3 <= maxDistanceDelta * maxDistanceDelta)) {
			return target;
		}

		float num4 = (float)Math.Sqrt(num3);
		return new Float2(current.x + num / num4 * maxDistanceDelta, current.y + num2 / num4 * maxDistanceDelta);
	}

	/// <summary>
	/// Make the vector have length of 1 while maintaining the same direction as original.
	/// </summary>
	public void Normalize () {
		float num = Magnitude;
		if (num > 1E-05f) {
			this /= num;
		} else {
			this = Zero;
		}
	}

	public readonly void Deconstruct (out float x, out float y) {
		x = this.x;
		y = this.y;
	}

	public readonly override string ToString () => $"({x}, {y})";

	public readonly override int GetHashCode () {
		return x.GetHashCode() ^ (y.GetHashCode() << 2);
	}

	public override readonly bool Equals (object other) {
		if (other is not Float2) {
			return false;
		}

		return Equals((Float2)other);
	}

	public readonly bool Equals (Float2 other) {
		return x == other.x && y == other.y;
	}

	/// <summary>
	/// The dot product of two vectors returns a float value representing the cosine of the angle between them
	/// </summary>
	public static float Dot (Float2 a, Float2 b) => a.x * b.x + a.y * b.y;

	/// <summary>
	/// Signed angle in degrees between from and to
	/// </summary>
	public static float SignedAngle (Float2 from, Float2 to) => (float)Math.Atan2(from.x * to.y - to.x * from.y, from.x * to.x + from.y * to.y) * -Util.Rad2Deg;

	/// <summary>
	/// Distance between two given position
	/// </summary>
	public static float Distance (Float2 a, Float2 b) {
		float x = a.x - b.x;
		float y = a.y - b.y;
		return (float)Math.Sqrt(x * x + y * y);
	}

	// OPR
	public static Float2 operator + (Float2 a, Float2 b) {
		return new Float2(a.x + b.x, a.y + b.y);
	}

	public static Float2 operator - (Float2 a, Float2 b) {
		return new Float2(a.x - b.x, a.y - b.y);
	}

	public static Float2 operator * (Float2 a, Float2 b) {
		return new Float2(a.x * b.x, a.y * b.y);
	}

	public static Float2 operator / (Float2 a, Float2 b) {
		return new Float2(a.x / b.x, a.y / b.y);
	}

	public static Float2 operator - (Float2 a) {
		return new Float2(0f - a.x, 0f - a.y);
	}

	public static Float2 operator * (Float2 a, float d) {
		return new Float2(a.x * d, a.y * d);
	}

	public static Float2 operator * (float d, Float2 a) {
		return new Float2(a.x * d, a.y * d);
	}

	public static Float2 operator / (Float2 a, float d) {
		return new Float2(a.x / d, a.y / d);
	}

	public static bool operator == (Float2 lhs, Float2 rhs) {
		float num = lhs.x - rhs.x;
		float num2 = lhs.y - rhs.y;
		return num * num + num2 * num2 < 9.99999944E-11f;
	}

	public static bool operator != (Float2 lhs, Float2 rhs) {
		return !(lhs == rhs);
	}


	public static explicit operator Float2 (Float3 v) => new(v.x, v.y);


	public static implicit operator Float3 (Float2 v) => new(v.x, v.y, 0f);


}
