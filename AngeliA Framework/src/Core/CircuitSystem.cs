using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace AngeliA;


public static class CircuitSystem {




	#region --- VAR ---


	// Data
	private static readonly Dictionary<int, (bool left, bool right, bool down, bool up)> WireIdPool = [];
	private static readonly Queue<(Int3 pos, bool left, bool right, bool down, bool up, int stamp)> TriggeringTask = [];
	private static readonly Dictionary<Int3, int> TriggeredTaskStamp = [];
	private static readonly Dictionary<int, MethodInfo> OperatorPool = [];
	private static readonly object[] OperateParamCache = [null, 0, Direction4.Down];
	[OnCircuitWireActived_Int3UnitPos] internal static System.Action<Int3> OnCircuitWireActived;
	[OnCircuitOperatorTriggered_Int3UnitPos] internal static System.Action<Int3> OnCircuitOperatorTriggered;


	#endregion




	#region --- MSG ---


	[OnGameInitialize]
	internal static void OnGameInitialize () {
		// Init Operator Pool
		OperatorPool.Clear();
		foreach (var (method, _) in Util.AllStaticMethodWithAttribute<CircuitOperator_Int3UnitPos_IntStamp_Direction5From>()) {
			if (method.DeclaringType == null) continue;
			var type = method.DeclaringType;
			if (type.IsAbstract) {
				foreach (var _type in type.AllChildClass()) {
					OperatorPool.Add(_type.AngeHash(), method);
				}
			} else {
				OperatorPool.Add(type.AngeHash(), method);
			}
		}

	}


	[OnGameUpdateLater]
	internal static void OnGameUpdateLater () {
		int targetCount = TriggeringTask.Count;
		for (int i = 0; i < targetCount; i++) {
			var (pos, left, right, down, up, stamp) = TriggeringTask.Dequeue();
			if (left) Interate(pos.Shift(-1, 0), stamp, Direction5.Right);
			if (right) Interate(pos.Shift(1, 0), stamp, Direction5.Left);
			if (down) Interate(pos.Shift(0, -1), stamp, Direction5.Up);
			if (up) Interate(pos.Shift(0, 1), stamp, Direction5.Down);
		}
	}


	#endregion




	#region --- API ---


	public static void RegisterWire (int id, bool connectL, bool connectR, bool connectD, bool connectU) => WireIdPool[id] = (connectL, connectR, connectD, connectU);


	public static bool IsWire (int typeID) => WireIdPool.ContainsKey(typeID);


	public static void TriggerCircuit (int unitX, int unitY, int unitZ, int stamp = int.MinValue, Direction5 startDirection = Direction5.Center) {
		stamp = stamp == int.MinValue ? Game.PauselessFrame : stamp;
		var startPos = new Int3(unitX, unitY, unitZ);
		Interate(startPos, stamp, startDirection.Opposite());
	}


	public static bool WireEntityID_to_WireConnection (int id, out bool connectL, out bool connectR, out bool connectD, out bool connectU) {
		if (WireIdPool.TryGetValue(id, out var connect)) {
			connectL = connect.left;
			connectR = connect.right;
			connectD = connect.down;
			connectU = connect.up;
			return true;
		} else {
			connectL = false;
			connectR = false;
			connectD = false;
			connectU = false;
			return false;
		}
	}


	public static bool IsCircuitOperator (int typeID) => OperatorPool.ContainsKey(typeID) || ICircuitOperator.IsOperator(typeID);


	public static int WireConnection_to_BitInt (bool connectL, bool connectR, bool connectD, bool connectU) => (connectL ? 0b1000 : 0b0000) | (connectR ? 0b0100 : 0b0000) | (connectD ? 0b0010 : 0b0000) | (connectU ? 0b0001 : 0b0000);


	public static void BitInt_to_WireConnection (int bitInt, out bool connectL, out bool connectR, out bool connectD, out bool connectU) {
		connectL = (bitInt & 0b1000) != 0;
		connectR = (bitInt & 0b0100) != 0;
		connectD = (bitInt & 0b0010) != 0;
		connectU = (bitInt & 0b0001) != 0;
	}


	public static int GetStamp (Int3 unitPos) => TriggeredTaskStamp.TryGetValue(unitPos, out var pos) ? pos : -1;


	#endregion



	#region --- LGC ---


	private static void Interate (Int3 pos, int stamp, Direction5 circuitFrom) {

		if (TriggeredTaskStamp.TryGetValue(pos, out int triggeredStamp) && stamp <= triggeredStamp) return;
		var _squad = WorldSquad.Stream;

		// Check for Wire Expand
		for (int bTypeIndex = 0; bTypeIndex < 4; bTypeIndex++) {
			int _id = _squad.GetBlockAt(pos.x, pos.y, pos.z, (BlockType)bTypeIndex);
			if (
				_id != 0 &&
				WireIdPool.TryGetValue(_id, out var connectDirections) &&
				(circuitFrom == Direction5.Center || ConnectionValid(circuitFrom, connectDirections))
			) {
				TriggeredTaskStamp[pos] = stamp;
				TriggeringTask.Enqueue((
					pos,
					connectDirections.left, connectDirections.right,
					connectDirections.down, connectDirections.up,
					stamp
				));
				OnCircuitWireActived?.Invoke(pos);
			}
		}

		// Check Block Operator
		int entityId = _squad.GetBlockAt(pos.x, pos.y, pos.z, BlockType.Entity);
		if (entityId != 0 && OperatorPool.TryGetValue(entityId, out var method)) {
			TriggeredTaskStamp[pos] = stamp;
			OperateParamCache[0] = pos;
			OperateParamCache[1] = stamp;
			OperateParamCache[2] = circuitFrom;
			var result = method?.Invoke(null, OperateParamCache);
			if (result is not bool bResult || bResult) {
				OnCircuitOperatorTriggered?.Invoke(pos);
			}
		}

		// Check Staged Operator
		if (pos.z == Stage.ViewZ && Physics.GetEntity<ICircuitOperator>(
				IRect.Point(pos.x.ToGlobal() + Const.HALF, pos.y.ToGlobal() + Const.HALF),
				PhysicsMask.ENTITY, null, OperationMode.ColliderAndTrigger
			) is ICircuitOperator _operator
		) {
			_operator.TriggerCircuit();
		}
	}


	private static bool ConnectionValid (Direction5 requireCon, (bool, bool, bool, bool) wireCons) => requireCon switch {
		Direction5.Left => wireCons.Item1,
		Direction5.Right => wireCons.Item2,
		Direction5.Down => wireCons.Item3,
		Direction5.Up => wireCons.Item4,
		_ => false,
	};


	#endregion




}
