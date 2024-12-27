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
	private static readonly object[] OperateParamCache = [null, null, null];
	[OnCircuitWireActived] internal static System.Action<Int3> OnCircuitWireActived;
	[OnCircuitOperatorTriggered] internal static System.Action<Int3> OnCircuitOperatorTriggered;


	#endregion




	#region --- MSG ---


	[OnGameInitialize]
	internal static void OnGameInitialize () {
		// Init Wire Pool
		WireIdPool.Clear();
		foreach (var type in typeof(IWire).AllClassImplemented()) {
			if (System.Activator.CreateInstance(type) is not IWire wire) continue;
			WireIdPool.TryAdd(type.AngeHash(), (wire.ConnectedLeft, wire.ConnectedRight, wire.ConnectedDown, wire.ConnectedUp));
		}
		// Init Operation Pool
		OperatorPool.Clear();
		foreach (var (method, _) in Util.AllStaticMethodWithAttribute<CircuitOperatorAttribute>()) {
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
			if (left) Interate(pos.Shift(-1, 0), stamp, Direction4.Right);
			if (right) Interate(pos.Shift(1, 0), stamp, Direction4.Left);
			if (down) Interate(pos.Shift(0, -1), stamp, Direction4.Up);
			if (up) Interate(pos.Shift(0, 1), stamp, Direction4.Down);
			// Func
			static void Interate (Int3 _pos, int _stamp, Direction4 requireCon) {
				if (TriggeredTaskStamp.TryGetValue(_pos, out int triggeredStamp) && _stamp <= triggeredStamp) return;
				var _squad = WorldSquad.Stream;
				// Check for Wire Expand
				int _id = _squad.GetBlockAt(_pos.x, _pos.y, _pos.z, BlockType.Element);
				if (
					_id != 0 &&
					WireIdPool.TryGetValue(_id, out var connectDirections) &&
					ConnectionValid(requireCon, connectDirections)
				) {
					TriggeredTaskStamp[_pos] = _stamp;
					TriggeringTask.Enqueue((
						_pos,
						connectDirections.left, connectDirections.right,
						connectDirections.down, connectDirections.up,
						_stamp
					));
					OnCircuitWireActived?.Invoke(_pos);
				}
				// Check for Operators
				int entityId = _squad.GetBlockAt(_pos.x, _pos.y, _pos.z, BlockType.Entity);
				if (OperatorPool.TryGetValue(entityId, out var method)) {
					TriggeredTaskStamp[_pos] = _stamp;
					OperateParamCache[0] = WorldSquad.Stream;
					OperateParamCache[1] = _pos;
					var result = method?.Invoke(null, OperateParamCache);
					if (result is not bool bResult || bResult) {
						OnCircuitOperatorTriggered?.Invoke(_pos);
					}
				}
			}
			static bool ConnectionValid (Direction4 requireCon, (bool, bool, bool, bool) wireCons) => requireCon switch {
				Direction4.Left => wireCons.Item1,
				Direction4.Right => wireCons.Item2,
				Direction4.Down => wireCons.Item3,
				Direction4.Up => wireCons.Item4,
				_ => false,
			};
		}

	}


	#endregion




	#region --- API ---


	public static void TriggerCircuit (int unitX, int unitY, int unitZ) {
		int startWireID = WorldSquad.Stream.GetBlockAt(unitX, unitY, unitZ, BlockType.Element);
		if (startWireID == 0 || !WireIdPool.TryGetValue(startWireID, out var startCon)) return;
		var startPos = new Int3(unitX, unitY, unitZ);
		int stamp = Game.PauselessFrame;
		TriggeringTask.Enqueue((
			startPos,
			startCon.left, startCon.right, startCon.down, startCon.up,
			stamp
		));
		TriggeredTaskStamp[startPos] = stamp;
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


	public static bool IsCircuitOperator (int typeID) => OperatorPool.ContainsKey(typeID);


	public static bool IsWire (int typeID) => WireIdPool.ContainsKey(typeID);


	public static int WireConnection_to_BitInt (bool connectL, bool connectR, bool connectD, bool connectU) => (connectL ? 0b1000 : 0b0000) | (connectR ? 0b0100 : 0b0000) | (connectD ? 0b0010 : 0b0000) | (connectU ? 0b0001 : 0b0000);


	#endregion




}
