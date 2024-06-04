using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace AngeliA;

public sealed class WorldStream : IBlockSquad {




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

	// Data
	private static readonly Dictionary<string, WorldStream> StreamPool = new();
	private readonly Dictionary<Int3, WorldData> WorldPool = new();
	private readonly WorldPathPool PathPool = new();
	private readonly List<KeyValuePair<Int3, WorldData>> CacheReleaseList = new(START_RELEASE_COUNT);
	private int CurrentValidMapCount = 0;
	private int InternalFrame = int.MinValue;


	#endregion




	#region --- API ---


	public static WorldStream GetOrCreateStream (string mapFolder) {
		if (StreamPool.TryGetValue(mapFolder, out var stream)) {
			return stream;
		} else {
			stream = new WorldStream();
			stream.Load(mapFolder);
			StreamPool.Add(mapFolder, stream);
			return stream;
		}
	}


	public void Load (string mapRoot) {
		Clear();
		PathPool.SetMapRoot(mapRoot);
		MapRoot = mapRoot;
	}


	public void Clear () {
		WorldPool.Clear();
		PathPool.Clear();
		CurrentValidMapCount = 0;
		InternalFrame = int.MinValue;
	}


	public void SaveAllDirty () {
		foreach (var pair in WorldPool) {
			var data = pair.Value;
			if (data != null && data.IsDirty) {
				var pos = data.World.WorldPosition;
				string path = PathPool.GetOrAddPath(pos);
				data.World?.SaveToDisk(path);
				data.IsDirty = false;
			}
		}
	}


	public bool TryGetMapFilePath (Int3 worldPos, out string path) => PathPool.TryGetPath(worldPos, out path);


	public bool TryGetWorld (int worldX, int worldY, int worldZ, out World world) {
		world = null;
		if (TryGetWorldData(worldX, worldY, worldZ, out var data)) {
			world = data.World;
		}
		return world != null;
	}


	// Block
	public void GetBlocksAt (int unitX, int unitY, int z, out int entity, out int level, out int background, out int element) {
		entity = 0;
		level = 0;
		background = 0;
		element = 0;
		int worldX = unitX.UDivide(Const.MAP);
		int worldY = unitY.UDivide(Const.MAP);
		if (TryGetWorldData(worldX, worldY, z, out var worldData)) {
			worldData.LastReadWriteFrame = InternalFrame++;
			var world = worldData.World;
			int index = unitY.UMod(Const.MAP) * Const.MAP + unitX.UMod(Const.MAP);
			entity = world.Entities[index];
			level = world.Levels[index];
			background = world.Backgrounds[index];
			element = world.Elements[index];
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
		if (WorldPool.TryGetValue(pos, out worldData)) return worldData != null;
		// Load From Disk
		worldData = new WorldData {
			World = new World(pos),
			LastReadWriteFrame = InternalFrame++,
			IsDirty = false,
		};
		bool loaded = false;
		if (PathPool.TryGetPath(pos, out string path)) {
			loaded = worldData.World.LoadFromDisk(path, pos.x, pos.y, pos.z);
		}
		if (!loaded) worldData = null;
		WorldPool.Add(pos, worldData);
		return loaded;
	}


	private WorldData CreateOrGetWorldData (int worldX, int worldY, int worldZ) {
		var pos = new Int3(worldX, worldY, worldZ);
		if (WorldPool.TryGetValue(pos, out var worldData) && worldData != null) return worldData;
		// Create New
		worldData = new WorldData {
			World = new World(pos),
			LastReadWriteFrame = InternalFrame++,
			IsDirty = false,
		};
		WorldPool[pos] = worldData;
		bool loaded = false;
		if (PathPool.TryGetPath(pos, out string path)) {
			loaded = worldData.World.LoadFromDisk(path, pos.x, pos.y, pos.z);
		}
		if (!loaded) {
			path = PathPool.GetOrAddPath(pos);
			worldData.World.SaveToDisk(path);
		}
		CurrentValidMapCount++;
		TryReleaseOverload();
		return worldData;
	}


	private void TryReleaseOverload () {
		if (CurrentValidMapCount < START_RELEASE_COUNT) return;
		CacheReleaseList.Clear();
		CacheReleaseList.AddRange(WorldPool.TakeWhile(a => a.Value != null));
		CacheReleaseList.Sort((a, b) => b.Value.LastReadWriteFrame.CompareTo(a.Value.LastReadWriteFrame));
		for (int i = CacheReleaseList.Count - 1; i >= END_RELEASE_COUNT; i--) {
			if (WorldPool.Remove(CacheReleaseList[i].Key, out var worldData)) {
				if (worldData.IsDirty && worldData.World != null) {
					string path = PathPool.GetOrAddPath(worldData.World.WorldPosition);
					worldData.World.SaveToDisk(path);
					worldData.IsDirty = false;
				}
			}
			CurrentValidMapCount--;
		}
		CacheReleaseList.Clear();
	}


	#endregion




}