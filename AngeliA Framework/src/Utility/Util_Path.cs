using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace AngeliA;

public static partial class Util {


	private static readonly char[] INVALID_PATH_CHARS = Path.GetInvalidPathChars();
	private static readonly char[] INVALID_NAME_CHARS = Path.GetInvalidFileNameChars();


	/// <summary>
	/// Get the parent full path of the given file/folder
	/// </summary>
	public static string GetParentPath (string path) {
		if (string.IsNullOrEmpty(path)) return "";
		var info = Directory.GetParent(path);
		return info != null ? info.FullName : "";
	}


	/// <summary>
	/// Get the full path of given relative file path
	/// </summary>
	public static string GetFullPath (string path) => Path.GetFileName(path);


	/// <summary>
	/// Get the full path of given relative folder path
	/// </summary>
	public static string GetFolderFullPath (string path) => Path.GetDirectoryName(path);


	/// <inheritdoc cref="CombinePaths(string, string, string, string)"/>
	public static string CombinePaths (string path1, string path2) => Path.Combine(path1, path2);
	/// <inheritdoc cref="CombinePaths(string, string, string, string)"/>
	public static string CombinePaths (string path1, string path2, string path3) => Path.Combine(path1, path2, path3);
	/// <summary>
	/// Combine strings into a path
	/// </summary>
	/// <param name="path1">First part to combine</param>
	/// <param name="path2">Second part to combine</param>
	/// <param name="path3">Third part to combine</param>
	/// <param name="path4">Fourth part to combine</param>
	/// <returns>Full path</returns>
	public static string CombinePaths (string path1, string path2, string path3, string path4) => Path.Combine(path1, path2, path3, path4);
	/// <summary>
	/// Combine strings into a path
	/// </summary>
	/// <param name="paths">Array of parts of path</param>
	/// <returns>Full path</returns>
	public static string CombinePaths (params string[] paths) => Path.Combine(paths);


	/// <summary>
	/// Get file extension with the dot at front
	/// </summary>
	public static string GetExtensionWithDot (string path) => Path.GetExtension(path);//.txt


	/// <summary>
	/// Get file/folder name without extension
	/// </summary>
	public static string GetNameWithoutExtension (string path) => Path.GetFileNameWithoutExtension(path);


	/// <summary>
	/// Get file/folder name with extension
	/// </summary>
	public static string GetNameWithExtension (string path) => Path.GetFileName(path);


	/// <summary>
	/// Change extension of the given path
	/// </summary>
	/// <param name="path"></param>
	/// <param name="newEx">Works both with and without the dot</param>
	/// <returns>New path with extension changed</returns>
	public static string ChangeExtension (string path, string newEx) => Path.ChangeExtension(path, newEx);//txt or .txt


	/// <summary>
	/// True if the given path refers to an existing folder in disk
	/// </summary>
	public static bool FolderExists (string path) => !string.IsNullOrEmpty(path) && Directory.Exists(path);

	/// <summary>
	/// True if the given path refers to an existing file in disk
	/// </summary>
	public static bool FileExists (string path) => !string.IsNullOrEmpty(path) && File.Exists(path);


	/// <summary>
	/// True is the given path refers to an exist folder instead of file
	/// </summary>
	public static bool PathIsFolder (string path) => File.GetAttributes(path).HasFlag(FileAttributes.Directory);


	/// <summary>
	/// Get the AbsoluteUri of the given path
	/// </summary>
	public static string GetUrl (string path) => string.IsNullOrEmpty(path) ? "" : new System.Uri(path).AbsoluteUri;


	/// <summary>
	/// Get a string for current date and time
	/// </summary>
	public static string GetTimeString () => System.DateTime.Now.ToString("yyyyMMddHHmmssffff");


	/// <summary>
	/// Get FileTimeUTC from UtcNow
	/// </summary>
	/// <returns></returns>
	public static long GetLongTime () => System.DateTime.UtcNow.ToFileTimeUtc();


	/// <summary>
	/// True if two ticks inside same day
	/// </summary>
	public static bool IsSameDay (long timeA, long timeB) => new System.DateTime(timeA).Day == new System.DateTime(timeB).Day;


	/// <summary>
	/// Get time string for display label from given ticks
	/// </summary>
	public static string GetDisplayTimeFromTicks (long ticks) => new System.DateTime(ticks).ToString("yyyy-MM-dd HH:mm");


	/// <summary>
	/// Make given path valid. 
	/// 1. Directory separator will be fixed to the valid one.
	/// 2. All separators in the start of the path will be removed.
	/// 3. All separators in the end of the path will be removed.
	/// </summary>
	public static string FixPath (string path) {
		char dsChar = Path.DirectorySeparatorChar;
		char adsChar = Path.AltDirectorySeparatorChar;
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


	/// <summary>
	/// True if the two given paths refers to the same location
	/// </summary>
	public static bool IsSamePath (string pathA, string pathB) => FixPath(GetFullPath(pathA)) == FixPath(GetFullPath(pathB));


	/// <summary>
	/// True if the file/folder at path is hidden
	/// </summary>
	public static bool IsHidden (string path) => (File.GetAttributes(path) & FileAttributes.Hidden) != 0;


	/// <summary>
	/// True if the given string can be a file name.
	/// </summary>
	public static bool IsValidForFileName (string content) => content.IndexOfAny(INVALID_PATH_CHARS) < 0;


	/// <summary>
	/// True if the given path can be an qualified path
	/// </summary>
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


	/// <summary>
	/// Get relative path from given path and root
	/// </summary>
	/// <param name="relativeTo">The root folder path</param>
	/// <param name="path">The target path inside the root</param>
	/// <param name="relativePath">Result relative path</param>
	/// <returns>True if the path successfuly got</returns>
	public static bool TryGetRelativePath (string relativeTo, string path, out string relativePath) {
		try {
			relativePath = Path.GetRelativePath(relativeTo, path);
			return true;
		} catch (System.Exception ex) { Debug.LogException(ex); }
		relativePath = "";
		return false;
	}


}