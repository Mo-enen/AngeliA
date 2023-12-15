using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


namespace AngeliaFramework {

	[ItemCombination(typeof(iBoStaffWood), typeof(iDagger), 1)]
	public class iSpearWood : Polearm { }
	[ItemCombination(typeof(iSpearWood), typeof(iIngotIron), 1)]
	public class iSpearIron : Polearm { }
	[ItemCombination(typeof(iIngotGold), typeof(iSpearIron), 1)]
	public class iSpearGold : Polearm { }
	[ItemCombination(typeof(iSpearIron), typeof(iIngotIron), 1)]
	public class iSpearHeavy : Polearm { }
	[ItemCombination(typeof(iMagatama), typeof(iMagatama), typeof(iMagatama), typeof(iSpearIron), 1)]
	[ItemCombination(typeof(iRibbon), typeof(iTreeBranch), typeof(iTreeBranch), typeof(iTreeBranch), 1)]
	public class iBoStaffWood : Polearm { }
	[ItemCombination(typeof(iBoStaffWood), typeof(iIngotIron), 1)]
	public class iBoStaffIron : Polearm { }
	[ItemCombination(typeof(iBoStaffIron), typeof(iIngotGold), 1)]
	public class iBoStaffGold : Polearm { }
	[ItemCombination(typeof(iBoStaffWood), typeof(iKnifeGiant), 1)]
	public class iNaginata : Polearm { }
	[ItemCombination(typeof(iAxeWood), typeof(iBoStaffWood), 1)]
	public class iHalberd : Polearm { }
	[ItemCombination(typeof(iBoStaffWood), typeof(iWuXingHook), typeof(iWuXingHook), 1)]
	public class iJi : Polearm { }
	[ItemCombination(typeof(iAxeWood), typeof(iBoStaffWood), typeof(iWuXingHook), 1)]
	public class iMonkSpade : Polearm { }
	[ItemCombination(typeof(iBoStaffWood), typeof(iBolt), typeof(iBolt), typeof(iWuXingHook), 1)]
	public class iManCatcher : Polearm { }
	[ItemCombination(typeof(iScimitar), typeof(iScimitar), typeof(iBoStaffWood), 1)]
	public class iSwallow : Polearm { }
	[ItemCombination(typeof(iBoStaffWood), typeof(iDagger), typeof(iDagger), 1)]
	public class iFork : Polearm { }
	[ItemCombination(typeof(iBoStaffWood), typeof(iBolt), typeof(iBolt), typeof(iBolt), 1)]
	public class iBrandistock : Polearm { }
}
