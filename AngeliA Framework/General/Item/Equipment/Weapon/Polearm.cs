using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


namespace AngeliaFramework {

	[EntityAttribute.ItemCombination(typeof(iBoStaffWood), typeof(iDagger), 1)]
	public class iSpearWood : AutoSpritePolearm { }
	[EntityAttribute.ItemCombination(typeof(iSpearWood), typeof(iIngotIron), 1)]
	public class iSpearIron : AutoSpritePolearm { }
	[EntityAttribute.ItemCombination(typeof(iIngotGold), typeof(iSpearIron), 1)]
	public class iSpearGold : AutoSpritePolearm { }
	[EntityAttribute.ItemCombination(typeof(iMagatama), typeof(iMagatama), typeof(iMagatama), typeof(iSpearIron), 1)]
	[EntityAttribute.ItemCombination(typeof(iRibbon), typeof(iTreeBranch), typeof(iTreeBranch), typeof(iTreeBranch), 1)]
	public class iBoStaffWood : AutoSpritePolearm { }
	[EntityAttribute.ItemCombination(typeof(iBoStaffWood), typeof(iIngotIron), 1)]
	public class iBoStaffIron : AutoSpritePolearm { }
	[EntityAttribute.ItemCombination(typeof(iBoStaffIron), typeof(iIngotGold), 1)]
	public class iBoStaffGold : AutoSpritePolearm { }
	[EntityAttribute.ItemCombination(typeof(iBoStaffWood), typeof(iKnifeGiant), 1)]
	public class iNaginata : AutoSpritePolearm { }
	[EntityAttribute.ItemCombination(typeof(iAxeWood), typeof(iBoStaffWood), 1)]
	public class iHalberd : AutoSpritePolearm { }
	[EntityAttribute.ItemCombination(typeof(iBoStaffWood), typeof(iWuXingHook), typeof(iWuXingHook), 1)]
	public class iJi : AutoSpritePolearm { }
	[EntityAttribute.ItemCombination(typeof(iAxeWood), typeof(iBoStaffWood), typeof(iWuXingHook), 1)]
	public class iMonkSpade : AutoSpritePolearm { }
	[EntityAttribute.ItemCombination(typeof(iBoStaffWood), typeof(iBolt), typeof(iBolt), typeof(iWuXingHook), 1)]
	public class iManCatcher : AutoSpritePolearm { }
	[EntityAttribute.ItemCombination(typeof(iScimitar), typeof(iScimitar), typeof(iBoStaffWood), 1)]
	public class iSwallow : AutoSpritePolearm { }
	[EntityAttribute.ItemCombination(typeof(iBoStaffWood), typeof(iDagger), typeof(iDagger), 1)]
	public class iFork : AutoSpritePolearm { }
	[EntityAttribute.ItemCombination(typeof(iBoStaffWood), typeof(iBolt), typeof(iBolt), typeof(iBolt), 1)]
	public class iBrandistock : AutoSpritePolearm { }
}
