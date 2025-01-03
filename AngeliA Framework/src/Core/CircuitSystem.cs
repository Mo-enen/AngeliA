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
	private static readonly object[] OperateParamCache = [null, 0];
	[OnCircuitWireActived_Int3UnitPos] internal static System.Action<Int3> OnCircuitWireActived;
	[OnCircuitOperatorTriggered_Int3UnitPos] internal static System.Action<Int3> OnCircuitOperatorTriggered;


	#endregion




	#region --- MSG ---


	[OnGameInitialize]
	internal static void OnGameInitialize () {
		// Init Operator Pool
		OperatorPool.Clear();
		foreach (var (method, _) in Util.AllStaticMethodWithAttribute<CircuitOperator_Int3UnitPos_IntStampAttribute>()) {
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
				for (int bTypeIndex = 0; bTypeIndex < 4; bTypeIndex++) {
					int _id = _squad.GetBlockAt(_pos.x, _pos.y, _pos.z, (BlockType)bTypeIndex);
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
				}

				// Check for Operators
				int entityId = _squad.GetBlockAt(_pos.x, _pos.y, _pos.z, BlockType.Entity);
				if (entityId != 0 && OperatorPool.TryGetValue(entityId, out var method)) {
					TriggeredTaskStamp[_pos] = _stamp;
					OperateParamCache[0] = _pos;
					OperateParamCache[1] = _stamp;
					var result = method?.Invoke(null, OperateParamCache);
					if (result is not bool bResult || bResult) {
						OnCircuitOperatorTriggered?.Invoke(_pos);
					}
				}
				if (_pos.z == Stage.ViewZ && Physics.GetEntity<ICircuitOperator>(
						IRect.Point(_pos.x.ToGlobal() + Const.HALF, _pos.y.ToGlobal() + Const.HALF),
						PhysicsMask.ENTITY, null, OperationMode.ColliderAndTrigger
					) is ICircuitOperator _operator
				) {
					_operator.TriggerCircuit();
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


	public static void RegisterWire (int id, bool connectL, bool connectR, bool connectD, bool connectU) => WireIdPool[id] = (connectL, connectR, connectD, connectU);


	public static bool IsWire (int typeID) => WireIdPool.ContainsKey(typeID);


	public static void TriggerCircuit (int unitX, int unitY, int unitZ, int stamp = int.MinValue) {
		for (int bTypeIndex = 0; bTypeIndex < 4; bTypeIndex++) {
			int startWireID = WorldSquad.Stream.GetBlockAt(unitX, unitY, unitZ, (BlockType)bTypeIndex);
			if (startWireID == 0 || !WireIdPool.TryGetValue(startWireID, out var startCon)) continue;
			var startPos = new Int3(unitX, unitY, unitZ);
			stamp = stamp == int.MinValue ? Game.PauselessFrame : stamp;
			TriggeringTask.Enqueue((
				startPos,
				startCon.left, startCon.right, startCon.down, startCon.up,
				stamp
			));
			TriggeredTaskStamp[startPos] = stamp;
			break;
		}
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


	#endregion




}
