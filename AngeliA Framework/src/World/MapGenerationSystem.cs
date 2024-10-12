using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace AngeliA;


public static class MapGenerationSystem {




	#region --- SUB ---


	private enum MapState : byte { Generating, Success, Fail, }


	#endregion




	#region --- VAR ---


	// Api
	public static bool Enable { get; private set; }

	// Data
	private static readonly Dictionary<int, MapGenerator> Pool = [];
	private static readonly Dictionary<Int3, MapState> StatePool = [];
	private static readonly Pipe<(MapGenerator gen, Int3 worldPos)> AllTasks = new(64);
	private static readonly System.Random Random = new((int)(System.DateTime.Now.Ticks + System.Environment.UserName.AngeHash()));


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-32)]
	internal static void OnGameInitialize () {

		Enable = Universe.BuiltInInfo.UseProceduralMap && !Game.IsToolApplication;
		if (!Enable) return;

		// Create Map Generators
		Pool.Clear();
		foreach (var type in typeof(MapGenerator).AllChildClass()) {
			if (System.Activator.CreateInstance(type, type.AngeHash()) is not MapGenerator gen) continue;
			Pool.TryAdd(gen.TypeID, gen);
		}

		// Start Async Thread
		System.Threading.Tasks.Task.Run(AsyncUpdate);
		static void AsyncUpdate () {
			while (true) {
				try {
					if (AllTasks.Length == 0) {
						Thread.Sleep(50);
						continue;
					}
					while (AllTasks.TryPopHead(out var param)) {
						GenerateLogic(param.gen, param.worldPos);
					}
				} catch (System.Exception ex) {
					Debug.LogException(ex);
					Thread.Sleep(200);
				}
			}
		}

		// Copy All Built-in Maps into User Map Folder
		string userMapRoot = Universe.BuiltIn.SlotUserMapRoot;
		int fileCount = Util.GetFileCount(userMapRoot, $"*.{AngePath.MAP_FILE_EXT}");
		if (fileCount == 0) {
			Util.CopyFolder(Universe.BuiltIn.MapRoot, userMapRoot, false, true);
		}

	}


	#endregion




	#region --- API ---


	[CheatCode("RecreatedAllMaps")]
	public static void RemoveAllGeneratedMaps () {
		if (!Enable) return;
		AllTasks.Reset();
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


	public static void GenerateMap (int generatorID, Int3 worldPos, bool async) {
		if (!Enable || !Pool.TryGetValue(generatorID, out var gen)) return;
		GenerateMap(gen, worldPos, async);
	}
	public static void GenerateMap (MapGenerator generator, Int3 worldPos, bool async) {
		if (!Enable) return;
		StatePool[worldPos] = MapState.Generating;
		if (async) {
			AllTasks.LinkToTail((generator, worldPos));
		} else {
			GenerateLogic(generator, worldPos);
		}
	}


	public static bool TryGetGenerator (int generatorID, out MapGenerator result) => Pool.TryGetValue(generatorID, out result);


	#endregion




	#region --- LGC ---


	private static void GenerateLogic (MapGenerator generator, Int3 worldPos) {
		bool success = true;
		try {
			generator.ErrorMessage = "";
			generator.Seed = Random.NextInt64(long.MinValue, long.MaxValue);
			var result = generator.GenerateMap(worldPos);
			switch (result) {
				case MapGenerationResult.Success:
				case MapGenerationResult.Skipped:
					success = true;
					break;
				case MapGenerationResult.Fail:
					success = false;
					Debug.LogWarning($"Map generation fail: {generator.ErrorMessage}");
					break;
				case MapGenerationResult.CriticalError:
					success = false;
					Debug.LogError($"Map generation fail with critical error: {generator.ErrorMessage}");
					break;
			}
		} catch (System.Exception ex) {
			Debug.LogException(ex);
			success = false;
		}

		// Finish
		StatePool[worldPos] = success ? MapState.Success : MapState.Fail;
	}


	#endregion




}
