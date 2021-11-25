using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;



namespace AngeliaFramework {



	public struct Block {
		public uint InstanceID;
		public int BlockID;
		public Block (uint instanceID, int blockID) {
			InstanceID = instanceID;
			BlockID = blockID;
		}
	}



	public class Map {

		public int Width => Blocks.GetLength(0);
		public int Height => Blocks.GetLength(1);
		public int LayerCount => Blocks.GetLength(2);

		public Block[,,] Blocks = new Block[0, 0, 0];

	}



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


		// Map
		public static bool LoadMap (Map map, string path) {
			if (!Util.FileExists(path)) { return false; }
			using var stream = File.OpenRead(path);
			using var reader = new BinaryReader(stream);
			uint width = reader.ReadUInt32();
			uint height = reader.ReadUInt32();
			uint layerCount = reader.ReadUInt32();
#if UNITY_EDITOR
			if (map.Width != 0 && map.Height != 0 && map.LayerCount != 0 && (map.Width != width || map.Height != height || map.LayerCount != layerCount)) {
				Debug.LogError($"Map is having unexpected size.\nsize: {width} x {height} x {layerCount}\npath:{path}");
			}
#endif
			if (map.Width != width || map.Height != height || map.LayerCount != layerCount) {
				map.Blocks = new Block[width, height, layerCount];
			} else {
				System.Array.Clear(map.Blocks, 0, map.Blocks.Length);
			}
			uint cursorX = 0;
			uint cursorY = 0;
			uint cursorZ = 0;
			while (reader.NotEnd()) {
				uint insID = reader.ReadUInt32();
				if (insID >= 128) {
					// Block
					int blockID = reader.ReadInt32();
					map.Blocks[cursorX, cursorY, cursorZ] = new Block(insID, blockID);
					cursorX++;
				} else {
					// Func
					if (insID >= 1 && insID <= 9) {
						// Hard-Coded ID
						cursorX += insID;
					} else {
						switch (insID) {
							case 0: // Set Cursor
								cursorX = reader.ReadUInt32().Clamp(0, width - 1);
								cursorY = reader.ReadUInt32().Clamp(0, height - 1);
								cursorZ = reader.ReadUInt32().Clamp(0, layerCount - 1);
								break;
						}
					}
				}
			}
			return true;
		}


		public static void SaveMap (Map map, string path) {
			Util.CreateFolder(Util.GetParentPath(path));
			using var stream = File.Create(path);
			using var writer = new BinaryWriter(stream);
			uint width = (uint)map.Width;
			uint height = (uint)map.Height;
			uint layerCount = (uint)map.LayerCount;
			writer.Write(width);
			writer.Write(height);
			writer.Write(layerCount);
			uint cursorX = 0;
			uint cursorY = 0;
			uint cursorZ = 0;
			for (uint k = 0; k < layerCount; k++) {
				for (uint j = 0; j < height; j++) {
					for (uint i = 0; i < width; i++) {

						var block = map.Blocks[i, j, k];
						if (block.InstanceID < 128) { continue; }

						// Check Cusor
						if (i != cursorX) {
							if (
								j == cursorY &&
								k == cursorZ &&
								i > cursorX &&
								i <= cursorX + 9
							) {
								// Use Hard-Coded Command
								writer.Write(i - cursorX);
							} else {
								// Set Cursor
								writer.Write(0u);
								writer.Write((i - cursorX).Clamp(0, width - 1));
								writer.Write((j - cursorY).Clamp(0, height - 1));
								writer.Write((k - cursorZ).Clamp(0, layerCount - 1));
							}
						}

						// Write Block
						writer.Write(block.InstanceID);
						writer.Write(block.BlockID);
						cursorX++;
					}
				}
			}
		}


		// Map Info
		private static void LoadMapInfo (string infoPath, Project project) {
			project.MapPosToFileName.Clear();
			MapInfo info;
			if (Util.FileExists(infoPath)) {
				string json = Util.FileToText(infoPath);
				info = JsonUtility.FromJson<MapInfo>(json);
			} else {
				info = new MapInfo();
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
				info = new PlayerInfo();
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
