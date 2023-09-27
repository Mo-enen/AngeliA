using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace AngeliaFramework.Editor {


	public struct AseUtil {



		#region --- File ---



		public static string Read (string path) {
			path = FixPath(path, false);
			StreamReader sr = File.OpenText(path);
			string data = sr.ReadToEnd();
			sr.Close();
			return data;
		}



		public static void CreateFolder (string path) {
			if (!string.IsNullOrEmpty(path) && !DirectoryExists(path)) {
				string pPath = GetParentPath(path);
				if (!DirectoryExists(pPath)) {
					CreateFolder(pPath);
				}
				path = FixPath(path, false);
				Directory.CreateDirectory(path);
			}
		}



		public static byte[] FileToByte (string path) {
			byte[] bytes = null;
			if (FileExists(path)) {
				path = FixPath(path, false);
				bytes = File.ReadAllBytes(path);
			}
			return bytes;
		}



		public static void ByteToFile (byte[] bytes, string path) {
			CreateFolder(Directory.GetParent(path).FullName);
			path = FixPath(path, false);
			FileStream fs = new(path, FileMode.Create, FileAccess.Write);
			fs.Write(bytes, 0, bytes.Length);
			fs.Close();
			fs.Dispose();
		}



		public static FileInfo[] GetFilesIn (string path, params string[] searchPattern) {
			List<FileInfo> allFiles = new();
			path = FixPath(path, false);
			if (PathIsDirectory(path)) {
				if (searchPattern.Length <= 0) {
					allFiles.AddRange(new DirectoryInfo(path).GetFiles("*.*", SearchOption.AllDirectories));
				} else {
					for (int i = 0; i < searchPattern.Length; i++) {
						allFiles.AddRange(new DirectoryInfo(path).GetFiles(searchPattern[i], SearchOption.AllDirectories));
					}
				}
			}
			return allFiles.ToArray();
		}



		public static void DeleteFile (string path) {
			if (FileExists(path)) {
				path = FixPath(path, false);
				File.Delete(path);
			}
		}



		#endregion



		#region --- Path ---


		public static string FixPath (string path, bool forUnity = true) {
			char dsChar = forUnity ? '/' : Path.DirectorySeparatorChar;
			char adsChar = forUnity ? '\\' : Path.AltDirectorySeparatorChar;
			path = path.Replace(adsChar, dsChar);
			path = path.Replace(new string(dsChar, 2), dsChar.ToString());
			while (path.Length > 0 && path[0] == dsChar) {
				path = path.Remove(0, 1);
			}
			while (path.Length > 0 && path[^1] == dsChar) {
				path = path.Remove(path.Length - 1, 1);
			}
			return path;
		}



		public static string GetParentPath (string path) {
			path = FixPath(path, false);
			return FixedRelativePath(Directory.GetParent(path).FullName);
		}




		public static string FixedRelativePath (string path) {
			path = GetFullPath(path);
			path = FixPath(path);
			if (path.StartsWith("Assets")) {
				return path;
			}
			var fixedDataPath = FixPath(Application.dataPath);
			if (path.StartsWith(fixedDataPath)) {
				return "Assets" + path[fixedDataPath.Length..];
			} else {
				return "";
			}
		}




		public static string GetFullPath (string path) {
			path = FixPath(path, false);
			return new FileInfo(path).FullName;
		}



		public static string CombinePaths (params string[] paths) {
			string path = "";
			for (int i = 0; i < paths.Length; i++) {
				path = Path.Combine(path, paths[i]);
			}
			return path;
		}



		public static string GetExtension (string path) {
			return Path.GetExtension(path);//.txt
		}



		public static string GetNameWithoutExtension (string path) {
			return Path.GetFileNameWithoutExtension(path);
		}


		public static string GetNameWithExtension (string path) {
			return Path.GetFileName(path);
		}



		public static bool DirectoryExists (string path) {
			path = FixPath(path, false);
			return Directory.Exists(path);
		}



		public static bool FileExists (string path) {
			path = FixPath(path, false);
			return File.Exists(path);
		}



		public static bool PathIsDirectory (string path) {
			if (!DirectoryExists(path)) { return false; }
			path = FixPath(path, false);
			FileAttributes attr = File.GetAttributes(path);
			if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
				return true;
			else
				return false;
		}



		public static bool IsChildPathComparer (string longPath, string path) {
			if (longPath.Length <= path.Length || !PathIsDirectory(path) || !longPath.StartsWith(path)) {
				return false;
			}
			char c = longPath[path.Length];
			if (c != Path.DirectorySeparatorChar && c != Path.AltDirectorySeparatorChar) {
				return false;
			}
			return true;
		}



		#endregion




		#region --- Misc ---


		public static bool GetTrimOffset (Color32[] pixels, int width, int height, out int bottom, out int top, out int left, out int right) {
			return GetTrimOffset(pixels, width, height, out bottom, out top, out left, out right, 0, 0, width - 1, height - 1);
		}


		public static bool GetTrimOffset (Color32[] pixels, int width, int height, out int bottom, out int top, out int left, out int right, int minX, int minY, int maxX, int maxY) {
			bottom = -1;
			top = -1;
			left = -1;
			right = -1;
			minX = Mathf.Clamp(minX, 0, width - 1);
			minY = Mathf.Clamp(minY, 0, height - 1);
			maxX = Mathf.Clamp(maxX, minX, width - 1);
			maxY = Mathf.Clamp(maxY, minY, height - 1);
			for (int y = minY; y <= maxY && bottom < 0; y++) {
				for (int x = minX; x <= maxX; x++) {
					if (pixels[y * width + x].a > 0.001f) {
						bottom = y;
						break;
					}
				}
			}
			for (int y = Mathf.Min(height - 1, maxY); y >= minY && top < 0; y--) {
				for (int x = minX; x <= maxX; x++) {
					if (pixels[y * width + x].a > 0.001f) {
						top = y;
						break;
					}
				}
			}
			for (int x = minX; x <= maxX && left < minX; x++) {
				for (int y = minY; y <= maxY; y++) {
					if (pixels[y * width + x].a > 0.001f) {
						left = x;
						break;
					}
				}
			}
			for (int x = Mathf.Min(width - 1, maxX); x >= minX && right < 0; x--) {
				for (int y = minY; y <= maxY; y++) {
					if (pixels[y * width + x].a > 0.001f) {
						right = x;
						break;
					}
				}
			}
			if (bottom < 0) { bottom = minY; }
			if (top < 0) { top = maxY; }
			if (left < 0) { left = minX; }
			if (right < 0) { right = maxX; }
			return bottom < top && left < right && (bottom > minY || top < maxY || left > minX || right < maxX);
		}



		#endregion



	}


}
