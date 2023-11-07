using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {


	[EntityAttribute.ItemCombination(typeof(iRope), typeof(iTreeBranch), 1)]
	public class iBowWood : AutoSpriteBow { }
	[EntityAttribute.ItemCombination(typeof(iBowWood), typeof(iIngotIron), 1)]
	public class iBowIron : AutoSpriteBow { }
	[EntityAttribute.ItemCombination(typeof(iBowIron), typeof(iIngotGold), 1)]
	public class iBowGold : AutoSpriteBow { }
	[EntityAttribute.ItemCombination(typeof(iRope), typeof(iTreeBranch), typeof(iTreeBranch), 1)]
	public class iCrossbowWood : AutoSpriteFirearm { }
	[EntityAttribute.ItemCombination(typeof(iCrossbowWood), typeof(iIngotIron), 1)]
	public class iCrossbowIron : AutoSpriteFirearm { }
	[EntityAttribute.ItemCombination(typeof(iCrossbowIron), typeof(iIngotGold), 1)]
	public class iCrossbowGold : AutoSpriteFirearm { }
	[EntityAttribute.ItemCombination(typeof(iRunePoison), typeof(iNeedle), typeof(iTreeBranch), 1)]
	public class iBlowgun : AutoSpriteFirearm { }
	[EntityAttribute.ItemCombination(typeof(iRubberBall), typeof(iRibbon), typeof(iIngotIron), 1)]
	public class iSlingshot : AutoSpriteBow { }
	[EntityAttribute.ItemCombination(typeof(iBowIron), typeof(iBowIron), 1)]
	public class iCompoundBow : AutoSpriteBow { }
	[EntityAttribute.ItemCombination(typeof(iBowWood), typeof(iBowWood), 1)]
	public class iRepeatingCrossbow : AutoSpriteFirearm {
		public override int AttackDuration => 12;
		public override int AttackCooldown => 0;
		public override bool RepeatAttackWhenHolding => true;
	}
	[EntityAttribute.ItemCombination(typeof(iBowWood), typeof(iLeaf), 1)]
	public class iBowNature : AutoSpriteBow { }
	[EntityAttribute.ItemCombination(typeof(iBowWood), typeof(iSkull), typeof(iSkull), typeof(iRibbon), 1)]
	public class iBowSkull : AutoSpriteBow { }
	[EntityAttribute.ItemCombination(typeof(iBowIron), typeof(iRuneCube), 1)]
	public class iBowMage : AutoSpriteBow { }
	[EntityAttribute.ItemCombination(typeof(iBowGold), typeof(iLeafLegend), 1)]
	public class iBowSky : AutoSpriteBow {
		public override int AttackCooldown => 18;
	}
	[EntityAttribute.ItemCombination(typeof(iBowGold), typeof(iRope), typeof(iRope), typeof(iRope), 1)]
	public class iBowHarp : AutoSpriteBow {
		protected override Cell DrawWeaponSprite (Character character, int x, int y, int width, int height, int grabRotation, int grabScale, AngeSprite sprite, int z) {
			var cell = base.DrawWeaponSprite(character, x, y, width, height, grabRotation, grabScale, sprite, z);
			const int GAP_X0 = -32;
			const int GAP_X1 = -64;
			const int GAP_X2 = -96;
			int centerDeltaX0 = GAP_X0;
			int centerDeltaX1 = GAP_X1;
			int centerDeltaX2 = GAP_X2;
			if (character.IsAttacking) {
				int localFrame = Game.GlobalFrame - character.LastAttackFrame;
				if (localFrame < character.AttackDuration / 2) {
					centerDeltaX0 = 0;
					centerDeltaX1 = 0;
					centerDeltaX2 = 0;
				} else {
					localFrame -= character.AttackDuration / 2;
					if (localFrame < character.AttackDuration / 8) centerDeltaX0 = 0;
					if (localFrame < character.AttackDuration / 6) centerDeltaX1 = 0;
					if (localFrame < character.AttackDuration / 4) centerDeltaX2 = 0;
				}
			}
			DrawString(character, cell, new(GAP_X0, 00), new(GAP_X0, 000), new(centerDeltaX0, 0));
			DrawString(character, cell, new(GAP_X1, 16), new(GAP_X1, -16), new(centerDeltaX1, 0));
			DrawString(character, cell, new(GAP_X2, 20), new(GAP_X2, -20), new(centerDeltaX2, 0));
			return cell;
		}
	}
}
