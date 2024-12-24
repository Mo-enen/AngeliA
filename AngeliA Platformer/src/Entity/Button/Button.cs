using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using AngeliA;

namespace AngeliA.Platformer;


[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class Button : Entity, IBlockEntity {




	#region --- VAR ---


	// Const
	private const int MAX_TRIGGER_DIS = Const.MAP;
	[OnButtonWireActived] internal static System.Action<IBlockSquad, Int3, int> OnButtonWireActived;
	[OnButtonOperatorTriggered] internal static System.Action<IBlockSquad, Int3> OnButtonOperatorTriggered;

	// Data
	private static readonly Dictionary<int, MethodInfo> OperatorPool = [];
	private static readonly Queue<Int2> TriggerTaskQueue = [];
	private static readonly HashSet<Int2> TriggerTaskHash = [];
	private static readonly Queue<(MethodInfo method, Int3 opUnitPos, Int3 btnUnitPos)> OperationRequest = [];
	private static readonly object[] OperateParamCache = [null, null, null];


	#endregion




	#region --- MSG ---


	[OnGameInitialize]
	internal static void OnGameInitialize () {
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


	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true, Tag.OnewayUp);
	}


	#endregion




	#region --- API ---


	public virtual void PressButton () {
		TriggerTaskQueue.Clear();
		TriggerTaskHash.Clear();
		int step = 0;
		var squad = WorldSquad.Stream;
		int z = Stage.ViewZ;
		var startPos = new Int2((X + 1).ToUnit(), (Y + 1).ToUnit());
		var buttonPos = new Int3(startPos.x, startPos.y, z);
		TriggerTaskQueue.Enqueue(startPos);
		TriggerTaskHash.Add(startPos);
		while (TriggerTaskQueue.TryDequeue(out var pos)) {
			int id = squad.GetBlockAt(pos.x, pos.y, z, BlockType.Entity);
			if (id == 0) continue;
			if (pos.x > startPos.x - MAX_TRIGGER_DIS) {
				Interate(squad, pos.ShiftX(-1), buttonPos, z, step);
			}
			if (pos.x < startPos.x + MAX_TRIGGER_DIS) {
				Interate(squad, pos.ShiftX(1), buttonPos, z, step);
			}
			if (pos.y > startPos.y - MAX_TRIGGER_DIS) {
				Interate(squad, pos.ShiftY(-1), buttonPos, z, step);
			}
			if (pos.y < startPos.y + MAX_TRIGGER_DIS) {
				Interate(squad, pos.ShiftY(1), buttonPos, z, step);
			}
			step++;
			// Func
			static void Interate (IBlockSquad _squad, Int2 _pos, Int3 _buttonPos, int _z, int step) {
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
				if (IWire.IsWire(_id)) {
					TriggerTaskQueue.Enqueue(_pos);
					OnButtonWireActived?.Invoke(_squad, _pos3, step);
				}
			}
		}
	}


	#endregion




	#region --- LGC ---



	#endregion




}
