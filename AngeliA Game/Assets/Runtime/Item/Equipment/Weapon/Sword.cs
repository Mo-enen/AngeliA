using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {




	[EntityAttribute.ItemCombination(typeof(iItemWoodBoard), typeof(iTreeBranch), typeof(iTreeBranch), typeof(iTreeBranch), 1)]
	public class iSwordWood : Sword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iSwordWood), typeof(iIngotIron), 1)]
	public class iSwordIron : Sword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iIngotGold), typeof(iSwordIron), 1)]
	public class iSwordGold : Sword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iIngotIron), typeof(iTreeBranch), typeof(iTreeBranch), 1)]
	public class iDagger : Sword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iGemRed), typeof(iGemRed), typeof(iGemRed), typeof(iSwordIron), 1)]
	public class iSwordCrimson : Sword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iRubyRed), typeof(iRubyRed), typeof(iRubyRed), typeof(iSwordIron), 1)]
	public class iSwordScarlet : Sword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iIngotIron), typeof(iTreeBranch), typeof(iTreeBranch), typeof(iTreeBranch), 1)]
	public class iScimitar : Sword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iSkull), typeof(iMeatBone), typeof(iMeatBone), typeof(iSwordIron), 1)]
	public class iSwordPirate : Sword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iSwordIron), typeof(iRuneLightning), 1)]
	public class iSwordAgile : Sword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iScimitar), typeof(iRuneLightning), 1)]
	public class iScimitarAgile : Sword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iComb), typeof(iSwordIron), 1)]
	public class iSwordJagged : Sword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iSwordGold), typeof(iSwordGold), 1)]
	public class iSwordGreat : Sword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iClay), typeof(iGunpowder), typeof(iCharcoal), typeof(iSwordIron), 1)]
	public class iSwordDark : Sword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iBoStaffWood), typeof(iSwordIron), 1)]
	public class iSwordCrutch : Sword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iScimitar), typeof(iScimitar), 1)]
	public class iKnifeGiant : Sword {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
	}
}
