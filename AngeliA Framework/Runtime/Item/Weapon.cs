using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {



	public enum WeaponType { Hand, Sword, Axe, Hammer, Flail, Ranged, Polearm, Hook, Claw, Magic, Throwing, }

	public enum WeaponHandHeld { SingleHanded, DoubleHanded, OneOnEachHand, Pole, Bow, Firearm, Float, }


	public interface IMeleeWeapon {
		public int RangeXLeft { get; }
		public int RangeXRight { get; }
		public int RangeY { get; }
	}


	public abstract class Weapon : Equipment {

		public sealed override EquipmentType EquipmentType => EquipmentType.Weapon;
		public abstract WeaponType WeaponType { get; }
		public abstract WeaponHandHeld HandHeld { get; }
		public virtual int AttackDuration => 12;
		public virtual int AttackCooldown => 2;
		public virtual int BulletID => DefaultBullet.TYPE_ID;
		public virtual int ChargeAttackDuration => int.MaxValue;
		public virtual bool RepeatAttackWhenHolding => false;
		public virtual bool LockFacingOnAttack => false;

		public virtual bool AllowingAttack (Character character) => true;

	}



}
