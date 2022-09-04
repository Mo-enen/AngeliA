using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using UnityEngine.Networking.Types;


namespace Yaya {
	public class Attackness : EntityBehaviour<Entity>, IInitialize, ISerializationCallbackReceiver {




		#region --- VAR ---


		// Api
		public bool IsAttacking => Game.GlobalFrame < LastAttackFrame + Duration;
		public int LastAttackFrame { get; private set; } = int.MinValue;
		public int BulletID => _BulletID != 0 ? _BulletID : (_BulletID = BulletName.Value.AngeHash());
		public int Combo { get; private set; } = -1;

		// Buff
		public BuffString BulletName { get; private set; } = new("eDefaultBullet");
		public BuffInt Duration { get; private set; } = new(12);
		public BuffInt Colldown { get; private set; } = new(2);
		public BuffInt ComboGap { get; private set; } = new(12);
		public BuffBool AttackInAir { get; private set; } = new(true);
		public BuffBool AttackInWater { get; private set; } = new(true);
		public BuffBool StopMoveOnAttack { get; private set; } = new(true);
		public BuffBool CancelAttackOnJump { get; private set; } = new(false);
		public BuffBool RandomCombo { get; private set; } = new(false);
		public BuffBool KeepTriggerWhenHold { get; private set; } = new(true);
		public BuffInt HoldTriggerPunish { get; private set; } = new(4);

		// Ser
#pragma warning disable
		[SerializeField] string _BulletName = "eDefaultBullet";
		[SerializeField] int _Duration = 12;
		[SerializeField] int _Colldown = 2;
		[SerializeField] int _ComboGap = 12;
		[SerializeField] bool _AttackInAir = true;
		[SerializeField] bool _AttackInWater = true;
		[SerializeField] bool _StopMoveOnAttack = true;
		[SerializeField] bool _CancelAttackOnJump = false;
		[SerializeField] bool _RandomCombo = false;
		[SerializeField] bool _KeepTriggerWhenHold = true;
		[SerializeField] int _HoldTriggerPunish = 4;
#pragma warning restore

		// Data
		private static Game Game = null;
		private readonly static System.Random Random = new(19940516);
		private int _BulletID = 0;


		#endregion




		#region --- MSG ---


		public static void InitializeWithGame (Game game) => Game = game;


		public override void OnActived (Entity source) {
			base.OnActived(source);
			LastAttackFrame = int.MinValue;
		}


		public override void Update () {
			base.Update();
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
			if (Game.AddEntity(BulletID, Source.X, Source.Y) is eBullet bullet) {
				bullet.Attackness = this;
				bullet.Combo = Combo;
			}
			Combo = RandomCombo ? Random.Next() : Combo + 1;
			return true;
		}


		public void CancelAttack () => LastAttackFrame = int.MinValue;


		public void SetBullet (string name) {
			BulletName.Value = name;
			_BulletID = 0;
		}


		public bool CheckReady (bool isHoldingAttack) => isHoldingAttack ?
			Game.GlobalFrame >= LastAttackFrame + Duration + Colldown + HoldTriggerPunish :
			Game.GlobalFrame >= LastAttackFrame + Duration + Colldown;


		#endregion




		#region --- LGC ---




		#endregion



	}
}
