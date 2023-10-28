using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {

	public abstract class AxeWeapon : AutoSpriteWeapon, IMeleeWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
		public sealed override WeaponType WeaponType => WeaponType.Axe;
		public override int BulletID => BULLET_ID;
		private static readonly int BULLET_ID = typeof(AxeBullet).AngeHash();
		int IMeleeWeapon.RangeXLeft => 275;
		int IMeleeWeapon.RangeXRight => 384;
		int IMeleeWeapon.RangeY => 512;
		public override int AttackDuration => 12;
		public override int AttackCooldown => 2;
	}

	public class iAxeWood : AxeWeapon { }
	public class iAxeIron : AxeWeapon { }
	public class iAxeGold : AxeWeapon { }
	public class iBattleAxe : AxeWeapon { }
	public class iErgonomicAxe : AxeWeapon { }
	public class iAxeJagged : AxeWeapon { }
	public class iAxeOrc : AxeWeapon { }
	public class iAxeCursed : AxeWeapon {
		protected override Cell DrawWeaponSprite (Character character, int x, int y, int width, int height, int grabRotation, int grabScale, AngeSprite sprite, int z) {
			var cell = base.DrawWeaponSprite(character, x, y, width, height, grabRotation, grabScale, sprite, z);
			AngeUtil.DrawGlitchEffect(cell, Game.GlobalFrame);
			return cell;
		}
	}
	public class iPickWood : AxeWeapon { }
	public class iPickIron : AxeWeapon { }
	public class iPickGold : AxeWeapon { }
	public class iAxeGreat : AxeWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
		public override int AttackDuration => 16;
		public override int AttackCooldown => 3;
	}
	public class iAxeButterfly : AxeWeapon { }
	public class iAxeBone : AxeWeapon { }
	public class iAxeStone : AxeWeapon { }
}
