using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract partial class Character {




		#region --- VAR ---


		// Api
		public bool IsAttacking => Game.GlobalFrame < LastAttackFrame + AttackDuration;
		public int AttackChargedDuration =>
			Game.GlobalFrame > ChargeStartFrame && Game.GlobalFrame - ChargeStartFrame >= MinimalChargeAttackDuration ?
			Game.GlobalFrame - ChargeStartFrame : 0;
		public int LastAttackFrame { get; private set; } = int.MinValue;
		public virtual int AttackStyleIndex { get; private set; } = -1;
		public virtual int AttackTargetTeam => Const.TEAM_ALL;
		public virtual bool IsChargingAttack => false;
		public virtual bool RandomAttackAnimationStyle => true;

		// Data
		private int AttackStyleLoop = 1;
		private int ChargeStartFrame = int.MaxValue;
		private bool AttackStartFacingRight = true;


		#endregion




		#region --- MSG ---


		private void OnActivated_Attack () {
			LastAttackFrame = int.MinValue;
			ChargeStartFrame = int.MaxValue;
		}


		private void PhysicsUpdate_Attack () {

			// Combo Break
			if (!RandomAttackAnimationStyle && AttackStyleIndex > -1 && Game.GlobalFrame > LastAttackFrame + AttackDuration + AttackCooldown + AttackComboGap) {
				AttackStyleIndex = -1;
			}

			// Charge
			if (IsChargingAttack) {
				if (ChargeStartFrame == int.MaxValue) ChargeStartFrame = Game.GlobalFrame;
			} else if (ChargeStartFrame != int.MaxValue) {
				// Charge Release
				if (AttackChargedDuration > 0) Attack(charged: true);
				ChargeStartFrame = int.MaxValue;
			}

		}


		#endregion




		#region --- API ---


		public void Attack (bool charged = false) {
			if (!IsAttackAllowedByMovement() || !IsAttackAllowedByEquipment()) return;
			LastAttackFrame = Game.GlobalFrame;
			AttackStyleIndex += RandomAttackAnimationStyle ? AngeUtil.RandomInt(1, Mathf.Max(2, AttackStyleLoop)) : 1;
			AttackStartFacingRight = _FacingRight;
		}


		protected virtual bool IsAttackAllowedByMovement () =>
			(AttackInAir || (IsGrounded || InWater || InSand || IsClimbing)) &&
			(AttackInWater || !InWater) &&
			(AttackWhenMoving || IntendedX == 0) &&
			(AttackWhenClimbing || !IsClimbing) &&
			(AttackWhenFlying || !IsFlying) &&
			(AttackWhenRolling || !IsRolling) &&
			(AttackWhenSquatting || !IsSquatting) &&
			(AttackWhenDashing || !IsDashing) &&
			(AttackWhenSliding || !IsSliding) &&
			(AttackWhenGrabbing || (!IsGrabbingTop && !IsGrabbingSide)) &&
			(AttackWhenRush || (!IsRushing));


		protected virtual bool IsAttackAllowedByEquipment () => GetEquippingItem(EquipmentType.Weapon) is not Weapon weapon || weapon.AllowingAttack(this);


		protected virtual void SpawnPunchBullet () => Bullet.SpawnBullet(DefaultPunchBullet.TYPE_ID, this, null);


		#endregion




	}
}