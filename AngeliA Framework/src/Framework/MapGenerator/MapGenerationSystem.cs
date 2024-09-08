using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AngeliA;


public static class MapGenerationSystem {




	#region --- SUB ---


	private enum MapState : byte { Generating, Success, Fail, }


	#endregion




	#region --- VAR ---


	// Api
	public static bool Enable { get; private set; }

	// Data
	private static readonly MapGenerator[] PrioritizedMapGenerators = new MapGenerator[1024];
	private static readonly Dictionary<Int3, MapState> StatePool = new();
	private static readonly Pipe<(MapGenerator gen, Int3 point, Direction8? dir)> AllTasks = new(64);
	private static readonly System.Random Random = new((int)(System.DateTime.Now.Ticks + System.Environment.UserName.AngeHash()));


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-32)]
	internal static void OnGameInitialize () {

		Enable = Universe.BuiltInInfo.UseProceduralMap && !Game.IsToolApplication;
		if (!Enable) return;

		// Create Map Generators
		float totalPriority = 0f;
		var genList = new List<MapGenerator>();
		foreach (var type in typeof(MapGenerator).AllChildClass()) {
			if (System.Activator.CreateInstance(type) is not MapGenerator gen) continue;
			if (!gen.IncludeInOpenWorld) continue;
			genList.Add(gen);
			totalPriority += Util.Max(gen.Priority, 0.001f);
		}

		// Prioritize
		int pGenratorLength = PrioritizedMapGenerators.Length;
		if (genList.Count > 0) {
			MapGenerator currentGen = null;
			int endIndex = 0;
			for (int i = 0; i < genList.Count; i++) {
				if (endIndex >= pGenratorLength) break;
				currentGen = genList[i];
				int currentCount = Util.Max((int)(pGenratorLength * currentGen.Priority / totalPriority), 1);
				int startIndex = endIndex;
				endIndex = (startIndex + currentCount).Clamp(startIndex, pGenratorLength);
				for (int j = startIndex; j < endIndex; j++) {
					PrioritizedMapGenerators[j] = currentGen;
				}
			}
			for (int j = endIndex; j < pGenratorLength; j++) {
				PrioritizedMapGenerators[j] = currentGen;
			}
		} else {
			var failback = new FailbackMapGenerator();
			for (int i = 0; i < pGenratorLength; i++) {
				PrioritizedMapGenerators[i] = failback;
			}
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
						GenerateLogic(param.gen, param.point, param.dir);
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
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
	public static void RegenerateAll () {
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


	public static void GenerateMap (Int3 startPoint, Direction8? startDirection, bool async) {
		if (!Enable) return;
		int randomIndex = Random.Next(0, PrioritizedMapGenerators.Length);
		var gen = PrioritizedMapGenerators[randomIndex];
		GenerateMapWithGenerator(gen, startPoint, startDirection, async);
	}


	public static void GenerateMapWithGenerator (MapGenerator generator, Int3 startPoint, Direction8? startDirection, bool async) {
		if (!Enable) return;
		StatePool[startPoint] = MapState.Generating;
		if (async) {
			AllTasks.LinkToTail((generator, startPoint, startDirection));
		} else {
			GenerateLogic(generator, startPoint, startDirection);
		}
	}


	#endregion




	#region --- LGC ---


	private static void GenerateLogic (MapGenerator generator, Int3 startPoint, Direction8? startDirection) {
		bool success = true;
		try {
			generator.ErrorMessage = "";
			generator.Seed = Random.NextInt64(long.MinValue, long.MaxValue);
			var result = generator.GenerateMap(WorldSquad.Stream, startPoint, startDirection);
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
		WorldSquad.Stream.SetBlockAt(startPoint.x, startPoint.y, startPoint.z, BlockType.Element, 0);
		StatePool[startPoint] = success ? MapState.Success : MapState.Fail;
	}


	#endregion




}
