namespace AngeliA;

public class PoseHandheld_EachHand : PoseAnimation {
	public override void Animate (PoseCharacter character) {
		base.Animate(character);
		if (Target.IsChargingAttack) {
			PoseAttack_Wave.EachHand_SmashDown();
		}
	}
}
