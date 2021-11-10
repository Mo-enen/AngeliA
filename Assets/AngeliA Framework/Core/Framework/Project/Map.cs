using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;



namespace AngeliaFramework {



	public struct Block {
		public bool IsEmpty => InstanceID == 0;
		public uint InstanceID;
		public ushort BlockID;
		public Block (uint instanceID, ushort blockID) {
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




	// ===== Map Stream ====
	public static class MapStream {




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



		#endregion




		#region --- VAR ---


		// Data
		private static readonly Dictionary<Vector3Int, string> MapPos_FileName = new();


		#endregion




		#region --- API ---


		public static void LoadInfo (string infoPath) {
			MapPos_FileName.Clear();
			MapInfo info = null;
			if (Util.FileExists(infoPath)) {
				string json = Util.FileToText(infoPath);
				info = JsonUtility.FromJson<MapInfo>(json);
			}
			if (info == null) {
				info = new MapInfo();
				Util.TextToFile(
					JsonUtility.ToJson(info, true),
					infoPath
				);
			}
			if (info != null) {
				foreach (var fileInfo in info.Data) {
					MapPos_FileName.TryAdd(
						new Vector3Int(fileInfo.X, fileInfo.Y, fileInfo.Z),
						fileInfo.FileName
					);
				}
			} else {
				Debug.LogError("[Map Stream] Info is not loaded.");
			}
		}


		public static void SaveInfo (string infoPath) {
			var info = new MapInfo();
			foreach (var pair in MapPos_FileName) {
				info.Data.Add(new MapInfo.FileInfo() {
					FileName = pair.Value,
					X = pair.Key.x,
					Y = pair.Key.y,
				});
			}
			Util.TextToFile(JsonUtility.ToJson(info, true), infoPath);
		}


		public static void Clear () => MapPos_FileName.Clear();


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
				uint id = reader.ReadUInt32();
				if (id >= 128) {
					// Block
					ushort blockID = reader.ReadUInt16();
					map.Blocks[cursorX, cursorY, cursorZ] = new Block(id, blockID);
					cursorX++;
				} else {
					// Func
					if (id >= 1 && id <= 9) {
						// Hard-Coded ID
						cursorX += id;
					} else {
						switch (id) {
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
						if (block.IsEmpty) { continue; }
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
								writer.Write((i - cursorX));
							} else {
								// Just Set Cursor
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


		public static bool HasMapAt (Vector3Int position) => MapPos_FileName.ContainsKey(position);


		public static void LoadMapAtPosition (Map map, string rootPath, Vector3Int position) => LoadMap(
			map,
			Util.CombinePaths(rootPath, MapPos_FileName[position] + ".map")
		);


		#endregion




		#region --- LGC ---



		#endregion




	}

}
