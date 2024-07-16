using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;


namespace AngeliA;
public static partial class Util {


	public static bool IsPowerOfTwo (int x) => (x != 0) && (x & (x - 1)) == 0;

	public static int SquareDistance (Int2 a, Int2 b) {
		int x = a.x - b.x;
		int y = a.y - b.y;
		return x * x + y * y;
	}
	public static int SquareDistance (int aX, int aY, int bX, int bY) {
		int x = aX - bX;
		int y = aY - bY;
		return x * x + y * y;
	}


	public static int DistanceInt (Int2 a, Int2 b) {
		int x = a.x - b.x;
		int y = a.y - b.y;
		return BabylonianSqrt(x * x + y * y);
	}
	public static int DistanceInt (int aX, int aY, int bX, int bY) {
		int x = aX - bX;
		int y = aY - bY;
		return BabylonianSqrt(x * x + y * y);
	}


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


	public static float PointLine_Distance (Float2 point, Float2 a, Float2 b) {
		if (a == b) { return Float2.Distance(point, a); }
		float x0 = point.x;
		float x1 = a.x;
		float x2 = b.x;
		float y0 = point.y;
		float y1 = a.y;
		float y2 = b.y;
		return Abs(
			(x2 - x1) * (y1 - y0) - (x1 - x0) * (y2 - y1)
		) / Sqrt(
			(x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1)
		);
	}


	public static bool Intersect_SegmentSegment (Int2 a0, Int2 a1, Int2 b0, Int2 b1, out Int2 intersection) {
		intersection = default;

		var qp = b0 - a0;
		var r = a1 - a0;
		var s = b1 - b0;
		var rs = Cross(r, s);
		var qpr = Cross(qp, r);
		var qps = Cross(qp, s);

		if (rs == 0 && qpr == 0) return false;
		if (rs == 0 && qpr != 0) return false;
		if (
			rs != 0 &&
			0f <= qps * rs && (rs > 0 ? qps <= rs : qps >= rs) &&
			0f <= qpr * rs && (rs > 0 ? qpr <= rs : qpr >= rs)
		) {
			intersection = a0 + qps * r / rs;
			return true;
		}

		return false;

		static int Cross (Int2 a, Int2 b) => a.x * b.y - a.y * b.x;
	}


	public static int Intersect_SegmentRect (Int2 a0, Int2 a1, IRect rect, out Int2 intersection0, out Int2 intersection1) => Intersect_SegmentRect(a0, a1, rect, out intersection0, out intersection1, out _, out _);


	public static int Intersect_SegmentRect (Int2 a0, Int2 a1, IRect rect, out Int2 intersection0, out Int2 intersection1, out Int2 normalDirection0, out Int2 normalDirection1) {
		intersection0 = default;
		intersection1 = default;
		int interCount = 0;
		normalDirection0 = default;
		normalDirection1 = default;

		// U
		if (Intersect_SegmentSegment(
			a0, a1, new Int2(rect.xMin, rect.yMax), new Int2(rect.xMax, rect.yMax),
			out var inter
		)) {
			intersection0 = inter;
			normalDirection0 = Int2.up;
			interCount++;
		}

		// D
		if (Intersect_SegmentSegment(
			a0, a1, new Int2(rect.xMin, rect.yMin), new Int2(rect.xMax, rect.yMin),
			out inter
		)) {
			if (interCount == 0) {
				intersection0 = inter;
				normalDirection0 = Int2.down;
				interCount++;
			} else {
				intersection1 = inter;
				normalDirection1 = Int2.down;
				return 2;
			}
		}

		// L 
		if (Intersect_SegmentSegment(
			a0, a1, new Int2(rect.xMin, rect.yMin), new Int2(rect.xMin, rect.yMax),
			out inter
		)) {
			if (interCount == 0) {
				intersection0 = inter;
				normalDirection0 = Int2.left;
				interCount++;
			} else {
				intersection1 = inter;
				normalDirection1 = Int2.left;
				return 2;
			}
		}

		// R
		if (Intersect_SegmentSegment(
			a0, a1, new Int2(rect.xMax, rect.yMin), new Int2(rect.xMax, rect.yMax),
			out inter
		)) {
			if (interCount == 0) {
				intersection0 = inter;
				normalDirection0 = Int2.right;
				interCount++;
			} else {
				intersection1 = inter;
				normalDirection1 = Int2.right;
				return 2;
			}
		}

		return interCount;
	}


	public static bool OverlapRectCircle (int radius, int circleX, int circleY, int minX, int minY, int maxX, int maxY) {
		int disX = Max(minX, Min(circleX, maxX)) - circleX;
		int disY = Max(minY, Min(circleY, maxY)) - circleY;
		return (disX * disX + disY * disY) <= radius * radius;
	}


	public static Color32 IntToColor (int i) => new((byte)(i >> 24), (byte)(i >> 16), (byte)(i >> 8), (byte)(i));
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


	private static readonly System.Data.DataTable DataTable = new();
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

		static IRect FixRect (IRect rect, bool removeMidX, bool removeMidY, int centerX, int centerY) {
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
			yield return FixRect(new IRect(centerX - radiusX, centerY, 2 * radiusX + 1, 1), removeMidX, removeMidY, centerX, centerY);
			yield break;
		}

		while (y >= 0 && x <= radiusX) {
			if (t + b2 * x <= crit1 ||
				t + a2 * y <= crit3) {
				if (_height == 1) {

				} else if (ry * 2 + 1 > (_height - 1) * 2) {
					yield return FixRect(new IRect(centerX - rx, centerY - ry, _width, _height - 1), removeMidX, removeMidY, centerX, centerY);
					yield return FixRect(new IRect(centerX - rx, centerY + ry + 1, _width, 1 - _height), removeMidX, removeMidY, centerX, centerY);
					ry -= _height - 1;
					_height = 1;
				} else {
					yield return FixRect(new IRect(centerX - rx, centerY - ry, _width, ry * 2 + 1), removeMidX, removeMidY, centerX, centerY);
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
					yield return FixRect(new IRect(centerX - rx, centerY - ry, _width, _height), removeMidX, removeMidY, centerX, centerY);
					yield return FixRect(new IRect(centerX - rx, centerY + ry + 1, _width, -_height), removeMidX, removeMidY, centerX, centerY);
				} else {
					yield return FixRect(new IRect(centerX - rx, centerY - ry, _width, ry * 2 + 1), removeMidX, removeMidY, centerX, centerY);
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
			yield return FixRect(new IRect(centerX - rx, centerY - ry, _width, _height), removeMidX, removeMidY, centerX, centerY);
			yield return FixRect(new IRect(centerX - rx, centerY + ry + 1, _width, -_height), removeMidX, removeMidY, centerX, centerY);
		} else {
			yield return FixRect(new IRect(centerX - rx, centerY - ry, _width, ry * 2 + 1), removeMidX, removeMidY, centerX, centerY);
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