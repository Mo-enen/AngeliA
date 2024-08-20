using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AngeliA;

public static class MapGenerationSystem {




	#region --- SUB ---


	private enum MapState : byte { Generating, Success, Fail, }


	#endregion




	#region --- VAR ---


	// Data
	private static readonly Dictionary<Int3, MapState> StatePool = new();
	private static readonly List<MapGenerator> AllMapGenerators = new(32);
	private static readonly Queue<World> ResultQueue = new();
	private static bool Enable;
	private static long Seed;


	#endregion




	#region --- MSG ---


	[OnGameInitialize]
	[OnSavingSlotChanged]
	internal static void OnGameInitialize_OnSavingSlotChanged () {

		Enable = Game.UseProceduralMap;
		if (!Enable) return;

		StatePool.Clear();

		// Find all Exist Maps
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
		foreach (var gen in AllMapGenerators) {
			gen.Initialize(seed);
		}

	}


	[OnGameInitialize(32)]
	internal static void OnGameInitialize () {

		if (!Enable) return;

		// Init Map Generators
		AllMapGenerators.Clear();
		foreach (var type in typeof(MapGenerator).AllChildClass()) {
			if (System.Activator.CreateInstance(type) is not MapGenerator gen) continue;
			AllMapGenerators.Add(gen);
			gen.Initialize(Seed);
		}
		AllMapGenerators.Sort((a, b) => a.Order.CompareTo(b.Order));

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


	public static void GenerateMapInRange (IRect range, int z, bool async) {
		int left = range.xMin.ToUnit().UDivide(Const.MAP);
		int right = (range.xMax.ToUnit() + 1).UDivide(Const.MAP);
		int down = range.yMin.ToUnit().UDivide(Const.MAP);
		int up = (range.yMax.ToUnit() + 1).UDivide(Const.MAP);
		for (int i = left; i <= right; i++) {
			for (int j = down; j <= up; j++) {
				MapGenerationSystem.GenerateMap(new Int3(i, j, z), async);
			}
		}
	}


	public static void GenerateMap (Int3 worldPos, bool async) {
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


	#endregion




	#region --- LGC ---


	private static void GenerateLogic (object worldPosObj) {

		var worldPos = (Int3)worldPosObj;
		bool success = true;

		// Trigger all Generators
		var world = new World(worldPos);
		int successCount = 0;
		foreach (var gen in AllMapGenerators) {
			try {
				var result = gen.GenerateMap(worldPos, Seed, world);
				switch (result) {
					case MapGenerationResult.Success:
						successCount++;
						break;
					case MapGenerationResult.Fail:
						Debug.LogWarning($"{gen.GetType().Name} Fail to generate map at {worldPos} with error: {gen.ErrorMessage}");
						break;
					case MapGenerationResult.CriticalError:
						success = false;
						Debug.LogError($"{gen.GetType().Name} fail to generate map at {worldPos} with critical error: {gen.ErrorMessage}");
						break;
				}
				// Skip All for Critical Error
				if (!success) break;
			} catch (System.Exception ex) {
				Debug.LogException(ex);
			}
		}

		// Done
		if (success && successCount > 0) {
			ResultQueue.Enqueue(world);
		}
		StatePool[worldPos] = success ? MapState.Success : MapState.Fail;

	}


	#endregion




}
