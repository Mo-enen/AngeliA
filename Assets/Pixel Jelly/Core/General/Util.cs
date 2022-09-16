using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using UnityEngine;
using System.Text;

namespace PixelJelly {


	[System.Flags]
	public enum Direction {
		None = 0,
		Left = 1 << 0,
		Right = 1 << 1,
		Down = 1 << 2,
		Up = 1 << 3,
	}



	public static class Util {


		// Math
		public static bool PointInTriangle (float x, float y, float ax, float ay, float bx, float by, float cx, float cy) {
			var s = ay * cx - ax * cy + (cy - ay) * x + (ax - cx) * y;
			var t = ax * by - ay * bx + (ay - by) * x + (bx - ax) * y;
			if ((s < 0) != (t < 0)) { return false; }
			var A = -by * cx + ay * (cx - bx) + ax * (by - cy) + bx * cy;
			return A < 0 ? (s < 0 && s + t > A) : (s > 0 && s + t < A);
		}
		public static bool PointInTriangle (Vector2 p, Vector2 a, Vector2 b, Vector2 c) => PointInTriangle(p.x, p.y, a.x, a.y, b.x, b.y, c.x, c.y);


		public static void LineIntersection (Vector2 a0, Vector2 a1, Vector2 b0, Vector2 b1, out bool lines_intersect, out bool segments_intersect, out Vector2 intersection, out Vector2 close_p1, out Vector2 close_p2) {

			// Get the segments' parameters.
			float dx12 = a1.x - a0.x;
			float dy12 = a1.y - a0.y;
			float dx34 = b1.x - b0.x;
			float dy34 = b1.y - b0.y;

			// Solve for t1 and t2
			float denominator = (dy12 * dx34 - dx12 * dy34);

			float t1 =
				((a0.x - b0.x) * dy34 + (b0.y - a0.y) * dx34)
					/ denominator;
			if (float.IsInfinity(t1)) {
				// The lines are parallel (or close enough to it).
				lines_intersect = false;
				segments_intersect = false;
				intersection = new Vector2(float.NaN, float.NaN);
				close_p1 = new Vector2(float.NaN, float.NaN);
				close_p2 = new Vector2(float.NaN, float.NaN);
				return;
			}
			lines_intersect = true;

			float t2 =
				((b0.x - a0.x) * dy12 + (a0.y - b0.y) * dx12)
					/ -denominator;

			// Find the point of intersection.
			intersection = new Vector2(a0.x + dx12 * t1, a0.y + dy12 * t1);

			// The segments intersect if t1 and t2 are between 0 and 1.
			segments_intersect = t1 >= 0f && t1 <= 1f && t2 >= 0f && t2 <= 1f;

			// Find the closest points on the segments.
			if (t1 < 0) {
				t1 = 0;
			} else if (t1 > 1) {
				t1 = 1;
			}

			if (t2 < 0) {
				t2 = 0;
			} else if (t2 > 1) {
				t2 = 1;
			}

			close_p1 = new Vector2(a0.x + dx12 * t1, a0.y + dy12 * t1);
			close_p2 = new Vector2(b0.x + dx34 * t2, b0.y + dy34 * t2);
		}
		public static bool LineIntersection (Vector2 a0, Vector2 a1, Vector2 b0, Vector2 b1, out Vector2 intersection) {

			// Get the segments' parameters.
			float dx12 = a1.x - a0.x;
			float dy12 = a1.y - a0.y;
			float dx34 = b1.x - b0.x;
			float dy34 = b1.y - b0.y;

			// Solve for t1 and t2
			float denominator = (dy12 * dx34 - dx12 * dy34);

			float t1 =
				((a0.x - b0.x) * dy34 + (b0.y - a0.y) * dx34)
					/ denominator;
			if (float.IsInfinity(t1)) {
				// The lines are parallel (or close enough to it).
				intersection = new Vector2(float.NaN, float.NaN);
				return false;
			}

			// Find the point of intersection.
			intersection = new Vector2(a0.x + dx12 * t1, a0.y + dy12 * t1);

			return true;
		}


