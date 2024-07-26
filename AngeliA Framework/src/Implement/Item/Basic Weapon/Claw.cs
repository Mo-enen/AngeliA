using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public abstract class Claw : MeleeWeapon {
	public sealed override WeaponType WeaponType => WeaponType.Claw;
	public sealed override WeaponHandheld Handheld => WeaponHandheld.OneOnEachHand;
	public override int AttackDuration => 10;
	public override int AttackCooldown => 0;
	public override int? DefaultSpeedLoseOnAttack => 1000;
	public override int? WalkingSpeedLoseOnAttack => 1000;
	public override int? RunningSpeedLoseOnAttack => 1000;
	public override int RangeXLeft => 275;
	public override int RangeXRight => 275;
	public override int RangeY => 432;
}
