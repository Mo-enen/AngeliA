using System.Collections;
using System.Collections.Generic;


using AngeliA;namespace AngeliA.Platformer;


public abstract class Axe : MeleeWeapon {
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	public sealed override WeaponType WeaponType => WeaponType.Axe;
	public override int AttackDuration => 12;
	public override int AttackCooldown => 2;
	public override int RangeXLeft => 275;
	public override int RangeXRight => 384;
	public override int RangeY => 512;
}
