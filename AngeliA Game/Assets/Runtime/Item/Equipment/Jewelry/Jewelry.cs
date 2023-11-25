using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {


	[EntityAttribute.ItemCombination(typeof(iGem), typeof(iRope), typeof(iTreeBranch), 1)]
	public class iNecklaceWood : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[EntityAttribute.ItemCombination(typeof(iGem), typeof(iRope), typeof(iIngotIron), 1)]
	[EntityAttribute.ItemCombination(typeof(iNecklaceWood), typeof(iIngotIron), 1)]
	public class iNecklaceIron : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[EntityAttribute.ItemCombination(typeof(iGem), typeof(iRope), typeof(iIngotGold), 1)]
	[EntityAttribute.ItemCombination(typeof(iNecklaceIron), typeof(iIngotGold), 1)]
	public class iNecklaceGold : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[EntityAttribute.ItemCombination(typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
	[EntityAttribute.ItemCombination(typeof(iIngotGold), typeof(iBraceletIron), 1)]
	public class iBraceletGold : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[EntityAttribute.ItemCombination(typeof(iIngotIron), typeof(iIngotIron), typeof(iIngotIron), 1)]
	public class iBraceletIron : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[EntityAttribute.ItemCombination(typeof(iStonePolished), typeof(iAmber), typeof(iCuteGhost), 1)]
	public class iMagatama : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[EntityAttribute.ItemCombination(typeof(iGemOrange), typeof(iGemGreen), typeof(iGemRed), typeof(iGemBlue), 1)]
	public class iPendant : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[EntityAttribute.ItemCombination(typeof(iRope), typeof(iGourd), typeof(iGourd), typeof(iGourd), 1)]
	public class iPrayerBeads : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[EntityAttribute.ItemCombination(typeof(iPrayerBeads), typeof(iIngotGold), 1)]
	public class iPrayerBeadsGold : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iBeetle), typeof(iButter), 1)]
	public class iAmber : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[EntityAttribute.ItemCombination(typeof(iIngotGold), typeof(iIngotGold), 1)]
	public class iRingGold : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[EntityAttribute.ItemCombination(typeof(iRubyRed), typeof(iIngotGold), 1)]
	public class iRubyJewelRed : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[EntityAttribute.ItemCombination(typeof(iRubyBlue), typeof(iIngotGold), 1)]
	public class iRubyJewelBlue : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[EntityAttribute.ItemCombination(typeof(iRubyOrange), typeof(iIngotGold), 1)]
	public class iRubyJewelYellow : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[EntityAttribute.ItemCombination(typeof(iIngotGold), typeof(iRubyGreen), 1)]
	public class iRubyJewelGreen : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }


}
