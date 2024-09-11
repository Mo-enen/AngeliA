using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class TriggerablePlatform : Platform {

	// Api
	protected virtual PartializedMode TriggerMode => PartializedMode.Horizontal;
	protected object TriggeredData { get; private set; } = null;

	// Data
	private static object CurrentTriggeringData;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		TriggeredData = null;
	}

	protected abstract void OnTriggered (object data);

	// API
	public void Trigger (object data = null) {
		CurrentTriggeringData = data;
		FrameworkUtil.ForAllPartializedEntity<TriggerablePlatform>(
			PhysicsMask.ENVIRONMENT, TypeID, Rect, OperationMode.ColliderAndTrigger, TriggerMode,
			OnTrigger
		);
		static void OnTrigger (TriggerablePlatform platform) {
			platform.TriggeredData = CurrentTriggeringData;
			platform.OnTriggered(CurrentTriggeringData);
		}
	}

}
