using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;


namespace AngeliaFramework {
	[Serializable]
	public struct Float3 : IEquatable<Float3>, IFormattable {
		public const float kEpsilon = 1E-05f;
		public const float kEpsilonNormalSqrt = 1E-15f;

		public float x;
		public float y;
		public float z;

		private static readonly Float3 zeroVector = new(0f, 0f, 0f);

		private static readonly Float3 oneVector = new(1f, 1f, 1f);

		private static readonly Float3 upVector = new(0f, 1f, 0f);

		private static readonly Float3 downVector = new(0f, -1f, 0f);

		private static readonly Float3 leftVector = new(-1f, 0f, 0f);

		private static readonly Float3 rightVector = new(1f, 0f, 0f);

		private static readonly Float3 forwardVector = new(0f, 0f, 1f);

		private static readonly Float3 backVector = new(0f, 0f, -1f);

		private static readonly Float3 positiveInfinityVector = new(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);

		private static readonly Float3 negativeInfinityVector = new(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

		public float this[int index] {
			readonly get {
				return index switch {
					0 => x,
					1 => y,
					2 => z,
					_ => throw new IndexOutOfRangeException("Invalid Vector3 index!"),
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
						throw new IndexOutOfRangeException("Invalid Vector3 index!");
				}
			}
		}

		public readonly Float3 normalized {

			get {
				return Normalize(this);
			}
		}

		public readonly float magnitude {

			get {
				return (float)Math.Sqrt(x * x + y * y + z * z);
			}
		}

		public readonly float sqrMagnitude {

			get {
				return x * x + y * y + z * z;
			}
		}

		public static Float3 zero {

			get {
				return zeroVector;
			}
		}

		public static Float3 one {

			get {
				return oneVector;
			}
		}

		public static Float3 forward {

			get {
				return forwardVector;
			}
		}

		public static Float3 back {

			get {
				return backVector;
			}
		}

		public static Float3 up {

			get {
				return upVector;
			}
		}

		public static Float3 down {

			get {
				return downVector;
			}
		}

		public static Float3 left {

			get {
				return leftVector;
			}
		}

		public static Float3 right {

			get {
				return rightVector;
			}
		}

		public static Float3 positiveInfinity {

			get {
				return positiveInfinityVector;
			}
		}

		public static Float3 negativeInfinity {

			get {
				return negativeInfinityVector;
			}
		}

		public static Float3 Lerp (Float3 a, Float3 b, float t) {
			t = t.Clamp01();
			return new Float3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
		}

		public static Float3 LerpUnclamped (Float3 a, Float3 b, float t) {
			return new Float3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
		}

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

		public static Float3 SmoothDamp (Float3 current, Float3 target, ref Float3 currentVelocity, float smoothTime, float maxSpeed, float deltaTime) {
			float num;
			float num2;
			float num3;
			smoothTime = Math.Max(0.0001f, smoothTime);
			float num4 = 2f / smoothTime;
			float num5 = num4 * deltaTime;
			float num6 = 1f / (1f + num5 + 0.48f * num5 * num5 + 0.235f * num5 * num5 * num5);
			float num7 = current.x - target.x;
			float num8 = current.y - target.y;
			float num9 = current.z - target.z;
			Float3 vector = target;
			float num10 = maxSpeed * smoothTime;
			float num11 = num10 * num10;
			float num12 = num7 * num7 + num8 * num8 + num9 * num9;
			if (num12 > num11) {
				float num13 = (float)Math.Sqrt(num12);
				num7 = num7 / num13 * num10;
				num8 = num8 / num13 * num10;
				num9 = num9 / num13 * num10;
			}

			target.x = current.x - num7;
			target.y = current.y - num8;
			target.z = current.z - num9;
			float num14 = (currentVelocity.x + num4 * num7) * deltaTime;
			float num15 = (currentVelocity.y + num4 * num8) * deltaTime;
			float num16 = (currentVelocity.z + num4 * num9) * deltaTime;
			currentVelocity.x = (currentVelocity.x - num4 * num14) * num6;
			currentVelocity.y = (currentVelocity.y - num4 * num15) * num6;
			currentVelocity.z = (currentVelocity.z - num4 * num16) * num6;
			num = target.x + (num7 + num14) * num6;
			num2 = target.y + (num8 + num15) * num6;
			num3 = target.z + (num9 + num16) * num6;
			float num17 = vector.x - current.x;
			float num18 = vector.y - current.y;
			float num19 = vector.z - current.z;
			float num20 = num - vector.x;
			float num21 = num2 - vector.y;
			float num22 = num3 - vector.z;
			if (num17 * num20 + num18 * num21 + num19 * num22 > 0f) {
				num = vector.x;
				num2 = vector.y;
				num3 = vector.z;
				currentVelocity.x = (num - vector.x) / deltaTime;
				currentVelocity.y = (num2 - vector.y) / deltaTime;
				currentVelocity.z = (num3 - vector.z) / deltaTime;
			}

			return new Float3(num, num2, num3);
		}

		public Float3 (float x, float y, float z) {
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public Float3 (float x, float y) {
			this.x = x;
			this.y = y;
			z = 0f;
		}

		public void Set (float newX, float newY, float newZ) {
			x = newX;
			y = newY;
			z = newZ;
		}

		public static Float3 Scale (Float3 a, Float3 b) {
			return new Float3(a.x * b.x, a.y * b.y, a.z * b.z);
		}

		public void Scale (Float3 scale) {
			x *= scale.x;
			y *= scale.y;
			z *= scale.z;
		}

		public static Float3 Cross (Float3 lhs, Float3 rhs) {
			return new Float3(lhs.y * rhs.z - lhs.z * rhs.y, lhs.z * rhs.x - lhs.x * rhs.z, lhs.x * rhs.y - lhs.y * rhs.x);
		}

		public override int GetHashCode () {
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

		public static Float3 Reflect (Float3 inDirection, Float3 inNormal) {
			float num = -2f * Dot(inNormal, inDirection);
			return new Float3(num * inNormal.x + inDirection.x, num * inNormal.y + inDirection.y, num * inNormal.z + inDirection.z);
		}

		public static Float3 Normalize (Float3 value) {
			float num = Magnitude(value);
			if (num > 1E-05f) {
				return value / num;
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

		public static float Dot (Float3 lhs, Float3 rhs) {
			return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
		}

		private static volatile float FloatMinNormal = 1.17549435E-38f;
		private static volatile float FloatMinDenormal = float.Epsilon;
		private static bool IsFlushToZeroEnabled = FloatMinDenormal == 0f;
		private static readonly float Epsilon = IsFlushToZeroEnabled ? FloatMinNormal : FloatMinDenormal;

		public static Float3 Project (Float3 vector, Float3 onNormal) {
			float num = Dot(onNormal, onNormal);
			if (num < Epsilon) {
				return zero;
			}
			float num2 = Dot(vector, onNormal);
			return new Float3(onNormal.x * num2 / num, onNormal.y * num2 / num, onNormal.z * num2 / num);
		}

		public static Float3 ProjectOnPlane (Float3 vector, Float3 planeNormal) {
			float num = Dot(planeNormal, planeNormal);
			if (num < Epsilon) {
				return vector;
			}

			float num2 = Dot(vector, planeNormal);
			return new Float3(vector.x - planeNormal.x * num2 / num, vector.y - planeNormal.y * num2 / num, vector.z - planeNormal.z * num2 / num);
		}

		public static float Angle (Float3 from, Float3 to) {
			float num = (float)Math.Sqrt(from.sqrMagnitude * to.sqrMagnitude);
			if (num < 1E-15f) {
				return 0f;
			}
			float num2 = (Dot(from, to) / num).Clamp(-1f, 1f);
			return (float)Math.Acos(num2) * 57.29578f;
		}

		public static float SignedAngle (Float3 from, Float3 to, Float3 axis) {
			float num = Angle(from, to);
			float num2 = from.y * to.z - from.z * to.y;
			float num3 = from.z * to.x - from.x * to.z;
			float num4 = from.x * to.y - from.y * to.x;
			float num5 = Math.Sign(axis.x * num2 + axis.y * num3 + axis.z * num4);
			return num * num5;
		}

		public static float Distance (Float3 a, Float3 b) {
			float num = a.x - b.x;
			float num2 = a.y - b.y;
			float num3 = a.z - b.z;
			return (float)Math.Sqrt(num * num + num2 * num2 + num3 * num3);
		}

		public static Float3 ClampMagnitude (Float3 vector, float maxLength) {
			float num = vector.sqrMagnitude;
			if (num > maxLength * maxLength) {
				float num2 = (float)Math.Sqrt(num);
				float num3 = vector.x / num2;
				float num4 = vector.y / num2;
				float num5 = vector.z / num2;
				return new Float3(num3 * maxLength, num4 * maxLength, num5 * maxLength);
			}

			return vector;
		}

		public static float Magnitude (Float3 vector) {
			return (float)Math.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
		}

		public static float SqrMagnitude (Float3 vector) {
			return vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
		}

		public static Float3 Min (Float3 lhs, Float3 rhs) {
			return new Float3(Math.Min(lhs.x, rhs.x), Math.Min(lhs.y, rhs.y), Math.Min(lhs.z, rhs.z));
		}

		public static Float3 Max (Float3 lhs, Float3 rhs) {
			return new Float3(Math.Max(lhs.x, rhs.x), Math.Max(lhs.y, rhs.y), Math.Max(lhs.z, rhs.z));
		}

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
			return string.Format("({0}, {1}, {2})", x.ToString(format, formatProvider), y.ToString(format, formatProvider), z.ToString(format, formatProvider));
		}

#if UNITY_2017_1_OR_NEWER
		public static implicit operator UnityEngine.Vector3 (Float3 v) => new(v.x, v.y, v.z);
		public static implicit operator Float3 (UnityEngine.Vector3 v) => new(v.x, v.y, v.z);
#endif

	}
}