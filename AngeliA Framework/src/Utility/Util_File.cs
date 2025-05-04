using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Linq;
using System.IO.Compression;

namespace AngeliA;

/// <summary>
/// Utility class of AngeliA
/// </summary>
public static partial class Util {


	// API
	/// <summary>
	/// Load file into string text
	/// </summary>
	public static string FileToText (string path) {
		if (!FileExists(path)) return "";
		using var sr = File.OpenText(path);
		string data = sr.ReadToEnd();
		sr.Close();
		return data;
	}


	/// <summary>
	/// Load file into string text
	/// </summary>
	public static string FileToText (string path, Encoding encoding) {
		if (!FileExists(path)) return "";
		using StreamReader sr = new(path, encoding);
		string data = sr.ReadToEnd();
		sr.Close();
		return data;
	}


	/// <summary>
	/// Save string text into file
	/// </summary>
	/// <param name="data"></param>
	/// <param name="path"></param>
	/// <param name="append">True if keep the existing content in the file</param>
	public static void TextToFile (string data, string path, bool append = false) {
		CreateFolder(GetParentPath(path));
		using FileStream fs = new(path, append ? FileMode.Append : FileMode.Create);
		using StreamWriter sw = new(fs, Encoding.ASCII);
		sw.Write(data);
		sw.Close();
		fs.Close();
	}


	/// <summary>
	/// Save string text into file
	/// </summary>
	/// <param name="data"></param>
	/// <param name="path"></param>
	/// <param name="encoding"></param>
	/// <param name="append">True if keep the existing content in the file</param>
	public static void TextToFile (string data, string path, Encoding encoding, bool append = false) {
		CreateFolder(GetParentPath(path));
		using FileStream fs = new(path, append ? FileMode.Append : FileMode.Create);
		using StreamWriter sw = new(fs, encoding);
		sw.Write(data);
		fs.Flush();
		sw.Close();
		fs.Close();
	}


	/// <summary>
	/// Iterate through every text lines inside given file 
	/// </summary>
	public static IEnumerable<string> ForAllLinesInFile (string path) {
		if (!FileExists(path)) yield break;
		using StreamReader sr = new(path);
		while (sr.Peek() >= 0) yield return sr.ReadLine();
	}


	/// <summary>
	/// Iterate through every text lines inside given file 
	/// </summary>
	public static IEnumerable<string> ForAllLinesInFile (string path, Encoding encoding) {
		if (!FileExists(path)) yield break;
		using var sr = new StreamReader(path, encoding);
		while (sr.Peek() >= 0) yield return sr.ReadLine();
	}


	/// <summary>
	/// Iterate through every text lines inside given string
	/// </summary>
	public static IEnumerable<string> ForAllLinesInString (string content) {
		using var reader = new StringReader(content);
		while (reader.Peek() >= 0) yield return reader.ReadLine();
	}


	internal static IEnumerable<(string name, int value)> ForAllNameAndIntInFile (string path) {
		if (!FileExists(path)) yield break;
		using var sr = new StreamReader(path);
		foreach (var (name, value) in ForAllNameAndIntInFile(sr)) {
			yield return (name, value);
		}
	}
	internal static IEnumerable<(string name, int value)> ForAllNameAndIntInFile (string path, Encoding encoding) {
		if (!FileExists(path)) yield break;
		using var sr = new StreamReader(path, encoding);
		foreach (var (name, value) in ForAllNameAndIntInFile(sr)) {
			yield return (name, value);
		}
	}
	internal static IEnumerable<(string name, int value)> ForAllNameAndIntInFile (StreamReader reader) {
		while (reader.Peek() >= 0) {
			string line = reader.ReadLine();
			int midIndex = line.IndexOf(':');
			if (midIndex <= 0 || midIndex > line.Length) continue;
			string valueStr = line[(midIndex + 1)..];
			if (!int.TryParse(valueStr, out int value)) continue;
			yield return (line[..midIndex], value);
		}
	}


	internal static IEnumerable<(string name, string value)> ForAllNameAndStringInFile (string path) {
		if (!FileExists(path)) yield break;
		using var sr = new StreamReader(path);
		foreach (var (name, value) in ForAllNameAndStringInFile(sr)) {
			yield return (name, value);
		}
	}
	internal static IEnumerable<(string name, string value)> ForAllNameAndStringInFile (string path, Encoding encoding) {
		if (!FileExists(path)) yield break;
		using var sr = new StreamReader(path, encoding);
		foreach (var (name, value) in ForAllNameAndStringInFile(sr)) {
			yield return (name, value);
		}
	}
	internal static IEnumerable<(string name, string value)> ForAllNameAndStringInFile (StreamReader reader) {
		while (reader.Peek() >= 0) {
			string line = reader.ReadLine();
			int midIndex = line.IndexOf(':');
			if (midIndex <= 0 || midIndex > line.Length) continue;
			yield return (line[..midIndex], line[(midIndex + 1)..]);
		}
	}


