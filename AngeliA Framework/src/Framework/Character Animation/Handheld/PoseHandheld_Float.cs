using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class PoseHandheld_Float : PoseAnimation {
	public override void Animate (PoseCharacter character) {
		base.Animate(character);
		// Charging
		if (Attackness.IsChargingAttack) {
			PoseAttack_Float.WaveDown();
		}
	}
}