		public static float PointLineDistance (Vector2 point, Vector2 linePointA, Vector2 linePointB, bool isSegment = false) {
			float dist = CrossProduct(linePointA, linePointB, point) / Vector2.Distance(linePointA, linePointB);
			if (isSegment) {
				double dot1 = DotProduct(linePointA, linePointB, point);
				if (dot1 > 0)
					return Vector2.Distance(linePointB, point);

				double dot2 = DotProduct(linePointB, linePointA, point);
				if (dot2 > 0)
					return Vector2.Distance(linePointA, point);
			}
			return Mathf.Abs(dist);
			// Func
			float CrossProduct (Vector2 pointA, Vector2 pointB, Vector2 pointC) {
				var AB = pointB - pointA;
				var AC = pointC - pointA;
				return AB.x * AC.y - AB.y * AC.x;
			}
			float DotProduct (Vector2 pointA, Vector2 pointB, Vector2 pointC) {
				var AB = pointB - pointA;
				var BC = pointC - pointB;
				return AB.x * BC.x + AB.y * BC.y;
			}
		}


		public static float RemapUnclamped (float fromL, float fromR, float toL, float toR, float value) => fromL == fromR ? toL : Mathf.LerpUnclamped(toL, toR, (value - fromL) / (fromR - fromL));
		public static float Remap (float fromL, float fromR, float toL, float toR, float value) => fromL == fromR ? toL : Mathf.Lerp(toL, toR, (value - fromL) / (fromR - fromL));
		public static int RemapRounded (float fromL, float fromR, float toL, float toR, float value) => Mathf.RoundToInt(Remap(fromL, fromR, toL, toR, value));
		public static int RemapFloor (float fromL, float fromR, float toL, float toR, float value) => Mathf.FloorToInt(Remap(fromL, fromR, toL, toR, value));
		public static int RemapUnclampedRounded (float fromL, float fromR, float toL, float toR, float value) => Mathf.RoundToInt(RemapUnclamped(fromL, fromR, toL, toR, value));


		public static Vector2 GetBezierPoint (Vector2 p0, Vector2 p1, Vector2 p2, float t) {
			t = Mathf.Clamp01(t);
			float oneMinusT = 1f - t;
			return
				oneMinusT * oneMinusT * p0 +
				2f * oneMinusT * t * p1 +
				t * t * p2;
		}
		public static Vector2 GetBezierPoint (Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t) {
			t = Mathf.Clamp01(t);
			float oneMinusT = 1f - t;
			return
				oneMinusT * oneMinusT * oneMinusT * p0 +
				3f * oneMinusT * oneMinusT * t * p1 +
				3f * oneMinusT * t * t * p2 +
				t * t * t * p3;
		}
		public static Vector2 GetBezierVelocity (Vector2 p0, Vector2 p1, Vector2 p2, float t) {
			return
				2f * (1f - t) * (p1 - p0) +
				2f * t * (p2 - p1);
		}
		public static Vector2 GetBezierVelocity (Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t) {
			t = Mathf.Clamp01(t);
			float oneMinusT = 1f - t;
			return
				3f * oneMinusT * oneMinusT * (p1 - p0) +
				6f * oneMinusT * t * (p2 - p1) +
				3f * t * t * (p3 - p2);
		}


		public static Color32 Blend_OneMinusAlpha (Color32 back, Color32 front) {
			if (front.a < 255) {
				Color bg = back;
				Color fg = front;
				float a = 1f - (1f - bg.a) * (1f - fg.a);
				return new Color(
					fg.r * fg.a / a + bg.r * bg.a * (1f - fg.a) / a,
					fg.g * fg.a / a + bg.g * bg.a * (1f - fg.a) / a,
					fg.b * fg.a / a + bg.b * bg.a * (1f - fg.a) / a,
					a
				);
			} else {
				return front;
			}
		}
		public static Color32 Blend_Additive (Color32 back, Color32 front) {
			Color bg = back;
			Color fg = front;
			Color.RGBToHSV(fg, out _, out _, out float v);
			return Color.Lerp(bg, bg + fg, v * fg.a);
		}


		public static T[,] GetArray2D<T> (T[] array, int width, int height) {
			if (width <= 0 || height <= 0 || array.Length != width * height) { return null; }
			var result = new T[width, height];
			for (int j = 0; j < height; j++) {
				for (int i = 0; i < width; i++) {
					result[i, j] = array[j * width + i];
				}
			}
			return result;
		}
		public static T[] GetArray1D<T> (T[,] array) {
			int width = array.GetLength(0);
			var result = new T[width * array.GetLength(1)];
			for (int i = 0; i < result.Length; i++) {
				result[i] = array[i % width, i / width];
			}
			return result;
		}


