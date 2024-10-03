using AngeliA;namespace AngeliA.Platformer;

public abstract class StepTriggerPlatform : TriggerablePlatform {
	protected virtual bool TriggerOnRigidbodyTouch => false;
	protected virtual bool TriggerOnCharacterTouch => false;
	protected virtual bool TriggerOnPlayerTouch => true;
	protected override void OnRigidbodyTouched (Rigidbody rig) {
		base.OnRigidbodyTouched(rig);
		if (!TriggerOnRigidbodyTouch || TriggeredData != null) return;
		Trigger(rig);
	}
	protected override void OnCharacterTouched (Character character) {
		base.OnCharacterTouched(character);
		if (!TriggerOnCharacterTouch || TriggeredData != null) return;
		Trigger(character);
	}
	protected override void OnPlayerTouched (Character player) {
		base.OnPlayerTouched(player);
		if (!TriggerOnPlayerTouch || TriggeredData != null) return;
		Trigger(player);
	}
}
