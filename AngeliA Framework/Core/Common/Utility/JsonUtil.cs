using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;


namespace AngeliA; 
public interface IJsonSerializationCallback {
	void OnBeforeSaveToDisk ();
	void OnAfterLoadedFromDisk ();
}


public static class JsonUtil {


	public static string GetJsonPath<T> (string rootPath, string name = "", string ext = "json") => Util.CombinePaths(rootPath, $"{(string.IsNullOrEmpty(name) ? typeof(T).Name : name)}.{ext}");


	public static T LoadOrCreateJson<T> (string rootPath, string name = "", string ext = "json") where T : class, new() => LoadOrCreateJsonFromPath<T>(GetJsonPath<T>(rootPath, name, ext));
	public static T LoadOrCreateJsonFromPath<T> (string jsonPath) where T : class, new() {
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
				var target = JsonConvert.DeserializeObject<T>(data);
				if (target != null) {
					if (target is IJsonSerializationCallback ser) {
						ser.OnAfterLoadedFromDisk();
					}
					return target;
				}
			}
		} catch (System.Exception ex) { Util.LogException(ex); }
		return default;
	}


	public static bool OverrideJson<T> (string rootPath, T target, string name = "", string ext = "json") => OverrideJsonFromPath(GetJsonPath<T>(rootPath, name, ext), target);
	public static bool OverrideJsonFromPath (string jsonPath, object target) {
		if (target == null) return false;
		try {
			if (Util.FileExists(jsonPath)) {
				var data = Util.FileToText(jsonPath, Encoding.UTF8);
				JsonConvert.PopulateObject(data, target);
				if (target is IJsonSerializationCallback ser) {
					ser.OnAfterLoadedFromDisk();
				}
				return true;
			}
		} catch (System.Exception ex) { Util.LogException(ex); }
		return false;
	}


	public static void SaveJson<T> (T data, string rootPath, string name = "", string ext = "json", bool prettyPrint = false) => SaveJsonToPath(data, GetJsonPath<T>(rootPath, name, ext), prettyPrint);
	public static void SaveJsonToPath (object data, string jsonPath, bool prettyPrint = false) {
		if (data == null) return;
		if (data is IJsonSerializationCallback ser) {
			ser.OnBeforeSaveToDisk();
		}
		string jsonText = JsonConvert.SerializeObject(
			data, prettyPrint ? Formatting.Indented : Formatting.None
		);
		Util.TextToFile(jsonText, jsonPath, Encoding.UTF8);
	}


}