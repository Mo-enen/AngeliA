using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {


	[EntityAttribute.ItemCombination(typeof(iDagger), typeof(iDagger), typeof(iDagger), 1)]
	public class iClawWood : Claw { }

	[EntityAttribute.ItemCombination(typeof(iClawWood), typeof(iIngotIron), 1)]
	public class iClawIron : Claw { }

	[EntityAttribute.ItemCombination(typeof(iClawIron), typeof(iIngotGold), 1)]
	public class iClawGold : Claw { }

	[EntityAttribute.ItemCombination(typeof(iIngotIron), typeof(iWuXingHook), 1)]
	public class iMandarinDuckAxe : Claw { }

	[EntityAttribute.ItemCombination(typeof(iPaw), typeof(iDagger), typeof(iDagger), typeof(iDagger), 1)]
	public class iClawCat : Claw { }

	[EntityAttribute.ItemCombination(typeof(iPaw), typeof(iClawCat), 1)]
	public class iClawFox : Claw { }

	[EntityAttribute.ItemCombination(typeof(iIngotGold), typeof(iClawGold), 1)]
	public class iKatars : Claw { }

	[EntityAttribute.ItemCombination(typeof(iKatars), typeof(iKatars), typeof(iKatars), 1)]
	public class iKatarsTripple : Claw { }

	[EntityAttribute.ItemCombination(typeof(iNeedle), typeof(iNeedle), typeof(iRingGold), typeof(iRingGold), 1)]
	public class iEmeiPiercer : Claw { }

	[EntityAttribute.ItemCombination(typeof(iEmeiPiercer), typeof(iTreeBranch), 1)]
	public class iBaton : Claw { }

	[EntityAttribute.ItemCombination(typeof(iFist), typeof(iIngotIron), 1)]
	public class iKnuckleDuster : Claw { }

	[EntityAttribute.ItemCombination(typeof(iEmeiPiercer), typeof(iEmeiPiercer), typeof(iEmeiPiercer), 1)]
	public class iEmeiFork : Claw { }

	[EntityAttribute.ItemCombination(typeof(iIronHook), typeof(iIronHook), typeof(iTreeBranch), 1)]
	public class iWuXingHook : Claw { }

	[EntityAttribute.ItemCombination(typeof(iClawWood), typeof(iRubyRed), 1)]
	public class iKatarsRuby : Claw { }

	[EntityAttribute.ItemCombination(typeof(iComb), typeof(iKatars), 1)]
	public class iKatarsJagged : Claw { }


}
