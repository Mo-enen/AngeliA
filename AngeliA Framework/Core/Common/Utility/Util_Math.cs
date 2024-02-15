using System.Collections;
using System.Collections.Generic;


namespace AngeliA; 
public static partial class Util {


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


}