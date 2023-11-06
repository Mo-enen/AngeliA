using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGeneral {
	public class iWand : AutoSpriteMagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iStaffFire : AutoSpriteMagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
	public class iStaffWater : AutoSpriteMagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
	public class iStaffLightning : AutoSpriteMagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
	public class iStaffPoision : AutoSpriteMagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
	public class iRitualSkull : AutoSpriteMagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Float;
	}
	public class iBambooSlips : AutoSpriteMagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Float;
	}
	public class iRitualRuneCube : AutoSpriteMagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Float;
	}
	public class iGoblinTrophy : AutoSpriteMagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Float;
	}
	public class iMagicOrb : AutoSpriteMagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Float;
	}
	public class iMagicEyeball : AutoSpriteMagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Float;
	}
	public class iMagicPotion : AutoSpriteMagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Float;
	}
	public class iWandStar : AutoSpriteMagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iWandFairy : AutoSpriteMagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
}
