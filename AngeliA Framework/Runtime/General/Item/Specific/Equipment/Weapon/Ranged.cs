using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {


	[ItemCombination(typeof(iRope), typeof(iTreeBranch), 1)]
	public class iBowWood : Bow<iBowWood.BowWoodBullet, iArrowWood> {
		public class BowWoodBullet : ArrowBullet {
			protected override int SpeedX => 42;
		}
	}


	[ItemCombination(typeof(iBowWood), typeof(iIngotIron), 1)]
	public class iBowIron : Bow<iBowIron.BowIronBullet, iArrowIron> {
		public class BowIronBullet : ArrowBullet {
			protected override bool DestroyOnHitReceiver => false;
			protected override int SpeedX => 52;
		}
	}


	[ItemCombination(typeof(iBowIron), typeof(iIngotGold), 1)]
	public class iBowGold : Bow<iBowGold.BowGoldBullet, iArrowGold> {
		public class BowGoldBullet : ArrowBullet {
			protected override bool DestroyOnHitReceiver => false;
			protected override int SpeedX => 62;
		}
	}


	[ItemCombination(typeof(iRope), typeof(iTreeBranch), typeof(iTreeBranch), 1)]
	public class iCrossbowWood : Shooting<iCrossbowWood.CrossbowWoodBullet, iBoltsWood> {
		public class CrossbowWoodBullet : ArrowBullet { }
	}


	[ItemCombination(typeof(iCrossbowWood), typeof(iIngotIron), 1)]
	public class iCrossbowIron : Shooting<iCrossbowIron.CrossbowIronBullet, iBoltsIron> {
		public class CrossbowIronBullet : ArrowBullet {
			protected override bool DestroyOnHitReceiver => false;
		}
	}


	[ItemCombination(typeof(iCrossbowIron), typeof(iIngotGold), 1)]
	public class iCrossbowGold : Shooting<iCrossbowGold.CrossbowGoldBullet, iBoltsGold> {
		public class CrossbowGoldBullet : ArrowBullet {
			protected override bool DestroyOnHitReceiver => false;
		}
	}


	[ItemCombination(typeof(iRunePoison), typeof(iNeedle), typeof(iTreeBranch), 1)]
	public class iBlowgun : Shooting { }


	[ItemCombination(typeof(iRubberBall), typeof(iRibbon), typeof(iIngotIron), 1)]
	public class iSlingshot : Bow { }


	[ItemCombination(typeof(iBowIron), typeof(iBowIron), 1)]
	public class iCompoundBow : Bow { }


	[ItemCombination(typeof(iBowWood), typeof(iBowWood), 1)]
	public class iRepeatingCrossbow : Shooting {
		public override int AttackDuration => 12;
		public override int AttackCooldown => 0;
		public override bool RepeatAttackWhenHolding => true;
	}


	[ItemCombination(typeof(iBowWood), typeof(iLeaf), 1)]
	public class iBowNature : Bow { }


	[ItemCombination(typeof(iBowWood), typeof(iSkull), typeof(iSkull), typeof(iRibbon), 1)]
	public class iBowSkull : Bow { }


	[ItemCombination(typeof(iBowIron), typeof(iRuneCube), 1)]
	public class iBowMage : Bow { }


	[ItemCombination(typeof(iBowGold), typeof(iLeafLegend), 1)]
	public class iBowSky : Bow {
		public override int AttackCooldown => 18;
	}


	[ItemCombination(typeof(iBowGold), typeof(iRope), typeof(iRope), typeof(iRope), 1)]
	public class iBowHarp : Bow {
		protected override Cell DrawWeaponSprite (PoseCharacter character, int x, int y, int width, int height, int grabRotation, int grabScale, AngeSprite sprite, int z) {
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
