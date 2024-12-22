using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;


[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class Button : Entity, IBlockEntity {




	#region --- VAR ---


	// Data
	private static readonly Dictionary<int, ButtonOperator> OperatorPool = [];
	private static readonly Queue<Int2> TriggerTaskQueue = [];
	private static readonly HashSet<Int2> TriggerTaskHash = [];
	private static readonly Queue<(int id, Int3 opUnitPos, Int3 btnUnitPos)> OperationRequest = [];


	#endregion




	#region --- MSG ---



	[OnGameInitialize]
	internal static void OnGameInitialize () {
		OperatorPool.Clear();
		foreach (var type in typeof(ButtonOperator).AllChildClass()) {
			if (System.Activator.CreateInstance(type) is not ButtonOperator oper) continue;
			OperatorPool.Add(type.AngeHash(), oper);
		}
	}



	[OnGameUpdateLater]
	internal static void PerformOperation () {
		if (OperationRequest.Count == 0) return;
		for (int i = OperationRequest.Count; i > 0 && OperationRequest.TryDequeue(out var pair); i--) {
			// If anything added into RequestQueue duration this loop,
			// added ones will NOT be operate in this loop,
			// and still will be keep for next game frame
			var (id, opUnitPos, btnUnitPos) = pair;
			if (!OperatorPool.TryGetValue(id, out var _operator)) continue;
			_operator.Operate(WorldSquad.Stream, opUnitPos, btnUnitPos);
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
		//OperationRequest
		var unitPos = new Int2((X + 1).ToUnit(), (Y + 1).ToUnit());
		TriggerTaskQueue.Enqueue(unitPos);
		TriggerTaskHash.Add(unitPos);
		while (TriggerTaskQueue.TryDequeue(out var pos)) {


			// TODO


		}
	}


	#endregion




	#region --- LGC ---



	#endregion




}
