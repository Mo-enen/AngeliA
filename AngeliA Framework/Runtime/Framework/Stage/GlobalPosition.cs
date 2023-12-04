using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


namespace AngeliaFramework {
	public interface IGlobalPosition {


		[System.Serializable]
		public class Position {
			public int ID {
				get => i;
				set => i = value;
			}
			public Vector3Int UnitPosition {
				get => u;
				set => u = value;
			}
			[SerializeField] private int i;
			[SerializeField] private Vector3Int u;
		}


		[System.Serializable]
		public class GlobalPositionMeta {
			public Position[] Positions;
		}


		// Data
		private static readonly Dictionary<int, Vector3Int> PositionPool = new();
		private static readonly List<Position> CreateMetaFileListCache = new();
		private static readonly HashSet<int> AllGlobalPositionID = new();
		private static readonly CancellationTokenSource CreateMetaFileToken = new();
		private static Task CreateMetaFileTask = null;
		private static Vector3Int CreateMetaFilePosCache = default;


		// MSG
		[OnGameInitialize(-64)]
		public static void BeforeGameInitialize () {
			foreach (var type in typeof(IGlobalPosition).AllClassImplemented()) {
				AllGlobalPositionID.TryAdd(type.AngeHash());
			}
			ReloadPool();
#if UNITY_EDITOR
			CreateMetaFileFromMapsAsync();
#endif
		}


		[OnMapChannelChanged]
		public static void OnMapChannelChanged (MapChannel _) => ReloadPool();


		// API
		public static bool TryGetPosition (int id, out Vector3Int globalUnitPosition) => PositionPool.TryGetValue(id, out globalUnitPosition);


		public static void CreateMetaFileFromMapsAsync () {
			if (CreateMetaFileTask != null && !CreateMetaFileTask.IsCompleted && CreateMetaFileToken.Token.CanBeCanceled) {
				CreateMetaFileToken.Cancel();
				CreateMetaFileListCache.Clear();
			}
			CreateMetaFileTask = Task.Factory.StartNew(
				CreateMetaFileFromMaps,
				CreateMetaFileToken.Token
			);
		}


		public static void CreateMetaFileFromMaps () {
			if (AllGlobalPositionID.Count == 0) {
				foreach (var type in typeof(IGlobalPosition).AllClassImplemented()) {
					AllGlobalPositionID.TryAdd(type.AngeHash());
				}
			}
			CreateMetaFileListCache.Clear();
			var mapFolder = WorldSquad.MapRoot;
			foreach (var path in Util.EnumerateFiles(mapFolder, true, $"*.{AngePath.MAP_FILE_EXT}")) {
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
			AngeUtil.SaveJson(meta, mapFolder);
			CreateMetaFileListCache.Clear();
			CreateMetaFileTask = null;
			ReloadPool(meta);
			// Func
			static void AddPosToListCache (int id, int x, int y) {
				if (!AllGlobalPositionID.Contains(id)) return;
				CreateMetaFileListCache.Add(new Position() {
					ID = id,
					UnitPosition = new Vector3Int(
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
			meta ??= AngeUtil.LoadOrCreateJson<GlobalPositionMeta>(WorldSquad.MapRoot);
			if (meta == null || meta.Positions == null) return;
			foreach (var pos in meta.Positions) {
				PositionPool.TryAdd(pos.ID, pos.UnitPosition);
			}
		}


	}



}
