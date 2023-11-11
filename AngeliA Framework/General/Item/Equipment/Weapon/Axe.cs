using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {


	[EntityAttribute.ItemCombination(typeof(iIngotIron), typeof(iIngotIron), typeof(iTreeBranch), 1)]
	public class iAxeWood : Axe { }
	[EntityAttribute.ItemCombination(typeof(iAxeWood), typeof(iIngotIron), 1)]
	public class iAxeIron : Axe { }
	[EntityAttribute.ItemCombination(typeof(iAxeIron), typeof(iIngotGold), 1)]
	public class iAxeGold : Axe { }
	[EntityAttribute.ItemCombination(typeof(iAxeWood), typeof(iSwordWood), 1)]
	public class iBattleAxe : Axe { }
	[EntityAttribute.ItemCombination(typeof(iAxeWood), typeof(iFist), 1)]
	public class iErgonomicAxe : Axe { }
	[EntityAttribute.ItemCombination(typeof(iAxeIron), typeof(iComb), 1)]
	public class iAxeJagged : Axe { }
	[EntityAttribute.ItemCombination(typeof(iGoblinHead), typeof(iAxeIron), 1)]
	public class iAxeOrc : Axe { }
	[EntityAttribute.ItemCombination(typeof(iAxeOrc), typeof(iOracleEye), 1)]
	public class iAxeCursed : Axe {
		protected override Cell DrawWeaponSprite (Character character, int x, int y, int width, int height, int grabRotation, int grabScale, AngeSprite sprite, int z) {
			var cell = base.DrawWeaponSprite(character, x, y, width, height, grabRotation, grabScale, sprite, z);
			AngeUtil.DrawGlitchEffect(cell, Game.GlobalFrame);
			return cell;
		}
	}
	[EntityAttribute.ItemCombination(typeof(iIngotIron), typeof(iTreeBranch), 1)]
	public class iPickWood : Axe { }
	[EntityAttribute.ItemCombination(typeof(iPickWood), typeof(iIngotIron), 1)]
	public class iPickIron : Axe { }
	[EntityAttribute.ItemCombination(typeof(iPickIron), typeof(iIngotGold), 1)]
	public class iPickGold : Axe { }
	[EntityAttribute.ItemCombination(typeof(iAxeWood), typeof(iAxeWood), 1)]
	public class iAxeGreat : Axe {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
		public override int AttackDuration => 16;
		public override int AttackCooldown => 3;
	}
	[EntityAttribute.ItemCombination(typeof(iAxeWood), typeof(iDrill), 1)]
	public class iAxeButterfly : Axe { }
	[EntityAttribute.ItemCombination(typeof(iMeatBone), typeof(iTreeBranch), 1)]
	public class iAxeBone : Axe { }
	[EntityAttribute.ItemCombination(typeof(iFlintPolished), typeof(iTreeBranch), 1)]
	public class iAxeStone : Axe { }
}
