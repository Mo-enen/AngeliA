using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eCheckPoint : Entity, IInitialize {


		// SUB
		[System.Serializable]
		public class CheckPointData {
			[System.Serializable]
			public struct Data {
				public int Type;
				public int ID;
				public int X;
				public int Y;
			}
			public Data[] CPs = null;
		}


		// Api
		public override int Layer => (int)EntityLayer.Environment;
		protected abstract int ArtCode { get; }

		// Data
		private static readonly Dictionary<int, Vector2Int> PositionPool = new();
		private int ArtworkCode = 0;


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


		public override void OnCreate (int frame) {
			base.OnCreate(frame);
			ArtworkCode = ArtCode;
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
	public class CheckPointArtworkExtension : IArtworkEvent {
		public void Invoke () {
			var game = Object.FindObjectOfType<Game>();
			if (game == null) return;
			var cpPool = new Dictionary<(int type, int data), Vector2Int>();
			var world = new World();
			var cpIdHash = new HashSet<int>();
			foreach (var type in typeof(eCheckPoint).AllChildClass()) {
				cpIdHash.TryAdd(type.AngeHash());
			}
			const int SIZE = Const.WORLD_MAP_SIZE;
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
						if (cpIdHash.Contains(e.TypeID)) {
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
				var cpData = new eCheckPoint.CheckPointData() { CPs = new eCheckPoint.CheckPointData.Data[cpPool.Count] };
				int index = 0;
				foreach (var ((typeID, id), pos) in cpPool) {
					cpData.CPs[index] = new eCheckPoint.CheckPointData.Data() {
						Type = typeID,
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