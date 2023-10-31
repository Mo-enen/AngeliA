using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public abstract class MagicWeapon : AutoSpriteWeapon {
		public sealed override WeaponType WeaponType => WeaponType.Magic;
	}
	public class iWand : MagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iTheAncientOne : MagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Float;
		public override int ChargeAttackDuration => 20;
	}
	public class iStaffFire : MagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
	public class iStaffWater : MagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
	public class iStaffLightning : MagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
	public class iStaffPoision : MagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
		public override int ChargeAttackDuration => 20;
	}
	public class iRitualSkull : MagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Float;
	}
	public class iBambooSlips : MagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Float;
	}
	public class iRitualRuneCube : MagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Float;
	}
	public class iGoblinTrophy : MagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Float;
	}
	public class iMagicOrb : MagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Float;
	}
	public class iMagicEyeball : MagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Float;
	}
	public class iMagicPotion : MagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Float;
	}
	public class iWandStar : MagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iWandFairy : MagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
}
