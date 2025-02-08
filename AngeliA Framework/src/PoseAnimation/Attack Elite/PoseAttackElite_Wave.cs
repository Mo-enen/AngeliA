using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class PoseAttackElite_Wave : PoseAnimation {


	public static readonly int TYPE_ID = typeof(PoseAttackElite_Wave).AngeHash();


	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		Attackness.AttackStyleLoop = 4;
		Wave();
	}


	public static void Wave () {
		var handheld = Target.EquippingToolHeld;
		var weaponType = Target.EquippingToolType;
		switch (handheld) {

			// Single Handed
			default:
				WaveSingleHanded(
					Attackness.LastAttackCharged ||
					weaponType == ToolType.Hand ||
					weaponType == ToolType.Tool ||
					weaponType == ToolType.Throwing ||
					weaponType == ToolType.Flail ?
						0 : Attackness.AttackStyleIndex % Attackness.AttackStyleLoop
				);
				break;

			// Double Handed
			case ToolHandheld.DoubleHanded:
				WaveDoubleHanded(
					Attackness.LastAttackCharged ||
					weaponType == ToolType.Tool ||
					weaponType == ToolType.Throwing ||
					weaponType == ToolType.Flail ?
						0 : Attackness.AttackStyleIndex % Attackness.AttackStyleLoop
				);
				break;

			// Each Hand
			case ToolHandheld.OneOnEachHand:
				WaveEachHand(
					Attackness.LastAttackCharged ? 0 :
					Attackness.AttackStyleIndex % Attackness.AttackStyleLoop
				);
				break;

			// Pole
			case ToolHandheld.Pole:
				WavePolearm(Attackness.LastAttackCharged ||
					Target.EquippingToolType == ToolType.Flail ? 0 :
					Attackness.AttackStyleIndex % Attackness.AttackStyleLoop);
				break;
		}
	}


	public static void WaveSingleHanded (int style) {

		float ease01 = AttackEase;





	}


	public static void WaveDoubleHanded (int style) {



	}


	public static void WaveEachHand (int style) {



	}


	public static void WavePolearm (int style) {



	}


}
