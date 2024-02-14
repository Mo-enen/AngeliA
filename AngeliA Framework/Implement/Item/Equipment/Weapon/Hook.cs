using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework {



	// Hook
	public abstract class Hook<B> : Hook where B : MeleeBullet {
		public Hook () => BulletID = typeof(B).AngeHash();
	}
	public abstract class Hook : MeleeWeapon {
		public sealed override WeaponType WeaponType => WeaponType.Hook;
		public override int RangeXLeft => 275;
		public override int RangeXRight => 275;
		public override int RangeY => 432;
	}


	// Implement
	[ItemCombination(typeof(iScimitar), typeof(iTreeBranch), 1)]
	public class iScytheWood : Hook {
		public override WeaponHandheld Handheld => WeaponHandheld.Pole;
	}
	[ItemCombination(typeof(iScytheWood), typeof(iIngotIron), 1)]
	public class iScytheIron : Hook {
		public override WeaponHandheld Handheld => WeaponHandheld.Pole;
	}
	[ItemCombination(typeof(iScytheIron), typeof(iIngotGold), 1)]
	public class iScytheGold : Hook {
		public override WeaponHandheld Handheld => WeaponHandheld.Pole;
	}
	[ItemCombination(typeof(iIronHook), typeof(iTreeBranch), 1)]
	public class iSickle : Hook {
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	}
	[ItemCombination(typeof(iIronHook), typeof(iWuXingHook), 1)]
	public class iHookIron : Hook {
		public override WeaponHandheld Handheld => WeaponHandheld.OneOnEachHand;
	}
	[ItemCombination(typeof(iHookIron), typeof(iIngotGold), 1)]
	public class iHookGold : Hook {
		public override WeaponHandheld Handheld => WeaponHandheld.OneOnEachHand;
	}
	[ItemCombination(typeof(iIronHook), typeof(iIngotIron), 1)]
	public class iHookHand : Hook {
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	}
	[ItemCombination(typeof(iIronHook), typeof(iTreeBranch), typeof(iTreeBranch), 1)]
	public class iHookJungle : Hook {
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	}
	[ItemCombination(typeof(iChain), typeof(iMeatBone), typeof(iTreeBranch), 1)]
	public class iHookBone : Hook {
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	}
	[ItemCombination(typeof(iComb), typeof(iSickle), 1)]
	public class iHookJagged : Hook {
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	}
	[ItemCombination(typeof(iIronHook), typeof(iIronHook), typeof(iIronHook), 1)]
	public class iHookTripple : Hook {
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	}
	[ItemCombination(typeof(iSickle), typeof(iSickle), 1)]
	public class iHookBig : Hook {
		public override WeaponHandheld Handheld => WeaponHandheld.DoubleHanded;
	}
	[ItemCombination(typeof(iIronHook), typeof(iIronHook), typeof(iSickle), 1)]
	public class iHookPudge : Hook {
		public override WeaponHandheld Handheld => WeaponHandheld.DoubleHanded;
	}
	[ItemCombination(typeof(iSickle), typeof(iErgonomicAxe), 1)]
	public class iHookChicken : Hook {
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	}
	[ItemCombination(typeof(iRuneWater), typeof(iHookIron), typeof(iSalt), typeof(iRuneLightning), 1)]
	public class iHookRusty : Hook {
		public override WeaponHandheld Handheld => WeaponHandheld.OneOnEachHand;
	}
}
