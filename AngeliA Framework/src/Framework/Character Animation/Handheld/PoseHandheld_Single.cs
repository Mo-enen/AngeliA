namespace AngeliA;

public class PoseHandheld_Single : PoseAnimation {
	public override void Animate (PoseCharacter character) {
		base.Animate(character);
		if (Attackness.IsChargingAttack) {
			PoseAttack_Wave.SingleHanded_SmashDown();
			return;
		}
		if (Target.EquippingWeaponType == WeaponType.Block) {
			Target.HandGrabScaleL = Target.HandGrabScaleR = 618;
			Target.HandGrabRotationL = Target.HandGrabRotationR = 0;
		}
	}
}
