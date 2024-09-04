using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AngeliA;


public static class MapGenerationSystem {




	#region --- SUB ---


	private enum MapState : byte { Generating, Success, Fail, }


	#endregion




	#region --- VAR ---


	// Const
	public const int STARTER_ID = 72352367;

	// Api
	public static long Seed { get; private set; }

	// Data
	private static readonly Dictionary<Int3, MapState> StatePool = new();
	private static readonly List<MapGenerator> AllMapGenerators = new(32);
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
	[OnSavingSlotChanged(1)]
	internal static void OnGameInitialize_OnSavingSlotChanged () {

		if (!Enable) return;

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
			gen.Seed = seed;
			gen.Squad = WorldSquad.Stream;
			gen.Initialize();
		}

	}


	#endregion




	#region --- API ---


	public static void RegenerateAll () {
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


	public static bool IsGenerating (Int3 startPoint) => StatePool.TryGetValue(startPoint, out var state) && state == MapState.Generating;


	public static void GenerateMap (Int3 startPoint, bool async) => GenerateMap(startPoint, (Direction8)Util.QuickRandomWithSeed(Seed + startPoint.x * 2736731 + startPoint.y * 7315127 + startPoint.z * 41, 0, 8), async);
	public static void GenerateMap (Int3 startPoint, Direction8 startDirection, bool async) {
		StatePool[startPoint] = MapState.Generating;
		if (async) {
			System.Threading.Tasks.Task.Factory.StartNew(GenerateLogic, (startPoint, startDirection));
		} else {
			GenerateLogic((startPoint, startDirection));
		}
	}


	#endregion




	#region --- LGC ---


	private static void GenerateLogic (object param) {
		(Int3 startPoint, Direction8 startDirection) = ((Int3, Direction8))param;
		int len = AllMapGenerators.Count;
		int successCount = 0;
		for (int i = 0; i < len; i++) {
			try {
				var gen = AllMapGenerators[i];
				gen.ErrorMessage = "";
				var result = gen.GenerateMap(startPoint, startDirection);
				switch (result) {
					case MapGenerationResult.Success:
						successCount++;
						break;
					case MapGenerationResult.Skipped:
						break;
					case MapGenerationResult.Fail:
						Debug.LogWarning($"Map generation fail: {gen.ErrorMessage}");
						break;
					case MapGenerationResult.CriticalError:
						Debug.LogError($"Map generation fail with critical error: {gen.ErrorMessage}");
						break;
				}
				gen.ErrorMessage = "";
				if (result == MapGenerationResult.CriticalError) {
					successCount = -1;
					break;
				}
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}
		WorldSquad.Stream.SetBlockAt(startPoint.x, startPoint.y, startPoint.z, BlockType.Element, 0);
		StatePool[startPoint] = successCount >= 0 ? MapState.Success : MapState.Fail;
	}


	#endregion




}
