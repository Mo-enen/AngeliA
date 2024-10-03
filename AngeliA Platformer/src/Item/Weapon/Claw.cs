using System.Collections;
using System.Collections.Generic;

using AngeliA;namespace AngeliA.Platformer;


public abstract class Claw : MeleeWeapon {
	public sealed override WeaponType WeaponType => WeaponType.Claw;
	public sealed override WeaponHandheld Handheld => WeaponHandheld.OneOnEachHand;
	public override int AttackDuration => 10;
	public override int AttackCooldown => 0;
	public override int? DefaultSpeedRateOnAttack => 1000;
	public override int? WalkingSpeedRateOnAttack => 1000;
	public override int? RunningSpeedRateOnAttack => 1000;
	public override int RangeXLeft => 275;
	public override int RangeXRight => 275;
	public override int RangeY => 432;
}