		public static bool AlmostZero (this float value) => Mathf.Approximately(value, 0f);
		public static bool Almost (this float value, float target) => Mathf.Approximately(value, target);
		public static bool NotAlmostZero (this float value) => !Mathf.Approximately(value, 0f);
		public static bool NotAlmost (this float value, float target) => !Mathf.Approximately(value, target);
		public static bool GreaterThan (this float value, float target) => value.NotAlmost(target) && value > target;
		public static bool LessThan (this float value, float target) => value.NotAlmost(target) && value < target;
		public static bool GreaterThanZero (this float value) => value.GreaterThan(0f);
		public static bool LessThanZero (this float value) => value.LessThan(0f);


		public static int Sign (this int value, int signOfZero = 0) => value > 0 ? 1 : value < 0 ? -1 : signOfZero;


		public static Rect Expand (this Rect rect, float offset) => rect.Expand(offset, offset, offset, offset);
		public static Rect Expand (this Rect rect, float l, float r, float d, float u) {
			rect.x -= l;
			rect.y -= d;
			rect.width += l + r;
			rect.height += d + u;
			return rect;
		}
		public static Rect Fit (this Rect rect, float targetAspect) {
			var bgSize = GetFitInSize(rect.width, rect.height, targetAspect);
			return new Rect(rect.x + Mathf.Abs(rect.width - bgSize.x) / 2f, rect.y + Mathf.Abs(rect.height - bgSize.y) / 2f, bgSize.x, bgSize.y);
			Vector2 GetFitInSize (float boxX, float boxY, float aspect) =>
				aspect > boxX / boxY ? new Vector2(boxX, boxX / aspect) : new Vector2(boxY * aspect, boxY);
		}


		public static RectInt Expand (this RectInt rect, int offset) => rect.Expand(offset, offset, offset, offset);
		public static RectInt Expand (this RectInt rect, int l, int r, int d, int u) {
			rect.x -= l;
			rect.y -= d;
			rect.width += l + r;
			rect.height += d + u;
			return rect;
		}


		public static void Clamp (this ref Rect rect, Rect target) => rect = Rect.MinMaxRect(
			Mathf.Max(rect.xMin, target.xMin),
			Mathf.Max(rect.yMin, target.yMin),
			Mathf.Min(rect.xMax, target.xMax),
			Mathf.Min(rect.yMax, target.yMax)
		);
		public static void Clamp (this ref RectInt rect, RectInt target) => rect.SetMinMax(
			Vector2Int.Max(rect.min, target.min),
			Vector2Int.Min(rect.max, target.max)
		);


		public static bool Contains (this RectInt rect, int x, int y) => rect.Contains(new Vector2Int(x, y));


		public static bool IsSame (this Color32 a, Color32 b, bool ignoreAlpha = false) => a.r == b.r && a.g == b.g && a.b == b.b && (ignoreAlpha || a.a == b.a);


		public static Color32 SetAlpha (this Color32 color, float alpha) => color.SetAlpha((byte)Mathf.Clamp(alpha * 255f, 0, 255));
		public static Color32 SetAlpha (this Color32 color, byte alpha) {
			color.a = alpha;
			return color;
		}


		public static void TryAdd<U, V> (this Dictionary<U, V> map, U key, V value) {
			if (!map.ContainsKey(key)) {
				map.Add(key, value);
			}
		}
		public static V GetOrAdd<U, V> (this Dictionary<U, V> map, U key, V defaultValue) {
			if (!map.ContainsKey(key)) {
				map.Add(key, defaultValue);
			}
			return map[key];
		}
		public static V SetOrAdd<U, V> (this Dictionary<U, V> map, U key, V value) {
			if (!map.ContainsKey(key)) {
				map.Add(key, value);
			} else {
				map[key] = value;
			}
			return map[key];
		}

		public static void TryAdd<T> (this HashSet<T> hash, T value) {
			if (!hash.Contains(value)) {
				hash.Add(value);
			}
		}