	/// <summary>
	/// Create a folder at given path. Ignore if folder exists.
	/// </summary>
	public static void CreateFolder (string path) {
		if (string.IsNullOrEmpty(path) || FolderExists(path)) return;
		string pPath = GetParentPath(path);
		if (!FolderExists(pPath)) {
			CreateFolder(pPath);
		}
		Directory.CreateDirectory(path);
	}


	/// <summary>
	/// Load file as a byte array
	/// </summary>
	public static byte[] FileToBytes (string path) {
		byte[] bytes = null;
		if (FileExists(path)) {
			bytes = File.ReadAllBytes(path);
		}
		return bytes ?? [];
	}


	/// <summary>
	/// Save byte array into file
	/// </summary>
	/// <param name="bytes"></param>
	/// <param name="path"></param>
	/// <param name="length">Set to -1 to save all byte array</param>
	public static void BytesToFile (byte[] bytes, string path, int length = -1) {
		CreateFolder(GetParentPath(path));
		using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
		bytes ??= [];
		fs.Write(bytes, 0, length < 0 ? bytes.Length : length);
	}


	/// <summary>
	/// True if there's any file match the pattern
	/// </summary>
	/// <param name="path">Root folder path</param>
	/// <param name="topOnly">True if only search top level of the folder</param>
	/// <param name="searchPattern">("*" means all files, "*.txt" means all txt files)</param>
	public static bool HasFileIn (string path, bool topOnly, string searchPattern) {
		if (!FolderExists(path)) return false;
		foreach (var _ in EnumerateFiles(path, topOnly, searchPattern)) return true;
		return false;
	}
	/// <summary>
	/// True if there's any file match any of the patterns
	/// </summary>
	/// <param name="path">Root folder path</param>
	/// <param name="topOnly">True if only search top level of the folder</param>
	/// <param name="searchPattern">("*" means all files, "*.txt" means all txt files)</param>
	public static bool HasFileIn (string path, bool topOnly, params string[] searchPattern) {
		if (!FolderExists(path)) return false;
		foreach (var _ in EnumerateFiles(path, topOnly, searchPattern)) return true;
		return false;
	}


	/// <summary>
	/// Iterate through path of all files that match given pattern
	/// </summary>
	/// <param name="path">Root folder path</param>
	/// <param name="topOnly">True if only search top level of the folder</param>
	/// <param name="searchPattern">("*" means all files, "*.txt" means all txt files)</param>
	public static IEnumerable<string> EnumerateFiles (string path, bool topOnly, string searchPattern) {
		if (!FolderExists(path)) yield break;
		var option = topOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
		foreach (string str in Directory.EnumerateFiles(path, searchPattern, option)) {
			yield return str;
		}
	}
	/// <summary>
	/// Iterate through path of all files that match any given patterns
	/// </summary>
	/// <param name="path">Root folder path</param>
	/// <param name="topOnly">True if only search top level of the folder</param>
	/// <param name="searchPatterns">("*" means all files, "*.txt" means all txt files)</param>
	public static IEnumerable<string> EnumerateFiles (string path, bool topOnly, params string[] searchPatterns) {
		if (!FolderExists(path)) yield break;
		var option = topOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
		if (searchPatterns == null || searchPatterns.Length == 0) {
			foreach (var filePath in Directory.EnumerateFiles(path, "*", option)) {
				yield return filePath;
			}
		} else {
			foreach (var pattern in searchPatterns) {
				foreach (var filePath in Directory.EnumerateFiles(path, pattern, option)) {
					yield return filePath;
				}
			}
		}
	}


	/// <summary>
	/// Iterate through path of all folders that match given pattern
	/// </summary>
	/// <param name="path">Root folder path</param>
	/// <param name="topOnly">True if only search top level of the folder</param>
	/// <param name="searchPattern">("*" means all folders)</param>
	public static IEnumerable<string> EnumerateFolders (string path, bool topOnly, string searchPattern = "*") {
		if (!FolderExists(path)) yield break;
		var option = topOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
		foreach (string str in Directory.EnumerateDirectories(path, searchPattern, option)) {
			yield return str;
		}
	}
	/// <summary>
	/// Iterate through path of all folders that match any given patterns
	/// </summary>
	/// <param name="path">Root folder path</param>
	/// <param name="topOnly">True if only search top level of the folder</param>
	/// <param name="searchPatterns">("*" means all folders)</param>
	public static IEnumerable<string> EnumerateFolders (string path, bool topOnly, params string[] searchPatterns) {
		if (!FolderExists(path)) yield break;
		var option = topOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
		if (searchPatterns == null || searchPatterns.Length == 0) {
			foreach (var folderPath in Directory.EnumerateDirectories(path, "*", option)) {
				yield return folderPath;
			}
		} else {
			foreach (var pattern in searchPatterns) {
				foreach (var folderPath in Directory.EnumerateDirectories(path, pattern, option)) {
					yield return folderPath;
				}
			}
		}
	}


