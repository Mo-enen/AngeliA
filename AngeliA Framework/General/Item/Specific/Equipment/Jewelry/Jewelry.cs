using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {


	[ItemCombination(typeof(iGem), typeof(iRope), typeof(iTreeBranch), 1)]
	public class iNecklaceWood : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[ItemCombination(typeof(iGem), typeof(iRope), typeof(iIngotIron), 1)]
	[ItemCombination(typeof(iNecklaceWood), typeof(iIngotIron), 1)]
	public class iNecklaceIron : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[ItemCombination(typeof(iGem), typeof(iRope), typeof(iIngotGold), 1)]
	[ItemCombination(typeof(iNecklaceIron), typeof(iIngotGold), 1)]
	public class iNecklaceGold : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[ItemCombination(typeof(iIngotGold), typeof(iIngotGold), typeof(iIngotGold), 1)]
	[ItemCombination(typeof(iIngotGold), typeof(iBraceletIron), 1)]
	public class iBraceletGold : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[ItemCombination(typeof(iIngotIron), typeof(iIngotIron), typeof(iIngotIron), 1)]
	public class iBraceletIron : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[ItemCombination(typeof(iStonePolished), typeof(iAmber), typeof(iCuteGhost), 1)]
	public class iMagatama : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[ItemCombination(typeof(iGemOrange), typeof(iGemGreen), typeof(iGemRed), typeof(iGemBlue), 1)]
	public class iPendant : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[ItemCombination(typeof(iRope), typeof(iGourd), typeof(iGourd), typeof(iGourd), 1)]
	public class iPrayerBeads : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[ItemCombination(typeof(iPrayerBeads), typeof(iIngotGold), 1)]
	public class iPrayerBeadsGold : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[ItemCombination(typeof(iRuneWater), typeof(iBeetle), typeof(iButter), 1)]
	public class iAmber : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[ItemCombination(typeof(iIngotGold), typeof(iIngotGold), 1)]
	public class iRingGold : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[ItemCombination(typeof(iRubyRed), typeof(iIngotGold), 1)]
	public class iRubyJewelRed : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[ItemCombination(typeof(iRubyBlue), typeof(iIngotGold), 1)]
	public class iRubyJewelBlue : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[ItemCombination(typeof(iRubyOrange), typeof(iIngotGold), 1)]
	public class iRubyJewelYellow : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }

	[ItemCombination(typeof(iIngotGold), typeof(iRubyGreen), 1)]
	public class iRubyJewelGreen : Equipment { public override EquipmentType EquipmentType => EquipmentType.Jewelry; }


}
