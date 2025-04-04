using System.Collections;
using System.Collections.Generic;

using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Platform that can be trigger and perform some specified logic
/// </summary>
public abstract class TriggerablePlatform : Platform, IUnitable, ICircuitOperator {

	// Api
	/// <summary>
	/// How does the platforms get trigger in a group
	/// </summary>
	protected virtual IUnitable.UniteMode TriggerMode => IUnitable.UniteMode.Horizontal;
	/// <summary>
	/// True if a triggered platform can be trigger again
	/// </summary>
	protected virtual bool AllowMultipleTrigger => false;
	/// <summary>
	/// Custom data from the trigger function.
	/// </summary>
	protected object TriggeredData { get; set; } = null;
	public int LastTriggerFrame { get; private set; } = -1;
	int IUnitable.LocalUniteStamp { get; set; }

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		TriggeredData = null;
		LastTriggerFrame = -1;
	}

	/// <summary>
	/// This function is called when the platform is triggered
	/// </summary>
	/// <param name="data">Custom data. Use TriggerablePlatform.TriggeredData to get this value</param>
	protected virtual void OnTriggered (object data) { }

	// API
	/// <summary>
	/// Trigger the platform
	/// </summary>
	/// <param name="data">Custom data</param>
	public virtual void Trigger (object data = null) {
		if (!AllowMultipleTrigger && LastTriggerFrame >= 0) return;
		LastTriggerFrame = Game.GlobalFrame;
		IUnitable.ForAllUnitedEntity(
			PhysicsMask.ENVIRONMENT, TypeID, Rect, OperationMode.ColliderAndTrigger, TriggerMode,
			OnTrigger, data
		);
		static void OnTrigger (IUnitable unitable) {
			if (unitable is not TriggerablePlatform platform) return;
			platform.LastTriggerFrame = Game.GlobalFrame;
			platform.TriggeredData = IUnitable.UniteTempParam;
			platform.OnTriggered(IUnitable.UniteTempParam);
		}
	}

	void ICircuitOperator.OnTriggeredByCircuit () => Trigger();

}
