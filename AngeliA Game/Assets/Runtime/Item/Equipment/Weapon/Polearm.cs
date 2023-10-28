using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


namespace AngeliaGame {
	public abstract class SpearWeapon : AutoSpriteWeapon {
		public sealed override WeaponType WeaponType => WeaponType.Polearm;
		public sealed override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
		protected override bool IgnoreGrabTwist => true;
		public override int AttackDuration => 18;
		public override int AttackCooldown => 2;
	}
	public class iSpearWood : SpearWeapon { }
	public class iSpearIron : SpearWeapon { }
	public class iSpearGold : SpearWeapon { }
	public class iTrident : SpearWeapon { }
	public class iBoStaffWood : SpearWeapon { }
	public class iBoStaffIron : SpearWeapon { }
	public class iBoStaffGold : SpearWeapon { }
	public class iNaginata : SpearWeapon { }
	public class iHalberd : SpearWeapon { }
	public class iJi : SpearWeapon { }
	public class iMonkSpade : SpearWeapon { }
	public class iManCatcher : SpearWeapon { }
	public class iSwallow : SpearWeapon { }
	public class iFork : SpearWeapon { }
	public class iBrandistock : SpearWeapon { }
}
