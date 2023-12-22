using System;
using System.Globalization;


namespace AngeliaFramework {
	public struct FRect : IEquatable<FRect>, IFormattable {

		private float m_XMin;

		private float m_YMin;

		private float m_Width;

		private float m_Height;

		public static FRect zero => new(0f, 0f, 0f, 0f);

		public float x {
			readonly get {
				return m_XMin;
			}

			set {
				m_XMin = value;
			}
		}

		public float y {
			readonly get {
				return m_YMin;
			}

			set {
				m_YMin = value;
			}
		}

		public Float2 position {
			readonly get {
				return new Float2(m_XMin, m_YMin);
			}

			set {
				m_XMin = value.x;
				m_YMin = value.y;
			}
		}

		public Float2 center {
			readonly get {
				return new Float2(x + m_Width / 2f, y + m_Height / 2f);
			}

			set {
				m_XMin = value.x - m_Width / 2f;
				m_YMin = value.y - m_Height / 2f;
			}
		}

		public Float2 min {
			readonly get {
				return new Float2(xMin, yMin);
			}

			set {
				xMin = value.x;
				yMin = value.y;
			}
		}

		public Float2 max {
			readonly get {
				return new Float2(xMax, yMax);
			}

			set {
				xMax = value.x;
				yMax = value.y;
			}
		}

		public float width {
			readonly get {
				return m_Width;
			}

			set {
				m_Width = value;
			}
		}

		public float height {
			readonly get {
				return m_Height;
			}

			set {
				m_Height = value;
			}
		}

		public Float2 size {
			readonly get {
				return new Float2(m_Width, m_Height);
			}

			set {
				m_Width = value.x;
				m_Height = value.y;
			}
		}

		public float xMin {
			readonly get {
				return m_XMin;
			}

			set {
				float num = xMax;
				m_XMin = value;
				m_Width = num - m_XMin;
			}
		}

		public float yMin {
			readonly get {
				return m_YMin;
			}

			set {
				float num = yMax;
				m_YMin = value;
				m_Height = num - m_YMin;
			}
		}

		public float xMax {
			readonly get {
				return m_Width + m_XMin;
			}

			set {
				m_Width = value - m_XMin;
			}
		}

		public float yMax {
			readonly get {
				return m_Height + m_YMin;
			}

			set {
				m_Height = value - m_YMin;
			}
		}


		public FRect (float x, float y, float width, float height) {
			m_XMin = x;
			m_YMin = y;
			m_Width = width;
			m_Height = height;
		}

		public FRect (Float2 position, Float2 size) {
			m_XMin = position.x;
			m_YMin = position.y;
			m_Width = size.x;
			m_Height = size.y;
		}

		public FRect (FRect source) {
			m_XMin = source.m_XMin;
			m_YMin = source.m_YMin;
			m_Width = source.m_Width;
			m_Height = source.m_Height;
		}

		public static FRect MinMaxRect (float xmin, float ymin, float xmax, float ymax) {
			return new FRect(xmin, ymin, xmax - xmin, ymax - ymin);
		}

		public void Set (float x, float y, float width, float height) {
			m_XMin = x;
			m_YMin = y;
			m_Width = width;
			m_Height = height;
		}

		public readonly bool Contains (Float2 point) {
			return point.x >= xMin && point.x < xMax && point.y >= yMin && point.y < yMax;
		}

		public readonly bool Contains (Float3 point) {
			return point.x >= xMin && point.x < xMax && point.y >= yMin && point.y < yMax;
		}

		public readonly bool Contains (Float3 point, bool allowInverse) {
			if (!allowInverse) {
				return Contains(point);
			}

			bool flag = (width < 0f && point.x <= xMin && point.x > xMax) || (width >= 0f && point.x >= xMin && point.x < xMax);
			bool flag2 = (height < 0f && point.y <= yMin && point.y > yMax) || (height >= 0f && point.y >= yMin && point.y < yMax);
			return flag && flag2;
		}

		private static FRect OrderMinMax (FRect rect) {
			if (rect.xMin > rect.xMax) {
				(rect.xMax, rect.xMin) = (rect.xMin, rect.xMax);
			}

			if (rect.yMin > rect.yMax) {
				(rect.yMax, rect.yMin) = (rect.yMin, rect.yMax);
			}

			return rect;
		}

		public readonly bool Overlaps (FRect other) {
			return other.xMax > xMin && other.xMin < xMax && other.yMax > yMin && other.yMin < yMax;
		}

		public readonly bool Overlaps (FRect other, bool allowInverse) {
			FRect rect = this;
			if (allowInverse) {
				rect = OrderMinMax(rect);
				other = OrderMinMax(other);
			}

			return rect.Overlaps(other);
		}

		public static Float2 NormalizedToPoint (FRect rectangle, Float2 normalizedRectCoordinates) {
			return new Float2(Util.Lerp(rectangle.x, rectangle.xMax, normalizedRectCoordinates.x), Util.Lerp(rectangle.y, rectangle.yMax, normalizedRectCoordinates.y));
		}

		public static Float2 PointToNormalized (FRect rectangle, Float2 point) {
			return new Float2(Util.InverseLerp(rectangle.x, rectangle.xMax, point.x), Util.InverseLerp(rectangle.y, rectangle.yMax, point.y));
		}

		public static bool operator != (FRect lhs, FRect rhs) {
			return !(lhs == rhs);
		}

		public static bool operator == (FRect lhs, FRect rhs) {
			return lhs.x == rhs.x && lhs.y == rhs.y && lhs.width == rhs.width && lhs.height == rhs.height;
		}

		public override readonly int GetHashCode () {
			return x.GetHashCode() ^ (width.GetHashCode() << 2) ^ (y.GetHashCode() >> 2) ^ (height.GetHashCode() >> 1);
		}

		public override readonly bool Equals (object other) {
			if (other is not FRect) {
				return false;
			}

			return Equals((FRect)other);
		}

		public readonly bool Equals (FRect other) {
			return x.Equals(other.x) && y.Equals(other.y) && width.Equals(other.width) && height.Equals(other.height);
		}

		public override readonly string ToString () {
			return ToString(null, null);
		}

		public readonly string ToString (string format) {
			return ToString(format, null);
		}

		public readonly string ToString (string format, IFormatProvider formatProvider) {
			if (string.IsNullOrEmpty(format)) {
				format = "F2";
			}

			formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;

			return string.Format("(x:{0}, y:{1}, width:{2}, height:{3})", x.ToString(format, formatProvider), y.ToString(format, formatProvider), width.ToString(format, formatProvider), height.ToString(format, formatProvider));
		}

#if UNITY_2017_1_OR_NEWER
		public static implicit operator UnityEngine.Rect (FRect v) => new(v.x, v.y, v.width, v.height);
		public static implicit operator FRect (UnityEngine.Rect v) => new(v.x, v.y, v.width, v.height);
#endif

	}
}
