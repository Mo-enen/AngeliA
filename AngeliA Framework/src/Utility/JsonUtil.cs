using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;


namespace AngeliA;

/// <summary>
/// Utility class for json operation
/// </summary>
public static class JsonUtil {


	// Const
	private static readonly JsonSerializerOptions INDENTED = new() {
		WriteIndented = true,
		IgnoreReadOnlyFields = true,
		IgnoreReadOnlyProperties = true,
		IncludeFields = true,
		PropertyNameCaseInsensitive = true,
		AllowTrailingCommas = true,
	};
	private static readonly JsonSerializerOptions NO_INDENTED = new() {
		WriteIndented = false,
		IgnoreReadOnlyFields = true,
		IgnoreReadOnlyProperties = true,
		IncludeFields = true,
		PropertyNameCaseInsensitive = true,
		AllowTrailingCommas = true,
	};
	private static readonly JsonSerializerOptions READ = new() {
		IgnoreReadOnlyFields = true,
		IgnoreReadOnlyProperties = true,
		IncludeFields = true,
		PropertyNameCaseInsensitive = true,
		AllowTrailingCommas = true,
	};


	// API
	/// <summary>
	/// Calculate auto path fot json file
	/// </summary>
	/// <typeparam name="T">Type of the json object</typeparam>
	/// <param name="rootPath">Root folder</param>
	/// <param name="name">Name that override the type name</param>
	/// <param name="ext">Extension of the file</param>
	public static string GetJsonPath<T> (string rootPath, string name = "", string ext = "json") => Util.CombinePaths(rootPath, $"{(string.IsNullOrEmpty(name) ? typeof(T).Name : name)}.{ext}");


	/// <summary>
	/// Load json file inside given folder. Create new instance if file not found.
	/// </summary>
	/// <typeparam name="T">Type of the json object</typeparam>
	/// <param name="rootPath">Root folder</param>
	/// <param name="name">Name that override the type name</param>
	/// <param name="ext">Extension of the file</param>
	public static T LoadOrCreateJson<T> (string rootPath, string name = "", string ext = "json") where T : new() => LoadOrCreateJsonFromPath<T>(GetJsonPath<T>(rootPath, name, ext));

	/// <summary>
	/// Load json file at given path. Create new instance if file not found.
	/// </summary>
	/// <typeparam name="T">Type of the json object</typeparam>
	/// <param name="jsonPath"></param>
	public static T LoadOrCreateJsonFromPath<T> (string jsonPath) where T : new() {
		var result = LoadJsonFromPath<T>(jsonPath);
		if (result == null) {
			result = new T();
			if (result is IJsonSerializationCallback ser) {
				ser.OnAfterLoadedFromDisk();
			}
		}
		return result;
	}


	/// <summary>
	/// Load json file inside given folder.
	/// </summary>
	/// <typeparam name="T">Type of the json object</typeparam>
	/// <param name="rootPath">Root folder</param>
	/// <param name="name">Name that override the type name</param>
	/// <param name="ext">Extension of the file</param>
	/// <returns>Json object instance if file valid. Return default if file not valid.</returns>
	public static T LoadJson<T> (string rootPath, string name = "", string ext = "json") => LoadJsonFromPath<T>(GetJsonPath<T>(rootPath, name, ext));

	/// <summary>
	/// Load json file at given path.
	/// </summary>
	/// <typeparam name="T">Type of the json object</typeparam>
	/// <param name="jsonPath"></param>
	/// <returns>Json object instance if file valid. Return default if file not valid</returns>
	public static T LoadJsonFromPath<T> (string jsonPath) {
		try {
			if (!Util.FileExists(jsonPath)) return default;
			string data = Util.FileToText(jsonPath, Encoding.UTF8);
			var target = JsonSerializer.Deserialize<T>(data, READ);
			if (target == null) return default;
			if (target is IJsonSerializationCallback ser) {
				ser.OnAfterLoadedFromDisk();
			}
			return target;
		} catch (Exception ex) { Debug.LogException(ex); }
		return default;
	}

	/// <summary>
	/// Save json object into given folder
	/// </summary>
	/// <typeparam name="T">Type of the json object</typeparam>
	/// <param name="data"></param>
	/// <param name="rootPath">Root folder path</param>
	/// <param name="name">Name that override the type name</param>
	/// <param name="ext">Extension of the file</param>
	/// <param name="prettyPrint">True if write with indent and line warp</param>
	public static void SaveJson<T> (T data, string rootPath, string name = "", string ext = "json", bool prettyPrint = false) => SaveJsonToPath(data, GetJsonPath<T>(rootPath, name, ext), prettyPrint);

	/// <summary>
	/// Save json object into given path
	/// </summary>
	/// <param name="data"></param>
	/// <param name="jsonPath"></param>
	/// <param name="prettyPrint">True if write with indent and line warp</param>
	public static void SaveJsonToPath (object data, string jsonPath, bool prettyPrint = false) {
		if (data == null) return;
		if (data is IJsonSerializationCallback ser) {
			ser.OnBeforeSaveToDisk();
		}
		string jsonText = JsonSerializer.Serialize(data, data.GetType(), prettyPrint ? INDENTED : NO_INDENTED);
		Util.TextToFile(jsonText, jsonPath, Encoding.UTF8);
	}


}