		public static float NextFloat (this System.Random random) => (float)random.NextDouble();
		public static float NextFloat (this System.Random random, float min, float max) => Mathf.Lerp(min, max, (float)random.NextDouble());


		public static int FloorToInt (this float value) => Mathf.FloorToInt(value);
		public static int RoundToInt (this float value) => Mathf.RoundToInt(value);
		public static int CeilToInt (this float value) => Mathf.CeilToInt(value);


		public static Vector2Int FloorToInt (this Vector2 value) => new Vector2Int((int)value.x, (int)value.y);
		public static Vector2Int RoundToInt (this Vector2 value) => new Vector2Int(Mathf.RoundToInt(value.x), Mathf.RoundToInt(value.y));
		public static void Clamp (this ref Vector2 value, Vector2 min, Vector2 max) {
			value.x = Mathf.Clamp(value.x, min.x, max.x);
			value.y = Mathf.Clamp(value.y, min.y, max.y);
		}
		public static Vector2 Clamped (this Vector2 value, Vector2 min, Vector2 max) {
			value.x = Mathf.Clamp(value.x, min.x, max.x);
			value.y = Mathf.Clamp(value.y, min.y, max.y);
			return value;
		}


		public static float RandomBetween (this Vector2 value, System.Random random) => Mathf.Lerp(value.x, value.y, random.NextFloat());
		public static int RandomBetween (this Vector2Int value, System.Random random) => random.Next(value.x, value.y + 1);
		public static T RandomBetween<T> (this T[] arr, System.Random random) where T : class => arr.Length > 0 ? arr[random.Next(0, arr.Length)] : null;

		public static int NextClamped (this System.Random random, int min, int max) => random.Next(min, Mathf.Max(min, max));


		public static T MushroomPick<T> (this T[] source, int itemIndex, int itemCount) {
			return source.Length > 0 ? source[
				(itemIndex / Mathf.Max(itemCount / source.Length, 1)) % source.Length
			] : default;
		}


		public static uint ToUInt (this Color32 color) =>
			256u * 256u * 256u * color.r +
			256u * 256u * color.g +
			256u * color.b +
			color.a;


		public static Color32 ToColor32 (this uint value) => new Color32(
			(byte)((value / 256 / 256 / 256) % 256),
			(byte)((value / 256 / 256) % 256),
			(byte)((value / 256) % 256),
			(byte)(value % 256)
		);


		public static Rect GetMatrixRange (this Rect rect, Matrix4x4 matrix) {
			var p0 = matrix.MultiplyPoint3x4(new Vector2(rect.xMin, rect.yMin));
			var p1 = matrix.MultiplyPoint3x4(new Vector2(rect.xMin, rect.yMax));
			var p2 = matrix.MultiplyPoint3x4(new Vector2(rect.xMax, rect.yMax));
			var p3 = matrix.MultiplyPoint3x4(new Vector2(rect.xMax, rect.yMin));
			return Rect.MinMaxRect(
				Mathf.Min(Mathf.Min(p0.x, p1.x), Mathf.Min(p2.x, p3.x)),
				Mathf.Min(Mathf.Min(p0.y, p1.y), Mathf.Min(p2.y, p3.y)),
				Mathf.Max(Mathf.Max(p0.x, p1.x), Mathf.Max(p2.x, p3.x)),
				Mathf.Max(Mathf.Max(p0.y, p1.y), Mathf.Max(p2.y, p3.y))
			);
		}


		// File
		public static void TextToFile (string data, string path) {
			FileStream fs = new FileStream(path, FileMode.Create);
			StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
			sw.Write(data);
			sw.Close();
			fs.Close();
		}


		public static string FileToText (string path) {
			StreamReader sr = File.OpenText(path);
			string data = sr.ReadToEnd();
			sr.Close();
			return data;
		}


		public static byte[] FileToByte (string path) {
			byte[] bytes = null;
			if (FileExists(path)) {
				bytes = File.ReadAllBytes(path);
			}
			return bytes;
		}


		public static void ByteToFile (byte[] bytes, string path) {
			string parentPath = GetParentPath(path);
			CreateFolder(parentPath);
			FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
			fs.Write(bytes, 0, bytes.Length);
			fs.Close();
			fs.Dispose();
		}


