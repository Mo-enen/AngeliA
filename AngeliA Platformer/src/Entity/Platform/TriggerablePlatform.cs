using System.Collections;
using System.Collections.Generic;

using AngeliA;

namespace AngeliA.Platformer;

public abstract class TriggerablePlatform : Platform, IUnitable {

	// Api
	protected virtual IUnitable.UniteMode TriggerMode => IUnitable.UniteMode.Horizontal;
	protected virtual bool AllowMultipleTrigger => false;
	protected object TriggeredData { get; set; } = null;
	public int LastTriggerFrame { get; private set; } = -1;
	int IUnitable.LocalUniteStamp { get; set; }

	// MSG
	[CircuitOperator]
	internal static bool Operate (IBlockSquad _, Int3 operatorUnitPos, Int3 __) {
		if (Physics.GetEntity<TriggerablePlatform>(
			IRect.Point(operatorUnitPos.x.ToGlobal() + Const.HALF, operatorUnitPos.y.ToGlobal() + Const.HALF),
			PhysicsMask.ENVIRONMENT, null, OperationMode.ColliderAndTrigger
		) is not TriggerablePlatform platform) return false;
		bool result = platform.AllowMultipleTrigger || platform.LastTriggerFrame < 0;
		platform.Trigger();
		return result;
	}

	public override void OnActivated () {
		base.OnActivated();
		TriggeredData = null;
		LastTriggerFrame = -1;
	}

	protected virtual void OnTriggered (object data) { }

	// API
	public virtual void Trigger (object data = null) {
		if (!AllowMultipleTrigger && LastTriggerFrame >= 0) return;
		LastTriggerFrame = Game.GlobalFrame;
		IUnitable.ForAllUnitedEntity<TriggerablePlatform>(
			PhysicsMask.ENVIRONMENT, TypeID, Rect, OperationMode.ColliderAndTrigger, TriggerMode,
			OnTrigger, data
		);
		static void OnTrigger (TriggerablePlatform platform) {
			platform.LastTriggerFrame = Game.GlobalFrame;
			platform.TriggeredData = IUnitable.UniteTempParam;
			platform.OnTriggered(IUnitable.UniteTempParam);
		}
	}

}
