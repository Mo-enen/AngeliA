using System;
using System.Globalization;

namespace AngeliA;

/// <summary>
/// Rectangle with float data
/// </summary>
[Serializable]
public struct FRect : IEquatable<FRect> {

	/// <summary>
	/// Left position
	/// </summary>
	public float x;
	/// <summary>
	/// Bottom position
	/// </summary>
	public float y;
	/// <summary>
	/// Horizontal size
	/// </summary>
	public float width;
	/// <summary>
	/// Vertical size
	/// </summary>
	public float height;

	public Float2 position {
		readonly get {
			return new Float2(x, y);
		}

		set {
			x = value.x;
			y = value.y;
		}
	}
	public Float2 center {
		readonly get => new(x + width / 2f, y + height / 2f);
		set {
			x = value.x - width / 2f;
			y = value.y - height / 2f;
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
	public Float2 size {
		readonly get {
			return new Float2(width, height);
		}

		set {
			width = value.x;
			height = value.y;
		}
	}
	public float xMin {
		readonly get {
			return x;
		}

		set {
			float num = xMax;
			x = value;
			width = num - x;
		}
	}
	public float yMin {
		readonly get {
			return y;
		}

		set {
			float num = yMax;
			y = value;
			height = num - y;
		}
	}
	public float xMax {
		readonly get {
			return width + x;
		}

		set {
			width = value - x;
		}
	}
	public float yMax {
		readonly get {
			return height + y;
		}

		set {
			height = value - y;
		}
	}

	// API
	public FRect (float x, float y, float width, float height) {
		this.x = x;
		this.y = y;
		this.width = width;
		this.height = height;
	}

	public FRect (Float2 position, Float2 size) {
		x = position.x;
		y = position.y;
		width = size.x;
		height = size.y;
	}

	public FRect (FRect source) {
		x = source.x;
		y = source.y;
		width = source.width;
		height = source.height;
	}

	/// <summary>
	/// Create a rectangle from min and max positions
	/// </summary>
	public static FRect MinMaxRect (float xmin, float ymin, float xmax, float ymax) {
		return new FRect(xmin, ymin, xmax - xmin, ymax - ymin);
	}

	/// <summary>
	/// True if the given point inside this rectangle
	/// </summary>
	public readonly bool Contains (Float2 point) {
		return point.x >= xMin && point.x < xMax && point.y >= yMin && point.y < yMax;
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

	/// <summary>
	/// True if the given rectangle overlaps the current one
	/// </summary>
	public readonly bool Overlaps (FRect other) {
		return other.xMax > xMin && other.xMin < xMax && other.yMax > yMin && other.yMin < yMax;
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

	public override readonly string ToString () => $"({x}, {y}, {width}, {height})";

	// OPR
	public static bool operator != (FRect lhs, FRect rhs) {
		return !(lhs == rhs);
	}

	public static bool operator == (FRect lhs, FRect rhs) {
		return lhs.x == rhs.x && lhs.y == rhs.y && lhs.width == rhs.width && lhs.height == rhs.height;
	}

}
