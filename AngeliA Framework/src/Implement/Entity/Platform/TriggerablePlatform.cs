using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class TriggerablePlatform : Platform, IPartializable {

	// Api
	protected virtual PartializedMode TriggerMode => PartializedMode.Horizontal;
	protected object TriggeredData { get; set; } = null;
	int IPartializable.PartializeStamp { get; set; }
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
		IPartializable.ForAllPartializedEntity<TriggerablePlatform>(
			PhysicsMask.ENVIRONMENT, TypeID, Rect, OperationMode.ColliderAndTrigger, TriggerMode,
			OnTrigger, data
		);
		static void OnTrigger (TriggerablePlatform platform) {
			platform.LastTriggerFrame = Game.GlobalFrame;
			platform.TriggeredData = IPartializable.PartializeTempParam;
			platform.OnTriggered(IPartializable.PartializeTempParam);
		}
	}

}
