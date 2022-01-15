using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;



namespace AngeliaFramework {



	public class Project {
		public readonly Dictionary<Vector3Int, string> MapPosToFileName = new();
		public readonly Dictionary<string, string> PlayerProperty = new();
	}



	public static class ProjectStream {




		#region --- SUB ---



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



		[System.Serializable]
		private class ProjectInfo {


			[System.Serializable]
			public class MapFileInfo {
				public string FileName;
				public int X;
				public int Y;
				public int Z;
			}


			[System.Serializable]
			public class StringPair {
				public string Key;
				public string Value;
			}


			public List<MapFileInfo> MapFile = new();
			public List<StringPair> PlayerData = new();
			public int MapWidth = 0;
			public int MapHeight = 0;
			public int MapLayerCount = 0;

		}



		#endregion




		#region --- API ---


		public static void LoadProject (string projectPath, Project project) => 
			LoadProjectInfo(GetInfoPath(projectPath), project);


		public static void SaveProject (string projectPath, Project project) {
			Util.CreateFolder(projectPath);
			Util.CreateFolder(GetMapRoot(projectPath));
			SaveProjectInfo(GetInfoPath(projectPath), project);
		}


		// Map
		public static bool FileIntoMap (Map map, string path) {
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


		public static void MapIntoFile (Map map, string path) {
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


		#endregion




		#region --- LGC ---


		// Map Info
		private static void LoadProjectInfo (string infoPath, Project project) {
			project.MapPosToFileName.Clear();
			ProjectInfo info;
			if (Util.FileExists(infoPath)) {
				string json = Util.FileToText(infoPath);
				info = JsonUtility.FromJson<ProjectInfo>(json);
			} else {
				info = new();
			}
			if (info != null) {
				// Map File
				foreach (var fileInfo in info.MapFile) {
					project.MapPosToFileName.TryAdd(
						new Vector3Int(fileInfo.X, fileInfo.Y, fileInfo.Z),
						fileInfo.FileName
					);
				}
				// Player Data
				foreach (var pair in info.PlayerData) {
					project.PlayerProperty.TryAdd(pair.Key, pair.Value);
				}
			} else {
				throw new System.Exception($"Project info is not loaded\npath:{infoPath}");
			}
		}


		private static void SaveProjectInfo (string infoPath, Project project) {
			var info = new ProjectInfo();
			// Map File
			foreach (var pair in project.MapPosToFileName) {
				info.MapFile.Add(new() {
					FileName = pair.Value,
					X = pair.Key.x,
					Y = pair.Key.y,
				});
			}
			// Player Data
			foreach (var pair in project.PlayerProperty) {
				info.PlayerData.Add(new() {
					Key = pair.Key,
					Value = pair.Value,
				});
			}
			// To File
			Util.TextToFile(JsonUtility.ToJson(info, true), infoPath);
		}


		// Path
		private static string GetMapRoot (string projectPath) => Util.CombinePaths(
			projectPath, "Map"
		);


		private static string GetInfoPath (string projectPath) => Util.CombinePaths(
			projectPath, "Info.json"
		);


		#endregion




	}
}
