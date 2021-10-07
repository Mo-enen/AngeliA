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
		private class WorldInfoJson {
			[System.Serializable]
			public class FileInfo {
				public string ID;
				public int X;
				public int Y;
			}
			public List<FileInfo> Data = new List<FileInfo>();
		}



		#endregion




		#region --- VAR ---


		// Data
		private static readonly Dictionary<Vector2Int, string> WorldMap = new Dictionary<Vector2Int, string>();


		#endregion




		#region --- API ---


		public static void LoadInfo (string infoPath) {
			WorldMap.Clear();
			WorldInfoJson info = null;
			if (Util.FileExists(infoPath)) {
				string json = Util.FileToText(infoPath);
				info = JsonUtility.FromJson<WorldInfoJson>(json);
			}
			if (info == null) {
				info = new WorldInfoJson();
				Util.TextToFile(
					JsonUtility.ToJson(info, true),
					infoPath
				);
			}
			if (info != null) {
				foreach (var fileInfo in info.Data) {
					WorldMap.TryAdd(
						new Vector2Int(fileInfo.X, fileInfo.Y),
						fileInfo.ID
					);
				}
			} else {
				Debug.LogError("[World Stream] Info is not loaded.");
			}
		}


		public static void SaveInfo (string infoPath) {
			var info = new WorldInfoJson();
			foreach (var pair in WorldMap) {
				info.Data.Add(new WorldInfoJson.FileInfo() {
					ID = pair.Value,
					X = pair.Key.x,
					Y = pair.Key.y,
				});
			}
			Util.TextToFile(JsonUtility.ToJson(info, true), infoPath);
		}


		public static void Clear () {
			WorldMap.Clear();
		}


		// World
		public static void WorldToFile (World world, string path) {
			int width = world.Width;
			int height = world.Height;
			Util.CreateFolder(Util.GetParentPath(path));
			using var stream = File.Create(path);
			using var writer = new BinaryWriter(stream);
			writer.Write(width);
			writer.Write(height);
			for (int j = 0; j < height; j++) {
				for (int i = 0; i < width; i++) {
					var block = world.Blocks[i, j];
					writer.Write(block.ID);
				}
			}
		}


		public static World FileToWorld (string path) {
			if (!Util.FileExists(path)) { return null; }
			var world = new World();
			using var stream = File.OpenRead(path);
			using var reader = new BinaryReader(stream);
			int width = reader.ReadInt32();
			int height = reader.ReadInt32();
			world.Blocks = new Block[width, height];
			for (int j = 0; j < height; j++) {
				for (int i = 0; i < width; i++) {
					var block = new Block() {
						ID = reader.ReadInt32(),
					};
					world.Blocks[i, j] = block;
				}
			}
			return world;
		}


		#endregion




		#region --- LGC ---




		#endregion




	}

}
