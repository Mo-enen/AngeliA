using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {



	public enum WeaponType { Hand, Sword, Axe, Hammer, Flail, Ranged, Polearm, Hook, Claw, Wand, Throwing, }

	public enum WeaponHandHeld { NoHandHeld, SingleHanded, DoubleHanded, OneOnEachHand, Polearm, Bow, Firearm, Throw, }



	public abstract class Weapon : Equipment {

		public sealed override EquipmentType EquipmentType => EquipmentType.Weapon;
		public abstract WeaponType WeaponType { get; }
		public abstract WeaponHandHeld HandHeld { get; }
		public virtual int AttackDuration => 12;
		public virtual int AttackCooldown => 2;
		public virtual bool RepeatAttackWhenHolding => false;

		public virtual bool AllowingAttack (Character character) => true;

	}



}
