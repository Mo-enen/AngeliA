using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;


namespace AngeliA;

/// <summary>
/// 3D vector with float data values
/// </summary>
[Serializable]
public struct Float3 : IEquatable<Float3> {

	// VAR
	public static readonly Float3 Zero = new(0f, 0f, 0f);
	public static readonly Float3 One = new(1f, 1f, 1f);
	public static readonly Float3 Up = new(0f, 1f, 0f);
	public static readonly Float3 Down = new(0f, -1f, 0f);
	public static readonly Float3 Left = new(-1f, 0f, 0f);
	public static readonly Float3 Right = new(1f, 0f, 0f);
	public static readonly Float3 Forward = new(0f, 0f, 1f);
	public static readonly Float3 Back = new(0f, 0f, -1f);

	private static volatile float FloatMinNormal = 1.17549435E-38f;
	private static volatile float FloatMinDenormal = float.Epsilon;
	private static bool IsFlushToZeroEnabled = FloatMinDenormal == 0f;
	private static readonly float Epsilon = IsFlushToZeroEnabled ? FloatMinNormal : FloatMinDenormal;

	public float x;
	public float y;
	public float z;

	// API
	/// <summary>
	/// Get float data inside with given index. (0 means x, 1 means y, 2 means z)
	/// </summary>
	public float this[int index] {
		readonly get {
			return index switch {
				0 => x,
				1 => y,
				2 => z,
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
				case 2:
					z = value;
					break;
				default:
					throw new IndexOutOfRangeException("Invalid index");
			}
		}
	}

	/// <summary>
	/// Get a vector that has a length of 1, while maintaining the same direction as the original vector.
	/// </summary>
	public readonly Float3 Normalized => Normalize(this);

	/// <summary>
	/// Length of this vector
	/// </summary>
	public readonly float Magnitude => (float)Math.Sqrt(x * x + y * y + z * z);

	/// <summary>
	/// Square of the length of this vector
	/// </summary>
	public readonly float SqrMagnitude => x * x + y * y + z * z;

	/// <summary>
	/// Find a value transform between two given float smoothly
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <param name="t">Representation of the position. 0 means value A, 1 means value B.</param>
	public static Float3 Lerp (Float3 a, Float3 b, float t) {
		t = t.Clamp01();
		return new Float3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
	}

	/// <summary>
	/// Find a value transform between two given float smoothly without limit the t value
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <param name="t">Representation of the position. 0 means value A, 1 means value B.</param>
	public static Float3 LerpUnclamped (Float3 a, Float3 b, float t) {
		return new Float3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
	}

	/// <summary>
	/// Moves a point towards the target with distance limitation
	/// </summary>
	/// <param name="current">Starting position</param>
	/// <param name="target">Target end position</param>
	/// <param name="maxDistanceDelta">Distance limit</param>
	/// <returns>The new position</returns>
	public static Float3 MoveTowards (Float3 current, Float3 target, float maxDistanceDelta) {
		float num = target.x - current.x;
		float num2 = target.y - current.y;
		float num3 = target.z - current.z;
		float num4 = num * num + num2 * num2 + num3 * num3;
		if (num4 == 0f || (maxDistanceDelta >= 0f && num4 <= maxDistanceDelta * maxDistanceDelta)) {
			return target;
		}

		float num5 = (float)Math.Sqrt(num4);
		return new Float3(current.x + num / num5 * maxDistanceDelta, current.y + num2 / num5 * maxDistanceDelta, current.z + num3 / num5 * maxDistanceDelta);
	}

	/// <summary>
	/// 3D vector with float data values
	/// </summary>
	public Float3 (float x, float y, float z) {
		this.x = x;
		this.y = y;
		this.z = z;
	}

	/// <summary>
	/// 3D vector with float data values. z = 0
	/// </summary>
	public Float3 (float x, float y) {
		this.x = x;
		this.y = y;
		z = 0f;
	}

	/// <summary>
	/// Set all values of this vector
	/// </summary>
	/// <param name="newX"></param>
	/// <param name="newY"></param>
	/// <param name="newZ"></param>
	public void Set (float newX, float newY, float newZ) {
		x = newX;
		y = newY;
		z = newZ;
	}

	public readonly void Deconstruct (out float x, out float y, out float z) {
		x = this.x;
		y = this.y;
		z = this.z;
	}

	public readonly override int GetHashCode () {
		return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2);
	}

	public override readonly bool Equals (object other) {
		if (other is not Float3) {
			return false;
		}

		return Equals((Float3)other);
	}

	public readonly bool Equals (Float3 other) {
		return x == other.x && y == other.y && z == other.z;
	}

	public readonly override string ToString () => $"({x}, {y}, {z})";

	public static Float3 Normalize (Float3 value) {
		float num = value.Magnitude;
		if (num > 1E-05f) {
			return value / num;
		}

		return Zero;
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

	/// <summary>
	/// The dot product of two vectors returns a float value representing the cosine of the angle between them
	/// </summary>
	public static float Dot (Float3 lhs, Float3 rhs) => lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;

	/// <summary>
	/// Angle in degrees between from and to
	/// </summary>
	public static float Angle (Float3 from, Float3 to) {
		float num = (float)Math.Sqrt(from.SqrMagnitude * to.SqrMagnitude);
		if (num < 1E-15f) {
			return 0f;
		}
		float num2 = (Dot(from, to) / num).Clamp(-1f, 1f);
		return (float)Math.Acos(num2) * 57.29578f;
	}

	/// <summary>
	/// Signed angle in degrees between from and to in axis
	/// </summary>
	public static float SignedAngle (Float3 from, Float3 to, Float3 axis) {
		float num = Angle(from, to);
		float num2 = from.y * to.z - from.z * to.y;
		float num3 = from.z * to.x - from.x * to.z;
		float num4 = from.x * to.y - from.y * to.x;
		float num5 = Math.Sign(axis.x * num2 + axis.y * num3 + axis.z * num4);
		return num * num5;
	}

	/// <summary>
	/// Distance between to points
	/// </summary>
	public static float Distance (Float3 a, Float3 b) {
		float num = a.x - b.x;
		float num2 = a.y - b.y;
		float num3 = a.z - b.z;
		return (float)Math.Sqrt(num * num + num2 * num2 + num3 * num3);
	}

	// OPR
	public static Float3 operator + (Float3 a, Float3 b) {
		return new Float3(a.x + b.x, a.y + b.y, a.z + b.z);
	}

	public static Float3 operator - (Float3 a, Float3 b) {
		return new Float3(a.x - b.x, a.y - b.y, a.z - b.z);
	}

	public static Float3 operator - (Float3 a) {
		return new Float3(0f - a.x, 0f - a.y, 0f - a.z);
	}

	public static Float3 operator * (Float3 a, float d) {
		return new Float3(a.x * d, a.y * d, a.z * d);
	}

	public static Float3 operator * (float d, Float3 a) {
		return new Float3(a.x * d, a.y * d, a.z * d);
	}

	public static Float3 operator / (Float3 a, float d) {
		return new Float3(a.x / d, a.y / d, a.z / d);
	}

	public static bool operator == (Float3 lhs, Float3 rhs) {
		float num = lhs.x - rhs.x;
		float num2 = lhs.y - rhs.y;
		float num3 = lhs.z - rhs.z;
		float num4 = num * num + num2 * num2 + num3 * num3;
		return num4 < 9.99999944E-11f;
	}

	public static bool operator != (Float3 lhs, Float3 rhs) {
		return !(lhs == rhs);
	}

}