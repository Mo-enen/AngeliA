using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Platform that triggers when target step on it
/// </summary>
public abstract class StepTriggerPlatform : TriggerablePlatform {
	/// <summary>
	/// True if rigidbody can trigger this platform
	/// </summary>
	protected virtual bool TriggerOnRigidbodyTouch => false;
	/// <summary>
	/// True if characters can trigger this platform
	/// </summary>
	protected virtual bool TriggerOnCharacterTouch => false;
	/// <summary>
	/// True if selecting player can trigger this platform
	/// </summary>
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
