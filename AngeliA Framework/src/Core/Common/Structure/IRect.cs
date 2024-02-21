using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;


namespace AngeliA; 
[Serializable]
public struct IRect : IEquatable<IRect>, IFormattable {

	private int m_XMin;

	private int m_YMin;

	private int m_Width;

	private int m_Height;

	public int x {
		readonly get {
			return m_XMin;
		}

		set {
			m_XMin = value;
		}
	}

	public int y {
		readonly get {
			return m_YMin;
		}

		set {
			m_YMin = value;
		}
	}

	public readonly Float2 center {

		get {
			return new Float2((float)x + (float)m_Width / 2f, (float)y + (float)m_Height / 2f);
		}
	}

	public Int2 min {
		readonly get {
			return new Int2(xMin, yMin);
		}

		set {
			xMin = value.x;
			yMin = value.y;
		}
	}

	public Int2 max {
		readonly get {
			return new Int2(xMax, yMax);
		}

		set {
			xMax = value.x;
			yMax = value.y;
		}
	}

	public int width {
		readonly get {
			return m_Width;
		}

		set {
			m_Width = value;
		}
	}

	public int height {
		readonly get {
			return m_Height;
		}

		set {
			m_Height = value;
		}
	}

	public int xMin {
		readonly get {
			return Math.Min(m_XMin, m_XMin + m_Width);
		}

		set {
			int num = xMax;
			m_XMin = value;
			m_Width = num - m_XMin;
		}
	}

	public int yMin {
		readonly get {
			return Math.Min(m_YMin, m_YMin + m_Height);
		}

		set {
			int num = yMax;
			m_YMin = value;
			m_Height = num - m_YMin;
		}
	}

	public int xMax {
		readonly get {
			return Math.Max(m_XMin, m_XMin + m_Width);
		}

		set {
			m_Width = value - m_XMin;
		}
	}

	public int yMax {
		readonly get {
			return Math.Max(m_YMin, m_YMin + m_Height);
		}

		set {
			m_Height = value - m_YMin;
		}
	}

	public Int2 position {
		readonly get {
			return new Int2(m_XMin, m_YMin);
		}

		set {
			m_XMin = value.x;
			m_YMin = value.y;
		}
	}

	public Int2 size {
		readonly get {
			return new Int2(m_Width, m_Height);
		}

		set {
			m_Width = value.x;
			m_Height = value.y;
		}
	}

	public static IRect zero => new(0, 0, 0, 0);

	public void SetMinMax (Int2 minPosition, Int2 maxPosition) {
		min = minPosition;
		max = maxPosition;
	}

	public static IRect MinMaxRect (int minX, int minY, int maxX, int maxY) => new() {
		xMin = minX,
		yMin = minY,
		xMax = maxX,
		yMax = maxY,
	};

	public IRect (int xMin, int yMin, int width, int height) {
		m_XMin = xMin;
		m_YMin = yMin;
		m_Width = width;
		m_Height = height;
	}

	public IRect (Int2 position, Int2 size) {
		m_XMin = position.x;
		m_YMin = position.y;
		m_Width = size.x;
		m_Height = size.y;
	}

	public void ClampToBounds (IRect bounds) {
		position = new Int2(Math.Max(Math.Min(bounds.xMax, position.x), bounds.xMin), Math.Max(Math.Min(bounds.yMax, position.y), bounds.yMin));
		size = new Int2(Math.Min(bounds.xMax - position.x, size.x), Math.Min(bounds.yMax - position.y, size.y));
	}

	public readonly bool Contains (Int2 position) {
		return position.x >= xMin && position.y >= yMin && position.x < xMax && position.y < yMax;
	}

	public readonly bool Overlaps (IRect other) => other.xMin < xMax && other.xMax > xMin && other.yMin < yMax && other.yMax > yMin;

	public override readonly string ToString () {
		return ToString(null, null);
	}

	public readonly string ToString (string format) {
		return ToString(format, null);
	}

	public readonly string ToString (string format, System.IFormatProvider formatProvider) {
		formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;
		return string.Format("(x:{0}, y:{1}, width:{2}, height:{3})", x.ToString(format, formatProvider), y.ToString(format, formatProvider), width.ToString(format, formatProvider), height.ToString(format, formatProvider));
	}

	public static bool operator != (IRect lhs, IRect rhs) {
		return !(lhs == rhs);
	}

	public static bool operator == (IRect lhs, IRect rhs) {
		return lhs.x == rhs.x && lhs.y == rhs.y && lhs.width == rhs.width && lhs.height == rhs.height;
	}

	public override readonly int GetHashCode () {
		int hashCode = x.GetHashCode();
		int hashCode2 = y.GetHashCode();
		int hashCode3 = width.GetHashCode();
		int hashCode4 = height.GetHashCode();
		return hashCode ^ (hashCode2 << 4) ^ (hashCode2 >> 28) ^ (hashCode3 >> 4) ^ (hashCode3 << 28) ^ (hashCode4 >> 4) ^ (hashCode4 << 28);
	}

	public override readonly bool Equals (object other) {
		if (other is not IRect) {
			return false;
		}

		return Equals((IRect)other);
	}

	public readonly bool Equals (IRect other) {
		return m_XMin == other.m_XMin && m_YMin == other.m_YMin && m_Width == other.m_Width && m_Height == other.m_Height;
	}


}