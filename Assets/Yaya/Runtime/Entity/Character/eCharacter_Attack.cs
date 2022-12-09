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
		public bool AntiAttack => Game.GlobalFrame <= AntiAttackFrame;
		public int BulletID => _BulletID != 0 ? _BulletID : (_BulletID = BulletName.Value.AngeHash());
		public int AttackCombo { get; private set; } = -1;
		public virtual bool IsChargingAttack => false;

		// Buff
		public BuffString BulletName { get; private set; } = new();
		public BuffInt AttackDuration { get; private set; } = new();
		public BuffInt AttackColldown { get; private set; } = new();
		public BuffInt AttackComboGap { get; private set; } = new();
		public BuffInt HoldAttackPunish { get; private set; } = new();
		public BuffInt MinimalChargeAttackDuration { get; private set; } = new();
		public BuffBool StopMoveOnAttack { get; private set; } = new();
		public BuffBool CancelAttackOnJump { get; private set; } = new();
		public BuffBool UseRandomAttackCombo { get; private set; } = new();
		public BuffBool KeepAttackWhenHold { get; private set; } = new();
		public BuffBool AttackInAir { get; private set; } = new();
		public BuffBool AttackInWater { get; private set; } = new();
		public BuffBool AttackWhenFlying { get; private set; } = new();
		public BuffBool AttackWhenClimbing { get; private set; } = new();
		public BuffBool AttackWhenRolling { get; private set; } = new();
		public BuffBool AttackWhenSquating { get; private set; } = new();
		public BuffBool AttackWhenDashing { get; private set; } = new();

		// Ser
#pragma warning disable
		[SerializeField] string _BulletName = "DefaultBullet";
		[SerializeField] int _AttackDuration = 12;
		[SerializeField] int _AttackColldown = 2;
		[SerializeField] int _AttackComboGap = 12;
		[SerializeField] int _HoldAttackPunish = 4;
		[SerializeField] int _MinimalChargeAttackDuration = int.MaxValue;
		[SerializeField] bool _StopMoveOnAttack = true;
		[SerializeField] bool _CancelAttackOnJump = false;
		[SerializeField] bool _UseRandomAttackCombo = false;
		[SerializeField] bool _KeepAttackWhenHold = true;
		[SerializeField] bool _AttackInAir = true;
		[SerializeField] bool _AttackInWater = true;
		[SerializeField] bool _AttackWhenClimbing = false;
		[SerializeField] bool _AttackWhenFlying = false;
		[SerializeField] bool _AttackWhenRolling = false;
		[SerializeField] bool _AttackWhenSquating = false;
		[SerializeField] bool _AttackWhenDashing = false;
#pragma warning restore

		// Data
		private readonly static System.Random Random = new(19940516);
		private int AntiAttackFrame = int.MinValue;
		private int LastAttackFrame = int.MinValue;
		private int ChargeStartFrame = int.MaxValue;
		private int _BulletID = 0;


		#endregion




		#region --- MSG ---


		public void OnActived_Attack () {
			LastAttackFrame = int.MinValue;
			AntiAttackFrame = int.MinValue;
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
				if (Game.GlobalFrame - ChargeStartFrame >= MinimalChargeAttackDuration) {
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
			if (BulletID == 0 || frame <= AntiAttackFrame) return;
			LastAttackFrame = frame;

			// Spawn Bullet
			if (Game.Current.TryAddEntity(BulletID, X, Y, out var entity) && entity is eBullet bullet) {
				bullet.Release(
					this, FacingRight ? Vector2Int.right : Vector2Int.left, AttackCombo,
					frame > ChargeStartFrame ? frame - ChargeStartFrame : 0
				);
				AttackCombo = UseRandomAttackCombo ? Random.Next() : AttackCombo + 1;
			}

		}


		public void CancelAttack () => LastAttackFrame = int.MinValue;


		public bool AttackCooldownReady (bool isHolding) => isHolding ?
			Game.GlobalFrame >= LastAttackFrame + AttackDuration + AttackColldown + HoldAttackPunish :
			Game.GlobalFrame >= LastAttackFrame + AttackDuration + AttackColldown;


		public void IgnoreAttack (int duration = 0) => AntiAttackFrame = Game.GlobalFrame + duration;


		#endregion




	}
}