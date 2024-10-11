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
		public int CreateFrame;
		public bool IsDirty;
	}


	#endregion




	#region --- VAR ---


	// Const 
	private const int START_RELEASE_COUNT = 256;
	private const int END_RELEASE_COUNT = 128;
	private static readonly object STREAM_LOCK = new();
	private static readonly object POOL_LOCK = new();

	// Api
	public string MapRoot { get; init; }
	public bool IsDirty { get; private set; } = false;

	// Data
	private static readonly Dictionary<string, WorldStream> StreamPool = [];
	private readonly Dictionary<Int3, WorldData> WorldPool = [];
	private readonly WorldPathPool PathPool = [];
	private readonly List<KeyValuePair<Int3, WorldData>> CacheReleaseList = new(START_RELEASE_COUNT);
	private int CurrentValidMapCount = 0;
	private int InternalFrame = int.MinValue;


	#endregion




	#region --- API ---


	public static WorldStream GetOrCreateStreamFromPool (string mapFolder) {
		lock (STREAM_LOCK) {
			if (StreamPool.TryGetValue(mapFolder, out var stream)) {
				return stream;
			} else {
				stream = new WorldStream(mapFolder);
				StreamPool.Add(mapFolder, stream);
				return stream;
			}
		}
	}


	public static void ClearStreamPool () => StreamPool.Clear();


	public WorldStream (string mapFolder) {
		PathPool.SetMapRoot(mapFolder);
		MapRoot = mapFolder;
		WorldPool.Clear();
		CurrentValidMapCount = 0;
		InternalFrame = int.MinValue;
	}


	public void SaveAllDirty () {
		if (!IsDirty) return;
		IsDirty = false;
		lock (POOL_LOCK) {
			foreach (var pair in WorldPool) {
				ref var data = ref CollectionsMarshal.GetValueRefOrNullRef(WorldPool, pair.Key);
				bool notExists = Unsafe.IsNullRef(ref data);
				if (notExists || data.World == null || !data.IsDirty) continue;
				var pos = data.World.WorldPosition;
				string path = PathPool.GetOrAddPath(pos);
				data.World.SaveToDisk(path);
				data.IsDirty = false;
			}
		}
	}


	public void DiscardAllChanges (bool forceDiscard = false) {
		if (!IsDirty && !forceDiscard) return;
		IsDirty = false;
		lock (POOL_LOCK) {
			foreach (var pair in WorldPool) {
				ref var data = ref CollectionsMarshal.GetValueRefOrNullRef(WorldPool, pair.Key);
				bool notExists = Unsafe.IsNullRef(ref data);
				if (notExists || data.World == null || (!forceDiscard && !data.IsDirty)) continue;
				var pos = data.World.WorldPosition;
				string path = PathPool.GetOrAddPath(pos);
				data.World.LoadFromDisk(path, pos.x, pos.y, pos.z);
				data.IsDirty = false;
			}
		}
	}


	public void ClearWorldPool () => WorldPool.Clear();


	// World
	public bool WorldExists (int worldX, int worldY, int worldZ) => TryGetWorldData(new(worldX, worldY, worldZ), out _);


	public bool TryGetWorld (int worldX, int worldY, int worldZ, out World world) => TryGetWorld(new Int3(worldX, worldY, worldZ), out world);
	public bool TryGetWorld (Int3 worldPos, out World world) {
		world = null;
		if (TryGetWorldData(worldPos, out var data)) {
			world = data.World;
		}
		return world != null;
	}


	public void AddWorld (World world, bool overrideExists = false) {
		if (WorldPool.TryGetValue(world.WorldPosition, out var data)) {
			if (data.World == null || overrideExists) {
				if (data.World == null) {
					CurrentValidMapCount++;
				}
				data.World = world;
				data.IsDirty = true;
				IsDirty = true;
			}
		} else {
			data = new WorldData {
				IsDirty = true,
				World = world,
				CreateFrame = InternalFrame++,
			};
			IsDirty = true;
			WorldPool.Add(world.WorldPosition, data);
			CurrentValidMapCount++;
		}
	}


	public void SetWorldDirty (Int3 worldPos) {
		if (TryGetWorldData(worldPos, out var data) && !data.IsDirty && data.World != null) {
			data.IsDirty = true;
			IsDirty = true;
		}
	}


	// Block
	public int GetBlockAt (int unitX, int unitY, int z, BlockType type) {
		int worldX = unitX.UDivide(Const.MAP);
		int worldY = unitY.UDivide(Const.MAP);
		if (TryGetWorldData(worldX, worldY, z, out var worldData)) {
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
		var (level, bg, entity, element) = GetAllBlocksAt(unitX, unitY, z);
		int id = entity;
		if (id == 0) id = element;
		if (id == 0) id = level;
		if (id == 0) id = bg;
		return id;
	}


	public (int level, int bg, int entity, int element) GetAllBlocksAt (int unitX, int unitY, int z) {
		int worldX = unitX.UDivide(Const.MAP);
		int worldY = unitY.UDivide(Const.MAP);
		if (TryGetWorldData(worldX, worldY, z, out var worldData)) {
			var world = worldData.World;
			int localX = unitX.UMod(Const.MAP);
			int localY = unitY.UMod(Const.MAP);
			int index = localY * Const.MAP + localX;
			return (world.Levels[index], world.Backgrounds[index], world.Entities[index], world.Elements[index]);
		}
		return default;
	}


	public void SetBlockAt (int unitX, int unitY, int z, BlockType type, int value) {
		var worldPos = new Int3(unitX.UDivide(Const.MAP), unitY.UDivide(Const.MAP), z);
		var worldData = CreateOrGetWorldData(worldPos.x, worldPos.y, z);
		if (!worldData.IsDirty) {
			worldData.IsDirty = true;
			WorldPool[worldPos] = worldData;
		}
		var world = worldData.World;
		int localX = unitX.UMod(Const.MAP);
		int localY = unitY.UMod(Const.MAP);
		IsDirty = true;
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


	#endregion




	#region --- LGC ---


	private bool TryGetWorldData (int worldX, int worldY, int worldZ, out WorldData worldData) => TryGetWorldData(new Int3(worldX, worldY, worldZ), out worldData);
	private bool TryGetWorldData (Int3 worldPos, out WorldData worldData) {
		lock (POOL_LOCK) {

			if (WorldPool.TryGetValue(worldPos, out worldData)) return worldData.World != null;
			WorldPool.Add(worldPos, worldData);
			worldData.CreateFrame = InternalFrame++;
			worldData.IsDirty = false;

			if (!PathPool.TryGetPath(worldPos, out string path)) return false;
			if (!Util.FileExists(path)) return false;

			// Load New World from Disk
			var newWorld = new World(worldPos);
			worldData.World = newWorld;
			bool loaded = worldData.World.LoadFromDisk(path, worldPos.x, worldPos.y, worldPos.z);

			// Check if Loaded
			if (loaded) {
				CurrentValidMapCount++;
				TryReleaseOverload();
			} else {
				worldData.World = null;
			}

			WorldPool[worldPos] = worldData;
			return worldData.World != null;
		}
	}


	private WorldData CreateOrGetWorldData (int worldX, int worldY, int worldZ) {
		lock (POOL_LOCK) {
			var pos = new Int3(worldX, worldY, worldZ);
			if (WorldPool.TryGetValue(pos, out var data) && data.World != null) return data;

			// Create New
			var newWorld = new World(pos);
			data.World = newWorld;
			data.CreateFrame = InternalFrame++;
			data.IsDirty = false;
			WorldPool[pos] = data;

			// Load Data
			if (PathPool.TryGetPath(pos, out string path)) {
				newWorld.LoadFromDisk(path, pos.x, pos.y, pos.z);
			}
			CurrentValidMapCount++;
			TryReleaseOverload();

			return data;
		}
	}


	private void TryReleaseOverload () {
		if (CurrentValidMapCount < START_RELEASE_COUNT) return;
		lock (POOL_LOCK) {
			// Get Release List
			CacheReleaseList.Clear();
			foreach (var pair in WorldPool) {
				if (pair.Value.World == null) continue;
				CacheReleaseList.Add(pair);
			}
			CacheReleaseList.Sort((a, b) => a.Value.CreateFrame.CompareTo(b.Value.CreateFrame));
			// Release
			SaveAllDirty();
			for (int i = END_RELEASE_COUNT; i < CacheReleaseList.Count; i++) {
				WorldPool.Remove(CacheReleaseList[i].Key, out var worldData);
				CurrentValidMapCount--;
			}
			CacheReleaseList.Clear();
		}
	}


	#endregion




}
