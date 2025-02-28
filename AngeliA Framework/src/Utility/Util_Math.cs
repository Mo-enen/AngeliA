using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AngeliA;

public static partial class Util {


	private static readonly System.Data.DataTable DataTable = new();


	public static int GreatestCommonFactor (int a, int b) {
		while (b != 0) {
			int temp = b;
			b = a % b;
			a = temp;
		}
		return a;
	}


	public static int LeastCommonMultiple (int a, int b) => (a / GreatestCommonFactor(a, b)) * b;


	public static Int2 RotateAround (int x, int y, int rotation, int originX, int originY) {
		var v = new Float2(x - originX, y - originY).Rotate(rotation);
		return new(originX + v.x.RoundToInt(), originY + v.y.RoundToInt());
	}


	[MethodImpl(INLINE)]
	public static bool IsPowerOfTwo (int x) => (x != 0) && (x & (x - 1)) == 0;


	[MethodImpl(INLINE)]
	public static float SquareDistanceF (float aX, float aY, float bX, float bY) {
		float x = aX - bX;
		float y = aY - bY;
		return x * x + y * y;
	}

	[MethodImpl(INLINE)]
	public static int SquareDistance (Int2 a, Int2 b) {
		int x = a.x - b.x;
		int y = a.y - b.y;
		return x * x + y * y;
	}

	[MethodImpl(INLINE)]
	public static int SquareDistance (int aX, int aY, int bX, int bY) {
		int x = aX - bX;
		int y = aY - bY;
		return x * x + y * y;
	}


	[MethodImpl(INLINE)]
	public static float DistanceFloat (Float2 a, Float2 b) {
		float x = a.x - b.x;
		float y = a.y - b.y;
		return Sqrt(x * x + y * y);
	}

	[MethodImpl(INLINE)]
	public static float DistanceFloat (float aX, float aY, float bX, float bY) {
		float x = aX - bX;
		float y = aY - bY;
		return Sqrt(x * x + y * y);
	}


	[MethodImpl(INLINE)]
	public static int DistanceInt (Int2 a, Int2 b) {
		int x = a.x - b.x;
		int y = a.y - b.y;
		return BabylonianSqrt(x * x + y * y);
	}

	[MethodImpl(INLINE)]
	public static int DistanceInt (int aX, int aY, int bX, int bY) {
		int x = aX - bX;
		int y = aY - bY;
		return BabylonianSqrt(x * x + y * y);
	}


	[MethodImpl(INLINE)]
	public static int BabylonianSqrt (int n) {
		int x = n;
		int y = 1;
		while (x > y) {
			x = (x + y) / 2;
			y = n / x;
		}
		return x;
	}


	public static bool PointInTriangle (Float2 p, Float2 a, Float2 b, Float2 c) => PointInTriangle(p.x, p.y, a.x, a.y, b.x, b.y, c.x, c.y);

	public static bool PointInTriangle (float px, float py, float p0x, float p0y, float p1x, float p1y, float p2x, float p2y) {
		var s = p0y * p2x - p0x * p2y + (p2y - p0y) * px + (p0x - p2x) * py;
		var t = p0x * p1y - p0y * p1x + (p0y - p1y) * px + (p1x - p0x) * py;
		if ((s < 0) != (t < 0)) { return false; }
		var A = -p1y * p2x + p0y * (p2x - p1x) + p0x * (p1y - p2y) + p1x * p2y;
		return A < 0 ? (s <= 0 && s + t >= A) : (s >= 0 && s + t <= A);
	}


	public static float PointLine_Distance (Float2 pt, Float2 p1, Float2 p2, out Float2 closest) {
		float dx = p2.x - p1.x;
		float dy = p2.y - p1.y;
		if ((dx == 0) && (dy == 0)) {
			// It's a point not a line segment.
			closest = p1;
			dx = pt.x - p1.x;
			dy = pt.y - p1.y;
			return (float)Math.Sqrt(dx * dx + dy * dy);
		}

		// Calculate the t that minimizes the distance.
		float t = ((pt.x - p1.x) * dx + (pt.y - p1.y) * dy) /
			(dx * dx + dy * dy);

		// See if this represents one of the segment's
		// end points or a point in the middle.
		if (t < 0) {
			closest = new Float2(p1.x, p1.y);
			dx = pt.x - p1.x;
			dy = pt.y - p1.y;
		} else if (t > 1) {
			closest = new Float2(p2.x, p2.y);
			dx = pt.x - p2.x;
			dy = pt.y - p2.y;
		} else {
			closest = new Float2(p1.x + t * dx, p1.y + t * dy);
			dx = pt.x - closest.x;
			dy = pt.y - closest.y;
		}

		return (float)Math.Sqrt(dx * dx + dy * dy);
	}


