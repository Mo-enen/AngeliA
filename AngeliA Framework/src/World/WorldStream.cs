using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


namespace AngeliA;

/// <summary>
/// Data stream that handles map data from file to instance
/// </summary>
public sealed class WorldStream : IBlockSquad {




	#region --- SUB ---


	private struct WorldData {
		public readonly bool IsDirty => Version != SavedVersion;
		public World World;
		public int CreateFrame;
		public uint Version;
		public uint SavedVersion;
	}


	#endregion




	#region --- VAR ---


	// Const 
	private static readonly object STREAM_LOCK = new();
	private static readonly object POOL_LOCK = new();

	// Api
	/// <summary>
	/// Invoke when a world instance created
	/// </summary>
	public static event System.Action<WorldStream, World> OnWorldCreated;
	/// <summary>
	/// Invoke when a world instance loaded from file
	/// </summary>
	public static event System.Action<WorldStream, World> OnWorldLoaded;
	/// <summary>
	/// Invoke when a world instance saved to file
	/// </summary>
	public static event System.Action<WorldStream, World> OnWorldSaved;
	/// <summary>
	/// Path of the map root folder 
	/// </summary>
	public string MapRoot { get; init; }
	/// <summary>
	/// True if there's unsaved block change
	/// </summary>
	public bool IsDirty => Version != SavedVersion;
	/// <summary>
	/// True if load file from built-in map folder when no map founded in MapRoot
	/// </summary>
	public bool UseBuiltInAsFailback { get; set; } = false;

	// Data
	private static readonly Dictionary<string, WorldStream> StreamPool = [];
	private static readonly WorldPathPool PathPoolBuiltIn = [];
	private static readonly Stack<World> WorldObjectPool = [];
	private readonly Dictionary<Int3, WorldData> WorldPool = [];
	private readonly WorldPathPool PathPool = [];
	private int InternalFrame = int.MinValue;
	private uint Version = 0;
	private uint SavedVersion = 0;
	private long FailbackResetFileDate = -1;


	#endregion




	#region --- API ---


	[OnGameInitialize]
	internal static void OnGameInitialize () => PathPoolBuiltIn.SetMapRoot(Universe.BuiltIn.BuiltInMapRoot);


	/// <summary>
	/// Get an instance of WorldStream. Create new one if not exists in pool.
	/// </summary>
	/// <param name="mapFolder">Root path of the map folder</param>
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


	/// <summary>
	/// Data stream that handles map data from file to instance
	/// </summary>
	/// <param name="mapFolder">Root path of the map folder</param>
	public WorldStream (string mapFolder) {
		PathPool.SetMapRoot(mapFolder);
		MapRoot = mapFolder;
		WorldPool.Clear();
		InternalFrame = int.MinValue;
	}


	/// <summary>
	/// Save all block changes into file
	/// </summary>
	public void SaveAllDirty () {
		if (!IsDirty) return;
		SavedVersion = Version;
		lock (POOL_LOCK) {
			foreach (var pair in WorldPool) {
				ref var data = ref CollectionsMarshal.GetValueRefOrNullRef(WorldPool, pair.Key);
				bool notExists = Unsafe.IsNullRef(ref data);
				if (notExists || data.World == null || !data.IsDirty) continue;
				var pos = data.World.WorldPosition;
				string path = PathPool.GetOrAddPath(pos);
				data.World.SaveToDisk(path);
				data.SavedVersion = data.Version;
				OnWorldSaved?.InvokeSafe(this, data.World);
			}
		}
	}


	/// <summary>
	/// Load changed map from file
	/// </summary>
	/// <param name="forceDiscard">True if perform this action no matter stream is dirty of not</param>
	public void DiscardAllChanges (bool forceDiscard = false) {
		if (!IsDirty && !forceDiscard) return;
		SavedVersion = Version;
		lock (POOL_LOCK) {
			foreach (var pair in WorldPool) {
				ref var data = ref CollectionsMarshal.GetValueRefOrNullRef(WorldPool, pair.Key);
				bool notExists = Unsafe.IsNullRef(ref data);
				if (notExists || data.World == null || (!forceDiscard && !data.IsDirty)) continue;
				var pos = data.World.WorldPosition;
				string path = PathPool.GetOrAddPath(pos);
				data.World.LoadFromDisk(path, pos.x, pos.y, pos.z);
				data.SavedVersion = data.Version;
			}
		}
	}


	/// <summary>
	/// Clear internal data pool
	/// </summary>
	public void ClearWorldPool () {
		foreach (var (_, data) in WorldPool) {
			if (data.World != null) {
				ReturnWorldObject(data.World);
			}
		}
		WorldPool.Clear();
		SavedVersion = Version;
	}


	/// <summary>
	/// Make all existing user maps require reload from built-in maps without deleting the user map folder
	/// </summary>
	public void ResetFailbackCopying () => FailbackResetFileDate = Util.GetLongTime();


	// World
	/// <summary>
	/// True if world exists at given world position
	/// </summary>
	/// <param name="worldPos">In world space (1 world space = 256 * 128 global space)</param>
	public bool WorldExists (Int3 worldPos) => TryGetWorldData(worldPos, out _);


	/// <summary>
	/// Get world instance on given world position
	/// </summary>
	/// <param name="worldX">Position of world bottom-left in world space (1 world space = 256 * 128 global space)</param>
	/// <param name="worldY">Position of world bottom-left in world space (1 world space = 256 * 128 global space)</param>
	/// <param name="worldZ">Position Z</param>
	/// <param name="world">Result world instance</param>
	/// <returns>True if world founded</returns>
	public bool TryGetWorld (int worldX, int worldY, int worldZ, out World world) => TryGetWorld(new Int3(worldX, worldY, worldZ), out world);
	/// <inheritdoc cref="TryGetWorld(int, int, int, out World)"/>
	public bool TryGetWorld (Int3 worldPos, out World world) {
		world = null;
		if (TryGetWorldData(worldPos, out var data)) {
			world = data.World;
		}
		return world != null;
	}


