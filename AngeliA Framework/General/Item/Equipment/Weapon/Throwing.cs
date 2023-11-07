using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {

	[EntityAttribute.ItemCombination(typeof(iBanana), typeof(iTreeBranch), 1)]
	public class iBoomerang : AutoSpriteThrowingWeapon { }

	[EntityAttribute.ItemCombination(typeof(iNinjaStar), 2)]
	public class iNinjaStarHalf : AutoSpriteThrowingWeapon { }

	[EntityAttribute.ItemCombination(typeof(iStar), typeof(iIngotIron), 1)]
	public class iNinjaStar : AutoSpriteThrowingWeapon { }

	[EntityAttribute.ItemCombination(typeof(iThrowingKnife), typeof(iIngotIron), 1)]
	public class iKunai : AutoSpriteThrowingWeapon { }

	[EntityAttribute.ItemCombination(typeof(iAxeWood), typeof(iBraceletIron), 1)]
	public class iChakram : AutoSpriteThrowingWeapon { }

	[EntityAttribute.ItemCombination(typeof(iFlintPolished), typeof(iIngotIron), 1)]
	public class iThrowingKnife : AutoSpriteThrowingWeapon { }

	[EntityAttribute.ItemCombination(typeof(iFlintPolished), typeof(iIngotIron), typeof(iTreeBranch), 1)]
	public class iThrowingAxe : AutoSpriteThrowingWeapon { }

	[EntityAttribute.ItemCombination(typeof(iIronWire), typeof(iIronWire), typeof(iIronWire), 1)]
	public class iNeedle : AutoSpriteThrowingWeapon { }

	[EntityAttribute.ItemCombination(typeof(iChainSpikeBall), typeof(iChainSpikeBall), typeof(iChain), 1)]
	public class iChainMaceBall : AutoSpriteThrowingWeapon { }

	[EntityAttribute.ItemCombination(typeof(iBowlingBall), typeof(iGunpowder), 1)]
	public class iBomb : AutoSpriteThrowingWeapon { }

	[EntityAttribute.ItemCombination(typeof(iIronHook), typeof(iIngotIron), typeof(iIngotIron), typeof(iIngotIron), 1)]
	public class iAnchor : AutoSpriteThrowingWeapon { }

	[EntityAttribute.ItemCombination(typeof(iThrowingAxe), typeof(iIngotGold), 1)]
	public class iCrossAxe : AutoSpriteThrowingWeapon { }

	[EntityAttribute.ItemCombination(typeof(iGrapePurple), typeof(iGunpowder), 1)]
	public class iGrapeBomb : AutoSpriteThrowingWeapon { }

	[EntityAttribute.ItemCombination(typeof(iPotionEmpty), typeof(iBlackPepper), typeof(iGunpowder), 1)]
	public class iTearGas : AutoSpriteThrowingWeapon { }

	[EntityAttribute.ItemCombination(typeof(iPotionEmpty), typeof(iGunpowder), 1)]
	public class iGrenade : AutoSpriteThrowingWeapon { }


}