	public static Float2? SegmentIntersect (Float2 a1, Float2 a2, Float2 b1, Float2 b2) {
		Float2 b = a2 - a1;
		Float2 d = b2 - b1;
		float bDotDPerp = b.x * d.y - b.y * d.x;

		// if b dot d == 0, it means the lines are parallel so have infinite intersection points
		if (bDotDPerp == 0)
			return null;

		Float2 c = b1 - a1;
		var t = (c.x * d.y - c.y * d.x) / bDotDPerp;
		if (t < 0f || t > 1f) {
			return null;
		}

		var u = (c.x * b.y - c.y * b.x) / bDotDPerp;
		if (u < 0f || u > 1f) {
			return null;
		}

		return a1 + t * b;
	}


	public static (Float2? intersect0, Float2? intersect1) SegmentRectIntersect (Float2 a0, Float2 a1, FRect rect) {
		Float2? inter0;
		Float2? inter1 = null;
		var tl = rect.TopLeft();
		var tr = rect.TopRight();
		var bl = rect.BottomLeft();
		var br = rect.BottomRight();

		inter0 = SegmentIntersect(a0, a1, tl, tr);

		var inter = SegmentIntersect(a0, a1, bl, br);
		if (inter.HasValue) {
			if (inter0.HasValue) {
				inter1 = inter;
				return (inter0, inter1);
			} else {
				inter0 = inter;
			}
		}

		inter = SegmentIntersect(a0, a1, bl, tl);
		if (inter.HasValue) {
			if (inter0.HasValue) {
				inter1 = inter;
				return (inter0, inter1);
			} else {
				inter0 = inter;
			}
		}

		inter = SegmentIntersect(a0, a1, br, tr);
		if (inter.HasValue) {
			if (inter0.HasValue) {
				inter1 = inter;
				return (inter0, inter1);
			} else {
				inter0 = inter;
			}
		}

		return (inter0, inter1);
	}


	public static bool OverlapRectCircle (int radius, int circleX, int circleY, int minX, int minY, int maxX, int maxY) {
		int disX = Max(minX, Min(circleX, maxX)) - circleX;
		int disY = Max(minY, Min(circleY, maxY)) - circleY;
		return (disX * disX + disY * disY) <= radius * radius;
	}


	[MethodImpl(INLINE)]
	public static Color32 IntToColor (int i) => new((byte)(i >> 24), (byte)(i >> 16), (byte)(i >> 8), (byte)(i));

	[MethodImpl(INLINE)]
	public static int ColorToInt (Color32 color) => color.r << 24 | color.g << 16 | color.b << 8 | color.a;


	public static string ColorToHtml (Color32 color, bool ignoreAlpha = false) => ignoreAlpha ? $"#{color.r:X2}{color.g:X2}{color.b:X2}" : $"#{color.r:X2}{color.g:X2}{color.b:X2}{color.a:X2}";
	public static bool HtmlToColor (string html, out Color32 color) {
		color = default;
		if (string.IsNullOrEmpty(html)) return false;
		int offset = html[0] == '#' ? 1 : 0;
		if (html.Length != offset + 8 && html.Length != offset + 6) return false;
		try {
			color.r = byte.Parse(html[(offset + 0)..(offset + 2)], System.Globalization.NumberStyles.HexNumber);
			color.g = byte.Parse(html[(offset + 2)..(offset + 4)], System.Globalization.NumberStyles.HexNumber);
			color.b = byte.Parse(html[(offset + 4)..(offset + 6)], System.Globalization.NumberStyles.HexNumber);
			if (html.Length == offset + 8) {
				color.a = byte.Parse(html[(offset + 6)..], System.Globalization.NumberStyles.HexNumber);
			} else {
				color.a = 255;
			}
			return true;
		} catch {
			return false;
		}
	}


