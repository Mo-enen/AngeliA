using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Linq;
using System.IO.Compression;

namespace AngeliA;

public static partial class Util {


	private static readonly byte[] Byte4Cache = new byte[4];


	public static string FileToText (string path) {
		if (!FileExists(path)) return "";
		StreamReader sr = File.OpenText(path);
		string data = sr.ReadToEnd();
		sr.Close();
		return data;
	}


	public static string FileToText (string path, Encoding encoding) {
		if (!FileExists(path)) return "";
		using StreamReader sr = new(path, encoding);
		string data = sr.ReadToEnd();
		sr.Close();
		return data;
	}


	public static void TextToFile (string data, string path, bool append = false) {
		CreateFolder(GetParentPath(path));
		using FileStream fs = new(path, append ? FileMode.Append : FileMode.Create);
		using StreamWriter sw = new(fs, Encoding.ASCII);
		sw.Write(data);
		sw.Close();
		fs.Close();
	}


	public static void TextToFile (string data, string path, Encoding encoding, bool append = false) {
		CreateFolder(GetParentPath(path));
		using FileStream fs = new(path, append ? FileMode.Append : FileMode.Create);
		using StreamWriter sw = new(fs, encoding);
		sw.Write(data);
		sw.Close();
		fs.Close();
	}


	public static IEnumerable<string> ForAllLines (string path) {
		if (!FileExists(path)) yield break;
		using StreamReader sr = new(path, Encoding.ASCII);
		while (sr.Peek() >= 0) yield return sr.ReadLine();
	}


	public static IEnumerable<string> ForAllLines (string path, Encoding encoding) {
		if (!FileExists(path)) yield break;
		using StreamReader sr = new(path, encoding);
		while (sr.Peek() >= 0) yield return sr.ReadLine();
	}


	public static IEnumerable<string> ForAllLinesInString (string content) {
		var reader = new StringReader(content);
		while (reader.Peek() >= 0) yield return reader.ReadLine();
	}


	public static void CreateFolder (string path) {
		if (string.IsNullOrEmpty(path) || FolderExists(path)) return;
		string pPath = GetParentPath(path);
		if (!FolderExists(pPath)) {
			CreateFolder(pPath);
		}
		Directory.CreateDirectory(path);
	}


	public static byte[] FileToByte (string path) {
		byte[] bytes = null;
		if (FileExists(path)) {
			bytes = File.ReadAllBytes(path);
		}
		return bytes ?? new byte[0];
	}


	public static Color32[] FileToByte4 (string path) {
		Color32[] bytes = null;
		if (FileExists(path)) {
			using FileStream fs = new(path, FileMode.Open);
			using BinaryReader sr = new(fs);
			bytes = new Color32[fs.Length / 4];
			for (int i = 0; i < bytes.Length; i++) {
				bytes[i] = new Color32(
					sr.ReadByte(),
					sr.ReadByte(),
					sr.ReadByte(),
					sr.ReadByte()
				);
			}
			fs.Close();
			sr.Close();
		}
		return bytes ?? new Color32[0];
	}


	public static void ByteToFile (byte[] bytes, string path, int length = -1) {
		string parentPath = GetParentPath(path);
		CreateFolder(parentPath);
		FileStream fs = new(path, FileMode.Create, FileAccess.Write);
		bytes ??= new byte[0];
		fs.Write(bytes, 0, length < 0 ? bytes.Length : length);
		fs.Close();
		fs.Dispose();
	}


	public static void Byte4ToFile (Color32[] bytes, string path) {
		string parentPath = GetParentPath(path);
		CreateFolder(parentPath);
		FileStream fs = new(path, FileMode.Create, FileAccess.Write);
		bytes ??= new Color32[0];
		for (int i = 0; i < bytes.Length; i++) {
			var byte4 = bytes[i];
			Byte4Cache[0] = byte4.r;
			Byte4Cache[1] = byte4.g;
			Byte4Cache[2] = byte4.b;
			Byte4Cache[3] = byte4.a;
			fs.Write(Byte4Cache, 0, 4);
		}
		fs.Close();
		fs.Dispose();
	}


