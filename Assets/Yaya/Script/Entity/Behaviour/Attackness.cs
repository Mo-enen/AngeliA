using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using UnityEngine.Networking.Types;


namespace Yaya {


	public interface IAttackReceiver {
		Health Health { get; }
		void TakeDamage (int damage);
	}


	public class Attackness : EntityBehaviour<Entity>, IInitialize, ISerializationCallbackReceiver {




		#region --- VAR ---


		// Api
		public bool IsAttacking => Game.GlobalFrame < LastAttackFrame + AttackDuration;
		public bool IsReady => Game.GlobalFrame >= LastAttackFrame + AttackDuration + AttackColldown;
		public int ActionIndex { get; private set; } = 0;
		public int LastAttackFrame { get; private set; } = int.MinValue;
		public int BulletID => _BulletID != 0 ? _BulletID : (_BulletID = BulletName.Value.AngeHash());

		// Buff
		public BuffString BulletName { get; private set; } = new("eDefaultBullet");
		public BuffInt AttackDuration { get; private set; } = new(12);
		public BuffInt AttackColldown { get; private set; } = new(2);
		public BuffBool AttackInAir { get; private set; } = new(true);
		public BuffBool AttackInWater { get; private set; } = new(true);
		public BuffBool StopMoveOnAttack { get; private set; } = new(true);
		public BuffBool CancelAttackOnJump { get; private set; } = new(false);

		// Ser
#pragma warning disable
		[SerializeField] string _BulletName = "eDefaultBullet";
		[SerializeField] int _AttackDuration = 12;
		[SerializeField] int _AttackColldown = 2;
		[SerializeField] bool _AttackInAir = true;
		[SerializeField] bool _AttackInWater = true;
		[SerializeField] bool _StopMoveOnAttack = true;
		[SerializeField] bool _CancelAttackOnJump = false;
#pragma warning restore

		// Data
		private static Game Game = null;
		private int _BulletID = 0;


		#endregion




		#region --- MSG ---


		public static void InitializeWithGame (Game game) => Game = game;


		public override void Initialize (Entity source) {
			base.Initialize(source);
			LastAttackFrame = int.MinValue;
		}


		public void OnBeforeSerialize () => BuffValue.SerializeBuffValues(this);
		public void OnAfterDeserialize () => BuffValue.DeserializeBuffValues(this);


		#endregion




		#region --- API ---


		// Attackness
		public bool Attack (int actionIndex = 0) {
			if (BulletID == 0) return false;
			int frame = Game.GlobalFrame;
			if (frame < LastAttackFrame + AttackDuration + AttackColldown) return false;
			ActionIndex = actionIndex;
			LastAttackFrame = frame;
			// Spawn Bullet
			if (Game.AddEntity(BulletID, Source.X, Source.Y) is eBullet bullet) {
				bullet.Attackness = this;
			}
			return true;
		}


		public void CancelAttack () => LastAttackFrame = int.MinValue;


		public void SetBullet (string name) {
			BulletName.Value = name;
			_BulletID = 0;
		}


		#endregion




		#region --- LGC ---




		#endregion



	}
}