	public static bool TryCompute (string expression, out int result) {
		result = 0;
		try {
			if (DataTable.Compute(expression, null) is int intResult) result = intResult;
			return true;
		} catch {
			return false;
		}
	}


	// Line
	public static IEnumerable<Int2> DrawLine_DDA (int x0, int y0, int x1, int y1) {

		// Calculate dx and dy 
		int dx = x1 - x0;
		int dy = y1 - y0;

		int step;

		// If dx > dy we will take step as dx 
		// else we will take step as dy to draw the complete 
		// line 
		if (Math.Abs(dx) > Math.Abs(dy))
			step = Math.Abs(dx);
		else
			step = Math.Abs(dy);

		// Calculate x-increment and y-increment for each 
		// step 
		float x_incr = (float)dx / step;
		float y_incr = (float)dy / step;

		// Take the initial points as x and y 
		float x = x0;
		float y = y0;

		for (int i = 0; i <= step; i++) {
			yield return new(x.RoundToInt(), y.RoundToInt());
			x += x_incr;
			y += y_incr;
		}
	}


	public static IEnumerable<IRect> DrawLineWithRect_DDA (int x0, int y0, int x1, int y1) {
		var endPoint = new Int2(x1, y1);
		var currentRect = new IRect(x0, y0, 1, 1);
		foreach (var point in DrawLine_DDA(x0, y0, x1, y1)) {
			bool difX = point.x != currentRect.x;
			bool difY = point.y != currentRect.y;
			if (difX && difY) {
				// Perform
				yield return currentRect;
				currentRect.x = point.x;
				currentRect.y = point.y;
				currentRect.width = 1;
				currentRect.height = 1;
			} else {
				// Grow
				if (difX) {
					currentRect.width++;
					currentRect.x = Min(currentRect.x, point.x);
				} else if (difY) {
					currentRect.height++;
					currentRect.y = Min(currentRect.y, point.y);
				}
			}
			if (point == endPoint) {
				yield return currentRect;
			}
		}

	}


	// Circle
	public static IEnumerable<IRect> DrawFilledEllipse_Patrick (int left, int bottom, int width, int height) {

		int radiusX = width / 2;
		int radiusY = height / 2;
		int centerX = left + radiusX;
		int centerY = bottom + radiusY;
		bool removeMidX = width % 2 == 0;
		bool removeMidY = height % 2 == 0;

		static IRect FixRect (IRect rect, bool removeMidX, bool removeMidY, int centerY) {
			// Fix X
			if (removeMidX) {
				rect.width--;
			}
			// Fix Y
			if (removeMidY) {
				if (rect.y < centerY && rect.yMax >= centerY) {
					rect.height--;
				} else if (rect.y >= centerY) {
					rect.y--;
				}
			}
			return rect;
		}


		int x = 0, y = radiusY;
		int rx = x, ry = y;
		int _width = 1;
		int _height = 1;
		long a2 = (long)radiusX * radiusX, b2 = (long)radiusY * radiusY;
		long crit1 = -(a2 / 4 + radiusX % 2 + b2);
		long crit2 = -(b2 / 4 + radiusY % 2 + a2);
		long crit3 = -(b2 / 4 + radiusY % 2);
		long t = -a2 * y;
		long dxt = 2 * b2 * x, dyt = -2 * a2 * y;
		long d2xt = 2 * b2, d2yt = 2 * a2;

		if (radiusY == 0) {
			yield return FixRect(new IRect(centerX - radiusX, centerY, 2 * radiusX + 1, 1), removeMidX, removeMidY, centerY);
			yield break;
		}

		while (y >= 0 && x <= radiusX) {
			if (t + b2 * x <= crit1 ||
				t + a2 * y <= crit3) {
				if (_height == 1) {

				} else if (ry * 2 + 1 > (_height - 1) * 2) {
					yield return FixRect(new IRect(centerX - rx, centerY - ry, _width, _height - 1), removeMidX, removeMidY, centerY);
					yield return FixRect(new IRect(centerX - rx, centerY + ry + 1, _width, 1 - _height), removeMidX, removeMidY, centerY);
					ry -= _height - 1;
					_height = 1;
				} else {
					yield return FixRect(new IRect(centerX - rx, centerY - ry, _width, ry * 2 + 1), removeMidX, removeMidY, centerY);
					ry -= ry;
					_height = 1;
				}
				// incX
				x++;
				dxt += d2xt;
				t += dxt;
				// 
				rx++;
				_width += 2;
			} else if (t - a2 * y > crit2) {
				// incY
				y--;
				dyt += d2yt;
				t += dyt;
				// 
				_height++;
			} else {
				if (ry * 2 + 1 > _height * 2) {
					yield return FixRect(new IRect(centerX - rx, centerY - ry, _width, _height), removeMidX, removeMidY, centerY);
					yield return FixRect(new IRect(centerX - rx, centerY + ry + 1, _width, -_height), removeMidX, removeMidY, centerY);
				} else {
					yield return FixRect(new IRect(centerX - rx, centerY - ry, _width, ry * 2 + 1), removeMidX, removeMidY, centerY);
				}
				// incX
				x++;
				dxt += d2xt;
				t += dxt;
				// incY
				y--;
				dyt += d2yt;
				t += dyt;
				// 
				rx++;
				_width += 2;
				ry -= _height;
				_height = 1;
			}
		}

		if (ry > _height) {
			yield return FixRect(new IRect(centerX - rx, centerY - ry, _width, _height), removeMidX, removeMidY, centerY);
			yield return FixRect(new IRect(centerX - rx, centerY + ry + 1, _width, -_height), removeMidX, removeMidY, centerY);
		} else {
			yield return FixRect(new IRect(centerX - rx, centerY - ry, _width, ry * 2 + 1), removeMidX, removeMidY, centerY);
		}

	}


