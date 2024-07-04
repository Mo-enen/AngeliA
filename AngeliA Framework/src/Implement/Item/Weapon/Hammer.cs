using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public abstract class Hammer : MeleeWeapon {
	public sealed override WeaponType WeaponType => WeaponType.Hammer;
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	protected override bool IgnoreGrabTwist => true;
	public override int RangeXLeft => 275;
	public override int RangeXRight => 275;
	public override int RangeY => 432;

}
