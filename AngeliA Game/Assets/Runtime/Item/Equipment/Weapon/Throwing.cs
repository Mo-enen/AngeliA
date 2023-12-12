using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {

	[EntityAttribute.ItemCombination(typeof(iBanana), typeof(iTreeBranch), 1)]
	public class iBoomerang : ThrowingWeapon {
		protected override int ProjectileSpinSpeed => 24;
	}

	[EntityAttribute.ItemCombination(typeof(iNinjaStar), 2)]
	public class iNinjaStarHalf : ThrowingWeapon {
		protected override int ProjectileSpinSpeed => 24;
	}

	[EntityAttribute.ItemCombination(typeof(iStar), typeof(iIngotIron), 1)]
	public class iNinjaStar : ThrowingWeapon {
		protected override int ProjectileSpinSpeed => 24;
	}

	[EntityAttribute.ItemCombination(typeof(iThrowingKnife), typeof(iIngotIron), 1)]
	public class iKunai : ThrowingWeapon { }

	[EntityAttribute.ItemCombination(typeof(iAxeWood), typeof(iBraceletIron), 1)]
	public class iChakram : ThrowingWeapon {
		protected override int ProjectileSpinSpeed => 12;
		public override int SpeedX => 32;
		protected override bool DestroyOnHitReceiver => false;
		protected override bool DestroyOnHitEnvironment => false;
		protected override bool StopOnHitEnvironment => true;
		protected override int ProjectileStartRotation => Util.QuickRandom(Game.GlobalFrame).UMod(360);
	}

	[EntityAttribute.ItemCombination(typeof(iFlintPolished), typeof(iIngotIron), 1)]
	public class iThrowingKnife : ThrowingWeapon {
		protected override int ProjectileSpinSpeed => 24;
	}

	[EntityAttribute.ItemCombination(typeof(iFlintPolished), typeof(iIngotIron), typeof(iTreeBranch), 1)]
	public class iThrowingAxe : ThrowingWeapon {
		protected override int ProjectileSpinSpeed => 24;
	}

	[EntityAttribute.ItemCombination(typeof(iIronWire), typeof(iIronWire), typeof(iIronWire), 1)]
	public class iNeedle : ThrowingWeapon { }

	[EntityAttribute.ItemCombination(typeof(iChainSpikeBall), typeof(iChainSpikeBall), typeof(iChain), 1)]
	public class iChainMaceBall : ThrowingWeapon {
		protected override int ProjectileSpinSpeed => 24;
	}

	[EntityAttribute.ItemCombination(typeof(iBowlingBall), typeof(iGunpowder), 1)]
	public class iBomb : ThrowingWeapon { }

	[EntityAttribute.ItemCombination(typeof(iIronHook), typeof(iIngotIron), typeof(iIngotIron), typeof(iIngotIron), 1)]
	public class iAnchor : ThrowingWeapon { }

	[EntityAttribute.ItemCombination(typeof(iThrowingAxe), typeof(iIngotGold), 1)]
	public class iCrossAxe : ThrowingWeapon {
		protected override int ProjectileSpinSpeed => 24;
	}

	[EntityAttribute.ItemCombination(typeof(iGrapePurple), typeof(iGunpowder), 1)]
	public class iGrapeBomb : ThrowingWeapon {
		protected override int ProjectileSpinSpeed => 24;
	}

	[EntityAttribute.ItemCombination(typeof(iPotionEmpty), typeof(iBlackPepper), typeof(iGunpowder), 1)]
	public class iTearGas : ThrowingWeapon {
		protected override int ProjectileSpinSpeed => 24;
	}

	[EntityAttribute.ItemCombination(typeof(iPotionEmpty), typeof(iGunpowder), 1)]
	public class iGrenade : ThrowingWeapon {
		protected override int ProjectileSpinSpeed => 24;
	}


}
