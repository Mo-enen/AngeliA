namespace AngeliA;

public abstract class StepTriggerPlatform : TriggerablePlatform {
	protected virtual bool TriggerOnRigidbodyTouch => false;
	protected virtual bool TriggerOnCharacterTouch => false;
	protected virtual bool TriggerOnPlayerTouch => true;
	protected override void OnRigidbodyTouched (Rigidbody rig) {
		base.OnRigidbodyTouched(rig);
		if (!TriggerOnRigidbodyTouch) return;
		Trigger(rig);
	}
	protected override void OnCharacterTouched (Character character) {
		base.OnCharacterTouched(character);
		if (!TriggerOnCharacterTouch) return;
		Trigger(character);
	}
	protected override void OnPlayerTouched (Player player) {
		base.OnPlayerTouched(player);
		if (!TriggerOnPlayerTouch) return;
		Trigger(player);
	}
}