	/// <summary>
	/// Delete the file at given path. Do nothing when file not exists
	/// </summary>
	public static void DeleteFile (string path) {
		if (FileExists(path)) {
			File.Delete(path);
		}
	}


	/// <summary>
	/// Copy file from one path to other
	/// </summary>
	/// <param name="from"></param>
	/// <param name="to"></param>
	/// <param name="overwrite">True if overwrite existing file at "to"</param>
	/// <returns>True if successfuly copied</returns>
	public static bool CopyFile (string from, string to, bool overwrite = true) {
		if (!FileExists(from)) return false;
		try {
			CreateFolder(GetParentPath(to));
			File.Copy(from, to, overwrite);
			return true;
		} catch (System.Exception ex) {
			Debug.LogException(ex);
			return false;
		}
	}


	/// <summary>
	/// Copy folder from one path to other
	/// </summary>
	/// <param name="from"></param>
	/// <param name="to"></param>
	/// <param name="copySubDirs">True if copy all folder/files inside root folder</param>
	/// <param name="ignoreHidden">True if hidden files/folders do not get copy</param>
	/// <param name="overrideFile">True if overwrite existing files at "to"</param>
	/// <returns>True if successfuly copied</returns>
	public static bool CopyFolder (string from, string to, bool copySubDirs, bool ignoreHidden, bool overrideFile = false) {

		// Get the subdirectories for the specified directory.
		DirectoryInfo dir = new(from);

		if (!dir.Exists) return false;

		DirectoryInfo[] dirs = dir.GetDirectories();
		// If the destination directory doesn't exist, create it.
		if (!Directory.Exists(to)) {
			Directory.CreateDirectory(to);
		}

		// Get the files in the directory and copy them to the new location.
		FileInfo[] files = dir.GetFiles();
		foreach (FileInfo file in files) {
			try {
				string tempPath = Path.Combine(to, file.Name);
				if (!ignoreHidden || (file.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden) {
					file.CopyTo(tempPath, overrideFile);
				}
			} catch { }
		}

		// If copying subdirectories, copy them and their contents to new location.
		if (copySubDirs) {
			foreach (DirectoryInfo subdir in dirs) {
				try {
					string temppath = Path.Combine(to, subdir.Name);
					if (!ignoreHidden || (subdir.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden) {
						CopyFolder(subdir.FullName, temppath, copySubDirs, ignoreHidden, overrideFile);
					}
				} catch { }
			}
		}
		return true;
	}


	/// <summary>
	/// Delete folder at given path. Do nothing when folder not exists
	/// </summary>
	public static void DeleteFolder (string path) {
		if (FolderExists(path)) {
			Directory.Delete(path, true);
		}
	}


	public static float GetFileSizeInMB (string path) {
		float size = -1f;
		if (FileExists(path)) {
			size = (new FileInfo(path).Length / 1024f) / 1024f;
		}
		return size;
	}


	public static T ReadXML<T> (string path) where T : class {
		var serializer = new XmlSerializer(typeof(T));
		using var stream = new FileStream(path, FileMode.Open);
		var container = serializer.Deserialize(stream) as T;
		stream.Close();
		return container;
	}


	public static void WriteXML<T> (T data, string path) where T : class {
		var serializer = new XmlSerializer(typeof(T));
		using var stream = new FileStream(path, FileMode.Create);
		serializer.Serialize(stream, data);
		stream.Close();
	}


	/// <summary>
	/// Get how many files mathchs the search pattern inside given folder path
	/// </summary>
	/// <param name="path">Root folder path</param>
	/// <param name="search">("*" means all files, "*.txt" means all txt files)</param>
	/// <param name="option"></param>
	public static int GetFileCount (string path, string search = "*", SearchOption option = SearchOption.TopDirectoryOnly) {
		if (FolderExists(path)) {
			return Directory.EnumerateFiles(path, search, option).Count();
		}
		return 0;
	}


	/// <summary>
	/// Get how many folders mathchs the search pattern inside given folder path
	/// </summary>
	/// <param name="path">Root folder path</param>
	/// <param name="search">("*" means all files)</param>
	/// <param name="option"></param>
	public static int GetFolderCount (string path, string search = "*", SearchOption option = SearchOption.TopDirectoryOnly) {
		if (FolderExists(path)) {
			return Directory.EnumerateDirectories(path, search, option).Count();
		}
		return 0;
	}


	/// <summary>
	/// Move file from one path to other. Use this function to rename files.
	/// </summary>
	/// <returns>True if successfuly moved</returns>
	public static bool MoveFile (string from, string to) {
		if (!from.Equals(to) && FileExists(from) && !FileExists(to)) {
			File.Move(from, to);
			return true;
		}
		return false;
	}


	/// <summary>
	/// Move folder from one path to other. Use this function to rename folder.
	/// </summary>
	/// /// <returns>True if successfuly moved</returns>
	public static bool MoveFolder (string from, string to) {
		if (from != to && FolderExists(from)) {
			try {
				Directory.Move(from, to);
				return true;
			} catch { }
		}
		return false;
	}


	public static long GetFileModifyDate (string path) {
		if (!FileExists(path)) return 0;
		return File.GetLastWriteTime(path).ToFileTime();
	}


	public static long GetFolderModifyDate (string path) {
		if (!FolderExists(path)) return 0;
		return Directory.GetLastWriteTime(path).ToFileTime();
	}


	public static void SetFolderModifyDate (string path, long fileTime) {
		if (!FolderExists(path)) return;
		Directory.SetLastWriteTime(path, System.DateTime.FromFileTime(fileTime));
	}


	public static long GetFileCreationDate (string path) {
		if (!FileExists(path)) return 0;
		return File.GetCreationTime(path).ToFileTime();
	}


	public static void SetFileModifyDate (string path, long fileTime) {
		if (!FileExists(path)) return;
		File.SetLastWriteTime(path, System.DateTime.FromFileTime(fileTime));
	}


	/// <summary>
	/// Save byte array into compressed file
	/// </summary>
	/// <param name="path"></param>
	/// <param name="rawBytes"></param>
	/// <param name="length">Set to -1 for the full array</param>
	/// <param name="level"></param>
	public static void ByteToCompressedFile (string path, byte[] rawBytes, int length = -1, CompressionLevel level = CompressionLevel.Optimal) {
		CreateFolder(GetParentPath(path));
		using var fileStream = File.Create(path);
		using var compressor = new ZLibStream(fileStream, level);
		compressor.Write(rawBytes, 0, length < 0 ? rawBytes.Length : length);
	}


	/// <summary>
	/// Load compressed file into byte array
	/// </summary>
	/// <param name="path"></param>
	/// <param name="byteLength">True length of the byte array</param>
	/// <returns>The raw byte array</returns>
	public static byte[] CompressedFileToByte (string path, out int byteLength) {
		byteLength = 0;
		if (!FileExists(path)) return [];
		using var fileStream = File.OpenRead(path);
		using var decompressor = new ZLibStream(fileStream, CompressionMode.Decompress);
		using var output = new MemoryStream();
		decompressor.CopyTo(output);
		byteLength = (int)output.Position;
		return output.GetBuffer();
	}


	/// <summary>
	/// Make compressed byte array into raw byte array
	/// </summary>
	public static byte[] DecompressBytes (byte[] compressedBytes) {
		if (compressedBytes == null || compressedBytes.Length == 0) return [];
		using var memStream = new MemoryStream(compressedBytes);
		using var decompressor = new ZLibStream(memStream, CompressionMode.Decompress);
		using var output = new MemoryStream();
		decompressor.CopyTo(output);
		int byteLength = (int)output.Position;
		var buffer = output.GetBuffer();
		if (byteLength != buffer.Length) {
			System.Array.Resize(ref buffer, byteLength);
		}
		return buffer;
	}


	/// <summary>
	/// True if path refers to existing file and the file is not empty.
	/// </summary>
	public static bool IsExistingFileEmpty (string path) => new FileInfo(path).Length == 0;


	/// <summary>
	/// Copy and override target if the modify date is different
	/// </summary>
	/// <param name="source"></param>
	/// <param name="target"></param>
	/// <param name="skipWhenTargetNotExists">True if only override existing file instead of create new file when target not exists.</param>
	public static void UpdateFile (string source, string target, bool skipWhenTargetNotExists = false) {
		if (!FileExists(source)) return;
		if (skipWhenTargetNotExists && !FileExists(target)) return;
		long sourceDate = GetFileModifyDate(source);
		long targetDate = GetFileModifyDate(target);
		if (sourceDate == targetDate) return;
		CopyFile(source, target, true);
		SetFileModifyDate(target, sourceDate);
		//Debug.Log("FileUpdate:", source, ">>", target);
	}


}