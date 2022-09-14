using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using UnityEngine.Networking.Types;


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
		public BuffString BulletName { get; private set; } = new("eDefaultBullet");
		public BuffInt Duration { get; private set; } = new(12);
		public BuffInt Colldown { get; private set; } = new(2);
		public BuffInt ComboGap { get; private set; } = new(12);
		public BuffBool StopMoveOnAttack { get; private set; } = new(true);
		public BuffBool CancelAttackOnJump { get; private set; } = new(false);
		public BuffBool RandomCombo { get; private set; } = new(false);
		public BuffBool KeepTriggerWhenHold { get; private set; } = new(true);
		public BuffInt HoldTriggerPunish { get; private set; } = new(4);
		public BuffBool AttackInAir { get; private set; } = new(true);
		public BuffBool AttackInWater { get; private set; } = new(true);
		public BuffBool AttackWhenFlying { get; private set; } = new(false);
		public BuffBool AttackWhenClimbing { get; private set; } = new(false);
		public BuffBool AttackWhenRolling { get; private set; } = new(false);
		public BuffBool AttackWhenSquating { get; private set; } = new(false);
		public BuffBool AttackWhenDashing { get; private set; } = new(false);

		// Ser
#pragma warning disable
		[SerializeField] string _BulletName = "eDefaultBullet";
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
		private static Game Game = null;
		private readonly static System.Random Random = new(19940516);
		private int _BulletID = 0;


		#endregion




		#region --- MSG ---


		public static void InitializeWithGame (Game game) => Game = game;


		public void OnActived (Entity source) {
			Source = source;
			LastAttackFrame = int.MinValue;
		}


		public void Update () {
			// Combo Break
			if (Combo > -1 && Game.GlobalFrame > LastAttackFrame + Duration + Colldown + ComboGap) {
				Combo = -1;
			}
		}


		public void OnBeforeSerialize () => BuffValue.SerializeBuffValues(this);
		public void OnAfterDeserialize () => BuffValue.DeserializeBuffValues(this);


		#endregion




		#region --- API ---


		public bool Attack () {
			int frame = Game.GlobalFrame;
			// Attack
			if (BulletID == 0) return false;
			if (frame < LastAttackFrame + Duration + Colldown) return false;
			LastAttackFrame = frame;
			// Spawn Bullet
			if (Game.TryAddEntity<eBullet>(BulletID, Source.X, Source.Y, out var bullet)) {
				bullet.Attackness = this;
				bullet.Combo = Combo;
			}
			Combo = RandomCombo ? Random.Next() : Combo + 1;
			return true;
		}


		public void CancelAttack () => LastAttackFrame = int.MinValue;


		public bool CheckReady (bool isHoldingAttack) => isHoldingAttack ?
			Game.GlobalFrame >= LastAttackFrame + Duration + Colldown + HoldTriggerPunish :
			Game.GlobalFrame >= LastAttackFrame + Duration + Colldown;


		#endregion




		#region --- LGC ---




		#endregion



	}
}
