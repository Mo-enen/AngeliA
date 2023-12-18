using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace AngeliaFramework {
	public sealed class WorldStream : System.IDisposable, IBlockSquad {




		#region --- SUB ---


		private class WorldData {
			public World World;
			public int LastReadWriteFrame;
		}


		#endregion




		#region --- VAR ---


		// Const
		private const int START_RELEASE_COUNT = 256;
		private const int END_RELEASE_COUNT = 128;

		// Data
		private readonly Dictionary<Int3, WorldData> Pool = new();
		private readonly List<KeyValuePair<Int3, WorldData>> CacheReleaseList = new(START_RELEASE_COUNT);
		private readonly string MapRoot;
		private readonly bool Readonly;
		private int CurrentValidMapCount = 0;
		private int InternalFrame = int.MinValue;


		#endregion




		#region --- API ---


		public WorldStream (string mapRoot, bool @readonly) {
			Pool.Clear();
			MapRoot = mapRoot;
			Readonly = @readonly;
			CurrentValidMapCount = 0;
			InternalFrame = 0;
		}


		public void Clear () => Pool.Clear();


		public void Dispose () {
			// Save Worlds
			if (!Readonly) {
				foreach (var pair in Pool) {
					pair.Value?.World?.SaveToDisk(MapRoot);
				}
			}
			// Clear Pool
			Pool.Clear();
		}


		// Block
		public void GetBlocksAt (int unitX, int unitY, int z, out int entity, out int level, out int background) {
			entity = 0;
			level = 0;
			background = 0;
			int worldX = unitX.UDivide(Const.MAP);
			int worldY = unitY.UDivide(Const.MAP);
			if (TryGetWorldData(worldX, worldY, z, out var worldData)) {
				worldData.LastReadWriteFrame = InternalFrame++;
				var world = worldData.World;
				int index = unitY.UMod(Const.MAP) * Const.MAP + unitX.UMod(Const.MAP);
				entity = world.Entity[index];
				level = world.Level[index];
				background = world.Background[index];
			}
		}


		public int GetBlockAt (int unitX, int unitY, int z, BlockType type) {
			int worldX = unitX.UDivide(Const.MAP);
			int worldY = unitY.UDivide(Const.MAP);
			if (TryGetWorldData(worldX, worldY, z, out var worldData)) {
				worldData.LastReadWriteFrame = InternalFrame++;
				var world = worldData.World;
				int localX = unitX.UMod(Const.MAP);
				int localY = unitY.UMod(Const.MAP);
				return type switch {
					BlockType.Entity => world.Entity[localY * Const.MAP + localX],
					BlockType.Level => world.Level[localY * Const.MAP + localX],
					BlockType.Background => world.Background[localY * Const.MAP + localX],
					_ => 0,
				};
			}
			return 0;
		}


		public void SetBlockAt (int unitX, int unitY, int z, BlockType type, int value) {
			if (Readonly) {
#if UNITY_EDITOR
				Debug.LogError("Can not write block data when the world stream is readonly.");
#endif
				return;
			}
			int worldX = unitX.UDivide(Const.MAP);
			int worldY = unitY.UDivide(Const.MAP);
			if (TryGetWorldData(worldX, worldY, z, out var worldData, createNew: true)) {
				worldData.LastReadWriteFrame = InternalFrame++;
				var world = worldData.World;
				int localX = unitX.UMod(Const.MAP);
				int localY = unitY.UMod(Const.MAP);
				switch (type) {
					case BlockType.Entity:
						world.Entity[localY * Const.MAP + localX] = value;
						break;
					case BlockType.Level:
						world.Level[localY * Const.MAP + localX] = value;
						break;
					case BlockType.Background:
						world.Background[localY * Const.MAP + localX] = value;
						break;
				}
			}
		}


		public void SetBlocksAt (int unitX, int unitY, int z, int entity, int level, int background) {
			if (Readonly) {
#if UNITY_EDITOR
				Debug.LogError("Can not write block data when the world stream is readonly.");
#endif
				return;
			}
			int worldX = unitX.UDivide(Const.MAP);
			int worldY = unitY.UDivide(Const.MAP);
			if (TryGetWorldData(worldX, worldY, z, out var worldData, createNew: true)) {
				worldData.LastReadWriteFrame = InternalFrame++;
				var world = worldData.World;
				int localX = unitX.UMod(Const.MAP);
				int localY = unitY.UMod(Const.MAP);
				int index = localY * Const.MAP + localX;
				world.Entity[index] = entity;
				world.Level[index] = level;
				world.Background[index] = background;
			}
		}


		#endregion




		#region --- LGC ---


		private bool TryGetWorldData (int worldX, int worldY, int worldZ, out WorldData worldData, bool createNew = false) {
			var pos = new Int3(worldX, worldY, worldZ);
			if (!Pool.TryGetValue(pos, out worldData)) {
				worldData = new WorldData {
					World = new World(pos),
					LastReadWriteFrame = InternalFrame++,
				};
				bool loaded = worldData.World.LoadFromDisk(MapRoot, worldX, worldY, worldZ);
				if (!loaded && !createNew) worldData = null;
				Pool.Add(pos, worldData);
				if (createNew) worldData.World.SaveToDisk(MapRoot);
				if (worldData != null) {
					CurrentValidMapCount++;
					TryReleaseOverload();
				}
			}
			return worldData != null;
		}


		private void TryReleaseOverload () {
			if (CurrentValidMapCount < START_RELEASE_COUNT) return;
			CacheReleaseList.Clear();
			CacheReleaseList.AddRange(Pool.TakeWhile(a => a.Value != null));
			CacheReleaseList.Sort((a, b) => b.Value.LastReadWriteFrame.CompareTo(a.Value.LastReadWriteFrame));
			for (int i = CacheReleaseList.Count - 1; i >= END_RELEASE_COUNT; i--) {
				if (Pool.Remove(CacheReleaseList[i].Key, out var worldData)) {
					if (!Readonly) worldData.World.SaveToDisk(MapRoot);
				}
				CurrentValidMapCount--;
			}
		}


		#endregion




	}
}