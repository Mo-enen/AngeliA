using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class Crowbar : AutoSpriteHook {

		// Api
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
		public override int RangeXLeft => 275;
		public override int RangeXRight => 384;
		public override int RangeY => 432;
		public override int ChargeAttackDuration => 20;


	}
}
