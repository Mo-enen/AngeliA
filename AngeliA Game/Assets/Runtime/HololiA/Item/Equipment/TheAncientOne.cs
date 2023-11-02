using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class iTheAncientOne : AutoSpriteMagicWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Float;
		public override int ChargeAttackDuration => 20;
	}
}