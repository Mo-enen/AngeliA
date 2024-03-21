using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;


namespace AngeliA;

public interface IJsonSerializationCallback {
	void OnBeforeSaveToDisk ();
	void OnAfterLoadedFromDisk ();
}


public static class JsonUtil {


	// Const
	private static readonly JsonSerializerOptions INDENTED = new() {
		WriteIndented = true,
		IgnoreReadOnlyFields = true,
		IgnoreReadOnlyProperties = true,
		IncludeFields = true,
		PropertyNameCaseInsensitive = true,
	};
	private static readonly JsonSerializerOptions NO_INDENTED = new() {
		WriteIndented = false,
		IgnoreReadOnlyFields = true,
		IgnoreReadOnlyProperties = true,
		IncludeFields = true,
		PropertyNameCaseInsensitive = true,
	};
	private static readonly JsonSerializerOptions READ = new() {
		IgnoreReadOnlyFields = true,
		IgnoreReadOnlyProperties = true,
		IncludeFields = true,
		PropertyNameCaseInsensitive = true,
	};


	// API
	public static string GetJsonPath<T> (string rootPath, string name = "", string ext = "json") => Util.CombinePaths(rootPath, $"{(string.IsNullOrEmpty(name) ? typeof(T).Name : name)}.{ext}");


	public static T LoadOrCreateJson<T> (string rootPath, string name = "", string ext = "json") where T : new() => LoadOrCreateJsonFromPath<T>(GetJsonPath<T>(rootPath, name, ext));
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


	public static T LoadJson<T> (string rootPath, string name = "", string ext = "json") => LoadJsonFromPath<T>(GetJsonPath<T>(rootPath, name, ext));
	public static T LoadJsonFromPath<T> (string jsonPath) {
		try {
			if (Util.FileExists(jsonPath)) {
				string data = Util.FileToText(jsonPath, Encoding.UTF8);
				var target = JsonSerializer.Deserialize<T>(data, READ);
				if (target != null) {
					if (target is IJsonSerializationCallback ser) {
						ser.OnAfterLoadedFromDisk();
					}
					return target;
				}
			}
		} catch (System.Exception ex) { Debug.LogException(ex); }
		return default;
	}


	public static void SaveJson<T> (T data, string rootPath, string name = "", string ext = "json", bool prettyPrint = false) => SaveJsonToPath(data, GetJsonPath<T>(rootPath, name, ext), prettyPrint);
	public static void SaveJsonToPath (object data, string jsonPath, bool prettyPrint = false) {
		if (data == null) return;
		if (data is IJsonSerializationCallback ser) {
			ser.OnBeforeSaveToDisk();
		}
		string jsonText = JsonSerializer.Serialize(data, data.GetType(), prettyPrint ? INDENTED : NO_INDENTED);
		Util.TextToFile(jsonText, jsonPath, Encoding.UTF8);
	}


}