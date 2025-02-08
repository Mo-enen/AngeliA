namespace AngeliA;

public class PoseHandheld_EachHand : PoseAnimation {
	public static readonly int TYPE_ID = typeof(PoseHandheld_EachHand).AngeHash();
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		if (Attackness.IsChargingAttack) {
			PoseAttack_WaveEachHand.SmashDown();
		}
	}
}