		public static void CreateFolder (string path) {
			if (!string.IsNullOrEmpty(path) && !FolderExists(path)) {
				string pPath = GetParentPath(path);
				if (!FolderExists(pPath)) {
					CreateFolder(pPath);
				}
				Directory.CreateDirectory(path);
			}
		}


		public static FileInfo[] GetFilesIn (string path, bool topOnly, params string[] searchPattern) {
			var allFiles = new List<FileInfo>();
			if (PathIsDirectory(path)) {
				var option = topOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
				if (searchPattern.Length == 0) {
					allFiles.AddRange(new DirectoryInfo(path).GetFiles("*", option));
				} else {
					for (int i = 0; i < searchPattern.Length; i++) {
						allFiles.AddRange(new DirectoryInfo(path).GetFiles(searchPattern[i], option));
					}
				}
			}
			return allFiles.ToArray();
		}


		public static DirectoryInfo[] GetFoldersIn (string path, bool topOnly) {
			var allDirs = new List<DirectoryInfo>();
			if (PathIsDirectory(path)) {
				allDirs.AddRange(new DirectoryInfo(path).GetDirectories("*", topOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories));
			}
			return allDirs.ToArray();
		}


		public static void DeleteFile (string path) {
			if (FileExists(path)) {
				File.Delete(path);
			}
		}


		public static void DeleteDirectory (string path) {
			if (FolderExists(path)) {
				Directory.Delete(path, true);
			}
		}



		public static IEnumerable<string> EnumerateFiles (string path, string search = "*", SearchOption option = SearchOption.TopDirectoryOnly) => Directory.EnumerateFiles(path, search, option);


		public static int GetFileCount (string path, string search = "*", SearchOption option = SearchOption.TopDirectoryOnly) {
			if (FolderExists(path)) {
				return Directory.EnumerateFiles(path, search, option).Count();
			}
			return 0;
		}


		public static void CopyFile (string from, string to) {
			if (FileExists(from)) {
				CreateFolder(GetParentPath(to));
				File.Copy(from, to, true);
			}
		}


		// Path
		public static string SanitizeFileName (string origFileName) => string.Join(
			"_", origFileName.Split(
				Path.GetInvalidFileNameChars(),
				System.StringSplitOptions.RemoveEmptyEntries)
			).TrimEnd('.');


		public static string GetParentPath (string path) => Directory.GetParent(path).FullName;


		public static string GetFullPath (string path) => new FileInfo(path).FullName;


		public static string CombinePaths (params string[] paths) {
			string path = "";
			for (int i = 0; i < paths.Length; i++) {
				path = Path.Combine(path, paths[i]);
			}
			return path;
		}


		public static string GetNameWithoutExtension (string path) => Path.GetFileNameWithoutExtension(path);
		public static string GetNameWithExtension (string path) => Path.GetFileName(path);
		public static string ChangeExtension (string path, string ext) => Path.ChangeExtension(path, ext);


		public static bool FolderExists (string path) => Directory.Exists(path);


		public static bool FileExists (string path) => !string.IsNullOrEmpty(path) && File.Exists(path);


		public static void MoveFile (string from, string to) {
			if (from != to && FileExists(from)) {
				try {
					CreateFolder(GetParentPath(to));
					File.Move(from, to);
				} catch { }
			}
		}


		public static bool PathIsDirectory (string path) => File.GetAttributes(path).HasFlag(FileAttributes.Directory);


		public static string FixPath (string path, bool forUnity = true) {
			char dsChar = forUnity ? '/' : Path.DirectorySeparatorChar;
			char adsChar = forUnity ? '\\' : Path.AltDirectorySeparatorChar;
			path = path.Replace(adsChar, dsChar);
			path = path.Replace(new string(dsChar, 2), dsChar.ToString());
			while (path.Length > 0 && path[0] == dsChar) {
				path = path.Remove(0, 1);
			}
			while (path.Length > 0 && path[path.Length - 1] == dsChar) {
				path = path.Remove(path.Length - 1, 1);
			}
			return path;
		}


		// Ref
		public static void InvokeMethod<T> (T obj, string methodName, params object[] param) {
			if (obj == null || string.IsNullOrEmpty(methodName)) { return; }
			try {
				obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(obj, param);
			} catch (System.Exception ex) { Debug.LogError(ex); }
		}


	}
}