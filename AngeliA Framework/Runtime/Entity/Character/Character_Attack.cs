using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;

using UnityEngine;


namespace AngeliaFramework {
	public abstract partial class Character {




		#region --- VAR ---


		// Api
		protected bool AttackStartAtCurrentFrame => Game.GlobalFrame == LastAttackFrame;
		protected bool IsAttacking => Game.GlobalFrame < LastAttackFrame + AttackDuration;
		protected bool IsAttackCharged => Game.GlobalFrame - ChargeStartFrame >= MinimalChargeAttackDuration;
		public virtual bool IsChargingAttack => false;
		public virtual int AttackTargetTeam => Const.TEAM_ALL;

		// Data
		private int AttackCombo = -1;
		private int LastAttackFrame = int.MinValue;
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


		// Spawn Bullet
		//if (Stage.TrySpawnEntity(BulletID.Value, X, Y, out var entity) && entity is Bullet bullet) {
		//	bullet.Release(
		//		this, AttackTargetTeam,
		//		FacingRight ? Vector2Int.right : Vector2Int.left,
		//		GetAttackBulletIndex(),
		//		Game.GlobalFrame > ChargeStartFrame ? Game.GlobalFrame - ChargeStartFrame : 0
		//	);
		//}
		public void Attack () {
			LastAttackFrame = Game.GlobalFrame;
			AttackCombo++;
		}


		public void CancelAttack () => LastAttackFrame = int.MinValue;


		public bool AttackCooldownReady (bool isHolding) => isHolding ?
			Game.GlobalFrame >= LastAttackFrame + AttackDuration + AttackCooldown + HoldAttackPunish :
			Game.GlobalFrame >= LastAttackFrame + AttackDuration + AttackCooldown;


		protected virtual int GetAttackBulletIndex () => AttackCombo;


		#endregion




	}
}