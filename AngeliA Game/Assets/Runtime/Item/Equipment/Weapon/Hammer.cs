using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class iHammerWood : AutoSpriteHammer { }
	public class iHammerIron : AutoSpriteHammer { }
	public class iHammerGold : AutoSpriteHammer { }
	public class iMaceRound : AutoSpriteHammer { }
	public class iMaceSkull : AutoSpriteHammer { }
	public class iBaseballBatWood : AutoSpriteHammer { }
	public class iMaceSpiked : AutoSpriteHammer { }
	public class iBian : AutoSpriteHammer {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.OneOnEachHand;
		public override int ChargeAttackDuration => 20;
	}
	public class iHammerRiceCake : AutoSpriteHammer {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
	public class iHammerGoatHorn : AutoSpriteHammer { }
	public class iBaseballBatIron : AutoSpriteHammer { }
	public class iHammerThunder : AutoSpriteHammer {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
	public class iHammerMoai : AutoSpriteHammer {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
		public override int ChargeAttackDuration => 20;
	}
	public class iHammerPaladin : AutoSpriteHammer {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
	public class iHammerRuby : AutoSpriteHammer {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
}
