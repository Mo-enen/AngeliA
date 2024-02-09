using System.Collections;
using System.Collections.Generic;
using System.IO;



namespace AngeliaFramework {
	public static partial class Util {


		public static string GetParentPath (string path) {
			if (string.IsNullOrEmpty(path)) return "";
			var info = Directory.GetParent(path);
			return info != null ? info.FullName : "";
		}


		public static string GetFullPath (string path) => new FileInfo(path).FullName;


		public static string GetFolderFullPath (string path) => new DirectoryInfo(path).FullName;


		public static string CombinePaths (params string[] paths) {
			string path = "";
			for (int i = 0; i < paths.Length; i++) {
				path = Path.Combine(path, paths[i]);
			}
			return path;
		}


		public static string GetExtension (string path) => Path.GetExtension(path);//.txt


		public static string GetNameWithoutExtension (string path) => Path.GetFileNameWithoutExtension(path);


		public static string GetNameWithExtension (string path) => Path.GetFileName(path);


		public static string ChangeExtension (string path, string newEx) => Path.ChangeExtension(path, newEx);//txt or .txt


		public static bool FolderExists (string path) => Directory.Exists(path);


		public static bool FileExists (string path) => !string.IsNullOrEmpty(path) && File.Exists(path);


		public static bool PathIsFolder (string path) => File.GetAttributes(path).HasFlag(FileAttributes.Directory);


		public static bool IsChildPath (string pathA, string pathB) {
			pathA = GetFullPath(pathA);
			pathB = GetFullPath(pathB);
			if (pathA.Length == pathB.Length) {
				return pathA == pathB;
			} else if (pathA.Length > pathB.Length) {
				return IsChildPathComparer(pathA, pathB);
			} else {
				return IsChildPathComparer(pathB, pathA);
			}
		}


		private static bool IsChildPathComparer (string longPath, string path) {
			if (longPath.Length <= path.Length || !PathIsFolder(path) || !longPath.StartsWith(path)) {
				return false;
			}
			char c = longPath[path.Length];
			if (c != Path.DirectorySeparatorChar && c != Path.AltDirectorySeparatorChar) {
				return false;
			}
			return true;
		}


		public static string GetUrl (string path) => string.IsNullOrEmpty(path) ? "" : new System.Uri(path).AbsoluteUri;


		public static string GetTimeString () => System.DateTime.Now.ToString("yyyyMMddHHmmssffff");


		public static long GetLongTime () => System.DateTime.Now.Ticks;


		public static string GetDisplayTimeFromTicks (long ticks) => new System.DateTime(ticks).ToString("yyyy-MM-dd HH:mm");


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


		public static bool IsSamePath (string pathA, string pathB) => FixPath(GetFullPath(pathA)) == FixPath(GetFullPath(pathB));


		public static bool IsFileHidden (string path) => (File.GetAttributes(path) & FileAttributes.Hidden) != 0;


		public static bool IsFolderHidden (string path) => (new DirectoryInfo(path).Attributes & FileAttributes.Hidden) != 0;



	}
}