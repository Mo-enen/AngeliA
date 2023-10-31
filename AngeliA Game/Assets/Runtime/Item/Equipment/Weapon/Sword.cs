using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public abstract class SwordWeapon : AutoSpriteWeapon {
		public sealed override WeaponType WeaponType => WeaponType.Sword;
	}
	public class iSwordWood : SwordWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iSwordIron : SwordWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iSwordGold : SwordWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iDagger : SwordWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iSwordCrimson : SwordWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
	}
	public class iSwordScarlet : SwordWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
	}
	public class iScimitar : SwordWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iSwordPirate : SwordWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iSwordAgile : SwordWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iScimitarAgile : SwordWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iSwordJagged : SwordWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
	}
	public class iSwordGreat : SwordWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
	}
	public class iSwordDark : SwordWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
	}
	public class iSwordCrutch : SwordWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iKnifeGiant : SwordWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
	}
}
