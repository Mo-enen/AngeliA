using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {




	[EntityAttribute.ItemCombination(typeof(iItemWoodBoard), typeof(iTreeBranch), typeof(iTreeBranch), typeof(iTreeBranch), 1)]
	public class iSwordWood : AutoSpriteSword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iSwordWood), typeof(iIngotIron), 1)]
	public class iSwordIron : AutoSpriteSword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iIngotGold), typeof(iSwordIron), 1)]
	public class iSwordGold : AutoSpriteSword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iIngotIron), typeof(iTreeBranch), typeof(iTreeBranch), 1)]
	public class iDagger : AutoSpriteSword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iGemRed), typeof(iGemRed), typeof(iGemRed), typeof(iSwordIron), 1)]
	public class iSwordCrimson : AutoSpriteSword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iRubyRed), typeof(iRubyRed), typeof(iRubyRed), typeof(iSwordIron), 1)]
	public class iSwordScarlet : AutoSpriteSword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iIngotIron), typeof(iTreeBranch), typeof(iTreeBranch), typeof(iTreeBranch), 1)]
	public class iScimitar : AutoSpriteSword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iSkull), typeof(iMeatBone), typeof(iMeatBone), typeof(iSwordIron), 1)]
	public class iSwordPirate : AutoSpriteSword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iSwordIron), typeof(iRuneLightning), 1)]
	public class iSwordAgile : AutoSpriteSword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iScimitar), typeof(iRuneLightning), 1)]
	public class iScimitarAgile : AutoSpriteSword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iComb), typeof(iSwordIron), 1)]
	public class iSwordJagged : AutoSpriteSword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iSwordGold), typeof(iSwordGold), 1)]
	public class iSwordGreat : AutoSpriteSword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iClay), typeof(iGunpowder), typeof(iCharcoal), typeof(iSwordIron), 1)]
	public class iSwordDark : AutoSpriteSword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iBoStaffWood), typeof(iSwordIron), 1)]
	public class iSwordCrutch : AutoSpriteSword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iScimitar), typeof(iScimitar), 1)]
	public class iKnifeGiant : AutoSpriteSword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
	}
}
