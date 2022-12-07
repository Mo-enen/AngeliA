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
		public int LastAttackFrame { get; private set; } = int.MinValue;
		public int BulletID => _BulletID != 0 ? _BulletID : (_BulletID = BulletName.Value.AngeHash());
		public int AttackCombo { get; private set; } = -1;
		public bool AntiAttack => Game.GlobalFrame <= AntiAttackFrame;

		// Buff
		public BuffString BulletName { get; private set; } = new();
		public BuffInt AttackDuration { get; private set; } = new();
		public BuffInt AttackColldown { get; private set; } = new();
		public BuffInt AttackComboGap { get; private set; } = new();
		public BuffBool StopMoveOnAttack { get; private set; } = new();
		public BuffBool CancelAttackOnJump { get; private set; } = new();
		public BuffBool UseRandomAttackCombo { get; private set; } = new();
		public BuffBool KeepAttackWhenHold { get; private set; } = new();
		public BuffInt HoldAttackPunish { get; private set; } = new();
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
		[SerializeField] bool _StopMoveOnAttack = true;
		[SerializeField] bool _CancelAttackOnJump = false;
		[SerializeField] bool _UseRandomAttackCombo = false;
		[SerializeField] bool _KeepAttackWhenHold = true;
		[SerializeField] int _HoldAttackPunish = 4;
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
		private int _BulletID = 0;


		#endregion




		#region --- MSG ---


		public void OnActived_Attack () {
			LastAttackFrame = int.MinValue;
			AntiAttackFrame = int.MinValue;
		}


		public void Update_Attack () {
			// Combo Break
			if (AttackCombo > -1 && Game.GlobalFrame > LastAttackFrame + AttackDuration + AttackColldown + AttackComboGap) {
				AttackCombo = -1;
			}
		}


		#endregion




		#region --- API ---


		public bool Attack (Vector2Int direction) {
			int frame = Game.GlobalFrame;
			if (frame <= AntiAttackFrame) return false;
			// Attack
			if (BulletID == 0) return false;
			if (frame < LastAttackFrame + AttackDuration + AttackColldown) return false;
			LastAttackFrame = frame;
			// Spawn Bullet
			if (Game.Current.TryAddEntity(BulletID, X, Y, out var entity) && entity is eBullet bullet) {
				bullet.Release(this, direction, AttackCombo);
			}
			AttackCombo = UseRandomAttackCombo ? Random.Next() : AttackCombo + 1;
			return true;
		}


		public void CancelAttack () => LastAttackFrame = int.MinValue;


		public bool CheckAttackReady (bool isHoldingAttack) => isHoldingAttack ?
			Game.GlobalFrame >= LastAttackFrame + AttackDuration + AttackColldown + HoldAttackPunish :
			Game.GlobalFrame >= LastAttackFrame + AttackDuration + AttackColldown;


		public void IgnoreAttack (int duration = 0) => AntiAttackFrame = Game.GlobalFrame + duration;


		#endregion




	}
}