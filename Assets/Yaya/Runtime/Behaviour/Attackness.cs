using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using Moenen.Standard;


namespace Yaya {
	public class Attackness : IInitialize, ISerializationCallbackReceiver {




		#region --- VAR ---


		// Api
		public Entity Source { get; private set; } = null;
		public bool IsAttacking => Game.GlobalFrame < LastAttackFrame + Duration;
		public int LastAttackFrame { get; private set; } = int.MinValue;
		public int BulletID => _BulletID != 0 ? _BulletID : (_BulletID = BulletName.Value.AngeHash());
		public int Combo { get; private set; } = -1;

		// Buff
		public BuffString BulletName { get; private set; } = new();
		public BuffInt Duration { get; private set; } = new();
		public BuffInt Colldown { get; private set; } = new();
		public BuffInt ComboGap { get; private set; } = new();
		public BuffBool StopMoveOnAttack { get; private set; } = new();
		public BuffBool CancelAttackOnJump { get; private set; } = new();
		public BuffBool RandomCombo { get; private set; } = new();
		public BuffBool KeepTriggerWhenHold { get; private set; } = new();
		public BuffInt HoldTriggerPunish { get; private set; } = new();
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
		[SerializeField] int _Duration = 12;
		[SerializeField] int _Colldown = 2;
		[SerializeField] int _ComboGap = 12;
		[SerializeField] bool _StopMoveOnAttack = true;
		[SerializeField] bool _CancelAttackOnJump = false;
		[SerializeField] bool _RandomCombo = false;
		[SerializeField] bool _KeepTriggerWhenHold = true;
		[SerializeField] int _HoldTriggerPunish = 4;
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


		public void OnInitialize (Entity source) {
			Source = source;
		}


		public void OnActived () {
			LastAttackFrame = int.MinValue;
			AntiAttackFrame = int.MinValue;
		}


		public void FrameUpdate () {
			// Combo Break
			if (Combo > -1 && Game.GlobalFrame > LastAttackFrame + Duration + Colldown + ComboGap) {
				Combo = -1;
			}
		}


		public void OnBeforeSerialize () => BuffValue.SerializeBuffValues(this);
		public void OnAfterDeserialize () => BuffValue.DeserializeBuffValues(this);


		#endregion




		#region --- API ---


		public bool Attack (Vector2Int direction) {
			int frame = Game.GlobalFrame;
			if (frame <= AntiAttackFrame) return false;
			// Attack
			if (BulletID == 0) return false;
			if (frame < LastAttackFrame + Duration + Colldown) return false;
			LastAttackFrame = frame;
			// Spawn Bullet
			if (Game.Current.TryAddEntity(BulletID, Source.X, Source.Y, out var entity) && entity is eBullet bullet) {
				bullet.Attackness = this;
				bullet.Combo = Combo;
				bullet.Direction = direction;
				bullet.Initialize();
			}
			Combo = RandomCombo ? Random.Next() : Combo + 1;
			return true;
		}


		public void CancelAttack () => LastAttackFrame = int.MinValue;


		public bool CheckReady (bool isHoldingAttack) => isHoldingAttack ?
			Game.GlobalFrame >= LastAttackFrame + Duration + Colldown + HoldTriggerPunish :
			Game.GlobalFrame >= LastAttackFrame + Duration + Colldown;


		public void NoAttackThisFrame () => AntiAttackFrame = Game.GlobalFrame;


		#endregion




	}
}