	public static IEnumerable<Int2> DrawHoloEllipse_Patrick (int left, int bottom, int width, int height) {

		int radiusX = width / 2;
		int radiusY = height / 2;
		int centerX = left + radiusX;
		int centerY = bottom + radiusY;
		bool removeMidX = width % 2 == 0;
		bool removeMidY = height % 2 == 0;

		static bool Valid (int x, int y, bool removeMidX, bool removeMidY, int centerX, int centerY) {
			return (!removeMidX || x != centerX) && (!removeMidY || y != centerY);
		}
		static Int2 GetPoint (int x, int y, bool removeMidX, bool removeMidY, int centerX, int centerY) {
			return new Int2(
				removeMidX && x > centerX ? x - 1 : x,
				removeMidY && y > centerY ? y - 1 : y
			);
		}

		int x = 0, y = radiusY;
		long a2 = (long)radiusX * radiusX, b2 = (long)radiusY * radiusY;
		long crit1 = -(a2 / 4 + radiusX % 2 + b2);
		long crit2 = -(b2 / 4 + radiusY % 2 + a2);
		long crit3 = -(b2 / 4 + radiusY % 2);
		long t = -a2 * y;
		long dxt = 2 * b2 * x, dyt = -2 * a2 * y;
		long d2xt = 2 * b2, d2yt = 2 * a2;

		while (y >= 0 && x <= radiusX) {

			if (Valid(centerX + x, centerY + y, removeMidX, removeMidY, centerX, centerY)) {
				yield return GetPoint(centerX + x, centerY + y, removeMidX, removeMidY, centerX, centerY);
			}

			if (x != 0 || y != 0) {
				if (Valid(centerX - x, centerY - y, removeMidX, removeMidY, centerX, centerY)) {
					yield return GetPoint(centerX - x, centerY - y, removeMidX, removeMidY, centerX, centerY);
				}
			}
			if (x != 0 && y != 0) {
				if (Valid(centerX - x, centerY - y, removeMidX, removeMidY, centerX, centerY)) {
					yield return GetPoint(centerX + x, centerY - y, removeMidX, removeMidY, centerX, centerY);
				}
				if (Valid(centerX - x, centerY - y, removeMidX, removeMidY, centerX, centerY)) {
					yield return GetPoint(centerX - x, centerY + y, removeMidX, removeMidY, centerX, centerY);
				}
			}
			if (t + b2 * x <= crit1 || t + a2 * y <= crit3) {
				x++;
				dxt += d2xt;
				t += dxt;
			} else if (t - a2 * y > crit2) {
				y--;
				dyt += d2yt;
				t += dyt;
			} else {
				x++;
				dxt += d2xt;
				t += dxt;
				y--;
				dyt += d2yt;
				t += dyt;
			}
		}
	}


}