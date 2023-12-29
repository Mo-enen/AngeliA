using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {


	[ItemCombination(typeof(iIngotIron), typeof(iIngotIron), typeof(iTreeBranch), 1)]
	public class iAxeWood : Axe { }
	[ItemCombination(typeof(iAxeWood), typeof(iIngotIron), 1)]
	public class iAxeIron : Axe { }
	[ItemCombination(typeof(iAxeIron), typeof(iIngotGold), 1)]
	public class iAxeGold : Axe { }
	[ItemCombination(typeof(iAxeWood), typeof(iSwordWood), 1)]
	public class iBattleAxe : Axe { }
	[ItemCombination(typeof(iAxeWood), typeof(iFist), 1)]
	public class iErgonomicAxe : Axe { }
	[ItemCombination(typeof(iAxeIron), typeof(iComb), 1)]
	public class iAxeJagged : Axe { }
	[ItemCombination(typeof(iGoblinHead), typeof(iAxeIron), 1)]
	public class iAxeOrc : Axe { }
	[ItemCombination(typeof(iAxeOrc), typeof(iCursedSoul), typeof(iCursedSoul), 1)]
	public class iAxeCursed : Axe {
		protected override Cell DrawWeaponSprite (PoseCharacter character, int x, int y, int width, int height, int grabRotation, int grabScale, AngeSprite sprite, int z) {
			var cell = base.DrawWeaponSprite(character, x, y, width, height, grabRotation, grabScale, sprite, z);
			AngeUtil.DrawGlitchEffect(cell, Game.GlobalFrame);
			return cell;
		}
	}
	[ItemCombination(typeof(iIngotIron), typeof(iTreeBranch), 1)]
	public class iPickWood : Axe { }
	[ItemCombination(typeof(iPickWood), typeof(iIngotIron), 1)]
	public class iPickIron : Axe { }
	[ItemCombination(typeof(iPickIron), typeof(iIngotGold), 1)]
	public class iPickGold : Axe { }
	[ItemCombination(typeof(iAxeWood), typeof(iAxeWood), 1)]
	public class iAxeGreat : Axe {
		public override WeaponHandheld Handheld => WeaponHandheld.DoubleHanded;
		public override int AttackDuration => 16;
		public override int AttackCooldown => 3;
	}
	[ItemCombination(typeof(iAxeWood), typeof(iDrill), 1)]
	public class iAxeButterfly : Axe { }
	[ItemCombination(typeof(iMeatBone), typeof(iTreeBranch), 1)]
	public class iAxeBone : Axe { }
	[ItemCombination(typeof(iFlintPolished), typeof(iTreeBranch), 1)]
	public class iAxeStone : Axe { }
}
