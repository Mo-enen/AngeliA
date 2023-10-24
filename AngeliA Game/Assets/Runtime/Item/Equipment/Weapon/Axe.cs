using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {

	public abstract class iAxeWeapon : AutoSpriteWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
		public sealed override WeaponType WeaponType => WeaponType.Axe;
		public override int BulletID => BULLET_ID;
		private static readonly int BULLET_ID = typeof(AxeBullet).AngeHash();
	}

	public class iAxeWood : iAxeWeapon { }
	public class iAxeIron : iAxeWeapon { }
	public class iAxeGold : iAxeWeapon { }
	public class iBattleAxe : iAxeWeapon { }
	public class iErgonomicAxe : iAxeWeapon { }
	public class iAxeJagged : iAxeWeapon { }
	public class iAxeOrc : iAxeWeapon { }
	public class iAxeCursed : iAxeWeapon {
		protected override Cell DrawWeaponSprite (Character character, int x, int y, int width, int height, int grabRotation, int grabScale, AngeSprite sprite, int z) {
			var cell = base.DrawWeaponSprite(character, x, y, width, height, grabRotation, grabScale, sprite, z);
			AngeUtil.DrawGlitchEffect(cell, Game.GlobalFrame);
			return cell;
		}
	}
	public class iPickWood : iAxeWeapon { }
	public class iPickIron : iAxeWeapon { }
	public class iPickGold : iAxeWeapon { }
	public class iAxeGreat : iAxeWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
	}
	public class iAxeButterfly : iAxeWeapon { }
	public class iAxeBone : iAxeWeapon { }
	public class iAxeStone : iAxeWeapon { }
}
