using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class PoseHandheld_Float : PoseAnimation {
	public static readonly int TYPE_ID = typeof(PoseHandheld_Float).AngeHash();
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		// Charging
		if (Attackness.IsChargingAttack) {
			PoseAttack_Float.Wave();
		}
	}
}