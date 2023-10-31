using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


namespace AngeliaGame {
	public abstract class PolearmWeapon : AutoSpriteWeapon {
		public sealed override WeaponType WeaponType => WeaponType.Polearm;
		public sealed override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
		protected override bool IgnoreGrabTwist => true;
		public override int AttackDuration => 18;
		public override int AttackCooldown => 2;
	}
	public class iSpearWood : PolearmWeapon {
		public override int ChargeAttackDuration => 20;
	}
	public class iSpearIron : PolearmWeapon { }
	public class iSpearGold : PolearmWeapon { }
	public class iTrident : PolearmWeapon { }
	public class iBoStaffWood : PolearmWeapon { }
	public class iBoStaffIron : PolearmWeapon { }
	public class iBoStaffGold : PolearmWeapon { }
	public class iNaginata : PolearmWeapon { }
	public class iHalberd : PolearmWeapon { }
	public class iJi : PolearmWeapon { }
	public class iMonkSpade : PolearmWeapon { }
	public class iManCatcher : PolearmWeapon { }
	public class iSwallow : PolearmWeapon { }
	public class iFork : PolearmWeapon { }
	public class iBrandistock : PolearmWeapon { }
}
