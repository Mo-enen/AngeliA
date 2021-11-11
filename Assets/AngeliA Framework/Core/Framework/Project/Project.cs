using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace AngeliaFramework {
	public class Project {


		public readonly Dictionary<Vector3Int, string> MapPosToFileName = new();
		public readonly Dictionary<string, string> PlayerProperty = new();



	}


	public static class ProjectStream {




		#region --- SUB ---



		[System.Serializable]
		private class MapInfo {
			[System.Serializable]
			public class FileInfo {
				public string FileName;
				public int X;
				public int Y;
				public int Z;
			}
			public List<FileInfo> Data = new();
		}


		[System.Serializable]
		private class PlayerInfo {
			[System.Serializable]
			public class Pair {
				public string Key;
				public string Value;
			}
			public List<Pair> Data = new();
		}


		#endregion




		#region --- API ---


		public static Project LoadProject (string projectPath) {
			var project = new Project();
			LoadMapInfo(GetMapInfoPath(projectPath), project);
			LoadPlayerInfo(GetPlayerInfoPath(projectPath), project);



			return project;
		}


		public static void SaveProject (string projectPath, Project project) {
			SaveMapInfo(GetMapInfoPath(projectPath), project);
			SavePlayerInfo(GetPlayerInfoPath(projectPath), project);
			Util.CreateFolder(GetMapRoot(projectPath));



		}


		#endregion




		#region --- LGC ---


		// Map Info
		private static void LoadMapInfo (string infoPath, Project project) {
			project.MapPosToFileName.Clear();
			MapInfo info;
			if (Util.FileExists(infoPath)) {
				string json = Util.FileToText(infoPath);
				info = JsonUtility.FromJson<MapInfo>(json);
			} else {
				throw new System.Exception($"Map info file not exists\npath:{infoPath}");
			}
			if (info != null) {
				foreach (var fileInfo in info.Data) {
					project.MapPosToFileName.TryAdd(
						new Vector3Int(fileInfo.X, fileInfo.Y, fileInfo.Z),
						fileInfo.FileName
					);
				}
			} else {
				throw new System.Exception($"Map info is not loaded\npath:{infoPath}");
			}
		}


		private static void SaveMapInfo (string infoPath, Project project) {
			var info = new MapInfo();
			foreach (var pair in project.MapPosToFileName) {
				info.Data.Add(new MapInfo.FileInfo() {
					FileName = pair.Value,
					X = pair.Key.x,
					Y = pair.Key.y,
				});
			}
			Util.TextToFile(JsonUtility.ToJson(info, true), infoPath);
		}


		// Player Info
		private static void LoadPlayerInfo (string infoPath, Project project) {
			project.PlayerProperty.Clear();
			PlayerInfo info;
			if (Util.FileExists(infoPath)) {
				string json = Util.FileToText(infoPath);
				info = JsonUtility.FromJson<PlayerInfo>(json);
			} else {
				throw new System.Exception($"Player info file not exists\npath:{infoPath}");
			}
			if (info != null) {
				foreach (var pair in info.Data) {
					project.PlayerProperty.TryAdd(pair.Key, pair.Value);
				}
			} else {
				throw new System.Exception($"Player info is not loaded\npath:{infoPath}");
			}
		}


		private static void SavePlayerInfo (string infoPath, Project project) {
			var info = new PlayerInfo();
			foreach (var pair in project.PlayerProperty) {
				info.Data.Add(new PlayerInfo.Pair() {
					Key = pair.Key,
					Value = pair.Value,
				});
			}
			Util.TextToFile(JsonUtility.ToJson(info, true), infoPath);
		}


		// Misc
		private static string GetMapRoot (string projectPath) => Util.CombinePaths(
			projectPath, "Map"
		);


		private static string GetMapInfoPath (string projectPath) => Util.CombinePaths(
			projectPath, "Map Info.json"
		);


		private static string GetPlayerInfoPath (string projectPath) => Util.CombinePaths(
			projectPath, "Player Info.json"
		);


		#endregion




	}
}
