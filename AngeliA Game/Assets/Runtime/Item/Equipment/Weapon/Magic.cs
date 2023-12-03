using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {


	[EntityAttribute.ItemCombination(typeof(iBook), typeof(iRuneCube), typeof(iTreeBranch), 1)]
	public class iWand : MagicWeapon {
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iWand), typeof(iCrystalBall), 1)]
	public class WandOrb : MagicWeapon {
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iRuneFire), typeof(iBookRed), typeof(iRubyRed), typeof(iBoStaffWood), 1)]
	public class iStaffFire : MagicWeapon {
		public override WeaponHandheld Handheld => WeaponHandheld.Pole;
	}
	[EntityAttribute.ItemCombination(typeof(iRuneWater), typeof(iBoStaffWood), typeof(iBookBlue), typeof(iRubyBlue), 1)]
	public class iStaffWater : MagicWeapon {
		public override WeaponHandheld Handheld => WeaponHandheld.Pole;
	}
	[EntityAttribute.ItemCombination(typeof(iRubyOrange), typeof(iBoStaffWood), typeof(iBookYellow), typeof(iRuneLightning), 1)]
	public class iStaffLightning : MagicWeapon {
		public override WeaponHandheld Handheld => WeaponHandheld.Pole;
	}
	[EntityAttribute.ItemCombination(typeof(iRunePoison), typeof(iBoStaffWood), typeof(iBookGreen), typeof(iRubyGreen), 1)]
	public class iStaffPoision : MagicWeapon {
		public override WeaponHandheld Handheld => WeaponHandheld.Pole;
	}
	[EntityAttribute.ItemCombination(typeof(iRunePoison), typeof(iRuneFire), typeof(iSkull), typeof(iRuneLightning), 1)]
	public class iRitualSkull : MagicWeapon {
		public override WeaponHandheld Handheld => WeaponHandheld.Float;
	}
	[EntityAttribute.ItemCombination(typeof(iItemWoodBoard), typeof(iBook), typeof(iScroll), 1)]
	public class iBambooSlips : MagicWeapon {
		public override WeaponHandheld Handheld => WeaponHandheld.Float;
	}
	[EntityAttribute.ItemCombination(typeof(iRunePoison), typeof(iRuneFire), typeof(iRuneCube), typeof(iRuneLightning), 1)]
	public class iRitualRuneCube : MagicWeapon {
		public override WeaponHandheld Handheld => WeaponHandheld.Float;
	}
	[EntityAttribute.ItemCombination(typeof(iRunePoison), typeof(iRuneFire), typeof(iGoblinHead), typeof(iRuneLightning), 1)]
	public class iGoblinTrophy : MagicWeapon {
		public override WeaponHandheld Handheld => WeaponHandheld.Float;
	}
	[EntityAttribute.ItemCombination(typeof(iRuneCube), typeof(iCrystalBall), 1)]
	public class iMagicOrb : MagicWeapon {
		public override WeaponHandheld Handheld => WeaponHandheld.Float;
	}
	[EntityAttribute.ItemCombination(typeof(iEyeBall), typeof(iRuneCube), 1)]
	public class iMagicEyeball : MagicWeapon {
		public override WeaponHandheld Handheld => WeaponHandheld.Float;
	}
	[EntityAttribute.ItemCombination(typeof(iPotionBlue), typeof(iPotionRed), typeof(iRuneCube), 1)]
	public class iMagicPotion : MagicWeapon {
		public override WeaponHandheld Handheld => WeaponHandheld.Float;
	}
	[EntityAttribute.ItemCombination(typeof(iWand), typeof(iStar), 1)]
	public class iWandStar : MagicWeapon {
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	}
	[EntityAttribute.ItemCombination(typeof(iWand), typeof(iCuteGhost), 1)]
	public class iWandFairy : MagicWeapon {
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	}
}
