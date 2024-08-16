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
	private static WorldStream Stream;
	private static bool Enable;
	private static int Seed;


	#endregion




	#region --- MSG ---


	[OnGameInitialize]
	[OnSavingSlotChanged]
	internal static void OnGameInitialize_OnSavingSlotChanged () {

		Enable = Game.UseProceduralMap;
		if (!Enable) return;

		StatePool.Clear();
		Stream = WorldStream.GetOrCreateStreamFromPool(Universe.BuiltIn.SlotUserMapRoot);

		// Find all Exist Maps
		foreach (string path in Util.EnumerateFiles(Stream.MapRoot, true, $"*.{AngePath.MAP_FILE_EXT}")) {
			if (!WorldPathPool.TryGetWorldPositionFromName(
				Util.GetNameWithoutExtension(path), out var pos
			)) continue;
			StatePool.TryAdd(pos, MapState.Success);
		}

		// Load or Create Seed
		int seed = int.MaxValue;
		string seedPath = Util.CombinePaths(Universe.BuiltIn.SlotUserMapRoot, AngePath.MAP_SEED_NAME);
		if (Util.FileExists(seedPath)) {
			string seedStr = Util.FileToText(seedPath);
			if (!int.TryParse(seedStr, out seed)) {
				seed = int.MaxValue;
			}
		}
		if (seed == int.MaxValue) {
			seed = new System.Random(
				(int)(System.DateTime.Now.Ticks + System.Environment.UserName.AngeHash())
			).Next(int.MinValue, int.MaxValue);
			Util.TextToFile(seed.ToString(), seedPath);
		}
		Seed = seed;

	}


	[OnGameInitialize(32)]
	internal static void OnGameInitialize () {

		if (!Enable) return;

		// Init Map Generators
		AllMapGenerators.Clear();
		foreach (var type in typeof(MapGenerator).AllChildClass()) {
			if (System.Activator.CreateInstance(type) is not MapGenerator gen) continue;
			AllMapGenerators.Add(gen);
		}
		AllMapGenerators.Sort((a, b) => a.Order.CompareTo(b.Order));

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
		foreach (var gen in AllMapGenerators) {
			try {
				var result = gen.GenerateMap(worldPos, Stream, Seed);
				if (result == MapGenerationResult.CriticalError) {
					success = false;
					Debug.LogError($"{gen.GetType().Name} fail to generate map at {worldPos} with critical error: {gen.ErrorMessage}");
					break;
				} else if (result == MapGenerationResult.Fail) {
					Debug.LogWarning($"{gen.GetType().Name} Fail to generate map at {worldPos} with error: {gen.ErrorMessage}");
				}
			} catch (System.Exception ex) {
				Debug.LogException(ex);
			}
		}

		// Done
		StatePool[worldPos] = success ? MapState.Success : MapState.Fail;

	}


	#endregion




}
