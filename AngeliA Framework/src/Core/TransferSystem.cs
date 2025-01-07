using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public static class TransferSystem {




	#region --- VAR ---


	// Data
	[OnTransferArrivedAttribute_Int3UnitPos_ObjectData] internal static System.Action<Int3, object> OnTransferArrived;
	private static readonly Dictionary<int, Direction4> PipePool = [];
	private static readonly Dictionary<Int3, int> TransferStamp = [];
	private static readonly Queue<(Int3 unitPos, object data)> TransferTask = [];


	#endregion




	#region --- MSG ---


	[OnGameUpdateLater]
	internal static void OnGameUpdateLater () {
		int count = TransferTask.Count;
		var stream = WorldSquad.Stream;
		for (int i = 0; i < count; i++) {
			var (unitPos, data) = TransferTask.Dequeue();
			Iterate(stream, unitPos, data);
		}
	}


	#endregion




	#region --- API ---


	public static void RegisterPipe (int pipeID, Direction4 direction) => PipePool.TryAdd(pipeID, direction);


	public static bool IsPipe (int pipeID) => PipePool.ContainsKey(pipeID);


	public static void StartTransfer (Int3 unitPos, object data) => Iterate(WorldSquad.Stream, unitPos, data);


	#endregion




	#region --- LGC ---


	private static void Iterate (WorldStream stream, Int3 unitPos, object data) {









	}


	#endregion




}
