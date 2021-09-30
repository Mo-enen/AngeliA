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




	// ===== World Stream ====
	public static class WorldStream {




		#region --- SUB ---



		[System.Serializable]
		private class InfoJson {
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
			InfoJson info = null;
			if (Util.FileExists(infoPath)) {
				string json = Util.FileToText(infoPath);
				info = JsonUtility.FromJson<InfoJson>(json);
			}
			if (info == null) {
				info = new InfoJson();
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


		public static void Clear () {
			WorldMap.Clear();
		}


		#endregion




		#region --- LGC ---




		#endregion




	}

}