	/// <summary>
	/// Get world instance on given world position. Create new one if not found.
	/// </summary>
	/// <param name="worldX">Position of world bottom-left in world space (1 world space = 256 * 128 global space)</param>
	/// <param name="worldY">Position of world bottom-left in world space (1 world space = 256 * 128 global space)</param>
	/// <param name="worldZ">Position Z</param>
	/// <returns>Result world instance</returns>
	public World GetOrCreateWorld (int worldX, int worldY, int worldZ) => CreateOrGetWorldData(worldX, worldY, worldZ).World;


	// Block
	/// <summary>
	/// Get block ID at given unit position
	/// </summary>
	/// <param name="unitX">Position in unit space</param>
	/// <param name="unitY">Position in unit space</param>
	/// <param name="z">Position Z</param>
	/// <param name="type">Type of the block</param>
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


	/// <summary>
	/// Get all ID of blocks at given unit position
	/// </summary>
	/// <param name="unitX">Position in unit space</param>
	/// <param name="unitY">Position in unit space</param>
	/// <param name="z">Z position</param>
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


	/// <summary>
	/// Set block ID at given unit position
	/// </summary>
	/// <param name="unitX">Position in unit space</param>
	/// <param name="unitY">Position in unit space</param>
	/// <param name="z">Position Z</param>
	/// <param name="type">Type of the block</param>
	/// <param name="value">ID of the block</param>
	public void SetBlockAt (int unitX, int unitY, int z, BlockType type, int value) {
		var worldPos = new Int3(unitX.UDivide(Const.MAP), unitY.UDivide(Const.MAP), z);
		var worldData = CreateOrGetWorldData(worldPos.x, worldPos.y, z);
		if (!worldData.IsDirty) {
			worldData.Version++;
			WorldPool[worldPos] = worldData;
		}
		var world = worldData.World;
		int localX = unitX.UMod(Const.MAP);
		int localY = unitY.UMod(Const.MAP);
		Version++;
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
			worldData.Version = 0;
			worldData.SavedVersion = 0;

			if (!PathPool.TryGetPath(worldPos, out string path)) return false;
			if (!UseBuiltInAsFailback && !Util.FileExists(path)) return false;

			// Load New World from Disk
			var newWorld = GetWorldObject(worldPos);
			worldData.World = newWorld;
			bool loaded = false;

			if (FailbackResetFileDate < 0 || Util.GetFileModifyDate(path) >= FailbackResetFileDate) {
				loaded = worldData.World.LoadFromDisk(path, worldPos.x, worldPos.y, worldPos.z);
			}

			// Failback Check
			if (
				!loaded &&
				UseBuiltInAsFailback &&
				PathPoolBuiltIn.TryGetPath(worldPos, out string builtInPath) &&
				Util.CopyFile(builtInPath, path)
			) {
				loaded = worldData.World.LoadFromDisk(path, worldPos.x, worldPos.y, worldPos.z);
				if (loaded) {
					Util.SetFileModifyDate(path, Util.GetLongTime());
				}
			}

			// Check if Loaded
			if (loaded) {
				OnWorldLoaded?.InvokeSafe(this, worldData.World);
			} else {
				worldData.World = null;
			}

			WorldPool[worldPos] = worldData;
			return worldData.World != null;
		}
	}


	private WorldData CreateOrGetWorldData (int worldX, int worldY, int worldZ) {
		lock (POOL_LOCK) {

			var worldPos = new Int3(worldX, worldY, worldZ);
			if (WorldPool.TryGetValue(worldPos, out var data) && data.World != null) return data;

			// Create New
			var newWorld = GetWorldObject(worldPos);
			data.World = newWorld;
			data.CreateFrame = InternalFrame++;
			data.Version = 0;
			data.SavedVersion = 0;
			WorldPool[worldPos] = data;

			// Load Data
			bool loaded = false;
			if (PathPool.TryGetPath(worldPos, out string path)) {

				// Load from Target Folder
				if (FailbackResetFileDate < 0 || Util.GetFileModifyDate(path) >= FailbackResetFileDate) {
					loaded = newWorld.LoadFromDisk(path, worldPos.x, worldPos.y, worldPos.z);
				}

				// Load from Failback Folder
				if (
					!loaded &&
					UseBuiltInAsFailback &&
					PathPoolBuiltIn.TryGetPath(worldPos, out string builtInPath) &&
					Util.CopyFile(builtInPath, path)
				) {
					loaded = newWorld.LoadFromDisk(path, worldPos.x, worldPos.y, worldPos.z);
					if (loaded) {
						Util.SetFileModifyDate(path, Util.GetLongTime());
					}
				}

			}

			// Final
			if (!loaded) {
				OnWorldCreated?.InvokeSafe(this, newWorld);
				Version++;
				data.Version++;
				WorldPool[worldPos] = data;
			}
			OnWorldLoaded?.InvokeSafe(this, newWorld);

			return data;
		}
	}


	private static World GetWorldObject (Int3 pos) {
		if (WorldObjectPool.Count > 0) {
			var world = WorldObjectPool.Pop();
			world.WorldPosition = pos;
			return world;
		} else {
			return new World(pos);
		}
	}


	private static void ReturnWorldObject (World world) {
		world.Clear();
		WorldObjectPool.Push(world);
	}


	#endregion




}
