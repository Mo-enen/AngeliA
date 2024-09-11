using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class DroppingPlatform : StepTriggerPlatform {

	// Api
	protected virtual int InitialDropSpeed => 0;
	protected virtual int MaxDropSpeed => 96;
	protected virtual int DropAcceleration => -3;
	public bool Dropping { get; private set; }

	// Data
	private int CurrentSpeedY = 0;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		CurrentSpeedY = 0;
		Dropping = false;
	}

	protected override void Move () {
		if (!Dropping) return;
		Y += CurrentSpeedY;
		CurrentSpeedY = (CurrentSpeedY + DropAcceleration).Clamp(-MaxDropSpeed, MaxDropSpeed);
	}

	// API
	protected override void OnTriggered (object data) {
		CurrentSpeedY = InitialDropSpeed;
		Dropping = true;
	}

}
