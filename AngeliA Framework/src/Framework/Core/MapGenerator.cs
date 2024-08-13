using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public static class MapGenerator {




	#region --- SUB ---


	private enum MapState : byte { InQueue, Generating, Success, Fail, Founded, }


	private class GeneratorAgent {

		// Api
		public bool IsWorking => !Task.IsCompleted;
		public System.Threading.Tasks.Task Task { get; init; }
		public Int3 WorldPos { get; set; }

		// Data
		private IBlockSquad Squad { get; init; }

		// API
		public GeneratorAgent (IBlockSquad squad) {
			Squad = squad;
			Task = new(GenerateLogic);
		}

		public void StartGenerate () {
			if (Task.IsCompleted) {
				Task.RunSynchronously();
			}
		}

		// LGC
		private void GenerateLogic () {
			bool success;
			try {







				success = true;
			} catch (System.Exception ex) {
				Debug.LogException(ex);
				success = false;
			}
			// Done
			OnAgentComplete(WorldPos, success);
		}

	}


	#endregion




	#region --- VAR ---


	// Data
	private static bool Enable;
	private static readonly GeneratorAgent[] CommonAgents = new GeneratorAgent[16];
	private static readonly GeneratorAgent[] EmergencyAgents = new GeneratorAgent[4];
	private static readonly Queue<Int3> RequirementQueue = new(32);
	private static readonly Queue<Int3> EmergencyRequirementQueue = new(32);
	private static readonly Dictionary<Int3, MapState> StatePool = new();


	#endregion




	#region --- MSG ---


	[OnGameInitialize]
	internal static void OnGameInitialize () {

		Enable = Universe.BuiltIn.Info.UseProceduralMap;
		if (!Enable) return;
		var stream = WorldStream.GetOrCreateStreamFromPool(Universe.BuiltIn.UserMapRoot);

		// Find all Exist Maps
		foreach (string path in Util.EnumerateFiles(stream.MapRoot, true, $"*.{AngePath.MAP_FILE_EXT}")) {
			if (!WorldPathPool.TryGetWorldPositionFromName(
				Util.GetNameWithoutExtension(path), out var pos
			)) continue;
			StatePool.TryAdd(pos, MapState.Founded);
		}

		// Init Agents
		for (int i = 0; i < CommonAgents.Length; i++) {
			CommonAgents[i] = new(stream);
		}
		for (int i = 0; i < EmergencyAgents.Length; i++) {
			EmergencyAgents[i] = new(stream);
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
			if (agent.IsWorking) continue;
			// Available Agent Found
			StatePool[worldPos] = MapState.Generating;
			agent.WorldPos = worldPos;
			agent.StartGenerate();
			return true;
		}
		return false;
	}


	private static void OnAgentComplete (Int3 worldPos, bool success) => StatePool[worldPos] = success ? MapState.Success : MapState.Fail;


	#endregion




}
