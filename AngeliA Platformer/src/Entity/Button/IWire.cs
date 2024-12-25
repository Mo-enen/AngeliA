using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using AngeliA;

namespace AngeliA.Platformer;

public interface IWire {


	// VAR
	private static readonly Dictionary<int, (bool left, bool right, bool down, bool up)> WireIdPool = [];
	private static readonly Dictionary<int, MethodInfo> OperatorPool = [];
	private static readonly Queue<Int3> TriggerTaskQueue = [];
	private static readonly HashSet<Int2> TriggerTaskHash = [];
	private static readonly Queue<(MethodInfo method, Int3 opUnitPos, Int3 btnUnitPos)> OperationRequest = [];
	private static readonly object[] OperateParamCache = [null, null, null];
	[OnButtonWireActived] internal static System.Action<IBlockSquad, Int3, int> OnButtonWireActived;
	[OnButtonOperatorTriggered] internal static System.Action<IBlockSquad, Int3> OnButtonOperatorTriggered;

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
			WireIdPool.TryAdd(type.AngeHash(), (wire.ConnectedLeft, wire.ConnectedRight, wire.ConnectedDown, wire.ConnectedUp));
		}
		// Init Operation Pool
		OperatorPool.Clear();
		foreach (var (method, _) in Util.AllStaticMethodWithAttribute<ButtonOperatorAttribute>()) {
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
		for (int i = OperationRequest.Count; i > 0 && OperationRequest.TryDequeue(out var pair); i--) {
			// If anything added into RequestQueue duration this loop,
			// added ones will NOT be operate in this loop,
			// and still will be keep for next game frame
			var (method, opUnitPos, btnUnitPos) = pair;
			OperateParamCache[0] = WorldSquad.Stream;
			OperateParamCache[1] = opUnitPos;
			OperateParamCache[2] = btnUnitPos;
			var result = method?.Invoke(null, OperateParamCache);
			if (result is not bool bResult || bResult) {
				OnButtonOperatorTriggered?.Invoke(WorldSquad.Stream, opUnitPos);
			}
		}
	}


	// API
	public static void TriggerCircuit (IBlockSquad squad, int unitX, int unitY, int unitZ, int maxUnitDistance, bool triggerL, bool triggerR, bool triggerD, bool triggerU) {
		TriggerTaskQueue.Clear();
		TriggerTaskHash.Clear();
		int step = 0;
		int connectInt =
			(triggerL ? 0b1000 : 0b0000) |
			(triggerR ? 0b0100 : 0b0000) |
			(triggerD ? 0b0010 : 0b0000) |
			(triggerU ? 0b0001 : 0b0000);
		var startPos = new Int3(unitX, unitY, connectInt);
		var buttonPos = new Int3(startPos.x, startPos.y, unitZ);
		TriggerTaskQueue.Enqueue(startPos);
		TriggerTaskHash.Add((Int2)startPos);
		while (TriggerTaskQueue.TryDequeue(out var pos)) {
			int id = squad.GetBlockAt(pos.x, pos.y, unitZ, BlockType.Entity);
			if (id == 0) continue;
			var pos2 = (Int2)pos;
			// L
			if (pos.x > startPos.x - maxUnitDistance && (pos.z & 0b1000) != 0) {
				Interate(squad, pos2.ShiftX(-1), buttonPos, unitZ, step, 0b0100);
			}
			// R
			if (pos.x < startPos.x + maxUnitDistance && (pos.z & 0b0100) != 0) {
				Interate(squad, pos2.ShiftX(1), buttonPos, unitZ, step, 0b1000);
			}
			// D
			if (pos.y > startPos.y - maxUnitDistance && (pos.z & 0b0010) != 0) {
				Interate(squad, pos2.ShiftY(-1), buttonPos, unitZ, step, 0b0001);
			}
			// U
			if (pos.y < startPos.y + maxUnitDistance && (pos.z & 0b0001) != 0) {
				Interate(squad, pos2.ShiftY(1), buttonPos, unitZ, step, 0b0010);
			}
			step++;
			// Func
			static void Interate (IBlockSquad _squad, Int2 _pos, Int3 _buttonPos, int _z, int step, int requireConnectDir) {
				if (TriggerTaskHash.Contains(_pos)) return;
				TriggerTaskHash.Add(_pos);
				int _id = _squad.GetBlockAt(_pos.x, _pos.y, _z, BlockType.Entity);
				if (_id == 0) return;
				var _pos3 = new Int3(_pos.x, _pos.y, _z);
				// Check for Btn Operators
				if (OperatorPool.TryGetValue(_id, out var method)) {
					OperationRequest.Enqueue((method, _pos3, _buttonPos));
				}
				// Check for Wire Expand
				if (WireIdPool.TryGetValue(_id, out var connectDirections)) {
					int connectInt =
						(connectDirections.left ? 0b1000 : 0b0000) |
						(connectDirections.right ? 0b0100 : 0b0000) |
						(connectDirections.down ? 0b0010 : 0b0000) |
						(connectDirections.up ? 0b0001 : 0b0000);
					if ((requireConnectDir & connectInt) != 0) {
						TriggerTaskQueue.Enqueue(new Int3(_pos.x, _pos.y, connectInt));
						OnButtonWireActived?.Invoke(_squad, _pos3, step);
					}
				}
			}
		}
	}


}