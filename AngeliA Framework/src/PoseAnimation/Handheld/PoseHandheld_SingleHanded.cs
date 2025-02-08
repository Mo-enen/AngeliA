namespace AngeliA;

public class PoseHandheld_SingleHanded : PoseAnimation {
	public static readonly int TYPE_ID = typeof(PoseHandheld_SingleHanded).AngeHash();
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		if (Attackness.IsChargingAttack) {
			PoseAttack_WaveSingleHanded.SmashDown();
			return;
		}
	}
}
