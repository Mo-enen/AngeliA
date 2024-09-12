using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class TriggerablePlatform : Platform, IPartializable {

	// Api
	protected virtual PartializedMode TriggerMode => PartializedMode.Horizontal;
	protected object TriggeredData { get; set; } = null;
	int IPartializable.PartializeStamp { get; set; }

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		TriggeredData = null;
	}

	protected virtual void OnTriggered (object data) { }

	// API
	public virtual void Trigger (object data = null) {
		IPartializable.ForAllPartializedEntity<TriggerablePlatform>(
			PhysicsMask.ENVIRONMENT, TypeID, Rect, OperationMode.ColliderAndTrigger, TriggerMode,
			OnTrigger, data
		);
		static void OnTrigger (TriggerablePlatform platform) {
			platform.TriggeredData = IPartializable.PartializeTempParam;
			platform.OnTriggered(IPartializable.PartializeTempParam);
		}
	}

}
