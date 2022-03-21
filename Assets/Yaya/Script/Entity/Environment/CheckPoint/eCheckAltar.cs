using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eCheckAltar : eCheckPoint {


		// SUB
		[System.Serializable]
		public class CheckPointData {
			[System.Serializable]
			public struct Data {
				public int ID;
				public int X;
				public int Y;
			}
			public Data[] CPs = null;
		}

		// Data
		private static readonly Dictionary<int, Vector2Int> PositionPool = new();


		// MSG
		public static void Initialize () {
			var game = Object.FindObjectOfType<Game>();
			if (game == null) return;
			try {
				PositionPool.Clear();
				var path = Util.CombinePaths(game.MapRoot, $"{Application.productName}.cp");
				if (Util.FileExists(path)) {
					var data = JsonUtility.FromJson<CheckPointData>(Util.FileToText(path));
					if (data != null) {
						foreach (var cp in data.CPs) {
							PositionPool.TryAdd(cp.ID, new(cp.X, cp.Y));
						}
					}
				}
			} catch (System.Exception ex) {
#if UNITY_EDITOR
				Debug.LogException(ex);
#endif
			}
		}


		private static readonly int[] ARTWORK_CODES = new int[] { "Check Altar 0".AngeHash(), "Check Altar 1".AngeHash(), "Check Altar 2".AngeHash(), "Check Altar 3".AngeHash(), "Check Altar 4".AngeHash(), "Check Altar 5".AngeHash(), "Check Altar 6".AngeHash(), "Check Altar 7".AngeHash(), "Check Altar 8".AngeHash(), "Check Altar 9".AngeHash(), "Check Altar 10".AngeHash(), "Check Altar 11".AngeHash(), "Check Altar 12".AngeHash(), "Check Altar 13".AngeHash(), "Check Altar 14".AngeHash(), "Check Altar 15".AngeHash(), "Check Altar 16".AngeHash(), "Check Altar 17".AngeHash(), "Check Altar 18".AngeHash(), "Check Altar 19".AngeHash(), "Check Altar 20".AngeHash(), "Check Altar 21".AngeHash(), "Check Altar 22".AngeHash(), "Check Altar 23".AngeHash(), };
		protected override int ArtCode => Data >= 0 && Data < ARTWORK_CODES.Length ? ARTWORK_CODES[Data] : 0;




	}
}
#if UNITY_EDITOR
namespace Yaya.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	public class CheckPointArtworkExtension : IArtworkEvent {
		public void Invoke () {
			var game = Object.FindObjectOfType<Game>();
			if (game == null) return;
			var cpPool = new Dictionary<(int type, int data), Vector2Int>();
			var world = new World();

			int altaID = typeof(eCheckAltar).AngeHash();

			const int SIZE = Const.MAP_SIZE;
			// Get Positions
			string mapRoot = game.MapRoot;
			foreach (var file in Util.GetFilesIn(mapRoot, true, $"*.{Const.MAP_FILE_EXT}")) {
				try {
					string name = Util.GetNameWithoutExtension(file.Name);
					int _index = name.IndexOf('_');
					int worldX = int.Parse(name[.._index]);
					int worldY = int.Parse(name[(_index + 1)..]);
					if (!world.LoadFromDisk(mapRoot, worldX, worldY)) continue;
					for (int i = 0; i < SIZE * SIZE; i++) {
						var e = world.Entities[i];
						if (e == null) continue;
						if (e.TypeID == altaID) {
							cpPool.TryAdd((e.TypeID, e.Data), new(
								(worldX * SIZE + i % SIZE) * Const.CELL_SIZE,
								(worldY * SIZE + i / SIZE) * Const.CELL_SIZE
							));
						}
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
			}
			// Write Position File
			try {
				var cpData = new eCheckAltar.CheckPointData() { CPs = new eCheckAltar.CheckPointData.Data[cpPool.Count] };
				int index = 0;
				foreach (var ((typeID, id), pos) in cpPool) {
					cpData.CPs[index] = new eCheckAltar.CheckPointData.Data() {
						ID = id,
						X = pos.x,
						Y = pos.y,
					};
					index++;
				}
				Util.TextToFile(
					JsonUtility.ToJson(cpData, true),
					Util.CombinePaths(mapRoot, $"{Application.productName}.cp")
				);
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}
	}
}
#endif