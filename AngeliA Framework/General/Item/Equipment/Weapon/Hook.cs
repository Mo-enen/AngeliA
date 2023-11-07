using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {
	

	[EntityAttribute.ItemCombination(typeof(iScimitar), typeof(iTreeBranch), 1)]
	public class iScytheWood : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
	[EntityAttribute.ItemCombination(typeof(iScytheWood), typeof(iIngotIron), 1)]
	public class iScytheIron : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
	[EntityAttribute.ItemCombination(typeof(iScytheIron), typeof(iIngotGold), 1)]
	public class iScytheGold : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
	}
	[EntityAttribute.ItemCombination(typeof(iIronHook), typeof(iTreeBranch), 1)]
	public class iSickle : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iIronHook), typeof(iWuXingHook), 1)]
	public class iHookIron : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.OneOnEachHand;
	}
	[EntityAttribute.ItemCombination(typeof(iHookIron), typeof(iIngotGold), 1)]
	public class iHookGold : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.OneOnEachHand;
	}
	[EntityAttribute.ItemCombination(typeof(iIronHook), typeof(iIngotIron), 1)]
	public class iHookHand : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iIronHook), typeof(iTreeBranch), typeof(iTreeBranch), 1)]
	public class iHookJungle : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iChain), typeof(iMeatBone), typeof(iTreeBranch), 1)]
	public class iHookBone : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iComb), typeof(iSickle), 1)]
	public class iHookJagged : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iIronHook), typeof(iIronHook), typeof(iIronHook), 1)]
	public class iHookTripple : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iSickle), typeof(iSickle), 1)]
	public class iHookBig : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iIronHook), typeof(iIronHook), typeof(iSickle), 1)]
	public class iHookPudge : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iSickle), typeof(iErgonomicAxe), 1)]
	public class iHookChicken : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iHookIron), typeof(iSalt), typeof(iRuneLightning), 1)]
	public class iHookRusty : AutoSpriteHook {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.OneOnEachHand;
	}
}
