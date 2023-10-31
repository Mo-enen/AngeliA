using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class iBowWood : AutoSpriteBow {
		public override int ChargeAttackDuration => 20;
	}
	public class iBowIron : AutoSpriteBow { }
	public class iBowGold : AutoSpriteBow { }
	public class iCrossbowWood : AutoSpriteFirearm { public override int ChargeAttackDuration => 20; }
	public class iCrossbowIron : AutoSpriteFirearm { }
	public class iCrossbowGold : AutoSpriteFirearm { }
	public class iBlowgun : AutoSpriteFirearm { }
	public class iSlingshot : AutoSpriteBow { }
	public class iCompoundBow : AutoSpriteBow { }
	public class iRepeatingCrossbow : AutoSpriteFirearm {
		public override int AttackDuration => 12;
		public override int AttackCooldown => 0;
		public override bool RepeatAttackWhenHolding => true;
	}
	public class iBowNature : AutoSpriteBow { }
	public class iBowSkull : AutoSpriteBow { }
	public class iBowMage : AutoSpriteBow { }
	public class iBowSky : AutoSpriteBow {
		public override int AttackCooldown => 18;
	}
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
