namespace AngeliA;

public class PoseHandheld_EachHand : PoseAnimation {
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		if (Attackness.IsChargingAttack) {
			PoseAttack_Wave.EachHand_SmashDown();
		}
	}
}
