using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGeneral {
	public class iAxeWood : AutoSpriteAxe { }
	public class iAxeIron : AutoSpriteAxe { }
	public class iAxeGold : AutoSpriteAxe { }
	public class iBattleAxe : AutoSpriteAxe { }
	public class iErgonomicAxe : AutoSpriteAxe { }
	public class iAxeJagged : AutoSpriteAxe { }
	public class iAxeOrc : AutoSpriteAxe { }
	public class iAxeCursed : AutoSpriteAxe {
		protected override Cell DrawWeaponSprite (Character character, int x, int y, int width, int height, int grabRotation, int grabScale, AngeSprite sprite, int z) {
			var cell = base.DrawWeaponSprite(character, x, y, width, height, grabRotation, grabScale, sprite, z);
			AngeUtil.DrawGlitchEffect(cell, Game.GlobalFrame);
			return cell;
		}
	}
	public class iPickWood : AutoSpriteAxe { }
	public class iPickIron : AutoSpriteAxe { }
	public class iPickGold : AutoSpriteAxe { }
	public class iAxeGreat : AutoSpriteAxe {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
		public override int AttackDuration => 16;
		public override int AttackCooldown => 3;
	}
	public class iAxeButterfly : AutoSpriteAxe { }
	public class iAxeBone : AutoSpriteAxe { }
	public class iAxeStone : AutoSpriteAxe { }
}
