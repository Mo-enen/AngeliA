using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework {


	// Sword
	public abstract class Sword<B> : Sword where B : MeleeBullet {
		public Sword () => BulletID = typeof(B).AngeHash();
	}
	public abstract class Sword : MeleeWeapon {
		public sealed override WeaponType WeaponType => WeaponType.Sword;
		public override int RangeXLeft => 275;
		public override int RangeXRight => 275;
		public override int RangeY => 432;
	}


	// Implement
	[ItemCombination(typeof(iItemWoodBoard), typeof(iTreeBranch), typeof(iTreeBranch), typeof(iTreeBranch), 1)]
	public class iSwordWood : Sword {
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	}
	[ItemCombination(typeof(iSwordWood), typeof(iIngotIron), 1)]
	public class iSwordIron : Sword {
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	}
	[ItemCombination(typeof(iIngotGold), typeof(iSwordIron), 1)]
	public class iSwordGold : Sword {
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	}
	[ItemCombination(typeof(iIngotIron), typeof(iTreeBranch), typeof(iTreeBranch), 1)]
	public class iDagger : Sword {
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	}
	[ItemCombination(typeof(iGemRed), typeof(iGemRed), typeof(iGemRed), typeof(iSwordIron), 1)]
	public class iSwordCrimson : Sword {
		public override WeaponHandheld Handheld => WeaponHandheld.DoubleHanded;
	}
	[ItemCombination(typeof(iRubyRed), typeof(iRubyRed), typeof(iRubyRed), typeof(iSwordIron), 1)]
	public class iSwordScarlet : Sword {
		public override WeaponHandheld Handheld => WeaponHandheld.DoubleHanded;
	}
	[ItemCombination(typeof(iIngotIron), typeof(iTreeBranch), typeof(iTreeBranch), typeof(iTreeBranch), 1)]
	public class iScimitar : Sword {
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	}
	[ItemCombination(typeof(iSkull), typeof(iMeatBone), typeof(iMeatBone), typeof(iSwordIron), 1)]
	public class iSwordPirate : Sword {
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	}
	[ItemCombination(typeof(iSwordIron), typeof(iRuneLightning), 1)]
	public class iSwordAgile : Sword {
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	}
	[ItemCombination(typeof(iScimitar), typeof(iRuneLightning), 1)]
	public class iScimitarAgile : Sword {
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	}
	[ItemCombination(typeof(iComb), typeof(iSwordIron), 1)]
	public class iSwordJagged : Sword {
		public override WeaponHandheld Handheld => WeaponHandheld.DoubleHanded;
	}
	[ItemCombination(typeof(iSwordGold), typeof(iSwordGold), 1)]
	public class iSwordGreat : Sword {
		public override WeaponHandheld Handheld => WeaponHandheld.DoubleHanded;
	}
	[ItemCombination(typeof(iClay), typeof(iGunpowder), typeof(iCharcoal), typeof(iSwordIron), 1)]
	public class iSwordDark : Sword {
		public override WeaponHandheld Handheld => WeaponHandheld.DoubleHanded;
	}
	[ItemCombination(typeof(iBoStaffWood), typeof(iSwordIron), 1)]
	public class iSwordCrutch : Sword {
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	}
	[ItemCombination(typeof(iScimitar), typeof(iScimitar), 1)]
	public class iKnifeGiant : Sword {
		public override WeaponHandheld Handheld => WeaponHandheld.DoubleHanded;
	}
}
