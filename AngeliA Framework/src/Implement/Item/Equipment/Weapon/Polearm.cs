using System.Collections;
using System.Collections.Generic;


namespace AngeliA; 


// Polearm
public abstract class Polearm<B> : Polearm where B : MeleeBullet {
	public Polearm () => BulletID = typeof(B).AngeHash();
}
public abstract class Polearm : MeleeWeapon {
	public sealed override WeaponType WeaponType => WeaponType.Polearm;
	public sealed override WeaponHandheld Handheld => WeaponHandheld.Pole;
	protected override bool IgnoreGrabTwist => true;
	public override int AttackDuration => 18;
	public override int AttackCooldown => 2;
	public override int RangeXLeft => 384;
	public override int RangeXRight => 384;
	public override int RangeY => 432;

}


// Implement
[ItemCombination(typeof(iBoStaffWood), typeof(iDagger), 1)]
public class iSpearWood : Polearm { }
[ItemCombination(typeof(iSpearWood), typeof(iIngotIron), 1)]
public class iSpearIron : Polearm { }
[ItemCombination(typeof(iIngotGold), typeof(iSpearIron), 1)]
public class iSpearGold : Polearm { }
[ItemCombination(typeof(iSpearIron), typeof(iIngotIron), 1)]
public class iSpearHeavy : Polearm { }
[ItemCombination(typeof(iMagatama), typeof(iMagatama), typeof(iMagatama), typeof(iSpearIron), 1)]
[ItemCombination(typeof(iRibbon), typeof(iTreeBranch), typeof(iTreeBranch), typeof(iTreeBranch), 1)]
public class iBoStaffWood : Polearm { }
[ItemCombination(typeof(iBoStaffWood), typeof(iIngotIron), 1)]
public class iBoStaffIron : Polearm { }
[ItemCombination(typeof(iBoStaffIron), typeof(iIngotGold), 1)]
public class iBoStaffGold : Polearm { }
[ItemCombination(typeof(iBoStaffWood), typeof(iKnifeGiant), 1)]
public class iNaginata : Polearm { }
[ItemCombination(typeof(iAxeWood), typeof(iBoStaffWood), 1)]
public class iHalberd : Polearm { }
[ItemCombination(typeof(iBoStaffWood), typeof(iWuXingHook), typeof(iWuXingHook), 1)]
public class iJi : Polearm { }
[ItemCombination(typeof(iAxeWood), typeof(iBoStaffWood), typeof(iWuXingHook), 1)]
public class iMonkSpade : Polearm { }
[ItemCombination(typeof(iBoStaffWood), typeof(iBolt), typeof(iBolt), typeof(iWuXingHook), 1)]
public class iManCatcher : Polearm { }
[ItemCombination(typeof(iScimitar), typeof(iScimitar), typeof(iBoStaffWood), 1)]
public class iSwallow : Polearm { }
[ItemCombination(typeof(iBoStaffWood), typeof(iDagger), typeof(iDagger), 1)]
public class iFork : Polearm { }
[ItemCombination(typeof(iBoStaffWood), typeof(iBolt), typeof(iBolt), typeof(iBolt), 1)]
public class iBrandistock : Polearm { }
