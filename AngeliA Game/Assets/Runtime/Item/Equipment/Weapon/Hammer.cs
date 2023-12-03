using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {

	[EntityAttribute.ItemCombination(typeof(iStonePolished), typeof(iTreeBranch), 1)]
	public class iHammerWood : Hammer { }
	[EntityAttribute.ItemCombination(typeof(iHammerWood), typeof(iIngotIron), 1)]
	public class iHammerIron : Hammer { }
	[EntityAttribute.ItemCombination(typeof(iHammerIron), typeof(iIngotGold), 1)]
	public class iHammerGold : Hammer { }
	[EntityAttribute.ItemCombination(typeof(iSpikeBall), typeof(iTreeBranch), 1)]
	public class iMaceRound : Hammer { }
	[EntityAttribute.ItemCombination(typeof(iSkull), typeof(iTreeBranch), 1)]
	public class iMaceSkull : Hammer { }
	[EntityAttribute.ItemCombination(typeof(iTreeTrunk), typeof(iTreeBranch), typeof(iTreeBranch), 1)]
	public class iBaseballBatWood : Hammer { }
	[EntityAttribute.ItemCombination(typeof(iBolt), typeof(iBolt), typeof(iBolt), typeof(iBaseballBatWood), 1)]
	public class iMaceSpiked : Hammer { }
	[EntityAttribute.ItemCombination(typeof(iIngotIron), typeof(iIngotIron), typeof(iIngotIron), typeof(iBaseballBatWood), 1)]
	public class iBian : Hammer {
		public override WeaponHandheld Handheld => WeaponHandheld.OneOnEachHand;
	}
	[EntityAttribute.ItemCombination(typeof(iTreeTrunk), typeof(iTreeTrunk), typeof(iTreeTrunk), typeof(iTreeBranch), 1)]
	public class iHammerRiceCake : Hammer {
		public override WeaponHandheld Handheld => WeaponHandheld.Pole;
	}
	[EntityAttribute.ItemCombination(typeof(iHorn), typeof(iTreeBranch), 1)]
	public class iHammerGoatHorn : Hammer { }
	[EntityAttribute.ItemCombination(typeof(iIngotIron), typeof(iBaseballBatWood), 1)]
	public class iBaseballBatIron : Hammer { }
	[EntityAttribute.ItemCombination(typeof(iHammerIron), typeof(iRuneLightning), typeof(iRuneLightning), typeof(iRuneLightning), 1)]
	public class iHammerThunder : Hammer {
		public override WeaponHandheld Handheld => WeaponHandheld.Pole;
	}
	[EntityAttribute.ItemCombination(typeof(iStonePolished), typeof(iStonePolished), typeof(iStonePolished), typeof(iHammerWood), 1)]
	public class iHammerMoai : Hammer {
		public override WeaponHandheld Handheld => WeaponHandheld.Pole;
	}
	[EntityAttribute.ItemCombination(typeof(iHammerGold), typeof(iIngotGold), 1)]
	public class iHammerPaladin : Hammer {
		public override WeaponHandheld Handheld => WeaponHandheld.Pole;
	}
	[EntityAttribute.ItemCombination(typeof(iHammerGold), typeof(iRubyRed), typeof(iRubyRed), typeof(iRubyRed), 1)]
	public class iHammerRuby : Hammer {
		public override WeaponHandheld Handheld => WeaponHandheld.Pole;
	}
}
