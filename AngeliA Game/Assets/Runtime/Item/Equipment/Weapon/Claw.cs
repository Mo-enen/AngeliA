using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public abstract class ClawWeapon : AutoSpriteWeapon {
		public sealed override WeaponType WeaponType => WeaponType.Claw;
		public sealed override WeaponHandHeld HandHeld => WeaponHandHeld.OneOnEachHand;
		public override int AttackDuration => 10;
		public override int AttackCooldown => 0;
		public override int? MovementLoseRateOnAttack => 1000;
	}
	public class iClawWood : ClawWeapon { }
	public class iClawIron : ClawWeapon { }
	public class iClawGold : ClawWeapon { }
	public class iMandarinDuckAxe : ClawWeapon { }
	public class iClawCat : ClawWeapon { }
	public class iClawFox : ClawWeapon { }
	public class iKatars : ClawWeapon { }
	public class iKatarsTripple : ClawWeapon { }
	public class iEmeiPiercer : ClawWeapon { }
	public class iBaton : ClawWeapon { }
	public class iKnuckleDuster : ClawWeapon { }
	public class iEmeiFork : ClawWeapon { }
	public class iWuXingHook : ClawWeapon { }
	public class iKatarsRuby : ClawWeapon { }
	public class iKatarsJagged : ClawWeapon { }
}
