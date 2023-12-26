using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace AngeliaFramework {
	public interface IGlobalPosition {


		[JsonObject(MemberSerialization.OptIn)]
		public class Position {
			public int ID {
				get => i;
				set => i = value;
			}
			public Int3 UnitPosition {
				get => u;
				set => u = value;
			}
			[JsonProperty] private int i;
			[JsonProperty] private Int3 u;
		}


		public class GlobalPositionMeta {
			public Position[] Positions;
		}


		// Data
		private static readonly Dictionary<int, Int3> PositionPool = new();
		private static readonly List<Position> CreateMetaFileListCache = new();
		private static readonly HashSet<int> AllGlobalPositionID = new();
		private static readonly CancellationTokenSource CreateMetaFileToken = new();
		private static Task CreateMetaFileTask = null;
		private static Int3 CreateMetaFilePosCache = default;


		// MSG
		[OnGameInitialize(-64)]
		public static void BeforeGameInitialize () {
			foreach (var type in typeof(IGlobalPosition).AllClassImplemented()) {
				AllGlobalPositionID.TryAdd(type.AngeHash());
			}
			ReloadPool();
#if UNITY_EDITOR
			CreateMetaFileFromMapsAsync(AngePath.BuiltInMapRoot);
#endif
		}


		[OnMapChannelChanged]
		public static void OnMapChannelChanged (MapChannel _) => ReloadPool();


		// API
		public static bool TryGetPosition (int id, out Int3 globalUnitPosition) => PositionPool.TryGetValue(id, out globalUnitPosition);


		public static async void CreateMetaFileFromMapsAsync (string mapFolder) {
			if (CreateMetaFileTask != null && !CreateMetaFileTask.IsCompleted && CreateMetaFileToken.Token.CanBeCanceled) {
				CreateMetaFileToken.Cancel();
				CreateMetaFileListCache.Clear();
			}
			await Task.Run(() => CreateMetaFileFromMaps(mapFolder), CreateMetaFileToken.Token);
		}


		public static void CreateMetaFileFromMaps (string mapFolder) {
			lock (CreateMetaFileListCache) {
				if (AllGlobalPositionID.Count == 0) {
					foreach (var type in typeof(IGlobalPosition).AllClassImplemented()) {
						AllGlobalPositionID.TryAdd(type.AngeHash());
					}
				}
				CreateMetaFileListCache.Clear();
				foreach (var path in Util.EnumerateFiles(mapFolder, true, $"*.{AngePath.MAP_FILE_EXT}")) {
					if (!Util.FileExists(path)) continue;
					if (World.GetWorldPositionFromName(
						Util.GetNameWithoutExtension(path),
						out CreateMetaFilePosCache
					)) {
						World.ForAllEntities(path, AddPosToListCache);
					}
				}
				var meta = new GlobalPositionMeta() {
					Positions = CreateMetaFileListCache.ToArray(),
				};
				JsonUtil.SaveJson(meta, mapFolder);
				CreateMetaFileListCache.Clear();
				CreateMetaFileTask = null;
				ReloadPool(meta);
			}
			// Func
			static void AddPosToListCache (int id, int x, int y) {
				if (!AllGlobalPositionID.Contains(id)) return;
				CreateMetaFileListCache.Add(new Position() {
					ID = id,
					UnitPosition = new Int3(
						CreateMetaFilePosCache.x * Const.MAP + x,
						CreateMetaFilePosCache.y * Const.MAP + y,
						CreateMetaFilePosCache.z
					),
				});
			}
		}


		// LGC
		private static void ReloadPool (GlobalPositionMeta meta = null) {
			PositionPool.Clear();
			meta ??= JsonUtil.LoadOrCreateJson<GlobalPositionMeta>(WorldSquad.MapRoot);
			if (meta == null || meta.Positions == null) return;
			foreach (var pos in meta.Positions) {
				PositionPool.TryAdd(pos.ID, pos.UnitPosition);
			}
		}


	}



}
