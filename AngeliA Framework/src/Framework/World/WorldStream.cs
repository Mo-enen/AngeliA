using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


namespace AngeliA;

public sealed class WorldStream : IBlockSquad {




	#region --- SUB ---


	private struct WorldData {
		public World World;
		public int LastReadWriteFrame;
		public bool IsDirty;
		public bool Valid;
	}


	#endregion




	#region --- VAR ---


	// Const 
	private const int START_RELEASE_COUNT = 256;
	private const int END_RELEASE_COUNT = 128;

	// Api
	public string MapRoot { get; init; }

	// Data
	private static readonly Stack<World> WorldItemPool = new(1024);
	private static readonly Dictionary<string, WorldStream> StreamPool = new();
	private readonly Dictionary<Int3, WorldData> WorldPool = new();
	private readonly WorldPathPool PathPool = new();
	private readonly WorldPathPool FallbackPathPool = new();
	private readonly List<KeyValuePair<Int3, WorldData>> CacheReleaseList = new(START_RELEASE_COUNT);
	private readonly bool UseFallback;
	private int CurrentValidMapCount = 0;
	private int InternalFrame = int.MinValue;


	#endregion




	#region --- API ---


	public static WorldStream GetOrCreateStreamFromPool (string mapFolder) {
		if (StreamPool.TryGetValue(mapFolder, out var stream)) {
			return stream;
		} else {
			stream = new WorldStream(mapFolder);
			StreamPool.Add(mapFolder, stream);
			return stream;
		}
	}


	public WorldStream (string mapFolder) {
		UseFallback = Util.IsSamePath(mapFolder, Universe.BuiltIn.UserMapRoot);
		PathPool.SetMapRoot(mapFolder);
		FallbackPathPool.SetMapRoot(UseFallback ? Universe.BuiltIn.MapRoot : "");
		MapRoot = mapFolder;
		WorldPool.Clear();
		CurrentValidMapCount = 0;
		InternalFrame = int.MinValue;
	}


	public void SaveAllDirty () {
		foreach (var pair in WorldPool) {
			ref var data = ref CollectionsMarshal.GetValueRefOrNullRef(WorldPool, pair.Key);
			bool notExists = Unsafe.IsNullRef(ref data);
			if (notExists || !data.Valid || !data.IsDirty) continue;
			var pos = data.World.WorldPosition;
			string path = PathPool.GetOrAddPath(pos);
			data.World?.SaveToDisk(path);
			data.IsDirty = false;
		}
	}


	public bool TryGetMapFilePath (Int3 worldPos, out string path) => PathPool.TryGetPath(worldPos, out path);


	public bool ContainsWorldPos (Int3 worldPos) => PathPool.ContainsKey(worldPos) || (UseFallback && FallbackPathPool.ContainsKey(worldPos));


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
		ref var worldData = ref CreateOrGetWorldData(worldX, worldY, z);
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
		ref var worldData = ref CreateOrGetWorldData(worldX, worldY, z);
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
		if (WorldPool.TryGetValue(pos, out worldData)) return worldData.Valid;

		// Load From Disk
		var newWorld = WorldItemPool.Count > 0 ? WorldItemPool.Pop() : new World();
		newWorld.Reset(pos);
		worldData = new WorldData {
			World = newWorld,
			LastReadWriteFrame = InternalFrame++,
			IsDirty = false,
			Valid = false,
		};
		bool loaded = false;
		if (PathPool.TryGetPath(pos, out string path)) {
			loaded = worldData.World.LoadFromDisk(path, pos.x, pos.y, pos.z);
		}
		if (!loaded && UseFallback && FallbackPathPool.TryGetPath(pos, out string fallPath)) {
			path = PathPool.GetOrAddPath(pos);
			Util.CopyFile(fallPath, path);
			loaded = worldData.World.LoadFromDisk(path, pos.x, pos.y, pos.z);
		}
		worldData.Valid = loaded;
		WorldPool.Add(pos, worldData);
		if (loaded) {
			CurrentValidMapCount++;
			TryReleaseOverload();
		} else {
			worldData.World = null;
			WorldItemPool.Push(newWorld);
		}
		return loaded;
	}


	private ref WorldData CreateOrGetWorldData (int worldX, int worldY, int worldZ) {

		var pos = new Int3(worldX, worldY, worldZ);
		ref var data = ref CollectionsMarshal.GetValueRefOrNullRef(WorldPool, pos);
		if (!Unsafe.IsNullRef(ref data)) return ref data;

		// Create New
		var newWorld = WorldItemPool.Count > 0 ? WorldItemPool.Pop() : new World();
		newWorld.Reset(pos);
		var worldData = new WorldData {
			World = newWorld,
			LastReadWriteFrame = InternalFrame++,
			IsDirty = false,
			Valid = true,
		};
		WorldPool[pos] = worldData;

		// Load Data
		bool loaded = false;
		if (PathPool.TryGetPath(pos, out string path)) {
			loaded = worldData.World.LoadFromDisk(path, pos.x, pos.y, pos.z);
		}
		if (!loaded) {
			path = PathPool.GetOrAddPath(pos);
			if (UseFallback && FallbackPathPool.TryGetPath(pos, out string fallPath)) {
				Util.CopyFile(fallPath, path);
				worldData.World.LoadFromDisk(path, pos.x, pos.y, pos.z);
			} else {
				worldData.World.SaveToDisk(path);
			}
		}
		CurrentValidMapCount++;
		TryReleaseOverload();

		return ref CollectionsMarshal.GetValueRefOrNullRef(WorldPool, pos);
	}


	private void TryReleaseOverload () {
		if (CurrentValidMapCount < START_RELEASE_COUNT) return;
		CacheReleaseList.Clear();
		CacheReleaseList.AddRange(WorldPool.TakeWhile(a => a.Value.Valid));
		CacheReleaseList.Sort((a, b) => b.Value.LastReadWriteFrame.CompareTo(a.Value.LastReadWriteFrame));
		for (int i = CacheReleaseList.Count - 1; i >= END_RELEASE_COUNT; i--) {
			if (WorldPool.Remove(CacheReleaseList[i].Key, out var worldData)) {
				if (worldData.World != null) {
					WorldItemPool.Push(worldData.World);
					if (worldData.IsDirty) {
						string path = PathPool.GetOrAddPath(worldData.World.WorldPosition);
						worldData.World.SaveToDisk(path);
					}
				}
			}
			CurrentValidMapCount--;
		}
		CacheReleaseList.Clear();
	}


	#endregion




}
