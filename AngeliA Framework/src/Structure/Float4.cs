using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;

namespace AngeliA;

/// <summary>
/// 4D vector with float data values
/// </summary>
[Serializable]
public struct Float4 : IEquatable<Float4> {

	public static readonly Float4 Zero = new(0f, 0f, 0f, 0f);
	public static readonly Float4 One = new(1f, 1f, 1f, 1f);

	public float x;
	public float y;
	public float z;
	public float w;

	public float this[int index] {
		readonly get {
			return index switch {
				0 => x,
				1 => y,
				2 => z,
				3 => w,
				_ => throw new IndexOutOfRangeException("Invalid Vector4 index!"),
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
				case 3:
					w = value;
					break;
				default:
					throw new IndexOutOfRangeException("Invalid Vector4 index!");
			}
		}
	}

	public readonly Float4 Normalized {

		get {
			return Normalize(this);
		}
	}

	public readonly float Magnitude => (float)Math.Sqrt(Dot(this, this));

	public readonly float SqrMagnitude => Dot(this, this);

	public Float4 (float x, float y, float z, float w) {
		this.x = x;
		this.y = y;
		this.z = z;
		this.w = w;
	}

	public readonly void Deconstruct (out float x, out float y, out float z, out float w) {
		x = this.x;
		y = this.y;
		z = this.z;
		w = this.w;
	}

	public readonly override int GetHashCode () {
		return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2) ^ (w.GetHashCode() >> 1);
	}

	public override readonly bool Equals (object other) {
		if (other is not Float4) {
			return false;
		}

		return Equals((Float4)other);
	}

	public readonly bool Equals (Float4 other) {
		return x == other.x && y == other.y && z == other.z && w == other.w;
	}

	public readonly override string ToString () => $"({x}, {y}, {z}, {w})";

	public static Float4 Normalize (Float4 a) {
		float num = a.Magnitude;
		if (num > 1E-05f) {
			return a / num;
		}

		return Zero;
	}

	private static float Dot (Float4 a, Float4 b) => a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;

	// OPR
	public static Float4 operator + (Float4 a, Float4 b) {
		return new Float4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
	}

	public static Float4 operator - (Float4 a, Float4 b) {
		return new Float4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
	}

	public static Float4 operator - (Float4 a) {
		return new Float4(0f - a.x, 0f - a.y, 0f - a.z, 0f - a.w);
	}

	public static Float4 operator * (Float4 a, float d) {
		return new Float4(a.x * d, a.y * d, a.z * d, a.w * d);
	}

	public static Float4 operator * (float d, Float4 a) {
		return new Float4(a.x * d, a.y * d, a.z * d, a.w * d);
	}

	public static Float4 operator / (Float4 a, float d) {
		return new Float4(a.x / d, a.y / d, a.z / d, a.w / d);
	}

	public static bool operator == (Float4 lhs, Float4 rhs) {
		float num = lhs.x - rhs.x;
		float num2 = lhs.y - rhs.y;
		float num3 = lhs.z - rhs.z;
		float num4 = lhs.w - rhs.w;
		float num5 = num * num + num2 * num2 + num3 * num3 + num4 * num4;
		return num5 < 9.99999944E-11f;
	}

	public static bool operator != (Float4 lhs, Float4 rhs) {
		return !(lhs == rhs);
	}

	public static implicit operator Float4 (Float3 v) {
		return new Float4(v.x, v.y, v.z, 0f);
	}

	public static implicit operator Float3 (Float4 v) {
		return new Float3(v.x, v.y, v.z);
	}

	public static implicit operator Float4 (Float2 v) {
		return new Float4(v.x, v.y, 0f, 0f);
	}

	public static implicit operator Float2 (Float4 v) {
		return new Float2(v.x, v.y);
	}

}
