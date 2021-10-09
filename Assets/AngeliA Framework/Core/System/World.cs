using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;



namespace AngeliaFramework {



	public struct Block {
		public int ID;

	}



	public class World {

		public int Width => Blocks.GetLength(0);
		public int Height => Blocks.GetLength(1);

		public Block[,] Blocks = null;

	}




	// ===== World Stream ====
	public static class WorldStream {




		#region --- SUB ---



		[System.Serializable]
		private class WorldInfo {
			[System.Serializable]
			public class FileInfo {
				public string ID;
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
						fileInfo.ID
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
					ID = pair.Value,
					X = pair.Key.x,
					Y = pair.Key.y,
				});
			}
			Util.TextToFile(JsonUtility.ToJson(info, true), infoPath);
		}


		public static void Clear () => WorldStreamMap.Clear();


		// World
		public static void WorldToFile (World world, string path) {
			int width = world.Width;
			int height = world.Height;
			Util.CreateFolder(Util.GetParentPath(path));
			using var stream = File.Create(path);
			using var writer = new BinaryWriter(stream);
			writer.Write((ushort)width);
			writer.Write((ushort)height);
			for (int j = 0; j < height; j++) {
				for (int i = 0; i < width; i++) {
					var block = world.Blocks[i, j];
					writer.Write((ushort)block.ID);
				}
			}
		}


		public static World FileToWorld (string path) {
			if (!Util.FileExists(path)) { return null; }
			var world = new World();
			using var stream = File.OpenRead(path);
			using var reader = new BinaryReader(stream);
			int width = reader.ReadUInt16();
			int height = reader.ReadUInt16();
			world.Blocks = new Block[width, height];
			for (int j = 0; j < height; j++) {
				for (int i = 0; i < width; i++) {
					var block = new Block() {
						ID = reader.ReadUInt16(),
					};
					world.Blocks[i, j] = block;
				}
			}
			return world;
		}


		public static bool HasWorldAt (Vector3Int position) => WorldStreamMap.ContainsKey(position);


		public static World LoadWorldAt (string rootPath, Vector3Int position) => FileToWorld(
			Util.CombinePaths(rootPath, WorldStreamMap[position] + ".world")
		);


		#endregion




		#region --- LGC ---




		#endregion




	}

}
