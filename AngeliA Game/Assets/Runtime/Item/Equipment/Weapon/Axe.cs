using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class iAxeWood : AutoSpriteWeapon {
		public override WeaponType WeaponType => WeaponType.Axe;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iAxeIron : AutoSpriteWeapon {
		public override WeaponType WeaponType => WeaponType.Axe;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iAxeGold : AutoSpriteWeapon {
		public override WeaponType WeaponType => WeaponType.Axe;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iBattleAxe : AutoSpriteWeapon {
		public override WeaponType WeaponType => WeaponType.Axe;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iErgonomicAxe : AutoSpriteWeapon {
		public override WeaponType WeaponType => WeaponType.Axe;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iAxeJagged : AutoSpriteWeapon {
		public override WeaponType WeaponType => WeaponType.Axe;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iAxeOrc : AutoSpriteWeapon {
		public override WeaponType WeaponType => WeaponType.Axe;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iAxeCursed : AutoSpriteWeapon {
		public override WeaponType WeaponType => WeaponType.Axe;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
		protected override Cell DrawWeaponSprite (int x, int y, int grabRotation, int grabScale, AngeSprite sprite, int z) {
			var cell = base.DrawWeaponSprite(x, y, grabRotation, grabScale, sprite, z);
			AngeUtil.DrawGlitchEffect(cell, Game.GlobalFrame);
			return cell;
		}
	}
	public class iPickWood : AutoSpriteWeapon {
		public override WeaponType WeaponType => WeaponType.Axe;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iPickIron : AutoSpriteWeapon {
		public override WeaponType WeaponType => WeaponType.Axe;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iPickGold : AutoSpriteWeapon {
		public override WeaponType WeaponType => WeaponType.Axe;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iAxeGreat : AutoSpriteWeapon {
		public override WeaponType WeaponType => WeaponType.Axe;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.DoubleHanded;
	}
	public class iAxeButterfly : AutoSpriteWeapon {
		public override WeaponType WeaponType => WeaponType.Axe;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iAxeBone : AutoSpriteWeapon {
		public override WeaponType WeaponType => WeaponType.Axe;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
	public class iAxeStone : AutoSpriteWeapon {
		public override WeaponType WeaponType => WeaponType.Axe;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}
}
