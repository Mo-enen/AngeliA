using System.Collections;
using System.Collections.Generic;


using AngeliA;namespace AngeliA.Platformer;


public abstract class Hook : MeleeWeapon {
	public sealed override WeaponType WeaponType => WeaponType.Hook;
	public override int RangeXLeft => 275;
	public override int RangeXRight => 275;
	public override int RangeY => 432;
}

