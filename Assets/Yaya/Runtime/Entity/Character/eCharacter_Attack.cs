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
		public BuffString BulletName { get; protected set; } = new("DefaultBullet");
		public BuffInt AttackDuration { get; protected set; } = new(12);
		public BuffInt AttackColldown { get; protected set; } = new(2);
		public BuffInt AttackComboGap { get; protected set; } = new(12);
		public BuffInt HoldAttackPunish { get; protected set; } = new(4);
		public BuffInt MinimalChargeAttackDuration { get; protected set; } = new(int.MaxValue);
		public BuffBool StopMoveOnAttack { get; protected set; } = new(true);
		public BuffBool CancelAttackOnJump { get; protected set; } = new(false);
		public BuffBool UseRandomAttackCombo { get; protected set; } = new(false);
		public BuffBool KeepAttackWhenHold { get; protected set; } = new(true);
		public BuffBool AttackInAir { get; protected set; } = new(true);
		public BuffBool AttackInWater { get; protected set; } = new(true);
		public BuffBool AttackWhenMoving { get; protected set; } = new(true);
		public BuffBool AttackWhenClimbing { get; protected set; } = new(false);
		public BuffBool AttackWhenFlying { get; protected set; } = new(false);
		public BuffBool AttackWhenRolling { get; protected set; } = new(false);
		public BuffBool AttackWhenSquating { get; protected set; } = new(false);
		public BuffBool AttackWhenDashing { get; protected set; } = new(false);
		public BuffBool AttackWhenSliding { get; protected set; } = new(false);
		public BuffBool AttackWhenGrabing { get; protected set; } = new(false);

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