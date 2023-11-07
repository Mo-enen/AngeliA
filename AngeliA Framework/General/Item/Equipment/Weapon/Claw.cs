using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {


	[EntityAttribute.ItemCombination(typeof(iDagger), typeof(iDagger), typeof(iDagger), 1)]
	public class iClawWood : AutoSpriteClaw { }

	[EntityAttribute.ItemCombination(typeof(iClawWood), typeof(iIngotIron), 1)]
	public class iClawIron : AutoSpriteClaw { }

	[EntityAttribute.ItemCombination(typeof(iClawIron), typeof(iIngotGold), 1)]
	public class iClawGold : AutoSpriteClaw { }

	[EntityAttribute.ItemCombination(typeof(iIngotIron), typeof(iWuXingHook), 1)]
	public class iMandarinDuckAxe : AutoSpriteClaw { }

	[EntityAttribute.ItemCombination(typeof(iPaw), typeof(iDagger), typeof(iDagger), typeof(iDagger), 1)]
	public class iClawCat : AutoSpriteClaw { }

	[EntityAttribute.ItemCombination(typeof(iPaw), typeof(iClawCat), 1)]
	public class iClawFox : AutoSpriteClaw { }

	[EntityAttribute.ItemCombination(typeof(iIngotGold), typeof(iClawGold), 1)]
	public class iKatars : AutoSpriteClaw { }

	[EntityAttribute.ItemCombination(typeof(iKatars), typeof(iKatars), typeof(iKatars), 1)]
	public class iKatarsTripple : AutoSpriteClaw { }

	[EntityAttribute.ItemCombination(typeof(iNeedle), typeof(iNeedle), typeof(iRingGold), typeof(iRingGold), 1)]
	public class iEmeiPiercer : AutoSpriteClaw { }

	[EntityAttribute.ItemCombination(typeof(iEmeiPiercer), typeof(iTreeBranch), 1)]
	public class iBaton : AutoSpriteClaw { }

	[EntityAttribute.ItemCombination(typeof(iFist), typeof(iIngotIron), 1)]
	public class iKnuckleDuster : AutoSpriteClaw { }

	[EntityAttribute.ItemCombination(typeof(iEmeiPiercer), typeof(iEmeiPiercer), typeof(iEmeiPiercer), 1)]
	public class iEmeiFork : AutoSpriteClaw { }

	[EntityAttribute.ItemCombination(typeof(iIronHook), typeof(iIronHook), typeof(iTreeBranch), 1)]
	public class iWuXingHook : AutoSpriteClaw { }

	[EntityAttribute.ItemCombination(typeof(iClawWood), typeof(iRubyRed), 1)]
	public class iKatarsRuby : AutoSpriteClaw { }

	[EntityAttribute.ItemCombination(typeof(iComb), typeof(iKatars), 1)]
	public class iKatarsJagged : AutoSpriteClaw { }


}
