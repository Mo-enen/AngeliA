using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using Moenen.Standard;
using UnityEngine;


namespace Yaya {
	public abstract partial class eCharacter {




		#region --- VAR ---


		// Api
		public bool IsAttacking => Game.GlobalFrame < LastAttackFrame + AttackDuration;
		public int BulletID => _BulletID != 0 ? _BulletID : (_BulletID = BulletName.Value.AngeHash());
		public int AttackCombo { get; private set; } = -1;
		public virtual bool IsChargingAttack => false;
		public bool IsAttackCharged => Game.GlobalFrame - ChargeStartFrame >= MinimalChargeAttackDuration;

		// Buff
		public readonly BuffString BulletName = new("DefaultBullet");
		public readonly BuffInt AttackDuration = new(12);
		public readonly BuffInt AttackColldown = new(2);
		public readonly BuffInt AttackComboGap = new(12);
		public readonly BuffInt HoldAttackPunish = new(4);
		public readonly BuffInt MinimalChargeAttackDuration = new(int.MaxValue);
		public readonly BuffBool StopMoveOnAttack = new(true);
		public readonly BuffBool CancelAttackOnJump = new(false);
		public readonly BuffBool UseRandomAttackCombo = new(false);
		public readonly BuffBool KeepAttackWhenHold = new(true);
		public readonly BuffBool AttackInAir = new(true);
		public readonly BuffBool AttackInWater = new(true);
		public readonly BuffBool AttackWhenMoving = new(true);
		public readonly BuffBool AttackWhenClimbing = new(false);
		public readonly BuffBool AttackWhenFlying = new(false);
		public readonly BuffBool AttackWhenRolling = new(false);
		public readonly BuffBool AttackWhenSquating = new(false);
		public readonly BuffBool AttackWhenDashing = new(false);
		public readonly BuffBool AttackWhenSliding = new(false);
		public readonly BuffBool AttackWhenGrabing = new(false);

		// Data
		private readonly static System.Random Random = new(19940516);
		private int LastAttackFrame = int.MinValue;
		private int ChargeStartFrame = int.MaxValue;
		private bool LastAttackCharged = false;
		private int _BulletID = 0;


		#endregion




		#region --- MSG ---


		public void OnActived_Attack () {
			LastAttackFrame = int.MinValue;
			ChargeStartFrame = int.MaxValue;
		}


		public void Update_Attack () {

			// Combo Break
			if (AttackCombo > -1 && Game.GlobalFrame > LastAttackFrame + AttackDuration + AttackColldown + AttackComboGap) {
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

			int frame = Game.GlobalFrame;
			LastAttackCharged = false;
			if (BulletID == 0 || IsSafe) return;
			LastAttackFrame = frame;

			// Spawn Bullet
			if (Game.Current.TrySpawnEntity(BulletID, X, Y, out var entity) && entity is eBullet bullet) {
				bool charged = LastAttackCharged = frame > ChargeStartFrame;
				bullet.Release(
					this, FacingRight ? Vector2Int.right : Vector2Int.left, AttackCombo,
					charged ? frame - ChargeStartFrame : 0
				);
				AttackCombo = UseRandomAttackCombo ? Random.Next() : AttackCombo + 1;
			}

		}


		public void CancelAttack () => LastAttackFrame = int.MinValue;


		public bool AttackCooldownReady (bool isHolding) => isHolding ?
			Game.GlobalFrame >= LastAttackFrame + AttackDuration + AttackColldown + HoldAttackPunish :
			Game.GlobalFrame >= LastAttackFrame + AttackDuration + AttackColldown;


		#endregion




	}
}