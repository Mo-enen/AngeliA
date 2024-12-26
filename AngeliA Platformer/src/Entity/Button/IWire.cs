using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using AngeliA;

namespace AngeliA.Platformer;

public interface IWire {


	// VAR
	private static readonly Dictionary<int, (bool left, bool right, bool down, bool up)> WireIdPool = [];
	private static readonly int[] WireConnectionPool = new int[16];
	private static readonly Dictionary<int, MethodInfo> OperatorPool = [];
	private static readonly Queue<(Int2 pos, int bit, int step)> TriggerTaskQueue = [];
	private static readonly HashSet<Int2> TriggerTaskHash = [];
	private static readonly Queue<(MethodInfo method, int frame, Int3 opUnitPos, Int3 btnUnitPos)> OperationRequest = [];
	private static readonly object[] OperateParamCache = [null, null, null];
	[OnCircuitWireActived] internal static System.Action<IBlockSquad, Int3, int> OnCircuitWireActived;
	[OnCircuitOperatorTriggered] internal static System.Action<IBlockSquad, Int3> OnCircuitOperatorTriggered;

	public bool ConnectedLeft { get; }
	public bool ConnectedRight { get; }
	public bool ConnectedDown { get; }
	public bool ConnectedUp { get; }


	// MSG
	[OnGameInitialize]
	internal static void OnGameInitialize () {
		// Init Wire Pool
		WireIdPool.Clear();
		foreach (var type in typeof(IWire).AllClassImplemented()) {
			if (System.Activator.CreateInstance(type) is not IWire wire) continue;
			int id = type.AngeHash();
			bool added = WireIdPool.TryAdd(id, (wire.ConnectedLeft, wire.ConnectedRight, wire.ConnectedDown, wire.ConnectedUp));
			if (added) {
				int bitInt = WireConnection_to_BitInt(wire.ConnectedLeft, wire.ConnectedRight, wire.ConnectedDown, wire.ConnectedUp);
				WireConnectionPool[bitInt] = id;
			}
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
	internal static void PerformOperation () {
		if (OperationRequest.Count == 0) return;
		int maxCount = OperationRequest.Count;
		while (maxCount > 0 && OperationRequest.TryPeek(out var peek)) {
			// If anything added into RequestQueue duration this loop,
			// added ones will NOT be operate in this loop,
			// and still will be keep for next game frame
			if (peek.frame > Game.GlobalFrame) break;
			OperationRequest.Dequeue();
			OperateParamCache[0] = WorldSquad.Stream;
			OperateParamCache[1] = peek.opUnitPos;
			OperateParamCache[2] = peek.btnUnitPos;
			var result = peek.method?.Invoke(null, OperateParamCache);
			if (result is not bool bResult || bResult) {
				OnCircuitOperatorTriggered?.Invoke(WorldSquad.Stream, peek.opUnitPos);
			}
			maxCount--;
		}
	}


	// API
	public static void TriggerCircuit (IBlockSquad squad, int unitX, int unitY, int unitZ, int maxUnitDistance) {
		TriggerTaskQueue.Clear();
		TriggerTaskHash.Clear();
		var startPos = new Int2(unitX, unitY);
		var startPos3 = new Int3(unitX, unitY, unitZ);
		int startWireID = squad.GetBlockAt(unitX, unitY, unitZ, BlockType.Element);
		if (startWireID == 0 || !WireIdPool.TryGetValue(startWireID, out var startCon)) return;
		TriggerTaskQueue.Enqueue((
			startPos,
			WireConnection_to_BitInt(startCon.left, startCon.right, startCon.down, startCon.up),
			0
		));
		TriggerTaskHash.Add(startPos);
		while (TriggerTaskQueue.TryDequeue(out var taskPair)) {
			var (pos, bit, taskStep) = taskPair;
			int id = squad.GetBlockAt(pos.x, pos.y, unitZ, BlockType.Element);
			if (id == 0) continue;
			// L
			if (pos.x > startPos.x - maxUnitDistance && CheckConnectWithBitInt(bit, Direction4.Left)) {
				Interate(squad, pos.ShiftX(-1), startPos3, unitZ, taskStep + 1, 0b0100);
			}
			// R
			if (pos.x < startPos.x + maxUnitDistance && CheckConnectWithBitInt(bit, Direction4.Right)) {
				Interate(squad, pos.ShiftX(1), startPos3, unitZ, taskStep + 1, 0b1000);
			}
			// D
			if (pos.y > startPos.y - maxUnitDistance && CheckConnectWithBitInt(bit, Direction4.Down)) {
				Interate(squad, pos.ShiftY(-1), startPos3, unitZ, taskStep + 1, 0b0001);
			}
			// U
			if (pos.y < startPos.y + maxUnitDistance && CheckConnectWithBitInt(bit, Direction4.Up)) {
				Interate(squad, pos.ShiftY(1), startPos3, unitZ, taskStep + 1, 0b0010);
			}
			// Func
			static void Interate (IBlockSquad _squad, Int2 _pos, Int3 _startPos, int _z, int step, int requireConnectDir) {
				if (TriggerTaskHash.Contains(_pos)) return;
				TriggerTaskHash.Add(_pos);
				var _pos3 = new Int3(_pos.x, _pos.y, _z);
				// Check for Btn Operators
				int entityId = _squad.GetBlockAt(_pos.x, _pos.y, _z, BlockType.Entity);
				if (OperatorPool.TryGetValue(entityId, out var method)) {
					OperationRequest.Enqueue((method, Game.GlobalFrame + step, _pos3, _startPos));
				}
				// Check for Wire Expand
				int _id = _squad.GetBlockAt(_pos.x, _pos.y, _z, BlockType.Element);
				if (_id != 0) {
					if (WireIdPool.TryGetValue(_id, out var connectDirections)) {
						int connectInt = WireConnection_to_BitInt(connectDirections.left, connectDirections.right, connectDirections.down, connectDirections.up);
						if ((requireConnectDir & connectInt) != 0) {
							TriggerTaskQueue.Enqueue((_pos, connectInt, step));
							OnCircuitWireActived?.Invoke(_squad, _pos3, step);
						}
					}
				}
			}
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


	public static int WireConnection_to_WireEntityID (bool connectL, bool connectR, bool connectD, bool connectU) => WireConnectionPool[WireConnection_to_BitInt(connectL, connectR, connectD, connectU)];


	public static bool IsCircuitOperator (int typeID) => OperatorPool.ContainsKey(typeID);


	public static bool IsWire (int typeID) => WireIdPool.ContainsKey(typeID);


	// UTL
	private static int WireConnection_to_BitInt (bool connectL, bool connectR, bool connectD, bool connectU) => (connectL ? 0b1000 : 0b0000) | (connectR ? 0b0100 : 0b0000) | (connectD ? 0b0010 : 0b0000) | (connectU ? 0b0001 : 0b0000);


	private static bool CheckConnectWithBitInt (int bitInt, Direction4 dir) => dir switch {
		Direction4.Left => (bitInt & 0b1000) != 0,
		Direction4.Right => (bitInt & 0b0100) != 0,
		Direction4.Down => (bitInt & 0b0010) != 0,
		Direction4.Up => (bitInt & 0b0001) != 0,
		_ => false,
	};


}