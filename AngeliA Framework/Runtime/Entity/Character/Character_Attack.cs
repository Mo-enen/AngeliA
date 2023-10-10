using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;

using UnityEngine;


namespace AngeliaFramework {
	public abstract partial class Character {




		#region --- VAR ---


		// Api
		protected bool IsAttacking => Game.GlobalFrame < LastAttackFrame + AttackDuration;
		protected bool IsAttackCharged => Game.GlobalFrame - ChargeStartFrame >= MinimalChargeAttackDuration;
		public int LastAttackFrame { get; private set; } = int.MinValue;
		public int AttackCombo { get; private set; } = -1;
		public virtual bool IsChargingAttack => false;
		public virtual int AttackTargetTeam => Const.TEAM_ALL;

		// Data
		private int ChargeStartFrame = int.MaxValue;


		#endregion




		#region --- MSG ---


		public void OnActivated_Attack () {
			LastAttackFrame = int.MinValue;
			ChargeStartFrame = int.MaxValue;
		}


		public void PhysicsUpdate_Attack () {

			// Combo Break
			if (AttackCombo > -1 && Game.GlobalFrame > LastAttackFrame + AttackDuration + AttackCooldown + AttackComboGap) {
				AttackCombo = -1;
			}

			// Charge
			if (IsChargingAttack) {
				if (ChargeStartFrame == int.MaxValue) ChargeStartFrame = Game.GlobalFrame;
			} else if (ChargeStartFrame != int.MaxValue) {
				// Charge Release
				if (IsAttackCharged) {
					if (IsAttackAllowedByMovement()) {
						Attack();
					}
				}
				ChargeStartFrame = int.MaxValue;
			}

		}


		#endregion




		#region --- API ---


		public void Attack () {
			LastAttackFrame = Game.GlobalFrame;
			AttackCombo++;
		}


		public void CancelAttack () => LastAttackFrame = int.MinValue;


		protected bool AttackCooldownReady (bool isHolding) => isHolding ?
			Game.GlobalFrame >= LastAttackFrame + AttackDuration + AttackCooldown + HoldAttackPunish :
			Game.GlobalFrame >= LastAttackFrame + AttackDuration + AttackCooldown;


		protected bool IsAttackAllowedByMovement () =>
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


		protected virtual int GetAttackBulletIndex () => AttackCombo;


		#endregion




	}
}