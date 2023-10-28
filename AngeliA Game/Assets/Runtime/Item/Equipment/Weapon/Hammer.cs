using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public abstract class HammerWeapon : AutoSpriteWeapon {
		public sealed override WeaponType WeaponType => WeaponType.Hammer;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
		protected override bool IgnoreGrabTwist => true;
	}
	public class iHammerWood : HammerWeapon { }
	public class iHammerIron : HammerWeapon { }
	public class iHammerGold : HammerWeapon { }
	public class iMaceRound : HammerWeapon { }
	public class iMaceSkull : HammerWeapon { }
	public class iBaseballBatWood : HammerWeapon { }
	public class iMaceSpiked : HammerWeapon { }
	public class iBian : HammerWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.OneOnEachHand;
	}
	public class iHammerRiceCake : HammerWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
	public class iHammerGoatHorn : HammerWeapon { }
	public class iBaseballBatIron : HammerWeapon { }
	public class iHammerThunder : HammerWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
	public class iHammerMoai : HammerWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
	public class iHammerPaladin : HammerWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
	public class iHammerRuby : HammerWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
}
