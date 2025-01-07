using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public static class TransferSystem {




	#region --- VAR ---


	// Data
	[OnTransferArrivedAttribute_IntEntityID_Int3UnitPos_ObjectData] internal static System.Action<int, Int3, object> OnTransferArrived;
	[OnTransferPassAttribute_Int3UnitPos_ObjectData] internal static System.Action<Int3, object> OnTransferPass;
	private static readonly Dictionary<int, Direction4> PipePool = [];
	private static readonly Dictionary<Int3, int> TransferStamp = [];
	private static readonly Queue<(Int3 unitPos, object data, int stamp)> TransferTask = [];


	#endregion




	#region --- MSG ---


	[OnGameUpdateLater]
	internal static void OnGameUpdateLater () {
		int count = TransferTask.Count;
		var stream = WorldSquad.Stream;
		for (int i = 0; i < count; i++) {
			var (unitPos, data, stamp) = TransferTask.Dequeue();
			Iterate(stream, unitPos, data, stamp);
		}
	}


	#endregion




	#region --- API ---


	public static void RegisterPipe (int pipeID, Direction4 direction) => PipePool.TryAdd(pipeID, direction);


	public static bool IsPipe (int pipeID) => PipePool.ContainsKey(pipeID);


	public static void StartTransfer (Int3 unitPos, object data, int stamp = int.MinValue) => Iterate(WorldSquad.Stream, unitPos, data, stamp == int.MinValue ? Game.PauselessFrame : stamp);


	#endregion




	#region --- LGC ---


	private static void Iterate (WorldStream stream, Int3 unitPos, object data, int stamp) {

		// Arrive at Non-Pipe Block
		int id = stream.GetBlockAt(unitPos.x, unitPos.y, unitPos.z, BlockType.Entity);
		if (id == 0 || !PipePool.TryGetValue(id, out var direction)) {
			OnTransferArrived?.Invoke(id, unitPos, data);
			return;
		}

		// Arrive at Pipe
		var normal = direction.Normal();
		var nextPos = unitPos.Shift(normal.x, normal.y);
		if (!TransferStamp.TryGetValue(nextPos, out int nextStamp) || stamp > nextStamp) {
			TransferStamp[nextPos] = stamp;
			TransferTask.Enqueue((nextPos, data, stamp));
			OnTransferPass?.Invoke(nextPos, data);
		}

	}


	#endregion




}
