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



	public class World {

		public int Width => Blocks.GetLength(0);
		public int Height => Blocks.GetLength(1);
		public int LayerCount => Blocks.GetLength(2);

		public Block[,,] Blocks = new Block[0, 0, 0];

	}




	// ===== World Stream ====
	public static class WorldStream {




		#region --- SUB ---



		[System.Serializable]
		private class WorldInfo {
			[System.Serializable]
			public class FileInfo {
				public string FileName;
				public int X;
				public int Y;
				public int Z;
			}
			public List<FileInfo> Data = new List<FileInfo>();
		}



		#endregion




		#region --- VAR ---


		// Data
		private static readonly Dictionary<Vector3Int, string> WorldStreamMap = new Dictionary<Vector3Int, string>();


		#endregion




		#region --- API ---


		public static void LoadInfo (string infoPath) {
			WorldStreamMap.Clear();
			WorldInfo info = null;
			if (Util.FileExists(infoPath)) {
				string json = Util.FileToText(infoPath);
				info = JsonUtility.FromJson<WorldInfo>(json);
			}
			if (info == null) {
				info = new WorldInfo();
				Util.TextToFile(
					JsonUtility.ToJson(info, true),
					infoPath
				);
			}
			if (info != null) {
				foreach (var fileInfo in info.Data) {
					WorldStreamMap.TryAdd(
						new Vector3Int(fileInfo.X, fileInfo.Y, fileInfo.Z),
						fileInfo.FileName
					);
				}
			} else {
				Debug.LogError("[World Stream] Info is not loaded.");
			}
		}


		public static void SaveInfo (string infoPath) {
			var info = new WorldInfo();
			foreach (var pair in WorldStreamMap) {
				info.Data.Add(new WorldInfo.FileInfo() {
					FileName = pair.Value,
					X = pair.Key.x,
					Y = pair.Key.y,
				});
			}
			Util.TextToFile(JsonUtility.ToJson(info, true), infoPath);
		}


		public static void Clear () => WorldStreamMap.Clear();


		// World
		public static bool FileToWorld (World world, string path) {
			if (!Util.FileExists(path)) { return false; }
			using var stream = File.OpenRead(path);
			using var reader = new BinaryReader(stream);
			int width = reader.ReadInt32();
			int height = reader.ReadInt32();
			int layerCount = reader.ReadInt32();
			if (world.Width != width || world.Height != height || world.LayerCount != layerCount) {
				world.Blocks = new Block[width, height, layerCount];
			} else {
				System.Array.Clear(world.Blocks, 0, world.Blocks.Length);
			}
			uint cursorX = 0;
			uint cursorY = 0;
			uint cursorZ = 0;
			for (int safe = 0; reader.NotEnd() && safe < 100000; safe++) {
				uint id = reader.ReadUInt32();
				if (id >= 128) {
					// Block
					ushort blockID = reader.ReadUInt16();
					world.Blocks[cursorX, cursorY, cursorZ] = new Block(id, blockID);
					cursorX++;
				} else {
					// Func
					if (id >= 1 && id <= 9) {
						// Hard-Coded ID
						cursorX += id;
					} else {
						switch (id) {
							case 0: // Set Cursor
								cursorX = reader.ReadUInt32();
								cursorY = reader.ReadUInt32();
								cursorZ = reader.ReadUInt32();
								break;
						}
					}
				}
			}
			return true;
		}


		public static void SaveWorld (World world, string path) {
			Util.CreateFolder(Util.GetParentPath(path));
			using var stream = File.Create(path);
			using var writer = new BinaryWriter(stream);
			int width = world.Width;
			int height = world.Height;
			int layerCount = world.LayerCount;
			writer.Write(width);
			writer.Write(height);
			writer.Write(layerCount);
			uint cursorX = 0;
			uint cursorY = 0;
			uint cursorZ = 0;
			for (int k = 0; k < layerCount; k++) {
				for (int j = 0; j < height; j++) {
					for (int i = 0; i < width; i++) {

						var block = world.Blocks[i, j, k];
						if (block.IsEmpty) { continue; }

						// Check Cusor
						if (i != cursorX) {
							if (
								j == cursorY &&
								k == cursorZ &&
								i > cursorX &&
								i <= cursorX + 9
							) {
								// Use Hard-Coded Command
								writer.Write((uint)(i - cursorX));
							} else {
								// Just Set Cursor
								writer.Write(0u);
								writer.Write((uint)(i - cursorX));
								writer.Write((uint)(j - cursorY));
								writer.Write((uint)(k - cursorZ));
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


		public static bool HasWorldAt (Vector3Int position) => WorldStreamMap.ContainsKey(position);


		public static void LoadWorldAtPosition (World world, string rootPath, Vector3Int position) => FileToWorld(
			world,
			Util.CombinePaths(rootPath, WorldStreamMap[position] + ".world")
		);


		#endregion




		#region --- LGC ---



		#endregion




	}

}
