using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;


namespace Yaya {
	public class Health : EntityBehaviour<Entity>, ISerializationCallbackReceiver {




		#region --- VAR ---


		// Api
		public int HealthPoint { get; private set; } = 1;
		public int LastDamageFrame { get; private set; } = int.MinValue;
		public bool FullHealth => HealthPoint >= MaxHP;
		public bool EmptyHealth => HealthPoint <= 0;
		public bool Invincible => Game.GlobalFrame < InvincibleStartFrame + InvincibleFrame;

		// Buff
		public BuffInt MaxHP { get; private set; } = new(1);
		public BuffInt InvincibleFrame { get; private set; } = new(120);
		public BuffInt KnockBackSpeed { get; private set; } = new(64);
		public BuffInt DamageStunDuration { get; private set; } = new(24);

		// Ser
#pragma warning disable
		[SerializeField] int _MaxHP = 1;
		[SerializeField] int _InvincibleFrame = 120;
		[SerializeField] int _KnockBackSpeed = 64;
		[SerializeField] int _DamageStunDuration = 24;
#pragma warning restore

		// Data
		private int InvincibleStartFrame = int.MinValue;


		#endregion




		#region --- MSG ---


		public override void OnActived (Entity source) {
			base.OnActived(source);
			HealthPoint = MaxHP.FinalValue;
			InvincibleStartFrame = int.MinValue;
		}


		public void OnBeforeSerialize () => BuffValue.SerializeBuffValues(this);
		public void OnAfterDeserialize () => BuffValue.DeserializeBuffValues(this);


		#endregion




		#region --- API ---


		// Health
		public bool Damage (int damage, bool ignoreInvincible = false, bool triggerInvincible = true) {
			if (damage <= 0) return false;
			if (!ignoreInvincible && Invincible) return false;
			if (HealthPoint <= 0) return false;
			HealthPoint = (HealthPoint - damage).Clamp(0, MaxHP);
			if (triggerInvincible) InvincibleStartFrame = Game.GlobalFrame;
			LastDamageFrame = Game.GlobalFrame;
			return true;
		}


		public bool Heal (int heal) {
			int oldPoint = HealthPoint;
			HealthPoint = (HealthPoint + heal).Clamp(0, MaxHP);
			return oldPoint != HealthPoint;
		}


		#endregion




		#region --- LGC ---




		#endregion




	}
}
