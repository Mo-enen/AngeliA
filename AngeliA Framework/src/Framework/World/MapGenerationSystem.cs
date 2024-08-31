using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AngeliA;

public static class MapGenerationSystem {




	#region --- SUB ---


	private enum MapState : byte { Generating, Success, Fail, }


	#endregion




	#region --- VAR ---


	// Api
	public static long Seed { get; private set; }

	// Data
	private static readonly Dictionary<Int3, MapState> StatePool = new();
	private static readonly List<MapGenerator> AllMapGenerators = new(32);
	private static readonly Queue<World> ResultQueue = new();
	private static bool Enable;


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-32)]
	internal static void OnGameInitialize () {

		Enable = Universe.BuiltInInfo.UseProceduralMap;
		if (!Enable) return;

		// Create Map Generators
		AllMapGenerators.Clear();
		foreach (var type in typeof(MapGenerator).AllChildClass()) {
			if (System.Activator.CreateInstance(type) is not MapGenerator gen) continue;
			AllMapGenerators.Add(gen);
		}
		AllMapGenerators.Sort((a, b) => a.Order.CompareTo(b.Order));

	}


	[OnGameInitialize]
	[OnSavingSlotChanged]
	internal static void OnGameInitialize_OnSavingSlotChanged () {

		if (!Enable) return;

		// Find all Exist Maps
		StatePool.Clear();
		foreach (string path in Util.EnumerateFiles(Universe.BuiltIn.SlotUserMapRoot, true, $"*.{AngePath.MAP_FILE_EXT}")) {
			if (!WorldPathPool.TryGetWorldPositionFromName(
				Util.GetNameWithoutExtension(path), out var pos
			)) continue;
			StatePool.TryAdd(pos, MapState.Success);
		}

		// Load or Create Seed
		long seed = long.MaxValue;
		string seedPath = Util.CombinePaths(Universe.BuiltIn.SlotUserMapRoot, AngePath.MAP_SEED_NAME);
		if (Util.FileExists(seedPath)) {
			string seedStr = Util.FileToText(seedPath);
			if (!long.TryParse(seedStr, out seed)) {
				seed = int.MaxValue;
			}
		}
		if (seed == long.MaxValue) {
			seed = new System.Random(
				(int)(System.DateTime.Now.Ticks + System.Environment.UserName.AngeHash())
			).NextInt64(long.MinValue, long.MaxValue);
			Util.TextToFile(seed.ToString(), seedPath);
		}
		Seed = seed;

		// Init Generators
		Util.InvokeAllStaticMethodWithAttribute<BeforeAnyMapGeneratorInitializedAttribute>();
		foreach (var gen in AllMapGenerators) {
			gen.Seed = seed;
			gen.Initialize();
		}
		Util.InvokeAllStaticMethodWithAttribute<AfterAllMapGeneratorInitializedAttribute>();

	}


	[OnGameUpdateLater]
	internal static void OnGameUpdateLater () {
		while (ResultQueue.Count > 0) {
			var world = ResultQueue.Dequeue();
			if (world != null) {
				WorldSquad.Stream.AddWorld(world, overrideExists: true);
			}
		}
	}


	#endregion




	#region --- API ---


	public static void RegenerateAll () {
		ResultQueue.Clear();
		WorldSquad.Stream.ClearWorldPool();
		var uni = Universe.BuiltIn;
		// Delete All User Map Files
		foreach (string path in Util.EnumerateFiles(uni.SlotUserMapRoot, true, $"*.{AngePath.MAP_FILE_EXT}")) {
			Util.DeleteFile(path);
		}
		// Reload Saving Slot
		uni.ReloadSavingSlot(uni.CurrentSavingSlot, forceReload: true);
		// Start Game
		Game.RestartGame();
	}


	public static void GenerateMapInRange (IRect overlapRange, int z, bool async) {
		int left = overlapRange.xMin.ToUnit().UDivide(Const.MAP);
		int right = (overlapRange.xMax.ToUnit() + 1).UDivide(Const.MAP);
		int down = overlapRange.yMin.ToUnit().UDivide(Const.MAP);
		int up = (overlapRange.yMax.ToUnit() + 1).UDivide(Const.MAP);
		for (int i = left; i <= right; i++) {
			for (int j = down; j <= up; j++) {
				MapGenerationSystem.GenerateMapAtPosition(new Int3(i, j, z), async);
			}
		}
	}


	public static void GenerateMapAtPosition (Int3 worldPos, bool async) {
		if (!Enable) return;
		if (StatePool.TryGetValue(worldPos, out var state)) {
			if (state != MapState.Fail) return;
		}
		StatePool[worldPos] = MapState.Generating;
		if (async) {
			System.Threading.Tasks.Task.Factory.StartNew(GenerateLogic, worldPos);
		} else {
			GenerateLogic(worldPos);
		}
	}


	public static bool GenerateIntoWorld (Int3 worldPos, World world) => GenerateIntoWorld(worldPos, world, out _);
	public static bool GenerateIntoWorld (Int3 worldPos, World world, out int successCount) {

		bool success = true;
		world.WorldPosition = worldPos;

		// Trigger all Generators
		successCount = 0;
		foreach (var gen in AllMapGenerators) {
			try {
				var result = gen.GenerateMap(worldPos, world);
				switch (result) {
					case MapGenerationResult.Success:
						successCount++;
						break;
					case MapGenerationResult.Fail:
						Debug.LogWarning($"{gen.GetType().Name} Fail to generate map at {worldPos}");
						break;
					case MapGenerationResult.CriticalError:
						success = false;
						Debug.LogError($"{gen.GetType().Name} Fail to generate map at {worldPos} with critical error");
						break;
				}
				// Skip All for Critical Error
				if (!success) break;
			} catch (System.Exception ex) {
				Debug.LogException(ex);
			}
		}

		return success;
	}


	#endregion




	#region --- LGC ---


	private static void GenerateLogic (object worldPosObj) {

		var worldPos = (Int3)worldPosObj;

		// Trigger all Generators
		var world = new World(worldPos);
		bool success = GenerateIntoWorld(worldPos, world, out int successCount);

		// Done
		if (success && successCount > 0) {
			ResultQueue.Enqueue(world);
		}
		StatePool[worldPos] = success ? MapState.Success : MapState.Fail;

	}


	#endregion




}
