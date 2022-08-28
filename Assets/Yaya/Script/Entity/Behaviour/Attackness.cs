using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	public interface IAttackReceiver {
		Health Health { get; }
		void TakeDamage (int damage);
	}


	public class Attackness : EntityBehaviour<Entity>, ITxtMeta {




		#region --- VAR ---


		// Api
		public int CurrentBulletID => _CurrentBulletID != 0 ? _CurrentBulletID : (_CurrentBulletID = BulletName.Value.AngeHash());
		public bool IsAttacking => Game.GlobalFrame < LastAttackFrame + AttackDuration;
		public int ActionIndex { get; private set; } = 0;
		public int LastAttackFrame { get; private set; } = int.MinValue;
		public int AttackDurationValue => AttackDuration;
		public bool StopOnAttackValue => StopOnAttack;

		// Ser
		[SerializeField] BuffString BulletName = new("Bullet");
		[SerializeField] BuffInt AttackDuration = new(24);
		[SerializeField] BuffInt AttackColldown = new(12);
		[SerializeField] BuffBool AttackInAir = new(true);
		[SerializeField] BuffBool AttackInWater = new(true);
		[SerializeField] BuffBool StopOnAttack = new(true);

		// Data
		private eCharacter Character = null;
		private int _CurrentBulletID = 0;
		private HitInfo[] c_Attack = new HitInfo[32];


		#endregion




		#region --- MSG ---


		public override void Initialize (Entity source) {
			base.Initialize(source);
			LastAttackFrame = int.MinValue;
			Character = source as eCharacter;
		}


		#endregion




		#region --- API ---


		// Meta
		public void LoadFromText (string text) => BuffValue.LoadBuffMetaFromText(this, text);


		public string SaveToText () => BuffValue.SaveBuffMetaToText(this);


		// Attackness
		public bool Attack (int actionIndex = 0) {
			int frame = Game.GlobalFrame;
			if (frame < LastAttackFrame + AttackDuration + AttackColldown) return false;
			if (Character != null) {
				if (!AttackInAir && Character.Movement.IsInAir) return false;
				if (!AttackInWater && Character.InWater) return false;
			}



			//IAttackReceiver
			//YayaConst.MASK_RIGIDBODY
			//c_Attack


			ActionIndex = actionIndex;
			LastAttackFrame = frame;
			return true;
		}


		#endregion




		#region --- LGC ---




		#endregion



	}
}
