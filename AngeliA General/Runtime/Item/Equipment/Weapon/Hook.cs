using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGeneral {
	public class iScytheWood : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
	public class iScytheIron : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
	public class iScytheGold : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
	public class iSickle : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iHookIron : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.OneOnEachHand;
	}
	public class iHookGold : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.OneOnEachHand;
	}
	public class iHookHand : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iHookJungle : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iHookBone : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iHookJagged : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iHookTripple : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iHookBig : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
	}
	public class iHookPudge : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
	}
	public class iHookChicken : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iHookRusty : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.OneOnEachHand;
	}
}
