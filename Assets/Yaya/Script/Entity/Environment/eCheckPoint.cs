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
				public int Tag;
				public int X;
				public int Y;
			}
			public Data[] CPs = null;
		}


		// Const
		private static readonly int[] ARTWORK_STATUE_CODES = new int[] { "Check Statue 0".AngeHash(), "Check Statue 1".AngeHash(), "Check Statue 2".AngeHash(), "Check Statue 3".AngeHash(), "Check Statue 4".AngeHash(), "Check Statue 5".AngeHash(), "Check Statue 6".AngeHash(), "Check Statue 7".AngeHash(), "Check Statue 8".AngeHash(), "Check Statue 9".AngeHash(), "Check Statue 10".AngeHash(), "Check Statue 11".AngeHash(), "Check Statue 12".AngeHash(), "Check Statue 13".AngeHash(), "Check Statue 14".AngeHash(), "Check Statue 15".AngeHash(), "Check Statue 16".AngeHash(), "Check Statue 17".AngeHash(), "Check Statue 18".AngeHash(), "Check Statue 19".AngeHash(), "Check Statue 20".AngeHash(), "Check Statue 21".AngeHash(), "Check Statue 22".AngeHash(), "Check Statue 23".AngeHash(), };
		private static readonly int[] ARTWORK_ALTAR_CODES = new int[] { "Check Altar 0".AngeHash(), "Check Altar 1".AngeHash(), "Check Altar 2".AngeHash(), "Check Altar 3".AngeHash(), "Check Altar 4".AngeHash(), "Check Altar 5".AngeHash(), "Check Altar 6".AngeHash(), "Check Altar 7".AngeHash(), "Check Altar 8".AngeHash(), "Check Altar 9".AngeHash(), "Check Altar 10".AngeHash(), "Check Altar 11".AngeHash(), "Check Altar 12".AngeHash(), "Check Altar 13".AngeHash(), "Check Altar 14".AngeHash(), "Check Altar 15".AngeHash(), "Check Altar 16".AngeHash(), "Check Altar 17".AngeHash(), "Check Altar 18".AngeHash(), "Check Altar 19".AngeHash(), "Check Altar 20".AngeHash(), "Check Altar 21".AngeHash(), "Check Altar 22".AngeHash(), "Check Altar 23".AngeHash(), };
		private static readonly Dictionary<int, int> TAG_POOL = new() {
			{ "cp0.0".AngeHash(), 0 },
			{ "cp0.1".AngeHash(), 0 },
			{ "cp1.0".AngeHash(), 1 },
			{ "cp1.1".AngeHash(), 1 },

		};

		// Api
		public override int Layer => (int)EntityLayer.Environment;

		// Data
		private static readonly Dictionary<int, Vector2Int> AltarPool = new();
		private static readonly HashSet<Vector2Int> AllAltarPosition = new();
		private static readonly HitInfo[] c_BlockCheck = new HitInfo[8];
		private int ArtCode = 0;
		private bool IsAltar = false;


		// MSG
		public static void Initialize () {
			var game = Object.FindObjectOfType<Game>();
			if (game == null) return;
			try {
				AltarPool.Clear();
				AllAltarPosition.Clear();
				var path = Util.CombinePaths(game.MapRoot, $"{Application.productName}.cp");
				if (Util.FileExists(path)) {
					var data = JsonUtility.FromJson<CheckPointData>(Util.FileToText(path));
					if (data != null) {
						foreach (var cp in data.CPs) {
							var pos = new Vector2Int(cp.X, cp.Y);
							AltarPool.TryAdd(cp.Tag, pos);
							AllAltarPosition.TryAdd(pos);
						}
					}
				}
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}


		public override void OnCreate (int frame) {
			base.OnCreate(frame);
			var globalUnitPos = new Vector2Int(X.UDivide(Const.CELL_SIZE), Y.UDivide(Const.CELL_SIZE));
			IsAltar = AllAltarPosition.Contains(globalUnitPos);
			if (AllAltarPosition.Contains(new(globalUnitPos.x, globalUnitPos.y - 1))) {
				Active = false;
			}
			Width = Const.CELL_SIZE;
			Height = IsAltar ? Const.CELL_SIZE * 2 : Const.CELL_SIZE;
		}


		public override void PhysicsUpdate (int frame) {
			base.PhysicsUpdate(frame);
			if (ArtCode == 0) {
				int artIndex = 0;
				int count = CellPhysics.OverlapAll(
					c_BlockCheck, (int)PhysicsMask.Level,
					new(X + Width / 2, Y - Height / 2, 1, 1),
					this
				);
				for (int i = 0; i < count; i++) {
					if (TAG_POOL.TryGetValue(c_BlockCheck[i].Tag, out artIndex)) break;
				}
				ArtCode = IsAltar ?
					ARTWORK_ALTAR_CODES[artIndex.Clamp(0, ARTWORK_ALTAR_CODES.Length - 1)] :
					ARTWORK_STATUE_CODES[artIndex.Clamp(0, ARTWORK_STATUE_CODES.Length - 1)];
			}
		}


		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);
			CellRenderer.Draw(ArtCode, Rect);
		}


	}
}
#if UNITY_EDITOR
namespace Yaya.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	public class CheckPointArtworkExtension : IArtworkEvent {
		public string Message => "Refreshing Check Altar Position";
		public void OnArtworkSynced () {

			var game = Object.FindObjectOfType<Game>();
			if (game == null) return;

			var allCpPool = new HashSet<Vector2Int>();
			var allCpBlockPool = new Dictionary<Vector2Int, int>();
			var altarPool = new Dictionary<Vector2Int, int>();
			var world = new World();
			int TARGET_ID = typeof(eCheckPoint).AngeHash();

			// Get All Cp Positions
			foreach (var file in Util.GetFilesIn(game.MapRoot, true, $"*.{Const.MAP_FILE_EXT}")) {
				try {
					if (!world.LoadFromDisk(file.FullName)) continue;
					for (int i = 0; i < Const.MAP_SIZE * Const.MAP_SIZE; i++) {
						for (var e = world.Entities[i]; e != null; e = e.Next) {
							if (e.TypeID == TARGET_ID) {
								var globalUnitPos = new Vector2Int(
									world.WorldPosition.x * Const.MAP_SIZE + i % Const.MAP_SIZE,
									world.WorldPosition.y * Const.MAP_SIZE + i / Const.MAP_SIZE
								);
								allCpPool.Add(globalUnitPos);
							}
						}
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
			}

			// Get All Blocks Below Cp
			foreach (var file in Util.GetFilesIn(game.MapRoot, true, $"*.{Const.MAP_FILE_EXT}")) {
				try {
					if (!world.LoadFromDisk(file.FullName)) continue;
					for (int i = 0; i < Const.MAP_SIZE * Const.MAP_SIZE; i++) {
						var b = world.Level[i];
						if (b == null) continue;
						int blockID = b.Last.TypeID;
						if (blockID == 0) continue;
						var globalUnitPos = new Vector2Int(
							world.WorldPosition.x * Const.MAP_SIZE + i % Const.MAP_SIZE,
							world.WorldPosition.y * Const.MAP_SIZE + i / Const.MAP_SIZE
						);
						if (allCpPool.Contains(new(globalUnitPos.x, globalUnitPos.y + 1))) {
							allCpBlockPool.Add(globalUnitPos, b.Last.Tag);
						}
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
			}

			// Get Altar Pool
			foreach (var cpPos in allCpPool) {
				if (
					allCpPool.Contains(new(cpPos.x, cpPos.y + 1)) &&
					!allCpPool.Contains(new(cpPos.x, cpPos.y + 2))
				) {
					if (allCpBlockPool.TryGetValue(new(cpPos.x, cpPos.y - 1), out int tag)) {
						altarPool.Add(cpPos, tag);
					} else {
						altarPool.Add(cpPos, 0);
					}
				}
			}

			// Write Altar Position File
			try {
				var cpData = new eCheckPoint.CheckPointData() { CPs = new eCheckPoint.CheckPointData.Data[altarPool.Count] };
				int index = 0;
				foreach (var (pos, tag) in altarPool) {
					cpData.CPs[index] = new eCheckPoint.CheckPointData.Data() {
						Tag = tag,
						X = pos.x,
						Y = pos.y,
					};
					index++;
				}
				Util.TextToFile(
					JsonUtility.ToJson(cpData, true),
					Util.CombinePaths(game.MapRoot, $"{Application.productName}.cp")
				);
			} catch (System.Exception ex) { Debug.LogException(ex); }

		}
	}
}
#endif