using System.Collections;
using System.Collections.Generic;

using AngeliA;
namespace AngeliA.Platformer;

public abstract class TriggerablePlatform : Platform, IUnitable {

	// Api
	protected virtual IUnitable.UniteMode TriggerMode => IUnitable.UniteMode.Horizontal;
	protected object TriggeredData { get; set; } = null;
	int IUnitable.LocalUniteStamp { get; set; }
	public int LastTriggerFrame { get; private set; } = -1;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		TriggeredData = null;
		LastTriggerFrame = -1;
	}

	protected virtual void OnTriggered (object data) { }

	// API
	public virtual void Trigger (object data = null) {
		LastTriggerFrame = Game.GlobalFrame;
		IUnitable.ForAllPartializedEntity<TriggerablePlatform>(
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
