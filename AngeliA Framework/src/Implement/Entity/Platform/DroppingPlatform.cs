using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class DroppingPlatform : TriggerablePlatform {

	// Api
	protected virtual bool DropOnRigidbodyTouch => false;
	protected virtual bool DropOnCharacterTouch => false;
	protected virtual bool DropOnPlayerTouch => true;
	protected virtual int InitialDropSpeed => 0;
	protected virtual int MaxDropSpeed => 96;
	protected virtual int DropAcceleration => -3;
	public bool Dropping => TriggeredData is bool dropping && dropping;

	// Data
	private int CurrentSpeedY = 0;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		CurrentSpeedY = 0;
	}

	protected override void Move () {
		if (!Dropping) return;
		Y += CurrentSpeedY;
		CurrentSpeedY = (CurrentSpeedY + DropAcceleration).Clamp(-MaxDropSpeed, MaxDropSpeed);
	}

	protected override void OnRigidbodyTouched (Rigidbody rig) {
		base.OnRigidbodyTouched(rig);
		if (!DropOnRigidbodyTouch) return;
		Trigger(true);
	}
	protected override void OnCharacterTouched (Character character) {
		base.OnCharacterTouched(character);
		if (!DropOnCharacterTouch) return;
		Trigger(true);
	}
	protected override void OnPlayerTouched (Player player) {
		base.OnPlayerTouched(player);
		if (!DropOnPlayerTouch) return;
		Trigger(true);
	}

	// API
	protected override void OnTriggered (object data) => CurrentSpeedY = InitialDropSpeed;

}
