using System.Collections;
using System.Collections.Generic;
using System.IO;



namespace AngeliA;
public static partial class Util {


	private static readonly char[] INVALID_PATH_CHARS = Path.GetInvalidPathChars();
	private static readonly char[] INVALID_NAME_CHARS = Path.GetInvalidFileNameChars();


	public static string GetParentPath (string path) {
		if (string.IsNullOrEmpty(path)) return "";
		var info = Directory.GetParent(path);
		return info != null ? info.FullName : "";
	}


	public static string GetFullPath (string path) => new FileInfo(path).FullName;


	public static string GetFolderFullPath (string path) => new DirectoryInfo(path).FullName;


	public static string CombinePaths (string path1, string path2) => Path.Combine(path1, path2);
	public static string CombinePaths (string path1, string path2, string path3) => Path.Combine(path1, path2, path3);
	public static string CombinePaths (string path1, string path2, string path3, string path4) => Path.Combine(path1, path2, path3, path4);
	public static string CombinePaths (params string[] paths) => Path.Combine(paths);


	public static string GetExtensionWithDot (string path) => Path.GetExtension(path);//.txt


	public static string GetNameWithoutExtension (string path) => Path.GetFileNameWithoutExtension(path);


	public static string GetNameWithExtension (string path) => Path.GetFileName(path);


	public static string ChangeExtension (string path, string newEx) => Path.ChangeExtension(path, newEx);//txt or .txt


	public static bool FolderExists (string path) => Directory.Exists(path);


	public static bool FileExists (string path) => !string.IsNullOrEmpty(path) && File.Exists(path);


	public static bool PathIsFolder (string path) => File.GetAttributes(path).HasFlag(FileAttributes.Directory);


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


	public static bool IsSubPathOf (string basePath, string subPath) {
		var rel = Path.GetRelativePath(basePath.Replace('\\', '/'), subPath.Replace('\\', '/'));
		return rel != "." && rel != ".." && !rel.StartsWith("../") && !Path.IsPathRooted(rel);
	}


	public static bool IsFileHidden (string path) => (File.GetAttributes(path) & FileAttributes.Hidden) != 0;


	public static bool IsFolderHidden (string path) => (new DirectoryInfo(path).Attributes & FileAttributes.Hidden) != 0;


	public static bool IsValidForFileName (string content) => content.IndexOfAny(INVALID_PATH_CHARS) < 0;


	public static bool IsPathValid (string path) {
		if (path == null) return false;
		if (!Path.IsPathFullyQualified(path)) return false;
		if (!IsValidForFileName(path)) return false;
		while (!string.IsNullOrEmpty(path)) {
			if (GetNameWithExtension(path).IndexOfAny(INVALID_NAME_CHARS) >= 0) return false;
			path = GetParentPath(path);
		}
		return true;
	}


	public static string Path_to_ArgPath (string path) => $"{path.Replace(" ", "#")}";
	public static string ArgPath_to_Path (string path) => path.Replace("#", " ");


}