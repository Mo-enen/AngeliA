using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;



namespace AngeliaFramework {



	public struct Block {
		public bool IsEmpty => InstanceID == 0;
		public uint InstanceID;
		public ushort BlockID;
	}



	public class World {

		public Block this[int x, int y, int layer] {
			get => Blocks[x, y, layer];
			set => Blocks[x, y, layer] = value;
		}
		public int Width => Blocks.GetLength(0);
		public int Height => Blocks.GetLength(1);
		public int LayerCount => Blocks.GetLength(2);

		public Block[,,] Blocks = null;
		public readonly string[] Commands = new string[64];
		public int CommandCount = 0;

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
		public static World LoadWorld (World world, string path) {
			if (!Util.FileExists(path)) { return null; }
			using var stream = File.OpenRead(path);
			using var reader = new BinaryReader(stream);






			return world;
		}


		public static void SaveWorld (World world, string path) {

			Util.CreateFolder(Util.GetParentPath(path));
			using var stream = File.Create(path);
			using var writer = new BinaryWriter(stream);

			int width = world.Width;
			int height = world.Height;
			int layerCount = world.LayerCount;
			int cmdCount = world.CommandCount;
			Vector3Int cursor = new Vector3Int(0, 0, 0);

			for (int layer = 0; layer < layerCount; layer++) {
				bool useEntity = (Layer)layer != Layer.Background && (Layer)layer != Layer.Level;
				for (int y = 0; y < height; y++) {
					for (int x = 0; x < width; x++) {
						var block = world[x, y, layer];
						if (block.IsEmpty) { continue; }




					}
				}
			}

		}


		public static bool HasWorldAt (Vector3Int position) => WorldStreamMap.ContainsKey(position);


		public static void LoadWorldAtPosition (World world, string rootPath, Vector3Int position) => LoadWorld(
			world,
			Util.CombinePaths(rootPath, WorldStreamMap[position] + ".world")
		);


		#endregion




		#region --- LGC ---




		#endregion




	}

}
