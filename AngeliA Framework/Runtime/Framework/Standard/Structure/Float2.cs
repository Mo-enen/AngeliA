using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;

namespace AngeliaFramework {
	[Serializable]
	public struct Float2 : IEquatable<Float2>, IFormattable {

		public float x;
		public float y;

		private static readonly Float2 zeroVector = new(0f, 0f);

		private static readonly Float2 oneVector = new(1f, 1f);

		private static readonly Float2 upVector = new(0f, 1f);

		private static readonly Float2 downVector = new(0f, -1f);

		private static readonly Float2 leftVector = new(-1f, 0f);

		private static readonly Float2 rightVector = new(1f, 0f);

		private static readonly Float2 positiveInfinityVector = new(float.PositiveInfinity, float.PositiveInfinity);

		private static readonly Float2 negativeInfinityVector = new(float.NegativeInfinity, float.NegativeInfinity);

		public const float kEpsilon = 1E-05f;

		public const float kEpsilonNormalSqrt = 1E-15f;

		public float this[int index] {
			readonly get {
				return index switch {
					0 => x,
					1 => y,
					_ => throw new IndexOutOfRangeException("Invalid Vector2 index!"),
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
						throw new IndexOutOfRangeException("Invalid Vector2 index!");
				}
			}
		}

		public readonly Float2 normalized {

			get {
				Float2 result = new(x, y);
				result.Normalize();
				return result;
			}
		}

		public readonly float magnitude {

			get {
				return (float)Math.Sqrt(x * x + y * y);
			}
		}

		public readonly float sqrMagnitude {

			get {
				return x * x + y * y;
			}
		}

		public static Float2 zero {

			get {
				return zeroVector;
			}
		}

		public static Float2 one {

			get {
				return oneVector;
			}
		}

		public static Float2 up {

			get {
				return upVector;
			}
		}

		public static Float2 down {

			get {
				return downVector;
			}
		}

		public static Float2 left {

			get {
				return leftVector;
			}
		}

		public static Float2 right {

			get {
				return rightVector;
			}
		}

		public static Float2 positiveInfinity {

			get {
				return positiveInfinityVector;
			}
		}

		public static Float2 negativeInfinity {

			get {
				return negativeInfinityVector;
			}
		}

		public Float2 (float x, float y) {
			this.x = x;
			this.y = y;
		}

		public void Set (float newX, float newY) {
			x = newX;
			y = newY;
		}

		public static Float2 Lerp (Float2 a, Float2 b, float t) {
			t = t.Clamp01();
			return new Float2(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t);
		}

		public static Float2 LerpUnclamped (Float2 a, Float2 b, float t) {
			return new Float2(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t);
		}

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

		public static Float2 Scale (Float2 a, Float2 b) {
			return new Float2(a.x * b.x, a.y * b.y);
		}

		public void Scale (Float2 scale) {
			x *= scale.x;
			y *= scale.y;
		}

		public void Normalize () {
			float num = magnitude;
			if (num > 1E-05f) {
				this /= num;
			} else {
				this = zero;
			}
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

			return string.Format("({0}, {1})", x.ToString(format, formatProvider), y.ToString(format, formatProvider));
		}

		public override int GetHashCode () {
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

		public static Float2 Reflect (Float2 inDirection, Float2 inNormal) {
			float num = -2f * Dot(inNormal, inDirection);
			return new Float2(num * inNormal.x + inDirection.x, num * inNormal.y + inDirection.y);
		}

		public static Float2 Perpendicular (Float2 inDirection) {
			return new Float2(0f - inDirection.y, inDirection.x);
		}

		public static float Dot (Float2 lhs, Float2 rhs) {
			return lhs.x * rhs.x + lhs.y * rhs.y;
		}

		public static float Angle (Float2 from, Float2 to) {
			float num = (float)Math.Sqrt(from.sqrMagnitude * to.sqrMagnitude);
			if (num < 1E-15f) return 0f;
			//float num2 = Mathf.Clamp(Dot(from, to) / num, -1f, 1f);
			float num2 = (Dot(from, to) / num).Clamp(-1f, 1f);
			return (float)Math.Acos(num2) * 57.29578f;
		}

		public static float SignedAngle (Float2 from, Float2 to) {
			float num = Angle(from, to);
			float num2 = Math.Sign(from.x * to.y - from.y * to.x);
			return num * num2;
		}

		public static float Distance (Float2 a, Float2 b) {
			float num = a.x - b.x;
			float num2 = a.y - b.y;
			return (float)Math.Sqrt(num * num + num2 * num2);
		}

		public static Float2 ClampMagnitude (Float2 vector, float maxLength) {
			float num = vector.sqrMagnitude;
			if (num > maxLength * maxLength) {
				float num2 = (float)Math.Sqrt(num);
				float num3 = vector.x / num2;
				float num4 = vector.y / num2;
				return new Float2(num3 * maxLength, num4 * maxLength);
			}

			return vector;
		}

		public static float SqrMagnitude (Float2 a) {
			return a.x * a.x + a.y * a.y;
		}

		public readonly float SqrMagnitude () {
			return x * x + y * y;
		}

		public static Float2 Min (Float2 lhs, Float2 rhs) {
			return new Float2(Math.Min(lhs.x, rhs.x), Math.Min(lhs.y, rhs.y));
		}

		public static Float2 Max (Float2 lhs, Float2 rhs) {
			return new Float2(Math.Max(lhs.x, rhs.x), Math.Max(lhs.y, rhs.y));
		}

		public static Float2 SmoothDamp (Float2 current, Float2 target, ref Float2 currentVelocity, float smoothTime, float maxSpeed, float deltaTime) {
			smoothTime = Math.Max(0.0001f, smoothTime);
			float num = 2f / smoothTime;
			float num2 = num * deltaTime;
			float num3 = 1f / (1f + num2 + 0.48f * num2 * num2 + 0.235f * num2 * num2 * num2);
			float num4 = current.x - target.x;
			float num5 = current.y - target.y;
			Float2 vector = target;
			float num6 = maxSpeed * smoothTime;
			float num7 = num6 * num6;
			float num8 = num4 * num4 + num5 * num5;
			if (num8 > num7) {
				float num9 = (float)Math.Sqrt(num8);
				num4 = num4 / num9 * num6;
				num5 = num5 / num9 * num6;
			}

			target.x = current.x - num4;
			target.y = current.y - num5;
			float num10 = (currentVelocity.x + num * num4) * deltaTime;
			float num11 = (currentVelocity.y + num * num5) * deltaTime;
			currentVelocity.x = (currentVelocity.x - num * num10) * num3;
			currentVelocity.y = (currentVelocity.y - num * num11) * num3;
			float num12 = target.x + (num4 + num10) * num3;
			float num13 = target.y + (num5 + num11) * num3;
			float num14 = vector.x - current.x;
			float num15 = vector.y - current.y;
			float num16 = num12 - vector.x;
			float num17 = num13 - vector.y;
			if (num14 * num16 + num15 * num17 > 0f) {
				num12 = vector.x;
				num13 = vector.y;
				currentVelocity.x = (num12 - vector.x) / deltaTime;
				currentVelocity.y = (num13 - vector.y) / deltaTime;
			}

			return new Float2(num12, num13);
		}

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

		public static implicit operator Float2 (Float3 v) {
			return new Float2(v.x, v.y);
		}

		public static implicit operator Float3 (Float2 v) {
			return new Float3(v.x, v.y, 0f);
		}


	}
}
