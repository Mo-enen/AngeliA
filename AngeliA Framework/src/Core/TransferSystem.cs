using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public static class TransferSystem {




	#region --- VAR ---


	// Data
	[OnTransferArrivedAttribute_IntEntityID_Int3UnitPos_ObjectData] internal static System.Action<int, Int3, object> OnTransferArrived;
	[OnTransferPassAttribute_Int3UnitPos_ObjectData] internal static System.Action<Int3, object> OnTransferPass;
	private static readonly Dictionary<int, Direction4> TransferPool = [];
	private static readonly Dictionary<Int3, int> TransferStamp = [];
	private static readonly Queue<(Int3 unitPos, object data, int stamp)> TransferTask = [];


	#endregion




	#region --- MSG ---


	[OnGameInitialize]
	internal static void OnGameInitialize () {
		foreach (var type in typeof(IItemTransfer).AllClassImplemented()) {
			if (System.Activator.CreateInstance(type) is not IItemTransfer ins) continue;
			TransferPool.TryAdd(type.AngeHash(), ins.Direction);
		}
		TransferPool.TrimExcess();
	}


	[OnGameUpdateLater]
	internal static void OnGameUpdateLater () {
		int count = TransferTask.Count;
		for (int i = 0; i < count; i++) {
			var (unitPos, data, stamp) = TransferTask.Dequeue();
			Iterate(unitPos, data, stamp);
		}
	}


	#endregion




	#region --- API ---


	public static bool IsTransfer (int transferID, out Direction4 direction) => TransferPool.TryGetValue(transferID, out direction);


	public static void StartTransfer (Int3 unitPos, object data, int stamp = int.MinValue) {
		OnTransferPass?.Invoke(unitPos, data);
		Iterate(unitPos, data, stamp == int.MinValue ? Game.PauselessFrame : stamp);
	}


	#endregion




	#region --- LGC ---


	private static void Iterate (Int3 unitPos, object data, int stamp) {

		// Arrive at Non-Transfer Block
		int id = WorldSquad.Front.GetBlockAt(unitPos.x, unitPos.y, unitPos.z, BlockType.Entity);
		if (id == 0 || !TransferPool.TryGetValue(id, out var direction)) {
			OnTransferArrived?.Invoke(id, unitPos, data);
			return;
		}

		// Arrive at Transfer
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
