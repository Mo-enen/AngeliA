using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AngeliA.Framework;

public sealed class WorldStream : System.IDisposable, IBlockSquad {




	#region --- SUB ---


	private class WorldData {
		public World World;
		public int LastReadWriteFrame;
		public bool IsDirty;
	}


	#endregion




	#region --- VAR ---


	// Const
	private const int START_RELEASE_COUNT = 256;
	private const int END_RELEASE_COUNT = 128;

	// Api
	public string MapRoot { get; private set; }
	public bool Readonly { get; private set; }

	// Data
	private readonly Dictionary<Int3, WorldData> Pool = new();
	private readonly List<KeyValuePair<Int3, WorldData>> CacheReleaseList = new(START_RELEASE_COUNT);
	private int CurrentValidMapCount = 0;
	private int InternalFrame = int.MinValue;


	#endregion




	#region --- API ---


	public WorldStream (string mapRoot, bool @readonly) => Load(mapRoot, @readonly);


	public void Load (string mapRoot, bool @readonly) {
		Clear();
		MapRoot = mapRoot;
		Readonly = @readonly;
	}


	public void Clear () {
		Pool.Clear();
		CurrentValidMapCount = 0;
		InternalFrame = int.MinValue;
	}


	public void Dispose () {
		SaveAllDirty();
		Clear();
	}


	public void SaveAllDirty () {
		if (Readonly) return;
		foreach (var pair in Pool) {
			var data = pair.Value;
			if (data != null && data.IsDirty) {
				data.World?.SaveToDisk(MapRoot);
				data.IsDirty = false;
			}
		}
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
			entity = world.Entities[index];
			level = world.Levels[index];
			background = world.Backgrounds[index];
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
				BlockType.Entity => world.Entities[localY * Const.MAP + localX],
				BlockType.Level => world.Levels[localY * Const.MAP + localX],
				BlockType.Background => world.Backgrounds[localY * Const.MAP + localX],
				BlockType.Element => world.Elements[localY * Const.MAP + localX],
				_ => 0,
			};
		}
		return 0;
	}


	public int GetBlockAt (int unitX, int unitY, int z) {
		int id = GetBlockAt(unitX, unitY, z, BlockType.Element);
		if (id == 0) id = GetBlockAt(unitX, unitY, z, BlockType.Entity);
		if (id == 0) id = GetBlockAt(unitX, unitY, z, BlockType.Level);
		if (id == 0) id = GetBlockAt(unitX, unitY, z, BlockType.Background);
		return id;
	}


	public void SetBlockAt (int unitX, int unitY, int z, BlockType type, int value) {
		if (Readonly) {
			if (Game.IsEdittime) {
				Util.LogError("Can not write block data when the world stream is readonly.");
			}
			return;
		}
		int worldX = unitX.UDivide(Const.MAP);
		int worldY = unitY.UDivide(Const.MAP);
		var worldData = CreateOrGetWorldData(worldX, worldY, z);
		worldData.LastReadWriteFrame = InternalFrame++;
		var world = worldData.World;
		int localX = unitX.UMod(Const.MAP);
		int localY = unitY.UMod(Const.MAP);
		worldData.IsDirty = true;
		switch (type) {
			case BlockType.Entity:
				world.Entities[localY * Const.MAP + localX] = value;
				break;
			case BlockType.Level:
				world.Levels[localY * Const.MAP + localX] = value;
				break;
			case BlockType.Background:
				world.Backgrounds[localY * Const.MAP + localX] = value;
				break;
			case BlockType.Element:
				world.Elements[localY * Const.MAP + localX] = value;
				break;
		}
	}


	public void SetBlocksAt (int unitX, int unitY, int z, int entity, int level, int background, int element) {
		if (Readonly) {
			if (Game.IsEdittime) {
				Util.LogError("Can not write block data when the world stream is readonly.");
			}
			return;
		}
		int worldX = unitX.UDivide(Const.MAP);
		int worldY = unitY.UDivide(Const.MAP);
		var worldData = CreateOrGetWorldData(worldX, worldY, z);
		worldData.LastReadWriteFrame = InternalFrame++;
		var world = worldData.World;
		int localX = unitX.UMod(Const.MAP);
		int localY = unitY.UMod(Const.MAP);
		int index = localY * Const.MAP + localX;
		world.Entities[index] = entity;
		world.Levels[index] = level;
		world.Backgrounds[index] = background;
		world.Elements[index] = element;
		worldData.IsDirty = true;
	}


	#endregion




	#region --- LGC ---


	private bool TryGetWorldData (int worldX, int worldY, int worldZ, out WorldData worldData) {
		var pos = new Int3(worldX, worldY, worldZ);
		if (Pool.TryGetValue(pos, out worldData)) return worldData != null;
		// Load From Disk
		worldData = new WorldData {
			World = new World(pos),
			LastReadWriteFrame = InternalFrame++,
			IsDirty = false,
		};
		bool loaded = worldData.World.LoadFromDisk(MapRoot, worldX, worldY, worldZ);
		if (!loaded) worldData = null;
		Pool.Add(pos, worldData);
		return loaded;
	}


	private WorldData CreateOrGetWorldData (int worldX, int worldY, int worldZ) {
		var pos = new Int3(worldX, worldY, worldZ);
		if (Pool.TryGetValue(pos, out var worldData) && worldData != null) return worldData;
		// Create New
		worldData = new WorldData {
			World = new World(pos),
			LastReadWriteFrame = InternalFrame++,
			IsDirty = false,
		};
		Pool[pos] = worldData;
		if (!worldData.World.LoadFromDisk(MapRoot, worldX, worldY, worldZ)) {
			worldData.World.SaveToDisk(MapRoot);
		}
		CurrentValidMapCount++;
		TryReleaseOverload();
		return worldData;

	}


	private void TryReleaseOverload () {
		if (CurrentValidMapCount < START_RELEASE_COUNT) return;
		CacheReleaseList.Clear();
		CacheReleaseList.AddRange(Pool.TakeWhile(a => a.Value != null));
		CacheReleaseList.Sort((a, b) => b.Value.LastReadWriteFrame.CompareTo(a.Value.LastReadWriteFrame));
		for (int i = CacheReleaseList.Count - 1; i >= END_RELEASE_COUNT; i--) {
			if (Pool.Remove(CacheReleaseList[i].Key, out var worldData)) {
				if (!Readonly && worldData.IsDirty) {
					worldData.World.SaveToDisk(MapRoot);
					worldData.IsDirty = false;
				}
			}
			CurrentValidMapCount--;
		}
	}


	#endregion




}