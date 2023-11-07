using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {


	[EntityAttribute.ItemCombination(typeof(iIngotIron), typeof(iIngotIron), typeof(iTreeBranch), 1)]
	public class iAxeWood : AutoSpriteAxe { }
	[EntityAttribute.ItemCombination(typeof(iAxeWood), typeof(iIngotIron), 1)]
	public class iAxeIron : AutoSpriteAxe { }
	[EntityAttribute.ItemCombination(typeof(iAxeIron), typeof(iIngotGold), 1)]
	public class iAxeGold : AutoSpriteAxe { }
	[EntityAttribute.ItemCombination(typeof(iAxeWood), typeof(iSwordWood), 1)]
	public class iBattleAxe : AutoSpriteAxe { }
	[EntityAttribute.ItemCombination(typeof(iAxeWood), typeof(iFist), 1)]
	public class iErgonomicAxe : AutoSpriteAxe { }
	[EntityAttribute.ItemCombination(typeof(iAxeIron), typeof(iComb), 1)]
	public class iAxeJagged : AutoSpriteAxe { }
	[EntityAttribute.ItemCombination(typeof(iGoblinHead), typeof(iAxeIron), 1)]
	public class iAxeOrc : AutoSpriteAxe { }
	[EntityAttribute.ItemCombination(typeof(iAxeOrc), typeof(iOracleEye), 1)]
	public class iAxeCursed : AutoSpriteAxe {
		protected override Cell DrawWeaponSprite (Character character, int x, int y, int width, int height, int grabRotation, int grabScale, AngeSprite sprite, int z) {
			var cell = base.DrawWeaponSprite(character, x, y, width, height, grabRotation, grabScale, sprite, z);
			AngeUtil.DrawGlitchEffect(cell, Game.GlobalFrame);
			return cell;
		}
	}
	[EntityAttribute.ItemCombination(typeof(iIngotIron), typeof(iTreeBranch), 1)]
	public class iPickWood : AutoSpriteAxe { }
	[EntityAttribute.ItemCombination(typeof(iPickWood), typeof(iIngotIron), 1)]
	public class iPickIron : AutoSpriteAxe { }
	[EntityAttribute.ItemCombination(typeof(iPickIron), typeof(iIngotGold), 1)]
	public class iPickGold : AutoSpriteAxe { }
	[EntityAttribute.ItemCombination(typeof(iAxeWood), typeof(iAxeWood), 1)]
	public class iAxeGreat : AutoSpriteAxe {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
		public override int AttackDuration => 16;
		public override int AttackCooldown => 3;
	}
	[EntityAttribute.ItemCombination(typeof(iAxeWood), typeof(iDrill), 1)]
	public class iAxeButterfly : AutoSpriteAxe { }
	[EntityAttribute.ItemCombination(typeof(iMeatBone), typeof(iTreeBranch), 1)]
	public class iAxeBone : AutoSpriteAxe { }
	[EntityAttribute.ItemCombination(typeof(iFlintPolished), typeof(iTreeBranch), 1)]
	public class iAxeStone : AutoSpriteAxe { }
}
