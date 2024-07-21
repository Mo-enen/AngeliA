namespace AngeliA;

public class PoseHandheld_Single : PoseAnimation {
	public override void Animate (PoseCharacter character) {
		base.Animate(character);
		if (Target.IsChargingAttack) {
			PoseAttack_Wave.SingleHanded_SmashDown();
		}
	}
}
