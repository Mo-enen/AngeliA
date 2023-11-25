using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


namespace AngeliaFramework {

	[EntityAttribute.ItemCombination(typeof(iBoStaffWood), typeof(iDagger), 1)]
	public class iSpearWood : Polearm { }
	[EntityAttribute.ItemCombination(typeof(iSpearWood), typeof(iIngotIron), 1)]
	public class iSpearIron : Polearm { }
	[EntityAttribute.ItemCombination(typeof(iIngotGold), typeof(iSpearIron), 1)]
	public class iSpearGold : Polearm { }
	[EntityAttribute.ItemCombination(typeof(iSpearIron), typeof(iIngotIron), 1)]
	public class iSpearHeavy : Polearm { }
	[EntityAttribute.ItemCombination(typeof(iMagatama), typeof(iMagatama), typeof(iMagatama), typeof(iSpearIron), 1)]
	[EntityAttribute.ItemCombination(typeof(iRibbon), typeof(iTreeBranch), typeof(iTreeBranch), typeof(iTreeBranch), 1)]
	public class iBoStaffWood : Polearm { }
	[EntityAttribute.ItemCombination(typeof(iBoStaffWood), typeof(iIngotIron), 1)]
	public class iBoStaffIron : Polearm { }
	[EntityAttribute.ItemCombination(typeof(iBoStaffIron), typeof(iIngotGold), 1)]
	public class iBoStaffGold : Polearm { }
	[EntityAttribute.ItemCombination(typeof(iBoStaffWood), typeof(iKnifeGiant), 1)]
	public class iNaginata : Polearm { }
	[EntityAttribute.ItemCombination(typeof(iAxeWood), typeof(iBoStaffWood), 1)]
	public class iHalberd : Polearm { }
	[EntityAttribute.ItemCombination(typeof(iBoStaffWood), typeof(iWuXingHook), typeof(iWuXingHook), 1)]
	public class iJi : Polearm { }
	[EntityAttribute.ItemCombination(typeof(iAxeWood), typeof(iBoStaffWood), typeof(iWuXingHook), 1)]
	public class iMonkSpade : Polearm { }
	[EntityAttribute.ItemCombination(typeof(iBoStaffWood), typeof(iBolt), typeof(iBolt), typeof(iWuXingHook), 1)]
	public class iManCatcher : Polearm { }
	[EntityAttribute.ItemCombination(typeof(iScimitar), typeof(iScimitar), typeof(iBoStaffWood), 1)]
	public class iSwallow : Polearm { }
	[EntityAttribute.ItemCombination(typeof(iBoStaffWood), typeof(iDagger), typeof(iDagger), 1)]
	public class iFork : Polearm { }
	[EntityAttribute.ItemCombination(typeof(iBoStaffWood), typeof(iBolt), typeof(iBolt), typeof(iBolt), 1)]
	public class iBrandistock : Polearm { }
}
