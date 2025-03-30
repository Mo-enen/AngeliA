using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;

namespace AngeliA;

/// <summary>
/// rectangle with intager data
/// </summary>
[Serializable]
public struct IRect : IEquatable<IRect>, IFormattable {

	/// <summary>
	/// Left position
	/// </summary>
	public int x;
	/// <summary>
	/// Bottom position
	/// </summary>
	public int y;
	/// <summary>
	/// Horizontal size
	/// </summary>
	public int width;
	/// <summary>
	/// Vertical size
	/// </summary>
	public int height;

	public readonly Float2 center => new((float)x + (float)width / 2f, (float)y + (float)height / 2f);

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

	public int xMin {
		readonly get => Math.Min(x, x + width);

		set {
			int num = xMax;
			x = value;
			width = num - x;
		}
	}

	public int yMin {
		readonly get {
			return Math.Min(y, y + height);
		}

		set {
			int num = yMax;
			y = value;
			height = num - y;
		}
	}

	public int xMax {
		readonly get => Math.Max(x, x + width);
		set {
			width = value - x;
		}
	}

	public int yMax {
		readonly get => Math.Max(y, y + height);
		set {
			height = value - y;
		}
	}

	/// <summary>
	/// Always return (x, y) no matter size is positive of negative
	/// </summary>
	public Int2 position {
		readonly get => new(x, y);

		set {
			x = value.x;
			y = value.y;
		}
	}

	/// <summary>
	/// (width, height)
	/// </summary>
	public Int2 size {
		readonly get => new(width, height);
		set {
			width = value.x;
			height = value.y;
		}
	}

	/// <summary>
	/// Set value of this rectangle with min and max position
	/// </summary>
	public void SetMinMax (Int2 minPosition, Int2 maxPosition) {
		min = minPosition;
		max = maxPosition;
	}

	/// <summary>
	/// Create a rectangle with given min and max positions
	/// </summary>
	public static IRect MinMaxRect (Int2 min, Int2 max) => new() {
		xMin = min.x,
		yMin = min.y,
		xMax = max.x,
		yMax = max.y,
	};
	/// <summary>
	/// Create a rectangle with given min and max positions
	/// </summary>
	public static IRect MinMaxRect (int minX, int minY, int maxX, int maxY) => new() {
		xMin = minX,
		yMin = minY,
		xMax = maxX,
		yMax = maxY,
	};

	public IRect (IRect source) {
		x = source.xMin;
		y = source.yMin;
		width = source.width;
		height = source.height;
	}

	public IRect (int xMin, int yMin, int width, int height) {
		this.x = xMin;
		this.y = yMin;
		this.width = width;
		this.height = height;
	}

	public IRect (Int2 position, Int2 size) {
		x = position.x;
		y = position.y;
		width = size.x;
		height = size.y;
	}

	/// <summary>
	/// True if the given position is inside this rectangle
	/// </summary>
	public readonly bool Contains (Int2 position) {
		return position.x >= xMin && position.y >= yMin && position.x < xMax && position.y < yMax;
	}

	/// <summary>
	/// True if the given rectangle overlap with current one
	/// </summary>
	public readonly bool Overlaps (IRect other) => other.xMin < xMax && other.xMax > xMin && other.yMin < yMax && other.yMax > yMin;

	/// <summary>
	/// Create a rectangle with 1 in width and height
	/// </summary>
	public static IRect Point (Int2 pos) => new(pos.x, pos.y, 1, 1);
	/// <summary>
	/// Create a rectangle with 1 in width and height
	/// </summary>
	public static IRect Point (int x, int y) => new(x, y, 1, 1);

	public override readonly string ToString () {
		return ToString(null, null);
	}

	public readonly string ToString (string format) {
		return ToString(format, null);
	}

	public readonly string ToString (string format, IFormatProvider formatProvider) {
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

	public readonly bool Equals (IRect other) => x == other.x && y == other.y && width == other.width && height == other.height;

}