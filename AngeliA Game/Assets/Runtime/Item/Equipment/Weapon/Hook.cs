using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public abstract class HookWeapon : AutoSpriteWeapon {
		public sealed override WeaponType WeaponType => WeaponType.Hook;
	}
	public class iScytheWood : HookWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
	public class iScytheIron : HookWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
	public class iScytheGold : HookWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
	public class iSickle : HookWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iHookIron : HookWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.OneOnEachHand;
	}
	public class iHookGold : HookWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.OneOnEachHand;
	}
	public class iHookHand : HookWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iHookJungle : HookWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iHookBone : HookWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iHookJagged : HookWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iHookTripple : HookWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iHookBig : HookWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
	}
	public class iHookPudge : HookWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
	}
	public class iHookChicken : HookWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iHookRusty : HookWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.OneOnEachHand;
	}
}
