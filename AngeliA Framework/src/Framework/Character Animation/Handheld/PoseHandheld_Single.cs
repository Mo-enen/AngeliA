namespace AngeliA;

public class PoseHandheld_Single : PoseAnimation {
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		if (Attackness.IsChargingAttack) {
			PoseAttack_Wave.SingleHanded_SmashDown();
			return;
		}
		if (Target.EquippingWeaponType == WeaponType.Block) {
			Rendering.HandGrabScaleL = Rendering.HandGrabScaleR = 618;
			Rendering.HandGrabRotationL = Rendering.HandGrabRotationR = 0;
		}
	}
}
