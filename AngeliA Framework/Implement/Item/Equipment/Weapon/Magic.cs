using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework {


	// Magic
	public abstract class MagicWeapon<B> : MagicWeapon where B : MovableBullet {
		public MagicWeapon () => BulletID = typeof(B).AngeHash();
	}
	public abstract class MagicWeapon : ProjectileWeapon {
		public sealed override WeaponType WeaponType => WeaponType.Magic;
	}


	// Implement
	[ItemCombination(typeof(iBook), typeof(iRuneCube), typeof(iTreeBranch), 1)]
	public class iWand : MagicWeapon<iWand.WandBullet> {
		public class WandBullet : MovableBullet {

		}
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	}


	[ItemCombination(typeof(iWand), typeof(iCrystalBall), 1)]
	public class iWandOrb : MagicWeapon<iWandOrb.WandOrbBullet> {
		public class WandOrbBullet : MovableBullet {

		}
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	}


	[ItemCombination(typeof(iRuneFire), typeof(iBookRed), typeof(iRubyRed), typeof(iBoStaffWood), 1)]
	public class iStaffFire : MagicWeapon<iStaffFire.StaffFireBullet> {
		public class StaffFireBullet : MovableBullet {

		}
		public override WeaponHandheld Handheld => WeaponHandheld.Pole;
	}


	[ItemCombination(typeof(iRuneWater), typeof(iBoStaffWood), typeof(iBookBlue), typeof(iRubyBlue), 1)]
	public class iStaffWater : MagicWeapon<iStaffWater.StaffWaterBullet> {
		public class StaffWaterBullet : MovableBullet {

		}
		public override WeaponHandheld Handheld => WeaponHandheld.Pole;
	}


	[ItemCombination(typeof(iRubyOrange), typeof(iBoStaffWood), typeof(iBookYellow), typeof(iRuneLightning), 1)]
	public class iStaffLightning : MagicWeapon<iStaffLightning.StaffLightningBullet> {
		public class StaffLightningBullet : MovableBullet {

		}
		public override WeaponHandheld Handheld => WeaponHandheld.Pole;
	}


	[ItemCombination(typeof(iRunePoison), typeof(iBoStaffWood), typeof(iBookGreen), typeof(iRubyGreen), 1)]
	public class iStaffPoision : MagicWeapon<iStaffPoision.StaffPoisionBullet> {
		public class StaffPoisionBullet : MovableBullet {

		}
		public override WeaponHandheld Handheld => WeaponHandheld.Pole;
	}


	[ItemCombination(typeof(iRunePoison), typeof(iRuneFire), typeof(iSkull), typeof(iRuneLightning), 1)]
	public class iRitualSkull : MagicWeapon<iRitualSkull.RitualSkullBullet> {
		public class RitualSkullBullet : MovableBullet {

		}
		public override WeaponHandheld Handheld => WeaponHandheld.Float;
	}


	[ItemCombination(typeof(iItemWoodBoard), typeof(iBook), typeof(iScroll), 1)]
	public class iBambooSlips : MagicWeapon<iBambooSlips.BambooSlipsBullet> {
		public class BambooSlipsBullet : MovableBullet {

		}
		public override WeaponHandheld Handheld => WeaponHandheld.Float;
	}


	[ItemCombination(typeof(iRunePoison), typeof(iRuneFire), typeof(iRuneCube), typeof(iRuneLightning), 1)]
	public class iRitualRuneCube : MagicWeapon<iRitualRuneCube.RitualRuneCubeBullet> {
		public class RitualRuneCubeBullet : MovableBullet {

		}
		public override WeaponHandheld Handheld => WeaponHandheld.Float;
	}


	[ItemCombination(typeof(iRunePoison), typeof(iRuneFire), typeof(iGoblinHead), typeof(iRuneLightning), 1)]
	public class iGoblinTrophy : MagicWeapon<iGoblinTrophy.GoblinTrophyBullet> {
		public class GoblinTrophyBullet : MovableBullet {

		}
		public override WeaponHandheld Handheld => WeaponHandheld.Float;
	}


	[ItemCombination(typeof(iRuneCube), typeof(iCrystalBall), 1)]
	public class iMagicOrb : MagicWeapon<iMagicOrb.MagicOrbBullet> {
		public class MagicOrbBullet : MovableBullet {

		}
		public override WeaponHandheld Handheld => WeaponHandheld.Float;
	}


	[ItemCombination(typeof(iEyeBall), typeof(iRuneCube), 1)]
	public class iMagicEyeball : MagicWeapon<iMagicEyeball.MagicEyeballBullet> {
		public class MagicEyeballBullet : MovableBullet {

		}
		public override WeaponHandheld Handheld => WeaponHandheld.Float;
	}


	[ItemCombination(typeof(iPotionBlue), typeof(iPotionRed), typeof(iRuneCube), 1)]
	public class iMagicPotion : MagicWeapon<iMagicPotion.MagicPotionBullet> {
		public class MagicPotionBullet : MovableBullet {

		}
		public override WeaponHandheld Handheld => WeaponHandheld.Float;
	}


	[ItemCombination(typeof(iWand), typeof(iStar), 1)]
	public class iWandStar : MagicWeapon<iWandStar.WandStarBullet> {
		public class WandStarBullet : MovableBullet {

		}
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	}


	[ItemCombination(typeof(iWand), typeof(iCuteGhost), 1)]
	public class iWandFairy : MagicWeapon<iWandFairy.WandFairyBullet> {
		public class WandFairyBullet : MovableBullet {

		}
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	}


}
