using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;


namespace AngeliA {
	[Serializable]
	public struct Float4 : IEquatable<Float4>, IFormattable {
		public const float kEpsilon = 1E-05f;

		public float x;

		public float y;

		public float z;

		public float w;

		private static readonly Float4 zeroVector = new(0f, 0f, 0f, 0f);

		private static readonly Float4 oneVector = new(1f, 1f, 1f, 1f);

		private static readonly Float4 positiveInfinityVector = new(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);

		private static readonly Float4 negativeInfinityVector = new(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

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

		public readonly Float4 normalized {

			get {
				return Normalize(this);
			}
		}

		public readonly float magnitude {

			get {
				return (float)Math.Sqrt(Dot(this, this));
			}
		}

		public readonly float sqrMagnitude {

			get {
				return Dot(this, this);
			}
		}

		public static Float4 zero {

			get {
				return zeroVector;
			}
		}

		public static Float4 one {

			get {
				return oneVector;
			}
		}

		public static Float4 positiveInfinity {

			get {
				return positiveInfinityVector;
			}
		}

		public static Float4 negativeInfinity {

			get {
				return negativeInfinityVector;
			}
		}

		public Float4 (float x, float y, float z, float w) {
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

		public Float4 (float x, float y, float z) {
			this.x = x;
			this.y = y;
			this.z = z;
			w = 0f;
		}

		public Float4 (float x, float y) {
			this.x = x;
			this.y = y;
			z = 0f;
			w = 0f;
		}

		public void Set (float newX, float newY, float newZ, float newW) {
			x = newX;
			y = newY;
			z = newZ;
			w = newW;
		}

		public static Float4 Lerp (Float4 a, Float4 b, float t) {
			t = t.Clamp01();
			return new Float4(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t, a.w + (b.w - a.w) * t);
		}

		public static Float4 LerpUnclamped (Float4 a, Float4 b, float t) {
			return new Float4(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t, a.w + (b.w - a.w) * t);
		}

		public static Float4 MoveTowards (Float4 current, Float4 target, float maxDistanceDelta) {
			float num = target.x - current.x;
			float num2 = target.y - current.y;
			float num3 = target.z - current.z;
			float num4 = target.w - current.w;
			float num5 = num * num + num2 * num2 + num3 * num3 + num4 * num4;
			if (num5 == 0f || (maxDistanceDelta >= 0f && num5 <= maxDistanceDelta * maxDistanceDelta)) {
				return target;
			}

			float num6 = (float)Math.Sqrt(num5);
			return new Float4(current.x + num / num6 * maxDistanceDelta, current.y + num2 / num6 * maxDistanceDelta, current.z + num3 / num6 * maxDistanceDelta, current.w + num4 / num6 * maxDistanceDelta);
		}

		public static Float4 Scale (Float4 a, Float4 b) {
			return new Float4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
		}

		public void Scale (Float4 scale) {
			x *= scale.x;
			y *= scale.y;
			z *= scale.z;
			w *= scale.w;
		}

		public override int GetHashCode () {
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

		public static Float4 Normalize (Float4 a) {
			float num = Magnitude(a);
			if (num > 1E-05f) {
				return a / num;
			}

			return zero;
		}

		public void Normalize () {
			float num = Magnitude(this);
			if (num > 1E-05f) {
				this /= num;
			} else {
				this = zero;
			}
		}

		public static float Dot (Float4 a, Float4 b) {
			return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
		}

		public static Float4 Project (Float4 a, Float4 b) {
			return b * (Dot(a, b) / Dot(b, b));
		}

		public static float Distance (Float4 a, Float4 b) {
			return Magnitude(a - b);
		}

		public static float Magnitude (Float4 a) {
			return (float)Math.Sqrt(Dot(a, a));
		}

		public static Float4 Min (Float4 lhs, Float4 rhs) {
			return new Float4(Math.Min(lhs.x, rhs.x), Math.Min(lhs.y, rhs.y), Math.Min(lhs.z, rhs.z), Math.Min(lhs.w, rhs.w));
		}

		public static Float4 Max (Float4 lhs, Float4 rhs) {
			return new Float4(Math.Max(lhs.x, rhs.x), Math.Max(lhs.y, rhs.y), Math.Max(lhs.z, rhs.z), Math.Max(lhs.w, rhs.w));
		}

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

		public override string ToString () {
			return ToString(null, null);
		}

		public string ToString (string format) {
			return ToString(format, null);
		}

		public string ToString (string format, IFormatProvider formatProvider) {
			if (string.IsNullOrEmpty(format)) {
				format = "F2";
			}

			formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;

			return string.Format("({0}, {1}, {2}, {3})", x.ToString(format, formatProvider), y.ToString(format, formatProvider), z.ToString(format, formatProvider), w.ToString(format, formatProvider));
		}

		public static float SqrMagnitude (Float4 a) {
			return Dot(a, a);
		}

		public readonly float SqrMagnitude () {
			return Dot(this, this);
		}


	}
}
