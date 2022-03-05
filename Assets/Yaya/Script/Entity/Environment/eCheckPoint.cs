using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eCheckPoint : Entity, IInitialize {


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


		// Const
		private static readonly int[] ARTWORK_CODES = new int[] { "Check Point 0".AngeHash(), "Check Point 1".AngeHash(), "Check Point 2".AngeHash(), "Check Point 3".AngeHash(), "Check Point 4".AngeHash(), "Check Point 5".AngeHash(), "Check Point 6".AngeHash(), "Check Point 7".AngeHash(), "Check Point 8".AngeHash(), "Check Point 9".AngeHash(), "Check Point 10".AngeHash(), "Check Point 11".AngeHash(), "Check Point 12".AngeHash(), "Check Point 13".AngeHash(), "Check Point 14".AngeHash(), "Check Point 15".AngeHash(), "Check Point 16".AngeHash(), "Check Point 17".AngeHash(), "Check Point 18".AngeHash(), "Check Point 19".AngeHash(), "Check Point 20".AngeHash(), "Check Point 21".AngeHash(), "Check Point 22".AngeHash(), "Check Point 23".AngeHash(), };
		private static readonly int[] ARTWORK_STATUE_CODES = new int[] { "Check Point Statue 0".AngeHash(), "Check Point Statue 1".AngeHash(), "Check Point Statue 2".AngeHash(), "Check Point Statue 3".AngeHash(), "Check Point Statue 4".AngeHash(), "Check Point Statue 5".AngeHash(), "Check Point Statue 6".AngeHash(), "Check Point Statue 7".AngeHash(), "Check Point Statue 8".AngeHash(), "Check Point Statue 9".AngeHash(), "Check Point Statue 10".AngeHash(), "Check Point Statue 11".AngeHash(), "Check Point Statue 12".AngeHash(), "Check Point Statue 13".AngeHash(), "Check Point Statue 14".AngeHash(), "Check Point Statue 15".AngeHash(), "Check Point Statue 16".AngeHash(), "Check Point Statue 17".AngeHash(), "Check Point Statue 18".AngeHash(), "Check Point Statue 19".AngeHash(), "Check Point Statue 20".AngeHash(), "Check Point Statue 21".AngeHash(), "Check Point Statue 22".AngeHash(), "Check Point Statue 23".AngeHash(), };

		// Api
		public override EntityLayer Layer => EntityLayer.Environment;

		// Short
		private int CheckPointID => Data >= 0 ? Data - 1 : -Data - 1;
		private bool IsStatue => Data > 0;

		// Data
		private static readonly Dictionary<int, Vector2Int> PositionPool = new();
		private int ArtworkCode = 0;


		// MSG
		public static void Initialize () {
			try {
				PositionPool.Clear();
				var path = Util.CombinePaths(AUtil.GetMapRoot(), $"{Application.productName}.cp");
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


		public override void OnCreate (int frame) {
			base.OnCreate(frame);
#if UNITY_EDITOR
			if (Data == 0) {
				Debug.LogWarning("Check point data can not be 0");
			}
#endif
			if (CheckPointID >= 0 && CheckPointID < ARTWORK_CODES.Length) {
				ArtworkCode = IsStatue ? ARTWORK_STATUE_CODES[CheckPointID] : ARTWORK_CODES[CheckPointID];
			}
			if (CellRenderer.GetUVRect(ArtworkCode, out var rect)) {
				Width = rect.Width;
				Height = rect.Height;
			}
		}


		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);
			CellRenderer.Draw(ArtworkCode, Rect);
		}


	}
}
#if UNITY_EDITOR
namespace Yaya.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	public static class CheckPointArtworkExtension {
		[MenuItem("AngeliA/Command/Sync Check Point Position Files")]
		private static void SyncCheckPointPositionFiles () {
			var cpPool = new Dictionary<int, Vector2Int>();
			var world = new World();
			int cpID = typeof(eCheckPoint).AngeHash();
			const int SIZE = Const.WORLD_MAP_SIZE;
			// Get Positions
			foreach (var file in Util.GetFilesIn(AUtil.GetMapRoot(), true, $"*.{Const.MAP_FILE_EXT}")) {
				try {
					string name = Util.GetNameWithoutExtension(file.Name);
					int _index = name.IndexOf('_');
					int worldX = int.Parse(name[.._index]);
					int worldY = int.Parse(name[(_index + 1)..]);
					if (!world.LoadFromDisk(worldX, worldY)) continue;
					for (int i = 0; i < SIZE * SIZE; i++) {
						var e = world.Entities[i];
						if (e.TypeID == cpID) {
							cpPool.TryAdd(e.Data, new(
								(worldX * SIZE + i % SIZE) * Const.CELL_SIZE,
								(worldY * SIZE + i / SIZE) * Const.CELL_SIZE
							));
						}
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
			}
			// Write Position File
			try {
				var cpData = new eCheckPoint.CheckPointData() { CPs = new eCheckPoint.CheckPointData.Data[cpPool.Count] };
				int index = 0;
				foreach (var (id, pos) in cpPool) {
					cpData.CPs[index] = new eCheckPoint.CheckPointData.Data() {
						ID = id,
						X = pos.x,
						Y = pos.y,
					};
					index++;
				}
				Util.TextToFile(
					JsonUtility.ToJson(cpData, true),
					Util.CombinePaths(AUtil.GetMapRoot(), $"{Application.productName}.cp")
				);
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}
	}
}
#endif