	public static bool HasFileIn (string path, bool topOnly, string searchPattern) {
		if (!FolderExists(path)) return false;
		foreach (var _ in EnumerateFiles(path, topOnly, searchPattern)) return true;
		return false;
	}
	public static bool HasFileIn (string path, bool topOnly, params string[] searchPattern) {
		if (!FolderExists(path)) return false;
		foreach (var _ in EnumerateFiles(path, topOnly, searchPattern)) return true;
		return false;
	}


	public static IEnumerable<string> EnumerateFiles (string path, bool topOnly, string searchPattern) {
		if (!FolderExists(path)) yield break;
		var option = topOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
		foreach (string str in Directory.EnumerateFiles(path, searchPattern, option)) {
			yield return str;
		}
	}
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


	public static IEnumerable<string> EnumerateFolders (string path, bool topOnly, string searchPattern = "*") {
		if (!FolderExists(path)) yield break;
		var option = topOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
		foreach (string str in Directory.EnumerateDirectories(path, searchPattern, option)) {
			yield return str;
		}
	}
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


	public static void DeleteFile (string path) {
		if (FileExists(path)) {
			File.Delete(path);
		}
	}


	public static void CopyFile (string from, string to) {
		if (FileExists(from)) {
			CreateFolder(GetParentPath(to));
			File.Copy(from, to, true);
		}
	}


	public static bool CopyFolder (string from, string to, bool copySubDirs, bool ignoreHidden) {

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
					file.CopyTo(tempPath, false);
				}
			} catch { }
		}

		// If copying subdirectories, copy them and their contents to new location.
		if (copySubDirs) {
			foreach (DirectoryInfo subdir in dirs) {
				try {
					string temppath = Path.Combine(to, subdir.Name);
					if (!ignoreHidden || (subdir.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden) {
						CopyFolder(subdir.FullName, temppath, copySubDirs, ignoreHidden);
					}
				} catch { }
			}
		}
		return true;
	}


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
		var stream = new FileStream(path, FileMode.Open);
		var container = serializer.Deserialize(stream) as T;
		stream.Close();
		return container;
	}


	public static void WriteXML<T> (T data, string path) where T : class {
		var serializer = new XmlSerializer(typeof(T));
		var stream = new FileStream(path, FileMode.Create);
		serializer.Serialize(stream, data);
		stream.Close();
	}


	public static int GetFileCount (string path, string search = "*", SearchOption option = SearchOption.TopDirectoryOnly) {
		if (FolderExists(path)) {
			return Directory.EnumerateFiles(path, search, option).Count();
		}
		return 0;
	}


	public static int GetFolderCount (string path, string search = "*", SearchOption option = SearchOption.TopDirectoryOnly) {
		if (FolderExists(path)) {
			return Directory.EnumerateDirectories(path, search, option).Count();
		}
		return 0;
	}


	public static void MoveFile (string from, string to) {
		if (!from.Equals(to) && FileExists(from) && !FileExists(to)) {
			File.Move(from, to);
		}
	}


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


	public static void ByteToCompressedFile (string path, byte[] rawBytes, int length = -1) {
		using var fileStream = File.Create(path);
		using var compressor = new ZLibStream(fileStream, CompressionLevel.SmallestSize);
		compressor.Write(rawBytes, 0, length < 0 ? rawBytes.Length : length);
	}


	public static byte[] CompressedFileToByte (string path, out int byteLength) {
		byteLength = 0;
		if (!FileExists(path)) return System.Array.Empty<byte>();
		using var fileStream = File.OpenRead(path);
		using var decompressor = new ZLibStream(fileStream, CompressionMode.Decompress);
		using var output = new MemoryStream();
		decompressor.CopyTo(output);
		byteLength = (int)output.Position;
		return output.GetBuffer();
	}


	public static byte[] DecompressBytes (byte[] compressedBytes) {
		if (compressedBytes == null || compressedBytes.Length == 0) return System.Array.Empty<byte>();
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


	public static bool IsExistingFileEmpty (string path) => new FileInfo(path).Length == 0;


}