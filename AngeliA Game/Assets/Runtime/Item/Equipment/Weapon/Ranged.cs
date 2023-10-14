using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class iBowWood : AutoSpriteBow {
		public override WeaponType WeaponType => WeaponType.Ranged;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Bow;
	}
	public class iBowIron : AutoSpriteBow {
		public override WeaponType WeaponType => WeaponType.Ranged;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Bow;
	}
	public class iBowGold : AutoSpriteBow {
		public override WeaponType WeaponType => WeaponType.Ranged;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Bow;
	}
	public class iCrossbowWood : AutoSpriteWeapon {
		public override WeaponType WeaponType => WeaponType.Ranged;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Firearm;
	}
	public class iCrossbowIron : AutoSpriteWeapon {
		public override WeaponType WeaponType => WeaponType.Ranged;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Firearm;
	}
	public class iCrossbowGold : AutoSpriteWeapon {
		public override WeaponType WeaponType => WeaponType.Ranged;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Firearm;
	}
	public class iBlowgun : AutoSpriteWeapon {
		public override WeaponType WeaponType => WeaponType.Ranged;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Firearm;
	}
	public class iSlingshot : AutoSpriteBow {
		public override WeaponType WeaponType => WeaponType.Ranged;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Bow;
	}
	public class iCompoundBow : AutoSpriteBow {
		public override WeaponType WeaponType => WeaponType.Ranged;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Bow;
	}
	public class iRepeatingCrossbow : AutoSpriteWeapon {
		public override WeaponType WeaponType => WeaponType.Ranged;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Firearm;
		public override int AttackDuration => 6;
		public override int AttackCooldown => 0;
		public override bool RepeatAttackWhenHolding => true;
	}
	public class iBowNature : AutoSpriteBow {
		public override WeaponType WeaponType => WeaponType.Ranged;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Bow;
	}
	public class iBowSkull : AutoSpriteBow {
		public override WeaponType WeaponType => WeaponType.Ranged;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Bow;
	}
	public class iBowMage : AutoSpriteBow {
		public override WeaponType WeaponType => WeaponType.Ranged;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Bow;
	}
	public class iBowSky : AutoSpriteBow {
		public override WeaponType WeaponType => WeaponType.Ranged;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Bow;
	}
	public class iBowHarp : AutoSpriteBow {
		public override WeaponType WeaponType => WeaponType.Ranged;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Bow;
	}
}
