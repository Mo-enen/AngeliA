using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public abstract class ThrowingWeapon : AutoSpriteWeapon {
		public sealed override WeaponType WeaponType => WeaponType.Throwing;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iBoomerang : ThrowingWeapon { }
	public class iNinjaStarHalf : ThrowingWeapon { }
	public class iNinjaStar : ThrowingWeapon { }
	public class iKunai : ThrowingWeapon { }
	public class iChakram : ThrowingWeapon { }
	public class iThrowingKnife : ThrowingWeapon { }
	public class iThrowingAxe : ThrowingWeapon { }
	public class iNeedle : ThrowingWeapon { }
	public class iChainMaceBall : ThrowingWeapon { }
	public class iBomb : ThrowingWeapon { }
	public class iAnchor : ThrowingWeapon { }
	public class iCrossAxe : ThrowingWeapon { }
	public class iGrapeBomb : ThrowingWeapon { }
	public class iTearGas : ThrowingWeapon { }
	public class iGrenade : ThrowingWeapon {
		public override int ChargeAttackDuration => 20;
	}
}
