using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {

	[EntityAttribute.ItemCombination(typeof(iStonePolished), typeof(iTreeBranch), 1)]
	public class iHammerWood : AutoSpriteHammer { }
	[EntityAttribute.ItemCombination(typeof(iHammerWood), typeof(iIngotIron), 1)]
	public class iHammerIron : AutoSpriteHammer { }
	[EntityAttribute.ItemCombination(typeof(iHammerIron), typeof(iIngotGold), 1)]
	public class iHammerGold : AutoSpriteHammer { }
	[EntityAttribute.ItemCombination(typeof(iSpikeBall), typeof(iTreeBranch), 1)]
	public class iMaceRound : AutoSpriteHammer { }
	[EntityAttribute.ItemCombination(typeof(iSkull), typeof(iTreeBranch), 1)]
	public class iMaceSkull : AutoSpriteHammer { }
	[EntityAttribute.ItemCombination(typeof(iTreeTrunk), typeof(iTreeBranch), typeof(iTreeBranch), 1)]
	public class iBaseballBatWood : AutoSpriteHammer { }
	[EntityAttribute.ItemCombination(typeof(iBolt), typeof(iBolt), typeof(iBolt), typeof(iBaseballBatWood), 1)]
	public class iMaceSpiked : AutoSpriteHammer { }
	[EntityAttribute.ItemCombination(typeof(iIngotIron), typeof(iIngotIron), typeof(iIngotIron), typeof(iBaseballBatWood), 1)]
	public class iBian : AutoSpriteHammer {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.OneOnEachHand;
	}
	[EntityAttribute.ItemCombination(typeof(iTreeTrunk), typeof(iTreeTrunk), typeof(iTreeTrunk), typeof(iTreeBranch), 1)]
	public class iHammerRiceCake : AutoSpriteHammer {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
	[EntityAttribute.ItemCombination(typeof(iHorn), typeof(iTreeBranch), 1)]
	public class iHammerGoatHorn : AutoSpriteHammer { }
	[EntityAttribute.ItemCombination(typeof(iIngotIron), typeof(iBaseballBatWood), 1)]
	public class iBaseballBatIron : AutoSpriteHammer { }
	[EntityAttribute.ItemCombination(typeof(iHammerIron), typeof(iRuneLightning), typeof(iRuneLightning), typeof(iRuneLightning), 1)]
	public class iHammerThunder : AutoSpriteHammer {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
	[EntityAttribute.ItemCombination(typeof(iStonePolished), typeof(iStonePolished), typeof(iStonePolished), typeof(iHammerWood), 1)]
	public class iHammerMoai : AutoSpriteHammer {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
	[EntityAttribute.ItemCombination(typeof(iHammerGold), typeof(iIngotGold), 1)]
	public class iHammerPaladin : AutoSpriteHammer {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
	[EntityAttribute.ItemCombination(typeof(iHammerGold), typeof(iRubyRed), typeof(iRubyRed), typeof(iRubyRed), 1)]
	public class iHammerRuby : AutoSpriteHammer {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
}
