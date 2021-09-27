using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Moenen.Standard;


namespace AngeliaFramework.World {



	public struct Block {
		public int ID;

	}



	public class World {

		public int Width => Blocks.GetLength(0);
		public int Height => Blocks.GetLength(1);

		public Block[,] Blocks = null;

	}



	[System.Serializable]
	public class WorldInfoJson {
		[System.Serializable]
		public class FileInfo {
			public string ID;
			public int X;
			public int Y;
		}
		public List<FileInfo> Data;
	}



	// ===== World Stream ====
	public static class WorldStream {




		#region --- SUB ---







		#endregion




		#region --- VAR ---


		// Short
		private static string RootPath => Util.CombinePaths(Util.GetRuntimeBuiltRootPath(), "World");
		private static string InfoPath => Util.CombinePaths(RootPath, "World Info.json");

		// Data
		private static readonly Dictionary<Vector2Int, string> WorldMap = new Dictionary<Vector2Int, string>();


		#endregion




		#region --- MSG ---


		[RuntimeInitializeOnLoadMethod]
		private static void Init () {
			string root = RootPath;
			string infoPath = InfoPath;
			if (!Util.FolderExists(root)) {
				Util.CreateFolder(root);
			}
			if (Util.FileExists(infoPath)) {
				string json = Util.FileToText(infoPath);
				var info = JsonUtility.FromJson<WorldInfoJson>(json);
				if (info != null) {
					foreach (var fileInfo in info.Data) {
						WorldMap.TryAdd(
							new Vector2Int(fileInfo.X, fileInfo.Y),
							fileInfo.ID
						);
					}
				} else {
					Debug.LogError("[World Stream] Info is null.");
				}
			} else {
				Util.TextToFile(
					JsonUtility.ToJson(new WorldInfoJson(), true),
					infoPath
				);
			}
		}


		#endregion




		#region --- API ---





		#endregion




		#region --- LGC ---




		#endregion




	}

}
