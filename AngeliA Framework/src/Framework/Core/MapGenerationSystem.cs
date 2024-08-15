using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AngeliA;

public static class MapGenerationSystem {




	#region --- SUB ---


	private enum MapState : byte { InQueue, Generating, Success, Fail, Founded, }


	private class Agent {

		public bool IsReady => Task == null || Task.IsCompleted;
		private System.Threading.Tasks.Task Task;
		private IBlockSquad Squad { get; init; }
		private Int3 WorldPos;

		public Agent (IBlockSquad squad) => Squad = squad;

		public void StartGenerate (Int3 worldPos) {
			if (!IsReady) return;
			if (Task != null) {
				Task.Dispose();
				Task = null;
			}
			WorldPos = worldPos;
			Task = System.Threading.Tasks.Task.Run(GenerateLogic);
		}

		private void GenerateLogic () {
			bool success = true;
			// Trigger all Generators
			foreach (var gen in AllMapGenerators) {
				try {
					var result = gen.GenerateMap(WorldPos, Squad, Seed);
					if (result == MapGenerationResult.CriticalError) {
						success = false;
						break;
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
			}
			// Done
			OnAgentComplete(WorldPos, success);
		}

	}


	#endregion




	#region --- VAR ---


	// Data
	private static readonly Agent[] CommonAgents = new Agent[16];
	private static readonly Agent[] EmergencyAgents = new Agent[4];
	private static readonly Queue<Int3> RequirementQueue = new(32);
	private static readonly Queue<Int3> EmergencyRequirementQueue = new(32);
	private static readonly Dictionary<Int3, MapState> StatePool = new();
	private static readonly List<MapGenerator> AllMapGenerators = new(32);
	private static bool Enable;
	private static int Seed;


	#endregion




	#region --- MSG ---


	[OnGameInitialize]
	[OnSavingSlotChanged]
	internal static void OnGameInitialize () {

		Enable = Universe.BuiltIn.Info.UseProceduralMap;
		if (!Enable) return;

		RequirementQueue.Clear();
		EmergencyRequirementQueue.Clear();
		StatePool.Clear();
		var stream = WorldStream.GetOrCreateStreamFromPool(Universe.BuiltIn.SlotUserMapRoot);

		// Find all Exist Maps
		foreach (string path in Util.EnumerateFiles(stream.MapRoot, true, $"*.{AngePath.MAP_FILE_EXT}")) {
			if (!WorldPathPool.TryGetWorldPositionFromName(
				Util.GetNameWithoutExtension(path), out var pos
			)) continue;
			StatePool.TryAdd(pos, MapState.Founded);
		}

		// Init Agents
		for (int i = 0; i < CommonAgents.Length; i++) {
			CommonAgents[i] = new Agent(stream);
		}
		for (int i = 0; i < EmergencyAgents.Length; i++) {
			EmergencyAgents[i] = new Agent(stream);
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
	internal static void OnGameInitializeOnly () {

		if (!Enable) return;

		// Init Map Generators
		AllMapGenerators.Clear();
		foreach (var type in typeof(MapGenerator).AllChildClass()) {
			if (System.Activator.CreateInstance(type) is not MapGenerator gen) continue;
			AllMapGenerators.Add(gen);
		}

	}


	[OnGameUpdate]
	internal static void OnGameUpdate () {

		if (!Enable) return;

		// Check for Requirement
		while (RequirementQueue.Count > 0) {
			var worldPos = RequirementQueue.Dequeue();
			if (!TryStartGenerationWithAgent(worldPos, false)) {
				RequirementQueue.Enqueue(worldPos);
				break;
			}
		}
		while (EmergencyRequirementQueue.Count > 0) {
			var worldPos = EmergencyRequirementQueue.Dequeue();
			if (!TryStartGenerationWithAgent(worldPos, true)) {
				EmergencyRequirementQueue.Enqueue(worldPos);
				break;
			}
		}
	}


	#endregion




	#region --- API ---


	public static void RequireGenerateMapAt (Int3 worldPos, bool emergency = false) {

		if (!Enable) return;

		// Gate
		if (StatePool.TryGetValue(worldPos, out var state)) {
			if (state != MapState.Fail) return;
		}

		// Try Start Generation
		if (TryStartGenerationWithAgent(worldPos, emergency)) return;

		// No Agent Available
		state = MapState.InQueue;
		StatePool[worldPos] = state;
		if (emergency) {
			EmergencyRequirementQueue.Enqueue(worldPos);
		} else {
			RequirementQueue.Enqueue(worldPos);
		}

	}


	#endregion




	#region --- LGC ---


	private static bool TryStartGenerationWithAgent (Int3 worldPos, bool emergency) {
		var agents = emergency ? EmergencyAgents : CommonAgents;
		int len = agents.Length;
		for (int i = 0; i < len; i++) {
			var agent = agents[i];
			if (!agent.IsReady) continue;
			// Available Agent Found
			StatePool[worldPos] = MapState.Generating;
			agent.StartGenerate(worldPos);
			return true;
		}
		return false;
	}


	private static void OnAgentComplete (Int3 worldPos, bool success) => StatePool[worldPos] = success ? MapState.Success : MapState.Fail;


	#endregion




}
