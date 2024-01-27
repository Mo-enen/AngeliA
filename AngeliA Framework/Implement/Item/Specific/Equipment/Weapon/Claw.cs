using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {


	[ItemCombination(typeof(iDagger), typeof(iDagger), typeof(iDagger), 1)]
	public class iClawWood : Claw { }

	[ItemCombination(typeof(iClawWood), typeof(iIngotIron), 1)]
	public class iClawIron : Claw { }

	[ItemCombination(typeof(iClawIron), typeof(iIngotGold), 1)]
	public class iClawGold : Claw { }

	[ItemCombination(typeof(iIngotIron), typeof(iWuXingHook), 1)]
	public class iMandarinDuckAxe : Claw { }

	[ItemCombination(typeof(iPaw), typeof(iDagger), typeof(iDagger), typeof(iDagger), 1)]
	public class iClawCat : Claw { }

	[ItemCombination(typeof(iPaw), typeof(iClawCat), 1)]
	public class iClawFox : Claw { }

	[ItemCombination(typeof(iIngotGold), typeof(iClawGold), 1)]
	public class iKatars : Claw { }

	[ItemCombination(typeof(iKatars), typeof(iKatars), typeof(iKatars), 1)]
	public class iKatarsTripple : Claw { }

	[ItemCombination(typeof(iNeedle), typeof(iNeedle), typeof(iRingGold), typeof(iRingGold), 1)]
	public class iEmeiPiercer : Claw { }

	[ItemCombination(typeof(iEmeiPiercer), typeof(iTreeBranch), 1)]
	public class iBaton : Claw { }

	[ItemCombination(typeof(iFist), typeof(iIngotIron), 1)]
	public class iKnuckleDuster : Claw { }

	[ItemCombination(typeof(iEmeiPiercer), typeof(iEmeiPiercer), typeof(iEmeiPiercer), 1)]
	public class iEmeiFork : Claw { }

	[ItemCombination(typeof(iIronHook), typeof(iIronHook), typeof(iTreeBranch), 1)]
	public class iWuXingHook : Claw { }

	[ItemCombination(typeof(iClawWood), typeof(iRubyRed), 1)]
	public class iKatarsRuby : Claw { }

	[ItemCombination(typeof(iComb), typeof(iKatars), 1)]
	public class iKatarsJagged : Claw { }


}
